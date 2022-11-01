using ColossalFramework;
using TransferManagerCE.Util;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using TransferManagerCE.Settings;
using System.Collections.Generic;
using static TransferManagerCE.CustomManager.TransferRestrictions;
using System.Threading;
using TransferManagerCE.Data;

namespace TransferManagerCE.CustomManager
{
    public sealed class CustomTransferManager : TransferManager
    {
        private enum WAREHOUSE_OFFERTYPE : int 
        { 
            INCOMING = 1, 
            OUTGOING = 2 
        };
        public enum BalancedMatchModeOption
        {
            MatchModeIncomingFirst = 0, // Vanilla
            MatchModeLeastFirst = 1,
            MatchModePassiveFirst = 2
        }

        private class CandidateData
        {
            public int m_offerIndex;
            public ushort m_nodeId;
            public float m_fTravelTime = float.MaxValue;
        }

        private static Stopwatch s_watch = new Stopwatch();

        // References to game managers:
        private static bool s_bInitNeeded = true;
        private static Array16<Building>? Buildings = null;
        private static Array16<Vehicle>? Vehicles = null;
        private static Array32<Citizen>? Citizens = null;

        // Current transfer job from workqueue
        public TransferJob? job = null;
        private PathDistance? m_pathDistance = null;
        private TransferRestrictions m_transferRestrictions;
        private bool m_bWarehouseOnly;
        private bool m_bPathDistanceSupported;
        private int m_iMatches = 0;


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

        public CustomTransferManager()
        {
            Init();
            m_transferRestrictions = new TransferRestrictions();
        }

#if DEBUG
        private void DebugPrintAllOffers(TransferReason material, int offerCountIncoming, int offerCountOutgoing)
        {
            for (int i = 0; i < offerCountIncoming; i++)
            {
                ref CustomTransferOffer incomingOffer = ref job.m_incomingOffers[i];
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   in #{i}: {TransferManagerUtils.DebugOffer(incomingOffer)}");
            }

            for (int i = 0; i < offerCountOutgoing; i++)
            {
                ref CustomTransferOffer outgoingOffer = ref job.m_outgoingOffers[i];
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   out #{i}: {TransferManagerUtils.DebugOffer(outgoingOffer)}");
            }
        }
#endif

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

            if (!s_watch.IsRunning)
            {
                s_watch.Start();
            }
            long startTime = s_watch.ElapsedMilliseconds;
            TransferManagerStats.UpdateLargestMatch(job);

