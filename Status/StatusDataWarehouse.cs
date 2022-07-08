using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataWarehouse : StatusData
    {
        public StatusDataWarehouse(TransferReason reason, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            WarehouseAI? warehouseAI = building.Info?.m_buildingAI as WarehouseAI;
            if (warehouseAI != null)
            {
                return (building.m_customBuffer1 * 0.1).ToString("N0") + "/" + (warehouseAI.m_storageCapacity * 0.001).ToString("N0");
            }
            else
            {
                return (building.m_customBuffer1 * 0.1).ToString("N0");
            }
        }

        public override string GetTarget()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                return CitiesUtils.GetVehicleName(m_targetVehicle) + " (" + (vehicle.m_transferSize * 0.001).ToString("N0") + ")";
            }

            return "None";
        }
    }
}