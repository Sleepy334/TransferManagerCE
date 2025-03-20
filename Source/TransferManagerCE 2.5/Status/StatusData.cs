using ColossalFramework;
using System;
using TransferManagerCE.Common;
using UnityEngine;
using static ColossalFramework.Threading.ContextSwitch;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusData : IComparable
    {
        public TransferReason m_material;
        public BuildingType m_eBuildingType;
        public ushort m_buildingId;
        public ushort m_responderBuilding;
        public ushort m_targetVehicle;

        protected string m_value;
        protected string m_target;
        protected string m_responder;
        protected string m_distance;
        protected string m_timer;
        protected string m_load;

        public StatusData(TransferReason reason, BuildingType eBuildingType, ushort buildingId, ushort responderBuilding, ushort targetVehicle)
        {
            m_material = reason;
            m_eBuildingType = eBuildingType;
            m_buildingId = buildingId;
            m_responderBuilding = responderBuilding;
            m_targetVehicle = targetVehicle;
        }

        public void Calculate()
        {
            // Cache these as they are slow
            m_value = CalculateValue();
            m_target = CalculateTarget();
            m_responder = CalculateResponder();
            m_distance = CalculateDistance();
            m_timer = CalculateTimer();
            m_load = CalculateLoad();
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            StatusData oSecond = (StatusData)second;
            return GetMaterialDescription().CompareTo(oSecond.GetMaterialDescription());
        }

        public virtual bool IsSeparator()
        {
            return false;
        }

        public virtual string GetMaterialDescription()
        {
            return GetMaterial().ToString();
        }

        public virtual CustomTransferReason GetMaterial()
        {
            return m_material;
        }

        public virtual string GetTooltip()
        {
            return "";
        }

        public virtual string GetValue()
        {
            return m_value;
        }

        public virtual string GetValueTooltip()
        {
            return "";
        }

        public virtual string GetTimer()
        {
            return m_timer;
        }

        public virtual string GetResponder()
        {
            return m_responder;
        }

        public virtual string GetResponderTooltip()
        {
            if (GetResponderId() != 0)
            {
                return $"#{GetResponderId()}:{GetResponder()}";
            }

            return "";
        }

        public virtual string GetTarget()
        {
            return m_target;
        }

        public virtual string GetTargetTooltip()
        {
            if (GetTargetId() != 0)
            {
                return $"#{GetTargetId()}:{GetTarget()}";
            }

            return "";
        }

        public virtual string GetLoad()
        {
            return m_load;
        }

        public virtual string GetDistance()
        {
            return m_distance;
        }

        public virtual void OnClickTarget()
        {
            ushort targetId = GetTargetId();
            if (targetId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { Vehicle = targetId });
            }
        }

        public virtual void OnClickResponder()
        {
            ushort buildingId = GetResponderId();
            if (buildingId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { Building = buildingId });
            }
        }

        public virtual ushort GetResponderId()
        {
            if (m_responderBuilding != 0)
            {
                return m_responderBuilding;
            }
            ushort targetId = GetTargetId();
            if (targetId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[targetId];
                return vehicle.m_sourceBuilding;
            }
            return 0;
        }

        public virtual ushort GetTargetId()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                if (vehicle.m_cargoParent != 0)
                {
                    return vehicle.m_cargoParent;
                }
                else
                {
                    return m_targetVehicle;
                }
            }

            return 0;
        }

        public virtual Color GetTextColor()
        {
            return Color.white;
        }

        protected virtual string CalculateValue()
        {
            return "";
        }

        protected virtual string CalculateTarget()
        {
            ushort vehicleId = GetTargetId();
            if (vehicleId != 0)
            {
                return CitiesUtils.GetVehicleName(vehicleId);
            }

            return Localization.Get("txtStatusNone");
        }

        protected virtual string CalculateResponder()
        {
            ushort buildingId = GetResponderId();
            if (buildingId != 0)
            {
                return CitiesUtils.GetBuildingName(buildingId);
            }
            return Localization.Get("txtStatusNone");
        }

        protected virtual string CalculateLoad()
        {
            ushort vehicleId = GetTargetId();
            if (vehicleId != 0)
            {
                return CitiesUtils.GetVehicleTransferValue(vehicleId);
            }

            return "";
        }

        protected virtual string CalculateTimer()
        {
            string sTimer = "";

            ushort targetId = GetTargetId();
            if (targetId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[targetId];
                if (vehicle.m_waitCounter > 0)
                {
                    sTimer += "W:" + vehicle.m_waitCounter + " ";
                }
                if (vehicle.m_blockCounter > 0)
                {
                    sTimer += "B:" + vehicle.m_blockCounter + " ";
                }
            }

            return sTimer;
        }

        protected virtual string CalculateDistance()
        {
            ushort vehicleId = GetTargetId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    InstanceID target = new InstanceID {  Building = m_buildingId };
                    Vector3 buildingPos = InstanceHelper.GetPosition(target);
                    Vector3 vehiclePos = vehicle.GetLastFramePosition();
                    return (Math.Sqrt(Vector3.SqrMagnitude(vehiclePos - buildingPos)) * 0.001).ToString("0.00");
                }
            }

            return "";
        }
    }
}