            // Reset members as we re-use the match jobs.
            m_iMatches = 0;
            m_bWarehouseOnly = false;
            m_bPathDistanceSupported = PathDistanceTypes.IsPathDistanceSupported(job.material);
            m_transferRestrictions.SetMaterial(material);

#if (DEBUG)
            // DEBUG LOGGING
            DebugLog.LogOnly((DebugLog.LogReason)material, $"-- TRANSFER REASON: {material}, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount}, count in {job.m_incomingCount}, count out {job.m_outgoingCount}");
            DebugPrintAllOffers(material, job.m_incomingCount, job.m_outgoingCount);
#endif
            if (TransferJobQueue.Instance.Count() > 100)
            {
                // We are falling behind, switch to fast matching to catch back up
                MatchOffersPriority();
            }
            else
            {
                switch (material)
                {
                    // These are all 1-E08 in the vanilla GetDistanceMultiplier so would effectively match first offer
                    case TransferReason.PartnerYoung:
                    case TransferReason.PartnerAdult:
                    case TransferReason.Family0:
                    case TransferReason.Family1:
                    case TransferReason.Family2:
                    case TransferReason.Family3:
                    case TransferReason.Single0:
                    case TransferReason.Single1:
                    case TransferReason.Single2:
                    case TransferReason.Single3:
                    case TransferReason.Single0B:
                    case TransferReason.Single1B:
                    case TransferReason.Single2B:
                    case TransferReason.Single3B:
                    case TransferReason.LeaveCity0:
                    case TransferReason.LeaveCity1:
                    case TransferReason.LeaveCity2:
                    case TransferReason.DummyCar: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                    case TransferReason.DummyTrain: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                    case TransferReason.DummyShip: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                    case TransferReason.DummyPlane: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                    case TransferReason.TouristA: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                    case TransferReason.TouristB: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                    case TransferReason.TouristC: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                    case TransferReason.TouristD: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                        {
                            MatchOffersPriority(); // Just match by priority, distance ignored
                            break;
                        }
                    case TransferReason.Garbage:        // We now have a custom loop for garbage, which matches trucks first (priority 7) then matches bodies to cemeteries after that
                    case TransferReason.Dead:           // We now have a custom loop for dead, which matches hearses first (priority 7) then matches bodies to cemeteries after that
                    case TransferReason.Crime:          // We now have a custom loop for crime, which matches police cars first (priority 7) then matches crime to police stations after that
                    case TransferReason.Mail:           // We now have a custom loop for Mail, which matches service vehicles first then issue to nearby service depot.
                    case TransferReason.Collapsed:      // We now have a custom loop for Collapsed, which matches service vehicles first then issue to nearby service depot.
                    case TransferReason.Collapsed2:     // We now have a custom loop for Collapsed2, which matches service vehicles first then issue to nearby service depot.
                    case TransferReason.Fire:           // We always want the closest fire station to respond
                    case TransferReason.Fire2:          // We always want the closest fire station to respond
                    case TransferReason.ForestFire:     // We always want the closest fire station to respond
                    case TransferReason.Sick:           // The hospitals put out priority 7 offers so end up getting matched first all the time which produces worse matches.
                    case TransferReason.Sick2:          // The hospitals put out priority 7 offers so end up getting matched first all the time which produces worse matches.
                    case TransferReason.SickMove:       // outgoing(active) from medcopter, incoming(passive) from hospitals -> moved to outgoing first so closest clinic is used
                    case TransferReason.Student1:       // Match elementary to closest school
                    case TransferReason.Student2:       // Match high school to closest school
                        {
                            MatchOffersOutgoingFirst();
                            break;
                        }
                    case TransferReason.Taxi:
                    case TransferReason.RoadMaintenance: // RoadMaintenance is OUT from service depot. Match vehicles (7) first then closest depot to segment
                        {
                            MatchOffersIncomingFirst();
                            break;
                        }
                    default:
                        {
                            // If WarehouseFirst is set we now literally match warehouses first, then do another match run for other matches
                            if (SaveGameSettings.GetSettings().WarehouseFirst && TransferManagerModes.IsWarehouseMaterial(job.material))
                            {
                                try
                                {
                                    m_bWarehouseOnly = true;

                                    // We match IN only first as matching OUT warehouses first can make some very bad matches
                                    MatchIncomingOffers(0);
                                }
                                finally
                                {
                                    m_bWarehouseOnly = false;
                                }
                            }

                            // Now perform normal matches
                            MatchOffersBalanced();
                            break;
                        }
                }
            }

            // Record longest match time for stats.
            long jobMatchTime = s_watch.ElapsedMilliseconds - startTime;
            if (jobMatchTime > TransferManagerStats.s_longestMatch)
            {
                TransferManagerStats.s_longestMatch = jobMatchTime;
                TransferManagerStats.s_longestMaterial = job.material;
#if (DEBUG)
                Debug.Log("Thread: " + Thread.CurrentThread.ManagedThreadId + " Material: " + material + " IN: " + job.m_incomingCount + " OUT: " + job.m_outgoingCount + " Elapsed time: " + jobMatchTime + "ms");
#endif  
            }

            //if (material == TransferReason.Goods)
            //    Debug.Log($"Thread:{Thread.CurrentThread.ManagedThreadId} Material:{material} IN:{job.m_incomingCount} OUT:{job.m_outgoingCount} Matches:{m_iMatches} Elapsed time:{jobMatchTime}ms");
           
            // Record longest path distance match time for stats.
            if (m_bPathDistanceSupported)
            {
                TransferManagerStats.s_TotalPathDistanceMatchJobs++;
                TransferManagerStats.s_TotalPathDistanceMatchTimeMS += jobMatchTime;
            }

