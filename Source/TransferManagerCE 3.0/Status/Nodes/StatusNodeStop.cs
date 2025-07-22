using ColossalFramework;
using ColossalFramework.Math;
using SleepyCommon;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusNodeStop : StatusData
    {
        protected ushort m_nodeId;
        protected ushort m_vehicleId;

        public StatusNodeStop(BuildingType eBuildingType, ushort m_buildingId, ushort nodeId, ushort vehicleId) :
            base(CustomTransferReason.Reason.None, eBuildingType, m_buildingId)
        {
            m_nodeId = nodeId;
            m_vehicleId = vehicleId;
            m_color = KnownColor.lightGrey;
        }

        public override int CompareTo(object second)
        {
            if (second is StatusNodeStop oSecond)
            {
                // Vehicles first
                if (oSecond.HasVehicle() != HasVehicle())
                {
                    return oSecond.HasVehicle().CompareTo(HasVehicle());
                }

                // Sort vehicles by distance (Ascending)
                if (oSecond.HasVehicle() && HasVehicle())
                {
                    return GetDistance().CompareTo(oSecond.GetDistance());
                }

                // Wait timer (Descending)
                if (oSecond.GetWaitTimer() != GetWaitTimer())
                {
                    return oSecond.GetWaitTimer().CompareTo(GetWaitTimer());
                }

                // Finally sort by node so they dont skip around
                if (oSecond.m_nodeId != m_nodeId)
                {
                    return m_nodeId.CompareTo(oSecond.m_nodeId);
                }
            }

            return base.CompareTo(second);
        }

        public override string GetMaterialDisplay()
        {
            return GetTransportType().ToString();
        }

        public override bool IsBuildingData()
        {
            return false;
        }

        public override bool HasVehicle()
        {
            return m_vehicleId != 0;
        }

        public override ushort GetVehicleId()
        {
            return m_vehicleId;
        }

        protected abstract TransportInfo.TransportType GetTransportType();

        protected override string CalculateValue(out string tooltip)
        {
            UpdateTextColor();
            int iCount = CitiesUtils.CalculatePassengerCount(m_nodeId, GetTransportType());
            tooltip = $"Waiting Passengers: {iCount}";
            return iCount.ToString();
        }

        public int GetWaitTimer()
        {
            NetNode node = NetManager.instance.m_nodes.m_buffer[m_nodeId];
            if (node.m_flags != 0)
            {
                return node.m_maxWaitTime;
            }
            return 0;
        }

        protected override string CalculateTimer(out string tooltip)
        {
            int iWaitTimer = GetWaitTimer();
            tooltip = $"Wait Timer: {iWaitTimer}";
            return $"W:{iWaitTimer}";
        }

        protected override double CalculateDistance()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    InstanceID target = new InstanceID { NetNode = m_nodeId };
                    Vector3 nodePos = InstanceHelper.GetPosition(target);
                    Vector3 vehiclePos = vehicle.GetLastFramePosition();
                    return Math.Sqrt(Vector3.SqrMagnitude(vehiclePos - nodePos)) * 0.001;
                }
            }

            return double.MaxValue;
        }

        protected override string CalculateVehicle(out string tooltip) 
        { 
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                tooltip = InstanceHelper.DescribeInstance(new InstanceID { Vehicle = vehicleId }, true, true);
                return CitiesUtils.GetVehicleName(vehicleId);
            }

            tooltip = "";
            return "";
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = $"Node:{m_nodeId}";
            return tooltip;
        }

        public override void OnClickResponder()
        {
            if (m_nodeId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { NetNode = m_nodeId });
            }
        }

        private void UpdateTextColor()
        {
            m_color = KnownColor.lightGrey;
        }
    }
}