using SleepyCommon;
using TransferManagerCE;
using TransferManagerCE.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class DistrictCheckListData : CheckListData
    {
        public DistrictData.DistrictType m_eType;
        public int m_districtId;
        public string m_districtName;
        public bool m_bIncoming;

        // ----------------------------------------------------------------------------------------
        public DistrictCheckListData(bool bIncoming, DistrictData.DistrictType eType, byte districtId)
        {
            m_bIncoming = bIncoming;
            m_eType = eType;
            m_districtId = districtId;
            m_districtName = GetDistrictName();
        }

        public DistrictCheckListData(DistrictCheckListData oSecond)
        {
            m_bIncoming = oSecond.m_bIncoming;
            m_eType = oSecond.m_eType;
            m_districtId = oSecond.m_districtId;
            m_districtName = GetDistrictName();
        }

        public override string GetText()
        {
            return m_districtName;
        }

        public override Color GetTextColor()
        {
            if (m_eType == DistrictData.DistrictType.District)
            {
                return KnownColor.white;
            }
            else
            {
                return KnownColor.cyan;
            }
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

        public override bool IsChecked()
        {
            DistrictSelectionPanel? panel = DistrictSelectionPanel.Instance;
            if (panel is not null)
            {
                ushort buildingId = panel.m_buildingId;
                int iRestrictionId = panel.m_iRestrictionId;

                BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);       
                if (settings is not null)
                {
                    RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                    if (restrictions is not null)
                    {
                        if (m_bIncoming)
                        {
                            return restrictions.m_incomingDistrictSettings.IsAdditionalDistrictAllowed(m_eType, m_districtId);
                        }
                        else
                        {
                            return restrictions.m_outgoingDistrictSettings.IsAdditionalDistrictAllowed(m_eType, m_districtId);
                        }
                    }
                }
            }

            return false;
        }

        public override void OnItemCheckChanged(bool bChecked)
        {
            ushort buildingId = DistrictSelectionPanel.Instance.m_buildingId;
            int iRestrictionId = DistrictSelectionPanel.Instance.m_iRestrictionId;

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(iRestrictionId);

            if (m_bIncoming)
            {
                restrictions.m_incomingDistrictSettings.SetDistrictAllowed(buildingId, m_eType, m_districtId, bChecked);
            }
            else
            {
                restrictions.m_outgoingDistrictSettings.SetDistrictAllowed(buildingId, m_eType, m_districtId, bChecked);
            }

            settings.SetRestrictions(iRestrictionId, restrictions);
            BuildingSettingsStorage.SetSettings(buildingId, settings);
        }

        public override void OnShow()
        {
            if (m_eType == DistrictData.DistrictType.District)
            {
                InstanceHelper.ShowInstance(new InstanceID { District = (byte)m_districtId });
            }
            else
            {
                InstanceHelper.ShowInstance(new InstanceID { Park = (byte)m_districtId });
            }
        }
    }
}