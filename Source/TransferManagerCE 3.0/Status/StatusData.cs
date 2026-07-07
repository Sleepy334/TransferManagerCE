using ColossalFramework;
using SleepyCommon;
using System;
using TransferManagerCore.UI;
using UnityEngine;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public abstract class StatusData : IComparable
    {
        // --------------------------------------------------------------------
        public CustomTransferReason.Reason m_material;
        public BuildingType m_eBuildingType;
        public ushort m_buildingId;

        // Status information
        protected Color m_color;

        private string? m_value = null;
        private string? m_timer = null;
        private double? m_distance = null;
        private string? m_description1 = null;
        private string? m_description2 = null;

        private string m_valueTooltip = "";
        private string m_timerTooltip = "";
        private string m_description1Tooltip = "";
        private string m_description2Tooltip = "";

        // --------------------------------------------------------------------
        public StatusData(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort buildingId)
        {
            m_material = reason;
            m_eBuildingType = eBuildingType;
            m_buildingId = buildingId;
            m_color = Color.white;
        }

        // --------------------------------------------------------------------
        public bool HasBuildingReason(CustomTransferReason.Reason reason)
        {
            return BuildingPanel.Instance.GetStatusHelper().HasBuildingReason(reason);
        }

        // --------------------------------------------------------------------
        public virtual int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            StatusData oSecond = (StatusData)second;

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

            return oSecond.GetValue().CompareTo(GetValue());
        }

        // --------------------------------------------------------------------
        // Type support
        public abstract bool IsBuildingData();
        public abstract bool IsVehicleData();

        // --------------------------------------------------------------------
        public virtual bool IsSeparator()
        {
            return false;
        }

        public virtual bool IsHeader()
        {
            return false;
        }

        // --------------------------------------------------------------------
        public virtual bool CanDelete()
        {
            return false;
        }

        public virtual string GetDeleteTooltip()
        {
            return "";
        }

        public virtual void OnClickDelete()
        {
        }

        // --------------------------------------------------------------------
        public virtual ushort GetVehicleId() { return 0; }

        // --------------------------------------------------------------------
        public abstract string GetMaterialDisplay();

        // --------------------------------------------------------------------
        // Status information
        protected abstract string CalculateValue(out string tooltip);
        protected abstract string CalculateTimer(out string tooltip);
        protected abstract double CalculateDistance();
        protected abstract string CalculateDescription1(out string tooltip);
        protected abstract string CalculateDescription2(out string tooltip);

        // --------------------------------------------------------------------
        public virtual string GetMaterialDescription()
        {
            return GetMaterial().ToString();
        }

        // --------------------------------------------------------------------
        public virtual CustomTransferReason.Reason GetMaterial()
        {
            return m_material;
        }

        // --------------------------------------------------------------------
        // Global tooltip
        public virtual string GetTooltip()
        {
            return "";
        }

        // --------------------------------------------------------------------
        public string GetValue()
        {
            if (m_value is null)
            {
                m_value = CalculateValue(out m_valueTooltip);
            }
            return m_value;
        }

        // --------------------------------------------------------------------
        public string GetValueTooltip()
        {
            return m_valueTooltip;
        }

        // --------------------------------------------------------------------
        public virtual string GetTimer()
        {
            if (m_timer is null)
            {
                m_timer = CalculateTimer(out m_timerTooltip);
            }
            return m_timer;
        }

        // --------------------------------------------------------------------
        public string GetTimerTooltip()
        {
            return m_timerTooltip;
        }

        // --------------------------------------------------------------------
        public virtual double GetDistance()
        {
            if (m_distance is null)
            {
                m_distance = CalculateDistance();
            }
            return m_distance.Value;
        }

        // --------------------------------------------------------------------
        public virtual string GetDistanceAsString()
        {
            if (IsVehicleData())
            {
                return GetDistance().ToString("0.00");
            }
            else
            {
                return "";
            }
        }

        // --------------------------------------------------------------------
        public virtual string GetDescription1()
        {
            if (m_description1 is null)
            {
                m_description1 = CalculateDescription1(out m_description1Tooltip);
            }
            return m_description1;
        }

        // --------------------------------------------------------------------
        public string GetDescription1Tooltip()
        {
            return m_description1Tooltip;
        }

        // --------------------------------------------------------------------
        public virtual string GetDescription2()
        {
            if (m_description2 is null)
            {
                m_description2 = CalculateDescription2(out m_description2Tooltip);
            }
            return m_description2;
        }

        // --------------------------------------------------------------------
        public string GetDescription2Tooltip()
        {
            return m_description2Tooltip;
        }

        // --------------------------------------------------------------------
        public virtual Color GetTextColor()
        {
            return m_color;
        }

        // --------------------------------------------------------------------
        public virtual void OnClickDescription1()
        {
        }

        // --------------------------------------------------------------------
        public virtual void OnClickDescription2()
        {
        }

        // --------------------------------------------------------------------
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

        // --------------------------------------------------------------------
        public static string DisplayBufferLong(int iBuffer)
        {
            return $"{iBuffer.ToString("N0")}";
        }

        // --------------------------------------------------------------------
        protected ushort FindBuilding(ushort nodeId)
        {
            NetNode node = NetManager.instance.m_nodes.m_buffer[nodeId];
            if (node.m_building != 0)
            {
                return node.m_building;
            }

            return BuildingManager.instance.FindBuilding(node.m_position, 192f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
        }
    }
}
