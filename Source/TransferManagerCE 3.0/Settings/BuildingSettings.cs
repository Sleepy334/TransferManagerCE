using SleepyCommon;
using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class BuildingSettings
    {
        // Increment this every time the settings change
        // Also increment Serializer.DataVersion
        public const int iBUILDING_SETTINGS_DATA_VERSION = 15;

        // Stores a restrictions setting object for each different type of restriction supported by the building
        // eg Dead, DeadMove etc...
        public Dictionary<int, RestrictionSettings> m_restrictions = new Dictionary<int, RestrictionSettings>();
        private static BuildingSettings s_defaultSettings = new BuildingSettings();

        // Only 1 setting per building
        public int m_iOutsideMultiplier = 0; 
        public bool m_bWarehouseOverride = false;
        public bool m_bImprovedWarehouseMatching = false;
        public int m_iWarehouseReserveTrucksPercent = 20;

        public BuildingSettings()
        {
        }

        public BuildingSettings(BuildingSettings oSecond)
        {
            m_bWarehouseOverride = oSecond.m_bWarehouseOverride;
            m_bImprovedWarehouseMatching = oSecond.m_bImprovedWarehouseMatching;
            m_iWarehouseReserveTrucksPercent = oSecond.m_iWarehouseReserveTrucksPercent;
            m_iOutsideMultiplier = oSecond.m_iOutsideMultiplier;

            foreach (KeyValuePair<int, RestrictionSettings> kvp in oSecond.m_restrictions)
            {
                // Ensure we take a copy of the settings
                m_restrictions[kvp.Key] = new RestrictionSettings(kvp.Value);
            }
        }

        // We just use IsDefault() rather than implementing this
        [Obsolete("This method has not yet been implemented.", true)]
        public bool Equals(BuildingSettings oSecond)
        {
            throw new NotImplementedException();
        }

        public bool IsDefault()
        {
            //DebugSettings();

            if (m_bWarehouseOverride != s_defaultSettings.m_bWarehouseOverride)
            {
                return false;
            }
            else 
            {
                // We only check these if warehouse override is on
                if (m_bWarehouseOverride)
                {
                    if (m_bImprovedWarehouseMatching != s_defaultSettings.m_bImprovedWarehouseMatching ||
                        m_iWarehouseReserveTrucksPercent != s_defaultSettings.m_iWarehouseReserveTrucksPercent)
                    {
                        return false;
                    }
                }
            }

            if (m_iOutsideMultiplier != s_defaultSettings.m_iOutsideMultiplier)
            {
                return false;
            }

            // Check restrictions as well.
            foreach (KeyValuePair<int, RestrictionSettings> kvp in m_restrictions)
            {
                if (!kvp.Value.IsDefault())
                {
                    return false;
                }
            }

            return true;
        }

        public void SaveData(FastList<byte> Data)
        {
            // Write variables
            StorageData.WriteBool(m_bWarehouseOverride, Data);
            StorageData.WriteBool(false, Data); // bWarehouseFirstNotUsed
            StorageData.WriteInt32(m_iWarehouseReserveTrucksPercent, Data);
            StorageData.WriteInt32(m_iOutsideMultiplier, Data);

            // Write out restrictions
            StorageData.WriteInt32(RestrictionSettings.iRESTRICTION_SETTINGS_DATA_VERSION, Data);
            StorageData.WriteInt32(m_restrictions.Count, Data);

            foreach (KeyValuePair<int, RestrictionSettings> kvp in m_restrictions)
            {
                StorageData.WriteInt32(kvp.Key, Data);
                kvp.Value.SaveData(Data);
            }

            StorageData.WriteBool(m_bImprovedWarehouseMatching, Data);
        }

        public void LoadData(int BuildingSettingsVersion, byte[] Data, ref int iIndex)
        {
            if (BuildingSettingsVersion < 11)
            {
                // Old versions were read in BuildingSettingsSerializer
                throw new Exception($"BuildingSettings.LoadData version not supported");
            }

            // Load variables
            m_bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            bool bWarehouseFirstNotUsed = StorageData.ReadBool(Data, ref iIndex);
            m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            m_iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex);

            // Restriction version was added in BuildingSettings version 12.
            int iRestrctionVersion = 1;
            if (BuildingSettingsVersion >= 12)
            {
                iRestrctionVersion = StorageData.ReadInt32(Data, ref iIndex);
            }

            // Load restrictions
            int iRestrictionCount = StorageData.ReadInt32(Data, ref iIndex);
            for (int i = 0; i < iRestrictionCount; i++)
            {
                int iRestrictionId = StorageData.ReadInt32(Data, ref iIndex);
                m_restrictions[iRestrictionId] = RestrictionSettings.LoadData(iRestrctionVersion, Data, ref iIndex);
            }

            if (BuildingSettingsVersion >= 13)
            {
                // Introduced in 13
                m_bImprovedWarehouseMatching = StorageData.ReadBool(Data, ref iIndex);
            }
        }

        public bool HasRestrictionSettings()
        {
            return m_restrictions.Count > 0;
        }

        public int GetRestrictionCount()
        {
            return m_restrictions.Count;
        }

        public RestrictionSettings? GetRestrictions(int iRestrictionId)
        {
            if (m_restrictions.ContainsKey(iRestrictionId))
            {
                return m_restrictions[iRestrictionId];
            }

            return null;
        }

        public RestrictionSettings GetRestrictionsOrDefault(int iRestrictionId)
        {
            if (m_restrictions.ContainsKey(iRestrictionId))
            {
                return m_restrictions[iRestrictionId];
            }
            else
            {
                return new RestrictionSettings();
            }
        }

        public void SetRestrictions(int iRestrictionId, RestrictionSettings settings)
        {
            // Save a copy not the pointer
            SetRestrictionsDirect(iRestrictionId, new RestrictionSettings(settings));
        }

        public void SetRestrictionsDirect(int iRestrictionId, RestrictionSettings settings)
        {
            if (settings.IsDefault())
            {
                if (m_restrictions.ContainsKey(iRestrictionId))
                {
                    // Default values, just remove settings
                    m_restrictions.Remove(iRestrictionId);
                }
            }
            else
            {
                // Directly save pointer, please ensure this settings object isnt shared
                m_restrictions[iRestrictionId] = settings;
            }
        }

        public bool IsImprovedWarehouseMatching()
        {
            if (m_bWarehouseOverride)
            {
                return m_bImprovedWarehouseMatching;
            }
            return SaveGameSettings.GetSettings().ImprovedWarehouseMatching;
        }
        
        public int ReserveCargoTrucksPercent()
        {
            if (m_bWarehouseOverride)
            {
                return m_iWarehouseReserveTrucksPercent;
            }
            return SaveGameSettings.GetSettings().WarehouseReserveTrucksPercent;
        }

        public void ReserveCargoTrucksPercent(int iPercent)
        {
            m_iWarehouseReserveTrucksPercent = iPercent;
        }

        public bool ReleaseBuilding(ushort buildingId)
        {
            bool bChanged = false;
            foreach (KeyValuePair<int, RestrictionSettings> kvp in m_restrictions)
            {
                RestrictionSettings settings = kvp.Value;

                // Check incoming building restrictions
                HashSet<ushort> incoming = settings.m_incomingBuildingSettings.GetBuildingRestrictionsCopy();
                if (incoming.Contains(buildingId))
                {
                    bChanged = true;
                    incoming.Remove(buildingId);
                    settings.m_incomingBuildingSettings.SetBuildingRestrictions(incoming);
                }

                // Check outgoing building restrictions
                HashSet<ushort> outgoing = settings.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                if (outgoing.Contains(buildingId))
                {
                    bChanged = true;
                    outgoing.Remove(buildingId);
                    settings.m_outgoingBuildingSettings.SetBuildingRestrictions(outgoing);
                }
            }
            return bChanged;
        }

        public string DescribeSettings(ushort buildingId, ref int iChanges)
        {
            // Restrictions as well
            string sRestrictions = "";
            foreach (KeyValuePair<int, RestrictionSettings> kvp in m_restrictions)
            {
                if (!kvp.Value.IsDefault())
                {
                    sRestrictions = Utils.AddStringsWithNewLine(sRestrictions, $"Restriction Id: {kvp.Key}");
                    sRestrictions = Utils.AddStringsWithNewLine(sRestrictions, kvp.Value.DescribeSettings(buildingId, ref iChanges));
                }
            }

            // Warehouse settings
            string sWarehouse = "";
            if (s_defaultSettings.m_bWarehouseOverride != m_bWarehouseOverride)
            {
                iChanges++;
                sWarehouse = Utils.AddStringsWithNewLine(sWarehouse, $"Warehouse Override: {m_bWarehouseOverride}");

                if (m_bWarehouseOverride)
                {
                    if (s_defaultSettings.m_iWarehouseReserveTrucksPercent != m_iWarehouseReserveTrucksPercent)
                    {
                        iChanges++;
                        sWarehouse = Utils.AddStringsWithNewLine(sWarehouse, $"Warehouse Reserve Trucks (%): {m_iWarehouseReserveTrucksPercent}");
                    }
                    if (s_defaultSettings.m_bImprovedWarehouseMatching != m_bImprovedWarehouseMatching)
                    {
                        iChanges++;
                        sWarehouse = Utils.AddStringsWithNewLine(sWarehouse, $"Improved Warehouse Matching: {m_bImprovedWarehouseMatching}");
                    }
                }
            }

            // Outside multiplier
            string sOutside = "";
            if (s_defaultSettings.m_iOutsideMultiplier != m_iOutsideMultiplier)
            {
                iChanges++;
                sOutside = Utils.AddStringsWithNewLine(sOutside, $"Outside Multiplier: {m_iOutsideMultiplier}");
            }

            string sMessage = "";

            if (sRestrictions.Length > 0)
            {
                sMessage = Utils.AddStringsWithNewLine(sMessage, sRestrictions);
            }

            if (sWarehouse.Length > 0)
            {
                sMessage = Utils.AddStringsWithNewLine(sMessage, "");
                sMessage = Utils.AddStringsWithNewLine(sMessage, "Warehouse Restrictions:");
                sMessage = Utils.AddStringsWithNewLine(sMessage, sWarehouse);
            }

            if (sOutside.Length > 0)
            {
                sMessage = Utils.AddStringsWithNewLine(sMessage, "");
                sMessage = Utils.AddStringsWithNewLine(sMessage, "Outside Connection Restrictions:");
                sMessage = Utils.AddStringsWithNewLine(sMessage, sOutside);
            }

            return sMessage;
        }
    }
}