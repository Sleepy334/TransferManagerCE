using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataFire2 : StatusData
    {
        public StatusDataFire2(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Fire2, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                return $"{building.m_fireIntensity} | {building.GetLastFrameData().m_fireDamage}";
            }
            return "0";
        }

        public override string GetValueTooltip()
        {
            return "Intensity | Damage";
        }
    }
}