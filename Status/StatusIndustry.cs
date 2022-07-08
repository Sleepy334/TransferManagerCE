using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataIndustry : StatusData
    {
        public StatusDataIndustry(TransferReason reason, ushort BuildingId, ushort responder, ushort target) :
            base(reason, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
            return (vehicle.m_transferSize * 0.001).ToString("N0");
        }
    }
}