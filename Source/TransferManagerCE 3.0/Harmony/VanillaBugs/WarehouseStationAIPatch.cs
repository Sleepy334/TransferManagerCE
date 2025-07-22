using HarmonyLib;
using UnityEngine;
using ColossalFramework.Math;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class WarehouseStationAIPatch
    {
        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(WarehouseStationAI), "CalculateUnspawnPosition")]
        [HarmonyPrefix]
        public static bool CalculateUnspawnPosition(WarehouseStationAI __instance, ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            // We want cargo trucks to unspawn inside warehouse so the pathing uses a train rather than trying to use cargo trucks
            if (ModSettings.GetSettings().FixCargoWarehouseUnspawn && 
                info.m_vehicleType == VehicleInfo.VehicleType.Car && 
                IsCargoTruck(info))
            {
                position = data.CalculatePosition(__instance.m_spawnPosition);
                target = data.CalculatePosition(__instance.m_spawnTarget);

                // Dont call base function
                return false;
            } 

            position = Vector3.zero;
            target = Vector3.zero;

            // Handle with normal function
            return true;
        }

        // ----------------------------------------------------------------------------------------
        private static bool IsCargoTruck(VehicleInfo info)
        {
            if (info.m_class.m_service == ItemClass.Service.Industrial)
            {
                return true;
            }

            if (info.m_class.m_service == ItemClass.Service.PlayerIndustry)
            {
                return true;
            }

            if (info.m_class.m_subService == ItemClass.SubService.PublicTransportPost)
            {
                return true;
            }

            if (info.m_class.m_service == ItemClass.Service.Fishing)
            {
                return true;
            }

            return false;
        }
    }
}