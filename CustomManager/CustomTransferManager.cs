using ColossalFramework;
using TransferManagerCE.Util;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using TransferManagerCE.Settings;
using System.Collections.Generic;

namespace TransferManagerCE.CustomManager
{
    public sealed class CustomTransferManager : TransferManager
    {
        private static bool _init = false;
        public static volatile bool _runThread = true;

        // Matching logic
        public enum OFFER_MATCHMODE : int { INCOMING_FIRST = 1, OUTGOING_FIRST = 2, BALANCED = 3 };

        public enum DistanceMode
        {
            Distance,
            PriorityDistance,
        }
        private enum WAREHOUSE_OFFERTYPE : int {  INCOMING = 1, OUTGOING = 2 }; 

        // References to game functionalities:
        public static TransferManager _TransferManager = null;
        public static BuildingManager _BuildingManager = null;
        public static VehicleManager _VehicleManager = null;
        public static InstanceManager _InstanceManager = null;
        public static DistrictManager _DistrictManager = null;
        public static CitizenManager _CitizenManager = null;

        // Current transfer job from workqueue
        private static TransferJob job = null;

        public static void InitDelegate()
        {
            TransferManagerStartTransferDG = FastDelegateFactory.Create<TransferManagerStartTransfer>(typeof(TransferManager), "StartTransfer", instanceMethod: true);
            CalculateOwnVehiclesDG = FastDelegateFactory.Create<CommonBuildingAICalculateOwnVehicles>(typeof(CommonBuildingAI), "CalculateOwnVehicles", instanceMethod: true);
        }

        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);
        public static TransferManagerStartTransfer TransferManagerStartTransferDG;

