namespace TransferManagerCE
{
    public class BuildingTypeHelper
    {
        public enum BuildingType
        {
            None,
            
            Residential,
            Commercial,
            Office,
            GenericIndustry,
            
            Electricity,
            PowerPlant,
            SolarPowerPlant,
            Water,
            Park,
            Healthcare,

            Disaster,
            DisasterResponseUnit,
            DisasterShelter,

            Education,
            Monument,
            CargoStation,
            Warehouse,
            OutsideConnection,

            // Services
            PoliceStation,
            Prison,
            FireStation,
            Hospital,
            Landfill,
            Recycling,
            WasteProcessing,
            WasteTransfer,
            PostOffice,
            ParkMaintenanceDepot,
            Cemetery,
            Cremetorium,
            RoadMaintenanceDepot,

            // Industry DLC
            MainBuilding,
            ExtractionFacility,
            ProcessingFacility,
            FishingHarbor,
            FishFarm,
            UniqueFactory,
        }

        public static BuildingType GetBuildingType(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                switch (building.Info.GetService())
                {
                    case ItemClass.Service.Residential:
                        {
                            return BuildingType.Residential;
                        }
                    case ItemClass.Service.Commercial:
                        {
                            return BuildingType.Commercial;
                        }
                    case ItemClass.Service.Industrial:
                        {
                            return BuildingType.GenericIndustry;
                        }
                    case ItemClass.Service.Office:
                        {
                            return BuildingType.Office;
                        }
                    case ItemClass.Service.Electricity:
                        {
                            if (building.Info.GetAI() is PowerPlantAI)
                            {
                                return BuildingType.PowerPlant;
                            }
                            else if (building.Info.GetAI() is SolarPowerPlantAI)
                            {
                                return BuildingType.SolarPowerPlant;
                            }
                            return BuildingType.Electricity;
                        }
                    case ItemClass.Service.Water:
                        {
                            return BuildingType.Water;
                        }
                    case ItemClass.Service.Beautification:
                        {
                            if (building.Info?.m_buildingAI is MaintenanceDepotAI)
                            {
                                return BuildingType.ParkMaintenanceDepot;
                            }
                            else if (building.Info?.m_buildingAI is ParkBuildingAI)
                            {
                                return BuildingType.Park;
                            }
                            else if (building.Info?.m_buildingAI is ParkAI)
                            {
                                return BuildingType.Park;
                            }
                            else if (building.Info?.m_buildingAI is ParkGateAI)
                            {
                                return BuildingType.Park;
                            }
                            return BuildingType.None;
                        }
                    case ItemClass.Service.Garbage:
                        {
                            switch (building.Info.GetClassLevel())
                            {
                                case ItemClass.Level.Level1:
                                    {
                                        return BuildingType.Landfill;
                                    }
                                case ItemClass.Level.Level2:
                                    {
                                        return BuildingType.Recycling;
                                    }
                                case ItemClass.Level.Level3:
                                    {
                                        return BuildingType.WasteTransfer;
                                    }
                                case ItemClass.Level.Level4:
                                    {
                                        return BuildingType.WasteProcessing;
                                    }
                            }
                            break;
                        }
                    case ItemClass.Service.HealthCare:
                        {
                            if (building.Info?.GetAI() is HospitalAI)
                            {
                                return BuildingType.Hospital;
                            }
                            if (building.Info?.GetAI() is HelicopterDepotAI)
                            {
                                return BuildingType.Hospital;
                            }
                            else if (building.Info?.GetAI() is CemeteryAI cemetery)
                            {
                                if (cemetery.name == "Crematory")
                                {
                                    return BuildingType.Cremetorium;
                                }
                                else if (cemetery.name == "Cemetery")
                                {
                                    return BuildingType.Cemetery;
                                }
                            }
                            return BuildingType.Healthcare;
                        }
                    case ItemClass.Service.PoliceDepartment:
                        {
                            switch (building.Info.GetClassLevel())
                            {
                                case ItemClass.Level.Level1:
                                    {
                                        return BuildingType.PoliceStation;
                                    }
                                case ItemClass.Level.Level3:
                                    {
                                        return BuildingType.PoliceStation; // Helicopter
                                    }
                                case ItemClass.Level.Level4:
                                    {
                                        return BuildingType.Prison;
                                    }
                            }
                            break;
                        }
                    case ItemClass.Service.Education:
                        {
                            return BuildingType.Education;
                        }
                    case ItemClass.Service.Monument:
                        {
                            return BuildingType.Monument;
                        }
                    case ItemClass.Service.FireDepartment:
                        {
                            return BuildingType.FireStation;
                        }
                    case ItemClass.Service.PublicTransport:
                        {
                            if (building.Info.GetAI() is CargoStationAI)
                            {
                                return BuildingType.CargoStation;
                            }
                            else if (building.Info.GetAI() is OutsideConnectionAI)
                            {
                                return BuildingType.OutsideConnection;
                            }
                            else
                            {
                                switch (building.Info.GetSubService())
                                {
                                    case ItemClass.SubService.PublicTransportPost:
                                        {
                                            return BuildingType.PostOffice;
                                        }
                                    default: break;
                                }
                            }
                            break;
                        }
                    case ItemClass.Service.Disaster:
                        {
                            switch (building.Info.GetClassLevel())
                            {
                                case ItemClass.Level.Level2:
                                    {
                                        return BuildingType.DisasterResponseUnit;
                                    }
                                case ItemClass.Level.Level4:
                                    {
                                        return BuildingType.DisasterShelter;
                                    }
                                default:
                                    {
                                        return BuildingType.Disaster;
                                    }
                            }

                        }
                    case ItemClass.Service.PlayerIndustry:
                        {
                            if (building.Info?.m_buildingAI is WarehouseAI)
                            {
                                return BuildingType.Warehouse;
                            }
                            else if (building.Info?.m_buildingAI.GetType().ToString() == "CargoFerries.AI.CargoFerryWarehouseHarborAI")
                            {
                                return BuildingType.Warehouse;
                            }
                            else if (building.Info?.m_buildingAI is UniqueFactoryAI)
                            {
                                return BuildingType.UniqueFactory;
                            }
                            else if (building.Info?.m_buildingAI is ExtractingFacilityAI)
                            {
                                return BuildingType.ExtractionFacility;
                            }
                            else if (building.Info?.m_buildingAI is ProcessingFacilityAI)
                            {
                                return BuildingType.ProcessingFacility;
                            }
                            break;
                        }
                    case ItemClass.Service.PlayerEducation: break;
                    case ItemClass.Service.Museums: break;
                    case ItemClass.Service.VarsitySports: break;
                    case ItemClass.Service.Fishing:
                        {
                            if (building.Info.GetAI() is FishingHarborAI)
                            {
                                return BuildingType.FishingHarbor;
                            }
                            else if (building.Info.GetAI() is FishFarmAI)
                            {
                                return BuildingType.FishFarm;
                            }
                            break;
                        }
                    case ItemClass.Service.Road:
                        {
                            if (building.Info.GetAI() is MaintenanceDepotAI)
                            {
                                return BuildingType.RoadMaintenanceDepot;
                            }
                            else if (building.Info?.m_buildingAI is OutsideConnectionAI)
                            {
                                return BuildingType.OutsideConnection;
                            }
                            break;
                        }
                }
            }
            
