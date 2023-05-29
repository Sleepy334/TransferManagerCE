
using System;
using System.Collections.Generic;
using TransferManagerCE.Common;

namespace TransferManagerCE
{
    public class RestrictionSettings
    {
        public const int iRESTRICTION_SETTINGS_DATA_VERSION = 2;
        private static RestrictionSettings s_defaultSettings = new RestrictionSettings();

        public enum PreferLocal
        {
            AllDistricts = 0,
            PreferLocalDistrict = 1,
            RestrictLocalDistrict = 2,
            AllDistrictsExceptFor = 3,
            Unknown = 255,
        }

        public enum ImportExport
        {
            ALLOW_IMPORT_AND_EXPORT,
            ALLOW_IMPORT_ONLY,
            ALLOW_EXPORT_ONLY,
            ALLOW_NEITHER,
        }

        // Building settings
        public bool m_bAllowImport;
        public bool m_bAllowExport;

        public PreferLocal m_iPreferLocalDistrictsIncoming;
        public PreferLocal m_iPreferLocalDistrictsOutgoing;
        public int m_iServiceDistance;

        public bool m_bIncomingAllowLocalDistrict;
        public bool m_bIncomingAllowLocalPark;
        public bool m_bOutgoingAllowLocalDistrict;
        public bool m_bOutgoingAllowLocalPark;
        public HashSet<DistrictData> m_incomingDistrictAllowed;
        public HashSet<DistrictData> m_outgoingDistrictAllowed;
        private HashSet<ushort> m_incomingBuildingsAllowed;
        private HashSet<ushort> m_outgoingBuildingsAllowed;

        public RestrictionSettings()
        {
            m_bAllowImport = true;
            m_bAllowExport = true;

            m_iPreferLocalDistrictsIncoming = PreferLocal.AllDistricts;
            m_iPreferLocalDistrictsOutgoing = PreferLocal.AllDistricts;
            m_iServiceDistance = 0;

            m_bIncomingAllowLocalDistrict = true;
            m_bIncomingAllowLocalPark = true;
            m_incomingDistrictAllowed = new HashSet<DistrictData>();
            m_incomingBuildingsAllowed = new HashSet<ushort>();

            m_bOutgoingAllowLocalDistrict = true;
            m_bOutgoingAllowLocalPark = true;
            m_outgoingDistrictAllowed = new HashSet<DistrictData>();
            m_outgoingBuildingsAllowed = new HashSet<ushort>();
        }

        public RestrictionSettings(RestrictionSettings oSecond)
        {
            m_bAllowImport = oSecond.m_bAllowImport;
            m_bAllowExport = oSecond.m_bAllowExport;

            m_iPreferLocalDistrictsIncoming = oSecond.m_iPreferLocalDistrictsIncoming;
            m_iPreferLocalDistrictsOutgoing = oSecond.m_iPreferLocalDistrictsOutgoing;
            m_iServiceDistance = oSecond.m_iServiceDistance;

            m_bIncomingAllowLocalDistrict = oSecond.m_bIncomingAllowLocalDistrict;
            m_bIncomingAllowLocalPark = oSecond.m_bIncomingAllowLocalPark;
            m_incomingDistrictAllowed = new HashSet<DistrictData>(oSecond.m_incomingDistrictAllowed);
            m_incomingBuildingsAllowed = new HashSet<ushort>(oSecond.m_incomingBuildingsAllowed);

            m_bOutgoingAllowLocalDistrict = oSecond.m_bOutgoingAllowLocalDistrict;
            m_bOutgoingAllowLocalPark = oSecond.m_bOutgoingAllowLocalPark;
            m_outgoingDistrictAllowed = new HashSet<DistrictData>(oSecond.m_outgoingDistrictAllowed);
            m_outgoingBuildingsAllowed = new HashSet<ushort>(oSecond.m_outgoingBuildingsAllowed);
        }

        // We just use IsDefault() rather than implementing this
        [Obsolete("This method has not yet been implemented.", true)]
        public bool Equals(RestrictionSettings second)
        {
            throw new NotImplementedException();
        }

