using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusIntercityStop : StatusNodeStop
    {
        public StatusIntercityStop(BuildingType eBuildingType, ushort buildingId, ushort nodeId, ushort targetVehicleId) :
            base(eBuildingType, buildingId, nodeId, targetVehicleId)
        {
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
            return "Intercity Stop";
        }
    }
}