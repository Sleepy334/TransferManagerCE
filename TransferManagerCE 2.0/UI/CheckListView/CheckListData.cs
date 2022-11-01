using ColossalFramework.UI;
using System;
using TransferManagerCE;
using static TransferManagerCE.BuildingSettings;

namespace SleepyCommon
{
    public class CheckListData : IComparable
    {
        public DistrictData.DistrictType m_eType;
        public int m_districtId;
        public string m_districtName;
        public bool m_bIncoming;

        public CheckListData(bool bIncoming, DistrictData.DistrictType eType, byte districtId)
        {
            m_bIncoming = bIncoming;
            m_eType = eType;
            m_districtId = districtId;
            m_districtName = GetDistrictName();
        }

        public CheckListData(CheckListData oSecond)
        {
            m_bIncoming = oSecond.m_bIncoming;
            m_eType = oSecond.m_eType;
            m_districtId = oSecond.m_districtId;
            m_districtName = GetDistrictName();
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            CheckListData oSecond = (CheckListData)second;
            return GetText().CompareTo(oSecond.GetText());
        }

        public string GetText()
        {
            return m_districtName;
        }

        public string GetDistrictName()
        {
            if (m_eType == DistrictData.DistrictType.Park)
            {
                DistrictPark district = DistrictManager.instance.m_parks.m_buffer[m_districtId];
                if (district.m_flags != 0)
                {
                    return DistrictManager.instance.GetParkName(m_districtId);
                }
            }
            else
            {
                District district = DistrictManager.instance.m_districts.m_buffer[m_districtId];
                if (district.m_flags != 0)
                {
                    return DistrictManager.instance.GetDistrictName(m_districtId);
                }
            }
            
            return "Unknown";
        }

        public bool IsChecked()
        {
            if (BuildingPanel.Instance != null)
            {
                BuildingSettings settings = BuildingSettings.GetSettings(BuildingPanel.Instance.m_buildingId);
                if (m_bIncoming)
                {
                    return settings.IsIncomingDistrictAllowed(m_eType, m_districtId);
                }
                else
                {
                    return settings.IsOutgoingDistrictAllowed(m_eType, m_districtId);
                }
            }

            return false;
        }
    }
}