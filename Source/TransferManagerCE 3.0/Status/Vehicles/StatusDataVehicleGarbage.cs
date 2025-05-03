using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleGarbage : StatusDataVehicle
    {
        public StatusDataVehicleGarbage(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateTarget(out string tooltip)
        {
            tooltip = "";

            if (m_material == TransferReason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateTarget(out tooltip);
            }
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = "";

            if (m_material == TransferReason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateResponder(out tooltip);
            }
        }
    }
}