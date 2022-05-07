using ColossalFramework;
using TransferManagerCE.Util;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using TransferManagerCE.Settings;

namespace TransferManagerCE.CustomManager
{
    public sealed class CustomTransferManager : TransferManager
    {
        private static bool _init = false;
        public static volatile bool _runThread = true;


        // Matching logic
        private enum OFFER_MATCHMODE : int { INCOMING_FIRST = 1, OUTGOING_FIRST = 2, BALANCED = 3 };
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


        #region DELEGATES
        public static void InitDelegate()
        {
            TransferManagerStartTransferDG = FastDelegateFactory.Create<TransferManagerStartTransfer>(typeof(TransferManager), "StartTransfer", instanceMethod: true);
            CalculateOwnVehiclesDG = FastDelegateFactory.Create<CommonBuildingAICalculateOwnVehicles>(typeof(CommonBuildingAI), "CalculateOwnVehicles", instanceMethod: true);
        }

        public delegate void TransferManagerStartTransfer(TransferManager TransferManager, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta);
        public static TransferManagerStartTransfer TransferManagerStartTransferDG;

        public delegate void CommonBuildingAICalculateOwnVehicles(CommonBuildingAI CommonBuildingAI, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside);
        public static CommonBuildingAICalculateOwnVehicles CalculateOwnVehiclesDG;
        #endregion


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


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static OFFER_MATCHMODE GetMatchOffersMode(TransferReason material)
        {
            //incoming first: pick highest priority outgoing offers by distance
            //outgoing first: try to fulfill all outgoing offers by descending priority. incoming offer mapped by distance only (priority not relevant).
            //balanced: outgoing/incoming together by priorty descending
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Grain:
                case TransferReason.Lumber:
                case TransferReason.Logs:
                case TransferReason.Goods:              
                case TransferReason.LuxuryProducts:                  
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Metals:
                case TransferReason.Glass:
                case TransferReason.PlanedTimber:
                case TransferReason.Paper:
                case TransferReason.Fish:
                //warehouse incoming behaviour: empty=prio 0; balanced=prio 0-2; fill=prio 2;
                //warehouse outgoing behaviour: empty=prio 2 ; balanced=prio 0-2; fill=prio 0;
                    return OFFER_MATCHMODE.BALANCED;

                // all mail services like goods transfers:
                case TransferReason.Mail:               //outgoing (passive) from buidings, incoming(active) from postoffice
                case TransferReason.SortedMail:         //outside connections outgoing(active), incoming(passive) from postoffice
                case TransferReason.UnsortedMail:       //outgoing(active) from ???, incoming(passive) from postsortingfacilities
                case TransferReason.IncomingMail:       //outside connections outgoing(active), incoming(passive) from postsortingfacilities
                case TransferReason.OutgoingMail:       //outside connections incoming(passive)
                    return OFFER_MATCHMODE.BALANCED;

                // Services which should be outgoing first, but also benefit from incoming match-making (vehicles in the field with capacity to spare)
                case TransferReason.Garbage:            //Garbage: outgoing offer (passive) from buldings with garbage to be collected, incoming (active) from landfills
                case TransferReason.Crime:              //Crime: outgoing offer (passive) 
                case TransferReason.Dead:               //Dead: outgoing offer (passive) 
                case TransferReason.Collapsed:          //Collapsed: outgoing (passive) from buildings
                case TransferReason.Collapsed2:         //Collapsed2: helicopter
                case TransferReason.Snow:               //outgoing (passive) from netsegements, incoming (active) from snowdumps
                case TransferReason.RoadMaintenance:    //incoming (passive) from netsegments, outgoing (active) from maintenance depot
                case TransferReason.ParkMaintenance:    //incoming (passive) from park main gate building, 
                case TransferReason.CriminalMove:       //outging (passive) from policestations, incoming(active) from prisons (REVERSED ACTIVE/PASSIVE COMPARED TO OTHER MOVE TRANSFERS!)
                case TransferReason.GarbageTransfer:    //GarbageTransfer: outgoing (passive) from landfills/wtf, incoming (active) from wasteprocessingcomplex
                case TransferReason.GarbageMove:        //GarbageMove: outgoing (active) from emptying landfills, incoming (passive) from receiving landfills/wastetransferfacilities/wasteprocessingcomplex
                case TransferReason.DeadMove:           //outgoing (active) from emptying, incoming (passive) from receiving
                case TransferReason.SnowMove:           //outgoing (active) from emptying snowdumps, incoming (passive) from receiving
                    return OFFER_MATCHMODE.BALANCED;                        

                case TransferReason.ForestFire:         //like Fire2
                case TransferReason.Fire2:              //Fire2: helicopter
                case TransferReason.Fire:               //Fire: outgoing offer (passive) - always prio7
                case TransferReason.Sick:               //Sick: outgoing offer (passive) [special case: citizen with outgoing and active]
                case TransferReason.Sick2:              //Sick2: helicopter
                case TransferReason.SickMove:           //outgoing(active) from medcopter, incoming(passive) from hospitals -> moved to outgoing first so closest clinic is used
                    return OFFER_MATCHMODE.OUTGOING_FIRST;

                case TransferReason.Taxi:               //outgoing(active) from depots/taxis, incoming(passive) from citizens and taxistands
                    return OFFER_MATCHMODE.INCOMING_FIRST;

                default: 
                    return OFFER_MATCHMODE.BALANCED;
            }
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool IsLocalUse(ref TransferOffer offerIn, ref TransferOffer offerOut, TransferReason material, int priority, out float distanceModifier)
        {
            const int PRIORITY_THRESHOLD_LOCAL = 3;     //upper prios also get non-local fulfillment
            const float LOCAL_DISTRICT_MODIFIER = 0.1f;   //modifier for distance within same district
            bool isMoveTransfer = false;
            bool isLocal = false;
            distanceModifier = 1.0f;

            // guard: current option setting?
            if (!ModSettings.GetSettings().optionPreferLocalService)
                return true;

            // priority of passive side above threshold -> any service is OK!
            priority = offerIn.Active ? offerOut.Priority : offerIn.Priority;
            if (priority >= PRIORITY_THRESHOLD_LOCAL)
            { 
                isLocal = true;
                // continue logic to set distanceModifier for service within same district
            }

            switch (material)
            {
                // Services subject to prefer local services:
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                    break;

                case TransferReason.Dead:                     
                    //isLocal = true;           //always allow but continue logic to profit from reduced distancemodifier if within same district
                    break;

                // Goods subject to prefer local:
                // -none-

                // Material Transfers for services subject to policy:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.CriminalMove:
                //case TransferReason.SickMove: //removed,as using homebuilding for medcopter does not make sense. let it choose closest clinic for dropoff
                case TransferReason.SnowMove:
                case TransferReason.DeadMove:
                    isMoveTransfer = true;      //Move Transfers: incoming offer is passive, allow move/emptying to global district buildings
                    break;

                default:
                    return true;                //guard: dont apply district logic to other materials
            }

            // determine buildings or vehicle parent buildings
            ushort buildingIncoming = 0, buildingOutgoing = 0;

            if (offerIn.Building != 0) buildingIncoming = offerIn.Building;
            else if (offerIn.Vehicle != 0) buildingIncoming = _VehicleManager.m_vehicles.m_buffer[offerIn.Vehicle].m_sourceBuilding;
            else if (offerIn.Citizen != 0) buildingIncoming = _CitizenManager.m_citizens.m_buffer[offerIn.Citizen].m_homeBuilding;

            if (offerOut.Building != 0) buildingOutgoing = offerOut.Building;
            else if (offerOut.Vehicle != 0) buildingOutgoing = _VehicleManager.m_vehicles.m_buffer[offerOut.Vehicle].m_sourceBuilding;
            else if (offerOut.Citizen != 0) buildingOutgoing = _CitizenManager.m_citizens.m_buffer[offerOut.Citizen].m_homeBuilding;

            // get respective districts
            byte districtIncoming = _DistrictManager.GetDistrict(_BuildingManager.m_buildings.m_buffer[buildingIncoming].m_position);
            byte districtOutgoing = _DistrictManager.GetDistrict(_BuildingManager.m_buildings.m_buffer[buildingOutgoing].m_position);

            // return true if: both within same district, or active offer is outside district ("in global area")
            if ((districtIncoming == districtOutgoing)
                  || (offerIn.Active && districtIncoming == 0)
                  || (offerOut.Active && districtOutgoing == 0)
                  || (isMoveTransfer && districtIncoming == 0)
               )
            {
                isLocal = true;

                // really same district? set modifier!
                if ((districtIncoming == districtOutgoing) && (districtIncoming != 0))
                    distanceModifier = LOCAL_DISTRICT_MODIFIER;
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
                DebugLog.LogDebug((DebugLog.LogReason)material, $"       ** Outside Offer: material: {material}, {DebugInspectOffer(ref offer)}, SubService class is: {_BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_class.m_subService}");

                if ((_BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_class.m_subService == ItemClass.SubService.PublicTransportTrain) ||
                    (_BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_class.m_subService == ItemClass.SubService.PublicTransportShip) ||
                    (_BuildingManager.m_buildings.m_buffer[offer.Building].Info?.m_class.m_subService == ItemClass.SubService.PublicTransportPlane))
                    return OUTSIDE_MODIFIER;
            }

            return 1f;
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float WarehouseFirst(ref TransferOffer offer, TransferReason material, WAREHOUSE_OFFERTYPE whInOut)
        {
            const float WAREHOUSE_MODIFIER = 0.1f;   //modifier for distance for warehouse

            if (!ModSettings.GetSettings().optionWarehouseFirst)
                return 1f;

            if (offer.Exclude)  //TransferOffer.Exclude is only ever set by WarehouseAI!
            {
                Building.Flags isFilling = (_BuildingManager.m_buildings.m_buffer[offer.Building].m_flags & Building.Flags.Filling);
                Building.Flags isEmptying = (_BuildingManager.m_buildings.m_buffer[offer.Building].m_flags & Building.Flags.Downgrading);

                // Filling Warehouses dont like to fulfill outgoing offers,
                // emptying warehouses dont like to fulfill incoming offers
                if ((whInOut == WAREHOUSE_OFFERTYPE.INCOMING && isEmptying != Building.Flags.None) ||
                    (whInOut == WAREHOUSE_OFFERTYPE.OUTGOING && isFilling != Building.Flags.None))
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
            // Option: optionWarehouseReserveTrucks
            if ((ModSettings.GetSettings().optionWarehouseReserveTrucks) && (outgoingOffer.Exclude && outgoingOffer.Active)) //further checks only relevant if outgoing from warehouse and active
            {
                // is outgoing a warehouse with active delivery, and is counterpart incoming an outside connection?
                if ((incomingOffer.Building != 0) && (_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI && _BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI is WarehouseAI))
                {
                    // guards: there are warehouses that DO NOT derive from warehouseAI, notably BARGES by bloodypenguin. We ignore them here and just allow any transfer.
                    int total = (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI as WarehouseAI).m_truckCount;
                    int count = 0, cargo = 0, capacity = 0, outside = 0;
                    float maxExport = (total * 0.75f);

                    CalculateOwnVehiclesDG(_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI as WarehouseAI,
                                            outgoingOffer.Building, ref _BuildingManager.m_buildings.m_buffer[outgoingOffer.Building], material, ref count, ref cargo, ref capacity, ref outside);

                    DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       ** checking canTransfer: total: {total}, ccco: {count}/{cargo}/{capacity}/{outside} => {((float)(outside + 1f) > maxExport)}");
                    if ((float)(outside + 1f) > maxExport)
                        return false;   //dont need further checks, we would be over reserved truck limit
                }
            }

            // Option: optionWarehouseNewBalanced
            if (ModSettings.GetSettings().optionWarehouseNewBalanced && (outgoingOffer.Exclude || incomingOffer.Exclude))
            {
                // attempting to import?
                if ((incomingOffer.Exclude) && (outgoingOffer.Building != 0) && (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI && _BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info?.m_buildingAI is WarehouseAI))
                {
                    bool isFilling  = (_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].m_flags & (Building.Flags.Filling)) != Building.Flags.None;
                    bool isBalanced = (_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].m_flags & (Building.Flags.Downgrading)) == Building.Flags.None &&
                                      (_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].m_flags & (Building.Flags.Filling)) == Building.Flags.None;

                    float current_filllevel = (float)(_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].m_customBuffer1 * 100) / ((_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info.m_buildingAI as WarehouseAI).m_storageCapacity);
                    DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       ** warehouse checking import restrictions: balanced/filling={isBalanced}/{isFilling}, filllevel={current_filllevel}");

                    if (isBalanced && current_filllevel >= 0.25f)
                        return false;   //balanced: over 25% fill level: no more imports!

                    if (isFilling && current_filllevel >= 0.75f)
                        return false;   //filling: over 75% fill level: no more imports!
                }

