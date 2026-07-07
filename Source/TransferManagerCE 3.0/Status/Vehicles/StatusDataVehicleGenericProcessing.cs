using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleGenericProcessing : StatusDataVehicle
    {
        public StatusDataVehicleGenericProcessing(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) :
            base(reason, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (m_material == GetOutgoingTransferReason(building))
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
            if (m_material == GetOutgoingTransferReason(building))
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateDescription2(out tooltip);
            }
        }

        public static CustomTransferReason.Reason GetOutgoingTransferReason(Building building)
        {
            switch (building.Info.m_class.m_subService)
            {
                case ItemClass.SubService.IndustrialForestry:
                    return CustomTransferReason.Reason.Lumber;
                case ItemClass.SubService.IndustrialFarming:
                    return CustomTransferReason.Reason.Food;
                case ItemClass.SubService.IndustrialOil:
                    return CustomTransferReason.Reason.Petrol;
                case ItemClass.SubService.IndustrialOre:
                    return CustomTransferReason.Reason.Coal;
                default:
                    return CustomTransferReason.Reason.Goods;
            }
        }
    }
}