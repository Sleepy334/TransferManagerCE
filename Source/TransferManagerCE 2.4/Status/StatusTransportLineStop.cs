using static RenderManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusTransportLineStop : StatusNodeStop
    {
        private ushort m_lineId;

        public StatusTransportLineStop(BuildingType eBuildingType, ushort buildingId, ushort LineId, ushort nodeId, ushort targetVehicleId) :
            base(eBuildingType, buildingId, nodeId, targetVehicleId)
        {
            m_lineId = LineId;
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

        public override string GetMaterialDescription()
        {
            TransportLine line = TransportManager.instance.m_lines.m_buffer[m_lineId];
            return line.Info.m_transportType.ToString();
        }

        public override string GetResponder()
        {
            return CitiesUtils.GetSafeLineName(m_lineId);
        }

        public override void OnClickResponder()
        {
            if (m_nodeId != 0)
            {
                // Select node
                InstanceID node = new InstanceID { NetNode = m_nodeId };
                InstanceHelper.ShowInstance(node);

                // Show line details panel.
                InstanceID line = new InstanceID { TransportLine = m_lineId };
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(InstanceHelper.GetPosition(node), line);
            }
        }
        
    }
}