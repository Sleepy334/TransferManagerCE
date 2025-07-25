using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataFire2 : StatusDataBuilding
    {
        public StatusDataFire2(BuildingType eBuildingType, ushort BuildingId) : 
            base(CustomTransferReason.Reason.Fire2, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Intensity | Damage";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                WarnText(false, true, building.m_fireIntensity, 1);
                return $"{building.m_fireIntensity} | {building.GetLastFrameData().m_fireDamage}";
            }

            return "0";
        }
    }
}