using ColossalFramework;
using SleepyCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ParadeGroupInfo;

namespace TransferManagerCore
{
    public class StuckVehicles
    {
        public static int ReleaseGhostVehicles()
        {
            bool wasPausedBeforeReset = Singleton<SimulationManager>.instance.ForcedSimulationPaused;
            if (!wasPausedBeforeReset)
            {
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = true;
            }

            string sMessage = "";

            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;

            HashSet<ushort> ghostVehicles = new HashSet<ushort>();
            for (int i = 1; i < VehicleManager.instance.m_vehicles.m_buffer.Length; i++)
            {
                ushort vehicleId = (ushort)i;
                ref Vehicle vehicle = ref Vehicles[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    Vector3 oPosition = vehicle.GetLastFramePosition();
                    if (oPosition.ToString().Contains("NaN") ||
                        Math.Abs(oPosition.x) > 100000 ||
                        Math.Abs(oPosition.y) > 100000 ||
                        Math.Abs(oPosition.z) > 100000)
                    {
                        sMessage += $"\r\n{GetVehicleDescription(vehicleId, vehicle)} - Invalid Position {oPosition}";

                        // Despawn vehicle so a new one can be created
                        ghostVehicles.Add(vehicleId);
                    }

                    // If m_flags has WaitingPath but not Created then it is broken
                    if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) != 0 && (vehicle.m_flags & Vehicle.Flags.Created) == 0)
                    {
                        sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle);

                        // Despawn vehicle so a new one can be created
                        ghostVehicles.Add(vehicleId);
                    }

