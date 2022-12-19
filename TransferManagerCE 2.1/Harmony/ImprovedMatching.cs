using System;
using static TransferManager;
using UnityEngine;
using ColossalFramework;

namespace TransferManagerCE.Patch
{
    internal class ImprovedMatching
    {
        public static void ImprovedDeadMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedDeathcareMatching)
            {
                Building building = Buildings[offer.Building];
                CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                if (cemeteryAI != null)
                {
                    // Determine free spots in cemetery / Crematorium
                    int iAmount;
                    int iMax;
                    cemeteryAI.GetMaterialAmount(offer.Building, ref building, TransferReason.Dead, out iAmount, out iMax);
                    int iCemeteryFree = iMax - iAmount;

                    // Get total vehicle count, Factor in budget
                    int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                    int iTotalHearses = (productionRate * cemeteryAI.m_hearseCount + 99) / 100;

                    // Determine how many free vehicles it has
                    int iHearseCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Dead);
                    int iReserveCount = Math.Max(1, (int)Math.Round((float)iTotalHearses * 0.33f)); // Reserve 1/3 of hearses so there is reserve capacity
                    int iHearsesFree = Math.Max(0, iTotalHearses - iHearseCount - iReserveCount);

                    // Set offer amount to be the number of free vehicles available.
                    if (iHearsesFree > 0 && iCemeteryFree > 0 && building.m_flags != Building.Flags.Downgrading)
                    {
                        offer.Amount = Math.Min(iHearsesFree, iCemeteryFree);
                    }

                    // Increase offer priority when less than 1/2 of vehicles are in use
                    int iHalfCount = Math.Max(1, (int)Math.Floor((float)iTotalHearses * 0.5f));
                    if (iHearseCount < iHalfCount)
                    {
                        offer.Priority = Math.Max(offer.Priority, 2);
                    }
                }
            }
        }

        public static void ImprovedGarbageMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedGarbageMatching)
            {
                Building building = Buildings[offer.Building];
                LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                if (garbageAI != null)
                {
                    // Get total vehicle count, Factor in budget
                    int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                    int iTotalTrucks = (productionRate * garbageAI.m_garbageTruckCount + 99) / 100;

                    // Determine how many free vehicles it has
                    int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Garbage);
                    int iReserveCount = Math.Max(1, (int)Math.Round((float)iTotalTrucks * 0.33f)); // Reserve 1/3 of trucks so there is reserve capacity

                    int iNewAmount = iTotalTrucks - iReserveCount - iCurrentCount;
                    if (iNewAmount > offer.Amount)
                    {
                        offer.Amount = iNewAmount;
                    }

                    // Increase offer priority when less than 1/2 of vehicles are in use
                    int iHalfCount = Math.Max(1, (int)Math.Floor((float)iTotalTrucks * 0.5f));
                    if (iCurrentCount < iHalfCount)
                    {
                        offer.Priority = Math.Max(offer.Priority, 2);
                    }
                }
            }
        }

        public static void ImprovedCrimeMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedCrimeMatching)
            {
                Building building = Buildings[offer.Building];
                PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                if (buildingAI != null)
                {
                    // Get total vehicle count, Factor in budget
                    int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                    int iTotalVehicles = (productionRate * buildingAI.m_policeCarCount + 99) / 100;

                    // Determine how many free vehicles it has
                    int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Crime);
                    int iReserveCount = Math.Max(1, (int)Math.Round((float)iTotalVehicles * 0.33f)); // Reserve 1/3 of trucks so there is reserve capacity

                    int iNewAmount = iTotalVehicles - iReserveCount - iCurrentCount;
                    if (iNewAmount > offer.Amount)
                    {
                        offer.Amount = iNewAmount;
                    }

                    // Increase offer priority when less than 1/2 of vehicles are in use
                    int iHalfCount = Math.Max(1, (int)Math.Floor((float)iTotalVehicles * 0.5f));
                    if (iCurrentCount < iHalfCount)
                    {
                        offer.Priority = Math.Max(offer.Priority, 2);
                    }
                }
            }
        }

        public static void ImprovedSickMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().OverrideResidentialSickHandler)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0)
                {
                    HospitalAI? buildingAI = building.Info?.m_buildingAI as HospitalAI;
                    if (buildingAI != null)
                    {
                        // Get total vehicle count, Factor in budget
                        int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Hospital, offer.Building);

                        // Determine how many free vehicles it has
                        int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Sick);

                        // Lower offer priority when more than 1/2 of vehicles are in use
                        int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5));
                        if (iCurrentCount >= iHalfCount)
                        {
                            offer.Priority = 0;
                        }
                    }
                }
            }
        }

        public static void ImprovedWarehouseMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Exclude && BuildingSettingsStorage.GetSettings(offer.Building).IsImprovedWarehouseMatching())
            {
                // It's a warehouse, set the priority based on storage level
                Building building = Buildings[offer.Building];

                // If warehouse mode is "Empty" then the Priority is already 0, we don't want to change this
                if (building.m_flags != 0 && (building.m_flags & Building.Flags.Downgrading) == 0)
                {
                    if (building.Info != null)
                    {
                        WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                        if (warehouse != null)
                        {
                            // Update priority based on storage level
                            // For incoming we use the storage buffer and incoming supply
                            TransferReason actualTransferReason = warehouse.GetActualTransferReason(offer.Building, ref building);
                            int iTransferSize = CitiesUtils.GetGuestVehiclesTransferSize(offer.Building, actualTransferReason);
                            float fStorage = building.m_customBuffer1 * 0.1f + (float)iTransferSize * 0.001f;
                            float fCapacity = warehouse.m_storageCapacity * 0.001f;
                            if (fCapacity > 0.0f)
                            {
                                // We want priority 0 for the top 20%
                                float fEmptyPercent = 1.0f - (fStorage / (fCapacity * 0.8f));

                                // This will set priority to 7 when emtpy and 0 when completely full.
                                offer.Priority = Mathf.Clamp((int)(fEmptyPercent * 7.0f), 0, 7);
                            }
                        }
                    }
                }
            }
        }

        public static void ImprovedWarehouseMatchingOutgoing(ref TransferOffer offer)
        {
            if (offer.Exclude)
            {
                // It's a warehouse, set the priority based on storage level
                int iReservePercent = BuildingSettingsStorage.GetSettings(offer.Building).ReserveCargoTrucksPercent();
                if (iReservePercent > 0 || SaveGameSettings.GetSettings().ImprovedWarehouseMatching)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];

                    // If warehouse mode is Full then OUT priority will already be 0. We don't need to change this
                    if (building.m_flags != 0 && building.Info != null && (building.m_flags & Building.Flags.Upgrading) == 0)
                    {
                        WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                        if (warehouse != null)
                        {
                            // Limit priority based on available truck count
                            if (iReservePercent > 0)
                            {
                                // Max vehicle count
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                int iTotalVehicles = (productionRate * warehouse.m_truckCount + 99) / 100;

                                // Determine how many free vehicles it has
                                TransferReason actualTransferReason = warehouse.GetActualTransferReason(offer.Building, ref building);
                                int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, actualTransferReason);

                                // Lower offer priority when 70% of vehicles are in use
                                int iLimitCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * (1.0f - (float)(iReservePercent * 0.01))));
                                if (iCurrentCount >= iLimitCount)
                                {
                                    // Lower priority as we are running out of vehicles
                                    offer.Priority = 0;
                                    return;
                                }
                            }

                            // Update priority based on storage level
                            if (BuildingSettingsStorage.GetSettings(offer.Building).IsImprovedWarehouseMatching())
                            {
                                // Check warehouse mode
                                int iMinPriority = 0;
                                if ((building.m_flags & Building.Flags.Downgrading) != 0)
                                {
                                    // Warehouse is set to Empty mode, minimum priority is 2 in this case
                                    iMinPriority = 2;
                                }

                                float fStorage = building.m_customBuffer1 * 0.1f;
                                float fCapacity = warehouse.m_storageCapacity * 0.001f;
                                if (fCapacity > 0.0f)
                                {
                                    // We want priority 0 for the bottom 20%
                                    float fInPercent = (fStorage - fCapacity * 0.2f) / (fCapacity * 0.8f);

                                    // This will set priority to 0 when empty and 7 when completely full.
                                    offer.Priority = Mathf.Clamp((int)(fInPercent * 7.0f), iMinPriority, 7);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
