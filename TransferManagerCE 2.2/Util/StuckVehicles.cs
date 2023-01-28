using ColossalFramework;
using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE;
using UnityEngine;

namespace TransferManagerCE {
    public class StuckVehicles
    {
        private static Dictionary<uint, int> s_stuckVehicles = new Dictionary<uint, int>();

        public static void CheckStuckVehicles()
        {
            string sMessage = "CheckStuckVehicles: Resetting vehicles that are waiting for a path.";

            bool wasPausedBeforeReset = Singleton<SimulationManager>.instance.ForcedSimulationPaused;
            if (!wasPausedBeforeReset)
            {
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = true;
            }

            Singleton<PathManager>.instance.WaitForAllPaths();

            VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
            if (s_stuckVehicles != null)
            {
                for (uint vehicleId = 1; vehicleId < vehicleManager.m_vehicles.m_buffer.Length; ++vehicleId)
                {
                    ref Vehicle vehicle = ref vehicleManager.m_vehicles.m_buffer[vehicleId];
                    if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) == Vehicle.Flags.WaitingPath && vehicle.m_path != 0u)
                    {
                        int iWaitingCount;
                        if (s_stuckVehicles.ContainsKey(vehicleId))
                        {
                            iWaitingCount = s_stuckVehicles[vehicleId] + 1;
                            s_stuckVehicles[vehicleId] = iWaitingCount;
                        }
                        else
                        {
                            iWaitingCount = 1;
                            s_stuckVehicles[vehicleId] = 1;
                        }

                        // We have a vehicle with a waiting for path flag and an actual path, may be stuck
                        sMessage += $"\r\nStuck vehicle {vehicleId} WaitingCount: {iWaitingCount} Flags: {vehicle.m_flags}";

                        if (iWaitingCount >= 10)
                        {
                            Debug.Log("Release vehicle: " + vehicleId);
                            Singleton<VehicleManager>.instance.ReleaseVehicle((ushort)vehicleId);
                        }
                    }
                    else
                    {
                        s_stuckVehicles.Remove(vehicleId);
                    }
                }

                if (s_stuckVehicles.Count > 0)
                {
                    Debug.Log(sMessage);
                }
            }

            if (!wasPausedBeforeReset)
            {
                //Debug.Log("UtilityManager.RemoveStuckEntities(): Unpausing simulation.");
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = false;
            }
        }

        public static int ReleaseGhostVehicles()
        {
            bool wasPausedBeforeReset = Singleton<SimulationManager>.instance.ForcedSimulationPaused;
            if (!wasPausedBeforeReset)
            {
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = true;
            }

            string sMessage = "";

            List<ushort> ghostVehicles = new List<ushort>();
            for (int i = 0; i < VehicleManager.instance.m_vehicles.m_buffer.Length; i++)
            {
                ushort vehicleId = (ushort)i;
                ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    Vector3 oPosition = vehicle.GetLastFramePosition();
                    if (oPosition.ToString().Contains("NaN") ||
                        Math.Abs(oPosition.x) > 100000 ||
                        Math.Abs(oPosition.y) > 100000 ||
                        Math.Abs(oPosition.z) > 100000)
                    {
                        sMessage += $"\r\nVehicle:{i} Flags: {vehicle.m_flags} Position {oPosition}";

                        // Despawn vehicle so a new one can be created
                        if (!ghostVehicles.Contains(vehicleId))
                        {
                            ghostVehicles.Add(vehicleId);
                        }
                    }

                    // If m_flags has WaitingPath but not Created then it is broken
                    if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) != 0 && (vehicle.m_flags & Vehicle.Flags.Created) == 0)
                    {
                        sMessage += $"\r\nVehicle:{i} Flags:{vehicle.m_flags}";

                        // Despawn vehicle so a new one can be created
                        if (!ghostVehicles.Contains(vehicleId))
                        {
                            ghostVehicles.Add(vehicleId);
                        }
                    }
                    else if (vehicle.m_cargoParent != 0)
                    {
                        Vehicle parent = VehicleManager.instance.m_vehicles.m_buffer[vehicle.m_cargoParent];
                        if (parent.m_flags == 0)
                        {
                            sMessage += $"\r\nVehicle:{i} Flags:{vehicle.m_flags} CargoParent:{vehicle.m_cargoParent} ParentFlags:{parent.m_flags}";

                            // Despawn vehicle so a new one can be created
                            if (!ghostVehicles.Contains(vehicleId))
                            {
                                ghostVehicles.Add(vehicleId);
                            }
                        }
                    }
                }
            }

            // Find all stuck vehicles in outside connections
            FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
            foreach (ushort connection in connections)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[connection];
                if (building.m_flags != 0)
                {
                    // Guest vehicles
                    int iLoopCount = 0;
                    ushort vehicleId = building.m_guestVehicles;
                    while (vehicleId != 0)
                    {
                        ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                        if (vehicle.m_flags == 0)
                        {
                            // Add the created flag so we can actually remove the vehicle
                            sMessage += $"\r\nVehicle:{vehicleId} Flags:{vehicle.m_flags}";
                            vehicle.m_flags |= Vehicle.Flags.Created;
                            ghostVehicles.Add(vehicleId);
                        }
                        else if (vehicle.m_waitCounter >= 255)
                        {
                            // Waiting timer is maxed and the vehicle hasnt moved, remove
                            sMessage += $"\r\nVehicle:{vehicleId} Flags:{vehicle.m_flags} WaitTimer:{vehicle.m_waitCounter}";
                            ghostVehicles.Add(vehicleId);
                        }
                        vehicleId = vehicle.m_nextGuestVehicle;

                        // Check for bad list
                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }

                    // Own vehicles
                    iLoopCount = 0;
                    vehicleId = building.m_ownVehicles;
                    while (vehicleId != 0)
                    {
                        ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                        if (vehicle.m_flags == 0)
                        {
                            // Add the created flag so we can actually remove the vehicle
                            sMessage += $"\r\nVehicle:{vehicleId} Flags:{vehicle.m_flags}";
                            vehicle.m_flags |= Vehicle.Flags.Created;
                            ghostVehicles.Add(vehicleId);
                        }
                        else if (vehicle.m_waitCounter >= 255)
                        {
                            // Waiting timer is maxed and the vehicle hasnt moved, remove
                            sMessage += $"\r\nVehicle:{vehicleId} Flags:{vehicle.m_flags} WaitTimer:{vehicle.m_waitCounter}";
                            ghostVehicles.Add(vehicleId);
                        }
                        vehicleId = vehicle.m_nextOwnVehicle;

                        // Check for bad list
                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }   
            }

            // Perform actual de-spawning
            foreach (ushort vehicleId in ghostVehicles)
            {
                ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
                }
            }

            if (sMessage.Length > 0)
            {
                Debug.Log("Ghost vehicles: " + sMessage);
            }

            if (!wasPausedBeforeReset)
            {
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = false;
            }

            return ghostVehicles.Count;
        }
    }
}