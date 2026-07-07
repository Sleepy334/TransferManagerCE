using ColossalFramework;
using HarmonyLib;
using System;
using TransferManagerCore.Settings;

namespace TransferManagerCore
{
    // Fix cargo trucks spawning at outside connections then disappearing.
    [HarmonyPatch]
    public class ArriveAtTargetPatches
    {
        // --------------------------------------------------------------------
        [HarmonyPatch(typeof(CargoShipAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoShipAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            return ArriveAtTargetPrefix(vehicleID, ref data, ref __result);
        }

        // --------------------------------------------------------------------
        [HarmonyPatch(typeof(CargoPlaneAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoPlaneAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            return ArriveAtTargetPrefix(vehicleID, ref data, ref __result);
        }

        // --------------------------------------------------------------------
        [HarmonyPatch(typeof(CargoTrainAI), "ArriveAtTarget")]
        [HarmonyPrefix]
        public static bool CargoTrainAIArriveAtTarget(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            return ArriveAtTargetPrefix(vehicleID, ref data, ref __result);
        }

        // --------------------------------------------------------------------
        private static bool ArriveAtTargetPrefix(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (ModSettings.GetSettings().FixCargoTrucksDisappearingOutsideConnections)
            {
                __result = ArriveAtTarget(vehicleID, ref data);
                return false; // Bypass original function
            }

            return true;
        }

        // --------------------------------------------------------------------
        private static bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            ushort num = data.m_firstCargo;
            data.m_firstCargo = 0;
            int num2 = 0;
            while (num != 0)
            {
                ref Vehicle childVehicle = ref buffer[num];

                ushort nextCargo = childVehicle.m_nextCargo;
                childVehicle.m_nextCargo = 0;
                childVehicle.m_cargoParent = 0;
                VehicleInfo info = childVehicle.Info;

                if (data.m_targetBuilding != 0)
                {
                    if (data.m_targetBuilding == childVehicle.m_targetBuilding)
                    {
                        // We have arrived at childs destination (either OC or Cargo Warehouse) if we call SetTarget the vehicle will
                        // briefly spawn then call ArriveAtDestination. We skip this by jumping straight to the ArriveAtDestination call
                        // and then releasing vehicle
                        info.m_vehicleAI.ArriveAtDestination(num, ref childVehicle);
                        childVehicle.m_transferSize = 0;
                        VehicleManager.instance.ReleaseVehicle(num);
                    }
                    else
                    {
                        info.m_vehicleAI.SetSource(num, ref childVehicle, data.m_targetBuilding);
                        info.m_vehicleAI.SetTarget(num, ref childVehicle, childVehicle.m_targetBuilding);
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
