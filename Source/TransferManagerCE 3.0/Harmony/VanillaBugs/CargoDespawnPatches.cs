using HarmonyLib;
using SleepyCommon;
using System;
using TransferManagerCore.Settings;
using UnityEngine;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class CargoDespawnPatches
    {
        const float fDespawnDistanceSquared = 150000f;

        // --------------------------------------------------------------------
        // This patch forces cargo trains to despawn at outside connections which increases outside connection throughput.
        [HarmonyPatch(typeof(CargoTrainAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void SimulationStep(CargoTrainAI __instance, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            ForceCargoVehicleDespawnOutsideConnection(__instance, vehicleID, ref data);
        }

        // --------------------------------------------------------------------
        // This patch forces cargo trains to despawn at outside connections which increases outside connection throughput.
        [HarmonyPatch(typeof(CargoShipAI), "SimulationStep",
          new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) },
          new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void SimulationStep(CargoShipAI __instance, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            ForceCargoVehicleDespawnOutsideConnection(__instance, vehicleID, ref data);
        }

        // --------------------------------------------------------------------
        // This patch forces cargo trains to despawn at outside connections which increases outside connection throughput.
        [HarmonyPatch(typeof(CargoPlaneAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void SimulationStep(CargoPlaneAI __instance, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            ForceCargoVehicleDespawnOutsideConnection(__instance, vehicleID, ref data);
        }

        // --------------------------------------------------------------------
        public static void ForceCargoVehicleDespawnOutsideConnection(VehicleAI __instance, ushort vehicleID, ref Vehicle data)
        {
            if (!ModSettings.GetSettings().ForceCargoTrainDespawnOutsideConnections)
            {
                return;
            }

            if ((data.m_flags & Vehicle.Flags.Created) != 0 &&
                (data.m_flags & Vehicle.Flags.Spawned) != 0 &&
                //(data.m_flags & Vehicle.Flags.Arriving) != 0 && // This doesnt get set till too late on ships
                (data.m_flags & (Vehicle.Flags.Exporting)) != 0 &&
                (data.m_flags & Vehicle.Flags.TransferToTarget) != 0 &&
                data.m_targetBuilding != 0 &&
                CitiesUtils.IsNearEdgeOfMap(data.GetLastFramePosition()))
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[data.m_targetBuilding];
                if (building.m_flags != 0 && building.Info?.m_buildingAI is OutsideConnectionAI)
                {
                    float fDistanceSquared = Vector3.SqrMagnitude(data.GetLastFramePosition() - building.m_position);
                    if (fDistanceSquared < fDespawnDistanceSquared)
                    {
                        // Trigger the arrival sequence early to improve throughput
                        //CDebug.Log($"Trigger Despawn - Vehicle: {vehicleID} Flags: {data.m_flags} AI: {__instance} Material: {(CustomTransferReason.Reason)data.m_transferType} TransferSize: {data.m_transferSize} Parent: {data.m_cargoParent} Source: {data.m_sourceBuilding} Target: {data.m_targetBuilding} Cargo: {VehicleUtils.GetCargoVehicles(vehicleID).AllToString()}");
                        bool bResult = __instance.ArriveAtDestination(vehicleID, ref data);
                        if (!bResult &&
                            data.m_firstCargo == 0 &&
                            (data.m_flags & Vehicle.Flags.WaitingLoading) != 0)
                        {
                            //CDebug.Log($"Unspawn - Vehicle: {vehicleID} Flags: {data.m_flags} AI: {__instance}");
                            data.Unspawn(vehicleID);
                        }
                        else
                        {
                            //CDebug.Log($"Dont unspawn - Vehicle: {vehicleID} Flags: {data.m_flags} AI: {__instance}");
                        }
                    }
                }
            }
        }
    }
}
