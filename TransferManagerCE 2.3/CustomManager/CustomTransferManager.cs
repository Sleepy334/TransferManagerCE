using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using ColossalFramework.Math;
using TransferManagerCE.Util;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using System.Collections.Generic;
using static TransferManagerCE.CustomManager.TransferRestrictions;
using static TransferManagerCE.CustomManager.TransferManagerModes;
using static TransferManagerCE.CustomManager.PathDistanceTypes;
using System.Text;

namespace TransferManagerCE.CustomManager
{
    public sealed class CustomTransferManager : TransferManager
    {
        public enum BalancedMatchModeOption
        {
            MatchModeIncomingFirst = 0, // Vanilla
            MatchModeLeastFirst = 1,
            MatchModePassiveFirst = 2
        }
        public enum MatchOfferAlgorithm
        {
            Distance,
            Priority,
        }

        // Current transfer job from workqueue
        public TransferJob? job = null;
        private PathDistance? m_pathDistance = null;
        private TransferRestrictions m_transferRestrictions;
        private PathDistanceAlgorithm m_DistanceAlgorithm;
        private Randomizer m_randomizer = new Randomizer();
        private MatchJobLogFile? m_logFile = null;
        private int m_iMatches = 0;
        private Stopwatch m_watch = Stopwatch.StartNew();
        private TransferMode m_eTransferMode;

        // -------------------------------------------------------------------------------------------
        public CustomTransferManager()
        {
            m_transferRestrictions = new TransferRestrictions();
        }

        // -------------------------------------------------------------------------------------------
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void MatchOffers(TransferReason material)
        {
            // guard: ignore transferreason.none
            if (material == TransferReason.None)
            {
                return;
            }

            if (job is null)
            {
                return;
            }

            // Setup match logging if requested
            TransferReason logReason = (TransferReason) ModSettings.GetSettings().MatchLogReason;
            if (logReason != TransferReason.None && logReason == material)
            {
                m_logFile = new MatchJobLogFile(material);
                m_logFile.LogHeader(job);
            }
            else
            {
                m_logFile = null;
            }

            long startTimeTicks = m_watch.ElapsedTicks;
            TransferManagerStats.UpdateLargestMatch(job);

            // Reset members as we re-use the match jobs.
            m_transferRestrictions.SetMaterial(material);
            m_iMatches = 0;

            // Pathing support
            m_DistanceAlgorithm = PathDistanceTypes.GetDistanceAlgorithm(material);

            // Check we can actually use path distance and set up the path distance object
            if (m_DistanceAlgorithm == PathDistanceAlgorithm.PathDistance)
            {
                if (IsLargeMatchSet())
                {
                    // Fall back on simpler algorithm as the match set is large
                    m_DistanceAlgorithm = PathDistanceAlgorithm.LineOfSight;

                    if (m_logFile is not null)
                    {
                        m_logFile.LogInfo("Large match set detected, path distance disabled.");
                    }
                }
                else
                {
                    // Create path distance object if needed.
                    if (m_pathDistance is null)
                    {
                        m_pathDistance = new PathDistance();
                    }

                    // Set up lane requirements
                    m_pathDistance.SetMaterial(material);
                }
            }

            if (TransferJobQueue.Instance.Count() > 100)
            {
                // We are falling behind, switch to fast matching to catch back up
                MatchModePriority();
            }
            else
            {
                // If WarehouseFirst is set we match warehouses first, then do another match run for other matches
                bool bWarehouseFirst = IsWarehouseMaterial(job.material) && SaveGameSettings.GetSettings().WarehouseFirst;
                if (bWarehouseFirst)
                {
                    // WarehouseFirst - We match IN only first then we do tghe OUT matches after the factories.
                    // Stop at P:1 as P:0 warehouses arent really interested in matching anyway
                    MatchIncomingOffers(MatchOfferAlgorithm.Distance, 1, bWarehouseOnly: true, bFactoryOnly: false, bCloseByOnly: false);
                }

                // If FactoryFirst is set we match factories IN offers first, then
                if (IsFactoryMaterial(job.material) && SaveGameSettings.GetSettings().FactoryFirst)
                {
                    // We match IN only first as matching OUT factories first can make some very bad matches
                    MatchIncomingOffers(MatchOfferAlgorithm.Distance, 0, bWarehouseOnly: false, bFactoryOnly: true, bCloseByOnly: false);
                }

                // We now also occasionally do warehouse OUT matches first as well just to keep warehouses ticking over
                // We do it after FactoryFirst IN matches to ensure we don't produce a bad match with a factory.
                if (bWarehouseFirst)
                {
                    // Only match down to priority 2, warehouse offers less than this arent really interested in matching.
                    MatchOutgoingOffers(MatchOfferAlgorithm.Distance, 2, bWarehouseOnly: true, bCloseByOnly: false);
                }

                // Select match mode based on material
                m_eTransferMode = TransferManagerModes.GetTransferMode(material);
                switch (m_eTransferMode)
                {
                    case TransferMode.Priority:
                        {
                            MatchModePriority(); // Just match by priority, distance ignored
                            break;
                        }
                    case TransferMode.OutgoingFirst:
                        {
                            MatchModeOutgoingFirst(); // OUT loop first then IN loop with CloseByOnly
                            break;
                        }
                    case TransferMode.IncomingFirst:
                        {
                            MatchModeIncomingFirst(); // IN loop first then OUT loop with CloseByOnly
                            break;
                        }
                    case TransferMode.Balanced:
                    default:
                        {
                            MatchModeBalanced(); // Match IN and OUT in priority order with distance
                            break;
                        }
                }
            }

            // Record longest match time for stats.
            long jobMatchTimeTicks = m_watch.ElapsedTicks - startTimeTicks;
            TransferManagerStats.UpdateStats(job.m_cycle, material, m_DistanceAlgorithm, jobMatchTimeTicks);

            // Finished with log file
            if (m_logFile is not null)
            {
                m_logFile.LogFooter(job, m_iMatches, jobMatchTimeTicks);
                m_logFile.Close();
                m_logFile = null;
            }
        }

