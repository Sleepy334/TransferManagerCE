using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataUniqueFactory : StatusDataProcessingFacility
    {
        public StatusDataUniqueFactory(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }
    }
}