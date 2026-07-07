using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataImport : StatusDataBuilding
    {
        public StatusDataImport(BuildingType eBuildingType, ushort BuildingId) :
            base(CustomTransferReason.Reason.None, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Import";
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Amount Imported";

            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    return $"{building.m_tempImport + building.m_finalImport}";
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