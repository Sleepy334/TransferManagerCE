using System;
using System.Collections.Generic;
using System.Reflection;
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

            if (building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case WarehouseAI warehouseAI:
                        {
                            return (building.m_customBuffer1 * 0.1).ToString("N0") + "/" + (warehouseAI.m_storageCapacity * 0.001).ToString("N0");
                        }
                    case CargoStationAI cargoStationAI:
                        {
                            // CargoFerryWarehouseHarborAI also has a built in warehouse
                            Type buildingType = cargoStationAI.GetType();
                            FieldInfo? storageCapacity = buildingType.GetField("m_storageCapacity");
                            if (storageCapacity is not null)
                            {
                                return (building.m_customBuffer1 * 0.1).ToString("N0") + "/" + ((int)storageCapacity.GetValue(cargoStationAI) * 0.001).ToString("N0");
                            }
                            break;
                        }
                }
            }

            return (building.m_customBuffer1 * 0.1).ToString("N0");
        }
    }
}