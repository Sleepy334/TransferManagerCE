using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TransferManagerCE.Common;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class VehicleData : IComparable
    {
        public ushort m_vehicleId;
        public Vehicle m_vehicle;
        private string m_name;
        private string m_value;
        private string m_distance;
        private string m_target;

        public VehicleData(ushort vehicleId)
        {
            m_vehicleId = vehicleId;
            m_vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
            m_name = GetVehicleName();
            m_value = CitiesUtils.GetVehicleTransferValue(GetVehicleId());
            m_distance = CalculateDistance();
            m_target = CalculateTarget();
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

        public virtual string GetMaterialDescription()
        {
            return ((CustomTransferReason)m_vehicle.m_transferType).ToString();
        }

        public virtual string GetTarget()
        {
            return m_target;
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
            return m_name;
        }

        public virtual string GetValue()
        {
            return m_value;
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

        public virtual string GetDistance()
        {
            return m_distance;
        }

        private string GetVehicleName()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                string sName = CitiesUtils.GetVehicleName(vehicleId);
                if (string.IsNullOrEmpty(sName))
                {
                    sName = "Vehicle:" + m_vehicleId;
                }
                return sName;
            }

            return Localization.Get("txtVehiclesNone");
        }

        private string CalculateTarget()
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

        private string CalculateDistance()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0 && vehicle.m_sourceBuilding != 0)
                {
                    Vector3 buildingPos = Vector3.zero;

                    // Show distance to target if any, else use source
                    InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleId, vehicle);
                    if (target.Building != 0)
                    {
                        buildingPos = InstanceHelper.GetPosition(target);
                    }
                    else if (vehicle.m_sourceBuilding != 0)
                    {
                        InstanceID sourceBuilding = new InstanceID { Building = vehicle.m_sourceBuilding };
                        buildingPos = InstanceHelper.GetPosition(sourceBuilding);
                    }
                        
                    if (buildingPos != Vector3.zero)
                    {
                        Vector3 vehiclePos = vehicle.GetLastFramePosition();
                        return (Math.Sqrt(Vector3.SqrMagnitude(vehiclePos - buildingPos)) * 0.001).ToString("0.00");
                    }
                }
            }

            return "";
        }
    }
}
