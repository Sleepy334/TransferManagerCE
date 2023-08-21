using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataWarehouse : StatusData
    {
        public StatusDataWarehouse(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
            if (warehouseAI is not null)
            {
                return (building.m_customBuffer1 * 0.1).ToString("N0") + "/" + (warehouseAI.m_storageCapacity * 0.001).ToString("N0");
            }
            else
            {
                return (building.m_customBuffer1 * 0.1).ToString("N0");
            }
        }
    }
}