using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataFire : StatusDataFireBase
    {
        public StatusDataFire(BuildingType eBuildingType, ushort BuildingId) : 
            base(CustomTransferReason.Reason.Fire, eBuildingType, BuildingId)
        {
        }
    }
}