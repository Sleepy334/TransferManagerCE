using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class BuildingSettings
    {
        // Increment this every time the settings change
        // Also increment Serializer.DataVersion
        public const int iBUILDING_SETTINGS_DATA_VERSION = 14;

        // Stores a restrictions setting object for each different type of restriction supported by the building
        // eg Dead, DeadMove etc...
        public Dictionary<int, RestrictionSettings> m_restrictions = new Dictionary<int, RestrictionSettings>();

        // Only 1 setting per building
        public int m_iOutsideMultiplier = 0; 
        public bool m_bWarehouseOverride = false;
        public bool m_bWarehouseFirst = SaveGameSettings.GetSettings().WarehouseFirst;
        public bool m_bImprovedWarehouseMatching = SaveGameSettings.GetSettings().ImprovedWarehouseMatching;
        public int m_iWarehouseReserveTrucksPercent = SaveGameSettings.GetSettings().WarehouseReserveTrucksPercent;

        public BuildingSettings()
        {
        }

        public BuildingSettings(BuildingSettings oSecond)
        {
            m_bWarehouseOverride = oSecond.m_bWarehouseOverride;
            m_bWarehouseFirst = oSecond.m_bWarehouseFirst;
            m_bImprovedWarehouseMatching = oSecond.m_bImprovedWarehouseMatching;
            m_iWarehouseReserveTrucksPercent = oSecond.m_iWarehouseReserveTrucksPercent;
            m_iOutsideMultiplier = oSecond.m_iOutsideMultiplier;

            foreach (KeyValuePair<int, RestrictionSettings> kvp in oSecond.m_restrictions)
            {
                // Ensure we take a copy of the settings
                m_restrictions[kvp.Key] = new RestrictionSettings(kvp.Value);
            }
        }

        public bool Equals(BuildingSettings oSecond)
        {
            bool bResult =
                    m_bWarehouseOverride == oSecond.m_bWarehouseOverride &&
                    m_bWarehouseFirst == oSecond.m_bWarehouseFirst &&
                    m_bImprovedWarehouseMatching == oSecond.m_bImprovedWarehouseMatching &&
                    m_iWarehouseReserveTrucksPercent == oSecond.m_iWarehouseReserveTrucksPercent &&
                    m_iOutsideMultiplier == oSecond.m_iOutsideMultiplier;
            if (bResult)
            {
                return m_restrictions.Equals(oSecond.m_restrictions);
            }
            return false;
        }

        public void SaveData(FastList<byte> Data)
        {
            // Write variables
            StorageData.WriteBool(m_bWarehouseOverride, Data);
            StorageData.WriteBool(m_bWarehouseFirst, Data);
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
            m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
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

        public bool Contains(int iRestrictionId)
        {
            return m_restrictions.ContainsKey(iRestrictionId);
        }

        public bool HasRestrictions(int iRestrictionId)
        {
            return m_restrictions.ContainsKey(iRestrictionId);
        }

        public RestrictionSettings GetRestrictions(int iRestrictionId)
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
            // Save a copy so we don't end up duplicating settings
            SetRestrictionsDirect(iRestrictionId, new RestrictionSettings(settings));
        }

        public void SetRestrictionsDirect(int iRestrictionId, RestrictionSettings settings)
        {
            // Directly save settings object, we assume it's already a stand alone copy.
            m_restrictions[iRestrictionId] = settings;
        }

        public bool IsWarehouseFirst()
        {
            if (m_bWarehouseOverride)
            {
                return m_bWarehouseFirst;
            }
            return SaveGameSettings.GetSettings().WarehouseFirst;
        }

        public bool IsImprovedWarehouseMatching()
        {
            if (m_bWarehouseOverride)
            {
                return m_bImprovedWarehouseMatching;
            }
            return SaveGameSettings.GetSettings().WarehouseFirst;
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
                HashSet<ushort> incoming = settings.GetIncomingBuildingRestrictionsCopy();
                if (incoming.Contains(buildingId))
                {
                    bChanged = true;
                    incoming.Remove(buildingId);
                    settings.SetIncomingBuildingRestrictions(incoming);
                }

                // Check outgoing building restrictions
                HashSet<ushort> outgoing = settings.GetOutgoingBuildingRestrictionsCopy();
                if (outgoing.Contains(buildingId))
                {
                    bChanged = true;
                    outgoing.Remove(buildingId);
                    settings.SetOutgoingBuildingRestrictions(outgoing);
                }
            }
            return bChanged;
        }
    }
}