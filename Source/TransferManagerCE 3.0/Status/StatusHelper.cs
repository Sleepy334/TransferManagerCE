using System;
using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Data;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using TransferManagerCE.Util;
using UnityEngine;
using ICities;
using static TransferManagerCE.CustomTransferReason;
using SleepyCommon;

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

        public HashSet<CustomTransferReason.Reason> m_buildingReasons;
        private List<StatusData> m_listGeneral;
        private List<StatusData> m_listServices;
        private List<StatusData> m_listIncoming;
        private List<StatusData> m_listOutgoing;
        private List<StatusData> m_listIntercityStops;
        private List<StatusData> m_listLineStops;
        private HashSet<ushort> m_setAddedVehicles;

        private float m_fParentBuildingSize = 0f;
        private BuildingType m_eParentBuildingType = BuildingType.None;

        public StatusHelper()
        {
            m_listGeneral = new List<StatusData>();
            m_listServices = new List<StatusData>();
            m_listIncoming = new List<StatusData>();
            m_listOutgoing = new List<StatusData>();
            m_listIntercityStops = new List<StatusData>();
            m_listLineStops = new List<StatusData>();
            m_setAddedVehicles = new HashSet<ushort>();
            m_buildingReasons = new HashSet<CustomTransferReason.Reason>();
        } 

        public bool HasBuildingReason(CustomTransferReason.Reason reason)
        {
            return m_buildingReasons.Contains(reason);
        }

        public List<StatusData> GetStatusList(ushort buildingId, out int iVehicleCount)
        {
            List<StatusData> list = new List<StatusData>();

            m_listGeneral.Clear();
            m_listServices.Clear();
            m_listIncoming.Clear();
            m_listOutgoing.Clear();
            m_listIntercityStops.Clear();
            m_listLineStops.Clear();
            m_setAddedVehicles.Clear();
            m_buildingReasons.Clear();
            m_eParentBuildingType = BuildingType.None;
            m_fParentBuildingSize = 0.0f;

            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    m_eParentBuildingType = GetBuildingType(building);

                    // Store the parents building size
                    m_fParentBuildingSize = Mathf.Max(building.Length, building.Width);

                    // Common to all (Services & Workers etc...)
                    if (m_eParentBuildingType != BuildingType.OutsideConnection)
                    {
                        AddGeneral(m_eParentBuildingType, buildingId, building);
                        AddCommonServices(m_eParentBuildingType, buildingId, building);
                    }

                    // Add building specific values
                    AddBuildingSpecific(m_eParentBuildingType, buildingId, building);

                    // Add sub building values as well
                    int iLoopCount = 0;
                    ushort subBuildingId = building.m_subBuilding;
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

                    // Add vehicles for main building
                    AddVehicles(m_eParentBuildingType, buildingId, building);

                    // Add sub building vehicles as well
                    iLoopCount = 0;
                    subBuildingId = building.m_subBuilding;
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
                }

                SortAndMergeList(list, m_listGeneral, false);
                SortAndMergeList(list, m_listServices);
                SortAndMergeList(list, m_listOutgoing);
                SortAndMergeList(list, m_listIncoming);
                SortAndMergeList(list, m_listLineStops);
                SortAndMergeList(list, m_listIntercityStops);
            }

            iVehicleCount = m_setAddedVehicles.Count;
            return list;
        }

        private void AddToList(List<StatusData> list, StatusData data)
        {
            if (data.IsBuildingData())
            {
                if (data.GetMaterial() != Reason.None)
                {
                    m_buildingReasons.Add(data.GetMaterial());
                }

                list.Add(data);
            }
            else if (data.IsVehicleData())
            {
                if (data.HasVehicle() && !m_setAddedVehicles.Contains(data.GetVehicleId()))
                {
                    // Only add vehicle if not already in list
                    m_setAddedVehicles.Add(data.GetVehicleId());
                    list.Add(data);
                }
            }
            else
            {
                list.Add(data);
            }
        }

        private void SortAndMergeList(List<StatusData> list, List<StatusData> listToAdd, bool bSort = true)
        {
            if (listToAdd.Count > 0)
            {
                if (list.Count > 0)
                {
                    list.Add(new StatusDataSeparator());
                }
                if (bSort)
                {
                    listToAdd.Sort();
                }
                
                list.AddRange(listToAdd);
            }
        }

        private void AddGeneral(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Add citizen count for this building
            if (eBuildingType != BuildingType.ServicePoint)
            {
                AddToList(m_listGeneral, new StatusDataCitizens(CustomTransferReason.Reason.None, eBuildingType, buildingId));
            }
            
            int iTotalWorker = BuildingUtils.GetTotalWorkerPlaces(buildingId, building, out int worker0, out int worker1, out int worker2, out int worker3);
            if (iTotalWorker > 0)
            {
                AddToList(m_listGeneral, new StatusDataWorkers(CustomTransferReason.Reason.None, eBuildingType, buildingId));
                AddToList(m_listGeneral, new StatusDataWorkers(CustomTransferReason.Reason.Worker0, eBuildingType, buildingId));
                AddToList(m_listGeneral, new StatusDataWorkers(CustomTransferReason.Reason.Worker1, eBuildingType, buildingId));
                AddToList(m_listGeneral, new StatusDataWorkers(CustomTransferReason.Reason.Worker2, eBuildingType, buildingId));
                AddToList(m_listGeneral, new StatusDataWorkers(CustomTransferReason.Reason.Worker3, eBuildingType, buildingId));
            }
        }

        private void AddCommonServices(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Dead
            switch (eBuildingType)
            {
                case BuildingType.Cemetery:
                    AddToList(m_listIncoming, new StatusDataDead(eBuildingType, buildingId));
                    break;
                case BuildingType.ServicePoint:
                    // Don't add to service point
                    break;
                default:
                    AddToList(m_listServices, new StatusDataDead(eBuildingType, buildingId));
                    break;
            }

            // Healthcare
            switch (eBuildingType)
            {
                case BuildingType.Hospital:
                case BuildingType.UniversityHospital:
                case BuildingType.Eldercare:
                case BuildingType.Childcare:
                    AddToList(m_listIncoming, new StatusDataBuildingSick(CustomTransferReason.Reason.Sick, eBuildingType, buildingId));
                    break;
                case BuildingType.ServicePoint:
                    // Don't add to service point
                    break;
                default:
                    AddToList(m_listServices, new StatusDataBuildingSick(CustomTransferReason.Reason.Sick, eBuildingType, buildingId));
                    break;
            }

            // Garbage
            switch (eBuildingType)
            {
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                    AddToList(m_listIncoming, new StatusDataBuildingGarbage(CustomTransferReason.Reason.Garbage, eBuildingType, buildingId));
                    AddToList(m_listOutgoing, new StatusDataBuildingGarbage(CustomTransferReason.Reason.Goods, eBuildingType, buildingId));
                    break;
                case BuildingType.WasteTransfer:
                case BuildingType.Landfill:
                case BuildingType.IncinerationPlant:
                    AddToList(m_listIncoming, new StatusDataBuildingGarbage(CustomTransferReason.Reason.Garbage, eBuildingType, buildingId));
                    break;
                default:
                    AddToList(m_listServices, new StatusDataBuildingGarbage(CustomTransferReason.Reason.Garbage, eBuildingType, buildingId));
                    break;
            }

            // Fire
            switch (eBuildingType)
            {
                case BuildingType.FireStation:
                case BuildingType.FireHelicopterDepot:
                case BuildingType.FirewatchTower:
                    AddToList(m_listIncoming, new StatusDataFire(eBuildingType, buildingId));
                    break;
                default:
                    AddToList(m_listServices, new StatusDataFire(eBuildingType, buildingId));
                    break;
            }

            // Crime
            switch (eBuildingType)
            {
                case BuildingType.PoliceHelicopterDepot:
                    AddToList(m_listIncoming, new StatusDataCrime((CustomTransferReason.Reason)CustomTransferReason.Reason.Crime2, eBuildingType, buildingId));
                    break;
                case BuildingType.PoliceStation:
                case BuildingType.Prison:
                    AddToList(m_listIncoming, new StatusDataCrime(CustomTransferReason.Reason.Crime, eBuildingType, buildingId));
                    break;
                case BuildingType.ServicePoint:
                    // Don't add to service point
                    break;
                default:
                    AddToList(m_listServices, new StatusDataCrime(CustomTransferReason.Reason.Crime, eBuildingType, buildingId));
                    break;
            }

            // Cash
            switch (eBuildingType)
            {
                case BuildingType.Bank:
                    AddToList(m_listIncoming, new StatusDataCash(eBuildingType, buildingId));
                    break;
                case BuildingType.Commercial:
                case BuildingType.ServicePoint:
                    AddToList(m_listServices, new StatusDataCash(eBuildingType, buildingId));
                    break;
            }

            // Mail
            switch (eBuildingType)
            {
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                    // Don't add mail
                    break;
                default:
                    AddToList(m_listServices, new StatusDataBuildingMail(CustomTransferReason.Reason.Mail, eBuildingType, buildingId));
                    break;
            }
        }

        private void AddBuildingSpecific(BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Building specific
            switch (eBuildingType)
            {
                case BuildingType.Warehouse:
                case BuildingType.WarehouseStation:
                case BuildingType.CargoFerryWarehouseHarbor:
                    {
                        CustomTransferReason.Reason reason = BuildingTypeHelper.GetWarehouseActualTransferReason(buildingId);
                        if (reason != CustomTransferReason.Reason.None)
                        {
                            AddToList(m_listIncoming, new StatusDataWarehouse(reason, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ProcessingFacility:
                case BuildingTypeHelper.BuildingType.UniqueFactory:
                    {
                        ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                        if (buildingAI is not null)
                        {
                            if (buildingAI.m_inputResource1 != TransferReason.None)
                            {
                                AddToList(m_listIncoming, new StatusDataProcessingFacility((CustomTransferReason.Reason) buildingAI.m_inputResource1, eBuildingType, buildingId));
                            }
                            if (buildingAI.m_inputResource2 != TransferReason.None)
                            {
                                AddToList(m_listIncoming, new StatusDataProcessingFacility((CustomTransferReason.Reason)buildingAI.m_inputResource2, eBuildingType, buildingId));
                            }
                            if (buildingAI.m_inputResource3 != TransferReason.None)
                            {
                                AddToList(m_listIncoming, new StatusDataProcessingFacility((CustomTransferReason.Reason)buildingAI.m_inputResource3, eBuildingType, buildingId));
                            }
                            if (buildingAI.m_inputResource4 != TransferReason.None)
                            {
                                AddToList(m_listIncoming, new StatusDataProcessingFacility((CustomTransferReason.Reason)buildingAI.m_inputResource4, eBuildingType, buildingId));
                            }
                            AddToList(m_listOutgoing, new StatusDataProcessingFacility((CustomTransferReason.Reason)buildingAI.m_outputResource, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingType.FishFactory:
                    {
                        AddToList(m_listIncoming, new StatusDataFishFactory(CustomTransferReason.Reason.Fish, eBuildingType, buildingId));
                        AddToList(m_listOutgoing, new StatusDataFishFactory(CustomTransferReason.Reason.Goods, eBuildingType, buildingId));
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Commercial:
                    {
                        CommercialBuildingAI buildingAI = building.Info.GetAI() as CommercialBuildingAI;
                        AddToList(m_listIncoming, new StatusDataBuildingCommercial((CustomTransferReason.Reason)buildingAI.m_incomingResource, eBuildingType, buildingId));

                        TransferReason material = StatusDataBuildingCommercial.GetOutgoingTransferReason(buildingAI, buildingId);
                        AddToList(m_listOutgoing, new StatusDataBuildingCommercial((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.Office:
                    {
                        TransferReason material = StatusDataOffice.GetOutgoingTransferReason(building);
                        if (material != TransferReason.None)
                        {
                            AddToList(m_listOutgoing, new StatusDataOffice((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        }
                        if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Hotels))
                        {
                            AddToList(m_listOutgoing, new StatusDataOffice(CustomTransferReason.Reason.BusinessA, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingType.Hotel:
                    {
                        AddToList(m_listOutgoing, new StatusDataHotel(eBuildingType, buildingId));
                        AddToList(m_listOutgoing, new StatusDataHotelAttractiveness(eBuildingType, buildingId));
                        break;
                    }
                case BuildingTypeHelper.BuildingType.DisasterShelter:
                    {
                        AddToList(m_listIncoming, new StatusDataShelter(CustomTransferReason.Reason.Food, eBuildingType, buildingId));
                        AddNetStops(eBuildingType, building, buildingId);
                        break;
                    }
                case BuildingTypeHelper.BuildingType.GenericExtractor:
                    {
                        TransferReason material = StatusDataGenericExtractor.GetOutgoingTransferReason(building);
                        if (material != TransferReason.None)
                        {
                            AddToList(m_listOutgoing, new StatusDataGenericExtractor((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.GenericProcessing:
                case BuildingTypeHelper.BuildingType.GenericFactory:
                    {
                        TransferReason material = StatusDataBuildingGenericProcessing.GetIncomingTransferReason(buildingId);
                        if (material != TransferReason.None)
                        {
                            AddToList(m_listIncoming, new StatusDataBuildingGenericProcessing((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        }
                        TransferReason material2 = StatusDataBuildingGenericProcessing.GetSecondaryIncomingTransferReason(buildingId);
                        if (material2 != TransferReason.None)
                        {
                            AddToList(m_listIncoming, new StatusDataBuildingGenericProcessing((CustomTransferReason.Reason)material2, eBuildingType, buildingId));
                        }
                        TransferReason outMaterial = StatusDataBuildingGenericProcessing.GetOutgoingTransferReason(building);
                        if (outMaterial != TransferReason.None)
                        {
                            AddToList(m_listOutgoing, new StatusDataBuildingGenericProcessing((CustomTransferReason.Reason)outMaterial, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Park:
                    { 
                        if (building.Info.GetAI() is ParkBuildingAI)
                        {
                            byte park = Singleton<DistrictManager>.instance.GetPark(building.m_position);
                            if (park != 0)
                            {
                                AddToList(m_listIncoming, new StatusDataPark(CustomTransferReason.Reason.ParkMaintenance, eBuildingType, buildingId));
                            }
                        }
                        else
                        {
                            AddToList(m_listIncoming, new StatusDataPark(CustomTransferReason.Reason.ParkMaintenance, eBuildingType, buildingId));
                        }

                        if (BuildingUtils.GetTotalVisitPlaceCount(buildingId, building) > 0)
                        {
                            AddToList(m_listOutgoing, new StatusDataVisitors(eBuildingType, buildingId)); // Visitors
                        }
                        
                        break;
                    }
                case BuildingTypeHelper.BuildingType.Monument:
                    {
                        if (BuildingUtils.GetTotalVisitPlaceCount(buildingId, building) > 0)
                        {
                            AddToList(m_listOutgoing, new StatusDataVisitors(eBuildingType, buildingId)); // Visitors
                        }
                        break;
                    }
                case BuildingType.FishFarm:
                case BuildingType.FishHarbor:
                    {
                        AddToList(m_listOutgoing, new StatusDataFishHarbor(eBuildingType, buildingId));
                        break;
                    }
                case BuildingTypeHelper.BuildingType.ExtractionFacility:
                    {
                        TransferReason material = StatusDataExtractionFacility.GetOutputResource(buildingId);
                        AddToList(m_listOutgoing, new StatusDataExtractionFacility((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        break;
                    }
                case BuildingTypeHelper.BuildingType.BoilerStation:
                    {
                        TransferReason material = StatusWaterPlant.GetInputResource(buildingId);
                        if (material != TransferReason.None)
                        {
                            AddToList(m_listIncoming, new StatusWaterPlant((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PetrolPowerPlant:
                case BuildingTypeHelper.BuildingType.CoalPowerPlant:
                    {
                        TransferReason material = StatusDataPowerPlant.GetInputResource(buildingId);
                        if (material != TransferReason.None)
                        {
                            AddToList(m_listIncoming, new StatusDataPowerPlant((CustomTransferReason.Reason)material, eBuildingType, buildingId));
                        }
                        break;
                    }
                case BuildingTypeHelper.BuildingType.PostOffice:
                    {
                        AddToList(m_listOutgoing, new StatusDataBuildingMail(CustomTransferReason.Reason.UnsortedMail, eBuildingType, buildingId));
                        AddToList(m_listIncoming, new StatusDataBuildingMail(CustomTransferReason.Reason.SortedMail, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.PostSortingFacility:
                    {
                        // UnsortedMail from post offices, SortedMail from Outside connections
                        AddToList(m_listOutgoing, new StatusDataBuildingMail(CustomTransferReason.Reason.UnsortedMail, eBuildingType, buildingId));
                        AddToList(m_listIncoming, new StatusDataBuildingMail(CustomTransferReason.Reason.SortedMail, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.FishMarket:
                    {
                        AddToList(m_listIncoming, new StatusDataMarket(CustomTransferReason.Reason.Fish, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.ServicePoint:
                    {
                        HashSet<TransferReason> serviceValues = ServicePointUtils.GetServicePointMaterials(buildingId);
                        foreach (TransferReason reason in serviceValues)
                        {
                            if (!m_buildingReasons.Contains((CustomTransferReason.Reason) reason))
                            {
                                AddToList(m_listIncoming, new StatusDataServicePoint((CustomTransferReason.Reason) reason, eBuildingType, buildingId));
                            }
                        }

                        break;
                    }
                case BuildingType.ElementartySchool:
                    {
                        AddToList(m_listIncoming, new StatusDataSchool(CustomTransferReason.Reason.StudentES, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.HighSchool:
                    {
                        AddToList(m_listIncoming, new StatusDataSchool(CustomTransferReason.Reason.StudentHS, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.University:
                case BuildingType.UniversityHospital:
                    {
                        AddToList(m_listIncoming, new StatusDataSchool(CustomTransferReason.Reason.StudentUni, eBuildingType, buildingId));
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
                        AddToList(m_listIncoming, new StatusDataSnowDump(eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.Library:
                    {
                        AddToList(m_listIncoming, new StatusDataVisitors(eBuildingType, buildingId));
                        break;
                    }
            }
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
                                            AddToList(m_listIncoming, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
                                    break;
                                }
                            case AmbulanceCopterAI:
                            case AmbulanceAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.Hospital:
                                        case BuildingType.MedicalHelicopterDepot:
                                        case BuildingType.UniversityHospital:
                                            AddToList(m_listIncoming, new StatusDataVehicleSick((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicleSick((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
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
                                            AddToList(m_listIncoming, new StatusDataVehicleGarbage((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicleGarbage((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
                                    break;
                                }
                            case FireTruckAI:
                            case FireCopterAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.FireStation:
                                        case BuildingType.FireHelicopterDepot:
                                        case BuildingType.FirewatchTower:
                                            AddToList(m_listIncoming, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
                                    break;
                                }
                            case PoliceCarAI:
                            case PoliceCopterAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.PoliceHelicopterDepot:
                                        case BuildingType.PoliceStation:
                                            AddToList(m_listIncoming, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
                                    break;
                                }
                            case BankVanAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.Bank:
                                            AddToList(m_listIncoming, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
                                    break;
                                }
                            case PostVanAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.PostOffice:
                                        case BuildingType.PostSortingFacility:
                                            switch ((TransferReason)vehicle.m_transferType)
                                            {
                                                case TransferReason.IncomingMail:
                                                case TransferReason.SortedMail:
                                                case TransferReason.OutgoingMail:
                                                    {
                                                        AddToList(m_listIncoming, new StatusDataVehicleMail((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                        break;
                                                    }
                                                case TransferReason.UnsortedMail:
                                                    {
                                                        AddToList(m_listOutgoing, new StatusDataVehicleMail((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                        break;
                                                    }
                                            }
                                            break;
                                        default:
                                            AddToList(m_listServices, new StatusDataVehicleMail((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                    }
                                    break;
                                }
                            case ParkMaintenanceVehicleAI:
                            case SnowTruckAI:
                                {
                                    AddToList(m_listIncoming, new StatusDataVehicle((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                    break;
                                }
                            case CargoTruckAI:
                                {
                                    switch (eBuildingType)
                                    {
                                        case BuildingType.Commercial:
                                            {
                                                CommercialBuildingAI buildingAI = building.Info.GetAI() as CommercialBuildingAI;
                                                AddToList(m_listIncoming, new StatusDataVehicleCommercial((CustomTransferReason.Reason) buildingAI.m_incomingResource, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                break;
                                            }
                                        case BuildingType.Warehouse:
                                        case BuildingType.WarehouseStation:
                                        case BuildingType.CargoFerryWarehouseHarbor:
                                            {
                                                CustomTransferReason.Reason reason = BuildingTypeHelper.GetWarehouseActualTransferReason(buildingId);
                                                if (reason != CustomTransferReason.Reason.None)
                                                {
                                                    AddToList(m_listIncoming, new StatusDataVehicle(VehicleUtils.GetTransferType(vehicle), eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                }
                                                break;
                                            }
                                        case BuildingType.GenericProcessing:
                                        case BuildingType.GenericFactory:
                                            {
                                                AddToList(m_listIncoming, new StatusDataVehicleGenericProcessing(VehicleUtils.GetTransferType(vehicle), eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                break;
                                            }
                                        default:
                                            {
                                                // Add a generic vehicle
                                                AddToList(m_listIncoming, new StatusDataVehicle(VehicleUtils.GetTransferType(vehicle), eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                break;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    // Add a generic vehicle
                                    AddToList(m_listIncoming, new StatusDataVehicle(VehicleUtils.GetTransferType(vehicle), eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                    break;
                                }
                        }
                    }
                }
            });
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
                                        AddToList(m_listLineStops, new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop, vehicleId));
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
                                    AddToList(m_listLineStops, new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop, 0));
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
                                        AddToList(m_listLineStops, data);
                                    }
                                    else
                                    {
                                        AddToList(m_listIntercityStops, data);
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
                                    AddToList(m_listLineStops, data);
                                }
                                else
                                {
                                    AddToList(m_listIntercityStops, data);
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