using ColossalFramework;
using HarmonyLib;
using SleepyCommon;
using System;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    // CanSpawnAt should return true when near an outside connection so that outside connections don't get blocked up.
    [HarmonyPatch]
    public class ShipSpawnPatches
    {
        [HarmonyPatch(typeof(ShipAI), "CanSpawnAt")]
        [HarmonyPrefix]
        public static bool ShipAICanSpawnAtPrefix(ShipAI __instance, Vector3 pos, ref bool __result)
        {
            bool bForceSpawn = false;
            switch (__instance)
            {
                case CargoShipAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForceCargoShipSpawn;
                        break;
                    }
                case PassengerShipAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForcePassengerShipSpawn;
                        break;
                    }
            }

            // If the ship is near an outside connection then let it spawn so it doesnt block up the outside connection
            if (bForceSpawn && CitiesUtils.IsNearOutsideConnection(pos, ItemClass.SubService.PublicTransportShip))
            {
#if DEBUG
                CDebug.Log($"Force spawn: {__instance}");
#endif
                __result = true;
                
            }
            else
            {
                __result = CanSpawnAt(pos);
            }

            // Override vanilla function
            return false; 
        }

        [HarmonyPatch(typeof(ShipAI), "TrySpawn")]
        [HarmonyPrefix]
        public static bool ShipAITrySpawn(ShipAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            // Check if it is at an outside connection and force spawn
            bool bForceSpawn = false;
            switch (__instance)
            {
                case CargoShipAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForceCargoShipSpawn;
                        break;
                    }
                case PassengerShipAI:
                    {
                        bForceSpawn = ModSettings.GetSettings().ForcePassengerShipSpawn;
                        break;
                    }
            }

            if (bForceSpawn &&
                (vehicleData.m_flags & Vehicle.Flags.WaitingSpace) != 0 &&
                CitiesUtils.IsNearOutsideConnection(vehicleData.GetLastFramePosition(), ItemClass.SubService.PublicTransportShip))
            {
#if DEBUG
                CDebug.Log($"Force spawn: {__instance}");
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

        

        // We need to check that it's the same vehicle type, as fishing boats and helicopters also use this grid.
        private static bool CanSpawnAt(Vector3 pos)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            int num = Mathf.Max((int)((pos.x - 300f) / 320f + 27f), 0);
            int num2 = Mathf.Max((int)((pos.z - 300f) / 320f + 27f), 0);
            int num3 = Mathf.Min((int)((pos.x + 300f) / 320f + 27f), 53);
            int num4 = Mathf.Min((int)((pos.z + 300f) / 320f + 27f), 53);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_vehicleGrid2[i * 54 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        Vehicle vehicle = instance.m_vehicles.m_buffer[num5];
                        if (vehicle.Info is not null && vehicle.Info.GetAI() is ShipAI)
                        {
                            if (Vector3.SqrMagnitude(vehicle.GetLastFramePosition() - pos) < 90000f)
                            {
                                return false;
                            }
                        }


                        num5 = vehicle.m_nextGridVehicle;
                        if (++num6 > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return true;
        }

        // DEBUGGING - Use this to determine why vehicle isnt spawning.
        /*
        [HarmonyPatch(typeof(ShipAI), "TrySpawn")]
        [HarmonyPostfix]
        public static void ShipAITrySpawnPostfix(ShipAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            if (vehicleData.m_sourceBuilding == 11925)
                CDebug.Log($"Vehicle: {vehicleID} Result: {__result}");
        }

        public static bool TrySpawn(ShipAI __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.Spawned) != 0)
            {
                if (vehicleData.m_sourceBuilding == 11925)
                    CDebug.Log($"1. Vehicle: {vehicleID}");
                return true;
            }

            if (vehicleData.m_sourceBuilding == 11925)
                CDebug.Log($"2. Vehicle: {vehicleID}");
            if (CheckOverlap(__instance, vehicleData.m_segment, 0))
            {
                vehicleData.m_flags |= Vehicle.Flags.WaitingSpace;

                if (vehicleData.m_sourceBuilding == 11925)
                    CDebug.Log($"3. Vehicle: {vehicleID}");
                return false;
            }

            if (vehicleData.m_path != 0)
            {
                PathManager instance = Singleton<PathManager>.instance;
                if (instance.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(0, out var position))
                {
                    uint laneID = PathManager.GetLaneID(position);
                    if (laneID != 0 && !Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].CheckSpace(1000f, vehicleID))
                    {
                        vehicleData.m_flags |= Vehicle.Flags.WaitingSpace;

                        if (vehicleData.m_sourceBuilding == 11925)
                            CDebug.Log($"4. Vehicle: {vehicleID}");
                        return false;
                    }
                }
            }

            vehicleData.Spawn(vehicleID);
            vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;

            if (vehicleData.m_sourceBuilding == 11925)
                Debug.Log($"5. Vehicle: {vehicleID}");
            return true;
        }

        private static bool CheckOverlap(ShipAI __instance, Segment3 segment, ushort ignoreVehicle)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vector3 vector = segment.Min();
            Vector3 vector2 = segment.Max();
            int num = Mathf.Max((int)((vector.x - 100f) / 320f + 27f), 0);
            int num2 = Mathf.Max((int)((vector.z - 100f) / 320f + 27f), 0);
            int num3 = Mathf.Min((int)((vector2.x + 100f) / 320f + 27f), 53);
            int num4 = Mathf.Min((int)((vector2.z + 100f) / 320f + 27f), 53);
            bool overlap = false;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_vehicleGrid2[i * 54 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        num5 = CheckOverlap(segment, ignoreVehicle, num5, ref instance.m_vehicles.m_buffer[num5], ref overlap);
                        if (++num6 > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return overlap;
        }

        private static ushort CheckOverlap(Segment3 segment, ushort ignoreVehicle, ushort otherID, ref Vehicle otherData, ref bool overlap)
        {
            if ((ignoreVehicle == 0 || (otherID != ignoreVehicle && otherData.m_leadingVehicle != ignoreVehicle && otherData.m_trailingVehicle != ignoreVehicle)) && segment.DistanceSqr(otherData.m_segment, out var _, out var _) < 400f)
            {
                CDebug.Log($"Overlap found - Vehicle: {otherID}");
                overlap = true;
            }

            return otherData.m_nextGridVehicle;
        }
        */
    }
}
