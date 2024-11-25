using ColossalFramework;
using ColossalFramework.Math;
using ICities;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.CustomManager.CustomTransferOffer;
using static TransferManagerCE.VehicleTypeHelper;

namespace TransferManagerCE
{
    public class CitiesUtils
    {
        public static void CalculateGuestVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleID = data.m_guestVehicles;
            int num = 0;
            while (vehicleID != (ushort)0)
            {
                if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[(int)vehicleID].m_transferType == material)
                {
                    int size;
                    int max;
                    instance.m_vehicles.m_buffer[(int)vehicleID].Info.m_vehicleAI.GetSize(vehicleID, ref instance.m_vehicles.m_buffer[(int)vehicleID], out size, out max);
                    cargo += Mathf.Min(size, max);
                    capacity += max;
                    ++count;
                    if ((instance.m_vehicles.m_buffer[(int)vehicleID].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
                        ++outside;
                }
                vehicleID = instance.m_vehicles.m_buffer[(int)vehicleID].m_nextGuestVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        private static void AddCitizenToList(uint cim, ushort usBuildingId, Citizen.Flags flag, List<uint> cimList)
        {
            if (cim != 0)
            {
                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[cim];
                if ((citizen.m_flags & flag) == flag && 
                    (citizen.GetBuildingByLocation() == usBuildingId))
                {
                    cimList.Add(cim);
                }
            }
        }

        public static List<uint> GetCitizens(ushort usBuildingId, Building building, Citizen.Flags flags)
        {
            List<uint> cimList = new List<uint>();

            int iLoopCount = 0;
            uint uintCitizenUnit = building.m_citizenUnits;
            while (uintCitizenUnit != 0)
            {
                CitizenUnit citizenUnit = CitizenManager.instance.m_units.m_buffer[uintCitizenUnit];
                for (int i = 0; i < 5; ++i)
                {
                    AddCitizenToList(citizenUnit.GetCitizen(i), usBuildingId, flags, cimList);
                }
                
                uintCitizenUnit = citizenUnit.m_nextUnit;

                // Check for bad list
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    return new List<uint>();
                }
            }

            return cimList;
        }

        public static List<uint> GetDead(ushort usBuildingId, Building building)
        {
            return GetCitizens(usBuildingId, building, Citizen.Flags.Dead);
        }

        public static List<uint> GetSick(ushort usBuildingId, Building building)
        {
            return GetCitizens(usBuildingId, building, Citizen.Flags.Sick);
        }

        public static List<uint> GetCriminals(ushort usBuildingId, Building building)
        {
            return GetCitizens(usBuildingId, building, Citizen.Flags.Criminal);
        }

        public static List<uint> GetCitizens(Building building, CitizenUnit.Flags unitFlags)
        {
            List<uint> cimList = new List<uint>();

            int iLoopCount = 0;
            uint uintCitizenUnit = building.m_citizenUnits;
            while (uintCitizenUnit != 0)
            {
                CitizenUnit citizenUnit = CitizenManager.instance.m_units.m_buffer[uintCitizenUnit];
                if (unitFlags == CitizenUnit.Flags.None || (citizenUnit.m_flags & unitFlags) == unitFlags)
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        uint citizen = citizenUnit.GetCitizen(i);
                        if (citizen != 0)
                        {
                            cimList.Add(citizen);
                        }
                    }
                }

                uintCitizenUnit = citizenUnit.m_nextUnit;

                // Check for bad list
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    return new List<uint>();
                }
            }

