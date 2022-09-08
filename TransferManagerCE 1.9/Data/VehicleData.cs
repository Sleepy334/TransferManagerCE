using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.Util;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class VehicleData : IComparable
    {
        public ushort m_vehicleId;
        public Vehicle m_vehicle;

        public VehicleData(ushort vehicleId)
        {
            m_vehicleId = vehicleId;
            m_vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            VehicleData oSecond = (VehicleData)second;
            return m_vehicle.m_transferType.CompareTo(oSecond.m_vehicle.m_transferType);
        }

        public virtual string GetTarget()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleId, vehicle);
                    return VehicleTypeHelper.DescribeVehicleTarget(vehicle, target);
                }
            }

            return Localization.Get("txtVehiclesNone");
        }

        public ushort GetVehicleId()
        {
            if (m_vehicleId != 0)
            {
                if (m_vehicle.m_cargoParent != 0)
                {
                    return m_vehicle.m_cargoParent;
                }
            }
            return m_vehicleId;
        }

        public virtual string GetVehicle()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                string sName = CitiesUtils.GetVehicleName(vehicleId);
                if (string.IsNullOrEmpty(sName))
                {
                    sName = "Vehicle:" + m_vehicleId;
                }
                if ((m_vehicle.m_flags & Vehicle.Flags.WaitingLoading) == Vehicle.Flags.WaitingLoading ||
                    (m_vehicle.m_flags & Vehicle.Flags.WaitingCargo) == Vehicle.Flags.WaitingCargo)
                {
                    sName += " (Loading)";
                }
                return sName;
            }

            return Localization.Get("txtVehiclesNone");
        }

        public string GetMaterialDescription()
        {
            return ((TransferReason)m_vehicle.m_transferType).ToString();
        }

        public virtual string GetValue()
        {
            return CitiesUtils.GetVehicleTransferValue(GetVehicleId());
        }

        public virtual string GetTimer()
        {
            string sTimer = "";
            
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                if (m_vehicle.m_waitCounter > 0)
                {
                    sTimer += "W:" + m_vehicle.m_waitCounter + " ";
                }
                if (m_vehicle.m_blockCounter > 0)
                {
                    sTimer += "B:" + m_vehicle.m_blockCounter;
                }
            }

            return sTimer;
        }
    }
}
