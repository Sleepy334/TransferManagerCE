using ColossalFramework;
using HarmonyLib;
using System;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
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

        // Copied from CargoTrainAI.ArriveAtTarget
        private static bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            ushort num = data.m_firstCargo;
            data.m_firstCargo = 0;
            int num2 = 0;
            while (num != 0)
            {
                ushort nextCargo = buffer[num].m_nextCargo;
                buffer[num].m_nextCargo = 0;
                buffer[num].m_cargoParent = 0;
                VehicleInfo info = buffer[num].Info;
                if (data.m_targetBuilding != 0)
                {
                    if (data.m_targetBuilding == buffer[num].m_targetBuilding)
                    {
                        info.m_vehicleAI.ArriveAtDestination(num, ref buffer[num]);
                    }
                    else
                    {
                        info.m_vehicleAI.SetSource(num, ref buffer[num], data.m_targetBuilding);
                        info.m_vehicleAI.SetTarget(num, ref buffer[num], buffer[num].m_targetBuilding);
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
