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
using static TransferManagerCE.CustomManager.TransferManagerModes;
using ColossalFramework.Math;

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
        private bool m_bPathDistanceSupported;
        private Randomizer m_randomizer = new Randomizer();

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
            m_transferRestrictions.SetMaterial(material);

            // Pathing support
            m_bPathDistanceSupported = PathDistanceTypes.IsPathDistanceSupported(job.material);  
            if (m_bPathDistanceSupported)
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
            }

#if (DEBUG)
            // DEBUG LOGGING
            DebugLog.LogOnly((DebugLog.LogReason)job.material, $"------ TRANSFER JOB: {job.material.ToString()}, IN: {job.m_incomingAmount}({job.m_incomingCount}) OUT: {job.m_outgoingAmount}({job.m_outgoingCount}) ------");
            DebugPrintAllOffers(material, job.m_incomingCount, job.m_outgoingCount);
#endif
            if (TransferJobQueue.Instance.Count() > 100)
            {
                // We are falling behind, switch to fast matching to catch back up
                MatchOffersPriority();
            }
            else
            {
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
                            MatchOffersOutgoingFirst();
                            break;
                        }
                    case TransferMode.IncomingFirst:
                        {
                            MatchOffersIncomingFirst();
                            break;
                        }
                    case TransferMode.Balanced:
                    default:
                        {
                            // If WarehouseFirst is set we match warehouses first, then do another match run for other matches
                            if (TransferManagerModes.IsWarehouseMaterial(job.material) && SaveGameSettings.GetSettings().WarehouseFirst)
                            {
                                // We match IN only first as matching OUT warehouses/factories first can make some very bad matches
                                MatchIncomingOffers(0, bWarehouseOnly: true, bFactoryOnly: false, bCloseByOnly: false);
                            }

                            // If FactoryFirst is set we match factories first, then do another match run for other matches
                            if (TransferManagerModes.IsFactoryMaterial(job.material) && SaveGameSettings.GetSettings().FactoryFirst)
                            {
                                // We match IN only first as matching OUT warehouses/factories first can make some very bad matches
                                MatchIncomingOffers(0, bWarehouseOnly: false, bFactoryOnly: true, bCloseByOnly: false);
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
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode OUTGOING FIRST### IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount}");
#endif
            // 1: Match OUTGOING offers by descending priority first,
            // stop at 0/2 as 1/1 matches are usually not very close by.
            MatchOutgoingOffers(2, bCloseByOnly: false);

            // 2: Now match any INCOMING remaining,
            // stop at 2/0 as 1/1 matches are usually not very close by,
            // and limit the matches to close by only
            MatchIncomingOffers(2, false, false, bCloseByOnly: true);
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
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode INCOMING FIRST### IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount}");
#endif
            // 1: Match INCOMING offers by descending priority first,
            // stop at 2/0 as 1/1 matches are usually not very close by
            MatchIncomingOffers(2, false, false, false);

            // 2: Now match OUTGOING offers by descending priority,
            // stop at 0/2 as 1/1 matches are usually not very close by
            // and limit the matches to close by only
            MatchOutgoingOffers(2, bCloseByOnly: true);
        }

        private void MatchIncomingOffers(int iPriorityLimit, bool bWarehouseOnly, bool bFactoryOnly, bool bCloseByOnly)
        {
            // Now match INCOMING offers by descending priority
            if (job != null)
            {
                for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
                {
                    // Any matches remaining
                    if (job.m_incomingCountRemaining <= 0)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: No incoming counts remaining.");
#endif
                        break;
                    }

                    if (job.m_outgoingCountRemaining <= 0)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: No outgoing counts remaining.");
#endif
                        break;
                    }

                    // Any amount remaining
                    if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: No amounts remaining.");
#endif
                        break;
                    }

                    CustomTransferOffer incoming = job.m_incomingOffers[offerIndex];

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && incoming.Priority < iPriorityLimit)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: iPriorityLimit.");
#endif
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
                        if (!bIsWarehouse || m_randomizer.Int32(2U) != 0)
                        {
                            continue;
                        }
                    }

                    ApplyMatch(offerIndex, MatchIncomingOffer(MatchOfferAlgorithm.Distance, offerIndex, bCloseByOnly));
                }
            }
        }

        private void MatchOutgoingOffers(int iPriorityLimit, bool bCloseByOnly)
        {
            // Now match OUTGOING offers by descending priority
            if (job != null)
            {
                for (int offerIndex = 0; offerIndex < job.m_outgoingCount; offerIndex++)
                {
                    // Any matches remaining
                    if (job.m_incomingCountRemaining <= 0)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: No incoming counts remaining.");
#endif
                        break;
                    } 

                    if (job.m_outgoingCountRemaining <= 0)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: No outgoing counts remaining.");
#endif
                        break;
                    }

                    // Any amount remaining
                    if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0)
                    {
#if DEBUG
                        DebugLog.LogOnly((DebugLog.LogReason)job.material, "Break: No amounts remaining.");
#endif
                        break;
                    }

                    // Stop matching below priority limit if set
                    if (iPriorityLimit > 0 && job.m_outgoingOffers[offerIndex].Priority < iPriorityLimit)
                    {
                        break;
                    }

                    ApplyMatch(MatchOutgoingOffer(MatchOfferAlgorithm.Distance, offerIndex, bCloseByOnly), offerIndex);
                }
            }
        }

        private void MatchOffersBalanced()
        {
            // PRIORITY mode - match incoming/outgoing by descending priority choosing the closest match by distance
            // -------------------------------------------------------------------------------------------
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode BALANCED### IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount}");
            MatchOffersBalancedImpl(MatchOfferAlgorithm.Distance);
        }

        private void MatchOffersPriority()
        {
            // PRIORITY mode - match incoming/outgoing by descending priority only. Ignore distance
            // -------------------------------------------------------------------------------------------
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode PRIORITY### IN:{job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT:{job.m_outgoingCountRemaining}/{job.m_outgoingAmount}");
            MatchOffersBalancedImpl(MatchOfferAlgorithm.Priority);
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersBalancedImpl(MatchOfferAlgorithm eAlgoritm)
        {
            // BALANCED internal mode - match incoming/outgoing one by one descending priority
            // -------------------------------------------------------------------------------------------
            BalancedMatchModeOption matchMode = SaveGameSettings.s_SaveGameSettings.BalancedMatchMode;

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
                    if (current_prio == 0)
                    {
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
                        ApplyMatch(indexIn, MatchIncomingOffer(eAlgoritm, indexIn, bCloseByOnly: false));
                        indexIn++;
                    }
                    else
                    {
                        ApplyMatch(MatchOutgoingOffer(eAlgoritm, indexOut, bCloseByOnly: false), indexOut);
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

                    ApplyMatch(indexIn, MatchIncomingOffer(eAlgoritm, indexIn, bCloseByOnly: false));
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

                    ApplyMatch(MatchOutgoingOffer(eAlgoritm, indexOut, bCloseByOnly: false), indexOut);
                    indexOut++;
                }
                else
                {
                    break;
                }
            }
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchIncomingOffer(MatchOfferAlgorithm eAlgoritm, int offerIndex, bool bCloseByOnly)
        {
            switch (eAlgoritm)
            {
                case MatchOfferAlgorithm.Distance:
                    {
                        if (m_bPathDistanceSupported)
                        {
                            return MatchOfferPathDistance(true, ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                        }
                        else
                        {
                            return MatchOfferLOS(true, ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount, bCloseByOnly);
                        }
                    }

                case MatchOfferAlgorithm.Priority:
                    {
                        return MatchOfferPriority(true, ref job.m_incomingOffers[offerIndex], job.m_outgoingOffers, job.m_outgoingCount);
                    }
            }

            return -1;
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOutgoingOffer(MatchOfferAlgorithm eAlgoritm, int offerIndex, bool bCloseByOnly)
        {
            switch (eAlgoritm)
            {
                case MatchOfferAlgorithm.Distance:
                    {
                        if (m_bPathDistanceSupported)
                        {
                            return MatchOfferPathDistance(false, ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                        }
                        else
                        {
                            return MatchOfferLOS(false, ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount, bCloseByOnly);
                        }
                    }         

                case MatchOfferAlgorithm.Priority:
                    return MatchOfferPriority(false, ref job.m_outgoingOffers[offerIndex], job.m_incomingOffers, job.m_incomingCount);
            }

            return -1;
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferPathDistance(bool bIncoming, ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"\r\n{Thread.CurrentThread.ManagedThreadId}   ###Matching " + (bIncoming ? "INCOMING" : "OUTGOING") + $" Offer: {TransferManagerUtils.DebugOffer(offer)} CandidateCount:{iCandidateCount}");
#endif
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

            // Check for Path Distance support
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

            // Fall back on LOS if path distance not supported
            if (!bIsPathDistanceSupported)
            {
                return MatchOfferLOS(bIncoming, ref offer, offerCandidates, iCandidateCount, bCloseByOnly);
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
                if (bIncoming)
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
                        RoadAccessData.AddInstance(candidateOffer.m_object);
                        reason = ExclusionReason.NoStartNode;
                    }
                }
#if DEBUG
                double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - candidateOffer.Position));
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{Thread.CurrentThread.ManagedThreadId}       -> Matching " + (bIncoming ? "OUTGOING" : "INCOMING") + $" Offer: #{counterpart_index} {TransferManagerUtils.DebugOffer(candidateOffer)} Node:{candidateOffer.m_nearestNode} Distance:{dDistance} Exclusion:{reason}");
#endif   
            }

            // Now select closest candidate based on path distance
            int iBestCandidate = -1;
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
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"Match failed: {job.material} {TransferManagerUtils.DebugOffer(offer)} Candidates: {sCandidates}");
#endif
                    // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                    RemoveFailedMatch(bIncoming, ref offer);

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
                else
                {
#if DEBUG
                    double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - offerCandidates[iBestCandidate].Position));
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{job.material} Best#:{iBestCandidate} Object: {offerCandidates[iBestCandidate].m_object.Type} Index: {offerCandidates[iBestCandidate].m_object.Index} Distance: {dDistance}");
#endif
                }
            }

            return iBestCandidate;
        }

        /// <returns>match index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferLOS(bool bIncoming, ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount, bool bCloseByOnly)
        {
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"\r\n{Thread.CurrentThread.ManagedThreadId}   ###MatchingLOS " + (bIncoming ? "INCOMING" : "OUTGOING") + $" Offer: {TransferManagerUtils.DebugOffer(offer)} CandidateCount:{iCandidateCount}");
#endif
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
                if (bIncoming)
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref offer, ref candidateOffer, bCloseByOnly);
                }
                else
                {
                    reason = m_transferRestrictions.CanTransfer(job.material, ref candidateOffer, ref offer, bCloseByOnly);
                }
                if (reason == ExclusionReason.None)
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
                }
