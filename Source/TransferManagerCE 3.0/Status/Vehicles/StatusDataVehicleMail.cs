using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleMail : StatusDataVehicle
    {
        public StatusDataVehicleMail(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override CustomTransferReason GetMaterial()
        {
            switch (m_material)
            {
                // IncomingMail and OutgoingMail are both actually SortedMail
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                    return TransferReason.SortedMail;

                default:
                    return base.GetMaterial();
            }
        }
    }
}