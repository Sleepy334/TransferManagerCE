using ColossalFramework;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using TransferManagerCE.Util;

namespace TransferManagerCE.CustomManager
{
    /// <summary>
    /// TransferResult: individual work package for StartTransfers
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransferResult
    {
        public TransferManager.TransferReason material;
        public TransferManager.TransferOffer outgoingOffer;
        public TransferManager.TransferOffer incomingOffer;        
        public int deltaamount;
    }

    /// <summary>
    /// CustomTransferDisptacher: coordinate with match maker thread
    /// </summary>
    public sealed class CustomTransferDispatcher
    {
        private static CustomTransferDispatcher s_instance = null;
        private Queue<TransferJob> workQueue = null;
        private static readonly object _workQueueLock = new object();
        public static EventWaitHandle _waitHandle = new AutoResetEvent(false);
        public static Thread _transferThread = null;

        // TransferResults ring buffer:
        private TransferResult[] m_transferResultRingBuffer;
        private const int RINGBUF_SIZE = 256 * 8;
        private volatile int _ringbufReadPosition;
        private volatile int _ringbufWritePosition;

        #region STATISTICS
        private int _ringBufMaxUsageCount;
        public int GetMaxUsage() => _ringBufMaxUsageCount;
        #endregion

        // References to game functionalities:
        private static TransferManager _TransferManager = null;

        // Vanilla TransferManager internal fields and arrays
        private TransferManager.TransferOffer[] m_outgoingOffers;
        private TransferManager.TransferOffer[] m_incomingOffers;
        private ushort[] m_outgoingCount;
        private ushort[] m_incomingCount;
        private int[] m_outgoingAmount;
        private int[] m_incomingAmount;

        public static CustomTransferDispatcher Instance
        {
            get {
                if (s_instance == null)
                {
                    s_instance = new CustomTransferDispatcher();
                    //s_instance.Initialize();
                }
                return s_instance;
            }
        }

        public bool Initialize(out string strError)
        {
            // bind vanilla transfermanager fields
            _TransferManager = Singleton<TransferManager>.instance;
            if (_TransferManager == null)
            {
                strError = "ERROR: No instance of TransferManager found!";
                Debug.LogError(strError);
                return false;
            }

            FieldInfo? incomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (incomingCount == null)
            { 
                strError = "ERROR: m_incomingCount is null!";
                Debug.LogError(strError);
                return false;
            }
            FieldInfo? incomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (incomingOffers == null)
            {
                strError = "ERROR: m_incomingOffers is null!";
                Debug.LogError(strError);
                return false;
            }
            FieldInfo? incomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (incomingAmount == null)
            {
                strError = "ERROR: m_incomingAmount is null!";
                Debug.LogError(strError);
                return false;
            }
            FieldInfo? outgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (outgoingCount == null)
            {
                strError = "ERROR: m_outgoingCount is null!";
                Debug.LogError(strError);
                return false;
            }
            FieldInfo? outgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (outgoingOffers == null)
            {
                strError = "ERROR: m_outgoingOffers is null!";
                Debug.LogError(strError);
                return false;
            }
            FieldInfo? outgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (outgoingAmount == null)
            {
                strError = "ERROR: m_outgoingAmount is null!";
                Debug.LogError(strError);
                return false;
            }

            m_incomingCount = (ushort[]) incomingCount.GetValue(_TransferManager);
            m_incomingOffers = (TransferManager.TransferOffer[]) incomingOffers.GetValue(_TransferManager);
            m_incomingAmount = (int[]) incomingAmount.GetValue(_TransferManager);
            m_outgoingCount = (ushort[]) outgoingCount.GetValue(_TransferManager);
            m_outgoingOffers = (TransferManager.TransferOffer[]) outgoingOffers.GetValue(_TransferManager);
            m_outgoingAmount = (int[]) outgoingAmount.GetValue(_TransferManager);

            // allocate object pool of work packages
            workQueue = new Queue<TransferJob>(TransferManager.TRANSFER_REASON_COUNT);

            // results ring buffer array
            m_transferResultRingBuffer = new TransferResult[RINGBUF_SIZE];
            _ringbufReadPosition = 0;
            _ringbufWritePosition = 1;
            _ringBufMaxUsageCount = 0;
            for (int i = 0; i < RINGBUF_SIZE; i++)
            {
                m_transferResultRingBuffer[i].material = TransferManager.TransferReason.None;
            }

            unsafe
            {
                DebugLog.LogInfo($"CustomTransferDispatcher initialized, workqueue count is {workQueue.Count}, results ringbuffer size is {m_transferResultRingBuffer.Length}");
                DebugLog.LogInfo($"TransferOffer memsize: {sizeof(TransferManager.TransferOffer)}");
                long memsize = (long)sizeof(TransferManager.TransferOffer) * ((2 * 256 * 8 * 128) + (2*256*8));
                DebugLog.LogInfo($"Total memory size is: (2x256x8x128 + 2x256x8) x TransferOffer MemSize = {memsize} bytes, = {memsize>>20} MB");
            }

            strError = "";
            return true;
        }

