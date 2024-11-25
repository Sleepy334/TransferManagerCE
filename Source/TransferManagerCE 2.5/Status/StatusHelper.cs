using System;
using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Data;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using TransferManagerCE.Util;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TransferManagerCE.Settings;
using UnityEngine;
using ICities;
using UnityEngine.Networking.Types;
using Epic.OnlineServices.Presence;
using static TransferManagerCE.StatusHelper;

namespace TransferManagerCE
{
    public class StatusHelper
    {
        public enum StopType
        {
            None,
            Intercity,
            TransportLine,
            CableCar,
            Evacuation,
        };

        List<StatusData> m_listServices;
        List<StatusData> m_listIncoming;
        List<StatusData> m_listOutgoing;
        List<StatusData> m_listIntercityStops;
        List<StatusData> m_listLineStops;

        HashSet<TransferReason> m_setAddedReasons;
        HashSet<ushort> m_setAddedVehicles;

        private float m_fParentBuildingSize = 0f;
        private BuildingType m_eParentBuildingType = BuildingType.None;

        public StatusHelper()
        {
            m_listServices = new List<StatusData>();
            m_listIncoming = new List<StatusData>();
            m_listOutgoing = new List<StatusData>();
            m_listIntercityStops = new List<StatusData>();
            m_listLineStops = new List<StatusData>();
            m_setAddedReasons = new HashSet<TransferReason>();
            m_setAddedVehicles = new HashSet<ushort>();
        }

        public List<StatusData> GetStatusList(ushort buildingId, out int iVehicleCount)
        {
            List<StatusData> list = new List<StatusData>();
            

            if (buildingId != 0)
            {
                m_listServices.Clear();
                m_listIncoming.Clear();
                m_listOutgoing.Clear();
                m_listIntercityStops.Clear();
                m_listLineStops.Clear();
                m_setAddedReasons.Clear();
                m_setAddedVehicles.Clear();

                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                m_eParentBuildingType = GetBuildingType(building);

                if (building.m_flags != 0)
                {
                    // Store the parents building size
                    m_fParentBuildingSize = Mathf.Max(building.Length, building.Width);

                    // Add status entries and building specific for this building
                    AddVehicles(m_eParentBuildingType, buildingId, building);

                    // Add sub building vehicles as well
                    int iLoopCount = 0;
                    ushort subBuildingId = building.m_subBuilding;
                    while (subBuildingId != 0)
                    {
                        Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                        if (subBuilding.m_flags != 0)
                        {
                            BuildingType eSubBuildingType = GetBuildingType(subBuilding);

                            // Add status entries and building specific for this sub-building
                            AddVehicles(eSubBuildingType, subBuildingId, subBuilding);
                        }

                        // setup for next sub building
                        subBuildingId = subBuilding.m_subBuilding;

                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }

                    // Now add status values for items that didnt have vehicles responding
                    // Common to all (Services)
                    if (m_eParentBuildingType != BuildingType.OutsideConnection)
                    {
                        AddCommonServices(m_eParentBuildingType, buildingId);
                    }

                    // Add building specific values
                    AddBuildingSpecific(m_eParentBuildingType, buildingId, building);

                    // Add sub building values as well
                    iLoopCount = 0;
                    subBuildingId = building.m_subBuilding;
                    while (subBuildingId != 0)
                    {
                        Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                        if (subBuilding.m_flags != 0)
                        {
                            BuildingType eSubBuildingType = GetBuildingType(subBuilding);
                            AddBuildingSpecific(eSubBuildingType, subBuildingId, subBuilding);
                        }

                        // setup for next sub building
                        subBuildingId = subBuilding.m_subBuilding;

                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }

                if (m_listServices.Count > 0)
                {
                    m_listServices.ForEach(item => item.Calculate());
                    m_listServices.Sort();
                    list.AddRange(m_listServices);
                }

                if (m_listOutgoing.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new StatusDataSeparator());
                    }
                    m_listOutgoing.ForEach(item => item.Calculate());
                    m_listOutgoing.Sort();
                    list.AddRange(m_listOutgoing);
                }

                if (m_listIncoming.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new StatusDataSeparator());
                    }
                    m_listIncoming.ForEach(item => item.Calculate());
                    m_listIncoming.Sort();
                    list.AddRange(m_listIncoming);
                }