        public bool IsDefault()
        {
            return m_bAllowImport == s_defaultSettings.m_bAllowImport &&
                    m_bAllowExport == s_defaultSettings.m_bAllowExport &&
                    m_iPreferLocalDistrictsIncoming == s_defaultSettings.m_iPreferLocalDistrictsIncoming &&
                    m_iPreferLocalDistrictsOutgoing == s_defaultSettings.m_iPreferLocalDistrictsOutgoing &&
                    m_iServiceDistance == s_defaultSettings.m_iServiceDistance &&
                    m_bIncomingAllowLocalDistrict == s_defaultSettings.m_bIncomingAllowLocalDistrict &&
                    m_bIncomingAllowLocalPark == s_defaultSettings.m_bIncomingAllowLocalPark &&
                    m_bOutgoingAllowLocalDistrict == s_defaultSettings.m_bOutgoingAllowLocalDistrict &&
                    m_bOutgoingAllowLocalPark == s_defaultSettings.m_bOutgoingAllowLocalPark &&
                    m_incomingDistrictAllowed.SetEquals(s_defaultSettings.m_incomingDistrictAllowed) &&
                    m_outgoingDistrictAllowed.SetEquals(s_defaultSettings.m_outgoingDistrictAllowed) &&
                    m_incomingBuildingsAllowed.SetEquals(s_defaultSettings.m_incomingBuildingsAllowed) &&
                    m_outgoingBuildingsAllowed.SetEquals(s_defaultSettings.m_outgoingBuildingsAllowed);
        }

        public void SaveData(FastList<byte> Data)
        {
            StorageData.WriteBool(m_bAllowImport, Data);
            StorageData.WriteBool(m_bAllowExport, Data);
            StorageData.WriteInt32((int)m_iPreferLocalDistrictsIncoming, Data);
            StorageData.WriteInt32((int)m_iPreferLocalDistrictsOutgoing, Data);
            StorageData.WriteInt32(m_iServiceDistance, Data);
            StorageData.WriteBool(m_bIncomingAllowLocalDistrict, Data);
            StorageData.WriteBool(m_bIncomingAllowLocalPark, Data);
            StorageData.WriteBool(m_bOutgoingAllowLocalDistrict, Data);
            StorageData.WriteBool(m_bOutgoingAllowLocalPark, Data);

            // Incoming districts
            StorageData.WriteInt32(m_incomingDistrictAllowed.Count, Data);
            foreach (DistrictData data in m_incomingDistrictAllowed)
            {
                data.SaveData(Data);
            }

            // Outgoing districts
            StorageData.WriteInt32(m_outgoingDistrictAllowed.Count, Data);
            foreach (DistrictData data in m_outgoingDistrictAllowed)
            {
                data.SaveData(Data);
            }

            // Incoming building restrictions
            StorageData.WriteInt32(m_incomingBuildingsAllowed.Count, Data);
            foreach (ushort buildingId in m_incomingBuildingsAllowed)
            {
                StorageData.WriteUInt16(buildingId, Data);
            }

            // Outgoing building restrictions
            StorageData.WriteInt32(m_outgoingBuildingsAllowed.Count, Data);
            foreach (ushort buildingId in m_outgoingBuildingsAllowed)
            {
                StorageData.WriteUInt16(buildingId, Data);
            }
        }

        public static RestrictionSettings LoadData(int RestrictionSettingsVersion, byte[] Data, ref int iIndex)
        {
            RestrictionSettings settings = new RestrictionSettings();
            settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iServiceDistance = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bIncomingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bIncomingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bOutgoingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bOutgoingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

            // Load districts
            settings.m_incomingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
            settings.m_outgoingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);

            // Load buildings
            if (RestrictionSettingsVersion >= 2)
            {
                settings.m_incomingBuildingsAllowed = LoadBuildingsAllowed(Data, ref iIndex);
                settings.m_outgoingBuildingsAllowed = LoadBuildingsAllowed(Data, ref iIndex);
            }

            return settings;
        }

        public static HashSet<DistrictData> LoadDistrictAllowed(byte[] Data, ref int iIndex)
        {
            HashSet<DistrictData> list = new HashSet<DistrictData>();

            if (iIndex < Data.Length)
            {
                int iCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iCount; ++i)
                {
                    DistrictData data = DistrictData.LoadData(Data, ref iIndex);
                    list.Add(data);
                }
            }

