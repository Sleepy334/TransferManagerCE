using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataUniqueFactory : StatusDataProcessingFacility
    {
        public StatusDataUniqueFactory(TransferReason reason, ushort BuildingId, ushort responder, ushort target) :
            base(reason, BuildingId, responder, target)
        {
        }
    }
}