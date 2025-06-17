namespace TransferManagerCE
{
    public class TransportUtils
    {
        public enum TransportType
        {
            None,
            Road,
            Plane,
            Train,
            Ship,
        }

        // -------------------------------------------------------------------------------------------
        public static TransportType GetTransportType(Building building)
        {
            if (building.Info is not null)
            {
                switch (building.Info.GetSubService())
                {
                    case ItemClass.SubService.PublicTransportPlane:
                        {
                            return TransportType.Plane;
                        }
                    case ItemClass.SubService.PublicTransportShip:
                        {
                            return TransportType.Ship;
                        }
                    case ItemClass.SubService.PublicTransportTrain:
                        {
                            return TransportType.Train;
                        }
                }
            }

            return TransportType.Road;
        }

        // -------------------------------------------------------------------------------------------
        public static TransportType GetTransportType(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                return TransportUtils.GetTransportType(building);
            }

            return TransportType.None;
        }
    }
}