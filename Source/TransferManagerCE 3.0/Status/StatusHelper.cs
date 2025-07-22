using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Data;
using ICities;
using TransferManagerCE.Util;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.CustomTransferReason;

namespace TransferManagerCE
{
    public class StatusHelper
    {
        private HashSet<CustomTransferReason.Reason> m_buildingReasons = new HashSet<CustomTransferReason.Reason>();
        private List<StatusData> m_listGeneral = new List<StatusData>();
        private List<StatusData> m_listServices = new List<StatusData>();
        private List<StatusData> m_listIncoming = new List<StatusData>();
        private List<StatusData> m_listOutgoing = new List<StatusData>();
        private HashSet<ushort> m_setAddedVehicles = new HashSet<ushort>();
        private BuildingType m_eBuildingType = BuildingType.None;

        // ----------------------------------------------------------------------------------------
        public StatusHelper()
        {
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
            m_setAddedVehicles.Clear();
            m_buildingReasons.Clear();
            m_eBuildingType = BuildingType.None;

            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    m_eBuildingType = GetBuildingType(building);

                    // Common to all (Services & Workers etc...)
                    if (m_eBuildingType != BuildingType.OutsideConnection)
                    {
                        AddGeneral(m_eBuildingType, buildingId, building);
                        AddCommonServices(m_eBuildingType, buildingId, building);
                    }

                    // Add building specific values
                    AddBuildingSpecific(false, m_eBuildingType, buildingId, building);

                    // Add sub building values as well
                    int iLoopCount = 0;
                    ushort subBuildingId = building.m_subBuilding;
                    while (subBuildingId != 0)
                    {
                        Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                        if (subBuilding.m_flags != 0)
                        {
                            BuildingType eSubBuildingType = GetBuildingType(subBuilding);
                            AddBuildingSpecific(true, eSubBuildingType, subBuildingId, subBuilding);
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
                    AddVehicles(m_eBuildingType, buildingId, building);

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

                SortAndMergeList("General", list, m_listGeneral, false);
                SortAndMergeList("Services", list, m_listServices);
                SortAndMergeList("Incoming", list, m_listIncoming); 
                SortAndMergeList("Outgoing", list, m_listOutgoing);
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
            else if (data.HasVehicle())
            {
                if (!m_setAddedVehicles.Contains(data.GetVehicleId()))
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

        private void SortAndMergeList(string sHeader, List<StatusData> list, List<StatusData> listToAdd, bool bSort = true)
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

                list.Add(new StatusDataHeader(sHeader));
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
                    AddToList(m_listIncoming, new StatusDataSick(CustomTransferReason.Reason.Sick, eBuildingType, buildingId));
                    break;
                case BuildingType.Eldercare:
                case BuildingType.Childcare:
                    AddToList(m_listOutgoing, new StatusDataSick(CustomTransferReason.Reason.Sick, eBuildingType, buildingId));
                    break;
                case BuildingType.ServicePoint:
                    // Don't add to service point
                    break;
                default:
                    AddToList(m_listServices, new StatusDataSick(CustomTransferReason.Reason.Sick, eBuildingType, buildingId));
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

        private void AddBuildingSpecific(bool bSubBuilding, BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Building specific
            switch (eBuildingType)
            {
                case BuildingType.CargoWarehouse:
                    {
                        if (!bSubBuilding)
                        {
                            // Add warehouse information
                            ushort warehouseBuildingId = WarehouseUtils.GetWarehouseBuildingId(buildingId);

                            CustomTransferReason.Reason reason = BuildingTypeHelper.GetWarehouseActualTransferReason(warehouseBuildingId);
                            if (reason != CustomTransferReason.Reason.None)
                            {
                                AddToList(m_listIncoming, new StatusDataWarehouse(reason, eBuildingType, warehouseBuildingId));
                            }
                        }
                        break;
                    }
                case BuildingType.Warehouse:   
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
                        TransferReason material = StatusDataWaterPlant.GetInputResource(buildingId);
                        if (material != TransferReason.None)
                        {
                            AddToList(m_listIncoming, new StatusDataWaterPlant((CustomTransferReason.Reason)material, eBuildingType, buildingId));
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
                        AddToList(m_listIncoming, new StatusDataBuildingMail(CustomTransferReason.Reason.UnsortedMail, eBuildingType, buildingId));
                        AddToList(m_listIncoming, new StatusDataBuildingMail(CustomTransferReason.Reason.SortedMail, eBuildingType, buildingId));
                        break;
                    }
                case BuildingType.PostSortingFacility:
                    {
                        // UnsortedMail from post offices, SortedMail from Outside connections
                        AddToList(m_listIncoming, new StatusDataBuildingMail(CustomTransferReason.Reason.UnsortedMail, eBuildingType, buildingId));
                        AddToList(m_listOutgoing, new StatusDataBuildingMail(CustomTransferReason.Reason.SortedMail, eBuildingType, buildingId));
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
                                            AddToList(m_listIncoming, new StatusDataVehicleMail((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                            break;
                                        case BuildingType.PostSortingFacility:
                                            switch ((TransferReason)vehicle.m_transferType)
                                            {
                                                case TransferReason.IncomingMail:
                                                case TransferReason.SortedMail:
                                                case TransferReason.OutgoingMail:
                                                    {
                                                        AddToList(m_listOutgoing, new StatusDataVehicleMail((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
                                                        break;
                                                    }
                                                case TransferReason.UnsortedMail:
                                                    {
                                                        AddToList(m_listIncoming, new StatusDataVehicleMail((CustomTransferReason.Reason)vehicle.m_transferType, eBuildingType, buildingId, vehicle.m_sourceBuilding, actualVehicleId));
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
                                        case BuildingType.CargoWarehouse:
                                        case BuildingType.CargoFerryWarehouseHarbor:
                                            {
                                                ushort warehouseBuildingId = WarehouseUtils.GetWarehouseBuildingId(buildingId);

                                                CustomTransferReason.Reason reason = BuildingTypeHelper.GetWarehouseActualTransferReason(warehouseBuildingId);
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
                return true;
            });
        }
    }
}