            TransferManagerStats.s_TotalMatchJobs++;
            TransferManagerStats.s_TotalMatchTimeMS += jobMatchTime;
        }

        
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersOutgoingFirst()
        {
            // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
            // -------------------------------------------------------------------------------------------
            if (job == null)
            {
                return;
            }
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode OUTGOING FIRST###");
#endif
            
            int iMatchPriorityLimit = 0; // 0 is not set
            switch (job.material)
            {
                case TransferReason.Dead:
                case TransferReason.Garbage:
                case TransferReason.Crime:
                    // Stop matching at priority 2 as 1/1 matches are usually not very good whereas
                    // priority 2 can still be matched with 0.
                    iMatchPriorityLimit = 2;
                    break;
            }

            // 1st loop: all OUTGOING offers by descending priority
            MatchOutgoingOffers(iMatchPriorityLimit);

            // Now match any incoming remaining
            MatchIncomingOffers(iMatchPriorityLimit);
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersIncomingFirst()
        {
            if (job == null)
            {
                return;
            }

            // INCOMING FIRST mode - try to fulfill all incoming requests by finding outgoings by distance
            // -------------------------------------------------------------------------------------------
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode INCOMING FIRST###");
#endif
            // 1st loop: Match any incoming remaining
            MatchIncomingOffers(0);

            // Now match OUTGOING offers by descending priority
            MatchOutgoingOffers(0);
        }

        private void MatchIncomingOffers(int iPriorityLimit)
        {
            // Now match OUTGOING offers by descending priority
            if (job != null)
            {
                for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
                {
                    // Any matches remaining
                    // If there is only 1 outgoing remaining then just match it below.
                    if (job.m_incomingCountRemaining <= 0 || job.m_outgoingCountRemaining <= 1)
                    {
                        break;
                    }
                    // Any amount remaining
                    if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0)
                    {
                        break;
                    }

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && job.m_incomingOffers[offerIndex].Priority < iPriorityLimit)
                    {
                        break;
                    }

                    // If in warehouse only mode skip over non warehouse offers
                    if (m_bWarehouseOnly && !job.m_incomingOffers[offerIndex].IsWarehouse())
                    {
                        continue;
                    }

                    ApplyMatch(offerIndex, MatchIncomingOffer(offerIndex));
                }
            }
        }

        private void MatchOutgoingOffers(int iPriorityLimit)
        {
            // Now match OUTGOING offers by descending priority
            if (job != null)
            {
                for (int offerIndex = 0; offerIndex < job.m_outgoingCount; offerIndex++)
                {
                    // Any matches remaining
                    if (job.m_incomingCountRemaining <= 0 || job.m_outgoingCountRemaining <= 0)
                    {
                        break;
                    }

                    // Any amount remaining
                    if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0)
                    {
                        break;
                    }

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && job.m_outgoingOffers[offerIndex].Priority < iPriorityLimit)
                    {
                        break;
                    }

                    // If in warehouse only mode skip over non warehouse offers
                    if (m_bWarehouseOnly && !job.m_outgoingOffers[offerIndex].IsWarehouse())
                    {
                        continue;
                    }

                    ApplyMatch(MatchOutgoingOffer(offerIndex), offerIndex);
                }
            }
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersBalanced()
        {
            // BALANCED mode - match incoming/outgoing one by one by distance, descending priority
            // -------------------------------------------------------------------------------------------
            BalancedMatchModeOption matchMode = SaveGameSettings.s_SaveGameSettings.BalancedMatchMode;
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode BALANCED### WarehouseOnly:{m_bWarehouseOnly} IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount}");
#endif
            // loop incoming+outgoing offers by descending priority
            int indexIn = 0;
            int indexOut = 0;
            while (indexIn < job.m_incomingCount || indexOut < job.m_outgoingCount)
            {
                // Any matches remaining
                if (job.m_incomingCountRemaining <= 0 || job.m_outgoingCountRemaining <= 0)
                {
                    break;
                }
                // Any amount remaining
                if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0)
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

                    int current_prio = Math.Max(incoming.Priority, outgoing.Priority);
 
                    // If current priority is 0 then we can't match anymore as 0/0 is not a valid match
                    // and all the other priorities should already have had a match attempted.
                    if (!m_bWarehouseOnly && current_prio == 0)
                    {
                        break;
                    }

                    if (incoming.Priority == outgoing.Priority)
                    {
                        switch (matchMode)
                        {
                            case BalancedMatchModeOption.MatchModeIncomingFirst: // vanilla
                                {
                                    // Match incoming first (Vanilla mode)
                                    ApplyMatch(indexIn, MatchIncomingOffer(indexIn));
                                    indexIn++;
                                    break;
                                }
                            case BalancedMatchModeOption.MatchModeLeastFirst:
                                {
                                    // Match whichever has less offers available so that we maximise the matches for the side with
                                    // limited resources.
                                    if (job.m_incomingCountRemaining <= job.m_outgoingCountRemaining)
                                    {
                                        ApplyMatch(indexIn, MatchIncomingOffer(indexIn));
                                        indexIn++;
                                    }
                                    else
                                    {
                                        ApplyMatch(MatchOutgoingOffer(indexOut), indexOut);
                                        indexOut++;
                                    }
                                    break;
                                }
                            case BalancedMatchModeOption.MatchModePassiveFirst:
                                {
                                    // Match against passive side first
                                    if (!incoming.Active)
                                    {
                                        ApplyMatch(indexIn, MatchIncomingOffer(indexIn));
                                        indexIn++;
                                    }
                                    else
                                    {
                                        ApplyMatch(MatchOutgoingOffer(indexOut), indexOut);
                                        indexOut++;
                                    }
                                    break;
                                }
                        }
                        
                    }
                    else if (incoming.Priority > outgoing.Priority)
                    {
                        ApplyMatch(indexIn, MatchIncomingOffer(indexIn));
                        indexIn++;
                    }
                    else
                    {
                        ApplyMatch(MatchOutgoingOffer(indexOut), indexOut);
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
                    if (!m_bWarehouseOnly && incoming.Priority == 0)
                    {
                        break;
                    }

                    ApplyMatch(indexIn, MatchIncomingOffer(indexIn));
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
                    if (!m_bWarehouseOnly && outgoing.Priority == 0)
                    {
                        break;
                    }

                    ApplyMatch(MatchOutgoingOffer(indexOut), indexOut);
                    indexOut++;
                }
                else
                {
                    break;
                }
            }
        }

        private void MatchOffersPriority()
        {
            // PRIORITY mode - match incoming/outgoing by descending priority only. Ignore distance
            // -------------------------------------------------------------------------------------------
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode PRIORITY###");
#endif

            // Don't match with outside connection till higher priority when leaving the city.
            bool bOutsideConnectionLowPriorityIncrease = false;
            switch (job.material)
            {
                case TransferReason.PartnerYoung:
                case TransferReason.PartnerAdult:
                case TransferReason.Family0:
                case TransferReason.Family1:
                case TransferReason.Family2:
                case TransferReason.Family3:
                case TransferReason.Single0:
                case TransferReason.Single1:
                case TransferReason.Single2:
                case TransferReason.Single3:
                case TransferReason.Single0B:
                case TransferReason.Single1B:
                case TransferReason.Single2B:
                case TransferReason.Single3B:
                    bOutsideConnectionLowPriorityIncrease = true;
                    break;
            }

            // loop incoming+outgoing offers by descending priority
            int indexIn = 0;
            int indexOut = 0;
            while (indexIn < job.m_incomingCount && indexOut < job.m_outgoingCount)
            {
                // Any matches remaining
                if (job.m_incomingCountRemaining <= 0 || job.m_outgoingCountRemaining <= 0)
                {
                    break;
                }
                // Any amount remaining
                if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0)
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

                    // Check for low priority and break if found as we are in descending priorty order.
                    if (outgoing.Priority + incoming.Priority < 2)
                    {
                        break;
                    }

                    // Perform minimal set of fast checks only
                    ExclusionReason reason = m_transferRestrictions.CanTransferFastChecksOnly(job.material, ref incoming, ref outgoing, m_bWarehouseOnly);
                    if (reason != ExclusionReason.None)
                    {
                        indexOut++;
                        continue;
                    }

                    // Don't match with outside connection till higher priority when leaving the city.
                    if (bOutsideConnectionLowPriorityIncrease && incoming.IsOutside() && 
                        (outgoing.Priority + incoming.Priority < 3))
                    {
                        indexIn++;
                        continue;
                    }

                    ApplyMatch(indexIn, indexOut);

                    if (outgoing.Amount <= 0)
                    {
                        indexOut++;
                    }
                    if (incoming.Amount <= 0)
                    {
                        indexIn++;
                    }
                }
            }
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchIncomingOffer(int offerIndex)
        {
            return MatchOffer(true, ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount);
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOutgoingOffer(int offerIndex)
        {
            return MatchOffer(false, ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount);
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOffer(bool bIncoming, ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount)
        {
            if (job == null)
            {
                return -1;
            }
            
            if (offer.Amount <= 0)
            {
                return -1;
            }

            if (!offer.IsValid())
            {
                return -1;
            }

            if (m_bWarehouseOnly)
            {
                // In warehouse first mode, we only match warehouses
                if (!offer.IsWarehouse())
                {
                    return -1;
                }
                
                // Check if building override option turns warehouse first off for this building
                if (offer.GetBuilding() != 0 && !BuildingSettings.IsWarehouseFirst(offer.GetBuilding()))
                {
                    return -1;
                }
            }
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"\r\n{Thread.CurrentThread.ManagedThreadId}   ###Matching " + (bIncoming ? "INCOMING" : "OUTGOING") + $" Offer: {TransferManagerUtils.DebugOffer(offer)} CandidateCount:{iCandidateCount} WarehouseOnly: {m_bWarehouseOnly}");
#endif
            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;

            // Path Distance support
            List<CandidateData> candidates = new List<CandidateData>();
            bool bIsPathDistanceSupported = m_bPathDistanceSupported && CanUsePathingForCandidate(job.material, offer);

            // Now actually see if we get a start node for this offer.
            ushort offerNodeId = 0;
            if (bIsPathDistanceSupported)
            {
                offerNodeId = offer.GetNearestNode(job.material);
                if (offerNodeId == 0)
                {
                    // We couldn't get a start node for this offer, log it to No road access tab
                    RoadAccessData.AddInstance(offer.m_object);
                    bIsPathDistanceSupported = false;
                }
            }

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
                if (bIncoming)
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref offer, ref candidateOffer, m_bWarehouseOnly);
                }
                else
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref candidateOffer, ref offer, m_bWarehouseOnly);
                }
                if (reason == ExclusionReason.None)
                {
                    // Apply selected match distance algorithm
                    if (bIsPathDistanceSupported)
                    {
                        ushort candidateNodeId = candidateOffer.GetNearestNode(job.material);
                        if (candidateNodeId != 0)
                        {
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
#if DEBUG
                                //Debug.Log($"Material:{job.material} NotConnected:{TransferManagerUtils.DebugOffer(offer)} -> {TransferManagerUtils.DebugOffer(candidateOffer)}");
                                //PathFindFailure.AddFailPair(offer.m_object, candidateOffer.m_object, offer.IsOutside() || candidateOffer.IsOutside());                 
#endif 
                            }
                        }
                        else
                        {
                            // We couldn't get a start node for this candidate offer, log it to No road access tab
                            RoadAccessData.AddInstance(candidateOffer.m_object);
                        }
#if DEBUG
                        double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - candidateOffer.Position));
                        DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{Thread.CurrentThread.ManagedThreadId}       -> Matching " + (bIncoming ? "OUTGOING" : "INCOMING") + $" WarehouseOnly:{m_bWarehouseOnly} Offer: #{counterpart_index} {TransferManagerUtils.DebugOffer(candidateOffer)}, amt {candidateOffer.Amount}, Node:{candidateNodeId} Distance :{dDistance}");
