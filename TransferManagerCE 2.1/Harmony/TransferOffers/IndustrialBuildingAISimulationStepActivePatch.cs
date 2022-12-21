using ColossalFramework;
using ColossalFramework.Math;
using Epic.OnlineServices.Presence;
using HarmonyLib;
using ICities;
using System;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class IndustrialBuildingAISimulationStepActivePatch
    {
        // Generic processing buildings behave badly, they ask twice in one round and with really high priority due to a bug in IndustrialBuildingAI.SimulationStepActive
        // where it uses the max load capacity (8) instead of storage capacity (up to 16) so priority is often twice what it should be.
        // This patch is an attempt to solve this
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndustrialBuildingAI), "SimulationStepActive")]
        public static void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager && 
                SaveGameSettings.GetSettings().OverrideGenericIndustriesHandler)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;
                TransferManager.TransferReason incomingTransferReason = GetIncomingTransferReason(buildingID, buildingData.Info);
                TransferManager.TransferReason secondaryIncomingTransferReason = GetSecondaryIncomingTransferReason(buildingID, buildingData.Info);

                if (buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
                {
                    IndustrialBuildingAI? processingAI = buildingData.Info.GetAI() as IndustrialBuildingAI;
                    if (processingAI != null)
                    {
                        // Determine priority based on current storage level
                        int iProductionCapacity = processingAI.CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), buildingData.Width, buildingData.Length);
                        int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);

                        // Factor in trucks on the way
                        int iTransferSize = CitiesUtils.GetGuestVehiclesTransferSize(buildingID, incomingTransferReason, secondaryIncomingTransferReason);

                        // Calculate a more realistic priority
                        int iPriority = Mathf.Clamp((iStorageCapacity - buildingData.m_customBuffer1 - iTransferSize) * 8 / iStorageCapacity, 0, 7);

                        TransferManager.TransferOffer offer = default;
                        offer.Priority = iPriority;
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = 1;
                        offer.Active = false;

                        // remove previous offers whether we add new ones or not as they are buggy
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(incomingTransferReason, offer);
                        if (secondaryIncomingTransferReason != TransferManager.TransferReason.None)
                        {
                            // remove previous secondary offers
                            Singleton<TransferManager>.instance.RemoveIncomingOffer(secondaryIncomingTransferReason, offer);
                        }

                        // We don't request every time so it gives time for a vehicle to be matched and
                        // dispatched before we request again.
                        if (iPriority >= 3 && random.UInt32(3U) == 0)
                        {
                            // Add new offer
                            if (secondaryIncomingTransferReason != TransferManager.TransferReason.None)
                            {
                                // Alternate primary/secondary offer
                                if (random.UInt32(2U) == 0)
                                {
                                    Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                                }
                                else
                                {
                                    Singleton<TransferManager>.instance.AddIncomingOffer(secondaryIncomingTransferReason, offer);
                                }
                            }
                            else
                            {
                                Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                            }
                        }
                    }
                }
            }
        }

        private static TransferManager.TransferReason GetIncomingTransferReason(ushort buildingID, BuildingInfo info)
        {
            return info.m_class.m_subService switch
            {
                ItemClass.SubService.IndustrialForestry => TransferManager.TransferReason.Logs,
                ItemClass.SubService.IndustrialFarming => TransferManager.TransferReason.Grain,
                ItemClass.SubService.IndustrialOil => TransferManager.TransferReason.Oil,
                ItemClass.SubService.IndustrialOre => TransferManager.TransferReason.Ore,
                _ => new Randomizer(buildingID).Int32(4u) switch
                {
                    0 => TransferManager.TransferReason.Lumber,
                    1 => TransferManager.TransferReason.Food,
                    2 => TransferManager.TransferReason.Petrol,
                    3 => TransferManager.TransferReason.Coal,
                    _ => TransferManager.TransferReason.None,
                },
            };
        }

        private static TransferManager.TransferReason GetSecondaryIncomingTransferReason(ushort buildingID, BuildingInfo info)
        {
            if (info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric)
            {
                switch (new Randomizer(buildingID).Int32(8u))
                {
                    case 0:
                        return TransferManager.TransferReason.PlanedTimber;
                    case 1:
                        return TransferManager.TransferReason.Paper;
                    case 2:
                        return TransferManager.TransferReason.Flours;
                    case 3:
                        return TransferManager.TransferReason.AnimalProducts;
                    case 4:
                        return TransferManager.TransferReason.Petroleum;
                    case 5:
                        return TransferManager.TransferReason.Plastics;
                    case 6:
                        return TransferManager.TransferReason.Metals;
                    case 7:
                        return TransferManager.TransferReason.Glass;
                }
            }

            return TransferManager.TransferReason.None;
        }
    }
}
