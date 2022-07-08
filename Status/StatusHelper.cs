using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Data;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatusHelper
    {
        public static List<StatusContainer> GetStatusList(ushort buildingId)
        {
            List<StatusContainer> listStatus = new List<StatusContainer>();

            if (buildingId != 0)
            {
                List<TransferReason> listAddedReasons = new List<TransferReason>();

                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                BuildingTypeHelper.BuildingType eBuildingType = BuildingTypeHelper.GetBuildingType(buildingId);

                List<ushort> vehicles = CitiesUtils.GetGuestVehiclesForBuilding(buildingId);
                foreach (ushort vehicleId in vehicles)
                {
                    Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                    if (vehicle.Info != null)
                    {
                        // Found a vehicle for this building
                        if (vehicle.Info.m_vehicleAI is HearseAI)
                        {
                            // Hearse
                            listStatus.Add(new StatusContainer(new StatusDataDead(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Dead);
                        }
                        else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataSick(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Sick);
                        }
                        else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataGarbage(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Garbage);
                        }
                        else if (vehicle.Info.m_vehicleAI is FireTruckAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataFire(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Fire); 
                        }
                        else if (vehicle.Info.m_vehicleAI is FireCopterAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataFire2(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Fire2);
                        }
                        else if (vehicle.Info.m_vehicleAI is PoliceCarAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataCrime(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Crime); 
                        }
                        else if (vehicle.Info.m_vehicleAI is PoliceCopterAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataCrime(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Crime);
                        }
                        else if (vehicle.Info.m_vehicleAI is PostVanAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataMail(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.Mail); 
                        }
                        else if (vehicle.Info.GetAI() is ParkMaintenanceVehicleAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusParkMaintenance(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            listAddedReasons.Add(TransferReason.ParkMaintenance);
                        }
                        else if (vehicle.Info.m_vehicleAI is CargoTruckAI)
                        {
                            switch (eBuildingType)
                            {
                                case BuildingTypeHelper.BuildingType.Commercial:
                                    {
                                        listStatus.Add(new StatusContainer(new StatusDataCommercial(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                                        listAddedReasons.Add(TransferReason.Goods);
                                        break;
                                    }
                                case BuildingTypeHelper.BuildingType.Warehouse:
                                    {
                                        WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                                        if (warehouseAI != null)
                                        {
                                            TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                                            listStatus.Add(new StatusContainer(new StatusDataWarehouse(actualTransferReason, buildingId, vehicle.m_sourceBuilding, vehicleId)));
                                            listAddedReasons.Add(actualTransferReason);
                                        }
                                        break;
                                    }
                                case BuildingTypeHelper.BuildingType.ProcessingFacility:
                                case BuildingTypeHelper.BuildingType.UniqueFactory:
                                    {
                                        StatusDataProcessingFacility truck = new StatusDataProcessingFacility((TransferReason)vehicle.m_transferType, buildingId, vehicle.m_sourceBuilding, vehicleId);
                                        listStatus.Add(new StatusContainer(truck));
                                        listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                        break;
                                    }
                                case BuildingTypeHelper.BuildingType.GenericIndustry:
                                    {
                                        listStatus.Add(new StatusContainer(new StatusDataIndustrial((TransferReason)vehicle.m_transferType, buildingId, vehicle.m_sourceBuilding, vehicleId)));
                                        listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                        break;
                                    }
                                case BuildingTypeHelper.BuildingType.ExtractionFacility:
                                case BuildingTypeHelper.BuildingType.FishingHarbor:
                                case BuildingTypeHelper.BuildingType.FishFarm:
                                    {
                                        listStatus.Add(new StatusContainer(new StatusDataIndustry((TransferReason)vehicle.m_transferType, buildingId, vehicle.m_sourceBuilding, vehicleId)));
                                        listAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                        break;
                                    }
                                case BuildingTypeHelper.BuildingType.DisasterShelter:
                                    {
                                        // Add a generic vehicle
                                        listStatus.Add(new StatusContainer(new StatusDataShelter(TransferReason.Food, buildingId, vehicle.m_sourceBuilding, vehicleId)));
                                        listAddedReasons.Add(TransferReason.Food);
                                        break;
                                    }
                                default:
                                    {
                                        // Add a generic vehicle
                                        listStatus.Add(new StatusContainer(new StatusDataGeneric(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            // Add a generic vehicle
                            listStatus.Add(new StatusContainer(new StatusDataGeneric(buildingId, vehicle.m_sourceBuilding, vehicleId)));
                        }
                    }
                }

                // Now add status values for items that didnt have vehicles responding

                // Common to all
                if (!listAddedReasons.Contains(TransferReason.Dead))
                {
                    listStatus.Add(new StatusContainer(new StatusDataDead(buildingId, 0, 0)));
                }
                if (!listAddedReasons.Contains(TransferReason.Sick))
                {
                    listStatus.Add(new StatusContainer(new StatusDataSick(buildingId, 0, 0)));
                }
                if (!listAddedReasons.Contains(TransferReason.Garbage))
                {
                    listStatus.Add(new StatusContainer(new StatusDataGarbage(buildingId, 0, 0)));
                }
                if (!listAddedReasons.Contains(TransferReason.Fire))
                {
                    listStatus.Add(new StatusContainer(new StatusDataFire(buildingId, 0, 0)));
                }
                if (!listAddedReasons.Contains(TransferReason.Crime))
                {
                    listStatus.Add(new StatusContainer(new StatusDataCrime(buildingId, 0, 0)));
                }
                if (!listAddedReasons.Contains(TransferReason.Mail))
                {
                    listStatus.Add(new StatusContainer(new StatusDataMail(buildingId, 0, 0)));
                }

                // Building specific
                switch (eBuildingType)
                {
                    case BuildingTypeHelper.BuildingType.Warehouse:
                        {
                            
                            {
                                WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                                if (warehouseAI != null)
                                {
                                    TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                                    if (!listAddedReasons.Contains(actualTransferReason))
                                    {
                                        listStatus.Add(new StatusContainer(new StatusDataWarehouse(actualTransferReason, buildingId, 0, 0)));
                                    }
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
                                if (buildingAI.m_inputResource1 != TransferReason.None && !listAddedReasons.Contains(buildingAI.m_inputResource1))
                                {
                                    listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource1, buildingId, 0, 0)));
                                }
                                if (buildingAI.m_inputResource2 != TransferReason.None && !listAddedReasons.Contains(buildingAI.m_inputResource2))
                                {
                                    listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource2, buildingId, 0, 0)));
                                }
                                if (buildingAI.m_inputResource3 != TransferReason.None && !listAddedReasons.Contains(buildingAI.m_inputResource3))
                                {
                                    listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource3, buildingId, 0, 0)));
                                }
                                if (buildingAI.m_inputResource4 != TransferReason.None && !listAddedReasons.Contains(buildingAI.m_inputResource4))
                                {
                                    listStatus.Add(new StatusContainer(new StatusDataProcessingFacility(buildingAI.m_inputResource4, buildingId, 0, 0)));
                                }
                            }
                            break;
                        }
                    case BuildingTypeHelper.BuildingType.Commercial:
                        {
                            if (!listAddedReasons.Contains(TransferReason.Goods) && !listAddedReasons.Contains(TransferReason.Food))
                            {
                                listStatus.Add(new StatusContainer(new StatusDataCommercial(buildingId, 0, 0)));
                            }
                            break;
                        }
                    case BuildingTypeHelper.BuildingType.DisasterShelter:
                        {
                            bool bContainsFood = listAddedReasons.Contains(TransferReason.Goods) || listAddedReasons.Contains(TransferReason.Food);
                            if (!bContainsFood)
                            { 
                                listStatus.Add(new StatusContainer(new StatusDataShelter(TransferReason.Food, buildingId, 0, 0)));
                            }
                            break;
                        }
                    case BuildingTypeHelper.BuildingType.GenericIndustry:
                        {
                            TransferReason material = StatusDataIndustrial.GetIncomingTransferReason(buildingId);
                            if (!listAddedReasons.Contains(material))
                            {
                                listStatus.Add(new StatusContainer(new StatusDataIndustrial(material, buildingId, 0, 0)));
                            }
                            break;
                        }
                    case BuildingTypeHelper.BuildingType.Park:
                        {
                            if (!listAddedReasons.Contains(TransferReason.ParkMaintenance))
                            {
                                listStatus.Add(new StatusContainer(new StatusParkMaintenance(buildingId, 0, 0)));
                            }
                            break;
                        }
                }
            }

            return listStatus;
        }
    }
}