#if DEBUG
                double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - candidateOffer.Position));
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{Thread.CurrentThread.ManagedThreadId}       -> Matching " + (bIncoming ? "OUTGOING" : "INCOMING") + $" Offer: #{counterpart_index} {TransferManagerUtils.DebugOffer(candidateOffer)}, bestmatch:{bestmatch_position} Exclusion: {reason}");
#endif    
            }

            // LOS distance
            if (bestmatch_position == -1)
            {
                // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
                RemoveFailedMatch(bIncoming, ref offer);
            }
#if DEBUG
            if (bestmatch_position != -1)
            {
                double dDistance = Math.Sqrt(Vector3.SqrMagnitude(offer.Position - offerCandidates[bestmatch_position].Position));
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{job.material} Best#:{bestmatch_position} Object: {offerCandidates[bestmatch_position].m_object.Type} Index: {offerCandidates[bestmatch_position].m_object.Index} Distance: {dDistance}");
            }
#endif
            return bestmatch_position;
        }

        /// <returns>match index</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOfferPriority(bool bIncoming, ref CustomTransferOffer offer, CustomTransferOffer[] offerCandidates, int iCandidateCount)
        {
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"\r\n{Thread.CurrentThread.ManagedThreadId}   ###MatchingPriority " + (bIncoming ? "INCOMING" : "OUTGOING") + $" Offer: {TransferManagerUtils.DebugOffer(offer)} CandidateCount:{iCandidateCount}");
#endif
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

            // loop through all matching counterpart offers, the first valid one we find will be the highest priority
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
                if (bIncoming)
                {
                    exclude = m_transferRestrictions.CanTransfer(job.material, ref offer, ref candidateOffer, false);
                }
                else
                {
                    exclude = m_transferRestrictions.CanTransfer(job.material, ref candidateOffer, ref offer, false);
                }

