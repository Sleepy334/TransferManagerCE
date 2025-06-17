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

        public override string GetMaterialDisplay()
        {
            return GetTransportType().ToString();
        }

        public override bool IsBuildingData()
        {
            return false;
        }

        public override bool IsNodeData()
        {
            return true;
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
                tooltip = InstanceHelper.DescribeInstance(new InstanceID { Vehicle = vehicleId }, InstanceID.Empty, true);
                return CitiesUtils.GetVehicleName(vehicleId);
            }

            tooltip = "";
            return "";
        }

        protected override string CalculateResponder(out string tooltip)
        {
            NetManager instance = Singleton<NetManager>.instance;

            string sText = "";
            tooltip = "";

            ushort buildingId = FindConnectionBuilding(m_nodeId);
            if (buildingId != 0)
            {
                sText = InstanceHelper.DescribeInstance(new InstanceID {  Building =  buildingId }, InstanceID.Empty);
                tooltip = InstanceHelper.DescribeInstance(new InstanceID { Building = buildingId }, InstanceID.Empty, true);
            }
            else
            {
                sText = $"Node:{m_nodeId}";
            }

            // Node
            if (tooltip.Length > 0)
            {
                tooltip += " | ";
            }
            tooltip += $"Node:{m_nodeId}";

            // Errors
            int iErrorCount = GetNodePathError(out string sError, out int iCount);
            if (iErrorCount > 0)
            {
                if (tooltip.Length > 0)
                {
                    tooltip += " | ";
                }
                tooltip += $"PathFailed ({iErrorCount}/{iCount} {sError})";
            }

            return sText;
        }

        public override void OnClickResponder()
        {
            if (m_nodeId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { NetNode = m_nodeId });
            }
        }

        private ushort FindConnectionBuilding(ushort stop)
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

        private int GetNodePathError(out string sError, out int iSegmentCount)
        {
            NetManager instance = Singleton<NetManager>.instance;

            int iErrorCount = 0;
            iSegmentCount = 0;
            sError = "Segments:";

            NetNode node = NetManager.instance.m_nodes.m_buffer[m_nodeId];
            if (node.m_flags != 0)
            {

                for (int i = 0; i < 8; i++)
                {
                    ushort segmentId = instance.m_nodes.m_buffer[m_nodeId].GetSegment(i);
                    if (segmentId != 0)
                    {
                        iSegmentCount++;

                        NetSegment segment = instance.m_segments.m_buffer[segmentId];
                        if ((segment.m_flags & NetSegment.Flags.PathFailed) != 0)
                        {
                            sError += $" {segmentId}";
                            iErrorCount++;
                        }
                    }
                }
            }

            return iErrorCount;
        }

        private void UpdateTextColor()
        {
            // Update color
            int iErrorCount = GetNodePathError(out string sError, out int iTotalCount);
            if (iErrorCount > 0)
            {
                if (iErrorCount < iTotalCount)
                {
                    m_color = Color.magenta;
                }
                else
                {
                    m_color = Color.red;
                }
            }
            else
            {
                m_color = KnownColor.lightGrey;
            }
        }
    }
}