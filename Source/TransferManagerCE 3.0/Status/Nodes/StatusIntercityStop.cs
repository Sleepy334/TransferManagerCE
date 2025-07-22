using ColossalFramework;
using SleepyCommon;
using System;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;
using ColossalFramework.Math;

namespace TransferManagerCE.Data
{
    public class StatusIntercityStop : StatusData
    {
        private ushort m_segmentId;
        private ushort m_startNodeId;
        private ushort m_endNodeId;
        private ushort m_vehicleId;
        private ushort m_startBuildingId;
        private ushort m_endBuildingId;

        // ----------------------------------------------------------------------------------------
        public StatusIntercityStop(BuildingType eBuildingType, ushort buildingId, ushort segmentId, ushort startNodeId, ushort endNodeId, ushort targetVehicleId) :
            base(CustomTransferReason.Reason.None, eBuildingType, buildingId)
        {
            m_segmentId = segmentId;
            m_startNodeId = startNodeId;
            m_endNodeId = endNodeId;
            m_vehicleId = targetVehicleId;
            m_startBuildingId = FindOutsideConnectionBuilding(m_startNodeId);
            m_endBuildingId = FindOutsideConnectionBuilding(m_endNodeId);
        }

        public override int CompareTo(object second)
        {
            if (second is StatusIntercityStop oSecond)
            {
                // Vehicles first
                if (oSecond.HasVehicle() != HasVehicle())
                {
                    return oSecond.HasVehicle().CompareTo(HasVehicle());
                }

                // Sort vehicles by distance
                if (oSecond.HasVehicle() && HasVehicle())
                {
                    return GetDistance().CompareTo(oSecond.GetDistance());
                }

                // Wait timer
                if (oSecond.GetWaitTimer() != GetWaitTimer())
                {
                    return oSecond.GetWaitTimer().CompareTo(GetWaitTimer());
                }

                // Finally sort by OC id so they dont shift around
                if (oSecond.GetOutsideConnectionBuildingId() != GetOutsideConnectionBuildingId())
                {
                    return GetOutsideConnectionBuildingId().CompareTo(oSecond.GetOutsideConnectionBuildingId());
                }
            }

            return base.CompareTo(second);
        }

        public override bool IsBuildingData()
        {
            return false;
        }

        public override string GetMaterialDisplay()
        {
            return "Intercity Stop";
        }

        public override ushort GetVehicleId()
        {
            return m_vehicleId;
        }

        public override bool HasVehicle()
        {
            return m_vehicleId != 0;
        }

        protected override string CalculateValue(out string tooltip)
        {
            UpdateTextColor();
            int iCount = CitiesUtils.CalculatePassengerCount(m_startNodeId, GetTransportType());
            tooltip = $"Waiting Passengers: {iCount}";
            return iCount.ToString();
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
                    InstanceID target = new InstanceID { NetNode = m_endNodeId };
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
                tooltip = VehicleUtils.GetVehicleTooltip(vehicleId);
                return CitiesUtils.GetVehicleName(vehicleId);
            }

            tooltip = "";
            return "";
        }

        protected override string CalculateResponder(out string tooltip)
        {
            NetManager instance = Singleton<NetManager>.instance;

            string sText = "";

            tooltip = $"SegmentId: {m_segmentId}\r\nStartNodeId: {m_startNodeId}\r\nEndNodeId: {m_endNodeId}";

            NetSegment segment = Singleton<NetManager>.instance.m_segments.m_buffer[m_segmentId];
            if ((segment.m_flags & NetSegment.Flags.PathFailed) != 0)
            {
                tooltip += "\r\nPath Failed";
            }

            if (m_startBuildingId != 0)
            {
                sText = $"[IN] {InstanceHelper.DescribeInstance(new InstanceID { Building = m_startBuildingId }, false, false)}";
                tooltip = $"[IN] {InstanceHelper.DescribeInstance(new InstanceID { Building = m_startBuildingId }, true, true)}\r\n" + tooltip;
                return sText;
            }

            if (m_endBuildingId != 0)
            {
                sText = $"[OUT] {InstanceHelper.DescribeInstance(new InstanceID { Building = m_endBuildingId }, false, false)}";
                tooltip = $"[OUT] {InstanceHelper.DescribeInstance(new InstanceID { Building = m_endBuildingId }, true, true)}\r\n" + tooltip;
                return sText;
            }

            return $"Segment:{m_segmentId}";
        }

        private ushort GetOutsideConnectionBuildingId()
        {
            if (m_startBuildingId != 0)
            {
                return m_startBuildingId;
            }

            if (m_endBuildingId != 0)
            {
                return m_endBuildingId;
            }

            return 0;
        }

        public int GetWaitTimer()
        {
            NetNode node = NetManager.instance.m_nodes.m_buffer[m_startNodeId];
            if (node.m_flags != 0)
            {
                return node.m_maxWaitTime;
            }
            return 0;
        }

        public override void OnClickResponder()
        {
            // Cycle between nodes
            InstanceID target = InstanceHelper.GetTargetInstance();
            if (target.NetNode != 0)
            {
                if (m_startNodeId == target.NetNode)
                {
                    InstanceHelper.ShowInstance(new InstanceID { NetNode = m_endNodeId });
                    return;
                }
                else if (m_endNodeId == target.NetNode)
                {
                    InstanceHelper.ShowInstance(new InstanceID { NetNode = m_startNodeId });
                    return;
                }
            }

            // Show the start node first so we can see the waiting passengers.
            InstanceHelper.ShowInstance(new InstanceID { NetNode = m_startNodeId });
        }

        protected virtual TransportInfo.TransportType GetTransportType()
        {
            TransportInfo.TransportType eTransportType;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (building.Info.GetSubService())
            {
                case ItemClass.SubService.PublicTransportBus:
                    {
                        eTransportType = TransportInfo.TransportType.Bus;
                        break;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    {
                        eTransportType = TransportInfo.TransportType.Ship;
                        break;
                    }
                case ItemClass.SubService.PublicTransportPlane:
                    {
                        eTransportType = TransportInfo.TransportType.Airplane;
                        break;
                    }
                case ItemClass.SubService.PublicTransportTrain:
                    {
                        eTransportType = TransportInfo.TransportType.Train;
                        break;
                    }
                default:
                    {
                        eTransportType = TransportInfo.TransportType.Train;
                        break;
                    }
            }

            return eTransportType;
        }

        public override string GetMaterialDescription()
        {
            return "Intercity Stop";
        }

        private ushort FindOutsideConnectionBuilding(ushort stop)
        {
            Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
            BuildingManager instance = Singleton<BuildingManager>.instance;

            FastList<ushort> outsideConnections = instance.GetOutsideConnections();
            ushort result = 0;
            float num = 40000f;
            for (int i = 0; i < outsideConnections.m_size; i++)
            {
                ushort num2 = outsideConnections.m_buffer[i];
                float num3 = VectorUtils.LengthSqrXZ(instance.m_buildings.m_buffer[num2].m_position - position);
                if (num3 < num)
                {
                    result = num2;
                    num = num3;
                }
            }

            return result;
        }

        private void UpdateTextColor()
        {
            // Update color
            NetSegment segment = Singleton<NetManager>.instance.m_segments.m_buffer[m_segmentId];
            if ((segment.m_flags & NetSegment.Flags.PathFailed) != 0)
            {
                m_color = KnownColor.orange;
            }
            else
            {
                m_color = KnownColor.lightGrey;
            }
        }
    }
}