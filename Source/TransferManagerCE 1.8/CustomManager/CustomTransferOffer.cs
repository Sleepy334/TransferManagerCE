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

        private enum Boolean3State
        {
            Unknown,
            True,
            False,
        }

        public TransferOffer m_offer;

        // Store these so we don't have to calculate them every time.
        private int m_offerBuildingId; // Extracted from building/vehicle/citizen
        private OutsideConnection m_eOutside;
        private int m_effectiveOutsideMultiplier;
        private WarehouseMode m_warehouseMode;
        private Boolean3State m_IsReservedTrucksOk;
        private Boolean3State m_IsValid;
        private float m_fWarehouseStorage;
        private float m_fDistanceRestrictionSquared = -1;
        private List<DistrictData>? m_actualDistricts;
        private List<DistrictData>? m_allowedDistrictsIncoming;
        private List<DistrictData>? m_allowedDistrictsOutgoing;

        public CustomTransferOffer(TransferOffer offer)
        {
            m_offer = offer;
            m_eOutside = OutsideConnection.Unknown;
            m_effectiveOutsideMultiplier = -1;
            m_warehouseMode = WarehouseMode.Unknown;
            m_fWarehouseStorage = -2.0f;
            m_offerBuildingId = -1; 
            m_IsReservedTrucksOk = Boolean3State.Unknown;
            m_IsValid = Boolean3State.Unknown;
            m_actualDistricts = null;
            m_allowedDistrictsIncoming = null;
            m_allowedDistrictsOutgoing = null;
        }

        public CustomTransferOffer(CustomTransferOffer offer)
        {
            m_offer = offer.m_offer;
            m_eOutside = offer.m_eOutside;
            m_effectiveOutsideMultiplier = offer.m_effectiveOutsideMultiplier;
            m_warehouseMode = offer.m_warehouseMode;
            m_fWarehouseStorage = offer.m_fWarehouseStorage;
            m_offerBuildingId = offer.m_offerBuildingId;
            m_IsReservedTrucksOk = offer.m_IsReservedTrucksOk;
            m_IsValid = offer.m_IsValid;

            if (offer.m_actualDistricts != null)
            {
                m_actualDistricts = new List<DistrictData>(offer.m_actualDistricts);
            }
            if (offer.m_allowedDistrictsIncoming != null)
            {
                m_allowedDistrictsIncoming = new List<DistrictData>(offer.m_allowedDistrictsIncoming);
            }
            if (offer.m_allowedDistrictsOutgoing != null)
            {
                m_allowedDistrictsOutgoing = new List<DistrictData>(offer.m_allowedDistrictsOutgoing);
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

        public float GetWarehouseStoragePercent()
        {
            if (m_fWarehouseStorage == -2.0f)
            {
                m_fWarehouseStorage = 1.0f; // Not valid

                if (IsWarehouse() && GetBuilding() != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuilding()];
                    WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                    if (warehouse != null)
                    {
                        TransferReason actualTransferReason = warehouse.GetActualTransferReason(GetBuilding(), ref building);
                        int iTransferSize = CitiesUtils.GetGuestVehiclesTransferSize(GetBuilding(), actualTransferReason);
                        double dStorage = building.m_customBuffer1 * 0.1 + iTransferSize * 0.001;
                        double dCapacity = warehouse.m_storageCapacity * 0.001;
                        double dInPercent = dStorage / dCapacity;
                        m_fWarehouseStorage = (float)Math.Min(dInPercent, 1.0);
                    }
                }
            }

            return m_fWarehouseStorage;
        }

        public ushort GetBuilding()
        {
            if (m_offerBuildingId == -1)
            {
                m_offerBuildingId = CustomTransferManager.GetOfferBuilding(this);
            }
            return (ushort)m_offerBuildingId;
        }

        public List<DistrictData> GetAllowedIncomingDistrictList()
        {
            if (m_allowedDistrictsIncoming == null)
            {
                m_allowedDistrictsIncoming = BuildingSettings.GetAllowedDistrictsIncoming(GetBuilding());
            }
            return m_allowedDistrictsIncoming;
        }

        public List<DistrictData> GetAllowedOutgoingDistrictList()
        {
            if (m_allowedDistrictsOutgoing == null)
            {
                m_allowedDistrictsOutgoing = BuildingSettings.GetAllowedDistrictsOutgoing(GetBuilding());
            }
            return m_allowedDistrictsOutgoing;
        }

        public List<DistrictData> GetActualDistrictList()
        {
            if (m_actualDistricts == null)
            {
                // Handle non-building types as well
                m_actualDistricts = new List<DistrictData>();

                byte district = InstanceHelper.GetDistrict(m_object);
                if (district != 0)
                {
                    m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.District, district));
                }

                byte park = InstanceHelper.GetPark(m_object);
                if (park != 0)
                {
                    m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.Park, park));
                }     
            }

            return m_actualDistricts;
        }

        public bool IsReservedTrucksOk(TransferReason material)
        {
            if (m_IsReservedTrucksOk == Boolean3State.Unknown)
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
                        m_IsReservedTrucksOk = Boolean3State.True;
                    }
                    else
                    {
                        m_IsReservedTrucksOk = Boolean3State.False;
                    }
                }
                else
                {
                    m_IsReservedTrucksOk = Boolean3State.True;
                }

            }

            return m_IsReservedTrucksOk == Boolean3State.True;
        }

        public bool IsValid()
        {
            if (m_IsValid == Boolean3State.Unknown)
            {
                bool bValid = CustomTransferManager.IsValidObject(m_object);
                if (bValid)
                {
                    m_IsValid = Boolean3State.True;
                }
                else
                {
                    m_IsValid = Boolean3State.False;
                }
            }

            return m_IsValid == Boolean3State.True;
        }

        public float GetDistanceRestrictionSquaredMeters(TransferReason material)
        {
            if (m_fDistanceRestrictionSquared < 0)
            {
                int iDistance = BuildingSettings.GetSettings(GetBuilding()).m_iServiceDistance;
                if (iDistance > 0)
                {
                    m_fDistanceRestrictionSquared = (float)Math.Pow(iDistance * 1000, 2);
                }
                else
                {
                    m_fDistanceRestrictionSquared = SaveGameSettings.GetSettings().GetActiveDistanceRestrictionSquaredMeters(material);
                }
            }

            return m_fDistanceRestrictionSquared;
        }
    }
}
