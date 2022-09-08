using ColossalFramework;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using TransferManagerCE.Util;
using static TransferManager;

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

        // References to game functionalities:
        public static TransferManager? _TransferManager = null;

        // Vanilla TransferManager internal fields and arrays
        private TransferOffer[] m_outgoingOffers;
        private TransferOffer[] m_incomingOffers;
        private ushort[] m_outgoingCount;
        private ushort[] m_incomingCount;
        private int[] m_outgoingAmount;
        private int[] m_incomingAmount;

        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);
        public static TransferManagerStartTransfer TransferManagerStartTransferDG;

        public static void InitDelegate()
        {
            TransferManagerStartTransferDG = FastDelegateFactory.Create<TransferManagerStartTransfer>(typeof(TransferManager), "StartTransfer", instanceMethod: true);
        }

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

            InitDelegate();
            strError = "";
            return true;
        }

        public void Delete()
        {
            Debug.Log($"Deleting instance: {s_instance}");
            TransferJobQueue.Instance.Destroy();
            s_instance = null;
        }

        /// <summary>
        /// to be called from MatchOffers Prefix Patch:
        /// take requested material and submit all offers as TransferJob
        /// </summary>
        public void SubmitMatchOfferJob(TransferReason material)
        {
            // dont submit jobs for None reason or with no amounts
            if (material == TransferReason.None)
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

            if (SaveGameSettings.GetSettings().DisableDummyTraffic)
            {
                switch (material)
                {
                    case TransferReason.DummyCar:
                    case TransferReason.DummyTrain:
                    case TransferReason.DummyShip:
                    case TransferReason.DummyPlane:
                        ClearAllTransferOffers(material);
                        return;
                    default: break;
                }
            }

            // lease new job from pool
            TransferJob? job = TransferJobPool.Instance.Lease();
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
                    job.m_incomingOffers[jobInIdx++] = new CustomTransferOffer(m_incomingOffers[offer_offset * 256 + offerIndex]);
                }
                    
                for (int offerIndex = 0; offerIndex < m_outgoingCount[offer_offset]; offerIndex++)
                {
                    job.m_outgoingOffers[jobOutIdx++] = new CustomTransferOffer(m_outgoingOffers[offer_offset * 256 + offerIndex]);
                }
            }

            job.m_outgoingCountRemaining = job.m_outgoingCount; 
            job.m_incomingCountRemaining = job.m_incomingCount;
#if DEBUG
            // DEBUG mode: print job summary
            DebugJobSummarize(job);
#endif

            // Enqueue in work queue for match-making thread
            TransferJobQueue.Instance.EnqueueWork(job);

            // clear this material transfer:
            ClearAllTransferOffers(material);

        } //SubmitMatchOfferJob

        /// <summary>
        /// CLear all offers from original vanilla arrays
        /// </summary>
        /// <param name="material"></param>
        private void ClearAllTransferOffers(TransferReason material)
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

#if DEBUG
        private void DebugJobSummarize(TransferJob job)
        {
            DebugLog.LogOnly((DebugLog.LogReason)job.material, $"TRANSFER JOB: {job.material.ToString()}, amount in/out: {job.m_incomingAmount}/{job.m_outgoingAmount}; total offer count in/out: {job.m_incomingCount}/{job.m_outgoingCount}");
        }
#endif
    }
}
