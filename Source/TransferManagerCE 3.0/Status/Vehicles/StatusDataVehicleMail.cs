using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleMail : StatusDataVehicle
    {
        public StatusDataVehicleMail(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) :
            base(reason, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
        }

        public override CustomTransferReason.Reason GetMaterial()
        {
            switch (m_material)
            {
                // IncomingMail and OutgoingMail are both actually SortedMail
                case CustomTransferReason.Reason.IncomingMail:
                case CustomTransferReason.Reason.OutgoingMail:
                    return CustomTransferReason.Reason.SortedMail;

                default:
                    return base.GetMaterial();
            }
        }
    }
}