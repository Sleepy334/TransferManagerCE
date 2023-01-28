using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;
using TransferManagerCE.Util;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using System.Collections.Generic;
using static TransferManagerCE.CustomManager.TransferRestrictions;
using static TransferManagerCE.CustomManager.TransferManagerModes;
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
        public enum MatchOfferDistanceAlgorithm
        {
            PathDistance,
            ConnectedLOS,
            LOS,
        }

        private class CandidateData
        {
            public int m_offerIndex;
            public ushort m_nodeId;
            public float m_fTravelTime = float.MaxValue;
        }

        // References to game managers:
        private static bool s_bInitNeeded = true;
        private static Array16<Building>? Buildings = null;
        private static Array16<Vehicle>? Vehicles = null;
        private static Array32<Citizen>? Citizens = null;

        // Current transfer job from workqueue
        public TransferJob? job = null;
        private PathDistance? m_pathDistance = null;
        private TransferRestrictions m_transferRestrictions;
        private MatchOfferDistanceAlgorithm m_DistanceAlgorithm;
        private Randomizer m_randomizer = new Randomizer();
        private MatchJobLogFile? m_logFile = null;
        private int m_iMatches = 0;
        private Stopwatch m_watch = Stopwatch.StartNew();

        // -------------------------------------------------------------------------------------------
        private static void Init()
        {
            if (s_bInitNeeded)
            {
                s_bInitNeeded = false;
                Buildings = Singleton<BuildingManager>.instance.m_buildings;
                Vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                Citizens = Singleton<CitizenManager>.instance.m_citizens;
            }
        }

        // -------------------------------------------------------------------------------------------
        public CustomTransferManager()
        {
            Init();
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

            if (job == null)
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
            if (PathDistanceTypes.IsPathDistanceSupported(job.material))
            {
                m_DistanceAlgorithm = MatchOfferDistanceAlgorithm.PathDistance;
            }
            else if (PathDistanceTypes.IsConnectedLOSSupported(job.material))
            {
                m_DistanceAlgorithm = MatchOfferDistanceAlgorithm.ConnectedLOS;
            }
            else
            {
                m_DistanceAlgorithm = MatchOfferDistanceAlgorithm.LOS;
            }

            // Check we can actually use path distance and set up the path distance object
            if (m_DistanceAlgorithm == MatchOfferDistanceAlgorithm.PathDistance)
            {
                if (IsLargeMatchSet())
                {
                    // Fall back on simpler algorithm as the match set is large
                    m_DistanceAlgorithm = MatchOfferDistanceAlgorithm.LOS;

                    if (m_logFile != null)
                    {
                        m_logFile.LogInfo("Large match set detected, path distance disabled.");
                    }
                }
                else
                {
                    // Create path distance object if needed.
                    if (m_pathDistance == null)
                    {
                        m_pathDistance = new PathDistance();
                    }

                    // Set up lane requirements
                    PathDistanceTypes.GetService(material, out m_pathDistance.m_service1, out m_pathDistance.m_service2, out m_pathDistance.m_service3);
                    m_pathDistance.m_laneTypes = PathDistanceTypes.GetLaneTypes(material);
                    m_pathDistance.m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(material);
                    m_pathDistance.m_bPedestrianZone = PathDistanceTypes.IsPedestrianZoneService(material);
                }
            }

            if (TransferJobQueue.Instance.Count() > 100)
            {
                // We are falling behind, switch to fast matching to catch back up
                MatchOffersPriority();
            }
            else
            {
                // If WarehouseFirst is set we match warehouses first, then do another match run for other matches
                bool bWarehouseFirst = IsWarehouseMaterial(job.material) && SaveGameSettings.GetSettings().WarehouseFirst;
                if (bWarehouseFirst)
                {
                    // We match IN only first as matching OUT factories first can make some very bad matches
                    MatchIncomingOffers(MatchOfferAlgorithm.Distance, 0, bWarehouseOnly: true, bFactoryOnly: false, bCloseByOnly: false);
                }

                // If FactoryFirst is set we match factories first, then do another match run for other matches
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
                TransferMode eMatchMode = TransferManagerModes.GetTransferMode(material);
                switch (eMatchMode)
                {
                    case TransferMode.Priority:
                        {
                            MatchOffersPriority(); // Just match by priority, distance ignored
                            break;
                        }
                    case TransferMode.OutgoingFirst:
                        {
                            MatchOffersOutgoingFirst(); // OUT loop first then IN loop with CloseByOnly
                            break;
                        }
                    case TransferMode.IncomingFirst:
                        {
                            MatchOffersIncomingFirst(); // IN loop first then OUT loop with CloseByOnly
                            break;
                        }
                    case TransferMode.Balanced:
                    default:
                        {
                            MatchOffersBalanced(); // Match IN and OUT in priority order with distance
                            break;
                        }
                }
            }

            // Record longest match time for stats.
            long jobMatchTimeTicks = m_watch.ElapsedTicks - startTimeTicks;
            if (jobMatchTimeTicks > TransferManagerStats.s_longestMatchTicks)
            {
                TransferManagerStats.s_longestMatchTicks = jobMatchTimeTicks;
                TransferManagerStats.s_longestMaterial = job.material;
            }
           
            // Record longest path distance match time for stats.
            if (m_DistanceAlgorithm == MatchOfferDistanceAlgorithm.PathDistance)
            {
                TransferManagerStats.s_TotalPathDistanceMatchJobs++;
                TransferManagerStats.s_TotalPathDistanceMatchTimeTicks += jobMatchTimeTicks;
            }

            TransferManagerStats.s_TotalMatchJobs++;
            TransferManagerStats.s_TotalMatchTimeTicks += jobMatchTimeTicks;

            // Finished with log file
            if (m_logFile != null)
            {
                m_logFile.LogFooter(job, m_iMatches, jobMatchTimeTicks);
                m_logFile.Close();
                m_logFile = null;
            }
        }

        // -------------------------------------------------------------------------------------------
        // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersOutgoingFirst()
        {
            if (job == null)
            {
                return;
            }

            // Set close by only flags
            bool bCloseByOnlyIncoming = true;
            bool bCloseByOnlyOutgoing = false;

            if (m_logFile != null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"###MatchMode OUTGOING FIRST### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: {bCloseByOnlyIncoming} | CloseByOnlyOutgoing: {bCloseByOnlyOutgoing}");
            }

            // 1. We add an additional loop at start for the vehicles (P:7)
            // for certain services with large vehicle capacity
            switch (job.material)
            {
                case TransferReason.Mail:
                    {
                        MatchIncomingOffers(MatchOfferAlgorithm.Priority, 7, false, false, bCloseByOnlyIncoming);
                        break;
                    }
            }
            
            // 2: Match OUTGOING offers by descending priority first,
            // stop at 0/2 as 1/1 matches are usually not very close by.
            MatchOutgoingOffers(MatchOfferAlgorithm.Distance, 2, false, bCloseByOnlyOutgoing);

            // 3: Now match any INCOMING remaining,
            // stop at 2/0 as 1/1 matches are usually not very close by,
            // and limit the matches to close by only
            MatchIncomingOffers(MatchOfferAlgorithm.Distance, 2, false, false, bCloseByOnlyIncoming);
        }

        // -------------------------------------------------------------------------------------------
        // INCOMING FIRST mode - try to fulfill all incoming requests by finding outgoings by distance
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersIncomingFirst()
        {
            if (job == null)
            {
                return;
            }

            // Get close by only flags
            bool bCloseByOnlyIncoming;
            bool bCloseByOnlyOutgoing;

            switch (job.material)
            {
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                    {
                        bCloseByOnlyIncoming = false; // Match all incoming
                        bCloseByOnlyOutgoing = false; // Match all outgoing
                        break;
                    }
                default:
                    {
                        bCloseByOnlyIncoming = false; // Match all incoming
                        bCloseByOnlyOutgoing = true;  // Only match outgoing if it is close by
                        break;
                    }
            }

            if (m_logFile != null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"###MatchMode INCOMING FIRST### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: {bCloseByOnlyIncoming} | CloseByOnlyOutgoing: {bCloseByOnlyOutgoing}");
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
        private void MatchOffersBalanced()
        {
            // Set CloseByOnly preferences
            bool bCloseByOnlyIncoming = false;
            bool bCloseByOnlyOutgoing = false;

            if (m_logFile != null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"###MatchMode BALANCED### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: {bCloseByOnlyIncoming} | CloseByOnlyOutgoing: {bCloseByOnlyOutgoing}");
            }

            // Now perform normal matches
            MatchOffersBalancedImpl(MatchOfferAlgorithm.Distance, bCloseByOnlyIncoming, bCloseByOnlyOutgoing);
        }

        // -------------------------------------------------------------------------------------------
        // PRIORITY mode - match incoming/outgoing by descending priority only. Ignore distance
        private void MatchOffersPriority()
        {
            if (m_logFile != null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"###MatchMode PRIORITY### | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: false | CloseByOnlyOutgoing: false");
            }

            // Priority doesnt use distance so CloseByOnly flags are always false
            MatchOffersBalancedImpl(MatchOfferAlgorithm.Priority, false, false);
        }

        // -------------------------------------------------------------------------------------------
        // BALANCED match mode implementation - match incoming/outgoing one by one descending priority
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersBalancedImpl(MatchOfferAlgorithm eAlgoritm, bool bCloseByOnlyIncoming, bool bCloseByOnlyOutgoing)
        {
            if (job == null)
            {
                return;
            }

            if (m_logFile != null)
            {
                m_logFile.LogSeparator();
                m_logFile.LogInfo($"###MatchOffersBalancedImpl### | Algorithm: {eAlgoritm} | IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} | OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount} | CloseByOnlyIncoming: false | CloseByOnlyOutgoing: false");
            }

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
                    CustomTransferOffer incoming = job.m_incomingOffers[indexIn];
                    if (incoming.Amount <= 0)
                    {
                        indexIn++;
                        continue;
                    }

                    CustomTransferOffer outgoing = job.m_outgoingOffers[indexOut];
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
                        ApplyMatch(indexIn, MatchIncomingOffer(eAlgoritm, indexIn, bCloseByOnlyIncoming));
                        indexIn++;
                    }
                    else
                    {
                        ApplyMatch(MatchOutgoingOffer(eAlgoritm, indexOut, bCloseByOnlyOutgoing), indexOut);
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

                    ApplyMatch(indexIn, MatchIncomingOffer(eAlgoritm, indexIn, bCloseByOnlyIncoming));
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

                    ApplyMatch(MatchOutgoingOffer(eAlgoritm, indexOut, bCloseByOnlyOutgoing), indexOut);
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
            if (job != null)
            {
                if (m_logFile != null)
                {
                    m_logFile.LogSeparator();
                    m_logFile.LogInfo($"### MatchIncomingOffers ### | Algorithm: {algorithm} | PriorityLimit:{iPriorityLimit} | WarehouseOnly:{bWarehouseOnly} | FactoryOnly:{bFactoryOnly} | CloseByOnly:{bCloseByOnly}");
                }

                for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
                {
                    if (!CheckCountsAndAmounts())
                    {
                        break;
                    }

                    CustomTransferOffer incoming = job.m_incomingOffers[offerIndex];

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && incoming.Priority < iPriorityLimit)
                    {
                        if (m_logFile != null)
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
                        bool bIsWarehouse = incoming.IsWarehouse() && BuildingSettingsStorage.GetSettings(incoming.GetBuilding()).IsWarehouseFirst();
                        if (!bIsWarehouse || m_randomizer.Int32(3U) != 0)
                        {
                            continue;
                        }
                    }

                    ApplyMatch(offerIndex, MatchIncomingOffer(algorithm, offerIndex, bCloseByOnly));
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        private void MatchOutgoingOffers(MatchOfferAlgorithm algorithm, int iPriorityLimit, bool bWarehouseOnly, bool bCloseByOnly)
        {
            // Now match OUTGOING offers by descending priority
            if (job != null)
            {
                if (m_logFile != null)
                {
                    m_logFile.LogSeparator();
                    m_logFile.LogInfo($"### MatchOutgoingOffers ### | Algorithm: {algorithm} | PriorityLimit:{iPriorityLimit} | WarehouseOnly:{bWarehouseOnly} | CloseByOnly:{bCloseByOnly}");
                }

                for (int offerIndex = 0; offerIndex < job.m_outgoingCount; offerIndex++)
                {
                    if (!CheckCountsAndAmounts())
                    {
                        break;
                    }

                    CustomTransferOffer outgoing = job.m_outgoingOffers[offerIndex];

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && outgoing.Priority < iPriorityLimit)
                    {
                        if (m_logFile != null)
                        {
                            m_logFile.LogInfo($"Break: PriorityLimit reached: {iPriorityLimit}.");
                        }
                        break;
                    }

                    // If in warehouse only mode skip over non warehouse offers but only if it isn't disabled for this warehouse
                    if (bWarehouseOnly)
                    {
                        // For outgoing we only occasionally do a WarehouseFirst match just to keep the warehouses ticking over
                        bool bIsWarehouse = outgoing.IsWarehouse() && BuildingSettingsStorage.GetSettings(outgoing.GetBuilding()).IsWarehouseFirst();
                        if (!bIsWarehouse || m_randomizer.Int32(5U) != 0)
                        {
                            continue;
                        }
                    }

                    ApplyMatch(MatchOutgoingOffer(algorithm, offerIndex, bCloseByOnly), offerIndex);
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>best_index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchIncomingOffer(MatchOfferAlgorithm eAlgoritm, int offerIndex, bool bCloseByOnly)
        {
            switch (eAlgoritm)
            {
                case MatchOfferAlgorithm.Distance:
                    {
                        switch (m_DistanceAlgorithm)
                        {
                            case MatchOfferDistanceAlgorithm.PathDistance:
                                {
                                    return MatchOfferPathDistance(ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                                }
                            case MatchOfferDistanceAlgorithm.ConnectedLOS:
                            case MatchOfferDistanceAlgorithm.LOS:
                            default:
                                {
                                    return MatchOfferLOS(ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                                }
                        }
                    }
                case MatchOfferAlgorithm.Priority:
                    {
                        return MatchOfferPriority(ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                    }
            }

            return -1;
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>best_index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOutgoingOffer(MatchOfferAlgorithm eAlgoritm, int offerIndex, bool bCloseByOnly)
        {
            switch (eAlgoritm)
            {
                case MatchOfferAlgorithm.Distance:
                    {
                        switch (m_DistanceAlgorithm)
                        {
                            case MatchOfferDistanceAlgorithm.PathDistance:
                                {
                                    return MatchOfferPathDistance(ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                                }
                            case MatchOfferDistanceAlgorithm.ConnectedLOS:
                            case MatchOfferDistanceAlgorithm.LOS:
                            default:
                                {
                                    return MatchOfferLOS(ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                                }
                        }
                    }         

                case MatchOfferAlgorithm.Priority:
                    return MatchOfferPriority(ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
            }

            return -1;
        }

        // -------------------------------------------------------------------------------------------
        /// <returns>best_index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferPathDistance(ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (job == null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (m_logFile != null)
            {
                m_logFile.LogInfo($"\r\n###MatchOfferPD | {TransferManagerUtils.DebugOffer(job.material, offer, false, true, true)} | CandidateCount: " + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly:{bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            if (!offer.IsValid())
            {
                if (m_logFile != null)
                {
                    m_logFile.LogInfo("Offer not valid");
                }
                return -1;
            }

            if (offer.LocalPark > 0)
            {
                // No need for distance support as it is a pedestrian zone match which teleports magically
                if (m_logFile != null)
                {
                    m_logFile.LogInfo("Internal pedestrian zone offer, fall back to MatchingPriority");
                }
                return MatchOfferPriority(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
            }

            // Check for Path Distance support
            ushort offerNodeId = 0;
            if (CanUsePathingForCandidate(job.material, offer))
            {
                // Now actually see if we get a start node for this offer.
                offerNodeId = offer.GetNearestNode(job.material);
                if (offerNodeId == 0)
                {
                    // We couldn't get a start node for this offer, log it to No road access tab
                    RoadAccessData.AddInstance(offer.m_object);

                    // Fall back on LOS
                    if (m_logFile != null)
                    {
                        m_logFile.LogInfo("Unable to determine start node for offer, fall back to MatchOfferLOS");
                    }
                    return MatchOfferLOS(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
                }
            }
            else
            {
                // Fall back on LOS
                if (m_logFile != null)
                {
                    m_logFile.LogInfo("Path distance not supported for this offer, fall back to MatchOfferLOS");
                }
                return MatchOfferLOS(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
            }

            // loop through all matching counterpart offers to determine possible candidates
            List<CandidateData> candidates = new List<CandidateData>();
            for (int counterpart_index = 0; counterpart_index < iCandidateCount; counterpart_index++)
            {
                ref CustomTransferOffer candidateOffer = ref offerCandidates[counterpart_index];

                if (candidateOffer.Amount <= 0)
                {
                    continue;
                }

                if (!candidateOffer.IsValid())
                {
                    continue;
                }

                ExclusionReason reason;
                if (offer.IsIncoming())
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref offer, ref candidateOffer, bCloseByOnly);
                }
                else
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref candidateOffer, ref offer, bCloseByOnly);
                }
                if (reason == ExclusionReason.None)
                {
                    ushort candidateNodeId = candidateOffer.GetNearestNode(job.material);
                    if (candidateNodeId != 0)
                    {
                        // Check nodes are connected
                        if (UnconnectedGraphCache.IsConnected(job.material, offerNodeId, candidateNodeId))
                        {
                            CandidateData candidate = new CandidateData();
                            candidate.m_offerIndex = counterpart_index;
                            candidate.m_nodeId = candidateOffer.GetNearestNode(job.material);
                            candidates.Add(candidate);
                        }
                        else
                        {
                            reason = ExclusionReason.NotConnected;
                        }
                    }
                    else
                    {
                        // We couldn't get a start node for this candidate offer, log it to No road access tab
                        if (candidateOffer.Building != 0)
                        {
                            RoadAccessData.AddInstance(candidateOffer.m_object);
                        }
                        reason = ExclusionReason.NoStartNode;
                    }
                }

                if (m_logFile != null)
                {
                    m_logFile.LogCandidatePathDistance(counterpart_index, offer, candidateOffer, reason);
                }
            }

            // Display summary
            if (m_logFile != null)
            {
                m_logFile.LogCandidateSummary();
            }

            // Now select closest candidate based on path distance
            int iBestCandidate = -1;
            if (candidates.Count > 0)
            {
                iBestCandidate = GetNearestNeighborBestMatchIndex(job.material, offer, offerCandidates, candidates, out float fTravelTime);
                if (iBestCandidate == -1)
                {
                    // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                    if (!bCloseByOnly)
                    {
                        RemoveFailedMatch(ref offer);
                    }

                    if (m_logFile != null)
                    {
                        m_logFile.LogInfo($"       Path Distance Match - Failed, no candidate found.");
                    }
#if DEBUG
                    // Add a path failure for this match
                    CustomTransferOffer candidateOffer = offerCandidates[candidates[0].m_offerIndex];

                    // Determine which way round we are
                    CustomTransferOffer incoming = (offer.IsIncoming()) ? offer : candidateOffer;
                    CustomTransferOffer outgoing = (offer.IsIncoming()) ? candidateOffer : offer;

                    // Add nodes when debugging so we can fix the issues
                    PathFindFailure.AddFailPair(new InstanceID { NetNode = (ushort)incoming.GetNearestNode(job.material) }, new InstanceID { NetNode = (ushort)outgoing.GetNearestNode(job.material) }, offer.IsOutside() || candidateOffer.IsOutside());
#endif
                }
                else
                {
                    if (m_logFile != null)
                    {
                        m_logFile.LogInfo($"       Path Distance Match - #{iBestCandidate.ToString("0000")} [{offerCandidates[iBestCandidate].m_object.Type}: {offerCandidates[iBestCandidate].m_object.Index}] TravelTime:{fTravelTime}");
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
        private int MatchOfferLOS(ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
            if (job == null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            bool bConnectedMode = (m_DistanceAlgorithm == MatchOfferDistanceAlgorithm.ConnectedLOS);

            if (m_logFile != null)
            {
                m_logFile.LogInfo($"\r\n###" + (bConnectedMode ? "MatchOfferConnectedLOS " : "MatchOfferLOS ") + $" | {TransferManagerUtils.DebugOffer(job.material, offer, false, true, true)} | CandidateCount:" + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly: {bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            if (!offer.IsValid())
            {
                if (m_logFile != null)
                {
                    m_logFile.LogInfo("Offer not valid");
                }
                return -1;
            }

            if (offer.LocalPark > 0)
            {
                // No need for distance support as it is a pedestrian zone match which teleports magically
                if (m_logFile != null)
                {
                    m_logFile.LogInfo($"Internal pedestrian zone offer, fall back to MatchingPriority");
                }
                return MatchOfferPriority(ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
            }

            // If we are in connected LOS mode then get the offers node.
            ushort offerNodeId = 0;
            if (bConnectedMode && CanUsePathingForCandidate(job.material, offer))
            {
                offerNodeId = offer.GetNearestNode(job.material);

                // We couldn't get a start node for this offer, log it to No road access tab
                if (offerNodeId == 0 && offer.Building != 0)
                {
                    RoadAccessData.AddInstance(offer.m_object);
                }
            }

            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_index = 0; counterpart_index < iCandidateCount; counterpart_index++)
            {
                ref CustomTransferOffer candidateOffer = ref offerCandidates[counterpart_index];

                if (candidateOffer.Amount <= 0)
                {
                    continue;
                }

                if (!candidateOffer.IsValid())
                {
                    continue;
                }

                ExclusionReason reason;
                if (offer.IsIncoming())
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref offer, ref candidateOffer, bCloseByOnly);
                }
                else
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref candidateOffer, ref offer, bCloseByOnly);
                }

                // Check nodes are connected as well
                if (reason == ExclusionReason.None && bConnectedMode && offerNodeId != 0)
                {
                    ushort candidateNodeId = candidateOffer.GetNearestNode(job.material);
                    if (candidateNodeId != 0)
                    {
                        if (!UnconnectedGraphCache.IsConnected(job.material, offerNodeId, candidateNodeId))
                        {
                            reason = ExclusionReason.NotConnected;
                        }
                    }
                    else
                    {
                        // We couldn't get a start node for this candidate offer, log it to No road access tab
                        if (candidateOffer.Building != 0)
                        {
                            RoadAccessData.AddInstance(candidateOffer.m_object);
                        }
                        reason = ExclusionReason.NoStartNode;
                    }
                }

                float distanceOutsideFactor = 0.0f;
                if (reason == ExclusionReason.None)
                {
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

                    // LOS distance
                    float squaredDistance = Vector3.SqrMagnitude(offer.Position - candidateOffer.Position) * distanceOutsideFactor * fPriorityFactor;
                    if (squaredDistance < bestmatch_distance)
                    {
                        bestmatch_position = counterpart_index;
                        bestmatch_distance = squaredDistance;
                    }
                }

                if (m_logFile != null)
                {
                    m_logFile.LogCandidateDistanceLOS(counterpart_index, offer, candidateOffer, reason, bConnectedMode, distanceOutsideFactor);
                }
            }

            // Display summary
            if (m_logFile != null)
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
            if (job == null)
            {
                return -1;
            }

            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (m_logFile != null)
            {
                m_logFile.LogInfo($"\r\n###MatchOfferPriority | {TransferManagerUtils.DebugOffer(job.material, offer, false, true, true)} | CandidateCount:" + (offer.IsIncoming() ? job.m_outgoingCountRemaining : job.m_incomingCountRemaining) + $" | CloseByOnly: {bCloseByOnly}");
                m_logFile.ClearCandidateReasons();
            }

            if (!offer.IsValid())
            {
                if (m_logFile != null)
                {
                    m_logFile.LogInfo("Offer not valid");
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

                if (!candidateOffer.IsValid())
                {
                    continue;
                }

                ExclusionReason exclude;
                if (offer.IsIncoming())
                {
                    exclude = m_transferRestrictions.CanTransfer(job.material, ref offer, ref candidateOffer, bCloseByOnly);
                }
                else
                {
                    exclude = m_transferRestrictions.CanTransfer(job.material, ref candidateOffer, ref offer, bCloseByOnly);
                }

                if (m_logFile != null)
                {
                    m_logFile.LogCandidatePriority(counterpart_index, candidateOffer, exclude);
                }

                // We select the first candidate that passes the restrictions
                if (exclude == ExclusionReason.None)
                {
                    bestmatch_position = counterpart_index;
                    break;
                }
            }

            // Display summary
            if (m_logFile != null)
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

                if (m_logFile != null)
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
            if (job != null)
            {
                // Add a log message when we remove offer.
                if (m_logFile != null)
                {
                    m_logFile.LogInfo($"       Match failed - Removing {(offer.IsIncoming() ? "IN" : "OUT")} offer from match set.");
                }

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
        }

        // -------------------------------------------------------------------------------------------
        private int GetNearestNeighborBestMatchIndex(TransferReason material, CustomTransferOffer offer, CustomTransferOffer[] candidateOffers, List<CandidateData> candidates, out float fTravelTime)
        {
            int iBestCandidateIndex = -1;
            fTravelTime = 0.0f;

            if (m_pathDistance != null && candidates.Count > 0)
            {
                ushort uiStartNode = offer.GetNearestNode(material);
                if (uiStartNode != 0)
                {
                    // Create a list of node candidates to send to the path distance algorithm
                    HashSet<ushort> candidateNodes = new HashSet<ushort>();
                    foreach (CandidateData candidate in candidates)
                    {
                        candidateNodes.Add(candidate.m_nodeId);
                    }

                    // Calculate travel time
                    uint uiChosenNode = m_pathDistance.FindNearestNeighbor(offer.Active, uiStartNode, candidateNodes, out fTravelTime);

                    // Now select closest candidate
                    if (uiChosenNode != 0)
                    {
                        // Extract CandidateData for this node
                        for (int i = 0; i < candidates.Count; ++i)
                        {
                            CandidateData candidate = candidates[i];

                            // Note: Multiple candidates may have the same node, but we just choose first one we find
                            if (candidate.m_nodeId == uiChosenNode)
                            {
                                iBestCandidateIndex = candidate.m_offerIndex;
                                break;
                            }
                        }
                    }
                }
            }

            return iBestCandidateIndex;
        }

        // -------------------------------------------------------------------------------------------
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private float OutsideModifier(TransferReason material, CustomTransferOffer incoming, CustomTransferOffer outgoing)
        {
            bool bInIsOutside = incoming.IsOutside();
            bool bOutIsOutside = outgoing.IsOutside();

            if (IsImportRestrictionsSupported(material) && !bInIsOutside && bOutIsOutside)
            {
                // Apply building multiplier
                return (float)Math.Pow(outgoing.GetEffectiveOutsideModifier(), 2);
            }
            else if (IsExportRestrictionsSupported(material) && bInIsOutside && !bOutIsOutside)
            {
                // Apply building multiplier
                return (float)Math.Pow(incoming.GetEffectiveOutsideModifier(), 2);
            }

            return 1.0f;
        }

        // -------------------------------------------------------------------------------------------
        public static bool IsValidObject(InstanceID instance)
        {
            if (instance != null)
            {
                Init();
                switch (instance.Type)
                {
                    case InstanceType.Building:
                        {
                            Building building = Buildings.m_buffer[instance.Building];
                            return building.m_flags != 0;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = Vehicles.m_buffer[instance.Vehicle];
                            return vehicle.m_flags != 0;
                        }
                    case InstanceType.Citizen:
                        {
                            Citizen citizen = Citizens.m_buffer[instance.Citizen];
                            return citizen.m_flags != 0;
                        }
                }
            }

            return true;
        }

        // -------------------------------------------------------------------------------------------
        public static ushort GetOfferBuilding(CustomTransferOffer offer)
        {
            Init();
            switch (offer.m_object.Type)
            {
                case InstanceType.Building:
                    {
                        return offer.Building;
                    }
                case InstanceType.Vehicle:
                    {
                        Vehicle vehicle = Vehicles.m_buffer[offer.Vehicle];
                        if (vehicle.m_flags != 0)
                        {
                            return vehicle.m_sourceBuilding;
                        }
                        break;
                    }
                case InstanceType.Citizen:
                    {
                        Citizen citizen = Citizens.m_buffer[offer.Citizen];
                        if (citizen.m_flags != 0)
                        {
                            return citizen.GetBuildingByLocation();
                        }
                        break;
                    }
                case InstanceType.Park:
                    {
                        // Currently don't support restrictions for ServicePoints
                        break;
                    }
            }

            return 0;
        }

        // -------------------------------------------------------------------------------------------
        private bool CanUsePathingForCandidate(TransferReason material, CustomTransferOffer offer)
        {
            Init();
            if (offer.m_offer.m_isLocalPark > 0)
            {
                // It's a pedestrian zone, don't use pathing as they dont use trucks
                return false;
            }

            switch (offer.m_object.Type)
            {
                case InstanceType.Building:
                    var buildingAI = Buildings.m_buffer[offer.Building].Info.GetAI();
                    switch (buildingAI)
                    {
                        case HelicopterDepotAI:
                            {
                                return false;
                            }
                        case DisasterResponseBuildingAI:
                            {
                                // Collapsed2 is for helicopters, dont use path distance
                                return material != TransferReason.Collapsed2;
                            }
                    }
                    break;
                case InstanceType.Vehicle:
                    if (Vehicles.m_buffer[offer.Vehicle].Info.GetAI() is HelicopterAI)
                    {
                        return false;
                    }
                    break;
            }

            return true;
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
            if (m_logFile != null)
            {
                m_logFile.LogInfo(sMsg);
            }
        }
    }
}