#endif    
                    }
                    else
                    {
                        // For some materials types only. Similar to the vanilla matching higher priorities appear closer.
                        float fPriorityFactor = PriorityModifier(candidateOffer, job.material);

                        // Apply outside distance modifier to make them more or less desirable.
                        float distanceOutsideFactor;
                        if (bIncoming)
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
#if DEBUG
                        double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - candidateOffer.Position));
                        DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{Thread.CurrentThread.ManagedThreadId}       -> Matching " + (bIncoming ? "OUTGOING" : "INCOMING") + $" WarehouseOnly:{m_bWarehouseOnly} Offer: #{counterpart_index} {TransferManagerUtils.DebugOffer(candidateOffer)}, amt {candidateOffer.Amount}, Node:" + (bIsPathDistanceSupported ? candidateOffer.m_nearestNode : "NA") + $" distance : {dDistance} OF:{distanceOutsideFactor}/PF:{fPriorityFactor}, bestmatch: {bestmatch_distance}");
#endif    
                    }
                }
#if DEBUG
                if (reason != ExclusionReason.None)
                {
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{Thread.CurrentThread.ManagedThreadId}       -> Matching " + (bIncoming ? "OUTGOING" : "INCOMING") + $" WarehouseOnly:{m_bWarehouseOnly} Offer: #{counterpart_index} {TransferManagerUtils.DebugOffer(candidateOffer)}, amt {candidateOffer.Amount}, Node:" + (bIsPathDistanceSupported ? candidateOffer.m_nearestNode : "NA") + $" Exclusion: {reason}");
                }
