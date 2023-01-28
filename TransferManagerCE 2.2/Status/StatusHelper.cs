using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class StatusHelper
    {
        List<StatusData> m_listServices;
        List<StatusData> m_listIncoming;
        List<StatusData> m_listOutgoing;

        HashSet<TransferReason> m_setAddedReasons;
        HashSet<ushort> m_setAddedVehicles;

        public StatusHelper()
        {
            m_listServices = new List<StatusData>();
            m_listIncoming = new List<StatusData>();
            m_listOutgoing = new List<StatusData>();
            m_setAddedReasons = new HashSet<TransferReason>();
            m_setAddedVehicles = new HashSet<ushort>();
        }

        public List<StatusData> GetStatusList(ushort buildingId)
        {
            List<StatusData> list = new List<StatusData>();

            if (buildingId != 0)
            {
                m_listServices.Clear();
                m_listIncoming.Clear();
                m_listOutgoing.Clear();
                m_setAddedReasons.Clear();
                m_setAddedVehicles.Clear();

                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                BuildingType eBuildingType = GetBuildingType(buildingId);

                // Add status entries for each guest vehicle
                AddVehicles(eBuildingType, buildingId, building);

                // Now add status values for items that didnt have vehicles responding
                // Common to all (Services)
                AddCommonServices(eBuildingType, buildingId);

                // Building specific
                AddBuildingSpecific(eBuildingType, buildingId, building);

                if (m_listServices.Count > 0)
                {
                    m_listServices.Sort();
                    m_listServices.Reverse();
                    list.AddRange(m_listServices);
                }

                if (m_listOutgoing.Count > 0)
                {
                    list.Add(new StatusDataSeparator());
                    m_listOutgoing.Sort();
                    m_listOutgoing.Reverse();
                    list.AddRange(m_listOutgoing);
                }

                if (m_listIncoming.Count > 0)
                {
                    list.Add(new StatusDataSeparator());
                    m_listIncoming.Sort();
                    m_listIncoming.Reverse();
                    list.AddRange(m_listIncoming);
                }
            }

            return list;
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
                                switch (eBuildingType)
                                {
                                    case BuildingType.Cemetery:
                                        m_listIncoming.Add(new StatusDataDead(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataDead(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add(TransferReason.Dead);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case AmbulanceCopterAI:
                        case AmbulanceAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.Hospital:
                                    case BuildingType.MedicalHelicopterDepot:
                                        m_listIncoming.Add(new StatusDataSick(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataSick(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add(TransferReason.Sick);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case GarbageTruckAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.Recycling:
                                    case BuildingType.WasteProcessing:
                                    case BuildingType.WasteTransfer:
                                    case BuildingType.Landfill:
                                    case BuildingType.IncinerationPlant:
                                        m_listIncoming.Add(new StatusDataGarbage((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataGarbage((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add(TransferReason.Garbage);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case FireTruckAI:
                        case FireCopterAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.FireStation:
                                    case BuildingType.FireHelicopterDepot:
                                        m_listIncoming.Add(new StatusDataFire(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataFire(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add(TransferReason.Fire);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case PoliceCarAI:
                        case PoliceCopterAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.PoliceHelicopterDepot:
                                    case BuildingType.PoliceStation:
                                        m_listIncoming.Add(new StatusDataCrime((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataCrime((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add((TransferReason) vehicle.m_transferType);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case BankVanAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.Bank:
                                        m_listIncoming.Add(new StatusDataCash(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataCash(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add(TransferReason.Cash);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case PostVanAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.PostOffice:
                                    case BuildingType.PostSortingFacility:
                                        TransferReason material = (TransferReason)vehicle.m_transferType;
                                        if (material == TransferReason.IncomingMail)
                                        {
                                            // PostSorting facilities actually import their own output (wtf), ie sorted mail as IncomingMail.
                                            m_listOutgoing.Add(new StatusDataMail(material, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        }
                                        else
                                        {
                                            m_listIncoming.Add(new StatusDataMail(material, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        }
                                        break;
                                    default:
                                        m_listServices.Add(new StatusDataMail((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                        break;
                                }
                                m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case ParkMaintenanceVehicleAI:
                            {
                                m_listIncoming.Add(new StatusParkMaintenance(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                m_setAddedReasons.Add(TransferReason.ParkMaintenance);
                                m_setAddedVehicles.Add(actualVehicleId);
                                break;
                            }
                        case CargoTruckAI:
                            {
                                switch (eBuildingType)
                                {
                                    case BuildingType.Commercial:
                                        {
                                            m_listIncoming.Add(new StatusDataCommercial(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add(TransferReason.Goods);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.Warehouse:
                                        {
                                            WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                                            if (warehouseAI != null)
                                            {
                                                TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                                                m_listIncoming.Add(new StatusDataWarehouse(actualTransferReason, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                m_setAddedReasons.Add(actualTransferReason);
                                                m_setAddedVehicles.Add(actualVehicleId);
                                            }
                                            break;
                                        }
                                    case BuildingType.ProcessingFacility:
                                    case BuildingType.UniqueFactory:
                                        {
                                            StatusDataProcessingFacility truck = new StatusDataProcessingFacility((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId);
                                            m_listIncoming.Add(truck);
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.GenericExtractor:
                                        {
                                            m_listIncoming.Add(new StatusGenericExtractor((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.GenericProcessing:
                                    case BuildingType.GenericFactory:
                                        {
                                            m_listIncoming.Add(new StatusGenericProcessing((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.DisasterShelter:
                                        {
                                            // Add a generic vehicle
                                            m_listIncoming.Add(new StatusDataShelter(TransferReason.Food, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add(TransferReason.Food);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.BoilerStation:
                                        {
                                            // Add a generic vehicle
                                            m_listIncoming.Add(new StatusWaterPlant((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.PetrolPowerPlant:
                                    case BuildingType.CoalPowerPlant:
                                        {
                                            // Add a generic vehicle
                                            m_listIncoming.Add(new StatusPowerPlant((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.FishFactory:
                                        {
                                            // Add a generic vehicle
                                            m_listIncoming.Add(new StatusDataFishFactory((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.FishMarket:
                                        {
                                            // Add a generic vehicle
                                            m_listIncoming.Add(new StatusDataMarket((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    case BuildingType.ServicePoint:
                                        {
                                            // Add a generic vehicle
                                            m_listIncoming.Add(new StatusDataServicePoint((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                            m_setAddedVehicles.Add(actualVehicleId);
                                            break;
                                        }
                                    default:
                                        {
                                            if (!m_setAddedVehicles.Contains(actualVehicleId))
                                            {
                                                // Add a generic vehicle
                                                m_listIncoming.Add(new StatusDataGeneric((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                m_setAddedVehicles.Add(actualVehicleId);
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        default:
                            {
                                if (!m_setAddedVehicles.Contains(actualVehicleId))
                                {
                                    // Add a generic vehicle
                                    m_listIncoming.Add(new StatusDataGeneric((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                    m_setAddedVehicles.Add(actualVehicleId);
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
            if (!m_setAddedReasons.Contains(TransferReason.Dead))
            {
                switch (eBuildingType)
                {
                    case BuildingType.Cemetery:
                        m_listIncoming.Add(new StatusDataDead(eBuildingType, buildingId, 0, 0));
                        break;
                    default:
                        m_listServices.Add(new StatusDataDead(eBuildingType, buildingId, 0, 0));
                        break;
                }
                m_setAddedReasons.Add(TransferReason.Dead);
            }
            if (!(m_setAddedReasons.Contains(TransferReason.Sick) || m_setAddedReasons.Contains(TransferReason.Sick2)))
            {
                switch (eBuildingType)
                {
                    case BuildingType.Hospital:
                    case BuildingType.MedicalHelicopterDepot:
                        m_listIncoming.Add(new StatusDataSick(eBuildingType, buildingId, 0, 0));
                        break;
                    default:
                        m_listServices.Add(new StatusDataSick(eBuildingType, buildingId, 0, 0));
                        break;
                }
                m_setAddedReasons.Add(TransferReason.Sick);
            }
            if (!m_setAddedReasons.Contains(TransferReason.Garbage))
            {
                switch (eBuildingType)
                {
                    case BuildingType.Recycling:
                    case BuildingType.WasteProcessing:
                        m_listIncoming.Add(new StatusDataGarbage(TransferReason.Garbage, eBuildingType, buildingId, 0, 0));
                        m_listOutgoing.Add(new StatusDataGarbage(TransferReason.Goods, eBuildingType, buildingId, 0, 0));
                        break;
                    case BuildingType.WasteTransfer:
                    case BuildingType.Landfill:
                    case BuildingType.IncinerationPlant:
                        m_listIncoming.Add(new StatusDataGarbage(TransferReason.Garbage, eBuildingType, buildingId, 0, 0));
                        break;
                    default:
                        m_listServices.Add(new StatusDataGarbage(TransferReason.Garbage, eBuildingType, buildingId, 0, 0));
                        break;
                }
                m_setAddedReasons.Add(TransferReason.Garbage);
            }
            if (!m_setAddedReasons.Contains(TransferReason.Fire))
            {
                switch (eBuildingType)
                {
                    case BuildingType.FireStation:
                    case BuildingType.FireHelicopterDepot:
                        m_listIncoming.Add(new StatusDataFire(eBuildingType, buildingId, 0, 0));
                        break;
                    default:
                        m_listServices.Add(new StatusDataFire(eBuildingType, buildingId, 0, 0));
                        break;
                }
                m_setAddedReasons.Add(TransferReason.Fire);
            }
            if (!(m_setAddedReasons.Contains(TransferReason.Crime) || m_setAddedReasons.Contains((TransferReason) CustomTransferReason.Reason.Crime2)))
            {
                switch (eBuildingType)
                {
                    case BuildingType.PoliceHelicopterDepot:
                        m_listIncoming.Add(new StatusDataCrime((TransferReason)CustomTransferReason.Reason.Crime2, eBuildingType, buildingId, 0, 0));
                        m_setAddedReasons.Add((TransferReason) CustomTransferReason.Reason.Crime2);
                        break;
                    case BuildingType.PoliceStation:
                    case BuildingType.Prison:
                        m_listIncoming.Add(new StatusDataCrime(TransferReason.Crime, eBuildingType, buildingId, 0, 0));
                        m_setAddedReasons.Add(TransferReason.Crime);
                        break;
                    default:
                        m_listServices.Add(new StatusDataCrime(TransferReason.Crime, eBuildingType, buildingId, 0, 0));
                        m_setAddedReasons.Add(TransferReason.Crime);
                        break;
                }
                
            }
            if (!m_setAddedReasons.Contains(TransferReason.Cash))
            {
                switch (eBuildingType)
                {
                    case BuildingType.Bank:
                        m_listIncoming.Add(new StatusDataCash(eBuildingType, buildingId, 0, 0));
                        break;

                    // Banks only supply services to commercial buildings
                    case BuildingType.Commercial:
                        m_listServices.Add(new StatusDataCash(eBuildingType, buildingId, 0, 0));
                        break;
                }
                m_setAddedReasons.Add(TransferReason.Cash);
            }
            if (!m_setAddedReasons.Contains(TransferReason.Mail))
            {
                switch (eBuildingType)
                {
                    case BuildingType.PostOffice:
                    case BuildingType.PostSortingFacility:
                        //m_listIncoming.Add(new StatusDataMail(TransferReason.Mail, eBuildingType, buildingId, 0, 0));
                        break;
                    default:
                        m_listServices.Add(new StatusDataMail(TransferReason.Mail, eBuildingType, buildingId, 0, 0));
                        break;
                }
                m_setAddedReasons.Add(TransferReason.Mail);
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
                            if (!m_setAddedReasons.Contains(actualTransferReason))
                            {
                                m_listIncoming.Add(new StatusDataWarehouse(actualTransferReason, eBuildingType, buildingId, 0, 0));
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
                            if (buildingAI.m_inputResource1 != TransferReason.None && !m_setAddedReasons.Contains(buildingAI.m_inputResource1))
                            {
                                m_listIncoming.Add(new StatusDataProcessingFacility(buildingAI.m_inputResource1, eBuildingType, buildingId, 0, 0));
                            }
                            if (buildingAI.m_inputResource2 != TransferReason.None && !m_setAddedReasons.Contains(buildingAI.m_inputResource2))
                            {
                                m_listIncoming.Add(new StatusDataProcessingFacility(buildingAI.m_inputResource2, eBuildingType, buildingId, 0, 0));
                            }
                            if (buildingAI.m_inputResource3 != TransferReason.None && !m_setAddedReasons.Contains(buildingAI.m_inputResource3))
                            {
                                m_listIncoming.Add(new StatusDataProcessingFacility(buildingAI.m_inputResource3, eBuildingType, buildingId, 0, 0));
                            }
                            if (buildingAI.m_inputResource4 != TransferReason.None && !m_setAddedReasons.Contains(buildingAI.m_inputResource4))
                            {
                                m_listIncoming.Add(new StatusDataProcessingFacility(buildingAI.m_inputResource4, eBuildingType, buildingId, 0, 0));
                            }
                            m_listOutgoing.Add(new StatusDataProcessingFacility(buildingAI.m_outputResource, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingType.FishFactory:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.Fish))
                        {
                            m_listIncoming.Add(new StatusDataFishFactory(TransferReason.Fish, eBuildingType, buildingId, 0, 0));
                        }
                        if (!m_setAddedReasons.Contains(TransferReason.Goods))
                        {
                            m_listOutgoing.Add(new StatusDataFishFactory(TransferReason.Goods, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Commercial:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.Goods) && !m_setAddedReasons.Contains(TransferReason.Food))
                        {
                            m_listIncoming.Add(new StatusDataCommercial(eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.DisasterShelter:
                    {
                        bool bContainsFood = m_setAddedReasons.Contains(TransferReason.Goods) || m_setAddedReasons.Contains(TransferReason.Food);
                        if (!bContainsFood)
                        {
                            m_listIncoming.Add(new StatusDataShelter(TransferReason.Food, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.GenericExtractor:
                    {
                        TransferReason material = StatusGenericExtractor.GetOutgoingTransferReason(building);
                        if (material != TransferReason.None && !m_setAddedReasons.Contains(material))
                        {
                            m_listOutgoing.Add(new StatusGenericExtractor(material, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.GenericProcessing:
                case BuildingTypeHelper.BuildingType.GenericFactory:
                    {
                        TransferReason material = StatusGenericProcessing.GetIncomingTransferReason(buildingId);
                        if (material != TransferReason.None && !m_setAddedReasons.Contains(material))
                        {
                            m_listIncoming.Add(new StatusGenericProcessing(material, eBuildingType, buildingId, 0, 0));
                        }
                        TransferReason material2 = StatusGenericProcessing.GetSecondaryIncomingTransferReason(buildingId);
                        if (material2 != TransferReason.None && !m_setAddedReasons.Contains(material2))
                        {
                            m_listIncoming.Add(new StatusGenericProcessing(material2, eBuildingType, buildingId, 0, 0));
                        }
                        TransferReason outMaterial = StatusGenericProcessing.GetOutgoingTransferReason(building);
                        if (outMaterial != TransferReason.None && !m_setAddedReasons.Contains(outMaterial))
                        {
                            m_listOutgoing.Add(new StatusGenericProcessing(outMaterial, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }

                case BuildingTypeHelper.BuildingType.Park:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.ParkMaintenance))
                        {
                            m_listIncoming.Add(new StatusParkMaintenance(eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingType.FishFarm:
                case BuildingType.FishHarbor:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.Fish))
                        {
                            m_listOutgoing.Add(new StatusDataFishHarbor(eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ExtractionFacility:
                    {
                        TransferReason material = StatusDataExtractionFacility.GetOutputResource(buildingId);
                        if (!m_setAddedReasons.Contains(material))
                        {
                            m_listOutgoing.Add(new StatusDataExtractionFacility(material, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.BoilerStation:
                    {
                        TransferReason material = StatusWaterPlant.GetInputResource(buildingId);
                        if (material != TransferReason.None && !m_setAddedReasons.Contains(material))
                        {
                            m_listIncoming.Add(new StatusWaterPlant(material, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PetrolPowerPlant:
                case BuildingTypeHelper.BuildingType.CoalPowerPlant:
                    {
                        TransferReason material = StatusPowerPlant.GetInputResource(buildingId);
                        if (material != TransferReason.None && !m_setAddedReasons.Contains(material))
                        {
                            m_listIncoming.Add(new StatusPowerPlant(material, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PostOffice:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.UnsortedMail))
                        {
                            m_listOutgoing.Add(new StatusDataMail(TransferReason.UnsortedMail, eBuildingType, buildingId, 0, 0));
                        }
                        if (!m_setAddedReasons.Contains(TransferReason.SortedMail) && !m_setAddedReasons.Contains(TransferReason.IncomingMail))
                        {
                            m_listIncoming.Add(new StatusDataMail(TransferReason.SortedMail, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PostSortingFacility:
                    {
                        // Incoming
                        if (!m_setAddedReasons.Contains(TransferReason.UnsortedMail))
                        {
                            m_listIncoming.Add(new StatusDataMail(TransferReason.UnsortedMail, eBuildingType, buildingId, 0, 0));
                        }
                        // Outgoing
                        if (!m_setAddedReasons.Contains(TransferReason.SortedMail))
                        {
                            m_listOutgoing.Add(new StatusDataMail(TransferReason.SortedMail, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.FishMarket:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.Fish))
                        {
                            m_listIncoming.Add(new StatusDataMarket(TransferReason.Fish, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                 case BuildingTypeHelper.BuildingType.ServicePoint:
                    {
                        Dictionary<TransferReason, int> serviceValues = StatusHelper.GetServicePointValues(buildingId);
                        foreach (KeyValuePair<TransferReason, int> kvp in serviceValues)
                        {
                            if (!m_setAddedReasons.Contains(kvp.Key))
                            {
                                m_listIncoming.Add(new StatusDataServicePoint(kvp.Key, eBuildingType, buildingId, 0, 0));
                            }
                        }
                       
                        break;
                    }
                case BuildingType.ElementartySchool:
                    {
                        m_listIncoming.Add(new StatusDataSchool(TransferReason.Student1, eBuildingType, buildingId, 0, 0));
                        break;
                    }
                case BuildingType.HighSchool:
                    {
                        m_listIncoming.Add(new StatusDataSchool(TransferReason.Student2, eBuildingType, buildingId, 0, 0));
                        break;
                    }
                case BuildingType.University:
                    {
                        m_listIncoming.Add(new StatusDataSchool(TransferReason.Student3, eBuildingType, buildingId, 0, 0));
                        break;
                    }
            }
        }

        public static Dictionary<TransferReason, int> GetServicePointValues(ushort buildingId)
        {
            Dictionary<TransferReason, int> serviceValues = new Dictionary<TransferReason, int>();

            BuildingType buildingType = GetBuildingType(buildingId);
            if (buildingType == BuildingType.ServicePoint)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    // Print out parks request and suggestion arrays
                    byte parkId = DistrictManager.instance.GetPark(building.m_position);
                    if (parkId != 0)
                    {
                        DistrictPark park = DistrictManager.instance.m_parks.m_buffer[parkId];
                        if (park.m_flags != 0 && park.IsPedestrianZone)
                        {
                            for (int i = 0; i < DistrictPark.kPedestrianZoneTransferReasons.Length; ++i)
                            {
                                DistrictPark.PedestrianZoneTransferReason reason = DistrictPark.kPedestrianZoneTransferReasons[i];
                                int iMaterialCount = park.m_materialRequest[i].Count + park.m_materialSuggestion[i].Count;
                                if (iMaterialCount > 0)
                                {
                                    serviceValues[reason.m_material] = iMaterialCount;
                                }
                            }
                        }
                    }
                }
            }

            return serviceValues;
        }
    }
}