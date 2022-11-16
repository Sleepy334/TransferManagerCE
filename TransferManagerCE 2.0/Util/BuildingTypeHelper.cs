using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Reflection;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;

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

            ElementartySchool,
            HighSchool,
            University,
            Monument,
            CargoStation,
            CargoFerryWarehouseHarbor,
            Warehouse,
            OutsideConnection,
            SpaceElevator,

            // Services
            PoliceStation,
            PoliceHelicopterDepot,
            Prison,
            HelicopterPrison, // Prison Helicopter Mod
            FireStation,
            FireHelicopterDepot,
            Hospital,
            MedicalHelicopterDepot,
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

            FishHarbor,
            FishFarm,
            FishFactory,
            FishMarket,

            // Transport
            AirportMainBuilding,
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
                                return BuildingType.MedicalHelicopterDepot;
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
                                        return BuildingType.PoliceHelicopterDepot;
                                    }
                                case ItemClass.Level.Level4:
                                    {
                                        if (building.Info.GetAI().name.Contains("PrisonCopterPoliceStationAI"))
                                        {
                                            return BuildingType.HelicopterPrison;
                                        }
                                        else
                                        {
                                            return BuildingType.Prison;
                                        }
                                    }
                            }
                            break;
                        }
                    case ItemClass.Service.Education:
                        {
                            switch (building.Info.GetClassLevel())
                            {
                                case ItemClass.Level.Level1:
                                    {
                                        return BuildingType.ElementartySchool;
                                    }
                                case ItemClass.Level.Level2:
                                    {
                                        return BuildingType.HighSchool;
                                    }
                                case ItemClass.Level.Level3:
                                    {
                                        return BuildingType.University;
                                    }
                            }
                            break;
                        }
                    case ItemClass.Service.Monument:
                        {
                            return BuildingType.Monument;
                        }
                    case ItemClass.Service.FireDepartment:
                        {
                            if (building.Info.GetAI() is HelicopterDepotAI)
                            {
                                return BuildingType.FireHelicopterDepot;
                            }
                            else 
                            {
                                return BuildingType.FireStation;
                            }
                        }
                    case ItemClass.Service.PublicTransport:
                        {
                            switch (building.Info.GetAI())
                            {
                                case CargoStationAI cargoStation:
                                    {
                                        if (cargoStation.GetType().ToString().Contains("CargoFerryWarehouseHarborAI"))
                                        {
                                            return BuildingType.CargoFerryWarehouseHarbor;
                                        }
                                        else
                                        {
                                            return BuildingType.CargoStation;
                                        }
                                    }
                                case OutsideConnectionAI:
                                    {
                                        return BuildingType.OutsideConnection;
                                    }
                                case SpaceElevatorAI:
                                    {
                                        return BuildingType.SpaceElevator;
                                    }
                                case TransportStationAI:
                                    {
                                        return BuildingType.TransportStation;
                                    }
                                case AirportEntranceAI:
                                    {
                                        return BuildingType.AirportMainBuilding;
                                    }
                                default:
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
                                                    if (building.Info.GetAI() is TaxiStandAI)
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
                                        break;
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
                                        return BuildingType.FishHarbor;
                                    }
                                case FishFarmAI:
                                    {
                                        return BuildingType.FishFarm;
                                    }
                                case ProcessingFacilityAI:
                                    {
                                        return BuildingType.FishFactory;
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

        public static bool IsCargoStation(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                return building.Info.GetAI() is CargoStationAI;
            }
            return false;
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

        public static bool CanImport(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            switch (eType)
            {
                case BuildingType.GenericProcessing:
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                case BuildingType.Commercial:
                case BuildingType.DisasterShelter:
                    return true;

                case BuildingType.Warehouse:
                    return IsWarehouseCanImport(buildingId);

                case BuildingType.CargoFerryWarehouseHarbor:
                    return IsCargoFerryWarehouseCanImport(buildingId);

                case BuildingType.ProcessingFacility:
                case BuildingType.UniqueFactory:
                    return IsProcessingFacilityWithVehicles(buildingId);

                case BuildingType.OutsideConnection:
                    return OutsideConnectionCanImport(buildingId);
            }

            return false;
        }

        public static bool CanExport(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            switch (eType)
            {
                case BuildingType.Warehouse:
                case BuildingType.CargoFerryWarehouseHarbor:
                case BuildingType.ExtractionFacility:
                case BuildingType.GenericProcessing:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                case BuildingType.FishFarm:
                case BuildingType.FishHarbor:
                case BuildingType.FishFactory:
                    return true;

                case BuildingType.ProcessingFacility:
                case BuildingType.UniqueFactory:
                    return IsProcessingFacilityWithVehicles(buildingId);

                case BuildingType.OutsideConnection:
                    return OutsideConnectionCanExport(buildingId);
            }

            return false;
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
            return DistrictRestrictions.IncomingBuildingDistrictRestrictionsSupported(eType, buildingId).Count > 0 ||
                    DistrictRestrictions.OutgoingBuildingDistrictRestrictionsSupported(eType, buildingId).Count > 0;
        }

        public static HashSet<TransferReason> IncomingDistanceRestrictionSupported(BuildingType eType, ushort buildingId)
        {
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
                case BuildingType.ElementartySchool:
                case BuildingType.HighSchool:
                case BuildingType.University:
                    return DistrictRestrictions.IncomingBuildingDistrictRestrictionsSupported(eType, buildingId);
            }

            return new HashSet<TransferReason>();
        }

        public static HashSet<TransferReason> OutgoingDistanceRestrictionSupported(BuildingType eType, ushort buildingId)
        {
            switch (eType)
            {
                case BuildingType.Warehouse:
                    return DistrictRestrictions.OutgoingBuildingDistrictRestrictionsSupported(eType, buildingId);
            }

            return new HashSet<TransferReason>(); 
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
                    eType == BuildingType.FishHarbor ||
                    eType == BuildingType.FishFarm ||
                    eType == BuildingType.FishFactory ||
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