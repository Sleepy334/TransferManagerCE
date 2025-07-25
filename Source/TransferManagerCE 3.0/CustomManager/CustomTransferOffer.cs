﻿using ColossalFramework;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager.Stats;
using TransferManagerCE.Settings;
using UnityEngine;
using static SleepyCommon.TransportUtils;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.WarehouseUtils;

namespace TransferManagerCE.CustomManager
{
    public class CustomTransferOffer
    {
        public enum WarehouseStationOffer
        {
            None,
            WarehouseRoad,
            WarehouseTrain,
        }

        // Actual TransferOffer object
        public TransferOffer m_offer;
        private bool m_bIncoming; // Offer direction

        // Store these so we don't have to calculate them every time.
        private ushort? m_offerBuildingId = null; // Extracted from building/vehicle/citizen
        private bool? m_bIsValid = null;
        private BuildingType m_buildingType = BuildingType.None;
        public ushort m_nearestNode = ushort.MaxValue; // public so we can debug it without forcing it to be calculated.
        private Vector3? m_position = null;

        // Outside connection
        private bool? m_bOutside = null;
        private uint? m_effectiveOutsideCargoPriorityFactor = null;
        private uint? m_effectiveOutsideCitizenPriorityFactor = null;
        private bool? m_bExportAllowed = null;
        private bool? m_bImportAllowed = null;
        private HashSet<ushort>? m_excludedOutsideConnections = null;

        // Warehouse settings
        private bool? m_bIsWarehouse = null;
        private WarehouseMode m_warehouseMode = WarehouseMode.None;
        private float? m_fWarehouseStorage = null;
        private bool? m_IsExportVehicleLimitOk = null;

        // Building and Districts restrictions
        private TransferOfferDistrictRestrictions m_districtRestrictions = new TransferOfferDistrictRestrictions();
        private TransferOfferBuildingRestrictions m_buildingRestrictions = new TransferOfferBuildingRestrictions();

        // Distance settings
        private float? m_fDistanceRestrictionSquared = null;

        // Priority scaling
        private float? m_priorityFactor = null;

        // Cargo warehouse settings
        private TransportType m_transportType = TransportType.None;
        private WarehouseStationOffer m_warehouseStationOfferType = WarehouseStationOffer.None;
        
        // Used for outside connection scaling
        private static Util.OutsideConnectionCurve s_outsideConnectionCurve = new Util.OutsideConnectionCurve();

        // -------------------------------------------------------------------------------------------
        public CustomTransferOffer(bool bIncoming, TransferOffer offer)
        {
            m_bIncoming = bIncoming;
            m_offer = offer;
        }

        // -------------------------------------------------------------------------------------------
        public void SetOffer(bool bIncoming, TransferOffer offer)
        {
            m_bIncoming = bIncoming;
            m_offer = offer;
#if DEBUG
            if (m_offerBuildingId is not null)
            {
                CDebug.LogError("Cached values not reset");
            }
#endif
        }

        // -------------------------------------------------------------------------------------------
        public void ResetCachedValues()
        {
            // reset cache information
            m_offerBuildingId = null; // Extracted from building/vehicle/citizen
            m_bIsValid = null;
            m_buildingType = BuildingType.None;
            m_nearestNode = ushort.MaxValue; // public so we can debug it without forcing it to be calculated.
            m_position = null;

            // Outside connection
            m_bOutside = null;
            m_effectiveOutsideCargoPriorityFactor = null;
            m_effectiveOutsideCitizenPriorityFactor = null;
            m_bExportAllowed = null;
            m_bImportAllowed = null;
            m_excludedOutsideConnections = null;

            // Warehouse settings
            m_bIsWarehouse = null;
            m_warehouseMode = WarehouseMode.None;
            m_fWarehouseStorage = null;
            m_IsExportVehicleLimitOk = null;

            // Building and District settings
            m_districtRestrictions.ResetCachedValues();
            m_buildingRestrictions.ResetCachedValues();

            // Distance settings
            m_fDistanceRestrictionSquared = null;

            // Priority scaling
            m_priorityFactor = null;

            // Cargo warehouse
            m_transportType = TransportType.None;
            m_warehouseStationOfferType = WarehouseStationOffer.None;
        }

        // -------------------------------------------------------------------------------------------
        public static explicit operator TransferOffer(CustomTransferOffer offer)
        {
            return offer.m_offer;
        }

        // -------------------------------------------------------------------------------------------
        public InstanceID m_object
        {
            get { return m_offer.m_object; }
            set { m_offer.m_object = value; }
        }

