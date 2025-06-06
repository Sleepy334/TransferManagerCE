using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleMail : StatusDataVehicle
    {
        public StatusDataVehicleMail(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
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