        // -------------------------------------------------------------------------------------------
        // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchModeOutgoingFirst()
        {
            if (job is null)
            {
                return;
            }

            // Set close by only flags
            bool bCloseByOnlyIncoming;
            bool bCloseByOnlyOutgoing;
            if (ApplyCloseByOny((CustomTransferReason.Reason) job.material))
            {
                bCloseByOnlyIncoming = true;  // Only match incoming if it is close by
                bCloseByOnlyOutgoing = false; // Match all outgoing 
            }
            else
            {
                bCloseByOnlyIncoming = false; // Match all incoming
                bCloseByOnlyOutgoing = false; // Match all outgoing
            }

            if (m_logFile is not null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"### MatchModeOutgoingFirst ### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: {bCloseByOnlyIncoming} | CloseByOnlyOutgoing: {bCloseByOnlyOutgoing}");
            }
            
            // 1: Match OUTGOING offers by descending priority first,
            // stop at 0/2 as 1/1 matches are usually not very close by.
            MatchOutgoingOffers(MatchOfferAlgorithm.Distance, 2, false, bCloseByOnlyOutgoing);

            // 2: Now match any INCOMING remaining,
            // stop at 2/0 as 1/1 matches are usually not very close by,
            // and limit the matches to close by only
            MatchIncomingOffers(MatchOfferAlgorithm.Distance, 2, false, false, bCloseByOnlyIncoming);
        }

        // -------------------------------------------------------------------------------------------
        // INCOMING FIRST mode - try to fulfill all incoming requests by finding outgoings by distance
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchModeIncomingFirst()
        {
            if (job is null)
            {
                return;
            }

            // Set close by only flags
            bool bCloseByOnlyIncoming;
            bool bCloseByOnlyOutgoing;
            if (ApplyCloseByOny((CustomTransferReason.Reason) job.material))
            {
                bCloseByOnlyIncoming = false; // Match all incoming
                bCloseByOnlyOutgoing = true;  // Only match outgoing if it is close by
            }
            else
            {
                bCloseByOnlyIncoming = false; // Match all incoming
                bCloseByOnlyOutgoing = false; // Match all outgoing
            }

            if (m_logFile is not null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"### MatchModeIncomingFirst ### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: {bCloseByOnlyIncoming} | CloseByOnlyOutgoing: {bCloseByOnlyOutgoing}");
            }

            // 1: Match INCOMING offers by descending priority first,
            // stop at 2/0 as 1/1 matches are usually not very close by
            MatchIncomingOffers(MatchOfferAlgorithm.Distance, 2, false, false, bCloseByOnlyIncoming);