#endif
            }

            int iBestCandidate = -1;
            if (bIsPathDistanceSupported)
            {
                // Path distance
                if (candidates.Count > 0)
                {
                    iBestCandidate = GetNearestNeighborBestMatchIndex(job.material, offer, offerCandidates, candidates);
                    if (iBestCandidate == -1)
                    {
#if DEBUG
                        // DEBUGGING   
                        string sCandidates = "";
                        foreach (CandidateData candidate in candidates)
                        {
                            sCandidates += candidate.m_nodeId + ", ";
                        }
                        Debug.Log($"Match failed: {job.material} {TransferManagerUtils.DebugOffer(offer)} Candidates: {sCandidates}");
#endif
                        // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                        if (!m_bWarehouseOnly)
                        {
                            RemoveFailedMatch(bIncoming, ref offer);
                        }

                        // Add a path failure for this match
                        CustomTransferOffer candidateOffer = offerCandidates[candidates[0].m_offerIndex];
                        
                        // Determine which way round we are
                        CustomTransferOffer incoming = (bIncoming) ? offer : candidateOffer;
                        CustomTransferOffer outgoing = (bIncoming) ? candidateOffer : offer;

                        // Add to path failures
                        PathFindFailure.AddFailPair(incoming.m_object, outgoing.m_object, offer.IsOutside() || candidateOffer.IsOutside());
#if DEBUG
                        // Add nodes when debugging so we can fix the issues
                        PathFindFailure.AddFailPair(new InstanceID { NetNode = (ushort)incoming.GetNearestNode(job.material) }, new InstanceID { NetNode = (ushort)outgoing.GetNearestNode(job.material) }, offer.IsOutside() || candidateOffer.IsOutside());
#endif
                    }
                }
            }
            else
            {
                // LOS distance
                iBestCandidate = bestmatch_position;
                if (bestmatch_position == -1)
                {
                    // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                    if (!m_bWarehouseOnly)
                    {
                        RemoveFailedMatch(bIncoming, ref offer);
                    }
                }
            }

