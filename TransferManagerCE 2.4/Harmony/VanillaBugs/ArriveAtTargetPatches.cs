using ColossalFramework;
using HarmonyLib;
using System;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    // Fix cargo trucks spawning at outside connections then disappearing.
    [HarmonyPatch]
    public class ArriveAtTargetPatches
    {
        [HarmonyPatch(typeof(CargoShipAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoShipAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (ModSettings.GetSettings().FixCargoTrucksDisappearingOutsideConnections)
            {
                __result = ArriveAtTarget(vehicleID, ref data);
                return false; // Bypass original function
            }
            
            return true;
        }

        [HarmonyPatch(typeof(CargoPlaneAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoPlaneAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (ModSettings.GetSettings().FixCargoTrucksDisappearingOutsideConnections)
            {
                __result = ArriveAtTarget(vehicleID, ref data);
                return false; // Bypass original function
            }

            return true;
        }

        [HarmonyPatch(typeof(CargoTrainAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoTrainAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (ModSettings.GetSettings().FixCargoTrucksDisappearingOutsideConnections)
            {
                __result = ArriveAtTarget(vehicleID, ref data);
                return false; // Bypass original function
            }

            return true;
        }

        private static bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            ushort num = data.m_firstCargo;
            data.m_firstCargo = 0;
            int num2 = 0;
            while (num != 0)
            {
                ref Vehicle vehicle = ref buffer[num];

                ushort nextCargo = vehicle.m_nextCargo;
                vehicle.m_nextCargo = 0;
                vehicle.m_cargoParent = 0;
                VehicleInfo info = vehicle.Info;
                if (data.m_targetBuilding != 0)
                {
                    if (data.m_targetBuilding == vehicle.m_targetBuilding)
                    {
                        // Call ArriveAtDestination instead of SetTarget so we don't end up spawning briefly at edge of map.
                        info.m_vehicleAI.ArriveAtDestination(num, ref vehicle);

                        // If arriving at outside connection then we remove vehicle
                        vehicle.m_transferSize = 0;
                        VehicleManager.instance.ReleaseVehicle(num);
                    }
                    else
                    {
                        info.m_vehicleAI.SetSource(num, ref vehicle, data.m_targetBuilding);
                        info.m_vehicleAI.SetTarget(num, ref vehicle, vehicle.m_targetBuilding);
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
            return false;
        }
    }
}
