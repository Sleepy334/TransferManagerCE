using ColossalFramework;
using SleepyCommon;
using static TransferManagerCore.BuildingTypeHelper;

// ----------------------------------------------------------------------------------------
namespace TransferManagerCore.Data
{
    // ----------------------------------------------------------------------------------------
    public abstract class StatusSegmentStop : StatusData
    {
        public ushort m_startNodeId;
        public ushort m_endNodeId;

        public ushort m_startBuildingId;
        public ushort m_endBuildingId;

        protected ushort m_segmentId;
        private bool m_bPathFailed;

        // ----------------------------------------------------------------------------------------
        public StatusSegmentStop(BuildingType eBuildingType, ushort buildingId, ushort segmentId, ushort startNodeId, ushort endNodeId) :
            base(CustomTransferReason.Reason.None, eBuildingType, buildingId)
        {
            m_segmentId = segmentId;
            m_startNodeId = startNodeId;
            m_endNodeId = endNodeId;
            m_startBuildingId = FindBuilding(m_startNodeId);
            m_endBuildingId = FindBuilding(m_endNodeId);
            m_bPathFailed = IsPathFailed();
        }

        protected abstract TransportInfo.TransportType GetTransportType();

        public override int CompareTo(object second)
        {
            if (second is StatusSegmentStop oSecond)
            {
                // Sort path failed lines to bottom
                if (m_bPathFailed != oSecond.m_bPathFailed)
                {
                    return m_bPathFailed.CompareTo(oSecond.m_bPathFailed);
                }

                // Start node
                if (oSecond.m_startNodeId != m_startNodeId)
                {
                    return oSecond.m_startNodeId.CompareTo(m_startNodeId);
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
            return true;
        }

        public override bool IsVehicleData()
        {
            return false;
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
            string sTimer = "";
            tooltip = "";

            int iWaitTimer = GetWaitTimer();
            if (iWaitTimer > 0)
            {
                tooltip = $"Wait Timer: {iWaitTimer}";
                sTimer = $"W:{iWaitTimer}";
            }

            return sTimer;
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = GetTooltipImpl();

            string sText = "";
            if (m_startBuildingId != 0)
            {
                sText = $"{InstanceHelper.DescribeInstance(new InstanceID { Building = m_startBuildingId }, false, false)}";
            }
            else if (m_startNodeId != 0)
            {
                sText = $"{InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_startNodeId }, false, false)}";
            }

            return sText;
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = GetTooltipImpl();

            string sText = "";
            if (m_endBuildingId != 0)
            {
                sText = $"{InstanceHelper.DescribeInstance(new InstanceID { Building = m_endBuildingId }, false, false)}";
            }
            else if (m_endNodeId != 0)
            {
                sText = $"{InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_endNodeId }, false, false)}";
            }

            return sText;
        }

        public string GetTooltipImpl()
        {
            InstanceID startNode = new InstanceID { NetNode = m_startNodeId };
            InstanceID endNode = new InstanceID { NetNode = m_endNodeId };

            InstanceID startBuilding = new InstanceID { Building = m_startBuildingId };
            InstanceID endBuilding = new InstanceID { Building = m_endBuildingId };

            // Tooltip
            string tooltip = "";
            tooltip += $"{InstanceHelper.DescribeInstance(startNode, false, true)} | {InstanceHelper.DescribeInstance(startBuilding, false, true)}\r\n";
            tooltip += $"{InstanceHelper.DescribeInstance(endNode, false, true)} | {InstanceHelper.DescribeInstance(endBuilding, false, true)}\r\n";
            tooltip += $"Segment: #{m_segmentId}";

            if (m_bPathFailed)
            {
                tooltip += " [Path Failed]";
            }

            return tooltip;
        }

        private bool IsPathFailed()
        {
            NetSegment segment = Singleton<NetManager>.instance.m_segments.m_buffer[m_segmentId];
            return ((segment.m_flags & NetSegment.Flags.PathFailed) != 0);
        }

        private ushort GetOutsideConnectionBuildingId()
        {
            if (m_startBuildingId != 0 && BuildingTypeHelper.IsOutsideConnection(m_startBuildingId))
            {
                return m_startBuildingId;
            }

            if (m_endBuildingId != 0 && BuildingTypeHelper.IsOutsideConnection(m_endBuildingId))
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

        public override void OnClickDescription1()
        {
            InstanceHelper.ShowInstance(new InstanceID { NetNode = m_startNodeId });
        }

        public override void OnClickDescription2()
        {
            InstanceHelper.ShowInstance(new InstanceID { NetNode = m_endNodeId });
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
                m_color = KnownColor.white;
            }
        }
    }
}