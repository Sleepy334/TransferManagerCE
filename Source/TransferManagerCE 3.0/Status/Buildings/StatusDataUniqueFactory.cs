using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataUniqueFactory : StatusDataProcessingFacility
    {
        public StatusDataUniqueFactory(TransferReason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }
    }
}