        // -------------------------------------------------------------------------------------------
        public int Priority
        {
            get { return m_offer.Priority; }
            set { m_offer.Priority = value; }
        }

        // -------------------------------------------------------------------------------------------
        public bool Active
        {
            get { return m_offer.Active; }
            set { m_offer.Active = value; }
        }

        // -------------------------------------------------------------------------------------------
        public bool Exclude
        {
            get { return m_offer.Exclude; }
            set { m_offer.Exclude = value; }
        }

        // -------------------------------------------------------------------------------------------
        public int Amount
        {
            get { return m_offer.Amount; }
            set { m_offer.Amount = value; }
        }

        // -------------------------------------------------------------------------------------------
        public bool Unlimited
        {
            get { return m_offer.Unlimited; }
            set { m_offer.Unlimited = value; }
        }

        // -------------------------------------------------------------------------------------------
        public int LocalPark
        {
            get { return m_offer.m_isLocalPark; }
        }

        // -------------------------------------------------------------------------------------------
        public ushort Building
        {
            get { return m_offer.Building; }
            set { m_offer.Building = value; }
        }

        // -------------------------------------------------------------------------------------------
        public ushort Vehicle
        {
            get { return m_offer.Vehicle; }
            set { m_offer.Vehicle = value; }
        }

        // -------------------------------------------------------------------------------------------
        public uint Citizen
        {
            get { return m_offer.Citizen; }
            set { m_offer.Citizen = value; }
        }

        // -------------------------------------------------------------------------------------------
        public byte Park
        {
            get { return m_offer.Park; }
            set { m_offer.Park = value; }
        }

        // -------------------------------------------------------------------------------------------
        public ushort NetSegment
        {
            get { return m_offer.NetSegment; }
            set { m_offer.NetSegment = value; }
        }

        // -------------------------------------------------------------------------------------------
        public ushort TransportLine
        {
            get { return m_offer.TransportLine; }
            set { m_offer.TransportLine = value; }
        }

        // -------------------------------------------------------------------------------------------
        public Vector3 Position
        {
            get 
            { 
                if (m_position is null)
                {
                    // Try to extract position from object first as offer position can be buggy
                    m_position = InstanceHelper.GetPosition(m_object);

                    // If we got back zero then we failed to load position
                    if (m_position == Vector3.zero)
                    {
                        m_position = m_offer.Position;
                    }
                }

                return m_position.Value; 
            }
        }

