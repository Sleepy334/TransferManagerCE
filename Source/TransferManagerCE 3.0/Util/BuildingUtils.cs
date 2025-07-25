﻿using ColossalFramework;
using System;
using System.Collections.Generic;
using static TransferManager;
using UnityEngine;
using ColossalFramework.Math;
using System.Reflection;
using static Notification;
using TransferManagerCE.UI;
using SleepyCommon;
using static RenderManager;

namespace TransferManagerCE
{
    public static class BuildingUtils
    {
        public delegate bool BuildingDelegate(ushort buildingID, Building building);
        public delegate bool VehicleDelegate(ushort vehicleID, Vehicle vehicle);

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
                if (!func(vehicleID, vehicle))
                {
                    break;
                }

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
                if (!func(vehicleID, vehicle))
                {
                    break;
                }

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

            int xstart = Mathf.Clamp((int)(pos.x / 64f + 135f), 0, 269);
            int zstart = Mathf.Clamp((int)(pos.z / 64f + 135f), 0, 269);

            Bounds bounds = new Bounds(pos, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int minx = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int minz = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int maxx = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int maxz = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);

            // We loop inside out as the buildings will be closer to us in the middle of the grid search
            foreach (int i in UpDownCount(zstart, minz, maxz))
            {
                foreach (int j in UpDownCount(xstart, minx, maxx))
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
                            if (!func(currentBuilding, building))
                            {
                                break;
                            }
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

        private static IEnumerable<int> UpDownCount(int start, int lower, int upper)
        {
            int i = start, j = start;
            yield return start;

            while (true)
            {
                bool ilimit = ++i <= upper;
                bool jlimit = --j >= lower;

                if (ilimit)
                {
                    yield return i;
                }
                    
                if (jlimit)
                {
                    yield return j;
                }
                    
                if (!(ilimit || jlimit))
                {
                    yield break;
                }
            }
        }

        public static bool HasAnyCloserGuestVehicles(ushort vehicleId, Vector3 pos, ushort buildingID, Building data, HashSet<CustomTransferReason.Reason> materials)
        {
            double distanceSqr1 = VectorUtils.LengthSqrXZ(pos - data.m_position);

            VehicleManager instance = Singleton<VehicleManager>.instance;

            ushort vehicleID = data.m_guestVehicles;
            int num = 0;
            while (vehicleID != (ushort)0)
            {
                Vehicle vehicle = instance.m_vehicles.m_buffer[(int)vehicleID];

                if (vehicleID != (ushort)vehicleId) 
                {
                    // Check the vehicle is the right type
                    if (materials.Contains((CustomTransferReason.Reason) vehicle.m_transferType))
                    {
                        if ((vehicle.m_flags & Vehicle.Flags.Stopped) != 0)
                        {
                            return true; // Already there
                        }
                        else
                        {
                            double distanceSqr2 = VectorUtils.LengthSqrXZ(vehicle.GetLastFramePosition() - data.m_position);
                            if (distanceSqr2 < distanceSqr1)
                            {
                                // This vehicle is closer
                                return true;
                            }
                        }
                    }
                }

                vehicleID = vehicle.m_nextGuestVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }

            return false;
        }

        public static bool HasAnyGuestVehicles(ushort buildingID, Building data, HashSet<CustomTransferReason.Reason> materials)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;

            ushort vehicleID = data.m_guestVehicles;
            int num = 0;
            while (vehicleID != (ushort)0)
            {
                Vehicle vehicle = instance.m_vehicles.m_buffer[(int)vehicleID];
                if (materials.Contains((CustomTransferReason.Reason)vehicle.m_transferType))
                {
                    return true;
                }

                vehicleID = vehicle.m_nextGuestVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }

            return false;
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

        public static List<uint> GetSickWithoutVehicles(ushort usBuildingId, Building building)
        {
            Vehicle[] vehicles = VehicleManager.instance.m_vehicles.m_buffer;

            List<uint> cimList = new List<uint>();
            CitizenUtils.EnumerateCitizens(new InstanceID { Building = usBuildingId }, building.m_citizenUnits, (citizendId, citizen) =>
            {
                // Check if this citizen has an ambulance on the way. It's assigned to the citizen as it's vehicle.
                if (citizen.Sick && citizen.GetBuildingByLocation() == usBuildingId && citizen.m_vehicle == 0)
                {
                    // No assigned vehicle add cim
                    cimList.Add(citizendId);
                }

                return true; // continue loop
            });

            return cimList;
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

        public static int GetCitizenCount(ushort usBuildingId, Building building)
        {
            Citizen[] Citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer; 
            CitizenUnit[] CitizenUnits = Singleton<CitizenManager>.instance.m_units.m_buffer;
            const int iMaxLength = 524288; // Used in assembly eg HandleDead.

            uint citizenUnitId = building.m_citizenUnits;

            int iTotal = 0;
            int iLoopCount = 0;
            while (citizenUnitId != 0)
            {
                CitizenUnit citizenUnit = CitizenUnits[citizenUnitId];
                for (int i = 0; i < 5; ++i)
                {
                    uint cim = citizenUnit.GetCitizen(i);
                    if (cim != 0)
                    {
                        Citizen citizen = Citizens[cim];

                        // Call delegate for citizen
                        if (!citizen.Dead)
                        {
                            iTotal++;
                        }
                    }
                }

                citizenUnitId = citizenUnit.m_nextUnit;

                // Check for bad list
                if (++iLoopCount > iMaxLength)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, $"Invalid list detected - BuildingId: {usBuildingId} m_citizenUnits." + Environment.StackTrace);
                    break;
                }
            }
            
            return iTotal;
        }

