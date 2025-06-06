using HarmonyLib;
using SleepyCommon;
using System;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CargoShipDespawnPatches
    {
        const float fDespawnDistanceSquared = 200000f;
        private static ushort s_despawnVehicleId = 0;

        // This patch forces cargo trains to despawn at outside connections which increases outside connection throughput.
        [HarmonyPatch(typeof(CargoShipAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void SimulationStep(CargoShipAI __instance, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if (ModSettings.GetSettings().ForceCargoShipDespawnOutsideConnections &&
                data.m_flags != 0 &&
                data.m_targetBuilding != 0 &&
                CitiesUtils.IsNearEdgeOfMap(data.GetLastFramePosition()))
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[data.m_targetBuilding];
                if (building.m_flags != 0 && building.Info?.m_buildingAI is OutsideConnectionAI)
                {
                    float fDistanceSquared = Vector3.SqrMagnitude(data.GetLastFramePosition() - building.m_position);
                    if (fDistanceSquared < fDespawnDistanceSquared)
                    {
                        //CDebug.Log($"Vehicle:{vehicleID} Flags: {data.m_flags}, Outside connection:{data.m_targetBuilding} Distance: {fDistanceSquared}");
                        __instance.ArriveAtDestination(vehicleID, ref data);
                    }
                }
            }
        }

        // Check target before ArriveAtDestination is called
        [HarmonyPatch(typeof(CargoShipAI), "ArriveAtDestination")]
        [HarmonyPrefix]
        public static bool CargoShipAIArriveAtDestinationPrefix(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (ModSettings.GetSettings().ForceCargoShipDespawnOutsideConnections &&
                vehicleData.m_targetBuilding != 0 &&
                BuildingTypeHelper.IsOutsideConnection(vehicleData.m_targetBuilding))
            {
                // Set flag so we despawn 
                s_despawnVehicleId = vehicleID;
            }

            return true;
        }

        // We need to despawn vehicle if __result is false.
        [HarmonyPatch(typeof(CargoShipAI), "ArriveAtDestination")]
        [HarmonyPostfix]
        public static void CargoShipAIArriveAtDestinationPostfix(ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            if (!__result && s_despawnVehicleId != 0 && s_despawnVehicleId == vehicleID)
            {
                // We release the train and then return true to ensure SimulationStep ends processing
                VehicleManager.instance.ReleaseVehicle(vehicleID);
                __result = true;
            }

            // Reset flag
            s_despawnVehicleId = 0;
        }   
    }
}
