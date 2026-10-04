using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

using SleepyCommon;
namespace TransferManagerCore.Data
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
                        return GetLocalizedLabel("status_Visitors", "Visitors");
                    }
                default:
                    {
                        return CustomTransferReason.GetLocalizedReason(GetMaterial());
                    }

            }
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = Localization.Get("tip_CurrentVisitorsTotalVisitorPlaces");

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