            // 2: Now match OUTGOING offers by descending priority,
            // stop at 0/2 as 1/1 matches are usually not very close by
            // and limit the matches to close by only
            MatchOutgoingOffers(MatchOfferAlgorithm.Distance, 2, false, bCloseByOnlyOutgoing);
        }

        // -------------------------------------------------------------------------------------------
        // PRIORITY mode - match incoming/outgoing by descending priority choosing the closest match by distance
        private void MatchModeBalanced()
        {
            if (job is null)
            {
                return;
            }

            // Set CloseByOnly preferences
            bool bCloseByOnlyIncoming = false;
            bool bCloseByOnlyOutgoing = false;

            if (m_logFile is not null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"### MatchModeBalanced ### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: {bCloseByOnlyIncoming} | CloseByOnlyOutgoing: {bCloseByOnlyOutgoing}");
            }

            // Now perform normal matches
            MatchOffersBalancedImpl(MatchOfferAlgorithm.Distance, bCloseByOnlyIncoming, bCloseByOnlyOutgoing);
        }

        // -------------------------------------------------------------------------------------------
        // PRIORITY mode - match incoming/outgoing by descending priority only. Ignore distance
        private void MatchModePriority()
        {
            if (job is null)
            {
                return;
            }

            if (m_logFile is not null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"### MatchModePriority ### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: false | CloseByOnlyOutgoing: false");
            }

            // Priority doesnt use distance so CloseByOnly flags are always false
            MatchOffersBalancedImpl(MatchOfferAlgorithm.Priority, false, false);
        }

        // -------------------------------------------------------------------------------------------
        // BALANCED match mode implementation - match incoming/outgoing one by one descending priority
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersBalancedImpl(MatchOfferAlgorithm eAlgoritm, bool bCloseByOnlyIncoming, bool bCloseByOnlyOutgoing)
        {
            // Get equal priority mode - User preference is overridden in certain cases.
            BalancedMatchModeOption matchMode = SaveGameSettings.s_SaveGameSettings.BalancedMatchMode;

            // loop incoming+outgoing offers by descending priority
            int indexIn = 0;
            int indexOut = 0;
            while (indexIn < job.m_incomingCount || indexOut < job.m_outgoingCount)
            {
                if (!CheckCountsAndAmounts())
                {
                    break;
                }

                if (indexIn < job.m_incomingCount && indexOut < job.m_outgoingCount)
                {
                    // Both remaining
                    ref CustomTransferOffer incoming = ref job.m_incomingOffers[indexIn];
                    if (incoming.Amount <= 0)
                    {
                        indexIn++;
                        continue;
                    }

                    ref CustomTransferOffer outgoing = ref job.m_outgoingOffers[indexOut];
                    if (outgoing.Amount <= 0)
                    {
                        indexOut++;
                        continue;
                    }

                    // If current combined priority is 1 or less then we can't match anymore as 0/0 and 1/0 are not valid matches
                    // and all the higher priorities should already have had a match attempted.
                    int combinedPriority = incoming.Priority + outgoing.Priority;
                    if (combinedPriority <= 1)
                    {
                        LogInfo("Break: Low priority reached.");
                        break;
                    }

                    // Determine which offer to match first
                    bool bMatchIncomingFirst;
                    if (incoming.Priority == outgoing.Priority)
                    {
                        // Apply user setting to determine which offer to match when priority is equal
                        switch (matchMode)
                        {
                            case BalancedMatchModeOption.MatchModeIncomingFirst:
                                {
                                    // Match incoming first (Vanilla mode)
                                    bMatchIncomingFirst = true;
                                    break;
                                }
                            case BalancedMatchModeOption.MatchModeLeastFirst:
                                {
                                    // Match whichever has less offers available so that we maximise the matches for the side with
                                    // limited resources.
                                    bMatchIncomingFirst = (job.m_incomingCountRemaining <= job.m_outgoingCountRemaining);
                                    break;
                                }
                            case BalancedMatchModeOption.MatchModePassiveFirst:
                                {
                                    // Match against passive side first
                                    bMatchIncomingFirst = !incoming.Active || (incoming.Active == outgoing.Active);
                                    break;
                                }
                            default:
                                {
                                    bMatchIncomingFirst = true;
                                    break;
                                }
                        }
                    }
                    else if (incoming.Priority > outgoing.Priority)
                    {
                        bMatchIncomingFirst = true;
                    }
                    else
                    {
                        bMatchIncomingFirst = false;
                    }

                    // Perform match
                    if (bMatchIncomingFirst)
                    {
                        ApplyMatch(indexIn, MatchIncomingOffer(eAlgoritm, ref incoming, bCloseByOnlyIncoming));
                        indexIn++;
                    }
                    else
                    {
                        ApplyMatch(MatchOutgoingOffer(eAlgoritm, ref outgoing, bCloseByOnlyOutgoing), indexOut);
                        indexOut++;
                    }
                }
                else if (indexIn < job.m_incomingCount)
                {
                    // Only IN remaining
                    CustomTransferOffer incoming = job.m_incomingOffers[indexIn];
                    if (incoming.Amount <= 0)
                    {
                        indexIn++;
                        continue;
                    }
                    
                    // If current priority is 0 then we can't match anymore as 0/0 is not a valid match
                    // and all the other priorities should already have had a match attempted.
                    if (incoming.Priority == 0)
                    {
                        break;
                    }

                    ApplyMatch(indexIn, MatchIncomingOffer(eAlgoritm, ref incoming, bCloseByOnlyIncoming));
                    indexIn++;
                }
                else if (indexOut < job.m_outgoingCount)
                {
                    // Only OUT remaining
                    CustomTransferOffer outgoing = job.m_outgoingOffers[indexOut];
                    if (outgoing.Amount <= 0)
                    {
                        indexOut++;
                        continue;
                    }
                    
                    // If current priority is 0 then we can't match anymore as 0/0 is not a valid match
                    // and all the other priorities should already have had a match attempted.
                    if (outgoing.Priority == 0)
                    {
                        break;
                    }

                    ApplyMatch(MatchOutgoingOffer(eAlgoritm, ref outgoing, bCloseByOnlyOutgoing), indexOut);
                    indexOut++;
                }
                else
                {
                    break;
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        private void MatchIncomingOffers(MatchOfferAlgorithm algorithm, int iPriorityLimit, bool bWarehouseOnly, bool bFactoryOnly, bool bCloseByOnly)
        {
            // Now match INCOMING offers by descending priority
            if (job is not null)
            {
                if (m_logFile is not null)
                {
                    m_logFile.LogSeparator();
                    m_logFile.LogInfo($"### MatchIncomingOffers ### | Algorithm: {algorithm} | PriorityLimit:{iPriorityLimit} | {DebugFlags(bWarehouseOnly, bFactoryOnly, bCloseByOnly)}");
                }

                for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
                {
                    if (!CheckCountsAndAmounts())
                    {
                        break;
                    }

                    ref CustomTransferOffer incoming = ref job.m_incomingOffers[offerIndex];

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && incoming.Priority < iPriorityLimit)
                    {
                        if (m_logFile is not null)
                        {
                            m_logFile.LogInfo($"Break: PriorityLimit reached: {iPriorityLimit}.");
                        }
                        break;
                    }

                    // If in factory only mode skip over non factory offers
                    if (bFactoryOnly && !incoming.IsFactory())
                    {
                        continue;
                    }

                    // If in warehouse only mode skip over non warehouse offers but only if it isn't disabled for this warehouse
                    if (bWarehouseOnly)
                    {
                        // We only do WarehouseFirst 1/3 of the time otherwise it can be too strong
                        if (!incoming.IsWarehouse() || m_randomizer.Int32(3U) != 0)
                        {
                            continue;
                        }
                    }

                    ApplyMatch(offerIndex, MatchIncomingOffer(algorithm, ref incoming, bCloseByOnly));
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        private void MatchOutgoingOffers(MatchOfferAlgorithm algorithm, int iPriorityLimit, bool bWarehouseOnly, bool bCloseByOnly)
        {
            // Now match OUTGOING offers by descending priority
            if (job is not null)
            {
                if (m_logFile is not null)
                {
                    m_logFile.LogSeparator();
                    m_logFile.LogInfo($"### MatchOutgoingOffers ### | Algorithm: {algorithm} | PriorityLimit:{iPriorityLimit} | {DebugFlags(bWarehouseOnly, bFactoryOnly: false, bCloseByOnly)}");
                }

                for (int offerIndex = 0; offerIndex < job.m_outgoingCount; offerIndex++)
                {
                    if (!CheckCountsAndAmounts())
                    {
                        break;
                    }

                    ref CustomTransferOffer outgoing = ref job.m_outgoingOffers[offerIndex];

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && outgoing.Priority < iPriorityLimit)
                    {
                        if (m_logFile is not null)
                        {
                            m_logFile.LogInfo($"Break: PriorityLimit reached: {iPriorityLimit}.");
                        }
                        break;
                    }

                    // If in warehouse only mode skip over non warehouse offers but only if it isn't disabled for this warehouse
                    if (bWarehouseOnly)
                    {
                        // For outgoing we only occasionally do a WarehouseFirst match just to keep the warehouses ticking over
                        if (!outgoing.IsWarehouse() || m_randomizer.Int32(5U) != 0)
                        {
                            continue;
                        }
                    }

                    ApplyMatch(MatchOutgoingOffer(algorithm, ref outgoing, bCloseByOnly), offerIndex);
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>best_index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchIncomingOffer(MatchOfferAlgorithm eAlgoritm, ref CustomTransferOffer offer, bool bCloseByOnly)
        {
            switch (eAlgoritm)
            {
                case MatchOfferAlgorithm.Distance:
                    {
                        switch (m_DistanceAlgorithm)
                        {
                            case PathDistanceAlgorithm.PathDistance:
                                {
                                    return MatchOfferPathDistance(ref offer, job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                                }
                            case PathDistanceAlgorithm.ConnectedLineOfSight:
                                {
                                    return MatchOfferConnectedLOS(ref offer, job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                                }
                            case PathDistanceAlgorithm.LineOfSight:
                            default:
                                {
                                    return MatchOfferLOS(ref offer, job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                                }
                        }
                    }
                case MatchOfferAlgorithm.Priority:
                    {
                        return MatchOfferPriority(ref offer, job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                    }
            }

            return -1;
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>best_index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOutgoingOffer(MatchOfferAlgorithm eAlgoritm, ref CustomTransferOffer offer, bool bCloseByOnly)
        {
            switch (eAlgoritm)
            {
                case MatchOfferAlgorithm.Distance:
                    {
                        switch (m_DistanceAlgorithm)
                        {
                            case PathDistanceAlgorithm.PathDistance:
                                {
                                    return MatchOfferPathDistance(ref offer, job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                                }
                            case PathDistanceAlgorithm.ConnectedLineOfSight:
                                {
                                    return MatchOfferConnectedLOS(ref offer, job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                                }
                            case PathDistanceAlgorithm.LineOfSight:
                            default:
                                {
                                    return MatchOfferLOS(ref offer, job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                                }
                        }
                    }         

                case MatchOfferAlgorithm.Priority:
                    return MatchOfferPriority(ref offer, job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
            }

            return -1;
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>best_index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferPathDistance(ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (job is null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (m_logFile is not null)
            {
                m_logFile.LogInfo($"\r\n### MatchOfferPD ### | {TransferManagerUtils.DebugOffer(job.material, offer, false, true, true)} | CandidateCount: " + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly:{bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            if (!offer.IsValid())
            {
                RemoveInvalidOffer(ref offer);
                if (m_logFile is not null)
                {
                    m_logFile.LogInfo("Offer not valid, skipping");
                }
                return -1;
            }

            if (offer.LocalPark > 0)
            {
                // No need for distance support as it is a pedestrian zone match which teleports magically
                if (m_logFile is not null)
                {
                    m_logFile.LogInfo("Internal pedestrian zone offer, fall back to MatchingPriority");
                }
                return MatchOfferPriority(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
            }

            // Check for Path Distance support
            ushort offerNodeId = offer.GetNearestNode(job.material);
            if (offerNodeId == 0 || m_pathDistance is null)
            {
                // Fall back on LOS
                if (m_logFile is not null)
                {
                    m_logFile.LogInfo("Unable to determine start node for offer or path distance not supported, fall back to MatchOfferLOS");
                }
                return MatchOfferLOS(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
            }

            // Reset candidate array
            m_pathDistance.ClearCandidates();

            // loop through all matching counterpart offers to determine possible candidates
            for (int counterpart_index = 0; counterpart_index < iCandidateCount; counterpart_index++)
            {
                ref CustomTransferOffer candidateOffer = ref offerCandidates[counterpart_index];

                if (candidateOffer.Amount <= 0)
                {
                    continue;
                }

                ExclusionReason reason = ExclusionReason.None;
                    
                // Check candidate is still valid
                if (!candidateOffer.IsValid())
                {
                    RemoveInvalidOffer(ref candidateOffer);
                    reason = ExclusionReason.NotValid;
                }

                // Check offer and candidate can be matched
                if (reason == ExclusionReason.None)
                {
                    if (offer.IsIncoming())
                    {
                        reason = m_transferRestrictions.CanTransfer(job.material, m_eTransferMode, ref offer, ref candidateOffer, bCloseByOnly);
                    }
                    else
                    {
                        reason = m_transferRestrictions.CanTransfer(job.material, m_eTransferMode, ref candidateOffer, ref offer, bCloseByOnly);
                    }
                }

                if (reason == ExclusionReason.None)
                {
                    ushort candidateNodeId = candidateOffer.GetNearestNode(job.material);
                    if (candidateNodeId != 0)
                    {
                        // Check nodes are connected
                        if (PathConnectedCache.IsConnected(job.material, offerNodeId, candidateNodeId))
                        {
                            // Check if node already exists as we want the higher priority item to remain
                            if (m_pathDistance.ContainsNode(candidateNodeId))
                            {
                                reason = ExclusionReason.DuplicateNode;
                            }
                            else
                            {
                                // Add to candidate list
                                m_pathDistance.AddCandidate(candidateNodeId, counterpart_index);
                            }
                        }
                        else
                        {
                            reason = ExclusionReason.NotConnected;
                        }
                    }
                    else
                    {
                        reason = ExclusionReason.NoStartNode;
                    }
                }

                if (m_logFile is not null)
                {
                    m_logFile.LogCandidatePathDistance(counterpart_index, offer, candidateOffer, reason);
                }
            }

            // Display summary
            if (m_logFile is not null)
            {
                m_logFile.LogCandidateSummary();
            }

            // Now select closest candidate based on path distance
            int iBestCandidate = -1;
            if (m_pathDistance.CandidateCount() > 0)
            {
                iBestCandidate = m_pathDistance.FindNearestNeighborId(offer.Active, offerNodeId, out float fTravelTime, out long ticks, out int iNodesExamined);
                if (iBestCandidate == -1)
                {
                    if (m_logFile is not null)
                    {
                        m_logFile.LogInfo($"       Path Distance Match - Failed, no candidate found. StartNode {offerNodeId} CandidateCount: {m_pathDistance.CandidateCount()} NodesExamined:{iNodesExamined}");
                    }
                }
                else
                {
                    if (m_logFile is not null)
                    {
                        m_logFile.LogInfo($"       Path Distance Match - #{iBestCandidate.ToString("0000")} [{offerCandidates[iBestCandidate].m_object.Type}: {offerCandidates[iBestCandidate].m_object.Index}] TravelTime:{fTravelTime} Time:{(ticks * 0.0001).ToString("N3")}ms NodesExamined:{iNodesExamined}");
                    }
                }
            }

            if (iBestCandidate == -1 && !bCloseByOnly)
            {
                // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                RemoveFailedMatch(ref offer);
            }

            return iBestCandidate;
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>match index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferConnectedLOS(ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (job is null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (m_logFile is not null)
            {
                m_logFile.LogInfo($"\r\n### MatchOfferConnectedLOS ### | {TransferManagerUtils.DebugOffer(job.material, offer, false, true, true)} | CandidateCount:" + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly: {bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            return MatchOfferLOSImpl(true, ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>match index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferLOS(ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (job is null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (m_logFile is not null)
            {
                m_logFile.LogInfo($"\r\n### MatchOfferLOS ### | {TransferManagerUtils.DebugOffer(job.material, offer, false, false, true)} | CandidateCount:" + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly: {bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            return MatchOfferLOSImpl(false, ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>match index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferLOSImpl(bool bConnectedMode, ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (!offer.IsValid())
            {
                RemoveInvalidOffer(ref offer);
                if (m_logFile is not null)
                {
                    m_logFile.LogInfo("Offer not valid, skipping");
                }
                return -1;
            }

            if (offer.LocalPark > 0)
            {
                // No need for distance support as it is a pedestrian zone match which teleports magically
                if (m_logFile is not null)
                {
                    m_logFile.LogInfo($"Internal pedestrian zone offer, fall back to MatchingPriority");
                }
                return MatchOfferPriority(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
            }

            // If we are in connected LOS mode then get the offers node.
            ushort offerNodeId = 0;
            if (bConnectedMode)
            {
                offerNodeId = offer.GetNearestNode(job.material);
            }

            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;
            float fAcceptableDistance = GetAcceptableDistance(job.material);

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_index = 0; counterpart_index < iCandidateCount; counterpart_index++)
            {
                ref CustomTransferOffer candidateOffer = ref offerCandidates[counterpart_index];

                if (candidateOffer.Amount <= 0)
                {
                    continue;
                }

                ExclusionReason reason = ExclusionReason.None;

                // Check candidate is still valid
                if (!candidateOffer.IsValid())
                {
                    RemoveInvalidOffer(ref candidateOffer);
                    reason = ExclusionReason.NotValid;
                }

                // Check offer and candidate can be matched
                if (reason == ExclusionReason.None)
                {
                    if (offer.IsIncoming())
                    {
                        reason = m_transferRestrictions.CanTransfer(job.material, m_eTransferMode, ref offer, ref candidateOffer, bCloseByOnly);
                    }
                    else
                    {
                        reason = m_transferRestrictions.CanTransfer(job.material, m_eTransferMode, ref candidateOffer, ref offer, bCloseByOnly);
                    }
                }
                
                // Check nodes are connected as well
                if (reason == ExclusionReason.None && bConnectedMode && offerNodeId != 0)
                {
                    ushort candidateNodeId = candidateOffer.GetNearestNode(job.material);
                    if (candidateNodeId != 0)
                    {
                        if (!PathConnectedCache.IsConnected(job.material, offerNodeId, candidateNodeId))
                        {
                            reason = ExclusionReason.NotConnected;
                        }
                    }
                    else
                    {
                        reason = ExclusionReason.NoStartNode;
                    }
                }

                float distanceOutsideFactor = 0.0f;
                if (reason == ExclusionReason.None)
                {
                    // LOS distance
                    float squaredDistance = Vector3.SqrMagnitude(offer.Position - candidateOffer.Position);

                    // To reproduce vanilla behaviour we accept an offer if within "acceptable" distance, which allows us to exit early
                    // instead of having to find the closest possible match that may be lower priority
                    if (squaredDistance < fAcceptableDistance)
                    {
                        if (m_logFile is not null)
                        {
                            m_logFile.LogCandidateDistanceLOS(counterpart_index, offer, candidateOffer, reason, bConnectedMode, distanceOutsideFactor);
                            m_logFile.LogCandidateSummary();
                            m_logFile.LogInfo("       Match Found - Acceptable distance reached.");
                        }

                        return counterpart_index;
                    }

                    // For some materials types only. Similar to the vanilla matching higher priorities appear closer.
                    float fPriorityFactor = candidateOffer.GetPriorityFactor(job.material);

                    // Apply outside distance modifier to make them more or less desirable.
                    if (offer.IsIncoming())
                    {
                        distanceOutsideFactor = OutsideModifier(job.material, offer, candidateOffer);
                    }
                    else
                    {
                        distanceOutsideFactor = OutsideModifier(job.material, candidateOffer, offer);
                    }

                    // Scale by factors
                    float scaledSquaredDistance = squaredDistance * distanceOutsideFactor * fPriorityFactor;

                    // Check if it is closer than previous best
                    if (scaledSquaredDistance < bestmatch_distance)
                    {
                        // Update closest match
                        bestmatch_position = counterpart_index;
                        bestmatch_distance = scaledSquaredDistance;
                    }
                }

                if (m_logFile is not null)
                {
                    m_logFile.LogCandidateDistanceLOS(counterpart_index, offer, candidateOffer, reason, bConnectedMode, distanceOutsideFactor);
                }
            }

            // Display summary
            if (m_logFile is not null)
            {
                m_logFile.LogCandidateSummary();
            }

            if (bestmatch_position == -1 && !bCloseByOnly)
            {
                // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                RemoveFailedMatch(ref offer);
            }

            return bestmatch_position;
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>match index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferPriority(ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (job is null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (m_logFile is not null)
            {
                m_logFile.LogInfo($"\r\n###MatchOfferPriority | {TransferManagerUtils.DebugOffer(job.material, offer, false, true, true)} | CandidateCount:" + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly: {bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            if (!offer.IsValid())
            {
                RemoveInvalidOffer(ref offer);
                if (m_logFile is not null)
                {
                    m_logFile.LogInfo("Offer not valid, skipping");
                }
                return -1;
            }

            // loop through all matching counterpart offers, the first valid one we find will be the highest priority
            int bestmatch_position = -1;
            for (int counterpart_index = 0; counterpart_index < iCandidateCount; counterpart_index++)
            {
                ref CustomTransferOffer candidateOffer = ref offerCandidates[counterpart_index];

                if (candidateOffer.Amount <= 0)
                {
                    continue;
                }

                ExclusionReason reason = ExclusionReason.None;

                // Check candidate is still valid
                if (!candidateOffer.IsValid())
                {
                    RemoveInvalidOffer(ref candidateOffer);
                    reason = ExclusionReason.NotValid;
                }

                // Check offer and candidate can be matched
                if (reason == ExclusionReason.None)
                {
                    if (offer.IsIncoming())
                    {
                        reason = m_transferRestrictions.CanTransfer(job.material, m_eTransferMode, ref offer, ref candidateOffer, bCloseByOnly);
                    }
                    else
                    {
                        reason = m_transferRestrictions.CanTransfer(job.material, m_eTransferMode, ref candidateOffer, ref offer, bCloseByOnly);
                    }
                }

                if (m_logFile is not null)
                {
                    m_logFile.LogCandidatePriority(counterpart_index, candidateOffer, reason);
                }

                // We select the first candidate that passes the restrictions
                if (reason == ExclusionReason.None)
                {
                    bestmatch_position = counterpart_index;
                    break;
                }
            }

            // Display summary
            if (m_logFile is not null)
            {
                m_logFile.LogCandidateSummary();
            }

            if (bestmatch_position == -1 && !bCloseByOnly)
            {
                // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                RemoveFailedMatch(ref offer);
            }

            return bestmatch_position;
        }

        // -------------------------------------------------------------------------------------------
        private void ApplyMatch(int indexIn, int indexOut)
        {
            if (indexIn != -1 && indexOut != -1)
            {
                // Get offer references so we can adjust values.
                ref CustomTransferOffer incomingOffer = ref job.m_incomingOffers[indexIn];
                ref CustomTransferOffer outgoingOffer = ref job.m_outgoingOffers[indexOut];

                if (m_logFile is not null)
                {
                    m_logFile.LogMatch(incomingOffer, outgoingOffer);
                }

                // Start the transfer
                int deltaamount = Math.Min(incomingOffer.Amount, outgoingOffer.Amount);
                if (deltaamount > 0)
                {
                    CustomTransferDispatcher.Instance.EnqueueTransferResult(job.material, outgoingOffer.m_offer, incomingOffer.m_offer, deltaamount);

                    // reduce offer amount
                    incomingOffer.Amount -= deltaamount;
                    outgoingOffer.Amount -= deltaamount;
                    job.m_incomingAmount -= deltaamount;
                    job.m_outgoingAmount -= deltaamount;

                    if (incomingOffer.Amount <= 0)
                    {
                        job.m_incomingCountRemaining--;
                    }
                    if (outgoingOffer.Amount <= 0)
                    {
                        job.m_outgoingCountRemaining--;
                    }

                    m_iMatches++;
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        private void RemoveFailedMatch(ref CustomTransferOffer offer)
        {
            // Add a log message when we remove offer.
            if (m_logFile is not null)
            {
                m_logFile.LogInfo($"       Match failed - Removing {(offer.IsIncoming() ? "IN" : "OUT")} offer from match set.");
            }

            RemoveOffer(ref offer);
        }

        // -------------------------------------------------------------------------------------------
        private void RemoveInvalidOffer(ref CustomTransferOffer offer)
        {
            // Add a log message when we remove offer.
            if (m_logFile is not null)
            {
                m_logFile.LogInfo($"       Invalid offer - Removing {(offer.IsIncoming() ? "IN" : "OUT")} offer from match set.");
            }

            RemoveOffer(ref offer);
        }

        // -------------------------------------------------------------------------------------------
        private void RemoveOffer(ref CustomTransferOffer offer)
        {
            // remove offer amount
            int iAmount = offer.Amount;
            offer.Amount = 0;

            // Also reduce overall counts/amounts so we can exit early.
            if (offer.IsIncoming())
            {
                job.m_incomingAmount -= iAmount;
                job.m_incomingCountRemaining--;
            }
            else
            {
                job.m_outgoingAmount -= iAmount;
                job.m_outgoingCountRemaining--;
            }
        }

        // -------------------------------------------------------------------------------------------
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private float OutsideModifier(TransferReason material, CustomTransferOffer incoming, CustomTransferOffer outgoing)
        {
            if (incoming.IsOutside() && outgoing.IsOutside())
            {
                // Dont apply multiplier when both outside.
            }
            else if (incoming.IsOutside())
            {
                if (IsExportRestrictionsSupported(material))
                {
                    // Apply building multiplier
                    return (float)Math.Pow(incoming.GetEffectiveOutsideModifier(), 2);
                }
            }
            else if (outgoing.IsOutside())
            {
                if (IsImportRestrictionsSupported(material))
                {
                    // Apply building multiplier
                    return (float)Math.Pow(outgoing.GetEffectiveOutsideModifier(), 2);
                }
            }

            return 1.0f;
        }

        // -------------------------------------------------------------------------------------------
        private bool IsLargeMatchSet()
        {
            return Math.Min(job.m_incomingCount, job.m_outgoingCount) > 1500;
        }

        // -------------------------------------------------------------------------------------------
        private bool CheckCountsAndAmounts()
        {
            // Any matches remaining
            if (job.m_incomingCountRemaining <= 0)
            {
                LogInfo("Break: No IN offers remaining.");
                return false;
            }

            if (job.m_outgoingCountRemaining <= 0)
            {
                LogInfo("Break: No OUT offers remaining.");
                return false;
            }

            // Any amount remaining
            if (job.m_incomingAmount <= 0)
            {
                LogInfo("Break: No IN amounts remaining.");
                return false;
            }

            if (job.m_outgoingAmount <= 0)
            {
                LogInfo("Break: No OUT amounts remaining.");
                return false;
            }

            return true;
        }

        // -------------------------------------------------------------------------------------------
        private void LogInfo(string sMsg)
        {
            if (m_logFile is not null)
            {
                m_logFile.LogInfo(sMsg);
            }
        }

        // -------------------------------------------------------------------------------------------
        private string DebugFlags(bool bWarehouseOnly, bool bFactoryOnly, bool bCloseByOnly)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Flags:");

            bool bFlagAdded = false;
            if (bWarehouseOnly)
            {
                if (bFlagAdded)
                {
                    sb.Append(", ");
                }
                sb.Append("WarehouseOnly");
                bFlagAdded = true;
            }

            if (bFactoryOnly)
            {
                if (bFlagAdded)
                {
                    sb.Append(", ");
                }
                sb.Append("FactoryOnly");
                bFlagAdded = true;
            }

            if (bCloseByOnly)
            {
                if (bFlagAdded)
                {
                    sb.Append(", ");
                }
                sb.Append("CloseByOnly");
                bFlagAdded = true;
            }

            if (!bFlagAdded)
            {
                sb.Append("None");
            }

            return sb.ToString();
        }
    }
}