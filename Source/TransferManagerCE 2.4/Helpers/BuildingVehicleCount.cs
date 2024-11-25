using ColossalFramework;
using ColossalFramework.Math;
using Epic.OnlineServices.Presence;
using System;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class BuildingVehicleCount
    {
        public static int GetMaxVehicleCount(BuildingType eBuildingType, ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return GetMaxVehicleCount(eBuildingType, buildingId, building);
        }

        public static int GetMaxVehicleCount(BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Add vehicle count if generic processing as the vanilla form doesn't show vehicle count
            switch (eBuildingType)
            {
                case BuildingType.GenericExtractor:
                    {
                        if (building.m_flags != 0)
                        {
                            IndustrialExtractorAI? buildingAI = building.Info.GetAI() as IndustrialExtractorAI;
                            if (buildingAI is not null)
                            {
                                int productionCapacity = buildingAI.CalculateProductionCapacity((ItemClass.Level)building.m_level, new Randomizer(buildingId), building.Width, building.Length);
                                int maxVehicles = Mathf.Max(1, productionCapacity / 6);
                                return maxVehicles;
                            }
                        }
                        break;
                    }
                case BuildingType.GenericProcessing:
                case BuildingType.GenericFactory:
                    {
                        if (building.m_flags != 0)
                        {
                            IndustrialBuildingAI? buildingAI = building.Info.GetAI() as IndustrialBuildingAI;
                            if (buildingAI is not null)
                            {
                                int productionCapacity = buildingAI.CalculateProductionCapacity((ItemClass.Level)building.m_level, new Randomizer(buildingId), building.Width, building.Length);
                                int maxVehicles = Mathf.Max(1, productionCapacity / 6);
                                return maxVehicles;
                            }
                        }
                        break;
                    }
                case BuildingType.Cemetery:
                    {
                        CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                        if (cemeteryAI is not null)
                        {
                            // Factor in budget
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * cemeteryAI.m_hearseCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.Prison:
                case BuildingType.PoliceStation:
                    {
                        PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                        if (buildingAI is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.PoliceCarCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.Bank:
                    {
                        BankOfficeAI? buildingAI = building.Info.m_buildingAI as BankOfficeAI;
                        if (buildingAI is not null)
                        {
                            return buildingAI.CollectingVanCount;
                        }
                        break;
                    }
                case BuildingType.FireStation:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            FireStationAI? buildingAI = building.Info.m_buildingAI as FireStationAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_fireTruckCount + 99) / 100;
                            }
                        }
                        break;
                    }
                case BuildingType.Hospital:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            HospitalAI? buildingAI = building.Info?.GetAI() as HospitalAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.AmbulanceCount + 99) / 100;
                            }
                        }
                        break;
                    }
                //case BuildingType.Recycling: This type has cargo vehicles as well so is more complex
                //case BuildingType.WasteProcessing: This type has cargo vehicles as well so is more complex
                case BuildingType.WasteTransfer:
                case BuildingType.Landfill:
                case BuildingType.IncinerationPlant:
                    {
                        LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                        if (garbageAI is not null)
                        {
                            // Factor in budget
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * garbageAI.m_garbageTruckCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.WarehouseStation:
                case BuildingType.Warehouse:
                    {
                        WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                        if (warehouseAI is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * warehouseAI.m_truckCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.UniqueFactory:
                case BuildingType.FishFactory:
                case BuildingType.ProcessingFacility:
                    {
                        ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                        if (buildingAI is not null && building.Info is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                        }
                        break;
                    }
                /* Tricky due to boats and trucks
                case BuildingType.FishExtractor:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        FishingHarborAI? buildingAI = building.Info?.m_buildingAI as FishingHarborAI;
                        if (buildingAI is not null && building.Info is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                        }
                        break;
                    }
                */
                case BuildingType.FishFarm:
                    {
                        FishFarmAI? buildingAI = building.Info?.m_buildingAI as FishFarmAI;
                        if (buildingAI is not null && building.Info is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.ExtractionFacility:
                    {
                        ExtractingFacilityAI? buildingAI = building.Info?.m_buildingAI as ExtractingFacilityAI;
                        if (buildingAI is not null && building.Info is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.ParkMaintenanceDepot:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            MaintenanceDepotAI? buildingAI = building.Info.m_buildingAI as MaintenanceDepotAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);

                                // Park maintenance adjusts the budget
                                DistrictManager instance = Singleton<DistrictManager>.instance;
                                byte district = instance.GetDistrict(building.m_position);
                                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
                                if ((servicePolicies & DistrictPolicies.Services.ParkMaintenanceBoost) != 0)
                                {
                                    productionRate *= 2;
                                }

                                return (productionRate * buildingAI.m_maintenanceTruckCount + 99) / 100;
                            }
                        }
                        break;
                    }
                case BuildingType.RoadMaintenanceDepot:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            MaintenanceDepotAI? buildingAI = building.Info.m_buildingAI as MaintenanceDepotAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_maintenanceTruckCount + 99) / 100;
                            }
                        }
                        break;
                    }
                case BuildingType.PoliceHelicopterDepot:
                case BuildingType.MedicalHelicopterDepot:
                case BuildingType.FireHelicopterDepot:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            HelicopterDepotAI? buildingAI = building.Info.m_buildingAI as HelicopterDepotAI;
                            if (buildingAI is not null && buildingAI.m_helicopterCount < 16384)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_helicopterCount + 99) / 100;
                            }
                        }
                        break;
                    }
                case BuildingType.SnowDump:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            SnowDumpAI? buildingAI = building.Info.m_buildingAI as SnowDumpAI;
                            if (buildingAI is not null && buildingAI.m_snowTruckCount < 16384)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_snowTruckCount + 99) / 100;
                            }
                        }
                        break;
                    }
            }

            return 0;
        }
    }
}