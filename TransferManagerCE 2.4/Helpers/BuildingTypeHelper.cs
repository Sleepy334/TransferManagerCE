using System;
using System.Collections.Generic;
using System.Reflection;
using TransferManagerCE.CustomManager;
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

            CoalPowerPlant,
            PetrolPowerPlant,
            SolarPowerPlant,
            FusionPowerPlant,
            BoilerStation,
            PumpingService,
            Water,
            Park,
            Healthcare,

            DisasterResponseUnit,
            DisasterShelter,

            ElementartySchool,
            HighSchool,
            University,
            Library,
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
            Bank,
            FireStation,
            FireHelicopterDepot,
            Hospital,
            MedicalHelicopterDepot,
            Cemetery,
            ParkMaintenanceDepot,
            RoadMaintenanceDepot,
            TaxiDepot,
            TaxiStand, // Taxi's are actually on the guest vehicle side not the own vehicle side.
            SnowDump,
            ServicePoint,

            // Garbage
            Landfill,
            IncinerationPlant,
            Recycling,
            WasteProcessing,
            WasteTransfer,

            // Mail
            PostOffice,
            PostSortingFacility,
            
            // Generic industries
            GenericExtractor,
            GenericProcessing,
            GenericFactory,
            
            // Industry DLC, Fish farms
            MainIndustryBuilding,
            ExtractionFacility,
            ProcessingFacility,
            UniqueFactory,

            // Fishing
            FishHarbor,
            FishFarm,
            FishFactory,
            FishMarket,

            // Transport
            AirportMainTerminal,
            AirportCargoTerminal,
            AirportAuxBuilding,

            TransportStation,
            TourDepot,
            BusDepot,
            TramDepot,
            FerryDepot,
            PassengerHelicopterDepot,
            CableCarStation,

            CampusBuilding,
        }
        
        public enum BuildingSubType
        {
            None,
            WarehouseCrops,
            WarehouseOil,
            WarehouseLogs,
            WarehouseOre,

            ExtractorCrops,
            ExtractorOil,
            ExtractorLogs,
            ExtractorOre,

            ProcessorCrops,
            ProcessorOil,
            ProcessorLogs,
            ProcessorOre,
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
                return GetBuildingType(building);
            }

            return BuildingType.None;
        }

        public static BuildingType GetBuildingType(Building building)
        {
            // Check Info is valid
            if (building.m_flags == 0 || building.Info is null)
            {
                return BuildingType.None;
            }

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
                                    switch (building.Info.GetSubService())
                                    {
                                        case ItemClass.SubService.IndustrialFarming:
                                        case ItemClass.SubService.IndustrialForestry:
                                        case ItemClass.SubService.IndustrialOil:
                                        case ItemClass.SubService.IndustrialOre:
                                            {
                                                return BuildingType.GenericProcessing;
                                            }

                                        default:
                                            {
                                                return BuildingType.GenericFactory;
                                            }
                                    }
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
                        switch (building.Info.GetAI())
                        {
                            case SolarPowerPlantAI:
                                {
                                    return BuildingType.SolarPowerPlant;
                                }
                            case FusionPowerPlantAI:
                                {
                                    return BuildingType.FusionPowerPlant;
                                }
                            case PowerPlantAI powerPlantAI:
                                {
                                    switch (powerPlantAI.m_resourceType)
                                    {
                                        case TransferReason.Coal:
                                            {
                                                return BuildingType.CoalPowerPlant;
                                            }
                                        case TransferReason.Petrol:
                                            {
                                                return BuildingType.PetrolPowerPlant;
                                            }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case ItemClass.Service.Water:
                    {
                        if (building.Info?.GetAI() is HeatingPlantAI)
                        {
                            return BuildingType.BoilerStation;
                        }
                        else if (building.Info?.GetAI() is WaterFacilityAI)
                        {
                            return BuildingType.PumpingService;
                        }
                        else
                        {
                            return BuildingType.Water;
                        }
                    }
                case ItemClass.Service.Beautification:
                    {
                        switch (building.Info?.m_buildingAI)
                        {
                            case MaintenanceDepotAI: 
                                return BuildingType.ParkMaintenanceDepot;
                            case ParkBuildingAI:
                            case ParkAI:
                            case ParkGateAI: 
                                return BuildingType.Park;
                            default: 
                                return BuildingType.None;
                        }
                    }
                case ItemClass.Service.Garbage:
                    {
                        switch (building.Info.GetClassLevel())
                        {
                            case ItemClass.Level.Level1:
                                {
                                    if (building.Info?.GetAI() is LandfillSiteAI landFill)
                                    {
                                        if (landFill.m_electricityProduction > 0)
                                        {
                                            return BuildingType.IncinerationPlant;
                                        }
                                    }

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
                        switch (building.Info?.GetAI())
                        {
                            case HospitalAI:
                                return BuildingType.Hospital;
                            case HelicopterDepotAI:
                                return BuildingType.MedicalHelicopterDepot;
                            case CemeteryAI:
                                return BuildingType.Cemetery;
                            default:
                                return BuildingType.Healthcare;
                        }
                    }
                case ItemClass.Service.PoliceDepartment:
                    {
                        switch (building.Info.GetSubService())
                        {
                            case ItemClass.SubService.PoliceDepartmentBank:
                                {
                                    return BuildingType.Bank;
                                }
                            default:
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
                            case ItemClass.Level.Level4:
                                {
                                    return BuildingType.Library;
                                }
                        }
                        break;
                    }
                case ItemClass.Service.PlayerEducation:
                    {
                        switch (building.Info.GetSubService())
                        {
                            case ItemClass.SubService.PlayerEducationUniversity:
                            case ItemClass.SubService.PlayerEducationTradeSchool:
                            case ItemClass.SubService.PlayerEducationLiberalArts:
                                {
                                    // If it is a school building rather than an administration building then allow restrictions
                                    if (building.Info.GetAI() is SchoolAI)
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
                                            case ItemClass.Level.Level4:
                                                {
                                                    return BuildingType.Library;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        return BuildingType.CampusBuilding;
                                    }
                                    break;
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
                            case AirportAuxBuildingAI:
                                {
                                    return BuildingType.AirportAuxBuilding;
                                }
                            case AirportEntranceAI:
                                {
                                    switch (building.Info.GetClassLevel())
                                    {
                                        case ItemClass.Level.Level1:
                                            {
                                                return BuildingType.AirportMainTerminal;
                                            }
                                        case ItemClass.Level.Level4:
                                            {
                                                return BuildingType.AirportCargoTerminal;
                                            }
                                    }
                                    break;
                                }
                            case TransportStationAI:
                                {
                                    return BuildingType.TransportStation;
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
                                        case ItemClass.SubService.PublicTransportPlane:
                                            {
                                                if (building.Info.GetAI() is DepotAI)
                                                {
                                                    return BuildingType.PassengerHelicopterDepot;
                                                }
                                                break;
                                            }
                                        case ItemClass.SubService.PublicTransportTram:
                                            {
                                                if (building.Info.GetAI() is DepotAI)
                                                {
                                                    return BuildingType.TramDepot;
                                                }
                                                break;
                                            }
                                        case ItemClass.SubService.PublicTransportShip:
                                            {
                                                if (building.Info.GetAI() is DepotAI)
                                                {
                                                    return BuildingType.FerryDepot;
                                                }
                                                break;
                                            }
                                        case ItemClass.SubService.PublicTransportCableCar:
                                            {
                                                return BuildingType.CableCarStation;
                                            }
                                        default:
                                            {
                                                break;
                                            }
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
                        }
                        break;
                    }
                case ItemClass.Service.PlayerIndustry:
                    {
                        if (building.Info is not null)
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

#if DEBUG
            Debug.Log($"Service: {building.Info?.GetService()} SubService: {building.Info?.GetSubService()} AI: {building.Info?.GetAI()}");
#endif

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
                    case BuildingType.ExtractionFacility:
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                            switch (building.Info.GetSubService())
                            {
                                case ItemClass.SubService.PlayerIndustryFarming:
                                    {
                                        return BuildingSubType.ExtractorCrops;
                                    }
                                case ItemClass.SubService.PlayerIndustryForestry:
                                    {
                                        return BuildingSubType.ExtractorLogs;
                                    }
                                case ItemClass.SubService.PlayerIndustryOil:
                                    {
                                        return BuildingSubType.ExtractorOil;
                                    }
                                case ItemClass.SubService.PlayerIndustryOre:
                                    {
                                        return BuildingSubType.ExtractorOre;
                                    }
                                default:
                                    {
                                        return BuildingSubType.None;
                                    }
                            }
                        }
                    case BuildingType.ProcessingFacility:
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                            switch (building.Info.GetSubService())
                            {
                                case ItemClass.SubService.PlayerIndustryFarming:
                                    {
                                        return BuildingSubType.ProcessorCrops;
                                    }
                                case ItemClass.SubService.PlayerIndustryForestry:
                                    {
                                        return BuildingSubType.ProcessorLogs;
                                    }
                                case ItemClass.SubService.PlayerIndustryOil:
                                    {
                                        return BuildingSubType.ProcessorOil;
                                    }
                                case ItemClass.SubService.PlayerIndustryOre:
                                    {
                                        return BuildingSubType.ProcessorOre;
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
                    CustomTransferReason.Reason actualTransferReason = (CustomTransferReason.Reason) warehouseAI.GetActualTransferReason(buildingId, ref building);
                    return TransferManagerModes.IsImportRestrictionsSupported(actualTransferReason);
                } 
            }
            return false;
        }

        public static CustomTransferReason.Reason GetCargoFerryWarehouseActualTransferReason(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                PrefabAI buildingAI = building.Info.GetAI();
                if (buildingAI.GetType().ToString().Contains("CargoFerryWarehouseHarborAI"))
                {
                    if (s_CargoFerriesGetActualTransferReasonMethod is null)
                    {
                        try
                        {
                            Assembly? assembly = DependencyUtils.GetCargoFerriesAssembly();
                            if (assembly is not null)
                            {
                                s_CargoFerriesGetActualTransferReasonMethod = assembly.GetType("CargoFerries.AI.CargoFerryWarehouseHarborAI").GetMethod("GetActualTransferReason", BindingFlags.Public | BindingFlags.Instance);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex);
                        }
                    }
                    if (s_CargoFerriesGetActualTransferReasonMethod is not null)
                    {     
                        return (CustomTransferReason.Reason) s_CargoFerriesGetActualTransferReasonMethod.Invoke(buildingAI, new object[] { buildingId, building });
                    }
                }
            }
            return CustomTransferReason.Reason.None;
        }

        public static CustomTransferReason.Reason GetWarehouseTransferReason(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            WarehouseAI? warehouseAI = building.Info.GetAI() as WarehouseAI;
            if (warehouseAI is not null)
            {
                return (CustomTransferReason.Reason) warehouseAI.GetTransferReason(buildingId, ref building);
            }
            return CustomTransferReason.Reason.None;
        }

        public static TransferReason GetWarehouseActualTransferReason(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            WarehouseAI? warehouseAI = building.Info.GetAI() as WarehouseAI;
            if (warehouseAI is not null)
            {
                // Warehouses, just return the actual material they store
                return warehouseAI.GetActualTransferReason(buildingId, ref building);
            }
            return TransferReason.None;
        }

        public static bool IsProcessingFacilityWithVehicles(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            ProcessingFacilityAI? buildingAI = building.Info.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null)
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

        public static bool HasVehicles(ushort buildingId)
        {
            BuildingType eType = GetBuildingType(buildingId);
            switch (eType)
            {
                case BuildingType.Warehouse:
                case BuildingType.CargoFerryWarehouseHarbor:
                case BuildingType.ExtractionFacility:
                case BuildingType.GenericExtractor:
                case BuildingType.GenericProcessing:
                case BuildingType.GenericFactory:
                case BuildingType.FishHarbor:
                case BuildingType.FishFarm:
                case BuildingType.FishFactory:
                case BuildingType.PoliceStation:
                case BuildingType.PoliceHelicopterDepot:
                case BuildingType.Prison:
                case BuildingType.Bank:
                case BuildingType.FireStation:
                case BuildingType.FireHelicopterDepot:
                case BuildingType.Cemetery:
                case BuildingType.Hospital:
                case BuildingType.MedicalHelicopterDepot:
                case BuildingType.Landfill:
                case BuildingType.IncinerationPlant:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                case BuildingType.WasteTransfer:
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                case BuildingType.ParkMaintenanceDepot:
                case BuildingType.CargoStation:
                case BuildingType.DisasterResponseUnit:
                case BuildingType.DisasterShelter:
                case BuildingType.RoadMaintenanceDepot:
                case BuildingType.OutsideConnection:
                case BuildingType.TaxiDepot:
                case BuildingType.TransportStation:
                case BuildingType.TourDepot:
                case BuildingType.BusDepot:
                case BuildingType.TramDepot:
                case BuildingType.FerryDepot:
                case BuildingType.PassengerHelicopterDepot:
                case BuildingType.SnowDump:
                case BuildingType.PumpingService:
                case BuildingType.ServicePoint:
                    return true;

                case BuildingType.ProcessingFacility:
                case BuildingType.UniqueFactory:
                    return IsProcessingFacilityWithVehicles(buildingId);

                default:
                    return false;
            }
        }

        public static bool IsSameType(ushort buildingId, ushort otherBuildingId)
        {
            BuildingType eThisType = GetBuildingType(buildingId);
            BuildingType eOtherType = GetBuildingType(otherBuildingId);
            if (eThisType == eOtherType && eThisType != BuildingType.None)
            {
                // Check sub type is the same
                return GetBuildingSubType(buildingId) == GetBuildingSubType(otherBuildingId);
            }
            return false;
        }    
    }
}