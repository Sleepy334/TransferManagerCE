using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using static TransferManager;

namespace TransferManagerCE
{
    public class BuildingSettingsStorage
    {
        public static Dictionary<ushort, BuildingSettings> s_BuildingsSettings = new Dictionary<ushort, BuildingSettings>();
        private static readonly object s_dictionaryLock = new object();
        private static BuildingSettings s_defaultSettings = new BuildingSettings();

        public static BuildingSettings? GetSettings(ushort buildingId)
        {
            if (s_BuildingsSettings is not null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.TryGetValue(buildingId, out BuildingSettings settings))
                    {
                        return settings;
                    }
                }
            }

            return null;
        }

        public static BuildingSettings GetSettingsOrDefault(ushort buildingId)
        {
            if (s_BuildingsSettings is not null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.TryGetValue(buildingId, out BuildingSettings settings))
                    {
                        return settings;
                    }
                }
            }

            return new BuildingSettings();
        }

        public static void SetSettings(ushort buildingId, BuildingSettings settings)
        {
            if (s_BuildingsSettings is not null)
            {
                lock (s_dictionaryLock)
                {
                    if (settings.Equals(s_defaultSettings))
                    {
                        if (s_BuildingsSettings.ContainsKey(buildingId))
                        {
                            // Default values, just remove settings
                            s_BuildingsSettings.Remove(buildingId);
                        }
                    }
                    else
                    {
                        // Save a copy not the pointer
                        s_BuildingsSettings[buildingId] = new BuildingSettings(settings);
                    }
                }
            }
        }

        public static void ClearSettings()
        {
            if (s_BuildingsSettings is not null)
            {
                lock (s_dictionaryLock)
                {
                    s_BuildingsSettings.Clear();
                }
            }
        }

        public static RestrictionSettings? GetRestrictions(ushort buildingId, BuildingTypeHelper.BuildingType eType, TransferReason material)
        {
            int iRestrictionId = BuildingRuleSets.GetRestrictionId(eType, material);
            if (iRestrictionId != -1)
            {
                BuildingSettings? settings = GetSettings(buildingId);
                if (settings is not null)
                {
                    return settings.GetRestrictions(iRestrictionId);
                }
            }

            return null;
        }

        public static RestrictionSettings GetRestrictionsOrDefault(ushort buildingId, BuildingTypeHelper.BuildingType eType, TransferReason material)
        {
            int iRestrictionId = BuildingRuleSets.GetRestrictionId(eType, material);
            if (iRestrictionId != -1)
            {
                BuildingSettings? settings = GetSettings(buildingId);
                if (settings is not null)
                {
                    return settings.GetRestrictionsOrDefault(iRestrictionId);
                }
            }

            // Return default settings
            return new RestrictionSettings();
        }

        // Hooks into BuildingManager do not change
        public static void ReleaseBuilding(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                // Remove settings for this building
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    s_BuildingsSettings.Remove(buildingId);
                }

                // Remove this building from building restrictions arrays
                Dictionary<ushort, BuildingSettings> changedSettings = new Dictionary<ushort, BuildingSettings>();
                foreach (var kvp in s_BuildingsSettings)
                {
                    BuildingSettings settings = kvp.Value;
                    if (settings.ReleaseBuilding(buildingId))
                    {
                        // Store changed settings in a temp as we can't update original in a foreach loop.
                        changedSettings[kvp.Key] = settings;
                    }
                }

                // Update original settings
                foreach (var kvp in changedSettings)
                {
                    s_BuildingsSettings[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}