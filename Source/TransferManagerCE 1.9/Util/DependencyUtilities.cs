using ColossalFramework.Plugins;
using System.Collections.Generic;
using System.Reflection;

namespace TransferManagerCE
{
    public static class DependencyUtilities
    {
        private static Dictionary<string, bool> s_pluginRunning = new Dictionary<string, bool>();

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
            const string sUNIFIED_UI_ID = "2255219025";
            return IsPluginRunning(sUNIFIED_UI_ID, "");
        }

        public static bool IsPloppableRICORunning()
        {
            const string sMOD_ID = "2016920607";
            const string sAssemblyName = "ploppablerico";
            return IsPluginRunning(sMOD_ID, sAssemblyName);
        }

        public static bool IsRepainterRunning()
        {
            const string sMOD_ID = "2101551127";
            return IsPluginRunning(sMOD_ID, "");
        }

        public static bool IsAdvancedBuildingLevelRunning()
        {
            const string sMOD_ID = "2133705267";
            return IsPluginRunning(sMOD_ID, "");
        }

        public static bool IsRONRunning()
        {
            const string sMOD_ID = "2405917899";
            const string sAssemblyName = "RON";
            return IsPluginRunning(sMOD_ID, sAssemblyName);
        }

        public static bool IsAdvancedOutsideConnectionsRunning()
        {
            const string sMOD_ID = "2053500739";
            return IsPluginRunning(sMOD_ID, "AdvancedOutsideConnection");
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
    }
}
