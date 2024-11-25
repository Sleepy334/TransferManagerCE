using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferResultQueue
    {
        const int iINITIAL_QUEUE_SIZE = 5000;
        static public Queue<TransferResult> m_Matches = new Queue<TransferResult>(iINITIAL_QUEUE_SIZE);
        static readonly object s_MatchLock = new object();

        public static void EnqueueTransferResult(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
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
                }
            }
        }

        public static void StartTransfers()
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