        public static int GetCitizenCount(ushort usBuildingId, Building building, out int iInBuildingCount)
        {
            int iTotal = 0;
            int iInBuilding = 0;

            CitizenUtils.EnumerateCitizens(new InstanceID { Building = usBuildingId }, building.m_citizenUnits, (citizenId, citizen) =>
            {
                if (!citizen.Dead)
                {
                    iTotal++;

                    if (citizen.GetBuildingByLocation() == usBuildingId)
                    {
                        iInBuilding++;
                    }
                }

                return true; // continue loop
            });

            iInBuildingCount = iInBuilding;
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
                return true;
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
                return true;
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
                return true;
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
                    return true;
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
                    return true;
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
                    return true;
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
                    return true;
                });
            }

            return iTransferSize;
        }

        public static int GetVisitorCount(CommonBuildingAI buildingAI, ushort buildingId, Building building)
        {
            if (buildingAI is not null)
            {
                MethodInfo? methodGetVisitBehaviour = buildingAI.GetType().GetMethod("GetVisitBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodGetVisitBehaviour != null)
                {
                    // Get visiter number
                    Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                    int aliveCount = 0;
                    int iTotalCount = 0;
                    object[] args = new object[] { buildingId, building, behaviour, aliveCount, iTotalCount };
                    methodGetVisitBehaviour.Invoke(buildingAI, args);
                    return (int)args[3];
                }
            }

            return 0;
        }

        public static int GetTotalVisitPlaceCount(ushort buildingId, Building building)
        {
            int totalVisitors = 0;

            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case LibraryAI libraryAI:
                        {
                            totalVisitors = libraryAI.VisitorCount;
                            break;
                        }
                    case PlayerBuildingAI:
                        {
                            int visitorCount0 = 0;
                            int visitorCount1 = 0;
                            int visitorCount2 = 0;

                            PlayerBuildingAI buildingAI = building.Info.GetAI() as PlayerBuildingAI;

                            // Get visitor places available
                            Type buildingType = buildingAI.GetType();

                            FieldInfo? field0 = buildingType.GetField("m_visitPlaceCount0");
                            if (field0 is not null)
                            {
                                visitorCount0 = (int)field0.GetValue(buildingAI);
                            }
                            FieldInfo? field1 = buildingType.GetField("m_visitPlaceCount1");
                            if (field1 is not null)
                            {
                                visitorCount1 = (int)field1.GetValue(buildingAI);
                            }
                            FieldInfo? field2 = buildingType.GetField("m_visitPlaceCount2");
                            if (field2 is not null)
                            {
                                visitorCount2 = (int)field2.GetValue(buildingAI);
                            }

                            // Factor in budget
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);

                            visitorCount0 = (productionRate * visitorCount0 + 99) / 100;
                            visitorCount1 = (productionRate * visitorCount1 + 99) / 100;
                            visitorCount2 = (productionRate * visitorCount2 + 99) / 100;

                            totalVisitors = visitorCount0 + visitorCount1 + visitorCount2;
                            break;
                        }
                }
            }

            return totalVisitors;
        }

        public static int GetTotalWorkerPlaces(ushort buildingId, Building building, out int workPlaceCount0, out int workPlaceCount1, out int workPlaceCount2, out int workPlaceCount3)
        {
            workPlaceCount0 = 0;
            workPlaceCount1 = 0;
            workPlaceCount2 = 0;
            workPlaceCount3 = 0;

            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case ParkGateAI:
                    case ParkBuildingAI:
                        {
                            PlayerBuildingAI buildingAI = building.Info.GetAI() as PlayerBuildingAI;
                            Type buildingType = buildingAI.GetType();

                            MethodInfo? methodTargetWorkers = buildingType.GetMethod("TargetWorkers", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (methodTargetWorkers is not null)
                            {
                                workPlaceCount0 = (int)methodTargetWorkers.Invoke(buildingAI, new object[] { buildingId, building });
                            }

                            break;
                        }
                    case PlayerBuildingAI:
                        {
                            PlayerBuildingAI buildingAI = building.Info.GetAI() as PlayerBuildingAI;

                            // Get worker count
                            Type buildingType = buildingAI.GetType();

                            FieldInfo? field0 = buildingType.GetField("m_workPlaceCount0");
                            if (field0 is not null)
                            {
                                workPlaceCount0 = (int)field0.GetValue(buildingAI);
                            }
                            FieldInfo? field1 = buildingType.GetField("m_workPlaceCount1");
                            if (field1 is not null)
                            {
                                workPlaceCount1 = (int)field1.GetValue(buildingAI);
                            }
                            FieldInfo? field2 = buildingType.GetField("m_workPlaceCount2");
                            if (field2 is not null)
                            {
                                workPlaceCount2 = (int)field2.GetValue(buildingAI);
                            }
                            FieldInfo? field3 = buildingType.GetField("m_workPlaceCount3");
                            if (field3 is not null)
                            {
                                workPlaceCount3 = (int)field3.GetValue(buildingAI);
                            }

                            break;
                        }
                    case PrivateBuildingAI:
                        {
                            PrivateBuildingAI buildingAI = building.Info.GetAI() as PrivateBuildingAI;

                            buildingAI.CalculateWorkplaceCount((ItemClass.Level)building.m_level, new Randomizer(buildingId), building.Width, building.Length, out var level1, out var level2, out var level3, out var level4);
                            buildingAI.AdjustWorkplaceCount((ushort)buildingId, ref building, ref level1, ref level2, ref level3, ref level4);

                            workPlaceCount0 = level1;
                            workPlaceCount1 = level2;
                            workPlaceCount2 = level3;
                            workPlaceCount3 = level4;
                            break;
                        }
                }
            }

            return workPlaceCount0 + workPlaceCount1 + workPlaceCount2 + workPlaceCount3;
        }

        public static int GetCurrentWorkerCount(ushort buildingId, Building buildingData, out int worker0, out int worker1, out int worker2, out int worker3)
        {
            int totalCount = 0;
            worker0 = 0;
            worker1 = 0;
            worker2 = 0;
            worker3 = 0;

            if (buildingData.Info is not null)
            {
                switch (buildingData.Info.GetAI())
                {
                    case ParkGateAI:
                    case ParkBuildingAI:
                        {
                            PlayerBuildingAI buildingAI = buildingData.Info.GetAI() as PlayerBuildingAI;
                            Type buildingType = buildingAI.GetType();

                            MethodInfo? methodCountWorkers = buildingType.GetMethod("CountWorkers", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (methodCountWorkers is not null)
                            {
                                totalCount = (int)methodCountWorkers.Invoke(buildingAI, new object[] { buildingId, buildingData });
                                worker0 = totalCount;
                            }

                            break;
                        }
                    default:
                        {
                            Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                            int aliveCount = 0;

                            CitizenManager instance = Singleton<CitizenManager>.instance;
                            uint num = buildingData.m_citizenUnits;
                            int num2 = 0;
                            while (num != 0)
                            {
                                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Work) != 0)
                                {
                                    instance.m_units.m_buffer[num].GetCitizenWorkBehaviour(ref behaviour, ref aliveCount, ref totalCount);
                                }
                                num = instance.m_units.m_buffer[num].m_nextUnit;
                                if (++num2 > 524288)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }

                            worker0 = behaviour.m_educated0Count;
                            worker1 = behaviour.m_educated1Count;
                            worker2 = behaviour.m_educated2Count;
                            worker3 = behaviour.m_educated3Count;

                            break;
                        }
                }
                
            }

            return totalCount;
        }

        public static bool IsSickOnlyMajorProblem(ProblemStruct problems)
        {
            // Remove all the Sick problems
            problems = RemoveProblems(problems, Problem1.DirtyWater | Notification.Problem1.Pollution | Problem1.Noise);

            // Check there is nothing else set
            return ((problems & ProblemStruct.Mask).IsNone);
        }

        public static int GetChildCount(ushort buildingID, Building data)
        {
            return GetCitizenAgedCount(buildingID, data, Citizen.AgeGroup.Child);
        }

        public static int GetSeniorCount(ushort buildingID, Building data)
        {
            return GetCitizenAgedCount(buildingID, data, Citizen.AgeGroup.Senior);
        }

        public static int GetCitizenAgedCount(ushort buildingID, Building data, Citizen.AgeGroup groupType)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int iLoopCount = 0;
            int iCitizenCount = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Visit) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                        if (citizen != 0 && instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit && 
                            Citizen.GetAgeGroup(instance.m_citizens.m_buffer[citizen].Age) == groupType)
                        {
                            iCitizenCount++;
                        }
                    }
                }
                num = nextUnit;
                if (++iLoopCount > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iCitizenCount;
        }

        public static void HandleWorkAndVisitPlaces(PlayerBuildingAI buildingAI, ushort buildingId, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            MethodInfo? methodHandleWorkAndVisitPlaces = buildingAI.GetType().GetMethod("HandleWorkAndVisitPlaces", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodHandleWorkAndVisitPlaces is not null)
            {
                // Call function and
                object[] args = new object[] { buildingId, buildingData, behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount };
                methodHandleWorkAndVisitPlaces.Invoke(buildingAI, args);

                //  update values
                behaviour = (Citizen.BehaviourData)args[2];
                aliveWorkerCount = (int)args[3];
                totalWorkerCount = (int)args[4];
                workPlaceCount = (int)args[5];
                aliveVisitorCount = (int)args[6];
                totalVisitorCount = (int)args[7];
                visitPlaceCount = (int)args[8];
            }
        }

        public static ushort GetSelectedBuilding()
        {
            InstanceID selectedId = InstanceHelper.GetTargetInstance();
            if (selectedId.Building != 0)
            {
                return selectedId.Building;
            }
            else if (BuildingPanel.IsVisible())
            {
                return BuildingPanel.Instance.Building;
            }

            return 0;
        }

        public static void ShowInstanceSetBuildingPanel(InstanceID instance)
        {
            InstanceHelper.ShowInstance(instance);

            if (instance.Building != 0)
            {
                // Activate panel, if Left CTRL is held down then open panel.
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    // Force display of panel
                    BuildingPanel.Instance.ShowPanel(instance.Building);
                }
                else if (BuildingPanel.IsVisible())
                {
                    BuildingPanel.Instance.ShowPanel(instance.Building);
                }
            }
        }

        public static void GetBuildingSubBuildings(ushort buildingId, List<ushort> subBuildings)
        {
            subBuildings.Clear();

            // Update sub buildings (if any)
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                int iLoopCount = 0;
                ushort subBuildingId = building.m_subBuilding;
                while (subBuildingId != 0)
                {
                    Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                    if (subBuilding.m_flags != 0)
                    {
                        subBuildings.Add(subBuildingId);
                    }

                    // Setup for next sub building
                    subBuildingId = subBuilding.m_subBuilding;

                    if (++iLoopCount > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
            }
        }

        public static bool IsBuildingTurnedOff(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return (building.m_problems & Notification.Problem1.TurnedOff).IsNotNone;
        }
    }
}

