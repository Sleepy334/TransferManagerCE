using System;
using static TransferManager;
using UnityEngine;
using ColossalFramework;
using TransferManagerCE.Settings;
using static TransferManagerCE.CitiesUtils;
using static TransferManagerCE.WarehouseUtils;
using System.Reflection;
using ICities;

namespace TransferManagerCE
{
    internal class ImprovedTransfers
    {
        private static int? s_maxLoadSize = null;

        // Return false to skip adding of offer.
        public static bool HandleIncomingOffer(TransferReason material, ref TransferOffer offer)
        {
            if (offer.Exclude)
            {
                // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                ImprovedWarehouseMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
            }
            else
            {
                switch (material)
                {
                    case TransferReason.Dead:
                        {
                            ImprovedDeadMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                            break;
                        }
                    case TransferReason.Garbage:
                        {
                            ImprovedGarbageMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                            break;
                        }
                    case TransferReason.Crime:
                        {
                            ImprovedCrimeMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                            break;
                        }
                    case TransferReason.Sick:
                        {
                            ImprovedSickMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                            break;
                        }
                    case TransferReason.Mail:
                        {
                            ImprovedMailMatchingIncoming(BuildingManager.instance.m_buildings.m_buffer, ref offer);
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
                            HandleCommercialOffers(BuildingManager.instance.m_buildings.m_buffer, ref offer);
                            break;
                        }
                    case TransferReason.Worker0:
                    case TransferReason.Worker1:
                    case TransferReason.Worker2:
                    case TransferReason.Worker3:
                        {
                            if (offer.Priority < 7 && offer.Building != 0)
                            {
                                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                                if (building.m_flags != 0 && building.m_workerProblemTimer > 0)
                                {
                                    // Worker problem timer is running set priority to 7
                                    offer.Priority = 7;
                                }
                            }
                            break;
                        }
                }
            }
                
            // Add offer normally
            return true;
        }

        private static void ImprovedDeadMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
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
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Cemetery, offer.Building, building);

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
        }

        private static void ImprovedGarbageMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedGarbageMatching)
            {
                Building building = Buildings[offer.Building];
                LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                if (garbageAI is not null)
                {
                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Landfill, offer.Building, building);

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
        }

        private static void ImprovedCrimeMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().ImprovedCrimeMatching)
            {
                Building building = Buildings[offer.Building];
                PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                if (buildingAI is not null)
                {
                    // Get total vehicle count, Factor in budget
                    int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.PoliceStation, offer.Building, building);

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
        }

        private static void ImprovedSickMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            if (offer.Building != 0 && SaveGameSettings.GetSettings().OverrideResidentialSickHandler)
            {
                Building building = Buildings[offer.Building];
                if (building.m_flags != 0)
                {
                    HospitalAI? buildingAI = building.Info?.m_buildingAI as HospitalAI;
                    if (buildingAI is not null)
                    {
                        // Get total vehicle count, Factor in budget
                        int iTotalVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.Hospital, offer.Building, building);

                        // Determine how many free vehicles it has
                        int iCurrentCount = BuildingUtils.GetOwnVehicleCount(building, TransferReason.Sick);

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
                    building.Info is not null &&
                    building.Info.GetService() == ItemClass.Service.Commercial)
                {
                    // Cap priority to 6.
                    offer.Priority = 6;
                }
            }
        }

        private static void ImprovedWarehouseMatchingIncoming(Building[] Buildings, ref TransferOffer offer)
        {
            // Check it is actually a warehouse
            if (!offer.Exclude)
            {
                return;
            }

            Building building = Buildings[offer.Building];
            if (building.m_flags != 0 && building.Info is not null)
            {
                WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                if (warehouse is not null)
                {
                    // The H&T patch introduced a bad bug for the incoming amount calculation, this restores the old version
                    // We patch the offer amount always
                    int buffer = building.m_customBuffer1 * 100;
                    int maxLoadSize = GetMaxLoadSize(warehouse);
                    offer.Amount = Mathf.Max(1, (warehouse.m_storageCapacity - buffer) / Mathf.Max(1, maxLoadSize));

                    // It's a warehouse, set the priority based on storage level
                    if (BuildingSettingsFast.IsImprovedWarehouseMatching(offer.Building))
                    {
                        // Get warehouse mode.
                        WarehouseMode mode = WarehouseUtils.GetWarehouseMode(building);

                        // If warehouse mode is "Empty" then the Priority is already 0, we don't want to change this
                        if (mode != WarehouseMode.Empty)
                        {
                            // Update priority based on storage level
                            // For incoming we use the storage buffer and incoming supply
                            TransferReason actualTransferReason = warehouse.GetActualTransferReason(offer.Building, ref building);
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

                                    int iTransferSize = BuildingUtils.GetGuestVehiclesTransferSize(offer.Building, actualTransferReason);
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
        }

        private static int GetMaxLoadSize(WarehouseAI instance)
        {
            if (s_maxLoadSize == null)
            {
                // We try to call WarehouseAI.GetMaxLoadSize as some mods such as Industry Rebalanced modify this value
                // Unfortunately it is private so we need to use reflection, so we cache the results.
                MethodInfo getMaxLoadSize = typeof(WarehouseAI).GetMethod("GetMaxLoadSize", BindingFlags.Instance | BindingFlags.NonPublic);
                if (getMaxLoadSize != null)
                {
                    s_maxLoadSize = (int)getMaxLoadSize.Invoke(instance, null);
                }
                else
                {
                    // Fall back on default if we fail to get the function
                    s_maxLoadSize = 8000; 
                }
            }

            return s_maxLoadSize.Value;
        }

        public static void ImprovedWarehouseMatchingOutgoing(ref TransferOffer offer)
        {
            if (offer.Exclude)
            {
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
                        if (BuildingSettingsFast.IsImprovedWarehouseMatching(offer.Building))
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
}
