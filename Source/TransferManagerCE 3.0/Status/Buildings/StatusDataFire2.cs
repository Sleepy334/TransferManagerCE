using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataFire2 : StatusDataFireBase
    {
        public StatusDataFire2(BuildingType eBuildingType, ushort BuildingId) : 
            base(CustomTransferReason.Reason.Fire2, eBuildingType, BuildingId)
        {
        }
    }
}