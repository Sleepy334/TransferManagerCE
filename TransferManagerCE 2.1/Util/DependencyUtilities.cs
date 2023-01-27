using ColossalFramework.Plugins;
using System.Collections.Generic;
using System.Reflection;

namespace TransferManagerCE
{
    public static class DependencyUtilities
    {
        private static Dictionary<string, bool> s_pluginRunning = new Dictionary<string, bool>();
        private static bool? s_bNaturalDisastersDlcOwned = null;

        public static void SearchPlugins()
        {
            string sPlugins = "";
            foreach (PluginManager.PluginInfo oPlugin in PluginManager.instance.GetPluginsInfo())
            {
                sPlugins += oPlugin.name + " " + oPlugin.GetHashCode() + "\r\n";
            }
            Debug.Log(sPlugins);
        }

        public static bool IsPluginRunningNotCached(string sPluginId, string sAssemblyName)
        {
            bool bRunning = false;
            foreach (PluginManager.PluginInfo oPlugin in PluginManager.instance.GetPluginsInfo())
            {
                if (oPlugin.isEnabled)
                {
                    if (!string.IsNullOrEmpty(sPluginId))
                    {
                        if (oPlugin.name == sPluginId)
                        {
                            bRunning = true;
                            break;
                        };
                    }

                    if (!string.IsNullOrEmpty(sAssemblyName))
                    {
                        foreach (Assembly assembly in oPlugin.GetAssemblies())
                        {
                            if (assembly.GetName().Name.Contains(sAssemblyName))
                            {
                                bRunning = true;
                                break;
                            }
                        }
                    }
                }
            }

            return bRunning;
        }

        public static bool IsPluginRunning(string sPluginId, string sAssemblyName)
        {
            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_pluginRunning.ContainsKey(sPluginId))
                {
                    s_pluginRunning[sPluginId] = IsPluginRunningNotCached(sPluginId, sAssemblyName);
                }

                return s_pluginRunning[sPluginId];
            }
            else
            {
                return IsPluginRunningNotCached(sPluginId, sAssemblyName);
            }
        }

        public static bool IsHarmonyRunning()
        {
            // We look for either Harmony 2.2-0 steam ID or CitiesHarmony assembly name
            const string sPLUGIN_ID = "2040656402";
            return IsPluginRunningNotCached(sPLUGIN_ID, "CitiesHarmony");
        }

        public static bool IsSmarterFireFightersRunning()
        {
            const string sSMARTER_FIREFIGHTERS_ID = "2346565561";
            return IsPluginRunning(sSMARTER_FIREFIGHTERS_ID, "");
        }

        public static bool IsUnifiedUIRunning()
        {
            return IsPluginRunning("2255219025", "UnifiedUILib");
        }

        public static bool IsPloppableRICORunning()
        {
            return IsPluginRunning("2016920607", "ploppablerico");
        }

        public static bool IsRepainterRunning()
        {
            return IsPluginRunning("2101551127", "Painter");
        }

        public static bool IsAdvancedBuildingLevelRunning()
        {
            return IsPluginRunning("2133705267", "AdvancedBuildingLevelControl");
        }

        public static bool IsRONRunning()
        {
            return IsPluginRunning("2405917899", "RON");
        }

        public static bool IsAdvancedOutsideConnectionsRunning()
        {
            return IsPluginRunning("2053500739", "AdvancedOutsideConnection");
        }

        public static bool IsSeniorCitizenCenterModRunning()
        {
            return IsPluginRunning("2559105223", "SeniorCitizenCenterMod");
        }
        

        public static Assembly? GetCallAgainAssembly()
        {
            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin != null)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        if (assembly != null && assembly.GetName().Name.Equals("CallAgain") && plugin.isEnabled)
                        {
                            return assembly;
                        }
                    }
                }
            }
            return null;
        }

        public static Assembly? GetCargoFerriesAssembly()
        {
            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin != null)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        if (assembly != null && assembly.GetName().Name.Equals("CargoFerries") && plugin.isEnabled)
                        {
                            return assembly;
                        }
                    }
                }
            }
            return null;
        }

        public static bool IsNaturalDisastersDLC()
        {
            if (s_bNaturalDisastersDlcOwned == null)
            {
                s_bNaturalDisastersDlcOwned = SteamHelper.IsDLCOwned(SteamHelper.DLC.NaturalDisastersDLC);
            }

            return s_bNaturalDisastersDlcOwned.Value;
        }
    }
}
