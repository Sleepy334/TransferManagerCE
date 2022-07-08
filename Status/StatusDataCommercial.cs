using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataCommercial : StatusData
    {
        public StatusDataCommercial(ushort BuildingId, ushort responder, ushort target) :
            base(TransferReason.Goods, BuildingId, responder, target)
        {
        }

        public override string GetMaterialDescription()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                return ((TransferReason)vehicle.m_transferType).ToString();
            }
            else
            {
                return TransferReason.Goods.ToString();
            }
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return (building.m_customBuffer1 * 0.001).ToString("N0");
        }

        public override string GetTimer()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_incomingProblemTimer.ToString();
        }

        public override string GetTarget()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                return CitiesUtils.GetVehicleName(m_targetVehicle) + " (" + vehicle.m_transferSize * 0.001 + ")";
            }

            return "None";
        }
    }
}