#if DEBUG
            if (iBestCandidate != -1)
            {
                double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - offerCandidates[iBestCandidate].Position));
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{job.material} Best#:{iBestCandidate} Object: {offerCandidates[iBestCandidate].m_object.Type} Index: {offerCandidates[iBestCandidate].m_object.Index} Distance: {dDistance}");
            }
#endif
            return iBestCandidate;
        }

        private void ApplyMatch(int indexIn, int indexOut)
        {
            if (indexIn != -1 && indexOut != -1)
            {
                // Get offer references so we can adjust values.
                ref CustomTransferOffer incomingOffer = ref job.m_incomingOffers[indexIn];
                ref CustomTransferOffer outgoingOffer = ref job.m_outgoingOffers[indexOut];
#if DEBUG
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> ApplyMatch: {TransferManagerUtils.DebugMatch(job.material, outgoingOffer, incomingOffer)}");
#endif
                // Start the transfer
                int deltaamount = Math.Min(incomingOffer.Amount, outgoingOffer.Amount);
                if (deltaamount > 0)
                {
                    m_iMatches++;
                    TransferResultQueue.Instance.EnqueueTransferResult(job.material, outgoingOffer.m_offer, incomingOffer.m_offer, deltaamount);

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
                }
            }
        }

        private void RemoveFailedMatch(bool bIncoming, ref CustomTransferOffer offer)
        {
            if (job != null)
            {
                // remove offer amount
                int iAmount = offer.Amount;
                offer.Amount = 0;

                // Also reduce overall counts/amounts so we can exit early.
                if (bIncoming)
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

        private int GetNearestNeighborBestMatchIndex(TransferReason material, CustomTransferOffer offer, CustomTransferOffer[] candidateOffers, List<CandidateData> candidates)
        {
            int iBestCandidateIndex = -1;
            if (m_bPathDistanceSupported && candidates.Count > 0)
            {
                // Create path distance object if needed.
                if (m_pathDistance == null)
                {
                    m_pathDistance = new PathDistance();
                }

                ushort uiStartNode = offer.GetNearestNode(job.material);
                if (uiStartNode != 0)
                {
                    NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(material);

                    // Create a list of node candidates to send to the path distance algorithm
                    HashSet<ushort> candidateNodes = new HashSet<ushort>();
                    foreach (CandidateData candidate in candidates)
                    {
                        candidateNodes.Add(candidate.m_nodeId);
                    }

                    // Calculate travel time
                    uint uiChosenNode = m_pathDistance.FindNearestNeighbor(offer.Active, uiStartNode, candidateNodes, laneType);

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

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private float WarehouseFirst(CustomTransferOffer offer, TransferReason material, WAREHOUSE_OFFERTYPE whInOut)
        {
            const float WAREHOUSE_MODIFIER = 0.1f;   //modifier for distance for warehouse

            //TransferOffer.Exclude is only ever set by WarehouseAI!
            if (offer.Exclude && BuildingSettings.IsWarehouseFirst(offer.GetBuilding()))
            {
                Building.Flags flags = Buildings.m_buffer[offer.GetBuilding()].m_flags;
                bool isFilling = (flags & Building.Flags.Filling) == Building.Flags.Filling;
                bool isEmptying = (flags & Building.Flags.Downgrading) == Building.Flags.Downgrading;

                // Filling Warehouses dont like to fulfill outgoing offers,
                // emptying warehouses dont like to fulfill incoming offers
                if ((whInOut == WAREHOUSE_OFFERTYPE.INCOMING && isEmptying) ||
                    (whInOut == WAREHOUSE_OFFERTYPE.OUTGOING && isFilling))
                {
                    return WAREHOUSE_MODIFIER * 2;   //distance factorSqrt x2 further away
                }
                else
                {
                    return WAREHOUSE_MODIFIER;       //WarehouseDIstanceFactorSqr = 1 / 10
                }
            }

            return 1f;
        }

        private float PriorityModifier(CustomTransferOffer offer, TransferReason material)
        {
            switch (material)
            {
                case TransferReason.PartnerYoung:
                case TransferReason.PartnerAdult:
                case TransferReason.Family0:
                case TransferReason.Family1:
                case TransferReason.Family2:
                case TransferReason.Family3:
                case TransferReason.Single0:
                case TransferReason.Single1:
                case TransferReason.Single2:
                case TransferReason.Single3:
                case TransferReason.Single0B:
                case TransferReason.Single1B:
                case TransferReason.Single2B:
                case TransferReason.Single3B:
                case TransferReason.LeaveCity0:
                case TransferReason.LeaveCity1:
                case TransferReason.LeaveCity2:
                case TransferReason.Worker0:
                case TransferReason.Worker1:
                case TransferReason.Worker2:
                case TransferReason.Worker3:
                case TransferReason.Student1:
                case TransferReason.Student2:
                case TransferReason.Student3:
                case TransferReason.Entertainment:
                case TransferReason.Shopping:
                case TransferReason.ShoppingB:
                case TransferReason.ShoppingC:
                case TransferReason.ShoppingD:
                case TransferReason.ShoppingE:
                case TransferReason.ShoppingF:
                case TransferReason.ShoppingG:
                case TransferReason.ShoppingH:
                case TransferReason.EntertainmentB:
                case TransferReason.EntertainmentC:
                case TransferReason.EntertainmentD:
                case TransferReason.TouristA:
                case TransferReason.TouristB:
                case TransferReason.TouristC:
                case TransferReason.TouristD:
                case TransferReason.SortedMail:
                case TransferReason.OutgoingMail:
                case TransferReason.IncomingMail:
                    return 1.0f / (offer.Priority + 1); // Scale by priority. Higher priorities will appear closer
                default:
                    {
                        return 1.0f; // No priority scaling
                    }
            }
        }

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
                        return vehicle.m_sourceBuilding;
                    }
                case InstanceType.Citizen:
                    {
                        Citizen citizen = Citizens.m_buffer[offer.Citizen];
                        return citizen.GetBuildingByLocation();
                    }
                case InstanceType.Park:
                    {
                        // Currently don't support restrictions for ServicePoints
                        break;
                    }
            }

            return 0;
        }

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
    }
}