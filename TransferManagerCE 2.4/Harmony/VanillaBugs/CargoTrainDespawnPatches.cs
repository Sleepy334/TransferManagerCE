using HarmonyLib;
using System;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CargoTrainDespawnPatches
    {
        // This patch forces cargo trains to despawn at outside connections which increases outside connection throughput.
        [HarmonyPatch(typeof(CargoTrainAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void SimulationStep(CargoTrainAI __instance, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if (ModSettings.GetSettings().ForceCargoTrainDespawnOutsideConnections &&
                data.m_flags != 0 &&
                ((data.m_flags2 & Vehicle.Flags2.Yielding) != 0 || (data.m_flags & Vehicle.Flags.Arriving) != 0) &&
                data.m_targetBuilding != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[data.m_targetBuilding];
                if (building.m_flags != 0 && building.Info?.m_buildingAI is OutsideConnectionAI)
                {
                    float fDistanceSquared = Vector3.SqrMagnitude(data.GetLastFramePosition() - building.m_position);
                    if (fDistanceSquared < 110000f)
                    {
                        //Debug.Log($"Vehicle:{vehicleID} yielding, Outside connection:{vehicleData.m_targetBuilding} Distance: {fDistanceSquared}");
                        __instance.ArriveAtDestination(vehicleID, ref data);
                    }
                }
            }
        }

        // We need to despawn vehicle if __result is true.
        [HarmonyPatch(typeof(CargoTrainAI), "ArriveAtDestination")]
        [HarmonyPostfix]
        public static void CargoTrainAIArriveAtDestinationPostfix(ushort vehicleID, ref Vehicle vehicleData, ref bool __result)
        {
            // ArriveAtDestination returns true if despawned, so only apply if __result is false.
            if (!__result && 
                ModSettings.GetSettings().ForceCargoTrainDespawnOutsideConnections &&
                vehicleData.m_targetBuilding != 0 &&
                BuildingTypeHelper.IsOutsideConnection(vehicleData.m_targetBuilding))
            {
                // We release the train and then return true to ensure SimulationStep ends processing
                VehicleManager.instance.ReleaseVehicle(vehicleID);
                __result = true;
            }
        }
    }
}
