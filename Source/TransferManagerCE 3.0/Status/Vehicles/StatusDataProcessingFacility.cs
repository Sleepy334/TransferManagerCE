using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleProcessingFacility : StatusDataVehicle
    {
        public StatusDataVehicleProcessingFacility(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateVehicle(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null && m_material == (CustomTransferReason.Reason) buildingAI.m_outputResource)
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateVehicle(out tooltip);
            }
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null && m_material == (CustomTransferReason.Reason) buildingAI.m_outputResource)
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateResponder(out tooltip);
            }
        }
    }
}