            return list;
        }

        public static HashSet<ushort> LoadBuildingsAllowed(byte[] Data, ref int iIndex)
        {
            HashSet<ushort> list = new HashSet<ushort>();

            if (iIndex < Data.Length)
            {
                int iCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iCount; ++i)
                {
                    ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);
                    list.Add(buildingId);
                }
            }

            return list;
        }

        public HashSet<DistrictData> GetAllowedDistrictsIncoming(ushort buildingId, byte? district, byte? park)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(m_incomingDistrictAllowed);

            // Add current district if allowed
            if (m_bIncomingAllowLocalDistrict)
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
            if (m_bIncomingAllowLocalPark)
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

        public HashSet<DistrictData> GetAllowedDistrictsOutgoing(ushort buildingId, byte? district, byte? park)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(m_outgoingDistrictAllowed);

            // Add current district if allowed
            if (m_bOutgoingAllowLocalDistrict)
            {
                if (district is null)
                {
                    district = DistrictManager.instance.GetDistrict(building.m_position);
                }
                if (district != 0)
                {
                    list.Add(new DistrictData(DistrictData.DistrictType.District, district.Value));
                }
            }

            // Add current park if allowed
            if (m_bOutgoingAllowLocalPark)
            {
                if (park is null)
                {
                    park = DistrictManager.instance.GetPark(building.m_position);
                }
                if (park != 0)
                {
                    list.Add(new DistrictData(DistrictData.DistrictType.Park, park.Value));
                }
            }

            return list;
        }

        public bool IsIncomingDistrictAllowed(DistrictData.DistrictType eType, int district)
        {
            return m_incomingDistrictAllowed.Contains(new DistrictData(eType, district));
        }

        public void SetIncomingDistrictAllowed(DistrictData.DistrictType eType, int district, bool bChecked)
        {
            bool bContains = IsIncomingDistrictAllowed(eType, district);
            if (bChecked)
            {
                if (!bContains)
                {
                    m_incomingDistrictAllowed.Add(new DistrictData(eType, district));
                }
            }
            else
            {
                if (bContains)
                {
                    m_incomingDistrictAllowed.Remove(new DistrictData(eType, district));
                }
            }
        }

        public bool IsOutgoingDistrictAllowed(DistrictData.DistrictType eType, int district)
        {
            return m_outgoingDistrictAllowed.Contains(new DistrictData(eType, district));
        }

        public void SetOutgoingDistrictAllowed(DistrictData.DistrictType eType, int district, bool bChecked)
        {
            bool bContains = IsOutgoingDistrictAllowed(eType, district);
            if (bChecked)
            {
                if (!bContains)
                {
                    m_outgoingDistrictAllowed.Add(new DistrictData(eType, district));
                }
            }
            else
            {
                if (bContains)
                {
                    m_outgoingDistrictAllowed.Remove(new DistrictData(eType, district));
                }
            }
        }

        public string GetIncomingDistrictTooltip(ushort buildingId)
        {
            string sMessage = "";
            switch (m_iPreferLocalDistrictsIncoming)
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
            HashSet<DistrictData> list = GetAllowedDistrictsIncoming(buildingId, null, null);
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

        public string GetOutgoingDistrictTooltip(ushort buildingId)
        {
            string sMessage = "";
            switch (m_iPreferLocalDistrictsOutgoing)
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
            HashSet<DistrictData> list = GetAllowedDistrictsOutgoing(buildingId, null, null);
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

        public bool HasIncomingBuildingRestrictions()
        {
            return m_incomingBuildingsAllowed.Count > 0;
        }

        public HashSet<ushort> GetIncomingBuildingRestrictionsCopy()
        {
            return new HashSet<ushort>(m_incomingBuildingsAllowed);
        }

        public void SetIncomingBuildingRestrictions(HashSet<ushort> allowedBuildings)
        {
            m_incomingBuildingsAllowed = allowedBuildings;
        }

        public void ClearIncomingBuildingRestrictions()
        {
            m_incomingBuildingsAllowed.Clear();
        }

        public bool HasOutgoingBuildingRestrictions()
        {
            return m_outgoingBuildingsAllowed.Count > 0;
        }

        public HashSet<ushort> GetOutgoingBuildingRestrictionsCopy()
        {
            return new HashSet<ushort>(m_outgoingBuildingsAllowed);
        }

        public void SetOutgoingBuildingRestrictions(HashSet<ushort> allowedBuildings)
        {
            m_outgoingBuildingsAllowed = allowedBuildings;
        }

        public void ClearOutgoingBuildingRestrictions()
        {
            m_outgoingBuildingsAllowed.Clear();
        }

        public string DebugSettings()
        {
            string sMessage = "";

            sMessage += "\nImport:" + m_bAllowImport;
            sMessage += "\nExport:" + m_bAllowExport;
            sMessage += "\nPreferIn:" + m_iPreferLocalDistrictsIncoming;
            sMessage += "\nPreferOut:" + m_iPreferLocalDistrictsOutgoing;
            sMessage += "\nServiceDistance:" + m_iServiceDistance;
            sMessage += "\nIncomingAllowLocalDistrict:" + m_bIncomingAllowLocalDistrict;
            sMessage += "\nIncomingAllowLocalPark:" + m_bIncomingAllowLocalPark;
            sMessage += "\nOutgoingAllowLocalDistrict:" + m_bOutgoingAllowLocalDistrict;
            sMessage += "\nOutgoingAllowLocalPark:" + m_bOutgoingAllowLocalPark;
            sMessage += "\nIncomingAllowedCount:" + m_incomingDistrictAllowed.Count;
            sMessage += "\nOutgoingAllowedCount:" + m_outgoingDistrictAllowed.Count;
            sMessage += "\nIncomingBuildingsAllowedCount:" + m_incomingBuildingsAllowed.Count;
            sMessage += "\nOutgoingBuildingsAllowedCount:" + m_outgoingBuildingsAllowed.Count;

            return sMessage;
        }

        public HashSet<DistrictData> ValidateDistricts(HashSet<DistrictData> districts)
        {
            HashSet<DistrictData> newSet = new HashSet<DistrictData>();

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
                        // District doesn't exist any more
                        Debug.Log("District missing: " + districtId.m_iDistrictId);
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
                        // District doesn't exist any more
                        Debug.Log("Park missing: " + districtId.m_iDistrictId);
                    }
                }
            }

            return newSet;
        }

        public bool Validate()
        {
            // Check districts are still valid
            bool bChanged = false;

            if (m_incomingDistrictAllowed.Count > 0)
            {
                HashSet<DistrictData> newIncoming = ValidateDistricts(m_incomingDistrictAllowed);
                if (newIncoming != m_incomingDistrictAllowed)
                {
                    m_incomingDistrictAllowed = newIncoming;
                    bChanged = true;
                }

                HashSet<DistrictData> newOutgoing = ValidateDistricts(m_outgoingDistrictAllowed);
                if (newOutgoing != m_outgoingDistrictAllowed)
                {
                    m_outgoingDistrictAllowed = newOutgoing;
                    bChanged = true;
                }
            }

            return bChanged;
        }

        public bool IsDistrictRestrictionsIncomingSet()
        {
            return m_iPreferLocalDistrictsIncoming != PreferLocal.AllDistricts ||
                    m_incomingDistrictAllowed.Count > 0 ||
                    !m_bIncomingAllowLocalDistrict ||
                    !m_bIncomingAllowLocalPark;
        }

        public void ResetDistrictRestrictionsIncoming()
        {
            m_iPreferLocalDistrictsIncoming = PreferLocal.AllDistricts;
            m_incomingDistrictAllowed.Clear();
            m_bIncomingAllowLocalDistrict = true;
            m_bIncomingAllowLocalPark = true;
        }

        public bool IsDistrictRestrictionsOutgoingSet()
        {
            return m_iPreferLocalDistrictsOutgoing != PreferLocal.AllDistricts ||
                    m_outgoingDistrictAllowed.Count > 0 ||
                    !m_bOutgoingAllowLocalDistrict ||
                    !m_bOutgoingAllowLocalPark;
        }

        public void ResetDistrictRestrictionsOutgoing()
        {
            m_iPreferLocalDistrictsOutgoing = PreferLocal.AllDistricts;
            m_outgoingDistrictAllowed.Clear();
            m_bOutgoingAllowLocalDistrict = true;
            m_bOutgoingAllowLocalPark = true;
        }
    }
}