        // -------------------------------------------------------------------------------------------
        public bool IsIncoming()
        {
            return m_bIncoming;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsOutgoing()
        {
            return !IsIncoming();
        }

        // -------------------------------------------------------------------------------------------
        public bool IsOutside()
        {
            if (m_bOutside is null)
            {
                ushort usBuildingId = GetBuilding();
                if (usBuildingId != 0)
                {
                    m_bOutside = BuildingTypeHelper.IsOutsideConnection(usBuildingId);
                }
                else
                {
                    m_bOutside = false;
                }
            }

            return m_bOutside.Value;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsWarehouse()
        {
            if (m_bIsWarehouse is null)
            {
                m_bIsWarehouse = Exclude || BuildingTypeHelper.IsWarehouse(GetBuildingType());
            }
            return m_bIsWarehouse.Value;
        }

        // -------------------------------------------------------------------------------------------
        public uint GetEffectiveOutsideCargoPriorityFactor()
        {
            if (m_effectiveOutsideCargoPriorityFactor is null)
            {
                int iPercent = BuildingSettingsFast.GetEffectiveOutsideCargoPriority(GetBuilding());
                m_effectiveOutsideCargoPriorityFactor = (uint)s_outsideConnectionCurve.GetCurveValue(iPercent);
            }

            return m_effectiveOutsideCargoPriorityFactor.Value;
        }

        // -------------------------------------------------------------------------------------------
        public uint GetEffectiveOutsideCitizenPriorityFactor()
        {
            if (m_effectiveOutsideCitizenPriorityFactor is null)
            {
                int iPercent = BuildingSettingsFast.GetEffectiveOutsideCitizenPriority(GetBuilding());
                m_effectiveOutsideCitizenPriorityFactor = (uint)s_outsideConnectionCurve.GetCurveValue(iPercent);
            }

            return m_effectiveOutsideCitizenPriorityFactor.Value;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsFactory()
        {
            switch (GetBuildingType())
            {
                // Generic Industries
                case BuildingTypeHelper.BuildingType.GenericProcessing: // This is not fixed by the IndustrialBuildingAI harmony patch

                // TODO: Need to test this against Coal power plants etc... to see if it stops other matches
                //case BuildingTypeHelper.BuildingType.GenericFactory: 

                // DLC Industries
                case BuildingTypeHelper.BuildingType.ProcessingFacility:
                case BuildingTypeHelper.BuildingType.UniqueFactory:
                    return true;
            }
            return false;
        }

        // -------------------------------------------------------------------------------------------
        public WarehouseMode GetWarehouseMode()
        {
            if (m_warehouseMode == WarehouseMode.None)
            {
                ushort warehouseId = WarehouseUtils.GetWarehouseBuildingId(GetBuilding());
                m_warehouseMode = WarehouseUtils.GetWarehouseMode(warehouseId);
            }

            return m_warehouseMode;
        }

        // -------------------------------------------------------------------------------------------
        public float GetWarehouseStoragePercent()
        {
            if (m_fWarehouseStorage is null)
            {
                m_fWarehouseStorage = -1.0f; // Not valid

                if (IsWarehouse() && GetBuilding() != 0)
                {
                    ushort warehouseBuildingId = WarehouseUtils.GetWarehouseBuildingId(GetBuilding());

                    Building building = BuildingManager.instance.m_buildings.m_buffer[warehouseBuildingId];
                    if (building.m_flags != 0)
                    {
                        WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                        if (warehouse is not null)
                        {
                            if (m_bIncoming)
                            {
                                // For incoming we use the storage buffer and incoming supply
                                TransferReason actualTransferReason = warehouse.GetActualTransferReason(warehouseBuildingId, ref building);
                                int iTransferSize = BuildingUtils.GetGuestVehiclesTransferSize(warehouseBuildingId, actualTransferReason);
                                double dStorage = building.m_customBuffer1 * 0.1 + iTransferSize * 0.001;
                                double dCapacity = warehouse.m_storageCapacity * 0.001;
                                double dInPercent = dStorage / dCapacity;
                                m_fWarehouseStorage = (float)Math.Min(dInPercent, 1.0);
                            }
                            else
                            {
                                // For outgoing we use the actual storage buffer only, not including incoming supply
                                double dStorage = building.m_customBuffer1 * 0.1;
                                double dCapacity = warehouse.m_storageCapacity * 0.001;
                                double dInPercent = dStorage / dCapacity;
                                m_fWarehouseStorage = (float)Math.Min(dInPercent, 1.0);
                            }
                        }
                    }
                }
            }

            return m_fWarehouseStorage.Value;
        }

        // -------------------------------------------------------------------------------------------
        public ushort GetBuilding()
        {
            if (m_offerBuildingId is null)
            {
                m_offerBuildingId = 0;

                switch (m_object.Type)
                {
                    case InstanceType.Building:
                        {
                            m_offerBuildingId = Building;
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[Vehicle];
                            if (vehicle.m_flags != 0)
                            {
                                m_offerBuildingId = vehicle.m_sourceBuilding;
                            }
                            break;
                        }
                    case InstanceType.Citizen:
                        {
                            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[Citizen];
                            if (citizen.m_flags != 0)
                            {
                                m_offerBuildingId = citizen.GetBuildingByLocation();
                            }
                            break;
                        }
                    case InstanceType.Park:
                        {
                            // Currently don't support restrictions for ServicePoints
                            break;
                        }
                }

                if (m_offerBuildingId != 0)
                {
                    // Don't get parent if its a cargo warehouse as we use different settings for
                    // the sub building and warehouse
                    if (!IsCargoWarehouse())
                    {
                        // Check if it is a sub building and locate topmost building
                        m_offerBuildingId = GetTopMostParentBuilding(m_offerBuildingId.Value);
                    }
                }
            }
            return m_offerBuildingId.Value;
        }

        // -------------------------------------------------------------------------------------------
        public static ushort GetTopMostParentBuilding(ushort buildingId)
        {
            ushort parentBuildingId = buildingId;

            int iLoopCount = 0;
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[parentBuildingId];
            while (building.m_parentBuilding != 0)
            {
                // Set buildingId to be parent building id.
                parentBuildingId = building.m_parentBuilding;

                // Get next parent building
                building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[parentBuildingId];

                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }

            return parentBuildingId;
        }

        // -------------------------------------------------------------------------------------------
        public Vector3 GetBuildingPosition()
        {
            // If the offer is not from a building (eg. Vehicle) then find the associated building and return it's position.
            if (Building == 0 && GetBuilding() != 0)
            {
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[GetBuilding()];
                return building.m_position;
            }
            else
            {
                return Position;
            }
        }

        // -------------------------------------------------------------------------------------------
        public BuildingTypeHelper.BuildingType GetBuildingType() 
        {
            if (m_buildingType == BuildingTypeHelper.BuildingType.None && GetBuilding() != 0)
            {
                m_buildingType = BuildingTypeHelper.GetBuildingType(GetBuilding());
            }
            return m_buildingType;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsImportAllowed(CustomTransferReason.Reason material)
        {
            if (m_bImportAllowed is null)
            {
                m_bImportAllowed = BuildingSettingsFast.IsImportAllowed(GetBuilding(), GetBuildingType(), material, IsIncoming());
            }

            return m_bImportAllowed.Value;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsExportAllowed(CustomTransferReason.Reason material)
        {
            if (m_bExportAllowed is null)
            {
                m_bExportAllowed = BuildingSettingsFast.IsExportAllowed(GetBuilding(), GetBuildingType(), material, IsIncoming());
            }

            return m_bExportAllowed.Value;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsOutsideConnectionExcluded(CustomTransferReason.Reason material, ushort outsideConnectionbuildingId)
        {
            if (m_excludedOutsideConnections is null)
            {
                m_excludedOutsideConnections = BuildingSettingsFast.GetOutsideConnectionExclusionList(GetBuilding(), GetBuildingType(), material, IsIncoming());
            }

            return m_excludedOutsideConnections.Contains(outsideConnectionbuildingId);
        }

        // -------------------------------------------------------------------------------------------
        public DistrictRestrictionSettings.PreferLocal GetDistrictRestriction(CustomTransferReason.Reason material)
        {
            return m_districtRestrictions.GetDistrictRestriction(this, material);
        }

        // -------------------------------------------------------------------------------------------
        public HashSet<DistrictData> GetAllowedDistrictList(CustomTransferReason.Reason material)
        {
            return m_districtRestrictions.GetAllowedDistrictList(this, material);
        }

        // -------------------------------------------------------------------------------------------
        public HashSet<DistrictData> GetActualDistrictList()
        {
            return m_districtRestrictions.GetActualDistrictList(this);
        }

        // -------------------------------------------------------------------------------------------
        public byte GetDistrict()
        {
            return m_districtRestrictions.GetDistrict(this);
        }

        // -------------------------------------------------------------------------------------------
        public byte GetArea()
        {
            return m_districtRestrictions.GetArea(this);
        }

        // -------------------------------------------------------------------------------------------
        public HashSet<ushort> GetAllowedBuildingList(CustomTransferReason.Reason material)
        {
            return m_buildingRestrictions.GetAllowedBuildingList(this, material);
        }

        // -------------------------------------------------------------------------------------------
        public bool IsValid()
        {
            if (m_bIsValid is null)
            {
                switch (m_object.Type)
                {
                    case InstanceType.Building:
                        {
                            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_object.Building];
                            m_bIsValid = building.m_flags != 0;
                            if (!m_bIsValid.Value)
                            {
                                TransferManagerStats.s_iInvalidBuildingObjects++;
                            }
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[m_object.Vehicle];                     
                            m_bIsValid = vehicle.m_flags != 0 && vehicle.m_sourceBuilding != 0;
                            if (!m_bIsValid.Value)
                            {
                                TransferManagerStats.s_iInvalidVehicleObjects++;
                            }

                            break;
                        }
                    case InstanceType.Citizen:
                        {
                            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[m_object.Citizen];
                            m_bIsValid = citizen.m_flags != 0;
                            if (!m_bIsValid.Value)
                            {
                                TransferManagerStats.s_iInvalidCitizenObjects++;
                            }
                            break;
                        }
                    default:
                        {
                            m_bIsValid = true;
                            break;
                        }
                }
            }

            return m_bIsValid.Value;
        }

        // -------------------------------------------------------------------------------------------
        public float GetDistanceRestrictionSquaredMeters(CustomTransferReason.Reason material)
        {
            if (m_fDistanceRestrictionSquared is null)
            {
                m_fDistanceRestrictionSquared = BuildingSettingsFast.GetDistanceRestrictionSquaredMeters(GetBuilding(), GetBuildingType(), material, IsIncoming());
            }

            return m_fDistanceRestrictionSquared.Value;
        }

        // -------------------------------------------------------------------------------------------
        public float GetPriorityFactor(CustomTransferReason.Reason material)
        {
            if (m_priorityFactor is null)
            {
                m_priorityFactor = PriorityModifier(material, Priority);
            }
            return m_priorityFactor.Value;
        }

        // -------------------------------------------------------------------------------------------
        public ushort GetNearestNode(CustomTransferReason.Reason material)
        {
            if (m_nearestNode == ushort.MaxValue)
            {
                if (CanUsePathingForCandidate(material))
                {
                    m_nearestNode = PathNode.FindNearestNode(material, this);

                    // We couldn't get a start node for this candidate offer, log it to No road access tab
                    if (m_nearestNode == 0 && Building != 0)
                    {
                        RoadAccessStorage.AddInstance(m_object);
                    }
                }
                else
                {
                    m_nearestNode = 0;
                }
            }
            return m_nearestNode;
        }

        // -------------------------------------------------------------------------------------------
        public bool HasNearestNode()
        {
            return (m_nearestNode != ushort.MaxValue);
        }

        // -------------------------------------------------------------------------------------------
        public bool IsExportVehicleLimitOk(CustomTransferReason.Reason material)
        {
            if (m_IsExportVehicleLimitOk is null)
            {
                m_IsExportVehicleLimitOk = true;

                int iExportLimitPercent = SaveGameSettings.GetSettings().ExportVehicleLimit;
                if (iExportLimitPercent == 0)
                {
                    // No export allowed
                    m_IsExportVehicleLimitOk = false;
                }
                else if (iExportLimitPercent < 100)
                {
                    int iTotalTrucks = BuildingVehicleCount.GetMaxVehicleCount(GetBuildingType(), GetBuilding(), 0);
                    if (iTotalTrucks > 0)
                    {
                        int outside = BuildingUtils.CountImportExportVehicles(GetBuilding(), material);
                        float fExportLimitPercent = (float)iExportLimitPercent * 0.01f;
                        int maxExport = (int)((float)iTotalTrucks * fExportLimitPercent);
                        if (outside >= maxExport)
                        {
                            m_IsExportVehicleLimitOk = false;
                        }
                    }
                }
            }

            return m_IsExportVehicleLimitOk.Value;
        }

        // -------------------------------------------------------------------------------------------
        public TransportType GetTransportType()
        {
            if (m_transportType == TransportType.None)
            {
                if (GetBuilding() != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuilding()];
                    m_transportType = TransportUtils.GetTransportType(building);
                }
            }

            return m_transportType;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsMassTransit()
        {
            switch (GetTransportType())
            {
                case TransportType.Plane:
                case TransportType.Train:
                case TransportType.Ship:
                    return true;
            }

            return false;
        }

        // -------------------------------------------------------------------------------------------
        public bool IsCargoWarehouse()
        {
            return (GetBuildingType() == BuildingType.CargoWarehouse);
        }

        // -------------------------------------------------------------------------------------------
        public WarehouseStationOffer GetWarehouseStationOffer()
        {
            if (m_warehouseStationOfferType == WarehouseStationOffer.None)
            {
                if (IsCargoWarehouse())
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuilding()];
                    if (building.m_parentBuilding != 0)
                    {
                        m_warehouseStationOfferType = WarehouseStationOffer.WarehouseTrain;
                    }
                    else
                    {
                        m_warehouseStationOfferType = WarehouseStationOffer.WarehouseRoad;
                    }
                }
            }
            
            return m_warehouseStationOfferType;
        }

        // -------------------------------------------------------------------------------------------
        // Check Building and GetBuilding() as they may be different eg. Cargo Warehouse
        public bool Intersects(HashSet<ushort> buildingIds)
        {
            if (Building != 0 && buildingIds.Contains(Building))
            {
                return true;
            }
            else if (GetBuilding() != 0 && buildingIds.Contains(GetBuilding()))
            {
                return true;
            }

            return false;
        }

        // -------------------------------------------------------------------------------------------
        public static float PriorityModifier(CustomTransferReason.Reason material, int iPriority)
        {
            if (TransferManagerModes.IsScaleByPriority(material))
            {
                // Scale by priority. Higher priorities will appear closer
                // We use power of 2 rather than Priority squared (we are dealing with squared distance)
                // to really weight high priorities much closer as it has a much higher curve after P:5
                return 1.0f / (float)Math.Pow(2, iPriority);
            }
            else
            {
                return 1.0f;
            }
        }

        // -------------------------------------------------------------------------------------------
        private bool CanUsePathingForCandidate(CustomTransferReason.Reason material)
        {
            if (LocalPark > 0)
            {
                // It's a pedestrian zone, don't use pathing as they dont use trucks
                return false;
            }

            // Dont use path distance for helicopter transfer reasons
            if (TransferManagerModes.IsHelicopterReason(material))
            {
                return false;
            }

            // Don't use path distance for helicopters
            if (Vehicle != 0 && Singleton<VehicleManager>.instance.m_vehicles.m_buffer[Vehicle].Info.GetAI() is HelicopterAI)
            {
                return false;
            }

            return true;
        }
    }
}
