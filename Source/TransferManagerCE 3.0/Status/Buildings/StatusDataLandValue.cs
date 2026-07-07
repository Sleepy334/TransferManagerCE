using ColossalFramework;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataLandValue : StatusDataBuilding
    {
        public StatusDataLandValue(BuildingType eBuildingType, ushort BuildingId) :
            base(CustomTransferReason.Reason.None, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Land Value";
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.LandValue, building.m_position, out var local);

                tooltip = $"Land Value: {local}";
                return $"{local}";
            }

            tooltip = "";
            return "";
        }
    }
}