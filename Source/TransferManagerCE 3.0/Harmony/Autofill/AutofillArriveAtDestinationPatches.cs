using System;
using ColossalFramework;
using HarmonyLib;
using SleepyCommon;
using static TransferManager;

namespace TransferManagerCore
{
    // --------------------------------------------------------------------
    [HarmonyPatch]
    public class AutofillArriveAtDestinationPatches
    {
        // --------------------------------------------------------------------
        // Remove any vehicles without valid targets from cargo vehicles
        [HarmonyPatch(typeof(CargoShipAI), "ArriveAtDestination")]
        [HarmonyPrefix]
        public static void CargoShipAIArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            ArriveAtDestinationImpl(vehicleID, ref vehicleData);
        }

        // --------------------------------------------------------------------
        // Remove any vehicles without valid targets from cargo vehicles
        [HarmonyPatch(typeof(CargoPlaneAI), "ArriveAtDestination")]
        [HarmonyPrefix]
        public static void CargoPlaneAIArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            ArriveAtDestinationImpl(vehicleID, ref vehicleData);
        }

        // --------------------------------------------------------------------
        // Remove any vehicles without valid targets from cargo vehicles
        [HarmonyPatch(typeof(CargoTrainAI), "ArriveAtDestination")]
        [HarmonyPrefix]
        public static void CargoTrainAIArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            ArriveAtDestinationImpl(vehicleID, ref vehicleData);
        }

        // --------------------------------------------------------------------
        // Loop through cargo and find any vehicles that dont have a target and unspawn them
        private static void ArriveAtDestinationImpl(ushort vehicleID, ref Vehicle vehicleData)
        {
            // Check its enabled
            if (!SaveGameSettings.GetSettings().Autofill)
            {
                return;
            }

            if (vehicleData.m_firstCargo != 0)
            {
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                ushort cargoId = vehicleData.m_firstCargo;
                int iLoopCount = 0;
                while (cargoId != 0)
                {
                    ref Vehicle vehicle = ref vehicles[cargoId];
                    ushort nextCargo = vehicle.m_nextCargo;

                    if (vehicle.m_targetBuilding == 0)
                    {
                        // One of our Autofill vehicles that didnt find a target, release vehicle
                        VehicleManager.instance.ReleaseVehicle(cargoId);
                    }

                    // Update list pointer
                    cargoId = nextCargo;

                    // Check we arent infinite looping
                    if (++iLoopCount > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }
    }
}