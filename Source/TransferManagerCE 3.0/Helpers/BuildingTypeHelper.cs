using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using TransferManagerCE.CustomManager;
using static RenderManager;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.CustomTransferReason;

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
            Hotel,

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
            UniversityHospital,
            Library,
            Monument,
            CargoStation,
            CargoFerryWarehouseHarbor,
            Warehouse,
            WarehouseStation,
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
            FirewatchTower,
            Hospital,
            MedicalHelicopterDepot,
            Childcare,
            Eldercare,
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
            MainCampusBuilding,
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

            //CDebug.Log($"Service: {building.Info?.GetService()} SubService: {building.Info?.GetSubService()} AI: {building.Info?.GetAI()}");

            switch (building.Info.GetService())
            {
                case ItemClass.Service.Hotel:
                    {
                        return BuildingType.Hotel;
                    }
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
                        else if (building.Info?.GetAI() is WaterFacilityAI waterFacility)
                        {
                            if (waterFacility.m_pumpingVehicles > 0)
                            {
                                return BuildingType.PumpingService;
                            }
                            else
                            {
                                return BuildingType.Water;
                            }
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
                        if (building.Info is not null)
                        {
                            PrefabAI ai = building.Info.GetAI();
                            switch (ai)
                            {
                                case HospitalAI:
                                    return BuildingType.Hospital;
                                case HelicopterDepotAI:
                                    return BuildingType.MedicalHelicopterDepot;
                                case CemeteryAI:
                                    return BuildingType.Cemetery;
                                case ChildcareAI:
                                    return BuildingType.Childcare;
                                case EldercareAI:
                                    return BuildingType.Eldercare;
                                case UniqueFacultyAI:
                                    if (ai.GetType().ToString().Contains("UniversityHospitalAI"))
                                    {
                                        return BuildingType.UniversityHospital;
                                    }
                                    break;
                            }
                        }

                        return BuildingType.Healthcare;
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
                                                if (building.Info.GetAI().GetType().ToString().Contains("PrisonCopterPoliceStationAI"))
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
                                    switch (building.Info.GetAI())
                                    {
                                        case SchoolAI:
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
                                        case MainCampusBuildingAI:
                                            {
                                                return BuildingType.MainCampusBuilding;
                                            }
                                        default:
                                            {
                                                return BuildingType.CampusBuilding;
                                            }
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
                        switch (building.Info.GetAI())
                        {
                            case HelicopterDepotAI:
                                {
                                    return BuildingType.FireHelicopterDepot;
                                }                                
                            case FirewatchTowerAI:
                                {
                                    return BuildingType.FirewatchTower;
                                }
                            default:
                                {
                                    return BuildingType.FireStation;
                                }
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
                                case WarehouseStationAI: 
                                    return BuildingType.WarehouseStation;
                                case ExtractingFacilityAI: 
                                    return BuildingType.ExtractionFacility;
                                case UniqueFactoryAI: 
                                    return BuildingType.UniqueFactory;
                                case ProcessingFacilityAI: 
                                    return BuildingType.ProcessingFacility;
                                case MainIndustryBuildingAI: 
                                    return BuildingType.MainIndustryBuilding;

                                case CargoStationAI cargoStationAI:
                                    {
                                        if (cargoStationAI.GetType().ToString().Contains("CargoFerryWarehouseHarborAI"))
                                        {
                                            return BuildingType.CargoFerryWarehouseHarbor;
                                        }
                                        break;
                                    }

                                case WarehouseAI:
                                    {
                                        // Check if its the new warehouse with cargo station.
                                        if (building.m_subBuilding != 0)
                                        {
                                            Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[building.m_subBuilding];
                                            if (subBuilding.Info is not null && subBuilding.Info.GetAI() is WarehouseStationAI)
                                            {
                                                return BuildingType.WarehouseStation;
                                            }
                                        }
                                        return BuildingType.Warehouse;
                                    }
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
            //CDebug.Log($"Service: {building.Info?.GetService()} SubService: {building.Info?.GetSubService()} AI: {building.Info?.GetAI()}");
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
                    case BuildingType.WarehouseStation:
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

        public static bool IsWarehouse(BuildingType eType) 
        {
            switch (eType)
            {
                case BuildingType.Warehouse:
                case BuildingType.WarehouseStation:
                case BuildingType.CargoFerryWarehouseHarbor: 
                    return true;
                default:
                    return false;
            }
        }

        public static CustomTransferReason.Reason GetWarehouseTransferReason(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            switch (building.Info.GetAI())
            {
                case WarehouseAI warehouseAI:
                    {
                        return (CustomTransferReason.Reason) warehouseAI.GetTransferReason(buildingId, ref building);
                    }

                case CargoStationAI cargoStationAI:
                    {
                        Type buildingType = cargoStationAI.GetType();

                        if (buildingType.ToString().Contains("CargoFerryWarehouseHarborAI"))
                        {
                            MethodInfo? methodGetTransferReason = buildingType.GetMethod("GetTransferReason", BindingFlags.Public | BindingFlags.Instance);
                            return (CustomTransferReason.Reason)methodGetTransferReason.Invoke(cargoStationAI, new object[] { buildingId, building });
                        }

                        break;
                    }
            }

            return CustomTransferReason.Reason.None;
        }

        public static CustomTransferReason.Reason GetWarehouseActualTransferReason(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            if (building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case WarehouseAI warehouseAI:
                        {
                            return (CustomTransferReason.Reason)warehouseAI.GetActualTransferReason(buildingId, ref building);
                        }

                    case CargoStationAI cargoStationAI:
                        {
                            Type buildingType = cargoStationAI.GetType();
                            if (buildingType.ToString().Contains("CargoFerryWarehouseHarborAI"))
                            {
                                MethodInfo? methodGetActualTransferReason = buildingType.GetMethod("GetActualTransferReason", BindingFlags.Public | BindingFlags.Instance);
                                return (CustomTransferReason.Reason)methodGetActualTransferReason.Invoke(cargoStationAI, new object[] { buildingId, building });
                            }

                            break;
                        }
                }
            }

            return CustomTransferReason.Reason.None;
        }

        public static HashSet<CustomTransferReason.Reason> GetIncomingTransferReasons(ushort m_buildingId) 
        {
            HashSet<CustomTransferReason.Reason> transferReasons = new HashSet<CustomTransferReason.Reason>();

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.Info is not null) 
            {
                switch (building.Info.GetAI())
                {
                    case ProcessingFacilityAI facilityAI:
                        {
                            if (facilityAI.m_inputResource1 != TransferReason.None)
                            {
                                transferReasons.Add((CustomTransferReason.Reason)facilityAI.m_inputResource1);
                            }
                            if (facilityAI.m_inputResource2 != TransferReason.None)
                            {
                                transferReasons.Add((CustomTransferReason.Reason)facilityAI.m_inputResource2);
                            }
                            if (facilityAI.m_inputResource3 != TransferReason.None)
                            {
                                transferReasons.Add((CustomTransferReason.Reason)facilityAI.m_inputResource3);
                            }
                            if (facilityAI.m_inputResource4 != TransferReason.None)
                            {
                                transferReasons.Add((CustomTransferReason.Reason)facilityAI.m_inputResource4);
                            }
                            break;
                        }
                    case IndustrialBuildingAI industrialBuildingAI:
                        {
                            // Primary resource
                            MethodInfo? methodGetTransferReason = industrialBuildingAI.GetType().GetMethod("GetIncomingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodGetTransferReason is not null)
                            {
                                transferReasons.Add((CustomTransferReason.Reason)methodGetTransferReason.Invoke(industrialBuildingAI, new object[] { m_buildingId }));
                            }

                            // Secondary resource
                            MethodInfo? methodGetTransferReason2 = industrialBuildingAI.GetType().GetMethod("GetSecondaryIncomingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodGetTransferReason2 is not null)
                            {
                                transferReasons.Add((CustomTransferReason.Reason)methodGetTransferReason2.Invoke(industrialBuildingAI, new object[] { m_buildingId }));
                            }

                            break;
                        }
                }
            }

            return transferReasons;
        }

        public static CustomTransferReason.Reason GetOutgoingTransferReason(ushort m_buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case ProcessingFacilityAI facilityAI:
                        {
                            if (facilityAI.m_outputResource != TransferReason.None)
                            {
                                return (CustomTransferReason.Reason)facilityAI.m_outputResource;
                            }
                            break;
                        }
                    case ExtractingFacilityAI buildingAI:
                        {
                            if (buildingAI.m_outputResource != TransferReason.None)
                            {
                                return (CustomTransferReason.Reason)buildingAI.m_outputResource;
                            }
                            break;
                        }
                    case IndustrialBuildingAI industrialBuildingAI:
                        {
                            MethodInfo? methodGetOutgoingTransferReason = industrialBuildingAI.GetType().GetMethod("GetOutgoingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodGetOutgoingTransferReason is not null)
                            {
                                return (CustomTransferReason.Reason)methodGetOutgoingTransferReason.Invoke(industrialBuildingAI, new object[] { });
                            }

                            break;
                        }
                    
                    case IndustrialExtractorAI buildingAI:
                        {
                            MethodInfo? methodGetOutgoingTransferReason = buildingAI.GetType().GetMethod("GetOutgoingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodGetOutgoingTransferReason is not null)
                            {
                                return (CustomTransferReason.Reason)methodGetOutgoingTransferReason.Invoke(buildingAI, new object[] { });
                            }
                            break;
                        }
                }
            }

            return CustomTransferReason.Reason.None;
        }

        public static CustomTransferReason.Reason GetPrimaryIncomingTransferReason(ushort m_buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case IndustrialBuildingAI industrialBuildingAI:
                        {
                            // Primary resource
                            MethodInfo? methodGetTransferReason = industrialBuildingAI.GetType().GetMethod("GetIncomingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodGetTransferReason is not null)
                            {
                                return (CustomTransferReason.Reason)methodGetTransferReason.Invoke(industrialBuildingAI, new object[] { m_buildingId });
                            }

                            break;
                        }
                }
            }

            return CustomTransferReason.Reason.None;
        }

        public static CustomTransferReason.Reason GetSecondaryIncomingTransferReason(ushort m_buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case IndustrialBuildingAI industrialBuildingAI:
                        {
                            // Primary resource
                            MethodInfo? methodGetTransferReason = industrialBuildingAI.GetType().GetMethod("GetSecondaryIncomingTransferReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodGetTransferReason is not null)
                            {
                                return (CustomTransferReason.Reason)methodGetTransferReason.Invoke(industrialBuildingAI, new object[] { m_buildingId });
                            }

                            break;
                        }
                }
            }

            return CustomTransferReason.Reason.None;
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

        public static bool HasVehicles(BuildingType eType, ushort buildingId)
        {
            switch (eType)
            {
                case BuildingType.Warehouse:
                case BuildingType.WarehouseStation:
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
                case BuildingType.UniversityHospital:
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
                case BuildingType.CableCarStation:
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

        public static CustomTransferReason.Reason GetGlobalDistanceReason(BuildingType eType, ushort buildingId)
        {
            CustomTransferReason.Reason reason = Reason.None;

            switch (eType)
            {
                case BuildingType.Cemetery:
                    {
                        reason = CustomTransferReason.Reason.Dead;
                        break;
                    }
                case BuildingType.UniversityHospital:
                case BuildingType.Hospital:
                    {
                        reason = CustomTransferReason.Reason.Sick;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.FireStation:
                    {
                        reason = CustomTransferReason.Reason.Fire;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Landfill:
                case BuildingTypeHelper.BuildingType.IncinerationPlant:
                case BuildingTypeHelper.BuildingType.WasteProcessing:
                case BuildingTypeHelper.BuildingType.WasteTransfer:
                case BuildingTypeHelper.BuildingType.Recycling:
                    {
                        reason = CustomTransferReason.Reason.Garbage;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PoliceStation:
                    {
                        reason = CustomTransferReason.Reason.Crime;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PostOffice:
                    {
                        reason = CustomTransferReason.Reason.Mail;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ElementartySchool:
                    {
                        reason = CustomTransferReason.Reason.StudentES;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.HighSchool:
                    {
                        reason = CustomTransferReason.Reason.StudentHS;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.University:
                    {
                        reason = CustomTransferReason.Reason.StudentUni;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.SnowDump:
                    {
                        reason = CustomTransferReason.Reason.Snow;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.RoadMaintenanceDepot:
                    {
                        reason = CustomTransferReason.Reason.RoadMaintenance;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ParkMaintenanceDepot:
                    {
                        reason = CustomTransferReason.Reason.ParkMaintenance;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.TaxiDepot:
                case BuildingTypeHelper.BuildingType.TaxiStand:
                    {
                        reason = CustomTransferReason.Reason.Taxi;
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Bank:
                    {
                        reason = CustomTransferReason.Reason.Cash;
                        break;
                    }
                case BuildingType.UniqueFactory:
                    {
                        reason = CustomTransferReason.Reason.LuxuryProducts;
                        break;
                    }
            }

            if (reason == Reason.None && IsWarehouse(eType)) 
            {
                reason = (CustomTransferReason.Reason) GetWarehouseActualTransferReason(buildingId);
            }

            return reason;
        }

        // Comparing as string is slow so check each level so we only use string comparisons if absolutely necessary.
        public static bool IsUniversityHospital(Building building)
        {
            if (building.Info is not null && building.Info.GetService() == ItemClass.Service.HealthCare)
            {
                UniqueFacultyAI? ai = building.Info.GetAI() as UniqueFacultyAI;
                if (ai is not null)
                {
                    return ai.GetType().ToString().Contains("UniversityHospitalAI");
                }
            }

            return false;
        }

        public static bool IsPostSortingFacility(Building building)
        {
            return building.Info is not null &&
                   building.Info.GetService() == ItemClass.Service.PublicTransport &&
                   building.Info.GetSubService() == ItemClass.SubService.PublicTransportPost &&
                   building.Info.GetClassLevel() == ItemClass.Level.Level5;
        }
    }
}