        public void Delete()
        {
            Debug.Log($"Deleting instance: {s_instance}");
            // unallocate object pool of work packages
            workQueue.Clear();
            workQueue = null;
            m_transferResultRingBuffer = null;
            CustomTransferDispatcher.s_instance = null;
        }

        /// <summary>
        /// Thread-safe Enqueue
        /// </summary>
        /// <param name="job"></param>
        public void EnqueueWork(TransferJob job)
        {
            lock(_workQueueLock)
            {
                workQueue.Enqueue(job);
                _waitHandle.Set();
            }
        }

        /// <summary>
        /// Thread-safe Dequeue
        /// </summary>
        /// <returns></returns>
        public TransferJob DequeueWork()
        {
            lock (_workQueueLock)
            {
                if (workQueue.Count > 0)
                    return workQueue.Dequeue();
                else
                    return null;
            }
        }

        /// <summary>
        /// Enqueue transferresult from match-maker thread to results ring buffer for StartTransfers
        /// </summary>
        public void EnqueueTransferResult(TransferManager.TransferReason material, TransferManager.TransferOffer outgoingOffer, TransferManager.TransferOffer incomingOffer, int deltaamount)
        {
            if (_ringbufWritePosition == _ringbufReadPosition)
            {
                Debug.LogError($"RESULTS RINGBUFFER: NO MORE OPEN WRITE POSITIONS! readPos={_ringbufReadPosition}, writePos={_ringbufWritePosition}");
            }
            else
            {
                m_transferResultRingBuffer[_ringbufWritePosition].material = material;
                m_transferResultRingBuffer[_ringbufWritePosition].outgoingOffer = outgoingOffer;
                m_transferResultRingBuffer[_ringbufWritePosition].incomingOffer = incomingOffer;
                m_transferResultRingBuffer[_ringbufWritePosition].deltaamount = deltaamount;

                _ringbufWritePosition++;
                if (_ringbufWritePosition >= RINGBUF_SIZE)
                {
                    _ringbufWritePosition = 0;
                }
            }
        }

