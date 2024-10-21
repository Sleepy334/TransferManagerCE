using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    // CanSpawnAt should return true when near an outside connection so that outside connections don't get blocked up.
    [HarmonyPatch]
    public class AircraftSpawnPatches
    {
        [HarmonyPatch(typeof(AircraftAI), "CanSpawnAt")]
        [HarmonyPrefix]
        public static bool AircraftAICanSpawnAtPrefix(AircraftAI __instance, Vector3 pos, ref bool __result)
        {
            bool bForceSpawn = false;
            switch (__instance)
            {
                case CargoPlaneAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForceCargoPlaneSpawn;
                        break;
                    }
                case PassengerPlaneAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForcePassengerPlaneSpawn;
                        break;
                    }
            }

            // If the plane is near an outside connection then let it spawn so it doesnt block up the outside connection
            if (bForceSpawn && CitiesUtils.IsNearOutsideConnection(pos, ItemClass.SubService.PublicTransportPlane))
            {
#if DEBUG
                Debug.Log($"Force spawn: {__instance}");
#endif
                __result = true;
                return false; // Override vanilla function
            }

            return true;
        }

        [HarmonyPatch(typeof(AircraftAI), "TrySpawn")]
        [HarmonyPrefix]
        public static bool AircraftAITrySpawn(AircraftAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            bool bForceSpawn = false;
            switch (__instance)
            {
                case CargoPlaneAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForceCargoPlaneSpawn;
                        break;
                    }
                case PassengerPlaneAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForcePassengerPlaneSpawn;
                        break;
                    }
            }

            // Check if it is at an outside connection and force spawn
            if (bForceSpawn &&
                (vehicleData.m_flags & Vehicle.Flags.WaitingSpace) != 0 &&
                CitiesUtils.IsNearOutsideConnection(vehicleData.GetLastFramePosition(), ItemClass.SubService.PublicTransportPlane))
            {
#if DEBUG
                Debug.Log($"Force spawn: {__instance}");
#endif
                // Force spawn at outside connection
                vehicleData.Spawn(vehicleID);
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
                __result = true;
                return false; // Override vanilla function
            }

            // Handle normally
            return true;
        }
    }
}