                    // Check cargo parent is setup correctly
                    if (vehicle.m_cargoParent != 0)
                    {
                        Vehicle parent = Vehicles[vehicle.m_cargoParent];

                        // Check parent flags
                        if (parent.m_flags == 0)
                        {
                            sMessage += $"\r\n{GetVehicleDescription(vehicleId, vehicle)} - Invalid Parent: {vehicle.m_cargoParent} [{parent.m_flags}]";

                            // Despawn vehicle so a new one can be created
                            ghostVehicles.Add(vehicleId);
                        }
                        else if (vehicle.m_cargoParent == vehicleId)
                        {
                            sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - VehicleId = CargoParentId";

                            // Despawn vehicle so a new one can be created
                            ghostVehicles.Add(vehicleId);
                        }
                        else
                        {
                            // Check vehicle in parent cargo list
                            if (!VehicleUtils.IsVehicleInCargoList(vehicleId, vehicle.m_cargoParent))
                            {
                                sMessage += $"\r\n{GetVehicleDescription(vehicleId, vehicle)} - Vehicle not in parent cargo list: {VehicleUtils.GetCargoVehicles(vehicle.m_cargoParent).AllToString()}";
                                ghostVehicles.Add(vehicleId);
                            }
                        }
                    }
                    else
                    {
                        // No cargo parent

                        // Check source building setup correctly
                        if (vehicle.m_sourceBuilding != 0)
                        {
                            if (!BuildingUtils.IsVehicleInOwnList(vehicleId, vehicle.m_sourceBuilding, Buildings[vehicle.m_sourceBuilding]))
                            {
                                sMessage += $"\r\n{GetVehicleDescription(vehicleId, vehicle)} - Vehicle not in buildings OWN vehicles list, Building: {vehicle.m_sourceBuilding} Type: {Buildings[vehicle.m_sourceBuilding].Info} List: {BuildingUtils.GetOwnVehicles(vehicle.m_sourceBuilding, Buildings[vehicle.m_sourceBuilding]).AllToString()}";
                                vehicle.m_sourceBuilding = 0;
                                vehicle.m_nextOwnVehicle = 0;
                                ghostVehicles.Add(vehicleId);
                            }
                        }

                        // Check target building setup correctly
                        // WARNING: target building may be a node!!!
                        if (vehicle.m_targetBuilding != 0 && vehicle.Info is not null)
                        {
                            // Check target is a building
                            InstanceID vehicleTarget = vehicle.Info.m_vehicleAI.GetTargetID(vehicleId, ref vehicle);
                            if (vehicleTarget.Building != 0 &&
                                vehicleTarget.Building == vehicle.m_targetBuilding &&
                                vehicleTarget.Building != vehicle.m_sourceBuilding) // Fishing boats have source = target and are only added to OWN list
                            {
                                // Check vehicle in target guest list
                                if (!BuildingUtils.IsVehicleInGuestList(vehicleId, vehicle.m_targetBuilding, Buildings[vehicle.m_targetBuilding]))
                                {
                                    sMessage += $"\r\n{GetVehicleDescription(vehicleId, vehicle)} - Vehicle not in buildings GUEST vehicles list, Building: {vehicle.m_targetBuilding} Type: {Buildings[vehicle.m_targetBuilding].Info} List: {BuildingUtils.GetGuestVehicles(vehicle.m_targetBuilding, Buildings[vehicle.m_targetBuilding]).AllToString()}";
                                    vehicle.m_targetBuilding = 0;
                                    vehicle.m_nextGuestVehicle = 0;
                                    ghostVehicles.Add(vehicleId);
                                }
                            }
                        }
                    }
                }
                else if (vehicle.m_nextOwnVehicle != 0)
                {
                    sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - No flags but in building list [m_flags = 0, m_nextOwnVehicle = {vehicle.m_nextOwnVehicle}]";
                    vehicle.m_flags |= Vehicle.Flags.Created;
                    ghostVehicles.Add(vehicleId);
                }
                else if (vehicle.m_nextGuestVehicle != 0)
                {
                    sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - No flags but in building list [m_flags = 0, m_nextGuestVehicle = {vehicle.m_nextGuestVehicle}]";
                    vehicle.m_flags |= Vehicle.Flags.Created;
                    ghostVehicles.Add(vehicleId);
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
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if (vehicle.m_flags == 0)
                        {
                            // Add the created flag so we can actually remove the vehicle
                            sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - Invalid Flags, Outside connection {connection} guest vehicle.";
                            Vehicles[vehicleId].m_flags |= Vehicle.Flags.Created;
                            ghostVehicles.Add(vehicleId);
                        }
                        else if (vehicle.m_waitCounter >= 255)
                        {
                            // Waiting timer is maxed and the vehicle hasnt moved, remove
                            sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - WaitTimer = 255, Outside connection {connection} guest vehicle.";
                            ghostVehicles.Add(vehicleId);
                        }
                        return true;
                    });
                    
                    // Own vehicles
                    BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                    {
                        if (vehicle.m_flags == 0)
                        {
                            // Add the created flag so we can actually remove the vehicle
                            sMessage += "\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - Invalid Flags, Outside connection {connection} own vehicle.";
                            Vehicles[vehicleId].m_flags |= Vehicle.Flags.Created;
                            ghostVehicles.Add(vehicleId);
                        }
                        else if (vehicle.m_waitCounter >= 255)
                        {
                            // Waiting timer is maxed and the vehicle hasnt moved, remove
                            sMessage += $"\r\n" + GetVehicleDescription(vehicleId, vehicle) + $" - WaitTimer = 255, Outside connection {connection} own vehicle.";
                            ghostVehicles.Add(vehicleId);
                        }
                        return true;
                    });
                }   
            }

            // Perform actual de-spawning
            foreach (ushort vehicleId in ghostVehicles)
            {
                ref Vehicle vehicle = ref Vehicles[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    try
                    {
                        Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error releasing vehicle: {vehicleId}", ex);
                    }
                }
            }

            if (sMessage.Length > 0)
            {
                Log.Info("Ghost vehicles: " + sMessage);
            }

            if (!wasPausedBeforeReset)
            {
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = false;
            }

            return ghostVehicles.Count;
        }

        private static string GetVehicleDescription(ushort vehicleId, Vehicle vehicle)
        {
            string sText = $"Vehicle:{vehicleId} [{vehicle.m_flags}]";

            if (vehicle.Info is not null && vehicle.Info.GetAI() is not null)
            {
                sText += $" Type:{vehicle.Info.GetAI()}";
            }

            sText += $" Source:{vehicle.m_sourceBuilding} Target:{vehicle.m_targetBuilding} WaitTimer:{vehicle.m_waitCounter} CargoParent:{vehicle.m_cargoParent} First: {vehicle.m_firstCargo} Next: {vehicle.m_nextCargo} m_nextOwnVehicle {vehicle.m_nextOwnVehicle} m_nextGuestVehicle {vehicle.m_nextGuestVehicle}";

            return sText;
        }
    }
}