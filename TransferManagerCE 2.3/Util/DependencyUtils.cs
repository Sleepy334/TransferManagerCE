using ColossalFramework.Plugins;
using System.Collections.Generic;
using System.Reflection;

namespace TransferManagerCE
{
    public static class DependencyUtils
    {
        private static Dictionary<long, bool> s_pluginRunning = new Dictionary<long, bool>();
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

        public static bool IsPluginRunningNotCached(long pluginId, string sAssemblyName)
        {
            string sPluginId = pluginId.ToString();

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

        public static bool IsPluginRunning(long pluginId, string sAssemblyName)
        {
            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_pluginRunning.TryGetValue(pluginId, out bool bRunning))
                {
                    bRunning = IsPluginRunningNotCached(pluginId, sAssemblyName);
                    s_pluginRunning[pluginId] = bRunning;
                }

                return bRunning;
            }
            else
            {
                return IsPluginRunningNotCached(pluginId, sAssemblyName);
            }
        }

        public static bool IsHarmonyRunning()
        {
            // We look for either Harmony 2.2-0 steam ID or CitiesHarmony assembly name
            return IsPluginRunningNotCached(2040656402, "CitiesHarmony");
        }

        public static bool IsSmarterFireFightersRunning()
        {
            return IsPluginRunning(2346565561, "SmarterFirefighters");
        }

        public static bool IsUnifiedUIRunning()
        {
            return IsPluginRunning(2255219025, "UnifiedUILib");
        }

        public static bool IsPloppableRICORunning()
        {
            return IsPluginRunning(2016920607, "ploppablerico");
        }

        public static bool IsRepainterRunning()
        {
            return IsPluginRunning(2101551127, "Painter");
        }

        public static bool IsAdvancedBuildingLevelRunning()
        {
            return IsPluginRunning(2133705267, "AdvancedBuildingLevelControl");
        }

        public static bool IsRONRunning()
        {
            return IsPluginRunning(2405917899, "RON");
        }

        public static bool IsAdvancedOutsideConnectionsRunning()
        {
            return IsPluginRunning(2053500739, "AdvancedOutsideConnection");
        }

        public static bool IsSeniorCitizenCenterModRunning()
        {
            return IsPluginRunning(2559105223, "SeniorCitizenCenterMod");
        }

        public static bool IsEmployOverEducatedWorkersRunning()
        {
            return IsPluginRunning(1674732053, "EmployOvereducatedWorkers");
        }

        public static bool IsTaxiOverhaulRunning()
        {
            return IsPluginRunning(long.MaxValue, "TaxiOverhaul");
        }
        
        public static Assembly? GetCallAgainAssembly()
        {
            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin is not null)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        if (assembly is not null && assembly.GetName().Name.Equals("CallAgain") && plugin.isEnabled)
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
                if (plugin is not null)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        if (assembly is not null && assembly.GetName().Name.Equals("CargoFerries") && plugin.isEnabled)
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
            if (s_bNaturalDisastersDlcOwned is null)
            {
                s_bNaturalDisastersDlcOwned = SteamHelper.IsDLCOwned(SteamHelper.DLC.NaturalDisastersDLC);
            }

            return s_bNaturalDisastersDlcOwned.Value;
        }
    }
}