                if (m_listLineStops.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new StatusDataSeparator());
                    }
                    m_listLineStops.ForEach(item => item.Calculate());
                    list.AddRange(m_listLineStops);
                }

                if (m_listIntercityStops.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new StatusDataSeparator());
                    }
                    m_listIntercityStops.ForEach(item => item.Calculate());
                    list.AddRange(m_listIntercityStops);
                }
            }

            iVehicleCount = m_setAddedVehicles.Count;
            return list;
        }

        private void AddVehicles(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
            {
                if (vehicle.m_flags != 0 && vehicle.Info is not null)
                {
                    ushort actualVehicleId = vehicleId;

                    // Check if it is loaded onto some other vehicle (Train/Ship/Plane)
                    if (vehicle.m_cargoParent != 0)
                    {
                        actualVehicleId = vehicle.m_cargoParent;
                    }

                    // Check if we have already added this vehicle.
                    if (!m_setAddedVehicles.Contains(actualVehicleId))
                    {
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
                                    m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
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
                            case SnowTruckAI:
                                {
                                    m_listIncoming.Add(new StatusDataSnowDump(eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                    m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                    m_setAddedVehicles.Add(actualVehicleId);
                                    break;
                                }
                            case CargoTruckAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.Commercial:
                                            {
                                                CommercialBuildingAI buildingAI = building.Info.GetAI() as CommercialBuildingAI;
                                                m_listIncoming.Add(new StatusDataCommercial(buildingAI.m_incomingResource, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                m_setAddedReasons.Add(buildingAI.m_incomingResource);
                                                m_setAddedVehicles.Add(actualVehicleId);
                                                break;
                                            }
                                        case BuildingType.Warehouse:
                                        case BuildingType.WarehouseStation:
                                            {
                                                WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                                                if (warehouseAI is not null)
                                                {
                                                    TransferReason actualTransferReason = warehouseAI.GetActualTransferReason(buildingId, ref building);
                                                    m_listIncoming.Add(new StatusDataWarehouse(actualTransferReason, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                    m_setAddedReasons.Add(actualTransferReason);
                                                    m_setAddedVehicles.Add(actualVehicleId);
                                                }
                                                break;
                                            }
                                        case BuildingType.CargoStation:
                                            {
                                                if (!m_setAddedVehicles.Contains(actualVehicleId))
                                                {
                                                    if (m_eParentBuildingType == BuildingType.WarehouseStation)
                                                    {
                                                        // Add a generic vehicle
                                                        m_listIncoming.Add(new StatusDataWarehouse((TransferReason)vehicle.m_transferType, m_eParentBuildingType, building.m_parentBuilding, vehicle.m_sourceBuilding, actualVehicleId));
                                                        m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                                    }
                                                    else
                                                    {
                                                        // Add a generic vehicle
                                                        m_listIncoming.Add(new StatusDataGeneric((TransferReason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                        m_setAddedReasons.Add((TransferReason)vehicle.m_transferType);
                                                    }

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
            });
        }

        private void AddCommonServices(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId)
        {
            // Add citizen count for this building
            m_listServices.Add(new StatusDataCitizens(TransferReason.None, eBuildingType, buildingId, 0, 0));

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
            if (!(m_setAddedReasons.Contains(TransferReason.Crime) || m_setAddedReasons.Contains((TransferReason)CustomTransferReason.Reason.Crime2)))
            {
                switch (eBuildingType)
                {
                    case BuildingType.PoliceHelicopterDepot:
                        m_listIncoming.Add(new StatusDataCrime((TransferReason)CustomTransferReason.Reason.Crime2, eBuildingType, buildingId, 0, 0));
                        m_setAddedReasons.Add((TransferReason)CustomTransferReason.Reason.Crime2);
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
                    case BuildingType.Commercial:
                        m_listServices.Add(new StatusDataCash(eBuildingType, buildingId, 0, 0));
                        break;
                    case BuildingType.ServicePoint:
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
                case BuildingType.Warehouse:
                case BuildingType.WarehouseStation:
                    {
                        WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
                        if (warehouseAI is not null)
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
                        if (buildingAI is not null)
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
                        CommercialBuildingAI buildingAI = building.Info.GetAI() as CommercialBuildingAI;
                        if (!m_setAddedReasons.Contains(buildingAI.m_incomingResource))
                        {
                            m_listIncoming.Add(new StatusDataCommercial(buildingAI.m_incomingResource, eBuildingType, buildingId, 0, 0));
                        }
                        TransferReason material = StatusDataCommercial.GetOutgoingTransferReason(buildingId, building.Info);
                        if (!m_setAddedReasons.Contains(material))
                        {
                            m_listOutgoing.Add(new StatusDataCommercial(material, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingType.Office:
                    {
                        TransferReason material = StatusDataOffice.GetOutgoingTransferReason(building);
                        if (material != TransferReason.None)
                        {
                            m_listOutgoing.Add(new StatusDataOffice(material, eBuildingType, buildingId, 0, 0));
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

                        AddNetStops(eBuildingType, building, buildingId);

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
                case BuildingType.PostSortingFacility:
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
                case BuildingType.FishMarket:
                    {
                        if (!m_setAddedReasons.Contains(TransferReason.Fish))
                        {
                            m_listIncoming.Add(new StatusDataMarket(TransferReason.Fish, eBuildingType, buildingId, 0, 0));
                        }
                        break;
                    }
                case BuildingType.ServicePoint:
                    {
                        HashSet<TransferReason> serviceValues = ServicePointUtils.GetServicePointMaterials(buildingId);
                        foreach (TransferReason reason in serviceValues)
                        {
                            if (!m_setAddedReasons.Contains(reason))
                            {
                                m_listIncoming.Add(new StatusDataServicePoint(reason, eBuildingType, buildingId, 0, 0));
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
                case BuildingType.CableCarStation:
                    {
                        AddNetStops(eBuildingType, building, buildingId);
                        break;
                    }
                case BuildingType.TransportStation:
                    {
                        // Add stops
                        AddLineStops(eBuildingType, building, buildingId);

                        // Add intercity stops
                        AddNetStops(eBuildingType, building, buildingId);

                        break;
                    }
                case BuildingType.SnowDump:
                    {
                        m_listIncoming.Add(new StatusDataSnowDump(eBuildingType, buildingId, 0, 0));
                        break;
                    }
            }
        }

        private void AddLineStops(BuildingType eBuildingType, Building building, ushort buildingId)
        {
            NetNode[] Nodes = NetManager.instance.m_nodes.m_buffer;
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            // We use the parents building size always, squared for distance measure
            float fMaxDistanceSquared = Mathf.Max(64f, m_fParentBuildingSize * m_fParentBuildingSize); 

            // Add line stops
            uint iSize = TransportManager.instance.m_lines.m_size;
            for (int i = 0; i < iSize; i++)
            {
                TransportLine line = TransportManager.instance.m_lines.m_buffer[i];
                if (line.m_flags != 0 && line.Complete)
                {
                    // Enumerate stops
                    int iLoopCount = 0;
                    ushort firstStop = line.m_stops;
                    ushort stop = firstStop;
                    while (stop != 0)
                    {
                        NetNode node = Nodes[stop];
                        if (node.m_flags != 0)
                        {
                            // Scale allowed distance by size of building, we use FindTransportBuilding so that if there is a nearby transport station then we
                            // are less likely to think they are our stops.
                            ushort transportBuildingId = BuildingManager.instance.FindTransportBuilding(node.m_position, fMaxDistanceSquared, line.Info.m_transportType);
                            if (transportBuildingId == buildingId)
                            {
                                int iAdded = 0;
                                ushort vehicleId = line.m_vehicles;
                                int iVehicleLoopCount = 0;
                                while (vehicleId != 0)
                                {
                                    Vehicle vehicle = Vehicles[vehicleId];
                                    if (vehicle.m_flags != 0 && vehicle.m_targetBuilding == stop)
                                    {
                                        m_listLineStops.Add(new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop, vehicleId));
                                        iAdded++;
                                    }

                                    vehicleId = vehicle.m_nextLineVehicle;

                                    if (++iVehicleLoopCount >= 32768)
                                    {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }

                                // If there arent any vehicles for this stop then add a "None" one instead.
                                if (iAdded == 0)
                                {
                                    m_listLineStops.Add(new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop, 0));
                                }
                            }
                        }

                        stop = TransportLine.GetNextStop(stop);
                        if (stop == firstStop)
                        {
                            break;
                        }

                        if (++iLoopCount >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        private void AddNetStops(BuildingType eBuildingType, Building building, ushort buildingId)
        {
            NetNode[] Nodes = NetManager.instance.m_nodes.m_buffer;
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            // Find any vehicles heading to the stops
            Dictionary<ushort, ushort> vehicleNodes = new Dictionary<ushort, ushort>();

            uint uiSize = VehicleManager.instance.m_vehicles.m_size;
            ushort vehicleID = building.m_ownVehicles;
            int iLoopCount1 = 0;
            while (vehicleID != 0 && vehicleID < uiSize)
            {
                Vehicle vehicle = Vehicles[vehicleID];
                if (vehicle.m_flags != 0)
                {
                    InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleID, vehicle);
                    if (target.NetNode != 0)
                    {
                        vehicleNodes[vehicleID] = target.NetNode;
                    }
                }
                
                vehicleID = vehicle.m_nextOwnVehicle;

                if (++iLoopCount1 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }

            // Add net/intercity stops
            int iLoopCount2 = 0;
            ushort nodeId = building.m_netNode;
            while (nodeId != 0)
            {
                NetNode node = Nodes[nodeId];
                NetInfo info = node.Info;
                if ((object)info != null)
                {
                    StopType eStopType = GetStopType(eBuildingType, info.m_class.m_layer, node.m_transportLine);
                    if (eStopType != StopType.None)
                    {
                        // Add stops with vehicles first
                        int iAdded = 0;
                        foreach (KeyValuePair<ushort, ushort> kvp in vehicleNodes)
                        {
                            if (kvp.Value == nodeId)
                            {
                                StatusData? data = CreateStatusData(eStopType, eBuildingType, buildingId, node.m_transportLine, nodeId, kvp.Key);
                                if (data != null)
                                {
                                    if (eStopType == StopType.CableCar)
                                    {
                                        m_listLineStops.Add(data);
                                    }
                                    else
                                    {
                                        m_listIntercityStops.Add(data);
                                    }
                                    
                                    iAdded++;
                                }
                            }
                        }

                        // If there arent any vehicles for this stop then add a "None" one instead.
                        if (iAdded == 0)
                        {
                            StatusData? data = CreateStatusData(eStopType, eBuildingType, buildingId, node.m_transportLine, nodeId, 0);
                            if (data != null)
                            {
                                if (eStopType == StopType.CableCar)
                                {
                                    m_listLineStops.Add(data);
                                }
                                else
                                {
                                    m_listIntercityStops.Add(data);
                                }
                                iAdded++;
                            }
                        }
                    }
                }

                nodeId = node.m_nextBuildingNode;

                if (++iLoopCount2 > 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        private StopType GetStopType(BuildingType eBuildingType, ItemClass.Layer layer, ushort transportLineId)
        {
            if (layer == ItemClass.Layer.PublicTransport)
            {
                switch (eBuildingType)
                {
                    case BuildingType.TransportStation:
                        {
                            if (transportLineId == 0)
                            {
                                return StopType.Intercity;
                            }
                            else
                            {
                                return StopType.TransportLine;
                            }
                        }
                    case BuildingType.CableCarStation:
                        {
                            return StopType.CableCar;
                        }
                    case BuildingType.DisasterShelter:
                        {
                            return StopType.Evacuation;
                        }
                }
            }

            return StopType.None;
        }

        private StatusData? CreateStatusData(StopType stopType, BuildingType eBuildingType, ushort buildingId, ushort LineId, ushort nodeId, ushort vehicleId)
        {
            switch (stopType)
            {
                case StopType.Intercity: return new StatusIntercityStop(eBuildingType, buildingId, nodeId, vehicleId);
                case StopType.TransportLine: return new StatusTransportLineStop(eBuildingType, buildingId, LineId, nodeId, vehicleId);
                case StopType.CableCar: return new StatusCableCarStop(eBuildingType, buildingId, nodeId, vehicleId);
                case StopType.Evacuation: return new StatusEvacuationStop(eBuildingType, buildingId, nodeId, vehicleId);
            }

            return null;
        }
    }
}