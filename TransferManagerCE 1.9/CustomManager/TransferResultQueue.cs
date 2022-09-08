using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferResultQueue
    {
        const int iINITIAL_QUEUE_SIZE = 2000; 
        
        // Static members
        private static TransferResultQueue? s_instance = null;
        static readonly object s_MatchLock = new object();
        public static int s_iMaxQueueDepth = 0;

        // instance members
        private Queue<TransferResult>? m_Matches = null;

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

        public static int GetMaxUsageCount()
        {
            return s_iMaxQueueDepth;
        }

        public int GetCount()
        {
            if (m_Matches != null)
            {
                return m_Matches.Count;
            }
            return 0;
        }

        public TransferResultQueue()
        {
            m_Matches = new Queue<TransferResult>(iINITIAL_QUEUE_SIZE);
        }

        public void EnqueueTransferResult(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {            
            if (m_Matches != null)
            {
                lock (s_MatchLock)
                {
                    TransferResult result = new TransferResult();
                    result.material = material;
                    result.outgoingOffer = outgoingOffer;
                    result.incomingOffer = incomingOffer;
                    result.deltaamount = deltaamount;
                    m_Matches.Enqueue(result);

                    // Update max queue depth stat.
                    s_iMaxQueueDepth = Math.Max(m_Matches.Count, s_iMaxQueueDepth);
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
                    lock (s_MatchLock)
                    {
                        oResult = m_Matches.Dequeue();
                    }

                    if (oResult.material != TransferReason.None)
                    {
                        CustomTransferDispatcher.TransferManagerStartTransferDG(TransferManager.instance, oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);
                    }
                }
            }
        }
    }
}