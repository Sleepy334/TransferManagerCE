using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusCableCarStop : StatusSegmentStop
    {
        public StatusCableCarStop(BuildingType eBuildingType, ushort buildingId, ushort segmentId, ushort startNodeId, ushort endNodeId) :
            base(eBuildingType, buildingId, segmentId, startNodeId, endNodeId)
        {
        }

        protected override TransportInfo.TransportType GetTransportType()
        {
            return TransportInfo.TransportType.CableCar;
        }

        public override string GetMaterialDescription()
        {
            return "CableCar Stop";
        }

        public override string GetMaterialDisplay()
        {
            return "CableCar Stop";
        }
    }
}