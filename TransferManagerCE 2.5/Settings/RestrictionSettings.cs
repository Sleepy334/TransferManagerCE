
using System;
using System.Collections.Generic;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class RestrictionSettings
    {
        public const int iRESTRICTION_SETTINGS_DATA_VERSION = 2;
        private static RestrictionSettings s_defaultSettings = new RestrictionSettings();

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

        public DistrictRestrictionSettings m_incomingDistrictSettings;
        public DistrictRestrictionSettings m_outgoingDistrictSettings;
        public BuildingRestrictionSettings m_incomingBuildingSettings;
        public BuildingRestrictionSettings m_outgoingBuildingSettings;
        public int m_iServiceDistance;

        public RestrictionSettings()
        {
            m_bAllowImport = true;
            m_bAllowExport = true;
            m_iServiceDistance = 0;
            m_incomingDistrictSettings = new DistrictRestrictionSettings();
            m_outgoingDistrictSettings = new DistrictRestrictionSettings();
            m_incomingBuildingSettings = new BuildingRestrictionSettings();
            m_outgoingBuildingSettings = new BuildingRestrictionSettings();
        }

        public RestrictionSettings(RestrictionSettings oSecond)
        {
            m_bAllowImport = oSecond.m_bAllowImport;
            m_bAllowExport = oSecond.m_bAllowExport;
            m_iServiceDistance = oSecond.m_iServiceDistance;
            m_incomingDistrictSettings = new DistrictRestrictionSettings(oSecond.m_incomingDistrictSettings);
            m_outgoingDistrictSettings = new DistrictRestrictionSettings(oSecond.m_outgoingDistrictSettings);
            m_incomingBuildingSettings = new BuildingRestrictionSettings(oSecond.m_incomingBuildingSettings);
            m_outgoingBuildingSettings = new BuildingRestrictionSettings(oSecond.m_outgoingBuildingSettings);
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
                    m_iServiceDistance == s_defaultSettings.m_iServiceDistance &&
                    m_incomingDistrictSettings.Equals(s_defaultSettings.m_incomingDistrictSettings) &&
                    m_outgoingDistrictSettings.Equals(s_defaultSettings.m_outgoingDistrictSettings) &&
                    m_incomingBuildingSettings.Equals(s_defaultSettings.m_incomingBuildingSettings) &&
                    m_outgoingBuildingSettings.Equals(s_defaultSettings.m_outgoingBuildingSettings);
        }

        public void SaveData(FastList<byte> Data)
        {
            StorageData.WriteBool(m_bAllowImport, Data);
            StorageData.WriteBool(m_bAllowExport, Data);
            StorageData.WriteInt32((int)m_incomingDistrictSettings.m_iPreferLocalDistricts, Data);
            StorageData.WriteInt32((int)m_outgoingDistrictSettings.m_iPreferLocalDistricts, Data);
            StorageData.WriteInt32(m_iServiceDistance, Data);
            StorageData.WriteBool(m_incomingDistrictSettings.m_bAllowLocalDistrict, Data);
            StorageData.WriteBool(m_incomingDistrictSettings.m_bAllowLocalPark, Data);
            StorageData.WriteBool(m_outgoingDistrictSettings.m_bAllowLocalDistrict, Data);
            StorageData.WriteBool(m_outgoingDistrictSettings.m_bAllowLocalPark, Data);

            // Incoming districts
            StorageData.WriteInt32(m_incomingDistrictSettings.m_allowedDistricts.Count, Data);
            foreach (DistrictData data in m_incomingDistrictSettings.m_allowedDistricts)
            {
                data.SaveData(Data);
            }

            // Outgoing districts
            StorageData.WriteInt32(m_outgoingDistrictSettings.m_allowedDistricts.Count, Data);
            foreach (DistrictData data in m_outgoingDistrictSettings.m_allowedDistricts)
            {
                data.SaveData(Data);
            }

            // Building restrictions
            m_incomingBuildingSettings.Write(Data);
            m_outgoingBuildingSettings.Write(Data);
        }

        public static RestrictionSettings LoadData(int RestrictionSettingsVersion, byte[] Data, ref int iIndex)
        {
            RestrictionSettings settings = new RestrictionSettings();
            settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_incomingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_outgoingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iServiceDistance = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_incomingDistrictSettings.m_bAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            settings.m_incomingDistrictSettings.m_bAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
            settings.m_outgoingDistrictSettings.m_bAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            settings.m_outgoingDistrictSettings.m_bAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

            // Load districts
            settings.m_incomingDistrictSettings.m_allowedDistricts = LoadDistrictAllowed(Data, ref iIndex);
            settings.m_outgoingDistrictSettings.m_allowedDistricts = LoadDistrictAllowed(Data, ref iIndex);

            // Load buildings
            if (RestrictionSettingsVersion >= 2)
            {
                settings.m_incomingBuildingSettings.Read(Data, ref iIndex);
                settings.m_outgoingBuildingSettings.Read(Data, ref iIndex);
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

        public string DebugSettings()
        {
            string sMessage = "";

            sMessage += "\nImport:" + m_bAllowImport;
            sMessage += "\nExport:" + m_bAllowExport;
            sMessage += "\nPreferIn:" + m_incomingDistrictSettings.m_iPreferLocalDistricts;
            sMessage += "\nPreferOut:" + m_outgoingDistrictSettings.m_iPreferLocalDistricts;
            sMessage += "\nServiceDistance:" + m_iServiceDistance;
            sMessage += "\nIncomingAllowLocalDistrict:" + m_incomingDistrictSettings.m_bAllowLocalDistrict;
            sMessage += "\nIncomingAllowLocalPark:" + m_incomingDistrictSettings.m_bAllowLocalPark;
            sMessage += "\nOutgoingAllowLocalDistrict:" + m_outgoingDistrictSettings.m_bAllowLocalDistrict;
            sMessage += "\nOutgoingAllowLocalPark:" + m_outgoingDistrictSettings.m_bAllowLocalPark;
            sMessage += "\nIncomingAllowedCount:" + m_incomingDistrictSettings.m_allowedDistricts.Count;
            sMessage += "\nOutgoingAllowedCount:" + m_outgoingDistrictSettings.m_allowedDistricts.Count;
            sMessage += "\nIncomingBuildingsAllowedCount:" + m_incomingBuildingSettings.Count;
            sMessage += "\nOutgoingBuildingsAllowedCount:" + m_outgoingBuildingSettings.Count;

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

            if (m_incomingDistrictSettings.Validate())
            {
                bChanged = true;
            }

            if (m_outgoingDistrictSettings.Validate())
            {
                bChanged = true;
            }

            return bChanged;
        }
    }
}