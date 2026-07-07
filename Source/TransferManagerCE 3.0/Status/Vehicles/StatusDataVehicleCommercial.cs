using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    // --------------------------------------------------------------------------------------------
    public class StatusDataVehicleCommercial : StatusDataVehicle
    {
        public StatusDataVehicleCommercial(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) :
            base(reason, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = "";

            bool bIncoming = m_material == CustomTransferReason.Reason.Goods || m_material == CustomTransferReason.Reason.Food;
            if (bIncoming)
            {
                return base.CalculateDescription1(out tooltip);
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = "";

            bool bIncoming = m_material == CustomTransferReason.Reason.Goods || m_material == CustomTransferReason.Reason.Food;
            if (bIncoming)
            {
                return base.CalculateDescription2(out tooltip);
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }
    }
}