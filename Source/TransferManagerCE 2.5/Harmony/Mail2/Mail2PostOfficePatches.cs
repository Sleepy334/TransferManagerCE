using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class Mail2PostOfficePatches
    {
        // We add a Mail2 offer for Post Trucks to get mail from Service Points
        // Note: Looking into PostVanAI.getBufferStatus, CO don't seem to want to deliver
        // SortedMail to service points so we don't make it a restriction for offering Mail2.
        [HarmonyPatch(typeof(PostOfficeAI), "ProduceGoods")]
        [HarmonyPostfix]
        public static void ProduceGoods(PostOfficeAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            int ownVanCount = 0;
            int ownTruckCount = 0;
            CalculateVehicles(ref buildingData, ref ownVanCount, ref ownTruckCount);

            int iMaxTruckCount = (finalProductionRate * __instance.m_postTruckCount + 99) / 100;
            if (ownTruckCount < iMaxTruckCount)
            {
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = Mathf.Clamp(2 - ownTruckCount, 0, 7);
                offer.Building = buildingID;
                offer.Position = buildingData.m_position;
                offer.Amount = 1;
                offer.Active = true;
                Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason) CustomTransferReason.Reason.Mail2, offer);
            }
        }

        // Post trucks are level5 whereas post vans are level2
        public static void CalculateVehicles(ref Building data, ref int ownVanCount, ref int ownTruckCount)
        {
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            ushort num = data.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                Vehicle vehicle = Vehicles[num];
                if (vehicle.Info is not null && vehicle.Info.GetClassLevel() == ItemClass.Level.Level5)
                {
                    ownTruckCount++;
                }
                else
                {
                    ownVanCount++;
                }

                num = vehicle.m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }
    }
}
