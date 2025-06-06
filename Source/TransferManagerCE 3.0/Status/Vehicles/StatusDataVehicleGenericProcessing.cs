using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleGenericProcessing : StatusDataVehicle
    {
        public StatusDataVehicleGenericProcessing(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateVehicle(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (m_material == GetOutgoingTransferReason(building))
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
            if (m_material == GetOutgoingTransferReason(building))
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateResponder(out tooltip);
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