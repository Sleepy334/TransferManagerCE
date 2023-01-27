using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using static TransferManager;
using static TransferManagerCE.TransferResultQueue;

namespace TransferManagerCE
{
    public class TransferResultQueue
    {
        /// <summary>
        /// TransferResult: individual work package for StartTransfers
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TransferResult
        {
            public TransferReason material;
            public TransferOffer outgoingOffer;
            public TransferOffer incomingOffer;
            public int deltaamount;
        }

        const int iINITIAL_QUEUE_SIZE = 2000;

        // Vanilla TransferManager.StartTransfer internal fields and arrays
        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);

        // Static members
        private static TransferManagerStartTransfer? TransferManagerStartTransferDG = null;
        private static TransferResultQueue? s_instance = null;

        // Members
        private readonly object m_MatchLock = new object();
        private int m_iMaxQueueDepth = 0;
        private Queue<TransferResult>? m_Matches = new Queue<TransferResult>(iINITIAL_QUEUE_SIZE);

        public static TransferResultQueue Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new TransferResultQueue();
                }
                return s_instance;
            }
        }

        public TransferResultQueue()
        {
            // Load TransferManager.StartTransfer delegate
            if (TransferManagerStartTransferDG == null)
            {
                TransferManagerStartTransferDG = FastDelegateFactory.Create<TransferManagerStartTransfer>(typeof(TransferManager), "StartTransfer", instanceMethod: true);
            }
        }

        public void Delete()
        {
            TransferManagerStartTransferDG = null;
            s_instance = null;
        }

        public int GetMaxUsageCount()
        {
            return m_iMaxQueueDepth;
        }

        public int GetCount()
        {
            if (m_Matches != null)
            {
                return m_Matches.Count;
            }
            return 0;
        }

        public void EnqueueTransferResult(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {            
            if (m_Matches != null)
            {
                TransferResult result = new TransferResult();
                result.material = material;
                result.outgoingOffer = outgoingOffer;
                result.incomingOffer = incomingOffer;
                result.deltaamount = deltaamount;

                lock (m_MatchLock)
                {
                    m_Matches.Enqueue(result);

                    // Update max queue depth stat.
                    m_iMaxQueueDepth = Math.Max(m_Matches.Count, m_iMaxQueueDepth);
                }
            }
        }

        public void StartTransfers()
        {
            if (m_Matches != null)
            {
                while (m_Matches.Count > 0)
                {
                    TransferResult oResult;
                    lock (m_MatchLock)
                    {
                        oResult = m_Matches.Dequeue();
                    }

                    if (TransferManagerStartTransferDG != null && oResult.material != TransferReason.None)
                    {
                        TransferManagerStartTransferDG(TransferManager.instance, oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);
                    }
                }
            }
        }
    }
}