using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataGeneric : StatusData
    {
        public StatusDataGeneric(ushort buildingId, ushort responderBuilding, ushort targetVehicle)
            : base(TransferReason.None, buildingId, responderBuilding, targetVehicle)
        {
        }

        public override string GetMaterialDescription()
        {
            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
            TransferReason reason = (TransferReason) vehicle.m_transferType;
            return reason.ToString();
        }
    }
}