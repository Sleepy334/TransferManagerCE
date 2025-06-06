using ICities;
using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace TransferManagerCE.Settings
{
    public class DistrictRestrictionSettings
    {
        public enum PreferLocal
        {
            AllDistricts = 0,
            PreferLocalDistrict = 1,
            RestrictLocalDistrict = 2,
            AllDistrictsExceptFor = 3,
            Unknown = 255,
        }

        public PreferLocal m_iPreferLocalDistricts = PreferLocal.AllDistricts;
        public bool m_bAllowLocalDistrict = true;
        public bool m_bAllowLocalPark = true;
        public HashSet<DistrictData> m_allowedDistricts = new HashSet<DistrictData>();

        public DistrictRestrictionSettings()
        {

        }

        public DistrictRestrictionSettings(DistrictRestrictionSettings oSecond)
        {
            m_iPreferLocalDistricts = oSecond.m_iPreferLocalDistricts;
            m_bAllowLocalDistrict = oSecond.m_bAllowLocalDistrict;
            m_bAllowLocalPark = oSecond.m_bAllowLocalPark;
            m_allowedDistricts = new HashSet<DistrictData>(oSecond.m_allowedDistricts);
        }

        public bool Equals(DistrictRestrictionSettings oSecond)
        {
            return m_iPreferLocalDistricts == oSecond.m_iPreferLocalDistricts &&
                    m_bAllowLocalDistrict == oSecond.m_bAllowLocalDistrict &&
                    m_bAllowLocalPark == oSecond.m_bAllowLocalPark &&
                    m_allowedDistricts.SetEquals(oSecond.m_allowedDistricts);
        }

        public HashSet<DistrictData> GetAllowedDistricts(ushort buildingId, byte? district, byte? park)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(m_allowedDistricts);

            // Add current district if allowed
            if (m_bAllowLocalDistrict)
            {
                if (district is null)
                {
                    district = DistrictManager.instance.GetDistrict(building.m_position);
                }
                if (district != 0)
                {
                    DistrictData data = new DistrictData(DistrictData.DistrictType.District, district.Value);
                    if (!list.Contains(data))
                    {
                        list.Add(data);
                    }
                }
            }

            // Add current park if allowed
            if (m_bAllowLocalPark)
            {
                if (park is null)
                {
                    park = DistrictManager.instance.GetPark(building.m_position);
                }
                if (park != 0)
                {
                    DistrictData data = new DistrictData(DistrictData.DistrictType.Park, park.Value);
                    if (!list.Contains(data))
                    {
                        list.Add(data);
                    }
                }
            }

            return list;
        }

        public bool IsAdditionalDistrictAllowed(DistrictData.DistrictType eType, int district)
        {
            return m_allowedDistricts.Contains(new DistrictData(eType, district));
        }

        public void SetDistrictAllowed(ushort buildingId, DistrictData.DistrictType eType, int district, bool bChecked)
        {
            // Check "current" district
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            switch (eType)
            {
                case DistrictData.DistrictType.District:
                    {
                        byte currentDistrict = DistrictManager.instance.GetDistrict(building.m_position);
                        if (currentDistrict == district)
                        {
                            m_bAllowLocalDistrict = bChecked;
                            return;
                        }
                        break;
                    }
                case DistrictData.DistrictType.Park:
                    {
                        byte currentPark = DistrictManager.instance.GetPark(building.m_position);
                        if (currentPark == district)
                        {
                            m_bAllowLocalPark = bChecked;
                            return;
                        }
                        break;
                    }
            }

            // Check additional districts
            bool bContains = IsAdditionalDistrictAllowed(eType, district);
            if (bChecked)
            {
                if (!bContains)
                {
                    m_allowedDistricts.Add(new DistrictData(eType, district));
                }
            }
            else
            {
                if (bContains)
                {
                    m_allowedDistricts.Remove(new DistrictData(eType, district));
                }
            }
        }

        public void ToggleDistrictAllowed(ushort buildingId, DistrictData.DistrictType eType, int district)
        {
            // Turn on/off "current" option.
            bool bAdded = false;

            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            switch (eType)
            {
                case DistrictData.DistrictType.District:
                    {
                        byte currentDistrict = DistrictManager.instance.GetDistrict(building.m_position);
                        if (currentDistrict == district)
                        {
                            m_bAllowLocalDistrict = !m_bAllowLocalDistrict;
                            bAdded = true;
                        }
                        break;
                    }
                case DistrictData.DistrictType.Park:
                    {
                        byte currentPark = DistrictManager.instance.GetPark(building.m_position);
                        if (currentPark == district)
                        {
                            m_bAllowLocalPark = !m_bAllowLocalPark;
                            bAdded = true;
                        }
                        break;
                    }
            }

            // Remove from array
            if (IsAdditionalDistrictAllowed(eType, district))
            {
                m_allowedDistricts.Remove(new DistrictData(eType, district));
            }
            else if (!bAdded)
            {
                m_allowedDistricts.Add(new DistrictData(eType, district));
            }
        }

        public bool IsSet()
        {
            return m_iPreferLocalDistricts != PreferLocal.AllDistricts ||
                    m_allowedDistricts.Count > 0 ||
                    !m_bAllowLocalDistrict ||
                    !m_bAllowLocalPark;
        }

        public void Reset()
        {
            m_iPreferLocalDistricts = PreferLocal.AllDistricts;
            m_allowedDistricts.Clear();
            m_bAllowLocalDistrict = true;
            m_bAllowLocalPark = true;
        }

        public string GetTooltip(ushort buildingId)
        {
            string sMessage = "";
            switch (m_iPreferLocalDistricts)
            {
                case PreferLocal.AllDistricts:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        sMessage += "\r\n- " + Localization.Get("dropdownBuildingPanelPreferLocal1");
                        return sMessage;
                    }
                case PreferLocal.PreferLocalDistrict:
                case PreferLocal.RestrictLocalDistrict:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        break;
                    }
                case PreferLocal.AllDistrictsExceptFor:
                    {
                        sMessage += Localization.Get("txtBlockedDistricts") + ":";
                        break;
                    }
            }

            // Add allowed districts to message
            HashSet<DistrictData> list = GetAllowedDistricts(buildingId, null, null);
            foreach (DistrictData district in list)
            {
                if (district.m_eType == DistrictData.DistrictType.District)
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetDistrictName(district.m_iDistrictId);
                }
                else
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetParkName(district.m_iDistrictId);
                }
            }

            return sMessage;
        }

        public bool Validate()
        {
            // Check districts are still valid
            bool bChanged = false;

            if (m_allowedDistricts.Count > 0)
            {
                if (ValidateDistricts(m_allowedDistricts, out HashSet<DistrictData> newDistricts))
                {
                    m_allowedDistricts = newDistricts;
                    bChanged = true;
                }
            }

            return bChanged;
        }

        public string DescribeDistricts(ushort buildingId)
        {
            string sMessage = "";

            // Add allowed districts to message
            HashSet<DistrictData> list = GetAllowedDistricts(buildingId, null, null);
            foreach (DistrictData district in list)
            {
                if (district.m_eType == DistrictData.DistrictType.District)
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetDistrictName(district.m_iDistrictId);
                }
                else
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetParkName(district.m_iDistrictId);
                }
            }

            return sMessage;
        }

        public bool ValidateDistricts(HashSet<DistrictData> districts, out HashSet<DistrictData> newSet)
        {
            newSet = new HashSet<DistrictData>();

            bool bChanged = false;
            foreach (DistrictData districtId in districts)
            {
                if (districtId.IsDistrict())
                {
                    District district = DistrictManager.instance.m_districts.m_buffer[districtId.m_iDistrictId];
                    if ((district.m_flags & District.Flags.Created) != 0)
                    {
                        newSet.Add(districtId);
                    }
                    else
                    {
                        bChanged = true;
                        // District doesn't exist any more
                        CDebug.Log("District missing: " + districtId.m_iDistrictId);
                    }
                }
                else
                {
                    DistrictPark park = DistrictManager.instance.m_parks.m_buffer[districtId.m_iDistrictId];
                    if ((park.m_flags & DistrictPark.Flags.Created) != 0)
                    {
                        newSet.Add(districtId);
                    }
                    else
                    {
                        bChanged = true;
                        // District doesn't exist any more
                        CDebug.Log("Park missing: " + districtId.m_iDistrictId);
                    }
                }
            }

            return bChanged;
        }
    }
}
