
using System;
using System.Collections.Generic;
using TransferManagerCE.Common;

namespace TransferManagerCE
{
    public class RestrictionSettings : IEquatable<RestrictionSettings>
    {
        public const int iRESTRICTION_SETTINGS_DATA_VERSION = 2;

        public enum PreferLocal
        {
            ALL_DISTRICTS = 0,
            PREFER_LOCAL_DISTRICT = 1,
            RESTRICT_LOCAL_DISTRICT = 2,
            ALL_DISTRICTS_EXCEPT_FOR = 3,
            UNKNOWN = 255,
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
        public HashSet<ushort> m_incomingBuildingsAllowed;
        public HashSet<ushort> m_outgoingBuildingsAllowed;

        public RestrictionSettings()
        {
            m_bAllowImport = true;
            m_bAllowExport = true;

            m_iPreferLocalDistrictsIncoming = PreferLocal.ALL_DISTRICTS;
            m_iPreferLocalDistrictsOutgoing = PreferLocal.ALL_DISTRICTS;
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

        public bool Equals(RestrictionSettings oSecond)
        {
            return m_bAllowImport == oSecond.m_bAllowImport &&
                    m_bAllowExport == oSecond.m_bAllowExport &&
                    m_iPreferLocalDistrictsIncoming == oSecond.m_iPreferLocalDistrictsIncoming &&
                    m_iPreferLocalDistrictsOutgoing == oSecond.m_iPreferLocalDistrictsOutgoing &&
                    m_iServiceDistance == oSecond.m_iServiceDistance &&
                    m_bIncomingAllowLocalDistrict == oSecond.m_bIncomingAllowLocalDistrict &&
                    m_bIncomingAllowLocalPark == oSecond.m_bIncomingAllowLocalPark &&
                    m_bOutgoingAllowLocalDistrict == oSecond.m_bOutgoingAllowLocalDistrict &&
                    m_bOutgoingAllowLocalPark == oSecond.m_bOutgoingAllowLocalPark &&
                    m_incomingDistrictAllowed == oSecond.m_incomingDistrictAllowed &&
                    m_outgoingDistrictAllowed == oSecond.m_outgoingDistrictAllowed &&
                    m_incomingBuildingsAllowed == oSecond.m_incomingBuildingsAllowed &&
                    m_outgoingBuildingsAllowed == oSecond.m_outgoingBuildingsAllowed;
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

        public HashSet<DistrictData> GetAllowedDistrictsIncoming(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(m_incomingDistrictAllowed);

            // Add current district if allowed
            if (m_bIncomingAllowLocalDistrict)
            {
                byte district = DistrictManager.instance.GetDistrict(building.m_position);
                if (district != 0)
                {
                    DistrictData data = new DistrictData(DistrictData.DistrictType.District, district);
                    if (!list.Contains(data))
                    {
                        list.Add(data);
                    }
                }
            }

            // Add current park if allowed
            if (m_bIncomingAllowLocalPark)
            {
                byte park = DistrictManager.instance.GetPark(building.m_position);
                if (park != 0)
                {
                    DistrictData data = new DistrictData(DistrictData.DistrictType.Park, park);
                    if (!list.Contains(data))
                    {
                        list.Add(data);
                    }
                }
            }

            return list;
        }

        public HashSet<DistrictData> GetAllowedDistrictsOutgoing(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(m_outgoingDistrictAllowed);

            // Add current district if allowed
            if (m_bOutgoingAllowLocalDistrict)
            {
                byte district = DistrictManager.instance.GetDistrict(building.m_position);
                if (district != 0)
                {
                    list.Add(new DistrictData(DistrictData.DistrictType.District, district));
                }
            }

            // Add current park if allowed
            if (m_bOutgoingAllowLocalPark)
            {
                byte park = DistrictManager.instance.GetPark(building.m_position);
                if (park != 0)
                {
                    list.Add(new DistrictData(DistrictData.DistrictType.Park, park));
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
                case PreferLocal.ALL_DISTRICTS:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        sMessage += "\r\n- " + Localization.Get("dropdownBuildingPanelPreferLocal1");
                        return sMessage;
                    }
                case PreferLocal.PREFER_LOCAL_DISTRICT:
                case PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        break;
                    }
                case PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        sMessage += Localization.Get("txtBlockedDistricts") + ":";
                        break;
                    }
            }

            // Add allowed districts to message
            HashSet<DistrictData> list = GetAllowedDistrictsIncoming(buildingId);
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
                case PreferLocal.ALL_DISTRICTS:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        sMessage += "\r\n- " + Localization.Get("dropdownBuildingPanelPreferLocal1");
                        return sMessage;
                    }
                case PreferLocal.PREFER_LOCAL_DISTRICT:
                case PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        break;
                    }
                case PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        sMessage += Localization.Get("txtBlockedDistricts") + ":";
                        break;
                    }
            }

            // Add allowed districts to message
            HashSet<DistrictData> list = GetAllowedDistrictsOutgoing(buildingId);
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

        

        public string DebugSettings()
        {
            string sMessage = "";

            sMessage += " Import:" + m_bAllowImport;
            sMessage += " Export:" + m_bAllowExport;
            sMessage += " PreferIn:" + m_iPreferLocalDistrictsIncoming;
            sMessage += " PreferOut:" + m_iPreferLocalDistrictsOutgoing;
            sMessage += " ServiceDistance:" + m_iServiceDistance;
            sMessage += " IncomingAllowLocalDistrict:" + m_bIncomingAllowLocalDistrict;
            sMessage += " IncomingAllowLocalPark:" + m_bIncomingAllowLocalPark;
            sMessage += " OutgoingAllowLocalDistrict:" + m_bOutgoingAllowLocalDistrict;
            sMessage += " OutgoingAllowLocalPark:" + m_bOutgoingAllowLocalPark;

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
            return m_iPreferLocalDistrictsIncoming != PreferLocal.ALL_DISTRICTS ||
                    m_incomingDistrictAllowed.Count > 0 ||
                    !m_bIncomingAllowLocalDistrict ||
                    !m_bIncomingAllowLocalPark;
        }

        public void ResetDistrictRestrictionsIncoming()
        {
            m_iPreferLocalDistrictsIncoming = PreferLocal.ALL_DISTRICTS;
            m_incomingDistrictAllowed.Clear();
            m_bIncomingAllowLocalDistrict = true;
            m_bIncomingAllowLocalPark = true;
        }

        public bool IsDistrictRestrictionsOutgoingSet()
        {
            return m_iPreferLocalDistrictsOutgoing != PreferLocal.ALL_DISTRICTS ||
                    m_outgoingDistrictAllowed.Count > 0 ||
                    !m_bOutgoingAllowLocalDistrict ||
                    !m_bOutgoingAllowLocalPark;
        }

        public void ResetDistrictRestrictionsOutgoing()
        {
            m_iPreferLocalDistrictsOutgoing = PreferLocal.ALL_DISTRICTS;
            m_outgoingDistrictAllowed.Clear();
            m_bOutgoingAllowLocalDistrict = true;
            m_bOutgoingAllowLocalPark = true;
        }
    }
}