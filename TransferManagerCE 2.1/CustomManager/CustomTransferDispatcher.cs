using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Util;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    /// <summary>
    /// CustomTransferDisptacher: coordinate with match maker thread
    /// </summary>
    public sealed class CustomTransferDispatcher
    {
        private static CustomTransferDispatcher? s_instance = null;

        // A set of all running job reasons
        private static HashSet<TransferReason> s_dispatchedReasons = new HashSet<TransferReason>();
        private static readonly object s_reasonsLock = new object();

        public static CustomTransferDispatcher Instance
        {
            get {
                if (s_instance == null)
                {
                    s_instance = new CustomTransferDispatcher();
                }
                return s_instance;
            }
        }

        public static bool IsDispatchedReason(TransferReason material)
        {
            lock (s_reasonsLock)
            {
                return s_dispatchedReasons.Contains(material);
            }
        }

        private static void AddDispatchedReason(TransferReason material)
        {
            lock (s_reasonsLock)
            {
                s_dispatchedReasons.Add(material);
            }
        }

        public static void RemoveDispatchedReason(TransferReason material)
        {
            lock (s_reasonsLock)
            {
                if (s_dispatchedReasons.Contains(material))
                {
                    s_dispatchedReasons.Remove(material);
                }
            }
        }

        public static void ClearDispatchedReasons()
        {
            s_dispatchedReasons.Clear();
        }

        public void Delete()
        {
            TransferJobQueue.Instance.Destroy();
            s_instance = null;
        }

        /// <summary>
        /// to be called from MatchOffers Prefix Patch:
        /// take requested material and submit all offers as TransferJob
        /// </summary>
        public void SubmitMatchOfferJob(TransferReason material, 
            ref ushort[] incomingCount,
            ref ushort[] outgoingCount,
            TransferOffer[] incomingOffers,
            TransferOffer[] outgoingOffers,
            ref int[] incomingAmount,
            ref int[] outgoingAmount)
        {
            // dont submit jobs with None reason
            if (material == TransferReason.None)
            {
                return;
            }

            // Don't submit with no amounts
            if ((incomingAmount[(int)material] == 0) || (outgoingAmount[(int)material] == 0))
            {
                // At the end of vanilla Match offers it clears all transfers for the requested material
                // So clear them here as well to match that
                ClearAllTransferOffers(material, ref incomingCount, ref outgoingCount, ref incomingAmount, ref outgoingAmount);
                return;
            }

            // If disable dummy traffic is enabled then don't match any Dummy* materials.
            if (SaveGameSettings.GetSettings().DisableDummyTraffic)
            {
                switch (material)
                {
                    case TransferReason.DummyCar:
                    case TransferReason.DummyTrain:
                    case TransferReason.DummyShip:
                    case TransferReason.DummyPlane:
                        ClearAllTransferOffers(material, ref incomingCount, ref outgoingCount, ref incomingAmount, ref outgoingAmount);
                        return;
                    default: break;
                }
            }

            // Have we already got this match reason in the queue
            if (IsDispatchedReason(material))
            {
                Debug.Log($"Already in queue or running, discarding: {material}");
                ClearAllTransferOffers(material, ref incomingCount, ref outgoingCount, ref incomingAmount, ref outgoingAmount);
                return;
            }

            // lease new job from pool
            TransferJob? job = TransferJobPool.Instance.Lease();
            if (job == null)
            {
                Debug.LogError("NO MORE TRANSFER JOBS AVAILABLE, DROPPING TRANSFER REQUESTS!");
                return;
            }

            // Add it to current dispatched reasons so we don't add another one till this one completes
            AddDispatchedReason(material);

            // set job header info
            job.material = material;
            job.m_incomingCount = 0;
            job.m_outgoingCount = 0;
            job.m_incomingAmount = incomingAmount[(int)material];
            job.m_outgoingAmount = outgoingAmount[(int)material];
            int offer_offset;

            int jobInIdx = 0;
            int jobOutIdx = 0;
            for (int priority = 7; priority >= 0; --priority)
            {
                offer_offset = (int)material * 8 + priority;
                job.m_incomingCount += incomingCount[offer_offset];
                job.m_outgoingCount += outgoingCount[offer_offset];

                // linear copy to job's offer arrays
                //** TODO: evaluate speedup via unsafe pointer memcpy **
                for (int offerIndex = 0; offerIndex < incomingCount[offer_offset]; offerIndex++)
                {
                    job.m_incomingOffers[jobInIdx++] = new CustomTransferOffer(incomingOffers[offer_offset * 256 + offerIndex]);
                }
                    
                for (int offerIndex = 0; offerIndex < outgoingCount[offer_offset]; offerIndex++)
                {
                    job.m_outgoingOffers[jobOutIdx++] = new CustomTransferOffer(outgoingOffers[offer_offset * 256 + offerIndex]);
                }
            }

            job.m_outgoingCountRemaining = job.m_outgoingCount; 
            job.m_incomingCountRemaining = job.m_incomingCount;

            // Enqueue in work queue for match-making thread
            TransferJobQueue.Instance.EnqueueWork(job);

            // clear this material transfer:
            ClearAllTransferOffers(material, ref incomingCount, ref outgoingCount, ref incomingAmount, ref outgoingAmount);

        } //SubmitMatchOfferJob

        /// <summary>
        /// CLear all offers from original vanilla arrays
        /// </summary>
        /// <param name="material"></param>
        private void ClearAllTransferOffers(TransferReason material,
            ref ushort[] incomingCounts,
            ref ushort[] outgoingCounts,
            ref int[] incomingAmount,
            ref int[] outgoingAmount)
        {
            for (int k = 0; k < 8; ++k)
            {
                int material_offset = (int)material * 8 + k;
                incomingCounts[material_offset] = 0;
                outgoingCounts[material_offset] = 0;
            }
            incomingAmount[(int)material] = 0;
            outgoingAmount[(int)material] = 0;
        }
    }
}
