using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleGarbage : StatusDataVehicle
    {
        public StatusDataVehicleGarbage(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateVehicle(out string tooltip)
        {
            tooltip = "";

            if (m_material == CustomTransferReason.Reason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateVehicle(out tooltip);
            }
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = "";

            if (m_material == CustomTransferReason.Reason.Goods)
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