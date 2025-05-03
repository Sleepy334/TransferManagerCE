using ColossalFramework;
using Epic.OnlineServices.Presence;
using HarmonyLib;
using ICities;
using System;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class Mail2BuildingPatches
    {
        // We add a Mail2 offer for Post Trucks to get mail from Service Points
        // Note: Looking into PostVanAI.getBufferStatus, CO don't seem to want to deliver
        // SortedMail to service points so we don't make it a restriction for offering Mail2.
        [HarmonyPatch(typeof(PostOfficeAI), "ProduceGoods")]
        [HarmonyPostfix]
        public static void PostOfficeAIProduceGoods(PostOfficeAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
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
                Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)CustomTransferReason.Reason.Mail2, offer);
            }
        }

        // Handle the Mail2 transfer request. Create a Post Truck and set its transfer type to Mail.
        [HarmonyPatch(typeof(PostOfficeAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(PostOfficeAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if ((CustomTransferReason.Reason) material == CustomTransferReason.Reason.Mail2)
            {
                // Large Mail request
                int ownVanCount = 0;
                int ownTruckCount = 0;
                CalculateVehicles(ref data, ref ownVanCount, ref ownTruckCount);

                if (ownTruckCount < __instance.m_postTruckCount)
                {
                    VehicleInfo vehicleInfo2 = __instance.GetAdditionalSelectedVehicle(buildingID);
                    if (vehicleInfo2 == null)
                    {
                        vehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, ItemClass.Level.Level5); // Post Truck
                    }
                    if ((object)vehicleInfo2 == null)
                    {
                        return false;
                    }

                    // Check the post office has enough sorted mail to create vehicle.
                    Vehicle data2 = default(Vehicle);
                    vehicleInfo2.m_vehicleAI.GetSize(0, ref data2, out var _, out var max2);
                    int num = data.m_customBuffer2 * 1000;
                    if (num < max2)
                    {
                        return false;
                    }

                    Array16<Vehicle> vehicles2 = Singleton<VehicleManager>.instance.m_vehicles;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle2, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo2, data.m_position, TransferManager.TransferReason.Mail, transferToSource: true, transferToTarget: false))
                    {
                        bool flag2 = offer.Building != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building].Info.m_buildingAI is ServicePointAI;
                        if (flag2)
                        {
                            vehicles2.m_buffer[vehicle2].m_flags2 |= Vehicle.Flags2.TransferToServicePoint;
                        }

                        vehicleInfo2.m_vehicleAI.SetSource(vehicle2, ref vehicles2.m_buffer[vehicle2], buildingID);
                        vehicleInfo2.m_vehicleAI.StartTransfer(vehicle2, ref vehicles2.m_buffer[vehicle2], TransferManager.TransferReason.Mail, offer);
                    }
                }

                // Transfer handled do not call vanilla function
                return false;
            }

            // Normal transfer pass onto vanilla function.
            return true;
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
