using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    // --------------------------------------------------------------------------------------------
    public class StatusDataVehicleCommercial : StatusDataVehicle
    {
        public StatusDataVehicleCommercial(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateTarget(out string tooltip)
        {
            tooltip = "";

            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;
            if (bIncoming)
            {
                return base.CalculateTarget(out tooltip);
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = "";

            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;
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