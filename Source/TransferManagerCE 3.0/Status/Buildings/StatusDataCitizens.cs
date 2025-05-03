using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCitizens : StatusDataBuilding
    {
        public StatusDataCitizens(TransferReason material, BuildingType eBuildingType, ushort BuildingId) :
            base(material, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Citizens";
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Citizens in building | Total citizens allocated to building";

            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    int iCitizenCount = BuildingUtils.GetCitizenCount(m_buildingId, building, out int iInBuildingCount);
                    return $"{iInBuildingCount} | {iCitizenCount}";
                }
            }

            return "0";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateTarget(out string tooltip)
        {
            tooltip = ""; 
            return ""; // No vehicles
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = "";
            return ""; // No vehicles
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            return TransferReason.None;
        }
    }
}