        /// <summary>
        /// to be called from MatchOffers Prefix Patch:
        /// take requested material and submit all offers as TransferJob
        /// </summary>
        public void SubmitMatchOfferJob(TransferManager.TransferReason material)
        {
            // dont submit jobs for None reason or with no amounts
            if (material == TransferManager.TransferReason.None)
            {
                return;
            }
            if ((m_incomingAmount[(int)material] == 0) || (m_outgoingAmount[(int)material] == 0))
            {
                // At the end of vanilla Match offers it clears all transfers for the requested material
                // So clear them here as well to match that
                ClearAllTransferOffers(material); 
                return;
            }


            // lease new job from pool
            TransferJob job = TransferJobPool.Instance.Lease();
            if (job == null)
            {
                Debug.LogError("NO MORE TRANSFER JOBS AVAILABLE, DROPPING TRANSFER REQUESTS!");
                return;
            }

            // set job header info
            job.material = material;
            job.m_incomingCount = 0;
            job.m_outgoingCount = 0;
            job.m_incomingAmount = m_incomingAmount[(int)material];
            job.m_outgoingAmount = m_outgoingAmount[(int)material];
            int offer_offset;

            int jobInIdx = 0;
            int jobOutIdx = 0;
            for (int priority = 7; priority >= 0; --priority)
            {
                offer_offset = (int)material * 8 + priority;
                job.m_incomingCount += m_incomingCount[offer_offset];
                job.m_outgoingCount += m_outgoingCount[offer_offset];

                // linear copy to job's offer arrays
                //** TODO: evaluate speedup via unsafe pointer memcpy **
                for (int offerIndex = 0; offerIndex < m_incomingCount[offer_offset]; offerIndex++)
                {
                    job.m_incomingOffers[jobInIdx++] = m_incomingOffers[offer_offset * 256 + offerIndex];
                }
                    
                for (int offerIndex = 0; offerIndex < m_outgoingCount[offer_offset]; offerIndex++)
                {
                    job.m_outgoingOffers[jobOutIdx++] = m_outgoingOffers[offer_offset * 256 + offerIndex];
                }
            }

            // DEBUG mode: print job summary
            DebugJobSummarize(job);

            // Enqueue in work queue for match-making thread
            EnqueueWork(job);

            // clear this material transfer:
            ClearAllTransferOffers(material);

        } //SubmitMatchOfferJob

        /// <summary>
        /// to be called from MatchOffers Postfix Patch:
        /// receive match-maker results from ring buffer and start transfers
        /// </summary>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void StartTransfers()
        {
            int num_transfers_initiated = 0;
            int newReadPos = _ringbufReadPosition + 1;
            if (newReadPos >= RINGBUF_SIZE)
            {
                newReadPos = 0;
            }

            while (newReadPos != _ringbufWritePosition)
            {
                _ringbufReadPosition = newReadPos;

                // call delegate on vanilla transfer manager
                TransferResult oResult = m_transferResultRingBuffer[_ringbufReadPosition];
                if (oResult.material != TransferManager.TransferReason.None)
                {
                    CustomTransferManager.TransferManagerStartTransferDG(_TransferManager, oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);
                    TransferManagerCEThreading.StartTransfer(oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);

                }

                newReadPos = _ringbufReadPosition + 1;
                num_transfers_initiated++;
                if (newReadPos >= RINGBUF_SIZE)
                {
                    newReadPos = 0;
                }
            }

            _ringBufMaxUsageCount = num_transfers_initiated > _ringBufMaxUsageCount ? num_transfers_initiated : _ringBufMaxUsageCount;
            DebugLog.LogOnly($"StartTransfers: initiated {num_transfers_initiated} transfers.");
        }

        /// <summary>
        /// CLear all offers from original vanilla arrays
        /// </summary>
        /// <param name="material"></param>
        private void ClearAllTransferOffers(TransferManager.TransferReason material)
        {
            for (int k = 0; k < 8; ++k)
            {
                int material_offset = (int)material * 8 + k;
                m_incomingCount[material_offset] = 0;
                m_outgoingCount[material_offset] = 0;
            }
            m_incomingAmount[(int)material] = 0;
            m_outgoingAmount[(int)material] = 0;
        }

        [Conditional("DEBUG")]
        private void DebugJobSummarize(TransferJob job)
        {
            DebugLog.LogOnly((DebugLog.LogReason)job.material, $"TRANSFER JOB: {job.material.ToString()}, amount in/out: {job.m_incomingAmount}/{job.m_outgoingAmount}; total offer count in/out: {job.m_incomingCount}/{job.m_outgoingCount}");
        }
    }
}
