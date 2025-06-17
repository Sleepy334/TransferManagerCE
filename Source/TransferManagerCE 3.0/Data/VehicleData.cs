using ColossalFramework;
using SleepyCommon;
using System;
using System.Drawing;
using UnityEngine;

namespace TransferManagerCE.Data
{
    public class VehicleData : IComparable
    {
        public ushort m_vehicleId;
        public Vehicle m_vehicle;
        private string? m_name = null;
        
        private double? m_distance = null;
        private string? m_target = null;
        private Vector3 m_buildingPos = Vector3.zero;
        private CustomTransferReason.Reason? m_reason = null;

        protected string? m_value = null;
        protected string m_valueTooltip = "";

        public VehicleData(Vector3 buildingPos, ushort vehicleId)
        {
            m_buildingPos = buildingPos;
            m_vehicleId = vehicleId;
            m_vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
        }

        public virtual bool IsHeading()
        {
            return false;
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            VehicleData oSecond = (VehicleData)second;
            if (m_vehicle.m_transferType == oSecond.m_vehicle.m_transferType)
            {
                if (GetDistance() < oSecond.GetDistance())
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return m_vehicle.m_transferType.CompareTo(oSecond.m_vehicle.m_transferType);
            }
        }

        public CustomTransferReason.Reason GetMaterial()
        {
            if (m_reason is null)
            {
                m_reason = (CustomTransferReason.Reason) VehicleUtils.GetTransferType(m_vehicle);
            }
            return m_reason.Value;
        }

        public virtual string GetMaterialDescription()
        {
            return GetMaterial().ToString();
        }

        public virtual string GetTarget()
        {
            if (m_target is null)
            {
                m_target = CalculateTarget();
            }
            return m_target;
        }

        public virtual string GetTargetTooltip()
        {
            if (m_vehicle.m_targetBuilding != 0)
            {
                return CitiesUtils.GetBuildingName(m_vehicle.m_targetBuilding, InstanceID.Empty, true);
            }
            else if (m_vehicle.m_sourceBuilding != 0)
            {
                return CitiesUtils.GetBuildingName(m_vehicle.m_sourceBuilding, InstanceID.Empty, true);
            }
                
            return GetTarget();
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
            if (m_name is null)
            {
                m_name = GetVehicleName();
            }
            return m_name;
        }

        public virtual string GetVehicleTooltip()
        {
            if (GetVehicleId() != 0)
            {
                return VehicleUtils.GetVehicleTooltip(GetVehicleId());
            }

            return "";
        }

        public virtual string GetValue()
        {
            if (m_value is null)
            {
                m_value = CitiesUtils.GetVehicleTransferValue(GetVehicleId(), out int current, out int max);
                m_valueTooltip = $"{(CustomTransferReason)m_vehicle.m_transferType} | {StatusData.DisplayBufferLong(current)} / {StatusData.DisplayBufferLong(max)}";
            }
            return m_value;
        }

        public virtual string GetValueTooltip()
        {
            return m_valueTooltip;
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

        public virtual double GetDistance()
        {
            if (m_distance is null)
            {
                m_distance = CalculateDistance();
            }
            return m_distance.Value;
        }

        public virtual string GetDistanceAsString()
        {
            double distance = GetDistance();
            if (distance == double.MaxValue)
            {
                return "";
            }
            else
            {
                return distance.ToString("0.00");
            }
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
                    return VehicleTypeHelper.DescribeVehicleTarget(vehicleId, vehicle, target);
                }
            }

            return Localization.Get("txtVehiclesNone");
        }

        private double CalculateDistance()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    // How far is vehicle from source building
                    InstanceID sourceBuilding = new InstanceID { Building = vehicle.m_sourceBuilding };
                    Vector3 vehiclePos = vehicle.GetLastFramePosition();
                    return Math.Sqrt(Vector3.SqrMagnitude(vehiclePos - m_buildingPos)) * 0.001;
                }
            }

            return double.MaxValue;
        }

        public virtual UnityEngine.Color GetTextColor()
        {
            return SleepyCommon.KnownColor.white;
        }
    }
}
