using ColossalFramework;
using static TransferManager;

namespace TransferManagerCE
{
    public class VehicleUtils
    {
        public static TransferReason GetTransferType(Vehicle vehicle)
        {
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            uint uiSize = VehicleManager.instance.m_vehicles.m_size;

            TransferReason reason = (TransferReason)vehicle.m_transferType;
            if (reason == TransferReason.None)
            {
                ushort nextVehicleId = vehicle.m_firstCargo;
                int iLoopCounter = 0;
                while (nextVehicleId != 0 && nextVehicleId < uiSize)
                {
                    vehicle = Vehicles[nextVehicleId];

                    // Call delegate for this vehicle
                    TransferReason reason2 = (TransferReason)vehicle.m_transferType;
                    if (reason2 != TransferReason.None)
                    {
                        if (reason == TransferReason.None)
                        {
                            reason = reason2;
                        }
                        else if (reason != reason2)
                        {
                            // Different types
                            return TransferReason.None;
                        }
                    }

                    // Next cargo
                    nextVehicleId = vehicle.m_nextCargo;

                    if (++iLoopCounter > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
            }

            return reason;
        }

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