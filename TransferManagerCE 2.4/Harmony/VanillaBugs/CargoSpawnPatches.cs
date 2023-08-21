using ColossalFramework;
using HarmonyLib;
using System;
using TransferManagerCE.Data;
using UnityEngine;

namespace TransferManagerCE
{
    // CanSpawnAt should return true when near an outside connection so that outside connections don't get blocked up.
    [HarmonyPatch]
    public class CargoSpawnPatches
    {
        [HarmonyPatch(typeof(AircraftAI), "CanSpawnAt")]
        [HarmonyPrefix]
        public static bool AircraftAICanSpawnAtPrefix(AircraftAI __instance, Vector3 pos, ref bool __result)
        {
            // If the plane is near an outside connection then let it spawn so it doesnt block up the outside connection
            if (__instance is CargoPlaneAI && IsNearOutsideConnection(pos, ItemClass.SubService.PublicTransportPlane))
            {
                __result = true;
                return false; // Override vanilla function
            }

            return true;
        }

        [HarmonyPatch(typeof(AircraftAI), "TrySpawn")]
        [HarmonyPrefix]
        public static bool AircraftAITrySpawn(AircraftAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            // Check if it is at an outside connection and force spawn
            if (__instance is CargoPlaneAI &&
                (vehicleData.m_flags & Vehicle.Flags.WaitingSpace) != 0 &&
                IsNearOutsideConnection(vehicleData.GetLastFramePosition(), ItemClass.SubService.PublicTransportPlane))
            {
                // Force spawn at outside connection
                vehicleData.Spawn(vehicleID);
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
                __result = true;
                return false; // Override vanilla function
            }

            // Handle normally
            return true;
        }

        [HarmonyPatch(typeof(ShipAI), "CanSpawnAt")]
        [HarmonyPrefix]
        public static bool ShipAICanSpawnAtPrefix(ShipAI __instance, Vector3 pos, ref bool __result)
        {
            // If the ship is near an outside connection then let it spawn so it doesnt block up the outside connection
            if (__instance is CargoShipAI && IsNearOutsideConnection(pos, ItemClass.SubService.PublicTransportShip))
            {
                __result = true;
                return false; // Override vanilla function
            }

            return true;
        }

        [HarmonyPatch(typeof(ShipAI), "TrySpawn")]
        [HarmonyPrefix]
        public static bool ShipAITrySpawn(ShipAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            // Check if it is at an outside connection and force spawn
            if (__instance is CargoShipAI && 
                (vehicleData.m_flags & Vehicle.Flags.WaitingSpace) != 0 &&
                IsNearOutsideConnection(vehicleData.GetLastFramePosition(), ItemClass.SubService.PublicTransportShip))
            {
                // Force spawn at outside connection
                vehicleData.Spawn(vehicleID);
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
                __result = true;
                return false; // Override vanilla function
            }

            // Handle normally
            return true;
        }

        private static bool IsNearOutsideConnection(Vector3 position, ItemClass.SubService subService)
        {
            // If the plane is near an outside connection then let it spawn so it doesnt block up the outside connection
            foreach (ushort outsideConnectionId in BuildingManager.instance.GetOutsideConnections())
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[outsideConnectionId];
                if (building.m_flags != 0 &&
                    building.Info is not null &&
                    building.Info.GetSubService() == subService &&
                    Vector3.SqrMagnitude(building.m_position - position) < 2500f)
                {
#if DEBUG
                    Debug.Log($"CanSpawnAt: {position} Near outside connection: {outsideConnectionId}");
#endif
                    return true;
                }
            }

            return false;
        }
    }
}