#if DEBUG
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"{Thread.CurrentThread.ManagedThreadId}       -> Matching " + (bIncoming ? "OUTGOING" : "INCOMING") + $" Offer:#{counterpart_index} {TransferManagerUtils.DebugOffer(candidateOffer)} Exclusion:{exclude}");
#endif  
                // We select the first candidate that passes the restrictions
                if (exclude == ExclusionReason.None)
                {
                    return counterpart_index;
                }
            }

            // Match attempt failed, remove this offer from match amounts and set its amount to 0 so we dont try to match against it again
            RemoveFailedMatch(bIncoming, ref offer);

            return -1;
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
            if (m_bPathDistanceSupported && m_pathDistance != null && candidates.Count > 0)
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
                    uint uiChosenNode = m_pathDistance.FindNearestNeighbor(offer.Active, uiStartNode, candidateNodes);

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
                case TransferReason.OutgoingMail:
                case TransferReason.IncomingMail:

                // Need to scale the *Move functions by priority as well
                // so that we match Cemeteries with Crematoriums and Landfill with Recycling plants
                case TransferReason.CriminalMove: 
                case TransferReason.DeadMove:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                    {
                        // Scale by priority. Higher priorities will appear closer
                        return 1.0f / (float)Math.Pow(2, offer.Priority);
                    }
                    
                default:
                    {
                        // No priority scaling
                        return 1.0f; 
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