                // attempting to export?
                else if ((outgoingOffer.Exclude) && (incomingOffer.Building != 0) && (_BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI && _BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI is WarehouseAI))
                {
                    bool isEmptying = (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].m_flags & (Building.Flags.Downgrading)) != Building.Flags.None;
                    bool isBalanced = (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].m_flags & (Building.Flags.Downgrading)) == Building.Flags.None &&
                                      (_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].m_flags & (Building.Flags.Filling)) == Building.Flags.None;

                    float current_filllevel = (float)(_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].m_customBuffer1 * 100) / ((_BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info.m_buildingAI as WarehouseAI).m_storageCapacity);
                    DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       ** warehouse checking export restrictions: balanced/empyting={isBalanced}/{isEmptying}, filllevel={current_filllevel}");
                    
                    if (isBalanced && current_filllevel <= 0.75f)
                        return false;   //balanced: under 75% fill level: no more exports!

                    if (isEmptying && current_filllevel <= 0.2f)
                        return false;   //empyting: under 20% fill level: no more exports!
                }

            }

            // all good -> allow transfer
            return true;
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
                    DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded: material:{job.material}, in:{incomingOffer.Building}({DebugInspectOffer(ref incomingOffer)}) and out:{outgoingOffer.Building}({DebugInspectOffer(ref outgoingOffer)})");
                }
            }

            //check failed outside connection pair
            if ((incomingOffer.Building != 0) && _BuildingManager.m_buildings.m_buffer[incomingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI)
            {
                if (incomingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(incomingOffer.Building, outgoingOffer.Building);
                else if (outgoingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(outgoingOffer.Building, incomingOffer.Building);

                if (result)
                {
                    DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded incoming outsideconnection: material:{job.material}, {incomingOffer.Building}({DebugInspectOffer(ref incomingOffer)})");
                }
            }
            else if ((outgoingOffer.Building != 0) && _BuildingManager.m_buildings.m_buffer[outgoingOffer.Building].Info?.m_buildingAI is OutsideConnectionAI)
            {
                if (incomingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(incomingOffer.Building, outgoingOffer.Building);
                else if (outgoingOffer.Active)
                    result = PathFindFailure.FindOutsideConnectionPair(outgoingOffer.Building, incomingOffer.Building);

                if (result)
                {
                    DebugLog.LogDebug(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded ougoing outsideconnection: material:{job.material}, {outgoingOffer.Building}({DebugInspectOffer(ref outgoingOffer)})");
                }
            }

            return result;
        }


        private static String DebugInspectOffer(ref TransferOffer offer)
        {
            var instB = default(InstanceID);
            instB.Building = offer.Building;
            return (offer.Building > 0 && offer.Building < _BuildingManager.m_buildings.m_size) ? _BuildingManager.m_buildings.m_buffer[offer.Building].Info?.name + "(" + _InstanceManager.GetName(instB) + ")"
                    : (offer.Vehicle > 0 && offer.Vehicle < _VehicleManager.m_vehicles.m_size)  ? _VehicleManager.m_vehicles.m_buffer[offer.Vehicle].Info?.name 
                    : (offer.Citizen > 0) ? $"Citizen={offer.Citizen}"
                    : (offer.NetSegment > 0) ? $"NetSegment={offer.NetSegment}"
                    : (offer.TransportLine > 0) ? $"TransportLine={offer.TransportLine}"
                    : "unknown";
        }


        [Conditional("DEBUG")]
        private static void DebugPrintAllOffers(TransferReason material, int offerCountIncoming, int offerCountOutgoing)
        {
            for (int i=0; i< offerCountIncoming; i++)
            {
                ref TransferOffer incomingOffer = ref job.m_incomingOffers[i];
                String bname = DebugInspectOffer(ref incomingOffer);
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   in #{i}: prio: {incomingOffer.Priority}, act {incomingOffer.Active}, excl {incomingOffer.Exclude}, amt {incomingOffer.Amount}, bvcnt {incomingOffer.Building}/{incomingOffer.Vehicle}/{incomingOffer.Citizen}/{incomingOffer.NetSegment}/{incomingOffer.TransportLine} name={bname}");
            }

            for (int i = 0; i < offerCountOutgoing; i++)
            {
                ref TransferOffer outgoingOffer = ref job.m_outgoingOffers[i];
                String bname = DebugInspectOffer(ref outgoingOffer);
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   out #{i}: prio: {outgoingOffer.Priority}, act {outgoingOffer.Active}, excl {outgoingOffer.Exclude}, amt {outgoingOffer.Amount}, bvcnt {outgoingOffer.Building}/{outgoingOffer.Vehicle}/{outgoingOffer.Citizen}/{outgoingOffer.NetSegment}/{outgoingOffer.TransportLine} name={bname}");
            }
        }


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
                    // chirp about pathfinding issues
                    PathFindFailure.SendPathFindChirp();

                    // clean pathfind LRU
                    PathFindFailure.RemoveOldEntries();

                    // wait for signal
                    DebugLog.LogDebug(DebugLog.REASON_ALL, $"MatchOffersThread: waiting for work signal...");
                    CustomTransferDispatcher._waitHandle.WaitOne();
                }

            }

            DebugLog.LogInfo($"MatchOffersThread: Thread ended.");
        }


        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MatchOffers(TransferReason material)
        {
            const int REJECT_LOW_PRIORITY = 1;  //reject priorities below

            // delayed initialization until first call
            if (!_init)
            {
                Init();
                CheckInit();
            }

            // guard: ignore transferreason.none
            if (material == TransferReason.None)
                return;


            // DEBUG LOGGING
            DebugLog.LogInfo($"-- TRANSFER REASON: {material.ToString()}, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount}, count in {job.m_incomingCount}, count out {job.m_outgoingCount}");
#if (DEBUG)
            DebugPrintAllOffers(material, job.m_incomingCount, job.m_outgoingCount);
#endif

            // Select offer matching algorithm
            OFFER_MATCHMODE match_mode = GetMatchOffersMode(material);


            // OUTGOING FIRST mode - try to fulfill all outgoing requests by finding incomings by distance
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.OUTGOING_FIRST)
            {
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ###MatchMode OUTGOING FIRST###");
                bool has_counterpart_offers = true;

                // 1st loop: all OUTGOING offers by descending priority
                for (int offerIndex = 0; offerIndex < job.m_outgoingCount; offerIndex++)
                {
                    int prio_lower_limit = Math.Max(0, 2 - job.m_outgoingOffers[offerIndex].Priority);

                    if (job.m_incomingAmount <= 0 || !has_counterpart_offers)
                    {
                        DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ### MATCHMODE EXIT, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount}, has_counterparts {has_counterpart_offers} ###");
                        goto END_OFFERMATCHING;
                    }

                    has_counterpart_offers = MatchOutgoingOffer(prio_lower_limit, offerIndex);
                } //end loop priority

            } //end OFFER_MATCHMODE.OUTGOING_FIRST


            // INCOMING FIRST mode - try to fulfill all incoming offers by finding outgoings by distance
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.INCOMING_FIRST)
            {
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ###MatchMode INCOMING FIRST###");
                bool has_counterpart_offers = true;

                // 1st loop: all INCOMING offers by descending priority
                for (int offerIndex = 0; offerIndex < job.m_incomingCount; offerIndex++)
                {
                    int prio_lower_limit = Math.Max(0, 2 - job.m_incomingOffers[offerIndex].Priority);

                    if (job.m_outgoingAmount <= 0 || !has_counterpart_offers)
                    {
                        DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ### MATCHMODE EXIT, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount}, has_counterparts {has_counterpart_offers} ###");
                        goto END_OFFERMATCHING;
                    }

                    has_counterpart_offers = MatchIncomingOffer(prio_lower_limit, offerIndex);
                } //end loop priority

            } //end OFFER_MATCHMODE.INCOMING_FIRST


            // BALANCED mode - match incoming/outgoing one by one by distance, descending priority
            // -------------------------------------------------------------------------------------------
            if (match_mode == OFFER_MATCHMODE.BALANCED)
            {
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ###MatchMode BALANCED###");
                bool has_counterpart_offers = true;
                int maxoffers = job.m_incomingCount + job.m_outgoingCount;

                // loop incoming+outgoing offers by descending priority
                for (int offerIndex = 0, indexIn=0, indexOut=0; offerIndex < maxoffers; offerIndex++)
                {
                    int current_prio = Math.Max(job.m_incomingOffers[indexIn].Priority, job.m_outgoingOffers[indexOut].Priority);
                    if (current_prio < REJECT_LOW_PRIORITY)
                        break;
                    
                    int prio_lower_limit = Math.Max(0, 2 - current_prio);   //2 and higher: match all couterparts, 0: match only 7 down to 2, 1: match 7..1
                    has_counterpart_offers = false;

                    // match incoming if within current priority
                    if (indexIn < job.m_incomingCount && job.m_incomingOffers[indexIn].Priority == current_prio)
                    {
                        has_counterpart_offers |= MatchIncomingOffer(prio_lower_limit, indexIn);
                        indexIn++;
                    }

                    // match outgoing if within current priority
                    if (indexOut < job.m_outgoingCount && job.m_outgoingOffers[indexOut].Priority == current_prio)
                    {
                        has_counterpart_offers |= MatchOutgoingOffer(prio_lower_limit, indexOut);
                        indexOut++;
                    }

                    // no more matches possible?
                    if (job.m_incomingAmount <= 0 || job.m_outgoingAmount <= 0 || !has_counterpart_offers)
                    {
                        DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ### MATCHMODE EXIT, amt in {job.m_incomingAmount}, amt out {job.m_outgoingAmount}, has_counterparts {has_counterpart_offers} ###");
                        goto END_OFFERMATCHING;
                    }
                }

            } //end OFFER_MATCHMODE.BALANCED


        END_OFFERMATCHING:
            // finally: match job finished
            ;
        }


        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool MatchIncomingOffer(int prio_lower_limit, int offerIndex)
        {
            // Get incoming offer reference:
            ref TransferOffer incomingOffer = ref job.m_incomingOffers[offerIndex];

            // guard: offer valid?
            if (incomingOffer.Amount <= 0) return true;

            DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ###Matching INCOMING offer: {DebugInspectOffer(ref incomingOffer)}, priority: {incomingOffer.Priority}, remaining amount outgoing: {job.m_outgoingAmount}");

            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;
            bool counterpartMatchesLeft = false;

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
                bool isLocalAllowed = IsLocalUse(ref incomingOffer, ref outgoingOffer, job.material, incomingOffer.Priority, out districtFactor);

                // CHECK OPTION: WarehouseFirst && ImportExportPreferTrainShipPlane
                float distanceFactor = WarehouseFirst(ref outgoingOffer, job.material, WAREHOUSE_OFFERTYPE.OUTGOING);
                distanceFactor *= OutsideModifier(ref outgoingOffer, job.material);

                // CHECK OPTION: WarehouseReserveTrucks
                bool canTransfer = WarehouseCanTransfer(ref incomingOffer, ref outgoingOffer, job.material);

                // EVAL final distance
                float distance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceFactor * districtFactor;
                if (isLocalAllowed && canTransfer && (distance < bestmatch_distance))
                {
                    bestmatch_position = counterpart_index;
                    bestmatch_distance = distance;
                }

                counterpartMatchesLeft = true;
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       -> Matching outgoing offer: {DebugInspectOffer(ref outgoingOffer)}, amt {outgoingOffer.Amount}, local:{isLocalAllowed}, canTransfer:{canTransfer}, distance: {distance}@{districtFactor}/{distanceFactor}, bestmatch: {bestmatch_distance}");                
            }

            // Select bestmatch
            if (bestmatch_position != -1)
            {
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       -> Selecting bestmatch: {DebugInspectOffer(ref job.m_outgoingOffers[bestmatch_position])}");
                // ATTENTION: last outgoingOffer is NOT necessarily the bestmatch!

                // Start the transfer
                int deltaamount = Math.Min(incomingOffer.Amount, job.m_outgoingOffers[bestmatch_position].Amount);
                QueueStartTransferMatch(job.material, ref job.m_outgoingOffers[bestmatch_position], ref incomingOffer, deltaamount);

                // reduce offer amount
                incomingOffer.Amount -= deltaamount;
                job.m_outgoingOffers[bestmatch_position].Amount -= deltaamount;
                job.m_incomingAmount -= deltaamount;
                job.m_outgoingAmount -= deltaamount;
            }

            return counterpartMatchesLeft;
        }


        /// <returns>counterpartmacthesleft?</returns>
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool MatchOutgoingOffer(int prio_lower_limit, int offerIndex)
        {
            // Get Outgoing offer reference:
            ref TransferOffer outgoingOffer = ref job.m_outgoingOffers[offerIndex];

            // guard: offer valid?
            if (outgoingOffer.Amount <= 0) return true;

            DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"   ###Matching OUTGOING offer: {DebugInspectOffer(ref outgoingOffer)}, priority: {outgoingOffer.Priority}, remaining amount incoming: {job.m_incomingAmount}");

            int bestmatch_position = -1;
            float bestmatch_distance = float.MaxValue;
            bool counterpartMatchesLeft = false;

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
                bool isLocalAllowed = IsLocalUse(ref incomingOffer, ref outgoingOffer, job.material, outgoingOffer.Priority, out districtFactor);

                // CHECK OPTION: WarehouseFirst
                float distanceFactor = WarehouseFirst(ref incomingOffer, job.material, WAREHOUSE_OFFERTYPE.INCOMING);
                distanceFactor *= OutsideModifier(ref incomingOffer, job.material);

                // CHECK OPTION: WarehouseReserveTrucks
                bool canTransfer = WarehouseCanTransfer(ref incomingOffer, ref outgoingOffer, job.material);

                // EVAL final distance
                float distance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position) * distanceFactor * districtFactor;
                if ((isLocalAllowed && canTransfer) && (distance < bestmatch_distance))
                {
                    bestmatch_position = counterpart_index;
                    bestmatch_distance = distance;
                }

                counterpartMatchesLeft = true;
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       -> Matching incoming offer: {DebugInspectOffer(ref incomingOffer)}, amt {incomingOffer.Amount}, local:{isLocalAllowed}, canTransfer:{canTransfer}, distance: {distance}@{districtFactor}/{distanceFactor}, bestmatch: {bestmatch_distance}");
            }

            // Select bestmatch
            if (bestmatch_position != -1)
            {
                DebugLog.LogDebug((DebugLog.LogReason)(job.material), $"       -> Selecting bestmatch: {DebugInspectOffer(ref job.m_incomingOffers[bestmatch_position])}");
                // ATTENTION: last incomingOffer is NOT necessarily the bestmatch!

                // Start the transfer
                int deltaamount = Math.Min(outgoingOffer.Amount, job.m_incomingOffers[bestmatch_position].Amount);
                QueueStartTransferMatch(job.material, ref outgoingOffer, ref job.m_incomingOffers[bestmatch_position], deltaamount);

                // reduce offer amount
                job.m_incomingOffers[bestmatch_position].Amount -= deltaamount;
                outgoingOffer.Amount -= deltaamount;
                job.m_incomingAmount -= deltaamount;
                job.m_outgoingAmount -= deltaamount;
            }

            return counterpartMatchesLeft;
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


    }
}