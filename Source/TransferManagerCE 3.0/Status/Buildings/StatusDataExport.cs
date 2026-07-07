using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataExport : StatusDataBuilding
    {
        public StatusDataExport(BuildingType eBuildingType, ushort BuildingId) :
            base(CustomTransferReason.Reason.None, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Export";
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Amount Exported";

            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    return $"{building.m_tempExport + building.m_finalExport}";
                }
            }

            return "0";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = ""; 
            return ""; // No vehicles
        }

        protected override string CalculateDescription2(out string tooltip)
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