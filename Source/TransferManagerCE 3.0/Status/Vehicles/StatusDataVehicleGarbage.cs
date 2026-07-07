using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleGarbage : StatusDataVehicle
    {
        public StatusDataVehicleGarbage(CustomTransferReason.Reason reason, ushort vehicleId, BuildingType eBuildingType, ushort BuildingId, ushort sourceBuildingId, InstanceID target) :
            base(reason, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = "";

            if (m_material == CustomTransferReason.Reason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateDescription1(out tooltip);
            }
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = "";

            if (m_material == CustomTransferReason.Reason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateDescription2(out tooltip);
            }
        }
    }
}