using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusCableCarStop : StatusNodeStop
    {
        public StatusCableCarStop(BuildingType eBuildingType, ushort buildingId, ushort nodeId, ushort targetVehicleId) :
            base(eBuildingType, buildingId, nodeId, targetVehicleId)
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
    }
}