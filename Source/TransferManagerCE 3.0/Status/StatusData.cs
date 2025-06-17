using SleepyCommon;
using System;
using TransferManagerCE.UI;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusData : IComparable
    {
        public CustomTransferReason.Reason m_material;
        public BuildingType m_eBuildingType;
        public ushort m_buildingId;

        protected Color m_color;

        private string? m_value = null;
        private string? m_vehicle = null;
        private string? m_responder = null;
        private string? m_timer = null;
        private double? m_distance = null;

        private string m_valueTooltip = "";
        private string m_vehicleTooltip = "";
        private string m_responderTooltip = "";
        private string m_timerTooltip = "";

        public StatusData(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort buildingId)
        {
            m_material = reason;
            m_eBuildingType = eBuildingType;
            m_buildingId = buildingId;
            m_color = Color.white;
        }

        public bool HasBuildingReason(CustomTransferReason.Reason reason)
        {
            return BuildingPanel.Instance.GetStatusHelper().HasBuildingReason(reason);
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            StatusData oSecond = (StatusData)second;

            // Sort by waiting timer
            if (IsNodeData() && oSecond.IsNodeData())
            {
                return ((StatusNodeStop)oSecond).GetWaitTimer() - ((StatusNodeStop)this).GetWaitTimer();
            }

            // Sort by material
            if (GetMaterialDescription() != oSecond.GetMaterialDescription())
            {
                return GetMaterialDescription().CompareTo(oSecond.GetMaterialDescription());
            }

            // Put the building entry first for each material type
            if (IsBuildingData() != oSecond.IsBuildingData())
            {
                if (IsBuildingData())
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            // Sort by distance
            if (GetDistance() < oSecond.GetDistance())
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public virtual bool IsSeparator()
        {
            return false;
        }

        public virtual bool IsHeader()
        {
            return false;
        }

        public abstract string GetMaterialDisplay();

        // Building support
        public abstract bool IsBuildingData();
        public abstract bool IsNodeData(); 

        // Vehicle support
        public abstract bool HasVehicle();
        public abstract ushort GetVehicleId();

        protected abstract string CalculateValue(out string tooltip);
        protected abstract string CalculateVehicle(out string tooltip);
        protected abstract string CalculateResponder(out string tooltip);
        protected abstract string CalculateTimer(out string tooltip);
        protected abstract double CalculateDistance();

        public virtual string GetMaterialDescription()
        {
            return GetMaterial().ToString();
        }

        public virtual CustomTransferReason.Reason GetMaterial()
        {
            return m_material;
        }

        public virtual string GetTooltip()
        {
            return "";
        }

        public string GetValue()
        {
            if (m_value is null)
            {
                m_value = CalculateValue(out m_valueTooltip);
            }
            return m_value;
        }

        public string GetValueTooltip()
        {
            return m_valueTooltip;
        }

        public virtual string GetTimer()
        {
            if (m_timer is null)
            {
                m_timer = CalculateTimer(out string m_timerTooltip);
            }
            return m_timer;
        }

        public string GetTimerTooltip()
        {
            return m_timerTooltip;
        }

        public virtual string GetResponder()
        {
            if (m_responder is null)
            {
                m_responder = CalculateResponder(out m_responderTooltip);
            }
            return m_responder;
        }

        public string GetResponderTooltip()
        {
            return m_responderTooltip;
        }

        public virtual string GetVehicle()
        {
            if (m_vehicle is null)
            {
                m_vehicle = CalculateVehicle(out m_vehicleTooltip);
            }
            return m_vehicle;
        }

        public string GetVehicleTooltip()
        {
            return m_vehicleTooltip;
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
            if (GetVehicleId() != 0)
            {
                return GetDistance().ToString("0.00");
            }
            else
            {
                return "";
            }
        }

        public virtual ushort GetResponderId()
        {
            return 0;
        }

        public virtual Color GetTextColor()
        {
            return m_color;
        }

        public virtual void OnClickTarget()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { Vehicle = vehicleId });
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

        public static string DisplayBuffer(int iBuffer)
        {
            if (iBuffer > 10000)
            {
                return $"{((int)(iBuffer * 0.001)).ToString("N0")}k";
            }
            else
            {
                return $"{iBuffer.ToString("N0")}";
            }
        }

        public static string DisplayBufferLong(int iBuffer)
        {
            return $"{iBuffer.ToString("N0")}";
        }
    }
}
