using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class BuildingVehicleCount
    {
        public static string GetVehicleTabText(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            string sMessage = Localization.Get("tabBuildingPanelVehicles");

            int iVehicleTypes = GetVehicleTypeCount(eBuildingType, buildingId);
            if (iVehicleTypes > 0)
            {
                sMessage += " (";

                if (eBuildingType == BuildingType.PostOffice)
                {
                    // Special handler for post office as trucks can do Mail as well
                    int iPostVans = 0;
                    int iPostTrucks = 0;
                    Mail2PostOfficePatches.CalculateVehicles(ref building, ref iPostVans, ref iPostTrucks);
                    sMessage += $"{iPostVans}/{GetMaxVehicleCount(eBuildingType, buildingId, 0)} | {iPostTrucks}/{GetMaxVehicleCount(eBuildingType, buildingId, 1)}";
                }
                else
                {
                    for (int i = 0; i < iVehicleTypes; ++i)
                    {
                        // Add max count if available
                        int maxVehicles = BuildingVehicleCount.GetMaxVehicleCount(eBuildingType, buildingId, i);
                        if (maxVehicles > 0)
                        {
                            if (i > 0)
                            {
                                sMessage += " | ";
                            }
                            sMessage += $"{GetVehicleCount(eBuildingType, buildingId, building, i)}/{maxVehicles}";
                        }
                        else
                        {
                            // Only add count if > 0
                            int iVehicleCount = GetVehicleCount(eBuildingType, buildingId, building, i);
                            if (i == 0 || iVehicleCount > 0)
                            {
                                if (i > 0)
                                {
                                    sMessage += " | ";
                                }
                                sMessage += $"{iVehicleCount}";
                            }
                        }
                    }
                }

                sMessage += ")";
            }

            return sMessage;
        }

        public static int GetVehicleTypeCount(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId)
        {
            if (HasVehicles(eBuildingType, buildingId))
            {
                switch (eBuildingType)
                {
                    case BuildingType.Recycling:
                    case BuildingType.WasteProcessing:
                    case BuildingType.Landfill:
                    case BuildingType.IncinerationPlant:
                    case BuildingType.Cemetery:
                    case BuildingType.PoliceStation:
                    case BuildingType.MedicalHelicopterDepot:
                    case BuildingType.PoliceHelicopterDepot:
                    case BuildingType.WarehouseStation:
                    case BuildingType.FishHarbor:
                    case BuildingType.Hospital:
                    case BuildingType.PostOffice:
                        return 2;

                    default:
                        return 1;
                }
            }

            return 0;
        }

        public static int GetMaxVehicleCount(BuildingType eBuildingType, ushort buildingId, int iType)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return GetMaxVehicleCount(eBuildingType, buildingId, building, iType);
        }

        public static int GetVehicleCount(BuildingType eBuildingType, ushort buildingId, Building building, int iType)
        {
            // Add vehicle count if generic processing as the vanilla form doesn't show vehicle count
            switch (eBuildingType)
            {
                case BuildingType.TransportStation:
                    {
                        int iVehicleCount = BuildingUtils.GetOwnVehicleCount(building);

                        // Loop sub buildings as well
                        int iLoopCount = 0;
                        while (building.m_subBuilding != 0)
                        {
                            building = BuildingManager.instance.m_buildings.m_buffer[building.m_subBuilding];
                            if (building.m_flags != 0)
                            {
                                iVehicleCount += BuildingUtils.GetOwnVehicleCount(building);
                            }

                            if (iLoopCount > 100)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }

                        return iVehicleCount;
                    }
                case BuildingType.FishHarbor: 
                    {
                        return GetVehicleCount(TransferReason.Fish, buildingId, building, iType);
                    }
                case BuildingType.WarehouseStation:
                    {
                        if (iType == 0)
                        {
                            return BuildingUtils.GetOwnVehicleCount(building);
                        }
                        else
                        {
                            Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[building.m_subBuilding];
                            if (subBuilding.m_flags != 0)
                            {
                                return BuildingUtils.GetOwnVehicleCount(subBuilding);
                            }
                            return 0;
                        }
                    }
                case BuildingType.PostOffice:
                    {
                        // Special handler for post office as trucks can do Mail as well
                        int iPostVans = 0;
                        int iPostTrucks = 0;
                        Mail2PostOfficePatches.CalculateVehicles(ref building, ref iPostVans, ref iPostTrucks);
                        if (iType == 0)
                        {
                            return iPostVans;
                        }
                        else
                        {
                            return iPostTrucks;
                        }
                    }
                case BuildingType.WasteProcessing:
                    {
                        return GetVehicleCount(TransferReason.GarbageTransfer, buildingId, building, iType);
                    }
                case BuildingType.IncinerationPlant:
                case BuildingType.Landfill:
                case BuildingType.Recycling:
                    {
                        return GetVehicleCount(TransferReason.Garbage, buildingId, building, iType);
                    }
                case BuildingType.Hospital:
                    {
                        return GetVehicleCount(TransferReason.Sick, buildingId, building, iType);
                    }
                case BuildingType.MedicalHelicopterDepot:
                    {
                        return GetVehicleCount(TransferReason.Sick2, buildingId, building, iType);
                    }
                case BuildingType.PoliceHelicopterDepot:
                    {
                        return GetVehicleCount((TransferReason)CustomTransferReason.Reason.Crime2, buildingId, building, iType);
                    }
                case BuildingType.PoliceStation:
                    {
                        return GetVehicleCount(TransferReason.Crime, buildingId, building, iType);
                    }
                case BuildingType.Cemetery:
                    {
                        return GetVehicleCount(TransferReason.Dead, buildingId, building, iType);
                    }
                default:
                    {
                        // Just return total vehicle count
                        return BuildingUtils.GetOwnVehicleCount(building);
                    }
            }
        }

        public static int GetVehicleCount(TransferReason material, ushort buildingId, Building building, int iType)
        {
            BuildingUtils.CalculateOwnVehicles(buildingId, ref building, material, out int iCount, out int iTotal);
            if (iType == 0)
            {
                return iCount;
            }
            else
            {
                return iTotal - iCount;
            }
        }

        public static int GetMaxVehicleCount(BuildingType eBuildingType, ushort buildingId, Building building, int iVehicleType)
        {
            // Add vehicle count if generic processing as the vanilla form doesn't show vehicle count
            switch (eBuildingType)
            {
                case BuildingType.GenericExtractor:
                    {
                        if (iVehicleType == 0 && building.m_flags != 0)
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
                        if (iVehicleType == 0 && building.m_flags != 0)
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
                        if (iVehicleType == 0)
                        {
                            CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                            if (cemeteryAI is not null)
                            {
                                // Factor in budget
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * cemeteryAI.m_hearseCount + 99) / 100;
                            }
                        } 
                        break;
                    }
                case BuildingType.Prison:
                case BuildingType.PoliceStation:
                    {
                        if (iVehicleType == 0)
                        {
                            PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.PoliceCarCount + 99) / 100;
                            }
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
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
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
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
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
                case BuildingType.WasteTransfer:
                case BuildingType.WasteProcessing:
                case BuildingType.Landfill:
                case BuildingType.IncinerationPlant:
                case BuildingType.Recycling:
                {
                        if (iVehicleType == 0)
                        {
                            LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                            if (garbageAI is not null)
                            {
                                // Factor in budget
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * garbageAI.m_garbageTruckCount + 99) / 100;
                            }
                        }
                        else
                        {
                            return 0;
                        }
                        break;
                    }
                case BuildingType.WarehouseStation:
                case BuildingType.Warehouse:
                    {
                        if (iVehicleType == 0)
                        {
                            WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                            if (warehouseAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * warehouseAI.m_truckCount + 99) / 100;
                            }
                        }
                        break;
                    }
                case BuildingType.UniqueFactory:
                case BuildingType.FishFactory:
                case BuildingType.ProcessingFacility:
                    {
                        if (iVehicleType == 0)
                        {
                            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                            if (buildingAI is not null && building.Info is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                            }
                        }   
                        break;
                    }
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
                case BuildingType.FishHarbor:
                    {
                        FishingHarborAI? buildingAI = building.Info?.m_buildingAI as FishingHarborAI;
                        if (buildingAI is not null && building.Info is not null)
                        {
                            int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                            if (iVehicleType == 0)
                            {
                                return (productionRate * buildingAI.m_outputVehicleCount + 99) / 100;
                            }
                            else if (iVehicleType == 1)
                            {
                                return (productionRate * buildingAI.m_boatCount + 99) / 100;
                            }
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
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
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
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
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
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
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
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
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
                case BuildingType.PostOffice:
                    {
                        if (building.m_flags != 0 && building.Info is not null)
                        {
                            PostOfficeAI? buildingAI = building.Info.m_buildingAI as PostOfficeAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);

                                if (iVehicleType == 0)
                                {
                                    return (productionRate * buildingAI.m_postVanCount + 99) / 100;
                                }
                                else if(iVehicleType == 1)
                                {
                                    return (productionRate * buildingAI.m_postTruckCount + 99) / 100;
                                }
                            }
                        }
                        break;
                    }
                case BuildingType.PostSortingFacility:
                    {
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
                        {
                            PostOfficeAI? buildingAI = building.Info.m_buildingAI as PostOfficeAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_postTruckCount + 99) / 100;
                            }
                        }
                        break;
                    }
                case BuildingType.PumpingService:
                    {
                        if (iVehicleType == 0 && building.m_flags != 0 && building.Info is not null)
                        {
                            WaterFacilityAI? buildingAI = building.Info.m_buildingAI as WaterFacilityAI;
                            if (buildingAI is not null)
                            {
                                int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                return (productionRate * buildingAI.m_pumpingVehicles + 99) / 100;
                            }
                        }
                        break;
                    }
            }

            return 0;
        }
    }
}