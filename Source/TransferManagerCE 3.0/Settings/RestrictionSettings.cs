
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class RestrictionSettings
    {
        public const int iRESTRICTION_SETTINGS_DATA_VERSION = 5;
        private static RestrictionSettings s_defaultSettings = new RestrictionSettings();

        public enum ImportExport
        {
            ALLOW_IMPORT_AND_EXPORT,
            ALLOW_IMPORT_ONLY,
            ALLOW_EXPORT_ONLY,
            ALLOW_NEITHER,
        }

        // ----------------------------------------------------------------------------------------
        // WARNING: Update IsDefault as well.
        // Building settings
        public bool m_bAllowImport;
        public bool m_bAllowExport;
        public HashSet<ushort> m_excludedOutsideConnections;

        public DistrictRestrictionSettings m_incomingDistrictSettings;
        public DistrictRestrictionSettings m_outgoingDistrictSettings;
        public BuildingRestrictionSettings m_incomingBuildingSettings;
        public BuildingRestrictionSettings m_outgoingBuildingSettings;
        public int m_incomingServiceDistanceMeters;
        public int m_outgoingServiceDistanceMeters;

        // ----------------------------------------------------------------------------------------
        public bool IsDefault()
        {
            return m_bAllowImport == s_defaultSettings.m_bAllowImport &&
                    m_bAllowExport == s_defaultSettings.m_bAllowExport &&
                    m_excludedOutsideConnections.Equals(s_defaultSettings.m_excludedOutsideConnections) &&
                    m_incomingServiceDistanceMeters == s_defaultSettings.m_incomingServiceDistanceMeters &&
                    m_outgoingServiceDistanceMeters == s_defaultSettings.m_outgoingServiceDistanceMeters &&
                    m_incomingDistrictSettings.Equals(s_defaultSettings.m_incomingDistrictSettings) &&
                    m_outgoingDistrictSettings.Equals(s_defaultSettings.m_outgoingDistrictSettings) &&
                    m_incomingBuildingSettings.Equals(s_defaultSettings.m_incomingBuildingSettings) &&
                    m_outgoingBuildingSettings.Equals(s_defaultSettings.m_outgoingBuildingSettings);
        }

        public RestrictionSettings()
        {
            m_bAllowImport = true;
            m_bAllowExport = true;
            m_excludedOutsideConnections = new HashSet<ushort>();
            m_incomingServiceDistanceMeters = 0;
            m_outgoingServiceDistanceMeters = 0;
            m_incomingDistrictSettings = new DistrictRestrictionSettings();
            m_outgoingDistrictSettings = new DistrictRestrictionSettings();
            m_incomingBuildingSettings = new BuildingRestrictionSettings();
            m_outgoingBuildingSettings = new BuildingRestrictionSettings();
        }

        public RestrictionSettings(RestrictionSettings oSecond)
        {
            m_bAllowImport = oSecond.m_bAllowImport;
            m_bAllowExport = oSecond.m_bAllowExport;
            m_excludedOutsideConnections = new HashSet<ushort>(oSecond.m_excludedOutsideConnections);

            m_incomingServiceDistanceMeters = oSecond.m_incomingServiceDistanceMeters;
            m_outgoingServiceDistanceMeters = oSecond.m_outgoingServiceDistanceMeters;

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

        public void SaveData(FastList<byte> Data)
        {
            StorageData.WriteBool(m_bAllowImport, Data);
            StorageData.WriteBool(m_bAllowExport, Data);
            StorageData.WriteInt32((int)m_incomingDistrictSettings.m_iPreferLocalDistricts, Data);
            StorageData.WriteInt32((int)m_outgoingDistrictSettings.m_iPreferLocalDistricts, Data);
            StorageData.WriteInt32(m_incomingServiceDistanceMeters, Data);
            StorageData.WriteInt32(m_outgoingServiceDistanceMeters, Data);
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

            // Outside Connection Excludions
            StorageData.WriteInt32(m_excludedOutsideConnections.Count, Data);
            foreach (ushort buildingId in m_excludedOutsideConnections)
            {
                StorageData.WriteUInt16(buildingId, Data);
            }
        }

        public static RestrictionSettings LoadData(int RestrictionSettingsVersion, byte[] Data, ref int iIndex)
        {
            RestrictionSettings settings = new RestrictionSettings();
            settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_incomingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_outgoingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)StorageData.ReadInt32(Data, ref iIndex);

            // Distance used to be stored as km, now stored as meters
            if (RestrictionSettingsVersion <= 2)
            {
                int iDistanceKm = StorageData.ReadInt32(Data,ref iIndex);
                settings.m_incomingServiceDistanceMeters = iDistanceKm * 1000;
                settings.m_outgoingServiceDistanceMeters = iDistanceKm * 1000;
            }
            else if (RestrictionSettingsVersion == 3)
            {
                int oldServiceDistanceMeters = StorageData.ReadInt32(Data, ref iIndex);
                settings.m_incomingServiceDistanceMeters = oldServiceDistanceMeters;
                settings.m_outgoingServiceDistanceMeters = oldServiceDistanceMeters;
            }
            else
            {
                settings.m_incomingServiceDistanceMeters = StorageData.ReadInt32(Data, ref iIndex);
                settings.m_outgoingServiceDistanceMeters = StorageData.ReadInt32(Data, ref iIndex);
            }
            
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

            // Outside Connection Excludions
            if (RestrictionSettingsVersion >= 5)
            {
                int iCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iCount; ++i)
                {
                    ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);
                    settings.m_excludedOutsideConnections.Add(buildingId);
                }
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

        public string DescribeSettings(ushort buildingId, ref int iChanges)
        {
            string sMessage = "";

            // Prefer local
            if (s_defaultSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts != m_incomingDistrictSettings.m_iPreferLocalDistricts)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Incoming District Preference: {m_incomingDistrictSettings.m_iPreferLocalDistricts}");
            }
            if (s_defaultSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts != m_outgoingDistrictSettings.m_iPreferLocalDistricts)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Outgoing District Preference: {m_outgoingDistrictSettings.m_iPreferLocalDistricts}"); 
            }
            
            // Current District / Park
            if (s_defaultSettings.m_incomingDistrictSettings.m_bAllowLocalDistrict != m_incomingDistrictSettings.m_bAllowLocalDistrict)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Incoming Allow Local District: {m_incomingDistrictSettings.m_bAllowLocalDistrict}");
            }
            if (s_defaultSettings.m_incomingDistrictSettings.m_bAllowLocalPark != m_incomingDistrictSettings.m_bAllowLocalPark)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Incoming Allow Local Park: {m_incomingDistrictSettings.m_bAllowLocalPark}");
            }
            
            if (s_defaultSettings.m_outgoingDistrictSettings.m_bAllowLocalDistrict != m_outgoingDistrictSettings.m_bAllowLocalDistrict)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Outgoing Allow Local District: {m_outgoingDistrictSettings.m_bAllowLocalDistrict}");
            }
            if (s_defaultSettings.m_outgoingDistrictSettings.m_bAllowLocalPark != m_outgoingDistrictSettings.m_bAllowLocalPark)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Outgoing Allow Local Park: {m_outgoingDistrictSettings.m_bAllowLocalPark}");
            }
            
            // District restriction
            if (m_incomingDistrictSettings.m_allowedDistricts.Count > 0)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Incoming Districts Allowed: {m_incomingDistrictSettings.DescribeDistricts(buildingId)}");
            }
            if (m_outgoingDistrictSettings.m_allowedDistricts.Count > 0)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Outgoing Districts Allowed: {m_outgoingDistrictSettings.DescribeDistricts(buildingId)}");
            }

            // Building restrictions
            if (m_incomingBuildingSettings.Count > 0)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Incoming Buildings Allowed: {m_incomingBuildingSettings.Describe(buildingId)}");
            }
            if (m_outgoingBuildingSettings.Count > 0)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Outgoing Buildings Allowed: {m_outgoingBuildingSettings.Describe(buildingId)}");
            }

            // Distance restriction
            if (s_defaultSettings.m_incomingServiceDistanceMeters != m_incomingServiceDistanceMeters)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Incoming Service Distance(m): {m_incomingServiceDistanceMeters}");
            }
            // Distance restriction
            if (s_defaultSettings.m_outgoingServiceDistanceMeters != m_outgoingServiceDistanceMeters)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Outgoing Service Distance(m): {m_outgoingServiceDistanceMeters}");
            }

            // Import  / Export
            if (s_defaultSettings.m_bAllowImport != m_bAllowImport)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Allow Import: {m_bAllowImport}");
            }
            if (s_defaultSettings.m_bAllowExport != m_bAllowExport)
            {
                iChanges++;
                sMessage = Utils.AddStringsWithNewLine(sMessage, $"Allow Export: {m_bAllowExport}");
            }

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
                        // District doesn't exist any more
                        CDebug.Log("Park missing: " + districtId.m_iDistrictId);
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