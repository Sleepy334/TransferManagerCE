using System;
using System.Collections.Generic;
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
        private OutsideConnection m_eOutside = OutsideConnection.Unknown;
        private int m_effectiveOutsideMultiplier = -1;
        private WarehouseMode m_warehouseMode = WarehouseMode.Unknown;
        private bool? m_IsReservedTrucksOk = null;
        private bool? m_IsValid = null;
        private float? m_fWarehouseIncomingStorage = null;
        private float? m_fWarehouseOutgoingStorage = null;
        private float m_fDistanceRestrictionSquared = -1;
        private BuildingTypeHelper.BuildingType m_buildingType = BuildingTypeHelper.BuildingType.None;
        public BuildingSettings.PreferLocal m_preferLocal = BuildingSettings.PreferLocal.UNKNOWN;
        public ushort m_nearestNode = ushort.MaxValue; // public so we can debug it without forcing it to be calculated.

        private HashSet<DistrictData>? m_actualDistricts = null;
        private HashSet<DistrictData>? m_allowedDistrictsIncoming = null;
        private HashSet<DistrictData>? m_allowedDistrictsOutgoing = null;
        
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
            m_IsReservedTrucksOk = offer.m_IsReservedTrucksOk;
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
            get { return m_offer.Position; }
            set { m_offer.Position = value; }
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
                m_effectiveOutsideMultiplier = BuildingSettings.GetEffectiveOutsideMultiplier(GetBuilding());
            }

            return m_effectiveOutsideMultiplier;
        }

        public bool IsWarehouse()
        {
            return Exclude; // Only set by warehouses.
        }

        public WarehouseMode GetWarehouseMode()
        {
            if (m_warehouseMode == WarehouseMode.Unknown)
            {
                ushort usBuildingId = GetBuilding();
                if (usBuildingId != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[usBuildingId];
                    if ((building.m_flags & global::Building.Flags.Filling) == global::Building.Flags.Filling)
                    {
                        m_warehouseMode = WarehouseMode.Fill;
                    }
                    else if ((building.m_flags & global::Building.Flags.Downgrading) == global::Building.Flags.Downgrading)
                    {
                        m_warehouseMode = WarehouseMode.Empty;
                    }
                    else
                    {
                        m_warehouseMode = WarehouseMode.Balanced;
                    }
                }
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

        public BuildingSettings.PreferLocal GetDistrictRestriction(bool bIncoming, TransferReason material)
        {
            if (m_preferLocal == BuildingSettings.PreferLocal.UNKNOWN)
            {
                m_preferLocal = DistrictRestrictions.GetPreferLocal(bIncoming, material, this);
            }
            if (m_preferLocal == BuildingSettings.PreferLocal.UNKNOWN)
            {
                Debug.Log($"{GetBuilding()}: Unknown district restriction");
            }
            return m_preferLocal;
        }

        public HashSet<DistrictData> GetAllowedIncomingDistrictList()
        {
            if (m_allowedDistrictsIncoming == null)
            {
                m_allowedDistrictsIncoming = BuildingSettings.GetAllowedDistrictsIncoming(GetBuilding());
            }
            return m_allowedDistrictsIncoming;
        }

        public HashSet<DistrictData> GetAllowedOutgoingDistrictList()
        {
            if (m_allowedDistrictsOutgoing == null)
            {
                m_allowedDistrictsOutgoing = BuildingSettings.GetAllowedDistrictsOutgoing(GetBuilding());
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

        public bool IsReservedTrucksOk(TransferReason material)
        {
            if (m_IsReservedTrucksOk == null)
            {
                int iReservePercent = BuildingSettings.ReserveCargoTrucksPercent(GetBuilding());
                if (iReservePercent > 0)
                {
                    float fExportPercent = 1.0f - ((float)iReservePercent * 0.01f);
                    int iTotalTrucks = CitiesUtils.GetWarehouseTruckCount(GetBuilding());
                    int outside = CitiesUtils.CountImportExportVehicles(GetBuilding(), material);

                    int maxExport = (int)((float)iTotalTrucks * fExportPercent);
                    if (outside < maxExport)
                    {
                        m_IsReservedTrucksOk = true;
                    }
                    else
                    {
                        m_IsReservedTrucksOk = false;
                    }
                }
                else
                {
                    m_IsReservedTrucksOk = true;
                }

            }

            return m_IsReservedTrucksOk.Value;
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

        public float GetDistanceRestrictionSquaredMeters(bool bIncoming, TransferReason material)
        {
            if (m_fDistanceRestrictionSquared < 0)
            {
                // Try and get local distance setting
                HashSet<TransferReason> supportedReasons;
                if (bIncoming) 
                {
                    supportedReasons = BuildingTypeHelper.IncomingDistanceRestrictionSupported(GetBuildingType(), GetBuilding());
                }
                else
                {
                    supportedReasons = BuildingTypeHelper.OutgoingDistanceRestrictionSupported(GetBuildingType(), GetBuilding());
                }
                if (supportedReasons.Contains(material))
                {
                    int iDistance = BuildingSettings.GetSettings(GetBuilding()).m_iServiceDistance;
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
    }
}
