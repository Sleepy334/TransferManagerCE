using SleepyCommon;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataFireBase : StatusDataBuilding
    {
        public StatusDataFireBase(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Intensity | Damage";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                WarnText(false, true, building.m_fireIntensity, 1);
                return $"{Utils.MakePercent(building.m_fireIntensity, 255, 0)} | {Utils.MakePercent(building.GetLastFrameData().m_fireDamage, 255, 0)}";
            }
            return "0";
        }
    }
}