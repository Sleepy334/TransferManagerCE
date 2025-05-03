using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataFishFactory : StatusDataProcessingFacility
    {
        public StatusDataFishFactory(TransferReason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }
    }
}