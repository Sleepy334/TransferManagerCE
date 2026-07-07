using System.Collections.Generic;
using HarmonyLib;
using SleepyCommon;
using TransferManagerCore.Data;
using UnityEngine;
using static ParadeGroupInfo;

namespace TransferManagerCore
{
    // ------------------------------------------------------------------------
    [HarmonyPatch]
    public class CargoVehicleCheckPatch
    {
#if DEBUG
        // --------------------------------------------------------------------
        // DEBUGGING, check cargo vehicle arrays are still valid
        [HarmonyPatch(typeof(CargoTruckAI), "SimulationStep")]
        [HarmonyPostfix]
        public static void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            CheckCargoVehicleParent(vehicleID, ref data);
        }

        // --------------------------------------------------------------------
        public static void CheckCargoVehicleParent(ushort vehicleID, ref Vehicle data)
        {
            Vehicle[] vehicles = VehicleManager.instance.m_vehicles.m_buffer;

            if (data.m_cargoParent != 0)
            {
                if (vehicles[data.m_cargoParent].m_flags == 0)
                {
                    string sText = $"ERROR, parent bad";
                    sText += $"\r\nVehicle: {vehicleID} [{data.m_flags}] Parent: {data.m_cargoParent}  Material: {(CustomTransferReason.Reason)data.m_transferType} Source: {data.m_sourceBuilding} Target: {data.m_targetBuilding}";
                    sText += $"\r\nParent: {data.m_cargoParent} [{vehicles[data.m_cargoParent].m_flags}] Cargo: {VehicleUtils.GetCargoVehicles(data.m_cargoParent).AllToString()}";
                    CDebug.Log(sText);
                    Log.Error(sText);
                }
                else if (!VehicleUtils.IsVehicleInCargoList(vehicleID, data.m_cargoParent))
                {
                    string sText = $"ERROR, vehicle not in parent list";
                    sText += $"\r\nVehicle: {vehicleID} [{data.m_flags}] Parent: {data.m_cargoParent}  Material: {(CustomTransferReason.Reason)data.m_transferType} Source: {data.m_sourceBuilding} Target: {data.m_targetBuilding}";
                    sText += $"\r\nParent: {data.m_cargoParent} [{vehicles[data.m_cargoParent].m_flags}] Cargo: {VehicleUtils.GetCargoVehicles(data.m_cargoParent).AllToString()}";
                    CDebug.Log(sText);
                    Log.Error(sText);
                }
            }
        }

        // --------------------------------------------------------------------
        public static void CheckCargoVehicleList(ushort cargoVehicleId)
        {
            Vehicle[] vehicles = VehicleManager.instance.m_vehicles.m_buffer;

            if (cargoVehicleId != 0)
            {
                // Get Linked list and actual list
                HashSet<ushort> cargoVehiclesLinkedList = VehicleUtils.GetCargoVehicles(cargoVehicleId);
                HashSet<ushort> cargoVehiclesActualList = new HashSet<ushort>();

                for (int i = 0; i < vehicles.Length; ++i)
                {
                    if (vehicles[i].m_flags != 0 && vehicles[i].m_cargoParent == cargoVehicleId)
                    {
                        cargoVehiclesActualList.Add((ushort)i);
                    }
                }

                if (!cargoVehiclesLinkedList.SetEquals(cargoVehiclesActualList))
                {
                    string sText = $"ERROR, vehcile list and actual list different";
                    sText += $"\r\nVehicle: {cargoVehicleId} [{vehicles[cargoVehicleId].m_flags}]";
                    sText += $"\r\nLinkedList: {cargoVehiclesLinkedList.AllToString()}";
                    sText += $"\r\nActualList: {cargoVehiclesActualList.AllToString()}";
                    CDebug.Log(sText);
                    Log.Error(sText);

                }
            }
        }
#endif
    }
}