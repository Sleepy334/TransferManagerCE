using System;
using UnityEngine;
using ColossalFramework;
using TransferManagerCE.Settings;
using static TransferManager;
using static TransferManagerCE.WarehouseUtils;
using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    internal class ImprovedOutgoingTransfers
    {
        // ----------------------------------------------------------------------------------------
        // Return false to skip adding of offer.
        public static bool HandleOffer(ref TransferReason material, ref TransferOffer offer)
        {
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            // Warehouse offer
            if (offer.Exclude)
            {
                // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                ImprovedWarehouseMatchingOutgoing(ref offer);
                return true;
            }

            // NOTE: OUT offer from outside connection is an Import into the city!
            if (IsGlobalImportDisabled(settings, (CustomTransferReason.Reason) material, offer))
            {
                return false; // Don't add this offer as Import is completely restricted
            }

            switch (material)
            {
                case TransferReason.Taxi:
                    {
                        if (settings.TaxiMove && offer.Building != 0)
                        {
                            HandleOutgoingTaxiOffer(ref material, offer);
                        }
                        break;
                    }
                case TransferReason.Mail:
                    {
                        HandleOutgoingMailOffer(ref material, offer);
                        break;
                    }
            }

            // Add offer normally
            return true;
        }

        // ----------------------------------------------------------------------------------------
        // Check if import is completely disabled at the global level and don't add offer here
        private static bool IsGlobalImportDisabled(SaveGameSettings settings, CustomTransferReason.Reason material, TransferOffer offer)
        {
            return offer.Building != 0 &&
                    OutsideConnectionAIPatch.IsInAddConnectionOffers &&
                    TransferManagerModes.IsImportRestrictionsSupported(material) &&
                    settings.IsWarehouseImportRestricted(material) &&
                    settings.IsImportRestricted(material);
        }

        // ----------------------------------------------------------------------------------------
        private static void HandleOutgoingTaxiOffer(ref TransferReason material, TransferOffer offer)
        {
            // Check it is a Taxi Depot
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building];
            if (building.m_flags != 0 && building.Info is not null && building.Info.GetAI() is DepotAI)
            {
                // We occasionally allow a Taxi call from the depot so that they will still work if you don't have any taxi stands.
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) != 0)
                {
                    material = (TransferReason)CustomTransferReason.Reason.TaxiMove;
                }
                else
                {
                    offer.Amount = 1; // Only allow 1 taxi at a time to match so we keep some back for taxi stands
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private static void HandleOutgoingMailOffer(ref TransferReason material, TransferOffer offer)
        {
            if (offer.Park != 0 && offer.m_isLocalPark == 0)
            {
                // This is a service point request, convert it to Mail2.
                material = (TransferReason)CustomTransferReason.Reason.Mail2;
            }
            else if (offer.Building != 0 && SaveGameSettings.GetSettings().MainBuildingPostTruck)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                if (building.m_flags != 0 && building.Info is not null)
                {
                    switch (building.Info.GetAI())
                    {
                        case ParkGateAI:
                        case AirportEntranceAI:
                        case MainIndustryBuildingAI:
                        case MainCampusBuildingAI:
                            {
                                // We alternate Mail and Mail2 requests so we can collect mail with post trucks for these building types
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) != 0)
                                {
                                    material = (TransferReason) CustomTransferReason.Reason.Mail2;
                                }
                                break;
                            }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        public static void ImprovedWarehouseMatchingOutgoing(ref TransferOffer offer)
        {
            // Check it is actually a warehouse
            if (!offer.Exclude)
            {
                return;
            }

            // It's a warehouse, set the priority based on storage level
            Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];

            // Get warehouse mode
            WarehouseMode mode = WarehouseUtils.GetWarehouseMode(building);

            // If warehouse mode is Fill then OUT priority will already be 0. We don't need to change this
            if (building.m_flags != 0 && building.Info is not null && mode != WarehouseMode.Fill)
            {
                WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                if (warehouse is not null)
                {
                    // Limit priority based on available truck count
                    int iReservePercent = BuildingSettingsFast.ReserveCargoTrucksPercent(offer.Building);
                    if (iReservePercent > 0)
                    {
                        // Max vehicle count
                        int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                        int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                        int iTotalVehicles = (productionRate * warehouse.m_truckCount + 99) / 100;

                        // Determine how many free vehicles it has
                        TransferReason actualTransferReason = warehouse.GetActualTransferReason(offer.Building, ref building);
                        int iCurrentCount = BuildingUtils.GetOwnVehicleCount(building, actualTransferReason);

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
                    if (SaveGameSettings.GetSettings().ImprovedWarehouseMatching)
                    {
                        // Check warehouse mode
                        int iMinPriority = 0;
                        if (mode == WarehouseMode.Empty)
                        {
                            // Warehouse is set to Empty mode, minimum priority is 2 in this case
                            iMinPriority = 2;
                        }

                        int iCapacity = (int)(warehouse.m_storageCapacity * 0.001f);
                        if (iCapacity > 0)
                        {
                            int iStorage = (int)(building.m_customBuffer1 * 0.1f);
                            float fInPercent = (float)iStorage / (float)iCapacity;
                            if (fInPercent <= 0.20f)
                            {
                                // We want iMinPriority for the bottom 20%
                                offer.Priority = iMinPriority;
                            }
                            else if (fInPercent >= 0.80f)
                            {
                                // We want priority 7 for the top 20%
                                offer.Priority = 7;
                            }
                            else
                            {
                                // Otherwise scale priority based on storage level
                                offer.Priority = Mathf.Clamp((int)(fInPercent * 8.0f), iMinPriority, 7);
                            }
                        }
                    }
                }
            }
        }
    }
}
