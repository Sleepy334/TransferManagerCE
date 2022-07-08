using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class CitiesUtils
    {
        private static void AddCitizenToList(uint cim, ushort usBuildingId, Citizen.Flags flag, List<uint> cimList)
        {
            if (cim != 0)
            {
                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[cim];
                if ((citizen.m_flags & flag) == flag && citizen.GetBuildingByLocation() == usBuildingId)
                {
                    cimList.Add(cim);
                }
            }
        }

        public static List<uint> GetCitizens(ushort usBuildingId, Building building, Citizen.Flags flag)
        {
            List<uint> cimList = new List<uint>();

            uint uintCitizenUnit = building.m_citizenUnits;
            while (uintCitizenUnit != 0)
            {
                CitizenUnit citizenUnit = CitizenManager.instance.m_units.m_buffer[uintCitizenUnit];
                AddCitizenToList(citizenUnit.m_citizen0, usBuildingId, flag, cimList);
                AddCitizenToList(citizenUnit.m_citizen1, usBuildingId, flag, cimList);
                AddCitizenToList(citizenUnit.m_citizen2, usBuildingId, flag, cimList);
                AddCitizenToList(citizenUnit.m_citizen3, usBuildingId, flag, cimList);
                AddCitizenToList(citizenUnit.m_citizen4, usBuildingId, flag, cimList);

                uintCitizenUnit = citizenUnit.m_nextUnit;
            }

            return cimList;
        }

        public static List<uint> GetSickCitizens(ushort usBuildingId, Building building)
        {
            List<uint> cimSick = new List<uint>();
            if (building.m_healthProblemTimer > 0)
            {
                cimSick = GetCitizens(usBuildingId, building, Citizen.Flags.Sick);
            }

            return cimSick;
        }

        public static List<uint> GetDeadCitizens(ushort usBuildingId, Building building)
        {
            List<uint> cimDead = new List<uint>();
            if (building.m_deathProblemTimer > 0)
            {
                cimDead = GetCitizens(usBuildingId, building, Citizen.Flags.Dead);
            }

            return cimDead;
        }

        public static List<uint> GetCriminals(ushort usBuildingId, Building building)
        {
            return GetCitizens(usBuildingId, building, Citizen.Flags.Criminal); 
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
                list.Add(vehicleId);

                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
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
                list.Add(vehicleId);

                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                vehicleId = vehicle.m_nextOwnVehicle;
            }

            return list;
        }

        public static Vector3 GetVehiclePosition(Vehicle oVehicle)
        {
            switch (oVehicle.m_lastFrame)
            {
                case 0: return oVehicle.m_frame0.m_position;
                case 1: return oVehicle.m_frame1.m_position;
                case 2: return oVehicle.m_frame2.m_position;
                case 3: return oVehicle.m_frame3.m_position;
            }

            return oVehicle.m_frame0.m_position;
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
                        sName = "Building (" + buildingId + ") ";
                    }
#if DEBUG
                    sName = "(" + buildingId + ") " + sName;
#endif
                    return sName;
                } 
                else
                {
                    return "Abandoned (" + buildingId + ") ";
                }
            }
            return "";
        }

        public static string GetVehicleName(ushort vehicleId)
        {
            if (vehicleId > 0 && vehicleId < VehicleManager.instance.m_vehicles.m_size)
            {
#if DEBUG
                return "(" + vehicleId + ") " + Singleton<VehicleManager>.instance.GetVehicleName(vehicleId);
#else
                return Singleton<VehicleManager>.instance.GetVehicleName(vehicleId);
#endif
            }
            return "";
        }

        public static string GetCitizenName(uint citizenId)
        {
            if (citizenId != 0)
            {
#if DEBUG
                return "(" + citizenId + ") " + Singleton<CitizenManager>.instance.GetCitizenName(citizenId);
#else
                return Singleton<CitizenManager>.instance.GetCitizenName(citizenId);
#endif
            }
            return "";
        }

        public static Vector3 GetCitizenPosition(ushort CitizenId)
        {
            ref CitizenInstance cimInstance = ref CitizenManager.instance.m_instances.m_buffer[CitizenId];
            switch (cimInstance.m_lastFrame)
            {
                case 0: return cimInstance.m_frame0.m_position;
                case 1: return cimInstance.m_frame1.m_position;
                case 2: return cimInstance.m_frame2.m_position;
                case 3: return cimInstance.m_frame3.m_position;
            }

            return cimInstance.m_frame0.m_position;
        }

        public static void ShowBuilding(ushort buildingId)
        {
            if (buildingId > 0 && buildingId < BuildingManager.instance.m_buildings.m_size)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                Vector3 oPosition = building.m_position;
                InstanceID buildingInstance = new InstanceID { Building = buildingId };
                ToolsModifierControl.cameraController.SetTarget(buildingInstance, oPosition, false);
            }
        }

        public static void ShowVehicle(ushort vehicleId)
        {
            if (vehicleId > 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                Vector3 oPosition = GetVehiclePosition(vehicle);
                InstanceID vehicleInstance = new InstanceID { Vehicle = vehicleId };
                ToolsModifierControl.cameraController.SetTarget(vehicleInstance, oPosition, false);
            }
            
        }

        public static void ShowCitizen(uint CitizenId)
        {
            if (CitizenId > 0)
            {
                Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[CitizenId];
                Vector3 oPosition = CitiesUtils.GetCitizenPosition(oCitizen.m_instance);
                InstanceID buildingInstance = new InstanceID { Citizen = CitizenId };
                ToolsModifierControl.cameraController.SetTarget(buildingInstance, oPosition, false);
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
    }
}