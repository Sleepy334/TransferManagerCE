using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleProcessingFacility : StatusDataVehicle
    {
        public StatusDataVehicleProcessingFacility(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) :
            base(reason, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
        }

        protected override string CalculateDescription1(out string tooltip)
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
                return base.CalculateDescription1(out tooltip);
            }
        }

        protected override string CalculateDescription2(out string tooltip)
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
                return base.CalculateDescription2(out tooltip);
            }
        }
    }
}