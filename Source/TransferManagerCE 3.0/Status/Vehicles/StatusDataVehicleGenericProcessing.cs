using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleGenericProcessing : StatusDataVehicle
    {
        public StatusDataVehicleGenericProcessing(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateTarget(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (m_material == GetOutgoingTransferReason(building))
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateTarget(out tooltip);
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

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            switch (building.Info.m_class.m_subService)
            {
                case ItemClass.SubService.IndustrialForestry:
                    return TransferManager.TransferReason.Lumber;
                case ItemClass.SubService.IndustrialFarming:
                    return TransferManager.TransferReason.Food;
                case ItemClass.SubService.IndustrialOil:
                    return TransferManager.TransferReason.Petrol;
                case ItemClass.SubService.IndustrialOre:
                    return TransferManager.TransferReason.Coal;
                default:
                    return TransferManager.TransferReason.Goods;
            }
        }
    }
}