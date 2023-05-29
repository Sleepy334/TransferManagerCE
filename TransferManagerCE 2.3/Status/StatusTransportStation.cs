using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusTransportStation : StatusData
    {
        public StatusTransportStation(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(TransferReason.None, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Passengers";
        }

        protected override string CalculateValue()
        {
            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                int current = 0;

                switch (building.Info.GetAI())
                {
                    case HarborAI harborAI:
                        {
                            current = harborAI.GetPassengerCount(m_buildingId, ref building);
                            break;
                        }
                    case TransportStationAI stationAI:
                        {
                            current = stationAI.GetPassengerCount(m_buildingId, ref building);
                            break;
                        }
                }

                return current.ToString();
            }

            return "";
        }

        public override string GetResponder()
        {
            return "";
        }

        public override string GetTarget()
        {
            return "";
        }
    }
}