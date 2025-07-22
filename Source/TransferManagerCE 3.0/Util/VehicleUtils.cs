using ColossalFramework;
using SleepyCommon;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    public class VehicleUtils
    {
        public static CustomTransferReason.Reason GetTransferType(Vehicle vehicle)
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
                            return CustomTransferReason.Reason.None;
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

            return (CustomTransferReason) reason;
        }

        public static string GetVehicleTooltip(ushort vehicleId)
        {
            string sText = string.Empty;

            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            uint uiSize = VehicleManager.instance.m_vehicles.m_size;
            Vehicle vehicle = Vehicles[vehicleId];

            // Add reason if not None
            CustomTransferReason.Reason reason = GetTransferType(vehicle);
            if (reason != CustomTransferReason.Reason.None)
            {
                sText += $"{reason.ToString()} | ";
            }

            sText += $"{InstanceHelper.DescribeInstance(new InstanceID { Vehicle = vehicleId }, true, true)}";
            sText += $" ({CitiesUtils.GetVehicleTransferValue(vehicleId, out int current, out int max)})";

            // Add on target if available
            InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleId, vehicle);
            if (target != InstanceID.Empty)
            {
                sText += $" | {InstanceHelper.DescribeInstance(target, true, true)}";
            }

            List<string> materials = new List<string>();
            List<string> vehicles = new List<string>();
            List<string> targets = new List<string>();

            ushort nextVehicleId = vehicle.m_firstCargo;
            int iLoopCounter = 0;
            while (nextVehicleId != 0 && nextVehicleId < uiSize)
            {
                vehicle = Vehicles[nextVehicleId];

                // Add vehicle to tooltip
                materials.Add(((CustomTransferReason)vehicle.m_transferType).ToString());
                vehicles.Add(InstanceHelper.DescribeInstance(new InstanceID { Vehicle = nextVehicleId }, true, true));

                // Add on target if available and different to parent
                InstanceID cargoTarget = VehicleTypeHelper.GetVehicleTarget(nextVehicleId, vehicle);
                if (cargoTarget != InstanceID.Empty)
                {
                    targets.Add(InstanceHelper.DescribeInstance(cargoTarget, true, true));
                }
                else
                {
                    targets.Add(string.Empty);
                }

                // Next cargo
                nextVehicleId = vehicle.m_nextCargo;
                if (++iLoopCounter > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }

            if (materials.Count > 0)
            {
                sText += $"\r\n";

                materials = SleepyCommon.Utils.PadToWidth(materials, false);
                vehicles = SleepyCommon.Utils.PadToWidth(vehicles, false);
                targets = SleepyCommon.Utils.PadToWidth(targets, false);

                for (int i = 0; i < materials.Count; ++i)
                {
                    sText += $"\r\n{materials[i]} | {vehicles[i]} | {targets[i]}";
                }
            }

            return sText;
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

        public static bool IsPostTruck(Vehicle data)
        {
            // Post trucks are level5
            return data.Info is not null && data.Info.GetClassLevel() == ItemClass.Level.Level5;
        }
    }
}