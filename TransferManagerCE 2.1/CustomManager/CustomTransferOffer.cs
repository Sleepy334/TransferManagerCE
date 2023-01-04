using System;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    public class CustomTransferOffer
    {
        private enum OutsideConnection
        {
            Unknown,
            Internal,
            Outside,
        }

        public enum WarehouseMode
        {
            Unknown,
            Empty,
            Balanced,
            Fill,
        }

        // Actual TransferOffer object
        public TransferOffer m_offer;

        // Store these so we don't have to calculate them every time.
        private int m_offerBuildingId = -1; // Extracted from building/vehicle/citizen
        private bool? m_IsValid = null;
        private BuildingTypeHelper.BuildingType m_buildingType = BuildingTypeHelper.BuildingType.None;
        public ushort m_nearestNode = ushort.MaxValue; // public so we can debug it without forcing it to be calculated.
        private Vector3? m_position = null;

        // Outside connection
        private OutsideConnection m_eOutside = OutsideConnection.Unknown;
        private int m_effectiveOutsideMultiplier = -1;
        private bool? m_bExportAllowed = null;
        private bool? m_bImportAllowed = null;

        // Warehouse settings
        private WarehouseMode m_warehouseMode = WarehouseMode.Unknown;
        private float? m_fWarehouseIncomingStorage = null;
        private float? m_fWarehouseOutgoingStorage = null;
        private bool? m_IsExportVehicleLimitOk = null;

        // Districts settings
        public RestrictionSettings.PreferLocal m_preferLocal = RestrictionSettings.PreferLocal.UNKNOWN;
        private HashSet<DistrictData>? m_actualDistricts = null;
        private HashSet<DistrictData>? m_allowedDistrictsIncoming = null;
        private HashSet<DistrictData>? m_allowedDistrictsOutgoing = null;

        // Building restrictions
        private HashSet<ushort>? m_allowedBuildingsIncoming = null;
        private HashSet<ushort>? m_allowedBuildingsOutgoing = null;

        // Distance settings
        private float m_fDistanceRestrictionSquared = -1;

        public CustomTransferOffer(TransferOffer offer)
        {
            m_offer = offer;
        }

        public CustomTransferOffer(CustomTransferOffer offer)
        {
            m_offer = offer.m_offer;

            m_offerBuildingId = offer.m_offerBuildingId;
            m_eOutside = offer.m_eOutside;
            m_effectiveOutsideMultiplier = offer.m_effectiveOutsideMultiplier;
            m_warehouseMode = offer.m_warehouseMode;
            m_IsValid = offer.m_IsValid;
            m_fWarehouseIncomingStorage = offer.m_fWarehouseIncomingStorage;
            m_fWarehouseOutgoingStorage = offer.m_fWarehouseOutgoingStorage;
            m_fDistanceRestrictionSquared = offer.m_fDistanceRestrictionSquared;
            m_buildingType = offer.m_buildingType;
            m_preferLocal = offer.m_preferLocal;
            m_nearestNode = offer.m_nearestNode;

            if (offer.m_actualDistricts != null)
            {
                m_actualDistricts = new HashSet<DistrictData>(offer.m_actualDistricts);
            }
            if (offer.m_allowedDistrictsIncoming != null)
            {
                m_allowedDistrictsIncoming = new HashSet<DistrictData>(offer.m_allowedDistrictsIncoming);
            }
            if (offer.m_allowedDistrictsOutgoing != null)
            {
                m_allowedDistrictsOutgoing = new HashSet<DistrictData>(offer.m_allowedDistrictsOutgoing);
            }
            if (offer.m_allowedBuildingsIncoming != null)
            {
                m_allowedBuildingsIncoming = new HashSet<ushort>(offer.m_allowedBuildingsIncoming);
            }
            if (offer.m_allowedBuildingsOutgoing != null)
            {
                m_allowedBuildingsOutgoing = new HashSet<ushort>(offer.m_allowedBuildingsOutgoing);
            }
        }

        public static explicit operator TransferOffer(CustomTransferOffer offer)
        {
            return offer.m_offer;
        }

        public InstanceID m_object
        {
            get { return m_offer.m_object; }
            set { m_offer.m_object = value; }
        }

        public int Priority
        {
            get { return m_offer.Priority; }
            set { m_offer.Priority = value; }
        }

        public bool Active
        {
            get { return m_offer.Active; }
            set { m_offer.Active = value; }
        }

        public bool Exclude
        {
            get { return m_offer.Exclude; }
            set { m_offer.Exclude = value; }
        }

        public int Amount
        {
            get { return m_offer.Amount; }
            set { m_offer.Amount = value; }
        }

        public ushort Building
        {
            get { return m_offer.Building; }
            set { m_offer.Building = value; }
        }

        public ushort Vehicle
        {
            get { return m_offer.Vehicle; }
            set { m_offer.Vehicle = value; }
        }

        public uint Citizen
        {
            get { return m_offer.Citizen; }
            set { m_offer.Citizen = value; }
        }

        public byte Park
        {
            get { return m_offer.Park; }
            set { m_offer.Park = value; }
        }

        public ushort NetSegment
        {
            get { return m_offer.NetSegment; }
            set { m_offer.NetSegment = value; }
        }

        public ushort TransportLine
        {
            get { return m_offer.TransportLine; }
            set { m_offer.TransportLine = value; }
        }

        public Vector3 Position
        {
            get 
            { 
                if (m_position == null)
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

        public bool IsOutside()
        {
            if (m_eOutside == OutsideConnection.Unknown)
            {
                ushort usBuildingId = GetBuilding();
                if (usBuildingId != 0)
                {
                    bool bOutside = BuildingTypeHelper.IsOutsideConnection(usBuildingId);
                    if (bOutside)
                    {
                        m_eOutside = OutsideConnection.Outside;
                    }
                    else
                    {
                        m_eOutside = OutsideConnection.Internal;
                    }
                }
                else
                {
                    m_eOutside = OutsideConnection.Internal;
                }
            }

            return m_eOutside == OutsideConnection.Outside;
        }

        public int GetEffectiveOutsideModifier()
        {
            if (m_effectiveOutsideMultiplier == -1)
            {
                m_effectiveOutsideMultiplier = BuildingSettingsStorage.GetEffectiveOutsideMultiplier(GetBuilding());
            }

            return m_effectiveOutsideMultiplier;
        }

        public bool IsWarehouse()
        {
            return Exclude; // Only set by warehouses.
        }

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

        public WarehouseMode GetWarehouseMode()
        {
            if (m_warehouseMode == WarehouseMode.Unknown)
            {
                m_warehouseMode = CitiesUtils.GetWarehouseMode(GetBuilding());
            }

            return m_warehouseMode;
        }

        public float GetWarehouseIncomingStoragePercent()
        {
            if (m_fWarehouseIncomingStorage == null)
            {
                m_fWarehouseIncomingStorage = 1.0f; // Not valid

                if (IsWarehouse() && GetBuilding() != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuilding()];
                    WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                    if (warehouse != null)
                    {
                        // For incoming we use the storage buffer and incoming supply
                        TransferReason actualTransferReason = warehouse.GetActualTransferReason(GetBuilding(), ref building);
                        int iTransferSize = CitiesUtils.GetGuestVehiclesTransferSize(GetBuilding(), actualTransferReason);
                        double dStorage = building.m_customBuffer1 * 0.1 + iTransferSize * 0.001;
                        double dCapacity = warehouse.m_storageCapacity * 0.001;
                        double dInPercent = dStorage / dCapacity;
                        m_fWarehouseIncomingStorage = (float)Math.Min(dInPercent, 1.0);
                    }
                }
            }

            return m_fWarehouseIncomingStorage.Value;
        }

        public float GetWarehouseOutgoingStoragePercent()
        {
            if (m_fWarehouseOutgoingStorage == null)
            {
                m_fWarehouseOutgoingStorage = 1.0f; // Not valid

                if (IsWarehouse() && GetBuilding() != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuilding()];
                    WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                    if (warehouse != null)
                    {
                        // For outgoing we use the actual storage buffer only, not including incoming supply
                        double dStorage = building.m_customBuffer1 * 0.1;
                        double dCapacity = warehouse.m_storageCapacity * 0.001;
                        double dInPercent = dStorage / dCapacity;
                        m_fWarehouseOutgoingStorage = (float)Math.Min(dInPercent, 1.0);
                    }
                }
            }

            return m_fWarehouseOutgoingStorage.Value;
        }

        public ushort GetBuilding()
        {
            if (m_offerBuildingId == -1)
            {
                m_offerBuildingId = CustomTransferManager.GetOfferBuilding(this);
            }
            return (ushort)m_offerBuildingId;
        }

        public BuildingTypeHelper.BuildingType GetBuildingType() 
        {
            if (m_buildingType == BuildingTypeHelper.BuildingType.None && GetBuilding() != 0)
            {
                m_buildingType = BuildingTypeHelper.GetBuildingType(GetBuilding());
            }
            return m_buildingType;
        }

        public bool IsImportAllowed(TransferReason material)
        {
            if (m_bImportAllowed == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                m_bImportAllowed = restrictions.m_bAllowImport;
            }

            return m_bImportAllowed.Value;
        }

        public bool IsExportAllowed(TransferReason material)
        {
            if (m_bExportAllowed == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                m_bExportAllowed = restrictions.m_bAllowExport;
            }

            return m_bExportAllowed.Value;
        }

        public RestrictionSettings.PreferLocal GetDistrictRestriction(bool bIncoming, TransferReason material)
        {
            if (m_preferLocal == RestrictionSettings.PreferLocal.UNKNOWN)
            {
                m_preferLocal = DistrictRestrictions.GetPreferLocal(bIncoming, material, this);

                if (m_preferLocal == RestrictionSettings.PreferLocal.UNKNOWN)
                {
                    Debug.Log($"{GetBuilding()}: Unknown district restriction");
                }
            }
            
            return m_preferLocal;
        }

        public HashSet<DistrictData> GetAllowedIncomingDistrictList(TransferReason material)
        {
            if (m_allowedDistrictsIncoming == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                m_allowedDistrictsIncoming = restrictions.GetAllowedDistrictsIncoming(GetBuilding());
            }
            return m_allowedDistrictsIncoming;
        }

        public HashSet<DistrictData> GetAllowedOutgoingDistrictList(TransferReason material)
        {
            if (m_allowedDistrictsOutgoing == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                m_allowedDistrictsOutgoing = restrictions.GetAllowedDistrictsOutgoing(GetBuilding());
            }
            return m_allowedDistrictsOutgoing;
        }

        public HashSet<DistrictData> GetActualDistrictList()
        {
            if (m_actualDistricts == null)
            {
                // Handle non-building types as well
                m_actualDistricts = new HashSet<DistrictData>();

                byte district = DistrictManager.instance.GetDistrict(Position);
                if (district != 0)
                {
                    m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.District, district));
                }

                byte park;
                if (m_object.Type == InstanceType.Park)
                {
                    park = m_object.Park;
                }
                else
                {
                    park = DistrictManager.instance.GetPark(Position);
                }
                if (park != 0)
                {
                    m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.Park, park));
                }
            }

            return m_actualDistricts;
        }

        public bool IsValid()
        {
            if (m_IsValid == null)
            {
                bool bValid = CustomTransferManager.IsValidObject(m_object);
                if (bValid)
                {
                    m_IsValid = true;
                }
                else
                {
                    m_IsValid = false;
                }
            }

            return m_IsValid.Value;
        }

        public float GetDistanceRestrictionSquaredMeters(TransferReason material)
        {
            if (m_fDistanceRestrictionSquared < 0)
            {
                // Try and get local distance setting
                if (BuildingRuleSets.HasDistanceRules(GetBuildingType(), material))
                {
                    int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                    int iDistance = BuildingSettingsStorage.GetSettings(GetBuilding()).GetRestrictions(iRestrictionId).m_iServiceDistance;
                    if (iDistance > 0)
                    {
                        m_fDistanceRestrictionSquared = (float)Math.Pow(iDistance * 1000, 2);
                    }
                }

                // Load global setting if we didnt get a local one.
                if (m_fDistanceRestrictionSquared < 0)
                {
                    m_fDistanceRestrictionSquared = SaveGameSettings.GetSettings().GetActiveDistanceRestrictionSquaredMeters(material);
                }
            }

            return m_fDistanceRestrictionSquared;
        }

        public ushort GetNearestNode(TransferReason material)
        {
            if (m_nearestNode == ushort.MaxValue)
            {
                m_nearestNode = PathNode.FindNearestNode(material, this);
            }
            return m_nearestNode;
        }

        public HashSet<ushort> GetAllowedIncomingBuildingList(TransferReason material)
        {
            if (m_allowedBuildingsIncoming == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                if (iRestrictionId != -1)
                {
                    BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                    RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                    m_allowedBuildingsIncoming = restrictions.m_incomingBuildingsAllowed;
                }
                else
                {
                    m_allowedBuildingsIncoming = new HashSet<ushort>();
                }
            }
            return m_allowedBuildingsIncoming;
        }

        public HashSet<ushort> GetAllowedOutgoingBuildingList(TransferReason material)
        {
            if (m_allowedBuildingsOutgoing == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                if (iRestrictionId != -1)
                {
                    BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                    RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                    m_allowedBuildingsOutgoing = restrictions.m_outgoingBuildingsAllowed;
                }
                else
                {
                    m_allowedBuildingsOutgoing = new HashSet<ushort>();
                }
            }
            return m_allowedBuildingsOutgoing;
        }

        public bool IsExportVehicleLimitOk(TransferReason material)
        {
            if (m_IsExportVehicleLimitOk == null)
            {
                m_IsExportVehicleLimitOk = true;

                int iExportLimitPercent = SaveGameSettings.GetSettings().ExportVehicleLimit;
                if (iExportLimitPercent < 100)
                {
                    float fExportLimitPercent = (float)iExportLimitPercent * 0.01f;
                    int iTotalTrucks = BuildingVehicleCount.GetMaxVehicleCount(GetBuildingType(), GetBuilding());
                    if (iTotalTrucks > 0)
                    {
                        int outside = CitiesUtils.CountImportExportVehicles(GetBuilding(), material);
                        int maxExport = (int)((float)iTotalTrucks * fExportLimitPercent);
                        if (outside > maxExport)
                        {
                            m_IsExportVehicleLimitOk = false;
                        }
                    }
                }

            }

            return m_IsExportVehicleLimitOk.Value;
        }
    }
}
