using System;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    public class CustomTransferOffer
    {
        public enum WarehouseMode
        {
            Unknown,
            Empty,
            Balanced,
            Fill,
        }

        // Actual TransferOffer object
        public TransferOffer m_offer;
        bool m_bIncoming; // Offer direction

        // Store these so we don't have to calculate them every time.
        private ushort? m_offerBuildingId = null; // Extracted from building/vehicle/citizen
        private bool? m_bIsValid = null;
        private BuildingTypeHelper.BuildingType m_buildingType = BuildingTypeHelper.BuildingType.None;
        public ushort m_nearestNode = ushort.MaxValue; // public so we can debug it without forcing it to be calculated.
        private Vector3? m_position = null;

        // Outside connection
        private bool? m_bOutside = null;
        private byte? m_effectiveOutsideMultiplier = null;
        private bool? m_bExportAllowed = null;
        private bool? m_bImportAllowed = null;

        // Warehouse settings
        private WarehouseMode m_warehouseMode = WarehouseMode.Unknown;
        private float? m_fWarehouseStorage = null;
        private bool? m_IsExportVehicleLimitOk = null;

        // Districts settings
        private byte? m_district = null;
        private byte? m_area = null;
        private RestrictionSettings.PreferLocal m_preferLocal = RestrictionSettings.PreferLocal.Unknown;
        private HashSet<DistrictData>? m_actualDistricts = null;
        private HashSet<DistrictData>? m_allowedDistricts = null;
        private static readonly HashSet<DistrictData> s_emptyDistrictSet = new HashSet<DistrictData>(); // Used when empty

        // Building restrictions
        private HashSet<ushort>? m_allowedBuildings = null;
        private static readonly HashSet<ushort> s_emptyBuildingSet = new HashSet<ushort>(); // Used when empty

        // Distance settings
        private float? m_fDistanceRestrictionSquared = null;

        // Priority scaling
        private float? m_priorityFactor = null;

        public CustomTransferOffer(bool bIncoming, TransferOffer offer)
        {
            m_bIncoming = bIncoming;
            m_offer = offer;
        }

        private CustomTransferOffer(CustomTransferOffer offer)
        {
            throw new NotImplementedException();
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

        public bool IsIncoming()
        {
            return m_bIncoming;
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

        public int LocalPark
        {
            get { return m_offer.m_isLocalPark; }
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
            if (m_bOutside == null)
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

        public int GetEffectiveOutsideModifier()
        {
            if (m_effectiveOutsideMultiplier == null)
            {
                m_effectiveOutsideMultiplier = (byte) BuildingSettingsStorage.GetEffectiveOutsideMultiplier(GetBuilding());
            }

            return m_effectiveOutsideMultiplier.Value;
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

        public float GetWarehouseStoragePercent()
        {
            if (m_fWarehouseStorage == null)
            {
                m_fWarehouseStorage = 1.0f; // Not valid

                if (IsWarehouse() && GetBuilding() != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuilding()];
                    WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                    if (warehouse != null)
                    {
                        if (m_bIncoming)
                        {
                            // For incoming we use the storage buffer and incoming supply
                            TransferReason actualTransferReason = warehouse.GetActualTransferReason(GetBuilding(), ref building);
                            int iTransferSize = CitiesUtils.GetGuestVehiclesTransferSize(GetBuilding(), actualTransferReason);
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

            return m_fWarehouseStorage.Value;
        }

        public ushort GetBuilding()
        {
            if (m_offerBuildingId == null)
            {
                m_offerBuildingId = CustomTransferManager.GetOfferBuilding(this);
            }
            return m_offerBuildingId.Value;
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

        public RestrictionSettings.PreferLocal GetDistrictRestriction(TransferReason material)
        {
            if (m_preferLocal == RestrictionSettings.PreferLocal.Unknown)
            {
                // Default is all districts
                m_preferLocal = RestrictionSettings.PreferLocal.AllDistricts;

                // Local setting
                ushort buildingId = GetBuilding();
                if (buildingId != 0)
                {
                    BuildingType eBuildingType = GetBuildingType();
                    if (IsIncoming())
                    {
                        if (BuildingRuleSets.HasIncomingDistrictRules(eBuildingType, material))
                        {
                            RestrictionSettings settings = BuildingSettingsStorage.GetRestrictions(buildingId, eBuildingType, material);
                            m_preferLocal = settings.m_iPreferLocalDistrictsIncoming;
                        }
                    }
                    else
                    {
                        if (BuildingRuleSets.HasOutgoingDistrictRules(eBuildingType, material))
                        {
                            RestrictionSettings settings = BuildingSettingsStorage.GetRestrictions(buildingId, eBuildingType, material);
                            m_preferLocal = settings.m_iPreferLocalDistrictsOutgoing;
                        }
                    }
                }
            }
            
            return m_preferLocal;
        }

        public HashSet<DistrictData> GetAllowedDistrictList(TransferReason material)
        {
            if (m_allowedDistricts == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                if (m_bIncoming)
                {
                    m_allowedDistricts = restrictions.GetAllowedDistrictsIncoming(GetBuilding());
                }
                else
                {
                    m_allowedDistricts = restrictions.GetAllowedDistrictsOutgoing(GetBuilding());
                }
            }
            return m_allowedDistricts;
        }

        public HashSet<DistrictData> GetActualDistrictList()
        {
            if (m_actualDistricts == null)
            {
                // get districts
                byte district = GetDistrict();
                byte area = GetArea();

                if (district != 0 || area != 0)
                {
                    // Handle non-building types as well
                    m_actualDistricts = new HashSet<DistrictData>();

                    if (district != 0)
                    {
                        m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.District, district));
                    }
                    if (area != 0)
                    {
                        m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.Park, area));
                    }
                }
                else
                {
                    // Use the empty set so we save on memeory allocations
                    m_actualDistricts = s_emptyDistrictSet;
                }
            }

            return m_actualDistricts;
        }

        public byte GetDistrict()
        {
            if (m_district == null)
            {
                m_district = DistrictManager.instance.GetDistrict(Position);
            }
            return m_district.Value;
        }

        public byte GetArea()
        {
            if (m_area == null)
            {
                // Handle non-building types as well
                if (m_object.Type == InstanceType.Park)
                {
                    m_area = m_object.Park;
                }
                else
                {
                    m_area = DistrictManager.instance.GetPark(Position);
                }
            }
            return m_area.Value;
        }

        public HashSet<ushort> GetAllowedBuildingList(TransferReason material)
        {
            if (m_allowedBuildings == null)
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(GetBuildingType(), material);
                if (iRestrictionId != -1)
                {
                    BuildingSettings settings = BuildingSettingsStorage.GetSettings(GetBuilding());
                    if (settings.HasRestrictions(iRestrictionId))
                    {
                        RestrictionSettings restrictions = settings.GetRestrictions(iRestrictionId);
                        if (m_bIncoming)
                        {
                            if (restrictions.HasIncomingBuildingRestrictions())
                            {
                                // Take a copy for thread safety
                                m_allowedBuildings = restrictions.GetIncomingBuildingRestrictionsCopy();
                            }
                        }
                        else
                        {
                            if (restrictions.HasOutgoingBuildingRestrictions())
                            {
                                // Take a copy for thread safety
                                m_allowedBuildings = restrictions.GetOutgoingBuildingRestrictionsCopy();
                            }
                        }
                    }
                }

                if (m_allowedBuildings == null)
                {
                    m_allowedBuildings = s_emptyBuildingSet;
                }
            }
            return m_allowedBuildings;
        }

        public bool IsValid()
        {
            if (m_bIsValid == null)
            {
                m_bIsValid = CustomTransferManager.IsValidObject(m_object);
            }

            return m_bIsValid.Value;
        }

        public float GetDistanceRestrictionSquaredMeters(TransferReason material)
        {
            if (m_fDistanceRestrictionSquared == null)
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
                if (m_fDistanceRestrictionSquared == null)
                {
                    m_fDistanceRestrictionSquared = SaveGameSettings.GetSettings().GetActiveDistanceRestrictionSquaredMeters(material);
                }
            }

            return m_fDistanceRestrictionSquared.Value;
        }

        public float GetPriorityFactor(TransferReason material)
        {
            if (m_priorityFactor == null)
            {
                m_priorityFactor = PriorityModifier(material, Priority);
            }
            return m_priorityFactor.Value;
        }
        
        public ushort GetNearestNode(TransferReason material)
        {
            if (m_nearestNode == ushort.MaxValue)
            {
                m_nearestNode = PathNode.FindNearestNode(material, this);
            }
            return m_nearestNode;
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

        public static float PriorityModifier(CustomTransferReason material, int iPriority)
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
    }
}
