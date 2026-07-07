using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusEvacuationStop : StatusTransportLineStop
    {
        public StatusEvacuationStop(BuildingType eBuildingType, ushort buildingId, ushort LineId, ushort nodeId) :
             base(eBuildingType, buildingId, LineId, nodeId)
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

        public override string GetMaterialDisplay()
        {
            return GetMaterialDescription();
        }

        public override void OnClickDescription1()
        {
            // Disaable opening transport line panel for evacuation lines
        }
    }
}