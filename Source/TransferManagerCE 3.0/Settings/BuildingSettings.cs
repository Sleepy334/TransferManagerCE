using SleepyCommon;
using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class BuildingSettings
    {
        // Increment this every time the settings change
        // Also increment Serializer.DataVersion
        public const int iBUILDING_SETTINGS_DATA_VERSION = 20;

        // Stores a restrictions setting object for each different type of restriction supported by the building
        // eg Dead, DeadMove etc...
        public Dictionary<int, RestrictionSettings> m_restrictions = new Dictionary<int, RestrictionSettings>();
        private static BuildingSettings s_defaultSettings = new BuildingSettings();

        // Only 1 setting per building
        public int m_iCargoOutsidePriority = -1; // -1 = OFF
        public int m_iCitizenOutsidePriority = -1; // -1 = OFF
        public int m_iWarehouseReserveTrucksPercent = -1; // -1 = OFF

        public BuildingSettings()
        {
        }

        public BuildingSettings(BuildingSettings oSecond)
        {
            m_iWarehouseReserveTrucksPercent = oSecond.m_iWarehouseReserveTrucksPercent;
            m_iCargoOutsidePriority = oSecond.m_iCargoOutsidePriority;
            m_iCitizenOutsidePriority = oSecond.m_iCitizenOutsidePriority;

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

            if (m_iWarehouseReserveTrucksPercent != s_defaultSettings.m_iWarehouseReserveTrucksPercent)
            {
                return false;
            }

            if (m_iCargoOutsidePriority != s_defaultSettings.m_iCargoOutsidePriority)
            {
                return false;
            }

            if (m_iCitizenOutsidePriority != s_defaultSettings.m_iCitizenOutsidePriority)
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
            StorageData.WriteBool(false, Data); // BWarehouseOverride - Not used
            StorageData.WriteBool(false, Data); // bWarehouseFirstNotUsed
            StorageData.WriteInt32(m_iWarehouseReserveTrucksPercent, Data);
            
            StorageData.WriteInt32(m_iCargoOutsidePriority, Data); // Added in v18
            StorageData.WriteInt32(m_iCitizenOutsidePriority, Data); // Added in v18

            // Write out restrictions
            StorageData.WriteInt32(RestrictionSettings.iRESTRICTION_SETTINGS_DATA_VERSION, Data);
            StorageData.WriteInt32(m_restrictions.Count, Data);

            foreach (KeyValuePair<int, RestrictionSettings> kvp in m_restrictions)
            {
                StorageData.WriteInt32(kvp.Key, Data);
                kvp.Value.SaveData(Data);
            }

            StorageData.WriteBool(false, Data); // v20 - Improved warehouse matching no longer used
            
        }

        public void LoadData(int BuildingSettingsVersion, byte[] Data, ref int iIndex)
        {
            if (BuildingSettingsVersion < 11)
            {
                // Old versions were read in BuildingSettingsSerializer
                throw new Exception($"BuildingSettings.LoadData version not supported");
            }

            // Load variables
            bool bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            bool bWarehouseFirstNotUsed = StorageData.ReadBool(Data, ref iIndex);
            
            m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            if (BuildingSettingsVersion < 20 && !bWarehouseOverride)
            {
                m_iWarehouseReserveTrucksPercent = -1; // We now set this to -1 to indicate OFF
            }

            if (BuildingSettingsVersion >= 18)
            {
                m_iCargoOutsidePriority = StorageData.ReadInt32(Data, ref iIndex);
            } 
            else
            {
                // No longer used, replaced by m_iCargoOutsidePriority in version 18
                int iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex); 
            }

            if (BuildingSettingsVersion >= 19)
            {
                m_iCitizenOutsidePriority = StorageData.ReadInt32(Data, ref iIndex);
            }

            // ------------------------------------------------------------------------------------
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

            // ------------------------------------------------------------------------------------
            if (BuildingSettingsVersion >= 13)
            {
                // Introduced in 13
                bool bImprovedWarehouseMatching = StorageData.ReadBool(Data, ref iIndex);
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
        
        public int ReserveCargoTrucksPercent()
        {
            if (m_iWarehouseReserveTrucksPercent == -1)
            {
                return SaveGameSettings.GetSettings().WarehouseReserveTrucksPercent;
            }
            else
            {
                return m_iWarehouseReserveTrucksPercent;
            }
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
                    settings.m_incomingBuildingSettings.SetBuildingRestrictionsDirect(incoming);
                }

                // Check outgoing building restrictions
                HashSet<ushort> outgoing = settings.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                if (outgoing.Contains(buildingId))
                {
                    bChanged = true;
                    outgoing.Remove(buildingId);
                    settings.m_outgoingBuildingSettings.SetBuildingRestrictionsDirect(outgoing);
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
            if (s_defaultSettings.m_iWarehouseReserveTrucksPercent != m_iWarehouseReserveTrucksPercent)
            {
                iChanges++;
                sWarehouse = Utils.AddStringsWithNewLine(sWarehouse, $"Warehouse Reserve Trucks (%): {m_iWarehouseReserveTrucksPercent}");
            }

            // Outside priority
            string sOutside = "";
            if (s_defaultSettings.m_iCargoOutsidePriority != m_iCargoOutsidePriority)
            {
                iChanges++;
                sOutside = Utils.AddStringsWithNewLine(sOutside, $"Outside Cargo Priority: {m_iCargoOutsidePriority}");
            }
            if (s_defaultSettings.m_iCitizenOutsidePriority != m_iCitizenOutsidePriority)
            {
                iChanges++;
                sOutside = Utils.AddStringsWithNewLine(sOutside, $"Outside Citizen Priority: {m_iCitizenOutsidePriority}");
            }

            // Build message
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

        public void Validate()
        {
            foreach (RestrictionSettings settings in m_restrictions.Values) 
            {
                settings.Validate();
            }
        }
    }
}