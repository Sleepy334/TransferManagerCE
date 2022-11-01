using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataGeneric : StatusData
    {
        public StatusDataGeneric(TransferReason material, BuildingType eBuildingType, ushort buildingId, ushort responderBuilding, ushort targetVehicle)
            : base(material, eBuildingType, buildingId, responderBuilding, targetVehicle)
        {
        }
    }
}