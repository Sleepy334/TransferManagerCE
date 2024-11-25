using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using static TransferManager;

namespace TransferManagerCE
{
    public class BuildingSettingsStorage
    {
        public static Dictionary<ushort, BuildingSettings> s_BuildingsSettings = new Dictionary<ushort, BuildingSettings>();
        static readonly object s_dictionaryLock = new object();

        public static BuildingSettings? GetSettings(ushort buildingId)
        {
            if (s_BuildingsSettings != null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        return s_BuildingsSettings[buildingId];
                    }
                }
            }

            return null;
        }

        public static BuildingSettings GetSettingsOrDefault(ushort buildingId)
        {
            if (s_BuildingsSettings != null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        return s_BuildingsSettings[buildingId];
                    }
                }
            }

            return new BuildingSettings();
        }

        public static void SetSettings(ushort buildingId, BuildingSettings settings)
        {
            if (s_BuildingsSettings != null)
            {
                lock (s_dictionaryLock)
                {
                    if (settings.Equals(new BuildingSettings()))
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
            if (s_BuildingsSettings != null)
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
                if (settings != null)
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
                if (settings != null)
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
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    s_BuildingsSettings.Remove(buildingId);
                }

                // Remove building from building restrictions
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

        public static string DebugSettings(ushort buildingId)
        {
            /*
            if (s_BuildingsSettings != null && s_BuildingsSettings.ContainsKey(buildingId))
            {
                BuildingSettings settings = s_BuildingsSettings[buildingId];
                return "Building: " + buildingId + settings.DebugSettings();
            }
            else
            {
                return "Not found";
            }
            */
            return "";
        }

        public static void ValidateSettings()
        {
            /*
            if (s_BuildingsSettings != null)
            {
                Dictionary<ushort, BuildingSettings> updatedSettings = new Dictionary<ushort, BuildingSettings>();

                foreach (KeyValuePair<ushort, BuildingSettings> kvp in s_BuildingsSettings)
                {
                    BuildingSettings settings = kvp.Value;
                    if (settings.Validate())
                    {
                        updatedSettings[kvp.Key] = settings;
                    }
                }

                // Now update actual settings objects
                foreach (KeyValuePair<ushort, BuildingSettings> kvp in updatedSettings)
                {
                    s_BuildingsSettings[kvp.Key] = kvp.Value;
                }
            }
            */
        }
    }
}