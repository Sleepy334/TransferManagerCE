using ColossalFramework.Math;
using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CreateOutgoingVehiclePatch
    {
        [HarmonyPatch(typeof(TransportStationAI), "CreateOutgoingVehicle")]
        [HarmonyPrefix]
        public static bool CreateOutgoingVehiclePrefix(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort startStop, int gateIndex, ref bool __result)
        {
            // Airport DLC gates have their own spawn spot so we just check that no owned vehicles are nearby to allow spawning
            if (__instance is AirportGateAI && ModSettings.GetSettings().ForcePassengerPlaneSpawnAtGate)
            {
                TransportInfo transportInfo = ((!UseSecondaryTransportInfoForConnection(__instance)) ? __instance.m_transportInfo : __instance.m_secondaryTransportInfo);
                if ((object)__instance.m_transportLineInfo != null && FindConnectionVehicle(__instance, buildingID, ref buildingData, startStop, 3000f) == 0)
                {
                    VehicleInfo vehicleInfo = (((object)__instance.m_overrideVehicleClass == null) ?
                        Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_transportLineInfo.m_class.m_service, __instance.m_transportLineInfo.m_class.m_subService, __instance.m_transportLineInfo.m_class.m_level, transportInfo.m_vehicleType) :
                        Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_overrideVehicleClass.m_service, __instance.m_overrideVehicleClass.m_subService, __instance.m_overrideVehicleClass.m_level, transportInfo.m_vehicleType));

                    if ((object)vehicleInfo != null)
                    {
                        Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                        Randomizer randomizer = default(Randomizer);
                        randomizer.seed = (ulong)gateIndex;
                        __instance.CalculateSpawnPosition(buildingID, ref buildingData, ref randomizer, vehicleInfo, out var position, out var _);
                        if (CanSpawnAtGate(__instance, buildingID, ref buildingData) && Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, transportInfo.m_vehicleReason, transferToSource: false, transferToTarget: true))
                        {
                            vehicles.m_buffer[vehicle].m_gateIndex = (byte)gateIndex;
                            Vehicle.Flags flags = ((vehicleInfo.m_class.m_subService != ItemClass.SubService.PublicTransportBus) ? (Vehicle.Flags.Importing | Vehicle.Flags.Exporting) : Vehicle.Flags.Exporting);
                            vehicles.m_buffer[vehicle].m_flags |= flags;
                            vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                            vehicleInfo.m_vehicleAI.SetTarget(vehicle, ref vehicles.m_buffer[vehicle], startStop);
                            
                            //Debug.Log($"AirportGateAI.CreateOutgoingVehicle: buildingID{buildingID} startStop:{startStop} - SPAWNED");
                            __result = true;
                            return false; // Skip original function
                        }
                    }
                }

                //Debug.Log($"AirportGateAI.CreateOutgoingVehicle: buildingID{buildingID} startStop:{startStop} - DENIED");
                __result = false;
                return false; // Skip original function
            }

            // Use original function for other TransportStationAI types.
            return true;
        }

        private static bool UseSecondaryTransportInfoForConnection(TransportStationAI __instance)
        {
            return (object)__instance.m_secondaryTransportInfo != null && 
                __instance.m_secondaryTransportInfo.m_class.m_subService == __instance.m_transportLineInfo.m_class.m_subService && 
                __instance.m_secondaryTransportInfo.m_class.m_level == __instance.m_transportLineInfo.m_class.m_level;
        }

        private static ushort FindConnectionVehicle(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort targetStop, float maxDistance)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[targetStop].m_position;
            ushort num = buildingData.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                if (instance.m_vehicles.m_buffer[num].m_transportLine == 0)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    if (info.m_class.m_service == __instance.m_transportLineInfo.m_class.m_service && info.m_class.m_subService == __instance.m_transportLineInfo.m_class.m_subService && instance.m_vehicles.m_buffer[num].m_targetBuilding == targetStop && Vector3.SqrMagnitude(instance.m_vehicles.m_buffer[num].GetLastFramePosition() - position) < maxDistance * maxDistance)
                    {
                        return num;
                    }
                }

                num = instance.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return 0;
        }

        private static bool CanSpawnAtGate(TransportStationAI __instance, ushort buildingID, ref Building buildingData)
        {
            // Airport DLC gates have their own spawn spot so we just check that no owned vehicles are nearby to allow spawning
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            ushort num = buildingData.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                Vehicle vehicle = Vehicles[num];

                // We check if there are any other 'own' vehicles waiting nearby. Use quite a big distance ~100m
                if (vehicle.m_flags != 0 && Vector3.SqrMagnitude(vehicle.GetLastFramePosition() - buildingData.m_position) < 10000f)
                {
                    //Debug.Log($"Vehicle close by: {num}");
                    return false;
                }

                // Get next vehicle
                num = Vehicles[num].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return true;
        }
    }
}
