using SleepyCommon;
using System;
using System.Diagnostics;
using TransferManagerCE.CustomManager.Stats;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    /// <summary>
    /// CustomTransferDisptacher: coordinate with match maker thread
    /// </summary>
    public sealed class CustomTransferDispatcher
    {
        // Static members
        private static CustomTransferDispatcher? s_instance = null;

        // We randomize the loading of offers to ensure there is no bias to help with matching.
        // The simulation thread calls each building in order so there will normally be a bias towards
        // the buildings at the start of the loop
        private Random m_random = new Random();
        private int[] m_shuffledList = new int[TransferManager.TRANSFER_OFFER_COUNT];

        // A set of all running job reasons
        private DispatchedReasons m_dispatchedReasons = new DispatchedReasons();

        // A queue of matches that we send back to the vanilla tranfer manager
        private TransferResultQueue m_resultQueue = new TransferResultQueue();

        // Which match cycle are we on
        private int m_cycle = 0;
        private int m_droppedReasonCount = 0;

        public static CustomTransferDispatcher Instance
        {
            get {
                if (s_instance is null)
                {
                    s_instance = new CustomTransferDispatcher();
                }
                return s_instance;
            }
        }

        public CustomTransferDispatcher()
        {

        }

        public void Delete()
        {
            s_instance = null;
        }

        public int Cycle
        {
            get { return m_cycle; }
        }

        public int DroppedReasons
        {
            get { return m_droppedReasonCount; }
        }

        public void ResetStatistics()
        {
            m_droppedReasonCount = 0;
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

            // Snow is the first transfer reason in a cycle, see TransferManager.GetFrameReason
            if (material == TransferReason.Snow)
            {
                m_cycle++;
                TransferManagerStats.CycleData.CycleStarted(m_cycle);
            }

            // Don't submit with no amounts
            if (incomingAmount[(int)material] == 0 || outgoingAmount[(int)material] == 0)
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
            if (m_dispatchedReasons.IsDispatchedReason((CustomTransferReason.Reason) material))
            {
                m_droppedReasonCount++;
                ClearAllTransferOffers(material, ref incomingCount, ref outgoingCount, ref incomingAmount, ref outgoingAmount);
                CDebug.Log($"Already in queue or running, discarding: {material}");
                return;
            }

            // lease new job from pool
            TransferJob? job = TransferJobPool.Instance.Lease();
            if (job is null)
            {
                CDebug.LogError($"NO MORE TRANSFER JOBS AVAILABLE, DROPPING TRANSFER REQUESTS FOR {material}");
                ClearAllTransferOffers(material, ref incomingCount, ref outgoingCount, ref incomingAmount, ref outgoingAmount);
                return;
            }

            // Add it to current dispatched reasons so we don't add another one till this one completes
            m_dispatchedReasons.AddDispatchedReason((CustomTransferReason.Reason) material);

            // set job header info
            job.m_cycle = m_cycle;
            job.material = (CustomTransferReason.Reason) material;
            job.m_incomingCount = 0;
            job.m_outgoingCount = 0;
            job.m_incomingAmount = incomingAmount[(int)material];
            job.m_outgoingAmount = outgoingAmount[(int)material];

            int jobInIdx = 0;
            int jobOutIdx = 0;
            for (int priority = 7; priority >= 0; --priority)
            {
                int offer_offset = (int)material * 8 + priority;
                int inCount = incomingCount[offer_offset];
                int outCount = outgoingCount[offer_offset];

                // Add counts to overall values
                job.m_incomingCount += (ushort)inCount;
                job.m_outgoingCount += (ushort)outCount;

                // Load incoming
                int[] inIndexes = GenerateShuffledList(inCount);
                for (int i = 0; i < inCount; ++i)
                {
                    if (job.m_incomingOffers[jobInIdx] is null)
                    {
                        job.m_incomingOffers[jobInIdx++] = new CustomTransferOffer(true, incomingOffers[offer_offset * 256 + inIndexes[i]]);
                    }
                    else
                    {
                        job.m_incomingOffers[jobInIdx++].SetOffer(true, incomingOffers[offer_offset * 256 + inIndexes[i]]);
                    }
                }

                // Load outgoing
                int[] outIndexes = GenerateShuffledList(outCount);
                for (int i = 0; i < outCount; ++i)
                {
                    if (job.m_outgoingOffers[jobOutIdx] is null)
                    {
                        job.m_outgoingOffers[jobOutIdx++] = new CustomTransferOffer(false, outgoingOffers[offer_offset * 256 + outIndexes[i]]);
                    }
                    else
                    {
                        job.m_outgoingOffers[jobOutIdx++].SetOffer(false, outgoingOffers[offer_offset * 256 + outIndexes[i]]);
                    }
                }
            }

            // We start the remaining counts as total counts
            job.m_outgoingCountRemaining = job.m_outgoingCount; 
            job.m_incomingCountRemaining = job.m_incomingCount;

            // Enqueue in work queue for match-making thread
            TransferJobQueue.Instance.EnqueueWork(job);
            TransferManagerStats.JobStarted(m_cycle);

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

        private int[] GenerateShuffledList(int iCount)
        {
            if (iCount <= TransferManager.TRANSFER_OFFER_COUNT)
            {
                // Load indexes into s_shuffledList
                for (int i = 0; i < iCount; ++i)
                {
                    m_shuffledList[i] = i;
                }

                // Shuffle indexes
                for (int n = iCount - 1; n > 0; --n)
                {
                    //Step 2: Randomly pick an item which has not been shuffled
                    int k = m_random.Next(n + 1);

                    //Step 3: Swap the selected item with the last "unstruck" letter in the collection
                    int temp = m_shuffledList[n];
                    m_shuffledList[n] = m_shuffledList[k];
                    m_shuffledList[k] = temp;
                }
            }
            else
            {
                throw new Exception("Shuffle offers count out of range.");
            }

            return m_shuffledList;
        }

        public void RemoveDispatchedReason(CustomTransferReason.Reason material)
        {
            m_dispatchedReasons.RemoveDispatchedReason(material);
        }

        public TransferResultQueue GetResultQueue()
        {
            return m_resultQueue;
        }

        public void EnqueueTransferResult(CustomTransferReason.Reason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {
            m_resultQueue.EnqueueTransferResult(material, outgoingOffer, incomingOffer, deltaamount);
        }

        public void StartTransfers()
        {
            m_resultQueue.StartTransfers();
        }
    }
}
