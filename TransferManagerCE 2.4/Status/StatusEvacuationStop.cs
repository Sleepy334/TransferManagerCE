using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusEvacuationStop : StatusNodeStop
    {
        public StatusEvacuationStop(BuildingType eBuildingType, ushort buildingId, ushort nodeId) :
            base(eBuildingType, buildingId, nodeId)
        {
        }

        protected override TransportInfo.TransportType GetTransportType()
        {
            return TransportInfo.TransportType.Bus;
        }

        public override string GetMaterialDescription()
        {
            return "Evacuation Stop";
        }
    }
}