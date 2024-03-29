using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.VehicleTypeHelper;

namespace TransferManagerCE
{
    public class CitiesUtils
    {
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

            uint uintCitizenUnit = building.m_citizenUnits;
            while (uintCitizenUnit != 0)
            {
                CitizenUnit citizenUnit = CitizenManager.instance.m_units.m_buffer[uintCitizenUnit];
                for (int i = 0; i < 5; ++i)
                {
                    AddCitizenToList(citizenUnit.GetCitizen(i), usBuildingId, flags, cimList);
                }
                
                uintCitizenUnit = citizenUnit.m_nextUnit;
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
                    (vehicle.Info != null && (vehicle.Info.m_vehicleAI is CargoTruckAI) &&
                    ((TransferReason)vehicle.m_transferType == TransferReason.Goods || (TransferReason)vehicle.m_transferType == TransferManager.TransferReason.Food)) &&
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

            ushort vehicleId = building.m_guestVehicles;
            while (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    list.Add(vehicleId);
                }
                vehicleId = vehicle.m_nextGuestVehicle;
            }

            return list;
        }

        public static List<ushort> GetGuestParentVehiclesForBuilding(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buidlingId];

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
            }

            return list;
        }

        public static List<ushort> GetOwnVehiclesForBuilding(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buidlingId];

            ushort vehicleId = building.m_ownVehicles;
            while (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId]; 
                if (vehicle.m_flags != 0)
                {
                    list.Add(vehicleId);
                }
                vehicleId = vehicle.m_nextOwnVehicle;
            }

            return list;
        }

        public static List<ushort> GetOwnParentVehiclesForBuilding(ushort buidlingId)
        {
            List<ushort> list = new List<ushort>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[buidlingId];

            ushort vehicleId = building.m_ownVehicles;
            while (vehicleId != 0)
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
            }

            return list;
        }

        public static string GetBuildingName(ushort buildingId)
        {
            if (buildingId != 0 && buildingId < BuildingManager.instance.m_buildings.m_size)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != Building.Flags.None)
                {
                    string sName = Singleton<BuildingManager>.instance.GetBuildingName(buildingId, InstanceID.Empty);
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
                    return building.Info.name + ":" + buildingId;
                }
            }
            return "";
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
        }

        public static void ShowCitizen(uint CitizenId)
        {
            if (CitizenId > 0)
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
        }

        public static void ShowNode(ushort nodeId)
        {
            if (nodeId > 0)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeId];
                Vector3 oPosition = oNode.m_position;
                InstanceID instance = new InstanceID { NetNode = nodeId };
                ToolsModifierControl.cameraController.SetTarget(instance, oPosition, false);
            }
        }

        public static void ShowSegment(ushort segmentId)
        {
            if (segmentId > 0)
            {
                NetSegment oSegment = NetManager.instance.m_segments.m_buffer[segmentId];
                Vector3 oPosition = oSegment.m_middlePosition;
                InstanceID instance = new InstanceID { NetSegment = segmentId };
                ToolsModifierControl.cameraController.SetTarget(instance, oPosition, false);
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
                WarehouseAI? warehouse = building.Info?.m_buildingAI as WarehouseAI;
                if (warehouse != null)
                {
                    return warehouse.m_truckCount;
                }
                else if (building.Info?.m_buildingAI.GetType().ToString() == "CargoFerries.AI.CargoFerryWarehouseHarborAI")
                {
                    return 25; // Just return default number for now
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
            byte district = DistrictManager.instance.GetDistrict(building.m_position);
            byte park = DistrictManager.instance.GetPark(building.m_position);

            string sMessage = "";
            if (district != 0)
            {
                sMessage += DistrictManager.instance.GetDistrictName(district);
            }
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

        public static void ReleaseGhostVehicles()
        {
            string sMessage = "";

            List<ushort> ghostVehicles = new List<ushort>();
            for (int i = 0; i < VehicleManager.instance.m_vehicles.m_buffer.Length; i++)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[i];
                if (vehicle.m_flags != 0)
                {
                    Vector3 oPosition = vehicle.GetLastFramePosition();
                    if (oPosition.ToString().Contains("NaN") ||
                        Math.Abs(oPosition.x) > 100000 ||
                        Math.Abs(oPosition.y) > 100000 ||
                        Math.Abs(oPosition.z) > 100000)
                    {
                        sMessage += "\r\nVehicle: " + i + " has invalid position " + oPosition.ToString() + " Flags: " + vehicle.m_flags.ToString();

                        // Despawn vehicle so a new one can be created
                        if (!ghostVehicles.Contains((ushort)i)) {
                            ghostVehicles.Add((ushort)i);
                        }
                    }

                    if (vehicle.m_cargoParent != 0)
                    {
                        Vehicle parent = VehicleManager.instance.m_vehicles.m_buffer[vehicle.m_cargoParent];
                        if (parent.m_flags == 0)
                        {
                            sMessage += "\r\nVehicle: " + i + " has invalid cargo parent (" + vehicle.m_cargoParent + " Flags: " + parent.m_flags + ")";

                            // Despawn vehicle so a new one can be created
                            if (!ghostVehicles.Contains((ushort)i))
                            {
                                ghostVehicles.Add((ushort)i);
                            }
                        } 
                        else if (parent.m_flags == Vehicle.Flags.WaitingPath)
                        {
                            sMessage += "\r\nVehicle: " + i + " has invalid cargo parent (" + vehicle.m_cargoParent + " Flags: " + parent.m_flags + ")";

                            // Despawn vehicle so a new one can be created
                            if (!ghostVehicles.Contains((ushort)i)) {
                                ghostVehicles.Add((ushort)i);
                            }
                            if (!ghostVehicles.Contains(vehicle.m_cargoParent)) {
                                ghostVehicles.Add(vehicle.m_cargoParent);
                            }
                        }
                    }
                }
            }

            // Perform actual de-spawning
            foreach (ushort vehicleId in ghostVehicles) 
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
                }
            }

            if (sMessage.Length > 0)
            {
                Debug.Log("Ghost vehicles: " + sMessage);
            }
        }

        public static int CountImportExportVehicles(ushort buildingId, TransferReason material)
        {
            int iOutside = 0;

            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;
            BuildingManager buildingInstance = Singleton<BuildingManager>.instance;

            ref Building building = ref buildingInstance.m_buildings.m_buffer[buildingId];

            ushort num = building.m_ownVehicles;
            int num2 = 0;
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
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iOutside;
        }

        public static int GetGuestVehiclesTransferSize(ushort buildingId, TransferReason material)
        {
            int iTransferSize = 0;

            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;
            BuildingManager buildingInstance = Singleton<BuildingManager>.instance;

            ref Building building = ref buildingInstance.m_buildings.m_buffer[buildingId];

            ushort num = building.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                Vehicle vehicle = vehicleInstance.m_vehicles.m_buffer[num];
                if ((TransferReason)vehicle.m_transferType == material)
                {
                    iTransferSize += vehicle.m_transferSize;
                }

                num = vehicle.m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iTransferSize;
        }

        public static int GetCargoVehicleTransferSize(ushort vehicleId, Vehicle vehicle)
        {
            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;

            int iTransferSize = 0;
            ushort cargoId = vehicle.m_firstCargo;
            int num2 = 0;

            while (cargoId != 0)
            {
                Vehicle subVehicle = vehicleInstance.m_vehicles.m_buffer[cargoId];
                iTransferSize += subVehicle.m_transferSize;

                cargoId = subVehicle.m_nextCargo;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return (int) Math.Round((double)iTransferSize * 0.001);
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
                        case VehicleType.PostVan:
                        case VehicleType.CargoTruck:
                            {
                                return (vehicle.m_transferSize * 0.001).ToString("N0");
                            }
                        case VehicleType.CargoTrain:
                        case VehicleType.CargoShip:
                        case VehicleType.CargoPlane:
                            {
                                return vehicle.m_transferSize + ":" + CitiesUtils.GetCargoVehicleTransferSize(vehicleId, vehicle);
                            }
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
    }
}