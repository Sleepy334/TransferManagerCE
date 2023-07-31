using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class WarehouseAIPatches
    {
        // A patch for the fish warehouses creating fishing boats instead of trucks.
        [HarmonyPatch(typeof(WarehouseAI), "GetTransferVehicleService")]
        [HarmonyPrefix]
        public static bool GetTransferVehicleServicePrefix(TransferManager.TransferReason material, ItemClass.Level level, ref Randomizer randomizer, ref VehicleInfo __result)
        {
            if (material == TransferReason.Fish)
            {
                __result = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Fishing, ItemClass.SubService.None, ItemClass.Level.Level1, VehicleInfo.VehicleType.Car);
                return false;
            }

            return true; // Handle normally
        }
    }
}
