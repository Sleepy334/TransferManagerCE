using System;
using System.Reflection;
using TransferManagerCE.CustomManager;

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
            GenericProcessing,
            GenericExtractor,

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
            CargoFerryWarehouseHarbor,
            Warehouse,
            OutsideConnection,
            SpaceElevator,

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
            PostSortingFacility,
            ParkMaintenanceDepot,
            Cemetery,
            RoadMaintenanceDepot,
            TaxiDepot,
            TaxiStand, // Taxi's are actually on the guest vehicle side not the own vehicle side.
            SnowDump,
            ServicePoint,

            // Industry DLC, Fish farms
            MainIndustryBuilding,
            ExtractionFacility,
            ProcessingFacility,
            UniqueFactory,
            FishMarket,

            // Transport
            TransportStation,
            TourDepot,
            BusDepot,
        }

        public enum BuildingSubType
        {
            None,
            WarehouseCrops,
            WarehouseOil,
            WarehouseLogs,
            WarehouseOre,
        }

        public enum OutsideType
        {
            Unknown,
            Ship,
            Plane,
            Train,
            Road
        }

        private static MethodInfo? s_CargoFerriesGetActualTransferReasonMethod = null;

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
                            switch (building.Info.GetAI())
                            {
                                case IndustrialExtractorAI:
                                    {
                                        return BuildingType.GenericExtractor;
                                    }
                                case IndustrialBuildingAI:
                                    {
                                        return BuildingType.GenericProcessing; 
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            break;
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
                                return BuildingType.Cemetery;
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
                            PrefabAI buildingAI = building.Info.GetAI();
                            if (buildingAI is CargoStationAI)
                            {
                                if (buildingAI.GetType().ToString().Contains("CargoFerryWarehouseHarborAI"))
                                {
                                    return BuildingType.CargoFerryWarehouseHarbor;
                                }
                                else
                                {
                                    return BuildingType.CargoStation;
                                }
                                    
                            }
                            else if (buildingAI is OutsideConnectionAI)
                            {
                                return BuildingType.OutsideConnection;
                            }
                            else if (buildingAI is SpaceElevatorAI)
                            {
                                return BuildingType.SpaceElevator;
                            }
                            else if (buildingAI is TransportStationAI)
                            {
                                return BuildingType.TransportStation;
                            }
                            else
                            {
                                switch (building.Info.GetSubService())
                                {
                                    case ItemClass.SubService.PublicTransportPost:
                                        {
                                            switch (building.Info.GetClassLevel())
                                            {
                                                case ItemClass.Level.Level2:
                                                    {
                                                        return BuildingType.PostOffice;
                                                    }
                                                case ItemClass.Level.Level5:
                                                    {
                                                        return BuildingType.PostSortingFacility;
                                                    }
                                                default: break;
                                            }
                                            break;
                                        }
                                    case ItemClass.SubService.PublicTransportTaxi:
                                        {
                                            if (buildingAI is TaxiStandAI)
                                            {
                                                return BuildingType.TaxiStand;
                                            }
                                            else
                                            {
                                                return BuildingType.TaxiDepot;
                                            }
                                        }
                                    case ItemClass.SubService.PublicTransportTours:
                                        {
                                            return BuildingType.TourDepot;
                                        }
                                    case ItemClass.SubService.PublicTransportBus:
                                        {
                                            return BuildingType.BusDepot;
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
                            if (building.Info != null)
                            {
                                switch (building.Info.m_buildingAI)
                                {
                                    case WarehouseAI: return BuildingType.Warehouse;
                                    case ExtractingFacilityAI: return BuildingType.ExtractionFacility;
                                    case UniqueFactoryAI: return BuildingType.UniqueFactory;
                                    case ProcessingFacilityAI: return BuildingType.ProcessingFacility;
                                    case MainIndustryBuildingAI: return BuildingType.MainIndustryBuilding;
                                }
                                if (building.Info.m_buildingAI.GetType().ToString().Contains("CargoFerryWarehouseHarborAI"))
                                {
                                    return BuildingType.Warehouse;
                                }
                            }
                            break;
                        }
                    case ItemClass.Service.PlayerEducation: break;
                    case ItemClass.Service.Museums: break;
                    case ItemClass.Service.VarsitySports: break;
                    case ItemClass.Service.Fishing:
                        {
                            switch (building.Info.GetAI())
                            {
                                case FishingHarborAI:
                                    {
                                        return BuildingType.ExtractionFacility;
                                    }
                                case FishFarmAI:
                                    {
                                        return BuildingType.ExtractionFacility;
                                    }
                                case ProcessingFacilityAI:
                                    {
                                        return BuildingType.ProcessingFacility;
                                    }
                                case MarketAI:
                                    {
                                        return BuildingType.FishMarket;
                                    }
                            }
                            break;
                        }
                    case ItemClass.Service.Road:
                        {
                            switch (building.Info.GetAI())
                            {
                                case OutsideConnectionAI: return BuildingType.OutsideConnection;
                                case MaintenanceDepotAI: return BuildingType.RoadMaintenanceDepot;
                                case SnowDumpAI: return BuildingType.SnowDump;
                            }
                            break;
                        }
                    case ItemClass.Service.ServicePoint:
                        {
                            return BuildingType.ServicePoint;
                        }
                }
            }
            //ServicePointAI
            return BuildingType.None;
        }

        public static BuildingSubType GetBuildingSubType(ushort buildingId)
        {
            if (buildingId != 0)
            {
                BuildingType eMainType = GetBuildingType(buildingId);
                switch (eMainType)
                {
                    case BuildingType.Warehouse:
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                            switch (building.Info.GetSubService())
                            {
                                case ItemClass.SubService.PlayerIndustryFarming:
                                    {
                                        return BuildingSubType.WarehouseCrops;
                                    }
                                case ItemClass.SubService.PlayerIndustryForestry:
                                    {
                                        return BuildingSubType.WarehouseLogs;
                                    }
                                case ItemClass.SubService.PlayerIndustryOil:
                                    {
                                        return BuildingSubType.WarehouseOil;
                                    }
                                case ItemClass.SubService.PlayerIndustryOre:
                                    {
                                        return BuildingSubType.WarehouseOre;
                                    }
                                default:
                                    {
                                        return BuildingSubType.None;
                                    }
                            }
                        }
                    default:
                        {
                            return BuildingSubType.None;
                        }
                }
                
            }
           
            return BuildingSubType.None;
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

        public static OutsideType GetOutsideConnectionType(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                switch (building.Info?.m_class.m_service)
                {
                    case ItemClass.Service.PublicTransport:
                        {
                            ItemClass.SubService? subService = building.Info?.m_class.m_subService;
                            switch (subService)
                            {
                                case ItemClass.SubService.PublicTransportShip: return OutsideType.Ship;
                                case ItemClass.SubService.PublicTransportPlane: return OutsideType.Plane;
                                case ItemClass.SubService.PublicTransportTrain: return OutsideType.Train;
                                default: return OutsideType.Road;
                            }
                        }
                    case ItemClass.Service.Road:
                        {
                            return OutsideType.Road;
                        }
                }
            }

            return OutsideType.Unknown;
        }

        public static bool IsWarehouse(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                PrefabAI buildingAI = building.Info.GetAI();
                return buildingAI is WarehouseAI || buildingAI.GetType().ToString().Contains("CargoFerryWarehouseHarborAI");
            }
            return false;
        }

        public static bool IsWarehouseCanImport(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                PrefabAI buildingAI = building.Info.GetAI();
                if (buildingAI is WarehouseAI warehouseAI)
                {
                    TransferManager.TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                    return TransferRestrictions.IsImportRestrictionsSupported(actualTransferReason);
                } 
            }
            return false;
        }

        public static bool IsCargoFerryWarehouseCanImport(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                PrefabAI buildingAI = building.Info.GetAI();
                if (buildingAI.GetType().ToString().Contains("CargoFerryWarehouseHarborAI"))
                {
                    if (s_CargoFerriesGetActualTransferReasonMethod == null)
                    {
                        try
                        {
                            Assembly? assembly = DependencyUtilities.GetCargoFerriesAssembly();
                            if (assembly != null)
                            {
                                s_CargoFerriesGetActualTransferReasonMethod = assembly.GetType("CargoFerries.AI.CargoFerryWarehouseHarborAI").GetMethod("GetActualTransferReason", BindingFlags.Public | BindingFlags.Instance);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex);
                        }
                    }
                    if (s_CargoFerriesGetActualTransferReasonMethod != null)
                    {     
                        TransferManager.TransferReason actualTransferReason = (TransferManager.TransferReason)s_CargoFerriesGetActualTransferReasonMethod.Invoke(buildingAI, new object[] { buildingId, building });
                        return TransferRestrictions.IsImportRestrictionsSupported(actualTransferReason);
                    }
                }

            }
            return false;
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
            BuildingType eType = GetBuildingType(buildingId);
            return eType == BuildingType.GenericExtractor || eType == BuildingType.GenericProcessing;
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

        public static bool ProcessingFacilityCanImport(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI != null)
            {
                return TransferRestrictions.IsImportRestrictionsSupported(buildingAI.m_inputResource1) ||
                    TransferRestrictions.IsImportRestrictionsSupported(buildingAI.m_inputResource2) ||
                    TransferRestrictions.IsImportRestrictionsSupported(buildingAI.m_inputResource3) ||
                    TransferRestrictions.IsImportRestrictionsSupported(buildingAI.m_inputResource4);
            }

            return false;
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

        public static bool IsGarbageRecycling(ushort buildingId)
        {
            return GetBuildingType(buildingId) == BuildingType.Recycling;
        }

        public static bool CanImport(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return (eType == BuildingType.Warehouse && IsWarehouseCanImport(buildingId)) ||
                   (eType == BuildingType.CargoFerryWarehouseHarbor && IsCargoFerryWarehouseCanImport(buildingId)) ||
                    (eType == BuildingType.ProcessingFacility && ProcessingFacilityCanImport(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && ProcessingFacilityCanImport(buildingId)) ||
                    eType == BuildingType.GenericProcessing ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.PostSortingFacility ||
                    eType == BuildingType.Commercial ||
                    eType == BuildingType.DisasterShelter ||
                    (eType == BuildingType.OutsideConnection && OutsideConnectionCanImport(buildingId));
        }

        public static bool CanExport(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return eType == BuildingType.Warehouse ||
                    eType == BuildingType.CargoFerryWarehouseHarbor ||
                    eType == BuildingType.ExtractionFacility ||
                    (eType == BuildingType.ProcessingFacility && IsProcessingFacilityWithVehicles(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && IsProcessingFacilityWithVehicles(buildingId)) ||
                    eType == BuildingType.GenericProcessing ||
                    eType == BuildingType.Recycling ||
                    eType == BuildingType.WasteProcessing ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.PostSortingFacility ||
                    (eType == BuildingType.OutsideConnection && OutsideConnectionCanExport(buildingId));
        }

        public static bool OutsideConnectionCanExport(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return (building.m_flags & Building.Flags.Incoming) != 0;
        }

        public static bool OutsideConnectionCanImport(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return (building.m_flags & Building.Flags.Outgoing) != 0;
        }

        public static bool CanRestrictDistrict(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return CanDistrictRestrictIncoming(eType) || CanDistrictRestrictOutgoing(eType, buildingId);
        }

        public static bool CanDistrictRestrictIncoming(BuildingType eType)
        {
            switch (eType)
            {
                case BuildingType.Warehouse:
                case BuildingType.CargoFerryWarehouseHarbor:
                case BuildingType.ProcessingFacility:
                case BuildingType.UniqueFactory:
                case BuildingType.GenericProcessing:
                case BuildingType.Commercial:
                case BuildingType.DisasterShelter:
                case BuildingType.PoliceStation:
                case BuildingType.FireStation:
                case BuildingType.Hospital:
                case BuildingType.Cemetery:
                case BuildingType.Landfill:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                case BuildingType.WasteTransfer:
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                case BuildingType.ParkMaintenanceDepot:
                case BuildingType.DisasterResponseUnit:
                case BuildingType.SnowDump:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanDistrictRestrictOutgoing(BuildingType eType, ushort buildingId)
        {
            switch (eType)
            {
                case BuildingType.Warehouse:
                case BuildingType.CargoFerryWarehouseHarbor:
                case BuildingType.ExtractionFacility:
                case BuildingType.GenericExtractor:
                case BuildingType.GenericProcessing:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                //case BuildingType.WasteTransfer || We don't currently restrict GarbageMove or GarbageTransfer
                case BuildingType.Landfill:
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                case BuildingType.ParkMaintenanceDepot:
                case BuildingType.TaxiDepot:
                case BuildingType.RoadMaintenanceDepot:
                case BuildingType.MainIndustryBuilding: // Allow restricting services
                    return true;

                case BuildingType.ProcessingFacility:
                case BuildingType.UniqueFactory:
                    return IsProcessingFacilityWithVehicles(buildingId);

                default:
                    return false;
            }
        }

        public static bool IsDistanceRestrictionSupported(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            switch (eType)
            {
                case BuildingType.Cemetery:
                case BuildingType.PoliceStation:
                case BuildingType.Landfill:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                case BuildingType.WasteTransfer:
                case BuildingType.FireStation:
                case BuildingType.Hospital:
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                case BuildingType.ParkMaintenanceDepot:
                case BuildingType.Warehouse:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasVehicles(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            return (eType == BuildingType.Warehouse ||
                    eType == BuildingType.CargoFerryWarehouseHarbor ||
                    eType == BuildingType.ExtractionFacility ||
                    (eType == BuildingType.ProcessingFacility && IsProcessingFacilityWithVehicles(buildingId)) ||
                    (eType == BuildingType.UniqueFactory && IsProcessingFacilityWithVehicles(buildingId)) ||
                    eType == BuildingType.GenericExtractor ||
                    eType == BuildingType.GenericProcessing ||
                    eType == BuildingType.PoliceStation ||
                    eType == BuildingType.FireStation ||
                    eType == BuildingType.Cemetery ||
                    eType == BuildingType.FireStation ||
                    eType == BuildingType.Hospital ||
                    eType == BuildingType.Landfill ||
                    eType == BuildingType.Recycling ||
                    eType == BuildingType.WasteProcessing ||
                    eType == BuildingType.WasteTransfer ||
                    eType == BuildingType.PostOffice ||
                    eType == BuildingType.PostSortingFacility ||
                    eType == BuildingType.ParkMaintenanceDepot ||
                    eType == BuildingType.CargoStation ||
                    eType == BuildingType.DisasterResponseUnit ||
                    eType == BuildingType.DisasterShelter ||
                    eType == BuildingType.RoadMaintenanceDepot ||
                    eType == BuildingType.OutsideConnection ||
                    eType == BuildingType.Prison ||
                    eType == BuildingType.TaxiDepot ||
                    eType == BuildingType.TransportStation ||
                    eType == BuildingType.TourDepot ||
                    eType == BuildingType.BusDepot ||
                    eType == BuildingType.SnowDump);
        }

        public static bool IsSameType(ushort buildingId, ushort otherBuildingId)
        {
            BuildingType eThisType = GetBuildingType(buildingId);
            BuildingType eOtherType = GetBuildingType(otherBuildingId);
            if (eThisType == eOtherType && eThisType != BuildingType.None)
            {
                if (eThisType == BuildingType.Warehouse)
                {
                    // Check sub type is the same
                    return GetBuildingSubType(buildingId) == GetBuildingSubType(otherBuildingId);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }      
    }
}