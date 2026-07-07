using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public abstract class StatusDataGenericIndustry : StatusDataBuilding
    {
        // --------------------------------------------------------------------
        public StatusDataGenericIndustry(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        // --------------------------------------------------------------------
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
