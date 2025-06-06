using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVisitors : StatusDataBuilding
    {
        public StatusDataVisitors(BuildingType eBuildingType, ushort BuildingId) :
            base(CustomTransferReason.Reason.None, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            switch ((TransferReason)GetMaterial())
            {
                case TransferReason.None:
                    {
                        return "Visitors";
                    }
                default:
                    {
                        return GetMaterial().ToString();
                    }

            }
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Current Visitors / Total Visitor Places";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                int iTotalPlaces = BuildingUtils.GetTotalVisitPlaceCount(m_buildingId, building);
                if (iTotalPlaces > 0)
                {
                    return $"{BuildingUtils.GetVisitorCount(building.Info.GetAI() as CommonBuildingAI, m_buildingId, building)} / {iTotalPlaces}";
                }
                else
                {
                    return $"{BuildingUtils.GetVisitorCount(building.Info.GetAI() as CommonBuildingAI, m_buildingId, building)}";
                }
            }

            return $"";
        }
    }
}