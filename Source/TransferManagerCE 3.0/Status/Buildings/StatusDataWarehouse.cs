using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataWarehouse : StatusDataBuilding
    {
        public StatusDataWarehouse(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) : 
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info is not null)
            {
                
                int iStorageCapacity = 0;

                switch (building.Info.GetAI())
                {
                    case WarehouseAI warehouseAI:
                        {
                            iStorageCapacity = warehouseAI.m_storageCapacity;
                            break;
                        }
                    case CargoStationAI cargoStationAI:
                        {
                            // CargoFerryWarehouseHarborAI also has a built in warehouse
                            FieldInfo? storageCapacity = cargoStationAI.GetType().GetField("m_storageCapacity");
                            if (storageCapacity is not null)
                            {
                                iStorageCapacity = (int)storageCapacity.GetValue(cargoStationAI);
                            }
                            break;
                        }
                }

                int iCurrentBuffer = building.m_customBuffer1 * 100;
                if (iStorageCapacity > 0)
                {
                    WarnText(true, true, iCurrentBuffer, iStorageCapacity);
                    tooltip = $"{GetMaterialDescription()}: {DisplayBuffer(iCurrentBuffer)}/{DisplayBuffer(iStorageCapacity)}";
                    return Utils.MakePercent(iCurrentBuffer, iStorageCapacity);
                }
                else
                {
                    tooltip = "";
                    return DisplayBuffer(iCurrentBuffer);
                }
            }

            tooltip = "";
            return "";
        }
    }
}