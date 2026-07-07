using System;
using ColossalFramework;
using SleepyCommon;
using static RenderManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusTransportLineStop : StatusNodeStop
    {
        private ushort m_lineId;
        private int m_stopNumber;

        public StatusTransportLineStop(BuildingType eBuildingType, ushort buildingId, ushort LineId, ushort nodeId) :
            base(eBuildingType, buildingId, nodeId)
        {
            m_lineId = LineId;
            m_stopNumber = TransportUtils.GetStopNumber(m_lineId, m_nodeId);
        }

        public override int CompareTo(object second)
        {
            if (second is StatusTransportLineStop oSecond)
            {
                // Sort by stop number
                if (oSecond.m_stopNumber != m_stopNumber)
                {
                    return m_stopNumber - oSecond.m_stopNumber;
                }
            }

            return base.CompareTo(second);
        }

        protected override TransportInfo.TransportType GetTransportType()
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

        public override string GetMaterialDisplay()
        {
            return GetMaterialDescription();
        }

        public override string GetMaterialDescription()
        {
            TransportLine line = TransportManager.instance.m_lines.m_buffer[m_lineId];
            return line.Info.m_transportType.ToString();
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
        }

        protected override string CalculateTimer(out string tooltip)
        {
            int iWaitTimer = GetWaitTimer();

            tooltip = $"Wait Timer: {iWaitTimer}";

            if (iWaitTimer > 0)
            {
                return $"W:{iWaitTimer}";
            }
            else
            {
                return "";
            }
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = CitiesUtils.GetSafeLineName(m_lineId);
            return tooltip;
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            if (m_lineId != 0 && m_stopNumber != 0)
            {
                tooltip = $"Stop: {m_stopNumber} | {InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_nodeId }, true, false)}";
                return $"Stop: {m_stopNumber}";
            }

            tooltip = InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_nodeId }, true, false);
            return InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_nodeId }, false, false);
        }

        public override void OnClickDescription1()
        {
            if (m_nodeId != 0)
            {
                // Show line details panel.
                InstanceID node = new InstanceID { NetNode = m_nodeId };
                InstanceID line = new InstanceID { TransportLine = m_lineId };
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(InstanceHelper.GetPosition(node), line);
            }
        }

        public override void OnClickDescription2()
        {
            if (m_nodeId != 0)
            {
                // Select node
                InstanceID node = new InstanceID { NetNode = m_nodeId };
                InstanceHelper.ShowInstance(node);
            }
        }
    }
}