            return BuildingType.None;
        }

        public static bool IsOutsideConnection(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                return (building.Info?.m_buildingAI is OutsideConnectionAI);
            }

            return false;
        }

        public static bool IsWarehouse(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                return building.Info.GetAI() is WarehouseAI;
            }
            return false;
        }

        public static bool IsUniqueFactory(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.UniqueFactory;
        }

        public static bool IsCargoStation(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                return building.Info.GetAI() is CargoStationAI;
            }
            return false;
        }

        // Old industry building
        public static bool IsIndustrialBuilding(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.GenericIndustry;
        }

        // new Industry DLC building
        public static bool IsIndustryBuilding(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return IsIndustryBuilding(building);
        }

        public static bool IsIndustryBuilding(Building building)
        {
            return building.Info?.m_buildingAI is IndustryBuildingAI;
        }

        public static bool IsExtractingFacility(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.ExtractionFacility;
        }

        public static bool IsProcessingFacility(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return building.Info?.m_buildingAI is ProcessingFacilityAI;
        }

        public static bool IsProcessingFacilityWithVehicles(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            ProcessingFacilityAI? buildingAI = building.Info.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI != null)
            {
                return buildingAI.m_outputResource != TransferManager.TransferReason.None && buildingAI.m_outputVehicleCount != 0;
            }
            else
            {
                return false;
            }
        }

        public static bool IsCommercialBuilding(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.Commercial;
        }

        public static bool IsServiceBuilding(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            ItemClass.Service service = building.Info.GetService();
            ItemClass.SubService subService = building.Info.GetSubService();

            return service == ItemClass.Service.FireDepartment ||
                  service == ItemClass.Service.PoliceDepartment ||
                  service == ItemClass.Service.Garbage ||
                  service == ItemClass.Service.HealthCare ||
                  (service == ItemClass.Service.PublicTransport && subService == ItemClass.SubService.PublicTransportPost) ||
                  (service == ItemClass.Service.Beautification && subService == ItemClass.SubService.BeautificationParks);
        }

        public static bool IsMainIndustryBuilding(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return building.Info?.m_buildingAI is MainIndustryBuildingAI;
        }

        public static bool IsFishingHarbor(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.FishingHarbor;
        }

        public static bool IsGarbageRecycling(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.Recycling;
        }

        public static bool CanRestrictDistrict(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return eType == BuildingType.Warehouse ||
                    eType == BuildingType.ExtractionFacility ||
                    (eType == BuildingType.ProcessingFacility && IsProcessingFacilityWithVehicles(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && IsProcessingFacilityWithVehicles(buildingId)) ||
                    eType == BuildingType.GenericIndustry ||
                    eType == BuildingType.PoliceStation ||
                    eType == BuildingType.FireStation ||
                    eType == BuildingType.Hospital ||
                    eType == BuildingType.Cemetery ||
                    eType == BuildingType.Cremetorium ||
                    eType == BuildingType.Landfill ||
                    eType == BuildingType.Recycling ||
                    eType == BuildingType.WasteProcessing ||
                    eType == BuildingType.WasteTransfer ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.ParkMaintenanceDepot ||
                    eType == BuildingType.FishingHarbor ||
                    eType == BuildingType.FishFarm ||
                    eType == BuildingType.DisasterResponseUnit ||
                    eType == BuildingType.ParkMaintenanceDepot;
        }

        public static bool CanImport(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return eType == BuildingType.Warehouse ||
                    (eType == BuildingType.ProcessingFacility && IsProcessingFacilityWithVehicles(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && IsProcessingFacilityWithVehicles(buildingId)) ||
                    eType == BuildingType.GenericIndustry ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.FishingHarbor ||
                    eType == BuildingType.FishFarm ||
                    eType == BuildingType.Commercial;
        }

        public static bool CanExport(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return eType == BuildingType.Warehouse ||
                    eType == BuildingType.ExtractionFacility ||
                    (eType == BuildingType.ProcessingFacility && IsProcessingFacilityWithVehicles(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && IsProcessingFacilityWithVehicles(buildingId)) ||
                    eType == BuildingType.GenericIndustry ||
                    eType == BuildingType.Recycling ||
                    eType == BuildingType.WasteProcessing ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.FishingHarbor ||
                    eType == BuildingType.FishFarm;
        }

        public static bool HasVehicles(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return (eType == BuildingType.Warehouse ||
                    eType == BuildingType.ExtractionFacility ||
                    (eType == BuildingType.ProcessingFacility && IsProcessingFacilityWithVehicles(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && IsProcessingFacilityWithVehicles(buildingId)) ||
                    eType == BuildingType.GenericIndustry ||
                    eType == BuildingType.PoliceStation ||
                    eType == BuildingType.FireStation ||
                    eType == BuildingType.Cemetery ||
                    eType == BuildingType.Cremetorium ||
                    eType == BuildingType.FireStation ||
                    eType == BuildingType.Hospital ||
                    eType == BuildingType.Landfill ||
                    eType == BuildingType.Recycling ||
                    eType == BuildingType.WasteProcessing ||
                    eType == BuildingType.WasteTransfer ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.ParkMaintenanceDepot ||
                    eType == BuildingType.FishingHarbor ||
                    eType == BuildingType.FishFarm ||
                    eType == BuildingType.CargoStation ||
                    eType == BuildingType.DisasterResponseUnit ||
                    eType == BuildingType.RoadMaintenanceDepot ||
                    eType == BuildingType.OutsideConnection);
        }

        

        public static bool IsSameType(ushort buildingId, ushort otherBuildingId)
        {
            BuildingType eThisType = GetBuildingType(buildingId);
            BuildingType eOtherType = GetBuildingType(otherBuildingId);
            if (eThisType == eOtherType)
            {
                Building thisBuilding = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                Building otherBuilding = BuildingManager.instance.m_buildings.m_buffer[otherBuildingId];
                
                bool bResult = thisBuilding.Info.m_class == otherBuilding.Info.m_class &&
                        thisBuilding.Info.GetAI() == otherBuilding.Info.GetAI();
                if (bResult)
                {
                    return true;
                }
            }

            return false;
        }
    }
}