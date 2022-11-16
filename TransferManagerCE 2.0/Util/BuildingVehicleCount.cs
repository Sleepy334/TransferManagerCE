using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class BuildingVehicleCount
    {
        public static int GetMaxVehicleCount(BuildingType eBuildingType, ushort buildingId)
        {
            // Add vehicle count if generic processing as the vanilla form doesn't show vehicle count
            switch (eBuildingType)
            {
                case BuildingType.GenericProcessing:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        if (building.m_flags != 0)
                        {
                            IndustrialBuildingAI? buildingAI = building.Info.GetAI() as IndustrialBuildingAI;
                            if (buildingAI != null)
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
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                        if (cemeteryAI != null)
                        {
                            // Factor in budget
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * cemeteryAI.m_hearseCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.PoliceStation:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                        if (buildingAI != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_policeCarCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.FireStation:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        FireStationAI? buildingAI = building.Info.m_buildingAI as FireStationAI;
                        if (buildingAI != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_fireTruckCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.Hospital:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        HospitalAI? buildingAI = building.Info.m_buildingAI as HospitalAI;
                        if (buildingAI != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_ambulanceCount + 99) / 100;
                        }
                        break;
                    }
                //case BuildingType.Recycling: This type has cargo vehicles as well so is more complex
                //case BuildingType.WasteProcessing: This type has cargo vehicles as well so is more complex
                case BuildingType.WasteTransfer:
                case BuildingType.Landfill:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                        if (garbageAI != null)
                        {
                            // Factor in budget
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * garbageAI.m_garbageTruckCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.Warehouse:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                        if (warehouseAI != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * warehouseAI.m_truckCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.FishFactory:
                case BuildingType.ProcessingFacility:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                        if (buildingAI != null && building.Info != null)
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
                        if (buildingAI != null && building.Info != null)
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
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        FishFarmAI? buildingAI = building.Info?.m_buildingAI as FishFarmAI;
                        if (buildingAI != null && building.Info != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.ExtractionFacility:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        ExtractingFacilityAI? buildingAI = building.Info?.m_buildingAI as ExtractingFacilityAI;
                        if (buildingAI != null && building.Info != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                        }
                        break;
                    }
                case BuildingType.ParkMaintenanceDepot:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        MaintenanceDepotAI? buildingAI = building.Info?.m_buildingAI as MaintenanceDepotAI;
                        if (buildingAI != null && building.Info != null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            return (productionRate * buildingAI.m_maintenanceTruckCount + 99) / 100;
                        }
                        break;
                    }
            }

            return 0;
        }
    }
}