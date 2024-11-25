﻿using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using TransferManagerCE.TransferOffers;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class IndustrialBuildingAISimulationStepActive
    {
        public static bool s_bRejectOffers = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(IndustrialBuildingAI), "SimulationStepActive")]
        public static void SimulationStepActivePrefix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            // We flag rejecting these offers
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideGenericIndustriesHandler)
            {
                s_bRejectOffers = true;
            }
        }

        // Generic processing buildings behave badly, they ask twice in one round and with really high priority due to a bug in IndustrialBuildingAI.SimulationStepActive
        // where it uses the max load capacity (8) instead of storage capacity (up to 16) so priority is often twice what it should be.
        // This patch is an attempt to solve this
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IndustrialBuildingAI), "SimulationStepActive")]
        public static void SimulationStepActivePostfix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            // Turn reject flag off again
            s_bRejectOffers = false;

            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideGenericIndustriesHandler)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                // We don't request every time so it gives time for a vehicle to be matched and dispatched before we request again.
                if (buildingData.m_fireIntensity == 0 && random.UInt32(3U) == 0)
                {
                    TransferReason primary = GetIncomingTransferReason(buildingID, buildingData.Info);
                    if (primary != TransferReason.None)
                    {
                        IndustrialBuildingAI? processingAI = buildingData.Info.GetAI() as IndustrialBuildingAI;
                        if (processingAI != null)
                        {
                            // Determine priority based on current storage level
                            int iProductionCapacity = processingAI.CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), buildingData.Width, buildingData.Length);
                            int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);

                            // Factor in trucks on the way but if timer is ticking up, ignore far away trucks
                            TransferReason secondary = GetSecondaryIncomingTransferReason(buildingID, buildingData.Info);
                            Rerequest.ProblemLevel level = Rerequest.GetLevelIncomingTimer(buildingData.m_incomingProblemTimer);
                            int iTransferSize = Rerequest.GetNearbyGuestVehiclesTransferSize(buildingData, level, primary, secondary, out int iTotalTrucks);
                            if (iTotalTrucks < 10)
                            {
                                // Calculate a more realistic priority
                                int iPriority = Mathf.Clamp((iStorageCapacity - buildingData.m_customBuffer1 - iTransferSize) * 8 / iStorageCapacity, 0, 7);

                                // We clamp priority to 6 until timer starts so we can target the buildings with notification icons first
                                if (iPriority == 7 && buildingData.m_incomingProblemTimer == 0)
                                {
                                    iPriority = 6;
                                }

                                TransferOffer offer = default;
                                offer.Priority = iPriority;
                                offer.Building = buildingID;
                                offer.Position = buildingData.m_position;
                                offer.Amount = 1;
                                offer.Active = false;

                                if (iPriority >= 3)
                                {
                                    // Add new offer
                                    // Alternate primary/secondary offer
                                    if (secondary != TransferReason.None && random.UInt32(2U) == 0)
                                    {
                                        Singleton<TransferManager>.instance.AddIncomingOffer(secondary, offer);
                                    }
                                    else
                                    {
                                        Singleton<TransferManager>.instance.AddIncomingOffer(primary, offer);
                                    }
                                }
                            }
                        }
                    }
                }

                // We override the default outgoing offer so we can factor in the problem timer value into priority
                // to ensure buildings with the flashing icon get processed first
                // We don't request every time so it gives time for a vehicle to be matched and
                // dispatched before we request again.
                TransferManager.TransferReason outgoingReason = GetOutgoingTransferReason(buildingData.Info);
                if (outgoingReason != TransferReason.None && 
                    buildingData.m_fireIntensity == 0 &&  
                    random.UInt32(2U) == 0)
                {
                    const int iMAX_VEHICLE_LOAD = 8000;

                    // Check we have vehicles available before adding offer
                    int iVehicles = BuildingUtils.GetOwnVehicleCount(buildingData, outgoingReason);
                    int iMaxVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.GenericFactory, buildingID);
                    if (iVehicles < iMaxVehicles)
                    {
                        TransferManager.TransferOffer offer = default;

                        // P:0 at storage 8
                        // P:2 at Storage 12
                        // Then scale with Outgoing timer after that
                        if (buildingData.m_customBuffer2 > 12000)
                        {
                            offer.Priority = Mathf.Clamp(2 + buildingData.m_outgoingProblemTimer * 6 / 128, 2, 7); // 128 is when the problem icon appears
                        }
                        else if (buildingData.m_customBuffer2 > iMAX_VEHICLE_LOAD)
                        {
                            offer.Priority = Mathf.Clamp(buildingData.m_outgoingProblemTimer * 8 / 128, 0, 7); // 128 is when the problem icon appears
                        }
                        else
                        {
                            offer.Priority = 0;
                        }
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = Mathf.Min(buildingData.m_customBuffer2 / iMAX_VEHICLE_LOAD, iMaxVehicles - iVehicles);
                        offer.Active = true;

                        if (offer.Amount > 0)
                        {
                            // Add our version
                            Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingReason, offer);
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

        private static TransferManager.TransferReason GetOutgoingTransferReason(BuildingInfo info)
        {
            return info.m_class.m_subService switch
            {
                ItemClass.SubService.IndustrialForestry => TransferManager.TransferReason.Lumber,
                ItemClass.SubService.IndustrialFarming => TransferManager.TransferReason.Food,
                ItemClass.SubService.IndustrialOil => TransferManager.TransferReason.Petrol,
                ItemClass.SubService.IndustrialOre => TransferManager.TransferReason.Coal,
                _ => TransferManager.TransferReason.Goods,
            };
        }
    }
}