            return cimList;
        }

        public static List<uint> GetCriminalsAtPoliceStation(Building building)
        {
            List<uint> criminals = new List<uint>(); 
            
            List<uint> cims = GetCitizens(building, CitizenUnit.Flags.Visit);
            foreach (uint cim in cims)
            {
                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[cim];
                if (citizen.CurrentLocation == Citizen.Location.Visit)
                {
                    criminals.Add(cim);
                }
            }

            return criminals;
        }

        public static List<ushort> GetHearsesOnRoute(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            List<ushort> vehicles = GetGuestVehiclesForBuilding(buidlingId);
            foreach (ushort vehicleId in vehicles)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if ((vehicle.m_flags & Vehicle.Flags.TransferToSource) == Vehicle.Flags.TransferToSource &&
                    (vehicle.Info != null && (vehicle.Info.m_vehicleAI is HearseAI)))
                {
                    list.Add(vehicleId);
                }
            }

            return list;
        }

        public static List<ushort> GetAmbulancesOnRoute(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            List<ushort> vehicles = GetGuestVehiclesForBuilding(buidlingId);
            foreach (ushort vehicleId in vehicles)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if ((vehicle.m_flags & Vehicle.Flags.TransferToSource) == Vehicle.Flags.TransferToSource &&
                    (vehicle.Info != null && (vehicle.Info.m_vehicleAI is AmbulanceAI || vehicle.Info.m_vehicleAI is AmbulanceCopterAI)))
                {
                    list.Add(vehicleId);
                }
            }

            return list;
        }

        public static List<ushort> GetGoodsTrucksOnRoute(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            List<ushort> vehicles = GetGuestVehiclesForBuilding(buidlingId);
            foreach (ushort vehicleId in vehicles)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if ((vehicle.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.TransferToTarget &&
                    (vehicle.Info != null && vehicle.Info.m_vehicleAI is CargoTruckAI) && 
                    vehicle.m_sourceBuilding != 0) // Quite often importing vehicles have no target till they get to a cargo staion etc...
                {
                    list.Add(vehicleId);
                }
            }

            return list;
        }

        public static List<ushort> GetPoliceOnRoute(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            List<ushort> vehicles = GetGuestVehiclesForBuilding(buidlingId);
            foreach (ushort vehicleId in vehicles)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if ((vehicle.m_flags & Vehicle.Flags.TransferToSource) == Vehicle.Flags.TransferToSource &&
                    (vehicle.Info != null && (vehicle.Info.m_vehicleAI is PoliceCarAI || vehicle.Info.m_vehicleAI is PoliceCopterAI)))
                {
                    list.Add(vehicleId);
                }
            }

            return list;
        }

        public static List<ushort> GetGuestVehiclesForBuilding(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buidlingId];

            int iLoopCount = 0;
            ushort vehicleId = building.m_guestVehicles;
            while (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    list.Add(vehicleId);
                }
                vehicleId = vehicle.m_nextGuestVehicle;

                // Check for bad list
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    return new List<ushort>();
                }
            }

            return list;
        }

        public static List<ushort> GetGuestParentVehiclesForBuilding(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buidlingId];

            int iLoopCount = 0;
            ushort vehicleId = building.m_guestVehicles;
            while (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    if (vehicle.m_cargoParent == 0)
                    {
                        list.Add(vehicleId);
                    }
                    else if (!list.Contains(vehicle.m_cargoParent))
                    {
                        list.Add(vehicle.m_cargoParent);
                    }
                }
                vehicleId = vehicle.m_nextGuestVehicle;

                // Check for bad list
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    return new List<ushort>();
                }
            }

            return list;
        }

        public static int GetActiveVehicleCount(Building building, TransferReason material)
        {
            int iVehicles = 0;
            int iLoopCount = 0;
            ushort usVehicleId = building.m_ownVehicles;
            while (usVehicleId != 0)
            {
                // Check transfer type matches
                Vehicle oVehicle = VehicleManager.instance.m_vehicles.m_buffer[usVehicleId];
                if ((TransferReason)oVehicle.m_transferType == material)
                {
                    iVehicles++;
                }

                // Update for next car
                usVehicleId = oVehicle.m_nextOwnVehicle;

                // Check for bad list
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return iVehicles;
        }

        public static List<ushort> GetOwnVehiclesForBuilding(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buidlingId];
            if (building.m_flags != 0)
            {
                uint uiSize = VehicleManager.instance.m_vehicles.m_size;
                int iLoopCount = 0;
                ushort vehicleId = building.m_ownVehicles;
                while (vehicleId != 0 && vehicleId < uiSize)
                {
                    Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                    if (vehicle.m_flags != 0)
                    {
                        list.Add(vehicleId);
                    }
                    vehicleId = vehicle.m_nextOwnVehicle;

                    // Check for bad list
                    if (++iLoopCount > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        return new List<ushort>();
                    }
                }
            }

            return list;
        }

        public static List<ushort> GetOwnParentVehiclesForBuilding(ushort buildingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                uint uiSize = VehicleManager.instance.m_vehicles.m_size;
                int iLoopCount = 0;
                ushort vehicleId = building.m_ownVehicles;
                while (vehicleId != 0 && vehicleId < uiSize)
                {
                    Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                    if (vehicle.m_cargoParent == 0)
                    {
                        list.Add(vehicleId);
                    }
                    else if (!list.Contains(vehicle.m_cargoParent))
                    {
                        list.Add(vehicle.m_cargoParent);
                    }
                    vehicleId = vehicle.m_nextOwnVehicle;

                    // Check for bad list
                    if (++iLoopCount > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        return new List<ushort>();
                    }
                }
            }

            return list;
        }

        public static string GetBuildingName(ushort buildingId)
        {
            string sName = "";
            if (buildingId != 0 && buildingId < BuildingManager.instance.m_buildings.m_size)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != Building.Flags.None)
                {
                    sName = Singleton<BuildingManager>.instance.GetBuildingName(buildingId, InstanceID.Empty);
                    if (string.IsNullOrEmpty(sName))
                    {
                        sName = "Building:" + buildingId;
                    }
#if DEBUG
                    sName = "(" + buildingId + ") " + sName;
#endif
                    return sName;
                }
                else
                {
                    sName = "Building:" + buildingId;
                }
            }

            return sName;
        }

        public static string GetVehicleName(ushort vehicleId)
        {
            if (vehicleId > 0 && vehicleId < VehicleManager.instance.m_vehicles.m_size)
            {
                string sMessage = Singleton<VehicleManager>.instance.GetVehicleName(vehicleId);
                if (string.IsNullOrEmpty(sMessage))
                {
                    sMessage = "Vehicle:" + vehicleId;
                }
#if DEBUG
                return "(" + vehicleId + ") " + sMessage;
#else
                return sMessage;
#endif
            }
            return "";
        }

        public static string GetCitizenName(uint citizenId)
        {
            if (citizenId != 0)
            {
                string sMessage = Singleton<CitizenManager>.instance.GetCitizenName(citizenId);
                if (string.IsNullOrEmpty(sMessage))
                {
                    sMessage = "Citizen:" + citizenId;
                }
#if DEBUG
                return "(" + citizenId + ") " + sMessage;
#else
                return sMessage;
#endif
            }
            return "";
        }

        public static Vector3 GetCitizenInstancePosition(ushort CitizenInstanceId)
        {
            if (CitizenInstanceId > 0 && CitizenInstanceId < CitizenManager.instance.m_instances.m_size)
            {
                ref CitizenInstance cimInstance = ref CitizenManager.instance.m_instances.m_buffer[CitizenInstanceId];
                switch (cimInstance.m_lastFrame)
                {
                    case 0: return cimInstance.m_frame0.m_position;
                    case 1: return cimInstance.m_frame1.m_position;
                    case 2: return cimInstance.m_frame2.m_position;
                    case 3: return cimInstance.m_frame3.m_position;
                }

                return cimInstance.m_frame0.m_position;
            }
            return Vector3.zero;
        }

        public static void ShowBuilding(ushort buildingId, bool bZoom = false)
        {
            if (buildingId > 0 && buildingId < BuildingManager.instance.m_buildings.m_size)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                Vector3 oPosition = building.m_position;
                InstanceID buildingInstance = new InstanceID { Building = buildingId };
                ToolsModifierControl.cameraController.SetTarget(buildingInstance, oPosition, bZoom);
            }
            else
            {
                Debug.Log("BuildingId out of range: " + buildingId);
            }
        }

        public static void ShowPark(byte parkId, bool bZoom = false)
        {
            if (parkId > 0 && parkId < DistrictManager.instance.m_parks.m_size)
            {
                InstanceID instance = new InstanceID { Park = parkId };
                Vector3 oPosition = InstanceHelper.GetPosition(new InstanceID { Park = parkId });
                ToolsModifierControl.cameraController.SetTarget(instance, oPosition, bZoom);
            }
            else
            {
                Debug.Log("ParkId out of range: " + parkId);
            }
        }

        public static void ShowVehicle(ushort vehicleId)
        {
            if (vehicleId > 0 && vehicleId < VehicleManager.instance.m_vehicles.m_size)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    Vector3 oPosition = vehicle.GetLastFramePosition();
                    // We get a crash when trying to show a vehicle with invalid position
                    // Is this due to More Vehicles?
                    if (oPosition.ToString().Contains("NaN"))
                    {
                        Debug.Log("Ghost Vehicle: " + vehicleId + " has invalid position" + oPosition.ToString());
                        Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
                    }
                    else
                    {
                        InstanceID vehicleInstance = new InstanceID { Vehicle = vehicleId };
                        ToolsModifierControl.cameraController.SetTarget(vehicleInstance, oPosition, false);
                    }
                }
            }
            else
            {
                Debug.Log("VehicleId out of range: " + vehicleId);
            }
        }

        public static void ShowCitizen(uint CitizenId)
        {
            if (CitizenId > 0 && CitizenId < CitizenManager.instance.m_citizens.m_size)
            {
                Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[CitizenId];
                if (oCitizen.m_flags != 0)
                {
                    Vector3 oPosition = GetCitizenInstancePosition(oCitizen.m_instance);
                    if (oPosition == Vector3.zero)
                    {
                        ushort buildingId = oCitizen.GetBuildingByLocation();
                        if (buildingId != 0)
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                            oPosition = building.m_position;
                        }  
                    }

                    if (oPosition != Vector3.zero)
                    {
                        InstanceID instance = new InstanceID { Citizen = CitizenId };
                        ToolsModifierControl.cameraController.SetTarget(instance, oPosition, false);
                    }
                }
            }
            else
            {
                Debug.Log("CitizenId out of range: " + CitizenId);
            }
        }


        public static void ShowNode(ushort nodeId)
        {
            if (nodeId > 0 && nodeId < NetManager.instance.m_nodes.m_size)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeId];
                Vector3 oPosition = oNode.m_position;
                InstanceID instance = new InstanceID { NetNode = nodeId };
                ToolsModifierControl.cameraController.SetTarget(instance, oPosition, false);
            }
            else if (nodeId > 0)
            {
                Debug.Log("NodeId out of range: " + nodeId);
            }
        }

        public static void ShowSegment(ushort segmentId)
        {
            if (segmentId > 0 && segmentId < NetManager.instance.m_segments.m_size)
            {
                NetSegment oSegment = NetManager.instance.m_segments.m_buffer[segmentId];
                Vector3 oPosition = oSegment.m_middlePosition;
                InstanceID instance = new InstanceID { NetSegment = segmentId };
                ToolsModifierControl.cameraController.SetTarget(instance, oPosition, false);
            }
            else
            {
                Debug.Log("SegmentId out of range: " + segmentId);
            }
        }

        public static void ShowTransportLine(int iLineId)
        {
            if (iLineId > 0 && iLineId < TransportManager.instance.m_lines.m_size)
            {
                TransportLine line = TransportManager.instance.m_lines.m_buffer[iLineId];
                if (line.m_flags != 0 && line.m_stops != 0)
                {
                    // A line has nodes for each stop. Just show first stop.
                    ShowNode(line.m_stops);
                }
            }
        }

        public static void ShowPosition(Vector3 position)
        {
            ToolsModifierControl.cameraController.m_targetPosition = position;
        }

        public static int GetWarehouseTruckCount(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.Info != null)
                {
                    WarehouseAI? warehouse = building.Info.m_buildingAI as WarehouseAI;
                    if (warehouse != null)
                    {
                        // Factor in budget
                        int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                        int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                        return (productionRate * warehouse.m_truckCount + 99) / 100;
                    }
                    else if (building.Info?.m_buildingAI.GetType().ToString() == "CargoFerries.AI.CargoFerryWarehouseHarborAI")
                    {
                        return 25; // Just return default number for now
                    }
                }
            }

            return 0;
        }

        public static string GetDistrictName(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            byte district = DistrictManager.instance.GetDistrict(building.m_position);
            if (district != 0)
            {
                return DistrictManager.instance.GetDistrictName(district);
            }

            return "";
        }

        public static string GetParkName(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            byte park = DistrictManager.instance.GetPark(building.m_position);
            if (park != 0)
            {
                return DistrictManager.instance.GetParkName(park);
            }

            return "";
        }

        public static string GetDetectedDistricts(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            string sMessage = "";

            byte district = DistrictManager.instance.GetDistrict(building.m_position);
            if (district != 0)
            {
                sMessage += DistrictManager.instance.GetDistrictName(district);
            }

            byte park = DistrictManager.instance.GetPark(building.m_position);
            if (park != 0)
            {
                if (sMessage.Length > 0)
                {
                    sMessage += ", ";
                }
                sMessage += DistrictManager.instance.GetParkName(park);
            }

            return sMessage;
        }

        public static bool IsInDistrict(ushort buildingId)
        {
            byte district = 0;
            if (buildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                district = DistrictManager.instance.GetDistrict(inBuilding.m_position);
            }

            return district != 0;
        }

        public static bool IsInPark(ushort buildingId)
        {
            byte park = 0;
            if (buildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                park = DistrictManager.instance.GetPark(inBuilding.m_position);
            }

            return park != 0;
        }

        public static bool IsSameDistrict(ushort firstBuildingId, ushort secondBuildingId)
        {
            // get respective districts
            byte districtIncoming = 0;
            if (firstBuildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[firstBuildingId];
                districtIncoming = DistrictManager.instance.GetDistrict(inBuilding.m_position);
            }

            byte districtOutgoing = 0;
            if (secondBuildingId != 0)
            {
                Building outBuilding = BuildingManager.instance.m_buildings.m_buffer[secondBuildingId];
                districtOutgoing = DistrictManager.instance.GetDistrict(outBuilding.m_position);
            }

            return districtIncoming == districtOutgoing && districtIncoming != 0;
        }

        public static bool IsSamePark(ushort firstBuildingId, ushort secondBuildingId)
        {
            // get respective districts
            byte parkFirst = 0;
            if (firstBuildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[firstBuildingId];
                parkFirst = DistrictManager.instance.GetPark(inBuilding.m_position);
            }

            byte parkSecond = 0;
            if (secondBuildingId != 0)
            {
                Building outBuilding = BuildingManager.instance.m_buildings.m_buffer[secondBuildingId];
                parkSecond = DistrictManager.instance.GetPark(outBuilding.m_position);
            }

            return parkFirst == parkSecond && parkFirst != 0;
        }

        public static int CountImportExportVehicles(ushort buildingId, TransferReason material)
        {
            int iOutside = 0;

            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;
            BuildingManager buildingInstance = Singleton<BuildingManager>.instance;

            ref Building building = ref buildingInstance.m_buildings.m_buffer[buildingId];

            ushort num = building.m_ownVehicles;
            int iLoopCount = 0;
            while (num != 0)
            {
                Vehicle vehicle = vehicleInstance.m_vehicles.m_buffer[num];
                if ((TransferReason)vehicle.m_transferType == material)
                {
                    if ((vehicle.m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != 0)
                    {
                        iOutside++;
                    }
                }

                num = vehicle.m_nextOwnVehicle;
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iOutside;
        }

        public static int GetGuestVehiclesTransferSize(ushort buildingId, TransferReason material1, TransferReason material2 = TransferReason.None)
        {
            int iTransferSize = 0;

            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;
            BuildingManager buildingInstance = Singleton<BuildingManager>.instance;

            Building building = buildingInstance.m_buildings.m_buffer[buildingId];

            ushort num = building.m_guestVehicles;
            int iLoopCount = 0;
            while (num != 0)
            {
                Vehicle vehicle = vehicleInstance.m_vehicles.m_buffer[num];
                if ((TransferReason)vehicle.m_transferType == material1 || 
                    (material2 != TransferReason.None && (TransferReason)vehicle.m_transferType == material2))
                {
                    iTransferSize += vehicle.m_transferSize;
                }

                num = vehicle.m_nextGuestVehicle;
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iTransferSize;
        }

        public static string GetVehicleTransferValue(ushort vehicleId)
        {
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                
                // Show values for cargo parent if any
                if (vehicle.m_cargoParent != 0)
                {
                    vehicleId = vehicle.m_cargoParent;
                    vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                }

                if (vehicle.m_flags != 0)
                {
                    VehicleType eType = VehicleTypeHelper.GetVehicleType(vehicle);
                    switch (eType)
                    {
                        case VehicleType.BankVan:
                        case VehicleType.GarbageTruck:
                        case VehicleType.PostVan:
                        case VehicleType.CargoTruck:
                        case VehicleType.CargoTrain:
                        case VehicleType.CargoShip:
                        case VehicleType.CargoPlane:
                            {
                                int iCapacity;
                                int iCount = VehicleTypeHelper.GetBufferStatus(vehicleId, out iCapacity);
                                if (iCapacity > 0)
                                {
                                    return $"{Math.Round(((float)iCount / (float)iCapacity * 100.0), 0)}%";
                                }
                                else
                                {
                                    return "0%";
                                }
                            }
                        case VehicleType.CruiseShip:
                        case VehicleType.PassengerPlane:
                        case VehicleType.PassengerTrain:
                        case VehicleType.MetroTrain:
                        case VehicleType.Bus:
                            {
                                int iCapacity;
                                int iCount = VehicleTypeHelper.GetVehiclePassengerCount(vehicleId, out iCapacity);
                                return iCount + "/" + iCapacity;
                            }
                        default:
                            {
                                return vehicle.m_transferSize.ToString();
                            }
                    }
                }
            }

            return "";
        }

        public static string GetSafeLineName(int iLineId)
        {
            TransportLine oLine = TransportManager.instance.m_lines.m_buffer[iLineId];
            if ((oLine.m_flags & TransportLine.Flags.CustomName) == TransportLine.Flags.CustomName)
            {
                InstanceID oInstanceId = new InstanceID { TransportLine = (ushort)iLineId };
                return InstanceManager.instance.GetName(oInstanceId);
            }
            else
            {
                return oLine.Info.m_transportType + " Line " + oLine.m_lineNumber;
            }
        }


        public static int GetHomeCount(Building buildingData)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;

            int homeCount = 0;
            uint num = buildingData.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Home) != 0)
                {
                    homeCount++;
                }

                num = instance.m_units.m_buffer[num].m_nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return homeCount;
        }

        public static void CheckRoadAccess(TransferReason material, TransferOffer offer)
        {
            // Update access segment if using path distance but do it in simulation thread so we don't break anything
            if (offer.Building != 0 &&
                PathDistanceTypes.IsPathDistanceSupported(material) &&
                !BuildingTypeHelper.IsOutsideConnection(offer.Building))
            {
                ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                if (building.m_accessSegment == 0 && (building.m_problems & new Notification.ProblemStruct(Notification.Problem1.RoadNotConnected, Notification.Problem2.NotInPedestrianZone)).IsNone)
                {
                    // See if we can update m_accessSegment.
                    building.Info.m_buildingAI.CheckRoadAccess(offer.Building, ref building);
                    if (building.m_accessSegment == 0)
                    {
                        RoadAccessData.AddInstance(new InstanceID
                        {
                            Building = offer.Building
                        });
                    }
                }
            }
        }

        public static WarehouseMode GetWarehouseMode(ushort buildingId)
        {
            WarehouseMode mode = WarehouseMode.Unknown;

            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    if ((building.m_flags & global::Building.Flags.Filling) == global::Building.Flags.Filling)
                    {
                        mode = WarehouseMode.Fill;
                    }
                    else if ((building.m_flags & global::Building.Flags.Downgrading) == global::Building.Flags.Downgrading)
                    {
                        mode = WarehouseMode.Empty;
                    }
                    else
                    {
                        mode = WarehouseMode.Balanced;
                    }
                }
            }

            return mode;
        }
    }
}