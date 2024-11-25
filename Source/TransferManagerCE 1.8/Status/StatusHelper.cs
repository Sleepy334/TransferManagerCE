using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Data;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatusHelper
    {
        List<StatusContainer> m_listStatus;
        List<TransferReason> m_listAddedReasons;
        List<ushort> m_listAddedVehicles;
        public StatusHelper()
        {
            m_listStatus = new List<StatusContainer>();
            m_listAddedReasons = new List<TransferReason>();
            m_listAddedVehicles = new List<ushort>();
        }

        public List<StatusContainer> GetStatusList(ushort buildingId)
        {
            m_listStatus.Clear();

            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                BuildingTypeHelper.BuildingType eBuildingType = BuildingTypeHelper.GetBuildingType(buildingId);

                // Add status entries for each guest vehicle
                AddVehicles(eBuildingType, buildingId, building);

                // Now add status values for items that didnt have vehicles responding
                // Common to all (Services)
                AddCommonServices(eBuildingType, buildingId);

                // Building specific
                AddBuildingSpecific(eBuildingType, buildingId, building); 
            }

            return m_listStatus;
        }

        private void AddVehicles(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            List<ushort> vehicles = CitiesUtils.GetGuestVehiclesForBuilding(buildingId);
            foreach (ushort vehicleId in vehicles)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0 && vehicle.Info != null)
                {
                    ushort actualVehicleId = vehicleId;
                    // Check if it is loaded onto some other vehicle (Train/Ship/Plane)
                    if (vehicle.m_cargoParent != 0)
                    {
                        actualVehicleId = vehicle.m_cargoParent;
                    }

                    // Found a vehicle for this building
                    switch (vehicle.Info.m_vehicleAI)
                    {
                        case HearseAI:
                            {
                                // Hearse
                                m_listStatus.Add(new StatusContainer(new StatusDataDead(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Dead);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case AmbulanceAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataSick(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Sick);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case GarbageTruckAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataGarbage(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Garbage);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case FireTruckAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataFire(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Fire);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case FireCopterAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataFire(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Fire);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case PoliceCarAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataCrime(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Crime);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case PoliceCopterAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataCrime(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.Crime);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case PostVanAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataMail((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case ParkMaintenanceVehicleAI:
                            {
                                m_listStatus.Add(new StatusContainer(new StatusParkMaintenance(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                m_listAddedReasons.Add(TransferReason.ParkMaintenance);
                                m_listAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case CargoTruckAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingTypeHelper.BuildingType.Commercial:
                                        {
                                            m_listStatus.Add(new StatusContainer(new StatusDataCommercial(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add(TransferReason.Goods);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.Warehouse:
                                        {
                                            WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                                            if (warehouseAI != null)
                                            {
                                                TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                                                m_listStatus.Add(new StatusContainer(new StatusDataWarehouse(actualTransferReason, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                                m_listAddedReasons.Add(actualTransferReason);
                                                m_listAddedVehicles.Add(actualVehicleId);
                                            }
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.ProcessingFacility:
                                    case BuildingTypeHelper.BuildingType.UniqueFactory:
                                        {
                                            StatusDataProcessingFacility truck = new StatusDataProcessingFacility((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId);
                                            m_listStatus.Add(new StatusContainer(truck));
                                            m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.GenericExtractor:
                                        {
                                            m_listStatus.Add(new StatusContainer(new StatusIndustrialExtractor((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.GenericProcessing:
                                        {
                                            m_listStatus.Add(new StatusContainer(new StatusIndustrialProcessing((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.ExtractionFacility:
                                        {
                                            m_listStatus.Add(new StatusContainer(new StatusDataExtractionFacility((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.DisasterShelter:
                                        {
                                            // Add a generic vehicle
                                            m_listStatus.Add(new StatusContainer(new StatusDataShelter(TransferReason.Food, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add(TransferReason.Food);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.PowerPlant:
                                        {
                                            // Add a generic vehicle
                                            m_listStatus.Add(new StatusContainer(new StatusPowerPlant((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingTypeHelper.BuildingType.FishMarket:
                                        {
                                            // Add a generic vehicle
                                            m_listStatus.Add(new StatusContainer(new StatusDataMarket((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                            m_listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_listAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    default:
                                        {
                                            if (!m_listAddedVehicles.Contains(actualVehicleId))
                                            {
                                                // Add a generic vehicle
                                                m_listStatus.Add(new StatusContainer(new StatusDataGeneric((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                                m_listAddedVehicles.Add(actualVehicleId);
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        default:
                            {
                                if (!m_listAddedVehicles.Contains(actualVehicleId))
                                {
                                    // Add a generic vehicle
                                    m_listStatus.Add(new StatusContainer(new StatusDataGeneric((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId)));
                                    m_listAddedVehicles.Add(actualVehicleId);
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void AddCommonServices(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId)
        {  
            // Common to all
            if (!m_listAddedReasons.Contains(TransferReason.Dead))
            {
                m_listStatus.Add(new StatusContainer(new StatusDataDead(eBuildingType, buildingId, 0, 0)));
                m_listAddedReasons.Add(TransferReason.Dead);
            }
            if (!m_listAddedReasons.Contains(TransferReason.Sick))
            {
                m_listStatus.Add(new StatusContainer(new StatusDataSick(eBuildingType, buildingId, 0, 0)));
                m_listAddedReasons.Add(TransferReason.Sick);
            }
            if (!m_listAddedReasons.Contains(TransferReason.Garbage))
            {
                m_listStatus.Add(new StatusContainer(new StatusDataGarbage(eBuildingType, buildingId, 0, 0)));
                m_listAddedReasons.Add(TransferReason.Garbage);
            }
            if (!m_listAddedReasons.Contains(TransferReason.Fire))
            {
                m_listStatus.Add(new StatusContainer(new StatusDataFire(eBuildingType, buildingId, 0, 0)));
                m_listAddedReasons.Add(TransferReason.Fire);
            }
            if (!m_listAddedReasons.Contains(TransferReason.Crime))
            {
                m_listStatus.Add(new StatusContainer(new StatusDataCrime(eBuildingType, buildingId, 0, 0)));
                m_listAddedReasons.Add(TransferReason.Crime);
            }
            if (eBuildingType != BuildingTypeHelper.BuildingType.PostOffice &&
                eBuildingType != BuildingTypeHelper.BuildingType.PostSortingFacility &&
                !m_listAddedReasons.Contains(TransferReason.Mail))
            {
                m_listStatus.Add(new StatusContainer(new StatusDataMail(TransferReason.Mail, eBuildingType, buildingId, 0, 0)));
                m_listAddedReasons.Add(TransferReason.Mail);
            }
        }

        private void AddBuildingSpecific(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Building specific
            switch (eBuildingType)
            {
                case BuildingTypeHelper.BuildingType.Warehouse:
                    {
                        WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                        if (warehouseAI != null)
                        {
                            TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                            if (!m_listAddedReasons.Contains(actualTransferReason))
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataWarehouse(actualTransferReason, eBuildingType, buildingId, 0, 0)));
                            }
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ProcessingFacility:
                case BuildingTypeHelper.BuildingType.UniqueFactory:
                    {
                        ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                        if (buildingAI != null)
                        {
                            if (buildingAI.m_inputResource1 != TransferReason.None && !m_listAddedReasons.Contains(buildingAI.m_inputResource1))
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource1, eBuildingType, buildingId, 0, 0)));
                            }
                            if (buildingAI.m_inputResource2 != TransferReason.None && !m_listAddedReasons.Contains(buildingAI.m_inputResource2))
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource2, eBuildingType, buildingId, 0, 0)));
                            }
                            if (buildingAI.m_inputResource3 != TransferReason.None && !m_listAddedReasons.Contains(buildingAI.m_inputResource3))
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource3, eBuildingType, buildingId, 0, 0)));
                            }
                            if (buildingAI.m_inputResource4 != TransferReason.None && !m_listAddedReasons.Contains(buildingAI.m_inputResource4))
                            {
                                m_listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource4, eBuildingType, buildingId, 0, 0)));
                            }
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Commercial:
                    {
                        if (!m_listAddedReasons.Contains(TransferReason.Goods) && !m_listAddedReasons.Contains(TransferReason.Food))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusDataCommercial(eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.DisasterShelter:
                    {
                        bool bContainsFood = m_listAddedReasons.Contains(TransferReason.Goods) || m_listAddedReasons.Contains(TransferReason.Food);
                        if (!bContainsFood)
                        {
                            m_listStatus.Add(new StatusContainer(new StatusDataShelter(TransferReason.Food, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.GenericExtractor:
                    {
                        TransferReason material = StatusIndustrialExtractor.GetOutgoingTransferReason(building);
                        if (material != TransferReason.None && !m_listAddedReasons.Contains(material))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusIndustrialExtractor(material, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.GenericProcessing:
                    {
                        TransferReason material = StatusIndustrialProcessing.GetIncomingTransferReason(buildingId);
                        if (material != TransferReason.None && !m_listAddedReasons.Contains(material))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusIndustrialProcessing(material, eBuildingType, buildingId, 0, 0)));
                        }
                        TransferReason material2 = StatusIndustrialProcessing.GetSecondaryIncomingTransferReason(buildingId);
                        if (material2 != TransferReason.None && !m_listAddedReasons.Contains(material2))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusIndustrialProcessing(material2, eBuildingType, buildingId, 0, 0)));
                        }
                        TransferReason outMaterial = StatusIndustrialProcessing.GetOutgoingTransferReason(building);
                        if (outMaterial != TransferReason.None && !m_listAddedReasons.Contains(outMaterial))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusIndustrialProcessing(outMaterial, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }

                case BuildingTypeHelper.BuildingType.Park:
                    {
                        if (!m_listAddedReasons.Contains(TransferReason.ParkMaintenance))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusParkMaintenance(eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ExtractionFacility:
                    {
                        TransferReason material = StatusDataExtractionFacility.GetOutputResource(buildingId);
                        if (!m_listAddedReasons.Contains(material))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusDataExtractionFacility(material, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PowerPlant:
                    {
                        TransferReason material = StatusPowerPlant.GetInputResource(buildingId);
                        if (material != TransferReason.None && !m_listAddedReasons.Contains(material))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusPowerPlant(material, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PostOffice:
                case BuildingTypeHelper.BuildingType.PostSortingFacility:
                    {
                        if (!m_listAddedReasons.Contains(TransferReason.UnsortedMail))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusDataMail(TransferReason.UnsortedMail, eBuildingType, buildingId, 0, 0)));
                        }
                        if (!m_listAddedReasons.Contains(TransferReason.SortedMail) && !m_listAddedReasons.Contains(TransferReason.IncomingMail))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusDataMail(TransferReason.SortedMail, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.FishMarket:
                    {
                        if (!m_listAddedReasons.Contains(TransferReason.Fish))
                        {
                            m_listStatus.Add(new StatusContainer(new StatusDataMarket(TransferReason.Fish, eBuildingType, buildingId, 0, 0)));
                        }
                        break;
                    }
            }
        }
    }
}