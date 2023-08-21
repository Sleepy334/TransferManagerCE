using ColossalFramework;
using HarmonyLib;
using System;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class ArriveAtTargetPatches
    {
        [HarmonyPatch(typeof(CargoShipAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoShipAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            __result = ArriveAtTarget(vehicleID, ref data);
            return false; // Bypass original function
        }

        [HarmonyPatch(typeof(CargoPlaneAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoPlaneAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            __result = ArriveAtTarget(vehicleID, ref data);
            return false; // Bypass original function
        }

        [HarmonyPatch(typeof(CargoTrainAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoTrainAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            __result = ArriveAtTarget(vehicleID, ref data);
            return false; // Bypass original function
        }

        private static bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_firstCargo;
            data.m_firstCargo = 0;
            int num2 = 0;
            while (num != 0)
            {
                // Get a ref for this vehicle
                ref Vehicle vehicle = ref instance.m_vehicles.m_buffer[num];

                ushort nextCargo = vehicle.m_nextCargo;
                vehicle.m_nextCargo = 0;
                vehicle.m_cargoParent = 0;
                VehicleInfo info = vehicle.Info;
                if (data.m_targetBuilding != 0)
                {
                    // If cargo vehicle target is also vehicle target then just call ArriveAtDestination.
                    if (data.m_targetBuilding == vehicle.m_targetBuilding)
                    {
#if DEBUG
                        Debug.Log($"Vehicle:{num} - Arrived at destination");
#endif
                        info.m_vehicleAI.ArriveAtDestination(num, ref vehicle);
                    }
                    else
                    {
                        info.m_vehicleAI.SetSource(num, ref instance.m_vehicles.m_buffer[num], data.m_targetBuilding);
                        info.m_vehicleAI.SetTarget(num, ref instance.m_vehicles.m_buffer[num], vehicle.m_targetBuilding);
                    }

                }

                num = nextCargo;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            data.m_waitCounter = 0;
            data.m_flags |= Vehicle.Flags.WaitingLoading;
            return false; // Bypass original function
        }
    }
}
