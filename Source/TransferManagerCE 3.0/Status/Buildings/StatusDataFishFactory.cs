using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataFishFactory : StatusDataProcessingFacility
    {
        public StatusDataFishFactory(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }
    }
}