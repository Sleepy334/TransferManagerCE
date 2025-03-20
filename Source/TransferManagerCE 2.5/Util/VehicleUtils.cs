using static TransferManager;

namespace TransferManagerCE
{
    public static class VehicleUtils
    {
        public static bool IsAmbulance(ushort vehicleId)
        {
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];

                // Check if vehicle transfer type is Sick or Sick2
                switch ((TransferReason)vehicle.m_transferType)
                {
                    case TransferReason.Sick:
                    case TransferReason.Sick2:
                        {
                            // Ambulance
                            return true;
                        }
                }
            }

            return false;
        }
    }
}