        public delegate void CommonBuildingAICalculateOwnVehicles(CommonBuildingAI CommonBuildingAI, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        public static CommonBuildingAICalculateOwnVehicles CalculateOwnVehiclesDG;

        private static void Init()
        {
            // get references to other managers:
            CustomTransferManager._TransferManager = Singleton<TransferManager>.instance;
            CustomTransferManager._BuildingManager = Singleton<BuildingManager>.instance;
            CustomTransferManager._InstanceManager = Singleton<InstanceManager>.instance;
            CustomTransferManager._VehicleManager  = Singleton<VehicleManager>.instance;
            CustomTransferManager._DistrictManager = Singleton<DistrictManager>.instance;
            CustomTransferManager._CitizenManager  = Singleton<CitizenManager>.instance;

            InitDelegate();
            _init = true;
        }

        private static void CheckInit()
        {
            if (_init)
            {
                DebugLog.LogInfo("Checking initializations...");
                DebugLog.LogInfo($"- _TransferManager instance: {_TransferManager}");
                DebugLog.LogInfo($"- _InstanceManager instance: {_InstanceManager}");
                DebugLog.LogInfo($"- _BuildingManager instance: {_BuildingManager}");
                DebugLog.LogInfo($"- _VehicleManager instance: {_VehicleManager}");
                DebugLog.LogInfo($"- _CitizenManager instance: {_CitizenManager}");
                DebugLog.LogInfo($"- _DistrictManager instance: {_DistrictManager}");

                DebugLog.LogInfo("Checking delegates...");
                DebugLog.LogInfo($"- TransferManagerStartTransferDG instance: {TransferManagerStartTransferDG}");
                DebugLog.LogInfo($"- CalculateOwnVehicles instance: {CalculateOwnVehiclesDG}");
                
                if ((_TransferManager != null) && (_InstanceManager != null) && (_BuildingManager != null) && (_VehicleManager != null) && (_CitizenManager != null) &&
                    (_DistrictManager != null) && (TransferManagerStartTransferDG != null) && (CalculateOwnVehiclesDG != null))
                {
                    Debug.Log("ALL INIT CHECKS PASSED. This should work.");
                }
                else
                {
                    Debug.LogError("PROBLEM DETECTED! SOME MODS ARE CAUSING INCOMPATIBILITIES! Generating mod list and harmony report...");
                    DebugLog.ReportAllHarmonyPatches();
                    DebugLog.ReportAllMods();
                    DebugLog.FlushImmediate();
                    Debug.LogError("PROBLEM DETECTED! SOME MODS ARE CAUSING INCOMPATIBILITIES! Please check log >MoreEffectiveTransfer.log< in CSL directory!");
                }
            }
        }

        // Determine current local district setting by combining building and global settings
        private static BuildingSettings.PreferLocal GetPreferLocal(ushort buildingId, bool bIncoming, TransferReason material)
        {
            BuildingSettings.PreferLocal ePreferLocalDistrict = BuildingSettings.PreferLocal.ALLOW_ANY_DISTRICT;
            
            if (buildingId != 0 && TransferManagerModes.IsBuildingPreferLocalSupported(material))
            {
                if (bIncoming)
                {
                    ePreferLocalDistrict = BuildingSettings.PreferLocalDistrictServicesIncoming(buildingId);
                }
                else
                {
                    ePreferLocalDistrict = BuildingSettings.PreferLocalDistrictServicesOutgoing(buildingId);
                }
            }
            
            // Global setting is only applied to certain services as it is too powerful otherwise.
            if (ModSettings.GetSettings().optionPreferLocalService && TransferManagerModes.IsGlobalPreferLocalSupported(material))
            {
                ePreferLocalDistrict = (BuildingSettings.PreferLocal)Math.Max((int)ePreferLocalDistrict, (int)BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT);
            }

            return ePreferLocalDistrict;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool IsLocalDistrict(ref TransferOffer offerIn, ushort inBuildingId, ref TransferOffer offerOut, ushort outBuildingId, TransferReason material, out float distanceModifier)
        {
            const int PRIORITY_THRESHOLD_LOCAL = 3;     //upper prios also get non-local fulfillment
            const float LOCAL_DISTRICT_MODIFIER = 0.1f;   //modifier for distance within same district
            bool isLocal = false;
            distanceModifier = 1.0f;

            // Check if it is an Import/Export
            if (BuildingTypeHelper.IsOutsideConnection(inBuildingId) ||
                BuildingTypeHelper.IsOutsideConnection(outBuildingId))
            {
                // Prefer local district is still allowed to Import/Export
                return true;
            }

            // Find the maximum setting from both buildings
            BuildingSettings.PreferLocal eInBuildingLocalDistrict = GetPreferLocal(inBuildingId, true, material);
            BuildingSettings.PreferLocal eOutBuildingLocalDistrict = GetPreferLocal(outBuildingId, false, material);
            BuildingSettings.PreferLocal ePreferLocalDistrict = (BuildingSettings.PreferLocal) Math.Max((int) eInBuildingLocalDistrict, (int) eOutBuildingLocalDistrict);

            if (ePreferLocalDistrict == BuildingSettings.PreferLocal.ALLOW_ANY_DISTRICT)
            {
                return true; // Any match is fine
            }
            else if (ePreferLocalDistrict == BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT)
            {
                // priority of passive side above threshold -> any service is OK!
                int priority = offerIn.Active ? offerOut.Priority : offerIn.Priority;
                if (priority >= PRIORITY_THRESHOLD_LOCAL)
                {
                    isLocal = true;
                    // continue logic to set distanceModifier for service within same district
                }
            }

            // get respective districts
            byte districtIncoming = 0;
            byte parkIncoming = 0;
            if (inBuildingId != 0)
            {
                Building inBuilding = _BuildingManager.m_buildings.m_buffer[inBuildingId];
                districtIncoming = _DistrictManager.GetDistrict(inBuilding.m_position);
                parkIncoming = DistrictManager.instance.GetPark(inBuilding.m_position);
            }

            byte districtOutgoing = 0;
            byte parkOutgoing = 0;
            if (outBuildingId != 0)
            {
                Building outBuilding = _BuildingManager.m_buildings.m_buffer[outBuildingId];
                districtOutgoing = _DistrictManager.GetDistrict(outBuilding.m_position);
                parkOutgoing = DistrictManager.instance.GetPark(outBuilding.m_position);
            }

            bool bIncomingInDistrict = districtIncoming != 0 || parkIncoming != 0;
            bool bOutgoingInDistrict = districtOutgoing != 0 || parkOutgoing != 0;

            

            // Check we actually have a district
            if (bIncomingInDistrict || bOutgoingInDistrict)
            {
                // return true if: both within same district, or active offer is outside connection district ("in global area")
                bool bSameDistrict = districtIncoming == districtOutgoing && districtOutgoing != 0;
                bool bSamePark = parkIncoming == parkOutgoing && parkOutgoing != 0;
                if (bSameDistrict || bSamePark)
                {
                    // Both in same district/park. Allow it and apply district modifier to make it more attractive
                    distanceModifier = LOCAL_DISTRICT_MODIFIER;
                    isLocal = true;
                }
                else if (ModSettings.GetSettings().optionPreferLocalService && ePreferLocalDistrict == BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT)
                {
                    // If setting is prefer local district and it's from the global settings then also allow matching Active offers, where it's not our vehicle
                    if (offerIn.Active && (districtIncoming == 0 && parkIncoming == 0))
                    {
                        // Active side is outside any district so allow it, but don't apply modifier
                        isLocal = true;
                    }
                    else if (offerOut.Active && (districtOutgoing == 0 && parkOutgoing == 0))
                    {
                        // Active side is outside any district so allow it, but don't apply modifier
                        isLocal = true;
                    }
                }
                
            } 
            else
            {
                // neither in district so just allow transfer but don't apply modifier
                isLocal = true;
            }

            return isLocal;
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float OutsideModifier(ref TransferOffer offer, TransferReason material)
        {
            const float OUTSIDE_MODIFIER = 0.25f;

            if (!ModSettings.GetSettings().optionPreferExportShipPlaneTrain)
                return 1f;

            if ((offer.Building != 0) && (_BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_buildingAI is OutsideConnectionAI))
            {
#if (DEBUG)
                DebugLog.LogOnly((DebugLog.LogReason)material, $"       ** Outside Offer: material: {material}, {TransferManagerUtils.DebugOffer(offer)}, SubService class is: {_BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_class.m_subService}");
#endif
                ItemClass.SubService? subService = _BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_class.m_subService;
                if ((subService == ItemClass.SubService.PublicTransportTrain) ||
                    (subService == ItemClass.SubService.PublicTransportShip) ||
                    (subService == ItemClass.SubService.PublicTransportPlane))
                    return OUTSIDE_MODIFIER;
            }

            return 1f;
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float WarehouseFirst(ref TransferOffer offer, TransferReason material, WAREHOUSE_OFFERTYPE whInOut)
        {
            const float WAREHOUSE_MODIFIER = 0.1f;   //modifier for distance for warehouse

            if (!ModSettings.GetSettings().optionWarehouseFirst)
            {
                return 1f;
            }

            if (offer.Exclude)  //TransferOffer.Exclude is only ever set by WarehouseAI!
            {
                Building.Flags flags = _BuildingManager.m_buildings.m_buffer[offer.Building].m_flags;
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

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool WarehouseCanTransfer(ref TransferOffer incomingOffer, ref TransferOffer outgoingOffer, TransferReason material)
        {
            Building[] aBuildingManagerBuffer = _BuildingManager.m_buildings.m_buffer;

            // Option: optionWarehouseReserveTrucks
            if ((ModSettings.GetSettings().optionWarehouseReserveTrucks || BuildingSettings.IsReserveCargoTrucks(outgoingOffer.Building)) && 
                (outgoingOffer.Exclude && outgoingOffer.Active)) //further checks only relevant if outgoing from warehouse and active
            {
                // is outgoing a warehouse with active delivery, and is counterpart incoming an outside connection?
                if (BuildingTypeHelper.IsOutsideConnection(incomingOffer.Building) &&
                    BuildingTypeHelper.IsWarehouse(outgoingOffer.Building))
                {
                    CommonBuildingAI? buildingAI = aBuildingManagerBuffer[outgoingOffer.Building].Info?.m_buildingAI as CommonBuildingAI;
                    if (buildingAI != null)
                    {
                        int total = CitiesUtils.GetWarehouseTruckCount(outgoingOffer.Building);
                        int count = 0;
                        int cargo = 0;
                        int capacity = 0;
                        int outside = 0;
                        float maxExport = (total * 0.75f);

                        // Call CommonBuildingAI.CalculateOwnVehicles, which is protected so we need reflection
                        CalculateOwnVehiclesDG(buildingAI, outgoingOffer.Building, ref aBuildingManagerBuffer[outgoingOffer.Building], material, ref count, ref cargo, ref capacity, ref outside);
#if DEBUG
                        //DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       ** checking canTransfer: total: {total}, ccco: {count}/{cargo}/{capacity}/{outside} => {((float)(outside + 1f) > maxExport)}");
#endif
                        if ((float)(outside + 1f) > maxExport)
                        {
                            return false;   //dont need further checks, we would be over reserved truck limit
                        }
                    }
                    else
                    {
                        Debug.Log("Not CommonBuildingAI building type.");
                        return true; // Just allow default behaviour for now
                    }
                }
            }

            // all good -> allow transfer
            return true;
        }

        private static bool IsImportExportRestrictionsSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:
                case TransferReason.Goods:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Lumber:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Paper:
                case TransferReason.PlanedTimber:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Glass:
                case TransferReason.Metals:
                case TransferReason.LuxuryProducts:
                case TransferReason.Fish:
                    return true;

                default:
                    return false;
            }
        }

        private static bool OutsideConnectionCanTransfer(ref TransferOffer incomingOffer, ref TransferOffer outgoingOffer, TransferReason material)
        {
            if (IsImportExportRestrictionsSupported(material))
            {
                if (BuildingTypeHelper.IsOutsideConnection(incomingOffer.Building) &&
                    BuildingSettings.IsExportDisabled(outgoingOffer.Building))
                {
                    // Attempting to Export is disabled
                    return false;
                }

                if (BuildingTypeHelper.IsOutsideConnection(outgoingOffer.Building) &&
                    BuildingSettings.IsImportDisabled(incomingOffer.Building))
                {
                    // Attempting to Import is disabled
                    return false;
                }
            }

            return true;
        }

        private static bool ActivePassiveCanTransfer(ref TransferOffer incomingOffer, ref TransferOffer outgoingOffer, TransferReason material)
        {
            // Dont allow Passive/Passive or Active/Active transfers for the following types
            // TODO: Investigate PartnerAdult, PartnerYoung
            switch (material)
            {
                case TransferReason.Sick:
                case TransferReason.ElderCare:
                case TransferReason.ChildCare:
                    {
                        // Allow ACTIVE/PASSIVE or ACTIVE/ACTIVE but not PASSIVE/PASSIVE as neither travels to make the match
                        return (incomingOffer.Active != outgoingOffer.Active) || (incomingOffer.Active && outgoingOffer.Active);
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool PathfindExclusion(ref TransferOffer incomingOffer, ref TransferOffer outgoingOffer)
        {
            bool result = false;

            //check failed building pair
            if ((incomingOffer.Building != 0) && (outgoingOffer.Building != 0))
            {
                if (incomingOffer.Active)
                    result = PathFindFailure.FindBuildingPair(incomingOffer.Building, outgoingOffer.Building);
                else if (outgoingOffer.Active)
                    result = PathFindFailure.FindBuildingPair(outgoingOffer.Building, incomingOffer.Building);

                if (result)
                {
#if DEBUG
                    DebugLog.LogOnly(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded: material:{job.material}, in:{incomingOffer.Building}({TransferManagerUtils.DebugOffer(incomingOffer)}) and out:{outgoingOffer.Building}({TransferManagerUtils.DebugOffer(outgoingOffer)})");
#endif
                    return result;
                }
            }

            //check failed outside connection pair
            if ((incomingOffer.Building != 0) && _BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI)
            {
                if (incomingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(incomingOffer.Building, outgoingOffer.Building);
                else if (outgoingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(outgoingOffer.Building, incomingOffer.Building);
#if DEBUG
                if (result)
                {
                    DebugLog.LogOnly(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded incoming outsideconnection: material:{job.material}, {incomingOffer.Building}({TransferManagerUtils.DebugOffer(incomingOffer)})");
                }
#endif
            }
            else if ((outgoingOffer.Building != 0) && _BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI)
            {
                if (incomingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(incomingOffer.Building, outgoingOffer.Building);
                else if (outgoingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(outgoingOffer.Building, incomingOffer.Building);
#if DEBUG
                if (result)
                {
                    DebugLog.LogOnly(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded ougoing outsideconnection: material:{job.material}, {outgoingOffer.Building}({TransferManagerUtils.DebugOffer(outgoingOffer)})");
                }
#endif
            }

            return result;
        }

#if DEBUG
        private static void DebugPrintAllOffers(TransferReason material, int offerCountIncoming, int offerCountOutgoing)
        {
            for (int i=0; i< offerCountIncoming; i++)
            {
                ref TransferOffer incomingOffer = ref job.m_incomingOffers[i];
                String bname = TransferManagerUtils.DebugOffer(incomingOffer);
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   in #{i}: prio: {incomingOffer.Priority}, act {incomingOffer.Active}, excl {incomingOffer.Exclude}, amt {incomingOffer.Amount}, bvcnt {incomingOffer.Building}/{incomingOffer.Vehicle}/{incomingOffer.Citizen}/{incomingOffer.NetSegment}/{incomingOffer.TransportLine} name={bname}");
            }

            for (int i = 0; i < offerCountOutgoing; i++)
            {
                ref TransferOffer outgoingOffer = ref job.m_outgoingOffers[i];
                String bname = TransferManagerUtils.DebugOffer(outgoingOffer);
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   out #{i}: prio: {outgoingOffer.Priority}, act {outgoingOffer.Active}, excl {outgoingOffer.Exclude}, amt {outgoingOffer.Amount}, bvcnt {outgoingOffer.Building}/{outgoingOffer.Vehicle}/{outgoingOffer.Citizen}/{outgoingOffer.NetSegment}/{outgoingOffer.TransportLine} name={bname}");
            }
        }
#endif

        /// <summary>
        /// Thread loop: dequeue job from workqueue and perform offer matching
        /// </summary>
        public static void MatchOffersThread()
        {
            DebugLog.LogInfo($"MatchOffersThread: Thread started.");

            while (_runThread)
            {
                // Dequeue work job
                job = CustomTransferDispatcher.Instance.DequeueWork();

                if (job != null)
                {
                    // match offers in job
                    MatchOffers(job.material);

                    // return to jobpool
                    TransferJobPool.Instance.Return(job);
                    job = null;
                }
                else
                {
                    // clean pathfind LRU
                    PathFindFailure.RemoveOldEntries();

                    // wait for signal
#if (DEBUG)
                    DebugLog.LogOnly(DebugLog.REASON_ALL, $"MatchOffersThread: waiting for work signal...");
#endif
                    CustomTransferDispatcher._waitHandle.WaitOne();
                }
            }

            DebugLog.LogInfo($"MatchOffersThread: Thread ended.");
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MatchOffers(TransferReason material)
        {
            // delayed initialization until first call
            if (!_init)
            {
                Init();
                CheckInit();
            }

            // guard: ignore transferreason.none
            if (material == TransferReason.None)
            {
                return;
            }

#if (DEBUG)
            // DEBUG LOGGING
            DebugLog.LogOnly((DebugLog.LogReason)material, $"-- TRANSFER REASON: {material}, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount}, count in {job.m_incomingCount}, count out {job.m_outgoingCount}");
            DebugPrintAllOffers(material, job.m_incomingCount, job.m_outgoingCount);
#endif
            // Select offer matching algorithm
            OFFER_MATCHMODE match_mode = TransferManagerModes.GetMatchOffersMode(material);
            switch (match_mode)
            {
                case OFFER_MATCHMODE.OUTGOING_FIRST: MatchOffersOutgoingFirst(); break;
                case OFFER_MATCHMODE.INCOMING_FIRST: MatchOffersIncomingFirst(); break;
                case OFFER_MATCHMODE.BALANCED: MatchOffersBalanced(); break;
            }
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MatchOffersOutgoingFirst()
        {
            // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
            // -------------------------------------------------------------------------------------------
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode OUTGOING FIRST###");
#endif
            // 1st loop: all OUTGOING offers by descending priority
            for (int offerIndex = 0; offerIndex < job.m_outgoingCount; offerIndex++)
            {
                int prio_lower_limit = Math.Max(0, 2 - job.m_outgoingOffers[offerIndex].Priority);
                if (job.m_incomingAmount <= 0)
                {
#if (DEBUG)
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ### MATCHMODE EXIT, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount} ###");
#endif
                    break;
                }

                MatchOutgoingOffer(prio_lower_limit, offerIndex);
            } //end OFFER_MATCHMODE.OUTGOING_FIRST
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MatchOffersIncomingFirst()
        {
            // INCOMING FIRST mode - try to fulfill all incoming offers by finding outgoings by distance
            // -------------------------------------------------------------------------------------------
#if (DEBUG)
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###MatchMode INCOMING FIRST###");
#endif
            // 1st loop: all INCOMING offers by descending priority
            for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
            {
                int prio_lower_limit = Math.Max(0, 2 - job.m_incomingOffers[offerIndex].Priority);

                if (job.m_outgoingAmount <= 0)
                {
#if (DEBUG)
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ### MATCHMODE EXIT, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount} ###");
#endif
                    break;
                }

                MatchIncomingOffer(prio_lower_limit, offerIndex);
            } //end loop priority
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MatchOffersBalanced()
        {
            // BALANCED mode - match incoming/outgoing one by one by distance, descending priority
            // -------------------------------------------------------------------------------------------

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
                    TransferOffer incoming = job.m_incomingOffers[indexIn];
                    TransferOffer outgoing = job.m_outgoingOffers[indexOut];

                    int current_prio = Math.Max(incoming.Priority, outgoing.Priority);

                    // If current priority is 0 then we can't match anymore as 0/0 is not a valid match
                    // and all the other priorities should already have had a match attempted.
                    if (current_prio == 0)
                    {
                        break;
                    }

                    int prio_lower_limit = Math.Max(0, 2 - current_prio);   //2 and higher: match all couterparts, 0: match only 7 down to 2, 1: match 7..1

                    if (incoming.Priority == outgoing.Priority)
                    {
                        // Match whichever has less offers available so that we maximise the matches for the side with
                        // limited resources.
                        if (job.m_incomingCountRemaining <= job.m_outgoingCountRemaining)
                        {
                            MatchIncomingOffer(prio_lower_limit, indexIn);
                            indexIn++;
                        }
                        else
                        {
                            MatchOutgoingOffer(prio_lower_limit, indexOut);
                            indexOut++;
                        }
                    }
                    else if (incoming.Priority > outgoing.Priority)
                    {
                        MatchIncomingOffer(prio_lower_limit, indexIn);
                        indexIn++;
                    }
                    else
                    {
                        MatchOutgoingOffer(prio_lower_limit, indexOut);
                        indexOut++;
                    }
                }
                else if (indexIn < job.m_incomingCount)
                {
                    // Only IN remaining
                    TransferOffer incoming = job.m_incomingOffers[indexIn];
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

                    MatchIncomingOffer(prio_lower_limit, indexIn);
                    indexIn++;
                }
                else if (indexOut < job.m_outgoingCount)
                {
                    // Only OUT remaining
                    TransferOffer outgoing = job.m_outgoingOffers[indexOut];
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

                    MatchOutgoingOffer(prio_lower_limit, indexOut);
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
        private static void MatchIncomingOffer(int prio_lower_limit, int offerIndex)
        {
            // Get incoming offer reference:
            ref TransferOffer incomingOffer = ref job.m_incomingOffers[offerIndex];

            // guard: offer valid?
            if (incomingOffer.Amount <= 0)
            {
                return;
            }
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###Matching INCOMING offer: {TransferManagerUtils.DebugOffer(incomingOffer)}, priority: {incomingOffer.Priority}, remaining amount outgoing: {job.m_outgoingAmount}");
#endif
            int bestmatch_position = -1;
            float bestmatch_distance = -1f;// float.MaxValue;
            ushort inBuildingId = GetOfferBuilding(ref incomingOffer);
            float distanceMultiplier = TransferManagerModes.GetDistanceMultiplier(job.material);
            DistanceMode eDistanceMode = TransferManagerModes.GetMatchDistanceMode(job.material);

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_index = 0; counterpart_index < job.m_outgoingCount; counterpart_index++)
            {
                ref TransferOffer outgoingOffer = ref job.m_outgoingOffers[counterpart_index];

                //guard: below lower prio limit? ->end matching
                if (outgoingOffer.Priority < prio_lower_limit) break;

                // guards: out=in same? exclude offer (already used?)
                if ((outgoingOffer.Amount <= 0) || (outgoingOffer.m_object == incomingOffer.m_object)) continue;

                //guard: if both are warehouse, prevent low prio inter-warehouse transfers
                if ((incomingOffer.Exclude) && (outgoingOffer.Exclude) && (outgoingOffer.Priority < (prio_lower_limit + 1))) continue;

                //temporary exclusion due to pathfinding issues?
                if (PathfindExclusion(ref incomingOffer, ref outgoingOffer)) continue;

                // CHECK OPTION: preferlocalservice
                float districtFactor = 1f;
                bool isLocalAllowed = IsLocalDistrict(ref incomingOffer, inBuildingId, ref outgoingOffer, GetOfferBuilding(ref outgoingOffer), job.material, out districtFactor);

                // CHECK OPTION: WarehouseFirst && ImportExportPreferTrainShipPlane
                float distanceWarehouseFactor = WarehouseFirst(ref outgoingOffer, job.material, WAREHOUSE_OFFERTYPE.OUTGOING);
                float distanceOutsideFactor = OutsideModifier(ref outgoingOffer, job.material);

                // CHECK OPTION: WarehouseReserveTrucks
                bool canTransfer = WarehouseCanTransfer(ref incomingOffer, ref outgoingOffer, job.material) &&
                                    OutsideConnectionCanTransfer(ref incomingOffer, ref outgoingOffer, job.material) &&
                                    ActivePassiveCanTransfer(ref incomingOffer, ref outgoingOffer, job.material);

                // EVAL final distance
                float distanceValue = GetDistanceValue(eDistanceMode, distanceMultiplier, ref outgoingOffer, ref incomingOffer, distanceWarehouseFactor, distanceOutsideFactor, districtFactor);
                if (isLocalAllowed && canTransfer && (distanceValue > bestmatch_distance))
                {
                    bestmatch_position = counterpart_index;
                    bestmatch_distance = distanceValue;
                }
#if DEBUG
                double dDistance = Math.Sqrt(Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position));
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Matching outgoing offer: {TransferManagerUtils.DebugOffer(outgoingOffer)}, priority: {outgoingOffer.Priority}, amt {outgoingOffer.Amount}, local:{isLocalAllowed}, canTransfer:{canTransfer}, distance: {dDistance} distanceValue:{distanceValue}@{districtFactor}/{distanceWarehouseFactor}, bestmatch: {bestmatch_distance}");
#endif
            }

            // Select bestmatch
            if (bestmatch_position != -1)
            {
#if DEBUG
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Selecting bestmatch: {TransferManagerUtils.DebugOffer(job.m_outgoingOffers[bestmatch_position])}");
#endif
                // Start the transfer
                int deltaamount = Math.Min(incomingOffer.Amount, job.m_outgoingOffers[bestmatch_position].Amount);
                if (deltaamount > 0)
                {
                    QueueStartTransferMatch(job.material, ref job.m_outgoingOffers[bestmatch_position], ref incomingOffer, deltaamount);

                    // reduce offer amount
                    incomingOffer.Amount -= deltaamount;
                    job.m_outgoingOffers[bestmatch_position].Amount -= deltaamount;
                    job.m_incomingAmount -= deltaamount;
                    job.m_outgoingAmount -= deltaamount;

                    if (incomingOffer.Amount <= 0)
                    {
                        job.m_incomingCountRemaining--;
                    }
                    if (job.m_outgoingOffers[bestmatch_position].Amount <= 0)
                    {
                        job.m_outgoingCountRemaining--;
                    }
                }
            }
        }

        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MatchOutgoingOffer(int prio_lower_limit, int offerIndex)
        {
            // Get Outgoing offer reference:
            ref TransferOffer outgoingOffer = ref job.m_outgoingOffers[offerIndex];

            // guard: offer valid?
            if (outgoingOffer.Amount <= 0)
            {
                return;
            }
#if DEBUG
            DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"   ###Matching OUTGOING offer: {TransferManagerUtils.DebugOffer(outgoingOffer)}, priority: {outgoingOffer.Priority}, remaining amount incoming: {job.m_incomingAmount}");
#endif
            int bestmatch_position = -1;
            float bestmatch_distance = -1;// float.MaxValue;
            ushort outBuildingId = GetOfferBuilding(ref outgoingOffer);
            float distanceMultiplier = TransferManagerModes.GetDistanceMultiplier(job.material);
            DistanceMode eDistanceMode = TransferManagerModes.GetMatchDistanceMode(job.material);

            // loop through all matching counterpart offers and find closest one
            for (int counterpart_index = 0; counterpart_index < job.m_incomingCount; counterpart_index++)
            {
                ref TransferOffer incomingOffer = ref job.m_incomingOffers[counterpart_index];

                //guard: below lower prio limit? ->end matching
                if (incomingOffer.Priority < prio_lower_limit) break;

                // guards: out=in same? exclude offer (already used?)
                if ((incomingOffer.Amount <= 0) || (incomingOffer.m_object == outgoingOffer.m_object)) continue;

                //guard: if both are warehouse, prevent low prio inter-warehouse transfers
                if ((outgoingOffer.Exclude) && (incomingOffer.Exclude) && (incomingOffer.Priority < (prio_lower_limit + 1))) continue;

                //temporary exclusion due to pathfinding issues?
                if (PathfindExclusion(ref incomingOffer, ref outgoingOffer)) continue;

                // CHECK OPTION: preferlocalservice
                float districtFactor = 1f;
                bool isLocalAllowed = IsLocalDistrict(ref incomingOffer, GetOfferBuilding(ref incomingOffer), ref outgoingOffer, outBuildingId, job.material, out districtFactor);

                // CHECK OPTION: WarehouseFirst
                float distanceWarehouseFactor = WarehouseFirst(ref incomingOffer, job.material, WAREHOUSE_OFFERTYPE.INCOMING);
                float distanceOutsideFactor = OutsideModifier(ref incomingOffer, job.material);

                // CHECK OPTION: WarehouseReserveTrucks
                bool canTransfer = WarehouseCanTransfer(ref incomingOffer, ref outgoingOffer, job.material) &&
                                    OutsideConnectionCanTransfer(ref incomingOffer, ref outgoingOffer, job.material) &&
                                    ActivePassiveCanTransfer(ref incomingOffer, ref outgoingOffer, job.material);

                // EVAL final distance
                // Warehouse first and Prefer Plane/Train/Ship will reduce the effective distance making it more likely a warehouse is chosen
                float distanceValue = GetDistanceValue(eDistanceMode, distanceMultiplier, ref outgoingOffer, ref incomingOffer, distanceWarehouseFactor, distanceOutsideFactor, districtFactor);
                if (isLocalAllowed && canTransfer && (distanceValue > bestmatch_distance))
                {
                    bestmatch_position = counterpart_index;
                    bestmatch_distance = distanceValue;
                }
#if DEBUG
                double dDistance = Math.Sqrt(Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position));
                DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Matching incoming offer: {TransferManagerUtils.DebugOffer(incomingOffer)}, amt {incomingOffer.Amount}, local:{isLocalAllowed}, canTransfer:{canTransfer}, distance: {dDistance} distanceValue: {distanceValue}@{districtFactor}/{distanceWarehouseFactor}, bestmatch: {bestmatch_distance}");
#endif
            }

            // Select bestmatch
            if (bestmatch_position != -1)
            {
                // Start the transfer
                int deltaamount = Math.Min(outgoingOffer.Amount, job.m_incomingOffers[bestmatch_position].Amount);
                if (deltaamount > 0)
                {
#if DEBUG
                    DebugLog.LogOnly((DebugLog.LogReason)(job.material), $"       -> Selecting bestmatch: {TransferManagerUtils.DebugOffer(job.m_incomingOffers[bestmatch_position])} Amount: {deltaamount}");
#endif
                    QueueStartTransferMatch(job.material, ref outgoingOffer, ref job.m_incomingOffers[bestmatch_position], deltaamount);

                    // reduce offer amount
                    job.m_incomingOffers[bestmatch_position].Amount -= deltaamount;
                    outgoingOffer.Amount -= deltaamount;
                    job.m_incomingAmount -= deltaamount;
                    job.m_outgoingAmount -= deltaamount;

                    if (job.m_incomingOffers[bestmatch_position].Amount <= 0)
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

        /// <summary>
        /// Enqueue a StartTransfer result package with the TransferDispatcher
        /// </summary>
        [MethodImpl(256)] //=[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void QueueStartTransferMatch(TransferReason material, ref TransferOffer outgoingOffer, ref TransferOffer incomingOffer, int deltaamount)
        {
            //TransferManagerStartTransferDG(_TransferManager, material, outgoingOffer, incomingOffer, deltaamount);    //THREAD-SAFE??
            // alternative:
            CustomTransferDispatcher.Instance.EnqueueTransferResult(material, outgoingOffer, incomingOffer, deltaamount);
        }

        public static ushort GetOfferBuilding(ref TransferOffer offer)
        {
            if (offer.Building != 0)
            {
                return offer.Building;
            }
            else if (offer.Vehicle != 0)
            {
                Vehicle vehicle = _VehicleManager.m_vehicles.m_buffer[offer.Vehicle];
                return vehicle.m_sourceBuilding;
            }
            else if (offer.Citizen != 0)
            {
                Citizen citizen = _CitizenManager.m_citizens.m_buffer[offer.Citizen];
                return citizen.GetBuildingByLocation();
            }
            
            return 0;
        }

        private static float GetDistanceValue(DistanceMode eDistanceMode, float distanceMultiplier, ref TransferOffer outgoingOffer, ref TransferOffer incomingOffer, float distanceWarehouseFactor, float distanceOutsideFactor, float districtFactor)
        {
            float distanceValue;
            float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceWarehouseFactor * distanceOutsideFactor * districtFactor;
            if (eDistanceMode == DistanceMode.Distance)
            {
                // Straight distance calc                    
                if (squaredDistance == 0)
                {
                    distanceValue = float.MaxValue;
                }
                else
                {
                    distanceValue = 1000f / squaredDistance;
                }
            }
            else
            {
                // Vanilla match mode, Priority based distance
                float otherPriorityPlus = outgoingOffer.Priority + 0.1f;
                distanceValue = ((!(distanceMultiplier < 0f)) ? (otherPriorityPlus / (1f + squaredDistance * distanceMultiplier)) : (otherPriorityPlus - otherPriorityPlus / (1f - squaredDistance * distanceMultiplier)));
            }
            return distanceValue;
        }
    }
}