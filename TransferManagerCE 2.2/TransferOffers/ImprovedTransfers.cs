using System;
using static TransferManager;
using UnityEngine;
using ColossalFramework;

namespace TransferManagerCE
{
    internal class ImprovedTransfers
    {
        public static bool HandleIncomingOffer(TransferReason material, ref TransferOffer offer)
        {
            Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;

            switch (material)
            {
                case TransferReason.Dead:
                    {
                        ImprovedDeadMatchingIncoming(Buildings, ref offer);
                        break;
                    }
                case TransferReason.Garbage:
                    {
                        ImprovedGarbageMatchingIncoming(Buildings, ref offer);
                        break;
                    }
                case TransferReason.Crime:
                    {
                        ImprovedCrimeMatchingIncoming(Buildings, ref offer);
                        break;
                    }
                case TransferReason.Sick:
                    {
                        ImprovedSickMatchingIncoming(Buildings, ref offer);
                        break;
                    }
                case TransferReason.Mail:
                    {
                        ImprovedMailMatchingIncoming(Buildings, ref offer);
                        break;
                    }
                case TransferReason.Taxi:
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
                                        //Debug.Log($"Citizen: {offer.Citizen} Waiting for taxi at outside connection {instance.m_sourceBuilding} - SKIPPING");

                                        // Speed up waiting
                                        if (instance.m_waitCounter > 0)
                                        {
                                            instance.m_waitCounter = (byte)Math.Max((int)instance.m_waitCounter, 254);
                                        }

                                        // We return false so we don't add offer to match set
                                        return false;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                    {
                        HandleCommercialOffers(Buildings, ref offer); 
                        break;
                    }
            }

            // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
            ImprovedWarehouseMatchingIncoming(Buildings, ref offer);

            // Add offer normally
            return true;
        }

        private static void ImprovedDeadMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
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
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Cemetery, offer.Building, building);

                    // Determine how many free vehicles it has
                    int iHearseCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Dead);
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
        }

        private static void ImprovedGarbageMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedGarbageMatching)
            {
                Building building = Buildings[offer.Building];
                LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                if (garbageAI != null)
                {
                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Landfill, offer.Building, building);

                    // Determine how many free vehicles it has
                    int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Garbage);
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
        }

        private static void ImprovedCrimeMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedCrimeMatching)
            {
                Building building = Buildings[offer.Building];
                PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                if (buildingAI != null)
                {
                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.PoliceStation, offer.Building, building);

                    // Determine how many free vehicles it has
                    int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Crime);
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
        }

        private static void ImprovedSickMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
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
                        int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Hospital, offer.Building, building);

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

        private static void ImprovedMailMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedMailTransfers)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0 && building.Info != null)
                {
                    PostOfficeAI? buildingAI = building.Info.m_buildingAI as PostOfficeAI;
                    if (buildingAI != null)
                    {
                        // Get total vehicle count, Factor in budget
                        int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                        int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                        int iTotalVehicles = (productionRate * buildingAI.m_postVanCount + 99) / 100;

                        // Determine how many free vehicles it has
                        int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Mail);

                        // Increase offer priority when less than 1/2 of vehicles are in use
                        int iHalfCount = Math.Max(1, (int)Math.Floor(iTotalVehicles * 0.5f));
                        if (iCurrentCount < iHalfCount)
                        {
                            offer.Priority = Math.Max(offer.Priority, 2);
                        }
                    }
                }
            }
        }

        // We cap commercial building priority to 6 until the incoming timer starts
        // so that we can ensure we are matching the most urgent offers first
        private static void HandleCommercialOffers(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Priority == 7 && offer.Building != 0)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0 &&
                    building.m_incomingProblemTimer == 0 &&
                    building.Info != null &&
                    building.Info.GetService() == ItemClass.Service.Commercial)
                {
                    // Cap priority to 6.
                    offer.Priority = 6;
                }
            }
        }

        private static void ImprovedWarehouseMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
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
                            if (actualTransferReason != TransferReason.None)
                            {
                                int iTransferSize = CitiesUtils.GetGuestVehiclesTransferSize(offer.Building, actualTransferReason);
                                float fStorage = building.m_customBuffer1 * 0.1f + iTransferSize * 0.001f;
                                float fCapacity = warehouse.m_storageCapacity * 0.001f;
                                if (fCapacity > 0.0f)
                                {
                                    // We want priority 0 for the top 20%
                                    float fEmptyPercent = 1.0f - fStorage / (fCapacity * 0.8f);

                                    // This will set priority to 7 when emtpy and 0 when completely full.
                                    offer.Priority = Mathf.Clamp((int)(fEmptyPercent * 8.0f), 0, 7);
                                }
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

                                int iStorage = (int)(building.m_customBuffer1 * 0.1f);
                                int iCapacity = (int)(warehouse.m_storageCapacity * 0.001f);
                                if (iCapacity > 0)
                                {
                                    // We want priority 0 for the bottom 20%
                                    float fInPercent = ((float)iStorage - (float)iCapacity * 0.2f) / ((float)iCapacity * 0.8f);

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
