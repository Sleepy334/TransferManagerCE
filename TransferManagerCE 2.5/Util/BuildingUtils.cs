using ColossalFramework;
using System;
using System.Collections.Generic;
using static TransferManager;
using UnityEngine;
using ColossalFramework.Math;

namespace TransferManagerCE
{
    public static class BuildingUtils
    {
        public delegate void BuildingDelegate(ushort buildingID, Building building);
        public delegate void VehicleDelegate(ushort vehicleID, Vehicle vehicle);

        public static void EnumerateGuestVehicles(Building building, VehicleDelegate func)
        {
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            uint uiSize = VehicleManager.instance.m_vehicles.m_size;

            ushort vehicleID = building.m_guestVehicles;
            int num = 0;
            while (vehicleID != 0 && vehicleID < uiSize)
            {
                Vehicle vehicle = Vehicles[vehicleID];

                // Call delegate for this vehicle
                func(vehicleID, vehicle);

                vehicleID = vehicle.m_nextGuestVehicle;

                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        public static void EnumerateOwnVehicles(Building building, VehicleDelegate func)
        {
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            uint uiSize = VehicleManager.instance.m_vehicles.m_size;

            ushort vehicleID = building.m_ownVehicles;
            int num = 0;
            while (vehicleID != 0 && vehicleID < uiSize)
            {
                Vehicle vehicle = Vehicles[vehicleID];

                // Call delegate for this vehicle
                func(vehicleID, vehicle);

                vehicleID = vehicle.m_nextOwnVehicle;

                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        public static void EnumerateNearbyBuildings(Vector3 pos, float maxDistance, BuildingDelegate func)
        {
            Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            ushort[] BuildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            uint numUnits = Singleton<BuildingManager>.instance.m_buildings.m_size;    //get number of building units

            Bounds bounds = new Bounds(pos, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int minx = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int minz = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int maxx = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int maxz = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);

            // Loop through every building grid within maximum distance
            for (int i = minz; i <= maxz; i++)
            {
                for (int j = minx; j <= maxx; j++)
                {
                    ushort currentBuilding = BuildingGrid[i * 270 + j];
                    int iLoopCount = 0;

                    // Iterate through all buildings at this grid location
                    while (currentBuilding != 0)
                    {
                        Building building = Buildings[currentBuilding];
                        if (building.m_flags != 0)
                        {
                            // Call delegate for this building
                            func(currentBuilding, building);
                        }

                        currentBuilding = building.m_nextGridBuilding;
                        if (++iLoopCount >= numUnits)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        public static void CalculateGuestVehicles(ushort buildingID, ref Building data, TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleID = data.m_guestVehicles;
            int num = 0;
            while (vehicleID != (ushort)0)
            {
                ref Vehicle vehicle = ref instance.m_vehicles.m_buffer[(int)vehicleID];
                if ((TransferReason)vehicle.m_transferType == material)
                {
                    int size;
                    int max;
                    vehicle.Info.m_vehicleAI.GetSize(vehicleID, ref vehicle, out size, out max);
                    cargo += Mathf.Min(size, max);
                    capacity += max;
                    ++count;
                    if ((vehicle.m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
                        ++outside;
                }
                vehicleID = vehicle.m_nextGuestVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        public static void CalculateOwnVehicles(ushort buildingID, ref Building data, TransferReason material, out int count, out int total)
        {
            count = 0;
            total = 0;

            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleID = data.m_ownVehicles;
            int num = 0;
            while (vehicleID != (ushort)0)
            {
                total++;

                ref Vehicle vehicle = ref instance.m_vehicles.m_buffer[(int)vehicleID];
                if ((TransferReason)vehicle.m_transferType == material)
                {
                    count++;
                }
                vehicleID = vehicle.m_nextOwnVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        public static int GetOwnVehicleCount(Building data)
        {
            int count = 0;

            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            ushort vehicleID = data.m_ownVehicles;
            int num = 0;
            while (vehicleID != (ushort)0)
            {
                ++count;

                ref Vehicle vehicle = ref Vehicles[(int)vehicleID];
               
                vehicleID = vehicle.m_nextOwnVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }

            return count;
        }

        public static List<uint> GetCitizensInBuilding(ushort usBuildingId, Building building, Citizen.Flags flags)
        {
            List<uint> cimList = new List<uint>();

            CitizenUtils.EnumerateCitizens(new InstanceID { Building = usBuildingId }, building.m_citizenUnits, (citizendId, citizen) =>
            {
                if (citizen.GetBuildingByLocation() == usBuildingId && (citizen.m_flags & flags) == flags)
                {
                    cimList.Add(citizendId);
                }
                return true; // continue loop
            });

            return cimList;
        }

        public static List<uint> GetSick(ushort usBuildingId, Building building)
        {
            return GetCitizensInBuilding(usBuildingId, building, Citizen.Flags.Sick);
        }

        public static int GetCitizenCountInBuilding(ushort usBuildingId, Building building, Citizen.Flags flags)
        {
            int iTotal = 0;

            CitizenUtils.EnumerateCitizens(new InstanceID { Building = usBuildingId }, building.m_citizenUnits, (citizenId, citizen) =>
            {
                if (citizen.GetBuildingByLocation() == usBuildingId && (citizen.m_flags & flags) == flags)
                {
                    iTotal++;
                }
                // continue loop
                return true;
            });

            return iTotal;
        }

        public static int GetDeadCount(ushort usBuildingId, Building building)
        {
            return GetCitizenCountInBuilding(usBuildingId, building, Citizen.Flags.Dead);
        }

        public static int GetSickCount(ushort usBuildingId, Building building)
        {
            return GetCitizenCountInBuilding(usBuildingId, building, Citizen.Flags.Sick);
        }

        public static int GetCriminalCount(ushort usBuildingId, Building building)
        {
            return GetCitizenCountInBuilding(usBuildingId, building, Citizen.Flags.Criminal);
        }

        public static int GetCriminalsAtPoliceStation(ushort buildingId, Building building)
        {
            int iTotal = 0;

            CitizenUtils.EnumerateCitizens(new InstanceID { Building = buildingId }, building.m_citizenUnits, (citizenId, citizen) =>
            {
                if (citizen.m_flags != 0 &&
                    citizen.GetBuildingByLocation() == buildingId &&
                    citizen.CurrentLocation == Citizen.Location.Visit)
                {
                    iTotal++;
                }
                // continue loop
                return true;
            });

            return iTotal;
        }

        public static List<ushort> GetGuestParentVehiclesForBuilding(Building building)
        {
            List<ushort> list = new List<ushort>();

            EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
            {
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
            });

            return list;
        }

        public static int GetGuestVehicleCount(Building building, TransferReason material1, TransferReason material2 = TransferReason.None)
        {
            int iVehicles = 0;

            EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
            {
                if ((TransferReason)vehicle.m_transferType == material1 ||
                    (material2 != TransferReason.None && (TransferReason)vehicle.m_transferType == material2))
                {
                    iVehicles++;
                }
            });

            return iVehicles;
        }

        public static int GetOwnVehicleCount(Building building, TransferReason material1, TransferReason material2 = TransferReason.None)
        {
            int iVehicles = 0;

            EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
            {
                if ((TransferReason)vehicle.m_transferType == material1 ||
                    (material2 != TransferReason.None && (TransferReason)vehicle.m_transferType == material2))
                {
                    iVehicles++;
                }
            });

            return iVehicles;
        }

        public static List<ushort> GetGuestParentVehiclesForBuilding(ushort buildingId, out int iStuck)
        {
            List<ushort> list = new List<ushort>();
            int iTempStuck = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                {
                    if (vehicle.m_flags != 0)
                    {
                        if (vehicle.m_cargoParent == 0)
                        {
                            list.Add(vehicleId);

                            // Check if it is stuck
                            if (vehicle.m_waitCounter >= 255)
                            {
                                iTempStuck++;
                            }
                        }
                        else if (!list.Contains(vehicle.m_cargoParent))
                        {
                            list.Add(vehicle.m_cargoParent);

                            // Check if it is stuck
                            Vehicle cargoParent = VehicleManager.instance.m_vehicles.m_buffer[vehicle.m_cargoParent];
                            if (cargoParent.m_flags == 0 || cargoParent.m_waitCounter >= 255)
                            {
                                iTempStuck++;
                            }
                        }
                    }
                    else
                    {
                        iTempStuck++;
                    }
                });
            }

            iStuck = iTempStuck;
            return list;
        }

        public static List<ushort> GetOwnParentVehiclesForBuilding(ushort buildingId, out int iStuck)
        {
            List<ushort> list = new List<ushort>();
            int iTempStuck = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                {
                    if (vehicle.m_flags != 0)
                    {
                        if (vehicle.m_cargoParent == 0)
                        {
                            list.Add(vehicleId);

                            // Check if it is stuck
                            if (vehicle.m_waitCounter >= 255)
                            {
                                iTempStuck++;
                            }
                        }
                        else if (!list.Contains(vehicle.m_cargoParent))
                        {
                            list.Add(vehicle.m_cargoParent);

                            // Check if it is stuck
                            Vehicle cargoParent = VehicleManager.instance.m_vehicles.m_buffer[vehicle.m_cargoParent];
                            if (cargoParent.m_flags == 0 || cargoParent.m_waitCounter >= 255)
                            {
                                iTempStuck++;
                            }
                        }
                    }
                    else
                    {
                        iTempStuck++;
                    }
                });
            }

            iStuck = iTempStuck;
            return list;
        }

        public static int CountImportExportVehicles(ushort buildingId, CustomTransferReason.Reason material)
        {
            int iOutside = 0;

            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                {
                    if ((CustomTransferReason.Reason)vehicle.m_transferType == material)
                    {
                        if ((vehicle.m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != 0)
                        {
                            iOutside++;
                        }
                    }
                });
            }

            return iOutside;
        }

        public static int GetGuestVehiclesTransferSize(ushort buildingId, TransferReason material1, TransferReason material2 = TransferReason.None)
        {
            int iTransferSize = 0;

            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                {
                    if (vehicle.m_flags != 0 &&
                        (TransferReason)vehicle.m_transferType == material1 || (material2 != TransferReason.None && (TransferReason)vehicle.m_transferType == material2))
                    {
                        iTransferSize += vehicle.m_transferSize;
                    }
                });
            }

            return iTransferSize;
        }
    }
}
