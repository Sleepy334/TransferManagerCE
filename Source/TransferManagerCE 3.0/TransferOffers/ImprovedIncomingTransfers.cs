﻿using System;
using UnityEngine;
using ColossalFramework;
using TransferManagerCE.Settings;
using static TransferManager;
using static TransferManagerCE.WarehouseUtils;
using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    internal class ImprovedIncomingTransfers
    {
        // Return false to skip adding of offer.
        public static bool HandleOffer(TransferReason material, ref TransferOffer offer)
        {
            // Currently warehouse offers are the only ones that set the Exclude flag
            if (offer.Exclude)
            {
                // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                return ImprovedWarehouseMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
            }

            switch (material)
            {
                case TransferReason.Dead:
                    {
                        return HandleIncomingDeadOffer(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                    }
                case TransferReason.Garbage:
                    {
                        return HandleIncomingGarbageOffer(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                    }
                case TransferReason.Crime:
                    {
                        return HandleIncomingCrimeOffer(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                    }
                case TransferReason.Sick:
                    {
                        return HandleIncomingSickOffer(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                    }
                case TransferReason.Mail:
                    {
                        return HandleIncomingMailOffer(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                    }
                case TransferReason.Taxi:
                    {
                        return HandleIncomingTaxiOffer(ref offer);
                    }
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                    {
                        return HandleIncomingCommercialOffer(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                    }
            }
                
            // Add offer normally
            return true;
        }

        private static bool HandleIncomingDeadOffer(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedDeathcareMatching)
            {
                Building building = Buildings[offer.Building];
                CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                if (cemeteryAI is not null)
                {
                    // Determine free spots in cemetery / Crematorium
                    int iAmount;
                    int iMax;
                    cemeteryAI.GetMaterialAmount(offer.Building, ref building, TransferReason.Dead, out iAmount, out iMax);
                    int iCemeteryFree = iMax - iAmount;

                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Cemetery, offer.Building, building, 0);

                    // Determine how many free vehicles it has
                    int iHearseCount = BuildingUtils.GetOwnVehicleCount(building, TransferReason.Dead);
                    int iReserveCount = Math.Max(1, (int)Math.Round(iTotalVehicles * 0.33f)); // Reserve 1/3 of hearses so there is reserve capacity
                    int iHearsesFree = Math.Max(0, iTotalVehicles - iHearseCount - iReserveCount);

                    // Set offer amount to be the number of free vehicles available.
                    if (iHearsesFree > 0 && iCemeteryFree > 0 && building.m_flags != Building.Flags.Downgrading)
                    {
                        offer.Amount = Math.Min(iHearsesFree, iCemeteryFree);
                    }

                    // Increase offer priority when less than 1/2 of vehicles are in use
                    int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5f));
                    if (iHearseCount < iHalfCount)
                    {
                        offer.Priority = Math.Max(offer.Priority, 2);
                    }
                }
            }

            return true;
        }

        private static bool HandleIncomingGarbageOffer(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedGarbageMatching)
            {
                Building building = Buildings[offer.Building];
                LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                if (garbageAI is not null)
                {
                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Landfill, offer.Building, building, 0);

                    // Determine how many free vehicles it has
                    int iCurrentCount = BuildingUtils.GetOwnVehicleCount(building, TransferReason.Garbage);
                    int iReserveCount = Math.Max(1, (int)Math.Round(iTotalVehicles * 0.33f)); // Reserve 1/3 of trucks so there is reserve capacity

                    int iNewAmount = iTotalVehicles - iReserveCount - iCurrentCount;
                    if (iNewAmount > offer.Amount)
                    {
                        offer.Amount = iNewAmount;
                    }

                    // Increase offer priority when less than 1/2 of vehicles are in use
                    int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5f));
                    if (iCurrentCount < iHalfCount)
                    {
                        offer.Priority = Math.Max(offer.Priority, 2);
                    }
                }
            }

            return true;
        }

        private static bool HandleIncomingCrimeOffer(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedCrimeMatching)
            {
                Building building = Buildings[offer.Building];
                PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                if (buildingAI is not null)
                {
                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.PoliceStation, offer.Building, building, 0);

                    // Determine how many free vehicles it has
                    int iCurrentCount = BuildingUtils.GetOwnVehicleCount(building, TransferReason.Crime);
                    int iReserveCount = Math.Max(1, (int)Math.Round(iTotalVehicles * 0.33f)); // Reserve 1/3 of trucks so there is reserve capacity

                    int iNewAmount = iTotalVehicles - iReserveCount - iCurrentCount;
                    if (iNewAmount > offer.Amount)
                    {
                        offer.Amount = iNewAmount;
                    }

                    // Increase offer priority when less than 1/2 of vehicles are in use
                    int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5f));
                    if (iCurrentCount < iHalfCount)
                    {
                        offer.Priority = Math.Max(offer.Priority, 2);
                    }
                }
            }

            return true;
        }

        private static bool HandleIncomingSickOffer(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0 && building.Info != null && building.Info.GetService() == ItemClass.Service.HealthCare)
                {
                    BuildingTypeHelper.BuildingType eBuildingType = BuildingTypeHelper.GetBuildingType(building);
                    switch (eBuildingType)
                    {
                        case BuildingTypeHelper.BuildingType.Hospital:
                        case BuildingTypeHelper.BuildingType.UniversityHospital:
                            {
                                // Get total vehicle count, Factor in budget
                                int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(eBuildingType, offer.Building, building, 0);

                                // Determine how many free vehicles it has
                                int iCurrentCount = BuildingUtils.GetOwnVehicleCount(building, TransferReason.Sick);

                                // Lower offer priority when more than 1/2 of vehicles are in use
                                int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5));
                                if (iCurrentCount >= iHalfCount)
                                {
                                    offer.Priority = 0;
                                }
                            }
                            break;
                    }
                    
                }
            }

            return true;
        }

        private static bool HandleIncomingMailOffer(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedMailTransfers)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0 && building.Info is not null)
                {
                    PostOfficeAI? buildingAI = building.Info.m_buildingAI as PostOfficeAI;
                    if (buildingAI is not null)
                    {
                        // Get total vehicle count, Factor in budget
                        int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                        int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                        int iTotalVehicles = (productionRate * buildingAI.m_postVanCount + 99) / 100;

                        // Determine how many free vehicles it has
                        int iCurrentCount = BuildingUtils.GetOwnVehicleCount(building, TransferReason.Mail);

                        // Increase offer priority when less than 1/2 of vehicles are in use
                        int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5f));
                        if (iCurrentCount < iHalfCount)
                        {
                            offer.Priority = Math.Max(offer.Priority, 2);
                        }
                    }
                }
            }

            return true; // Add normally
        }

        // We cap commercial building priority to 6 until the incoming timer starts
        // so that we can ensure we are matching the most urgent offers first
        private static bool HandleIncomingCommercialOffer(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Priority == 7 && offer.Building != 0)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0 &&
                    building.m_incomingProblemTimer == 0 &&
                    building.Info is not null &&
                    building.Info.GetService() == ItemClass.Service.Commercial)
                {
                    // Cap priority to 6.
                    offer.Priority = 6;
                }
            }

            return true; // Add offer normally
        }

        // Return false to skip adding of offer.
        private static bool HandleIncomingTaxiOffer(ref TransferOffer offer)
        {
            if (offer.Citizen != 0)
            {
                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                if (citizen.m_flags != 0 && citizen.m_instance != 0)
                {
                    ref CitizenInstance instance = ref CitizenManager.instance.m_instances.m_buffer[citizen.m_instance];
                    if (instance.m_flags != 0)
                    {
                        if (instance.m_sourceBuilding != 0 && BuildingTypeHelper.IsOutsideConnection(instance.m_sourceBuilding))
                        {
                            // Taxi's do not work when cims coming from outside connections
                            //CDebug.Log($"Citizen: {offer.Citizen} {citizen.m_flags} Waiting for taxi at outside connection {instance.m_sourceBuilding} - SKIPPING");

                            // Speed up waiting
                            if (instance.m_waitCounter > 0)
                            {
                                instance.m_waitCounter = (byte)Math.Max((int)instance.m_waitCounter, 254);
                            }

                            return false; // Skip this offer
                        }
                    }
                }
            }

            return true; // Add offer normally
        }
                            
        private static bool ImprovedWarehouseMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            // If its a cargo warehouse offer then we need to get the parent warehouse
            ushort buildingId = WarehouseUtils.GetWarehouseBuildingId(offer.Building);

            Building building = Buildings[buildingId];
            if (building.m_flags != 0 && building.Info is not null)
            {
                WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                if (warehouse is not null)
                {
                    // It's a warehouse, set the priority based on storage level
                    if (SaveGameSettings.GetSettings().ImprovedWarehouseMatching)
                    {
                        // Get warehouse mode.
                        WarehouseMode mode = WarehouseUtils.GetWarehouseMode(building);

                        // If warehouse mode is "Empty" then the Priority is already 0, we don't want to change this
                        if (mode != WarehouseMode.Empty)
                        {
                            // Update priority based on storage level
                            // For incoming we use the storage buffer and incoming supply
                            TransferReason actualTransferReason = warehouse.GetActualTransferReason(buildingId, ref building);
                            if (actualTransferReason != TransferReason.None)
                            {
                                float fCapacity = warehouse.m_storageCapacity * 0.001f;
                                if (fCapacity > 0.0f)
                                {
                                    // Check warehouse mode
                                    int iMinPriority = 0;
                                    if (mode == WarehouseMode.Fill)
                                    {
                                        // Warehouse is set to Fill mode, minimum priority is 2 in this case
                                        iMinPriority = 2;
                                    }

                                    int iTransferSize = BuildingUtils.GetGuestVehiclesTransferSize(buildingId, actualTransferReason);
                                    float fStorage = building.m_customBuffer1 * 0.1f + iTransferSize * 0.001f;
                                    float fPercent = fStorage / fCapacity;

                                    if (fPercent <= 0.20f)
                                    {
                                        // We want P:7 for the bottom 20%
                                        offer.Priority = 7;
                                    }
                                    else if (fPercent >= 0.80f)
                                    {
                                        // We want iMinPriority for the top 20%
                                        offer.Priority = iMinPriority;
                                    }
                                    else
                                    {
                                        // Otherwise scale priority based on storage level
                                        offer.Priority = Mathf.Clamp((int)((1.0f - fPercent) * 8.0), iMinPriority, 7);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }


    }
}
