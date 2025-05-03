using System;
using TransferManagerCE.UI;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class SettingsData : IComparable
    {
        private ushort m_buildingId;

        private string? m_description = null;
        private BuildingType? m_buildingType = null;
        private string? m_tooltip = null;
        private int? m_restrictions = null;

        private byte? m_district = null;
        private byte? m_park = null;
        private string? m_districtName = null;
        private string? m_parkName = null;
        private string? m_describeDistricts = null;

        public SettingsData(ushort buildingId)
        {
            m_buildingId = buildingId;
            m_restrictions = 0;
        }

        public SettingsData(SettingsData oSecond)
        {
            m_buildingId = oSecond.m_buildingId;
            m_restrictions = oSecond.m_restrictions;
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            SettingsData oSecond = (SettingsData) second;

            // Districts first
            if (DescribeDistricts() != oSecond.DescribeDistricts())
            {
                if (DescribeDistricts().Length == 0)
                {
                    return 1; // Sort these to the end
                }
                else if (oSecond.DescribeDistricts().Length == 0)
                {
                    return -1; // Sort these to the end
                }

                return DescribeDistricts().ToString().CompareTo(oSecond.DescribeDistricts().ToString());
            }

            return GetDescription().CompareTo(oSecond.GetDescription());
        }

        public bool Contains(string search)
        {
            if (GetBuildingId().ToString().ToUpper().Contains(search))
            {
                return true;
            }
            if (GetDescription().ToUpper().Contains(search))
            {
                return true;
            }
            if (GetBuildingType().ToString().ToUpper().Contains(search))
            {
                return true;
            }
            if (GetDistrict() != 0 && GetDistrictName().ToString().ToUpper().Contains(search))
            {
                return true;
            }
            if (GetPark() != 0 && GetParkName().ToString().ToUpper().Contains(search))
            {
                return true;
            }
            return false;
        }

        public ushort GetBuildingId()
        {
            return m_buildingId;
        }

        public int GetRestrctionCount()
        {
            if (m_restrictions is null)
            {
                DescribeSettings();
            }
            return m_restrictions.Value;
        }

        public string GetDescription()
        {
            if (m_description is null)
            {
                m_description = InstanceHelper.DescribeInstance(new InstanceID { Building = GetBuildingId() });
            }
            return m_description;
        }

        public BuildingType GetBuildingType()
        {
            if (m_buildingType is null)
            {
                m_buildingType = BuildingTypeHelper.GetBuildingType(GetBuildingId());
            }
            return m_buildingType.Value;
        }

        public byte GetDistrict()
        {
            if (m_district is null)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuildingId()];
                if (building.m_flags != 0)
                {
                    m_district = DistrictManager.instance.GetDistrict(building.m_position);
                }
                else
                {
                    m_district = 0;
                }
            }
            return m_district.Value;
        }

        public string GetDistrictName()
        {
            if (m_districtName is null)
            {
                m_districtName = DistrictManager.instance.GetDistrictName(GetDistrict());
            }
            return m_districtName;
        }

        

        public byte GetPark()
        {
            if (m_park is null)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuildingId()];
                if (building.m_flags != 0)
                {
                    m_park = DistrictManager.instance.GetPark(building.m_position);
                }
                else
                {
                    m_park = 0;
                }
            }
            return m_park.Value;
        }

        public string GetParkName()
        {
            if (m_parkName is null)
            {
                m_parkName = DistrictManager.instance.GetParkName(GetPark());
            }
            return m_parkName;
        }

        public void ShowInstance()
        {
            InstanceHelper.ShowInstanceSetBuildingPanel(new InstanceID { Building = GetBuildingId() });
        }

        public string DescribeSettings()
        {
            if (m_tooltip is null)
            {
                BuildingSettings? setting = BuildingSettingsStorage.GetSettings(GetBuildingId());
                if (setting is not null)
                {
                    int iRestrictions = 0;
                    m_tooltip = setting.DescribeSettings(GetBuildingId(), ref iRestrictions);
                    m_restrictions = iRestrictions;
                }
                else
                {
                    m_tooltip = string.Empty;
                    m_restrictions = 0;
                }
            }

            return m_tooltip;
        }

        public string DescribeDistricts()
        {
            if (m_describeDistricts is null)
            {
                m_describeDistricts = "";

                if (GetDistrict() != 0)
                {
                    m_describeDistricts += GetDistrictName();
                }
                if (GetPark() != 0)
                {
                    if (m_describeDistricts.Length > 0)
                    {
                        m_describeDistricts += ", ";
                    }
                    m_describeDistricts += GetParkName();
                }
            }
            return m_describeDistricts;
        }
    }
}