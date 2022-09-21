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

        private static Stopwatch s_watch = new Stopwatch();

        // References to game functionalities:
        public static TransferManager s_TransferManager = null;
        public static BuildingManager s_BuildingManager = null;
        public static VehicleManager s_VehicleManager = null;
        public static InstanceManager s_InstanceManager = null;
        public static DistrictManager s_DistrictManager = null;
        public static CitizenManager s_CitizenManager = null;
        public static bool s_bInitManagers = false;

        // Current transfer job from workqueue
        public TransferJob? job = null;

        public CustomTransferManager()
        {
            if (!s_bInitManagers)
            {
                // get references to other managers:
                s_TransferManager = Singleton<TransferManager>.instance;
                s_BuildingManager = Singleton<BuildingManager>.instance;
                s_InstanceManager = Singleton<InstanceManager>.instance;
                s_VehicleManager = Singleton<VehicleManager>.instance;
                s_DistrictManager = Singleton<DistrictManager>.instance;
                s_CitizenManager = Singleton<CitizenManager>.instance;
                s_bInitManagers = true;
            }
        }

#if DEBUG
        private void DebugPrintAllOffers(TransferReason material, int offerCountIncoming, int offerCountOutgoing)
        {
            for (int i = 0; i < offerCountIncoming; i++)
            {
                ref CustomTransferOffer incomingOffer = ref job.m_incomingOffers[i];
                String bname = TransferManagerUtils.DebugOffer(incomingOffer);
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   in #{i}: prio: {incomingOffer.Priority}, act {incomingOffer.Active}, excl:{incomingOffer.Exclude}, amt:{incomingOffer.Amount}, LocalPark:{incomingOffer.m_offer.m_isLocalPark} Type:{incomingOffer.m_object.Type} Index:{incomingOffer.m_object.Index} name={bname}");
            }

            for (int i = 0; i < offerCountOutgoing; i++)
            {
                ref CustomTransferOffer outgoingOffer = ref job.m_outgoingOffers[i];
                String bname = TransferManagerUtils.DebugOffer(outgoingOffer);
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   out #{i}: prio: {outgoingOffer.Priority}, act {outgoingOffer.Active}, excl {outgoingOffer.Exclude}, amt {outgoingOffer.Amount}, LocalPark:{outgoingOffer.m_offer.m_isLocalPark} Type:{outgoingOffer.m_object.Type} Index:{outgoingOffer.m_object.Index} name={bname}");
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
                            MatchOffersBalanced();
                            break;
                        }
                }
            }
            
            // Record longest match time for stats.
            long matchTime = s_watch.ElapsedMilliseconds - startTime;
            if (matchTime > TransferManagerStats.s_longestMatch)
            {
                TransferManagerStats.s_longestMatch = matchTime;
                TransferManagerStats.s_longestMaterial = job.material;

#if (DEBUG)
                Debug.Log("Thread: " + Thread.CurrentThread.ManagedThreadId + " Material: " + material + " IN: " + job.m_incomingCount + " OUT: " + job.m_outgoingCount + " Elapsed time: " + matchTime + "ms");
#endif  
            }

            TransferManagerStats.s_TotalMatchJobs++;
            TransferManagerStats.s_TotalMatchTimeMS += matchTime;
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
            
            int iMatchPriorityLimit = -1; // -1 is not set
            float fPatrollingVehicleDistanceLimit = 0.0f; // Not set

            switch (job.material)
            {
                // Do not include Sick/Sick2 as the hospitals put out priority 7 offers and produce worse matches.
                case TransferReason.Dead:
                case TransferReason.Garbage:
                case TransferReason.Crime:
                    // Stop matching at priority 2 as 1/1 matches are usually not very good whereas
                    // priority 2 can still be matched with 0.
                    iMatchPriorityLimit = 2;

                    // Limit patrolling vehicle match distance to ensure they only respond to "nearby" issues.
                    fPatrollingVehicleDistanceLimit = 4000000.0f; // 2.0km
                    break;
            }

            // Match service vehicles first as they put out high priority offers (7) so
            // we get them to pick up lots of close by bodies/garbage/crime while they are out and about.
            switch (job.material)
            {
                // Do not include Sick/Sick2 as the hospitals put out priority 7 offers and produce worse matches.
                case TransferReason.Dead:
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Mail:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                    {
                        for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
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

                            int inPriority = job.m_incomingOffers[offerIndex].Priority;
                            if (inPriority == 7)
                            {
                                int prio_lower_limit = 0;
                                ApplyMatch(offerIndex, MatchIncomingOffer(prio_lower_limit, offerIndex, fPatrollingVehicleDistanceLimit));
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    }
            }

            // 1st loop: all OUTGOING offers by descending priority
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
                int iPriority = job.m_outgoingOffers[offerIndex].Priority;
                if (iMatchPriorityLimit >= 0 && iPriority < iMatchPriorityLimit)
                {
                    break;
                }

                int prio_lower_limit = Math.Max(0, 2 - iPriority);
                ApplyMatch(MatchOutgoingOffer(prio_lower_limit, offerIndex), offerIndex);
            }

            // Now match any incoming remaining
            for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
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

                int prio_lower_limit = Math.Max(0, 2 - job.m_incomingOffers[offerIndex].Priority);
                ApplyMatch(offerIndex, MatchIncomingOffer(prio_lower_limit, offerIndex, 0.0f));
            }
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
            switch (job.material)
            {
                case TransferReason.RoadMaintenance:
                    {
                        // Match service vehicles first as they put out high priority offers (7) so
                        // we get them to pick up lots of close by bodies/garbage/crime while they are out and about.
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

                            int outPriority = job.m_outgoingOffers[offerIndex].Priority;
                            if (outPriority == 7)
                            {
                                int prio_lower_limit = 0;
                                ApplyMatch(MatchOutgoingOffer(prio_lower_limit, offerIndex), offerIndex);
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    }
            }

            // 1st loop: Match any incoming remaining
            for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
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

                int prio_lower_limit = Math.Max(0, 2 - job.m_incomingOffers[offerIndex].Priority);
                ApplyMatch(offerIndex, MatchIncomingOffer(prio_lower_limit, offerIndex, 0.0f));
            }

            // Now match OUTGOING offers by descending priority
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

                int prio_lower_limit = Math.Max(0, 2 - job.m_outgoingOffers[offerIndex].Priority);
                ApplyMatch(MatchOutgoingOffer(prio_lower_limit, offerIndex), offerIndex);
            }
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void MatchOffersBalanced()
        {
            // BALANCED mode - match incoming/outgoing one by one by distance, descending priority
            // -------------------------------------------------------------------------------------------

            BalancedMatchModeOption matchMode = SaveGameSettings.s_SaveGameSettings.BalancedMatchMode;
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode BALANCED###");
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
                    if (current_prio == 0)
                    {
                        break;
                    }
                    int prio_lower_limit = Math.Max(0, 2 - current_prio);

                    if (incoming.Priority == outgoing.Priority)
                    {
                        switch (matchMode)
                        {
                            case BalancedMatchModeOption.MatchModeIncomingFirst: // vanilla
                                {
                                    // Match incoming first (Vanilla mode)
                                    ApplyMatch(indexIn, MatchIncomingOffer(prio_lower_limit, indexIn, 0.0f));
                                    indexIn++;
                                    break;
                                }
                            case BalancedMatchModeOption.MatchModeLeastFirst:
                                {
                                    // Match whichever has less offers available so that we maximise the matches for the side with
                                    // limited resources.
                                    if (job.m_incomingCountRemaining <= job.m_outgoingCountRemaining)
                                    {
                                        ApplyMatch(indexIn, MatchIncomingOffer(prio_lower_limit, indexIn, 0.0f));
                                        indexIn++;
                                    }
                                    else
                                    {
                                        ApplyMatch(MatchOutgoingOffer(prio_lower_limit, indexOut), indexOut);
                                        indexOut++;
                                    }
                                    break;
                                }
                            case BalancedMatchModeOption.MatchModePassiveFirst:
                                {
                                    // Match against passive side first
                                    if (!incoming.Active)
                                    {
                                        ApplyMatch(indexIn, MatchIncomingOffer(prio_lower_limit, indexIn, 0.0f));
                                        indexIn++;
                                    }
                                    else
                                    {
                                        ApplyMatch(MatchOutgoingOffer(prio_lower_limit, indexOut), indexOut);
                                        indexOut++;
                                    }
                                    break;
                                }
                        }
                        
                    }
                    else if (incoming.Priority > outgoing.Priority)
                    {
                        ApplyMatch(indexIn, MatchIncomingOffer(prio_lower_limit, indexIn, 0.0f));
                        indexIn++;
                    }
                    else
                    {
                        ApplyMatch(MatchOutgoingOffer(prio_lower_limit, indexOut), indexOut);
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
                    int prio_lower_limit = Math.Max(0, 2 - incoming.Priority);

                    ApplyMatch(indexIn, MatchIncomingOffer(prio_lower_limit, indexIn, 0.0f));
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
                    int prio_lower_limit = Math.Max(0, 2 - outgoing.Priority);

                    ApplyMatch(MatchOutgoingOffer(prio_lower_limit, indexOut), indexOut);
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
                    ExclusionReason reason = CanTransferFastChecksOnly(job.material, ref incoming, ref outgoing, 2);
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
        private int MatchIncomingOffer(int prio_lower_limit, int offerIndex, float fPatrollingVehicleDistanceLimit)
        {
            // Get incoming offer reference:
            ref CustomTransferOffer incomingOffer = ref job.m_incomingOffers[offerIndex];

            // guard: offer valid?
            if (incomingOffer.Amount <= 0)
            {
                return -1;
            }

            if (!incomingOffer.IsValid())
            {
                return - 1;
            }
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###Matching INCOMING offer: {TransferManagerUtils.DebugOffer(incomingOffer)}, priority: {incomingOffer.Priority}, remaining amount outgoing: {job.m_outgoingAmount}");
#endif
            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_index = 0; counterpart_index < job.m_outgoingCount; counterpart_index++)
            {
                ref CustomTransferOffer outgoingOffer = ref job.m_outgoingOffers[counterpart_index];

                //guard: below lower prio limit? ->end matching
                if (outgoingOffer.Priority < prio_lower_limit)
                {
                    break;
                }

                // guards: Any amount left to match?
                if (outgoingOffer.Amount <= 0)
                {
                    continue;
                }

                if (!outgoingOffer.IsValid())
                {
                    continue;
                }

                ExclusionReason reason = CanTransfer(job.material, ref incomingOffer, ref outgoingOffer, prio_lower_limit, fPatrollingVehicleDistanceLimit);
                if (reason == ExclusionReason.None)
                {
                    // CHECK OPTION: WarehouseFirst && ImportExportPreferTrainShipPlane
                    float distanceWarehouseFactor = WarehouseFirst(outgoingOffer, job.material, WAREHOUSE_OFFERTYPE.OUTGOING);
                    float distanceOutsideFactor = OutsideModifier(job.material, incomingOffer, outgoingOffer);
                    float fPriorityFactor = PriorityModifier(outgoingOffer, job.material);

                    // EVAL final distance
                    float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceWarehouseFactor * distanceOutsideFactor * fPriorityFactor;
                    if (squaredDistance < bestmatch_distance)
                    {
                        bestmatch_position = counterpart_index;
                        bestmatch_distance = squaredDistance;
                    }
#if DEBUG
                    double dDistance = Math.Sqrt(Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position));
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Matching outgoing offer: {TransferManagerUtils.DebugOffer(outgoingOffer)}, priority: {outgoingOffer.Priority}, amt {outgoingOffer.Amount}, distance: {dDistance} squaredDistance:{squaredDistance}@WF:{distanceWarehouseFactor}/OF:{distanceOutsideFactor}/PF:{fPriorityFactor}, bestmatch: {bestmatch_distance}");
#endif
                }
                else
                {
#if DEBUG
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Matching outgoing offer: {TransferManagerUtils.DebugOffer(outgoingOffer)}, amt {outgoingOffer.Amount}, Exclusion: {reason}");
#endif
                }
            }

            return bestmatch_position;
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MatchOutgoingOffer(int prio_lower_limit, int offerIndex)
        {
            // Get Outgoing offer reference:
            ref CustomTransferOffer outgoingOffer = ref job.m_outgoingOffers[offerIndex];

            // guard: offer valid?
            if (outgoingOffer.Amount <= 0)
            {
                return -1;
            }

            if (!outgoingOffer.IsValid())
            {
                return -1;
            }
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###Matching OUTGOING offer: {TransferManagerUtils.DebugOffer(outgoingOffer)}, priority: {outgoingOffer.Priority}, remaining amount incoming: {job.m_incomingAmount}");
#endif
            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_index = 0; counterpart_index < job.m_incomingCount; counterpart_index++)
            {
                ref CustomTransferOffer incomingOffer = ref job.m_incomingOffers[counterpart_index];

                //guard: below lower prio limit? ->end matching
                if (incomingOffer.Priority < prio_lower_limit) 
                {
                    break;
                }

                // guards: out=in same? exclude offer (already used?)
                if (incomingOffer.Amount <= 0) 
                {
                    continue;
                }

                if (!incomingOffer.IsValid())
                {
                    continue;
                }

                ExclusionReason reason = CanTransfer(job.material, ref incomingOffer, ref outgoingOffer, prio_lower_limit, 0.0f);
                if (reason == ExclusionReason.None)
                {
                    // Warehouse first and Prefer Plane/Train/Ship will reduce the effective distance making it more likely a warehouse is chosen
                    float distanceWarehouseFactor = WarehouseFirst(incomingOffer, job.material, WAREHOUSE_OFFERTYPE.INCOMING);
                    float distanceOutsideFactor = OutsideModifier(job.material, incomingOffer, outgoingOffer);
                    float fPriorityFactor = PriorityModifier(incomingOffer, job.material); // For some materials types only. Similar to the vanilla matching higher priorities appear closer.

                    float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceWarehouseFactor * distanceOutsideFactor * fPriorityFactor;
                    if (squaredDistance < bestmatch_distance)
                    {
                        bestmatch_position = counterpart_index;
                        bestmatch_distance = squaredDistance;
                    }
#if DEBUG
                    double dDistance = Math.Sqrt(Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position));
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Matching incoming offer: {TransferManagerUtils.DebugOffer(incomingOffer)}, amt {incomingOffer.Amount}, distance: {dDistance} squaredDistance: {squaredDistance}@WF:{distanceWarehouseFactor}/OF:{distanceOutsideFactor}/PF:{fPriorityFactor}, bestmatch: {bestmatch_distance}");
#endif
                }
                else
                {
#if DEBUG
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Matching incoming offer: {TransferManagerUtils.DebugOffer(incomingOffer)}, amt {incomingOffer.Amount}, Exclusion: {reason}");
#endif
                }
            }

            return bestmatch_position;
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
                Building.Flags flags = s_BuildingManager.m_buildings.m_buffer[offer.GetBuilding()].m_flags;
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
                switch (instance.Type)
                {
                    case InstanceType.Building:
                        {
                            Building building = s_BuildingManager.m_buildings.m_buffer[instance.Building];
                            return building.m_flags != 0;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = s_VehicleManager.m_vehicles.m_buffer[instance.Vehicle];
                            return vehicle.m_flags != 0;
                        }
                    case InstanceType.Citizen:
                        {
                            Citizen citizen = s_CitizenManager.m_citizens.m_buffer[instance.Citizen];
                            return citizen.m_flags != 0;
                        }
                }
            }

            return true;
        }

        public static ushort GetOfferBuilding(CustomTransferOffer offer)
        {
            switch (offer.m_object.Type)
            {
                case InstanceType.Building:
                    {
                        return offer.Building;
                    }
                case InstanceType.Vehicle:
                    {
                        Vehicle vehicle;
                        if (s_VehicleManager == null)
                        {
                            vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[offer.Vehicle];
                        }
                        else
                        {
                            vehicle = s_VehicleManager.m_vehicles.m_buffer[offer.Vehicle];
                        }
                        return vehicle.m_sourceBuilding;
                    }
                case InstanceType.Citizen:
                    {
                        Citizen citizen;
                        if (s_CitizenManager == null)
                        {
                            citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[offer.Citizen];
                        }
                        else
                        {
                            citizen = s_CitizenManager.m_citizens.m_buffer[offer.Citizen];
                        }
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
    }
}