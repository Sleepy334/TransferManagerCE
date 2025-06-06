using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    // --------------------------------------------------------------------------------------------
    public class StatusDataVehicleCommercial : StatusDataVehicle
    {
        public StatusDataVehicleCommercial(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateVehicle(out string tooltip)
        {
            tooltip = "";

            bool bIncoming = m_material == CustomTransferReason.Reason.Goods || m_material == CustomTransferReason.Reason.Food;
            if (bIncoming)
            {
                return base.CalculateVehicle(out tooltip);
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = "";

            bool bIncoming = m_material == CustomTransferReason.Reason.Goods || m_material == CustomTransferReason.Reason.Food;
            if (bIncoming)
            {
                return base.CalculateResponder(out tooltip);
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }
    }
}