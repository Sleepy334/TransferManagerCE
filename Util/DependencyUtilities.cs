using ColossalFramework.Plugins;

namespace TransferManagerCE
{
    public static class DependencyUtilities
    {
        private static bool s_bHasSmarterFireFightersBeenCheckedRunning = false;
        private static bool s_bIsSmarterFireFightersRunning = false;

        private static bool s_bHasUnifiedUIBeenCheckedRunning = false;
        private static bool s_bIsUnifiedUIRunning = false;

        private static bool s_bHasPloppableRicoBeenCheckedRunning = false;
        private static bool s_bIsPloppableRicoRunning = false;

        private static bool s_bHasRepainterBeenCheckedRunning = false;
        private static bool s_bIsRepainterRunning = false;

        private static bool s_bHasAdvancedBuildingLevelBeenCheckedRunning = false;
        private static bool s_bIsAdvancedBuildingLevelRunning = false;

        public static void SearchPlugins()
        {
            string sPlugins = "";
            foreach (PluginManager.PluginInfo oPlugin in PluginManager.instance.GetPluginsInfo())
            {
                sPlugins += oPlugin.name + " " + oPlugin.GetHashCode() + "\r\n";
            }
            Debug.Log(sPlugins);
        }

        public static bool IsPluginRunning(string sPluginId)
        {
            bool bRunning = false;
            foreach (PluginManager.PluginInfo oPlugin in PluginManager.instance.GetPluginsInfo())
            {
                if (oPlugin.name == sPluginId && oPlugin.isEnabled)
                {
                    bRunning = true;
                    break;
                };
            }
            return bRunning;
        }

        public static bool IsSmarterFireFightersRunning()
        {
            const string sSMARTER_FIREFIGHTERS_ID = "2346565561";

            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_bHasSmarterFireFightersBeenCheckedRunning)
                {
                    s_bIsSmarterFireFightersRunning = IsPluginRunning(sSMARTER_FIREFIGHTERS_ID);
                    s_bHasSmarterFireFightersBeenCheckedRunning = true;
                }

                return s_bIsSmarterFireFightersRunning;
            }
            else
            {
                return IsPluginRunning(sSMARTER_FIREFIGHTERS_ID);
            }
        }

        public static bool IsUnifiedUIRunning()
        {
            const string sUNIFIED_UI_ID = "2255219025";

            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_bHasUnifiedUIBeenCheckedRunning)
                {
                    s_bIsUnifiedUIRunning = IsPluginRunning(sUNIFIED_UI_ID);
                    s_bHasUnifiedUIBeenCheckedRunning = true;
                }

                return s_bIsUnifiedUIRunning;
            }
            else
            {
                return IsPluginRunning(sUNIFIED_UI_ID);
            }
        }

        public static bool IsPloppableRICORunning()
        {
            const string sMOD_ID = "2016920607";

            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_bHasPloppableRicoBeenCheckedRunning)
                {
                    s_bIsPloppableRicoRunning = IsPluginRunning(sMOD_ID);
                    s_bHasPloppableRicoBeenCheckedRunning = true;
                }

                return s_bIsPloppableRicoRunning;
            }
            else
            {
                return IsPluginRunning(sMOD_ID);
            }
        }

        public static bool IsRepainterRunning()
        {
            const string sMOD_ID = "2101551127";

            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_bHasRepainterBeenCheckedRunning)
                {
                    s_bIsRepainterRunning = IsPluginRunning(sMOD_ID);
                    s_bHasRepainterBeenCheckedRunning = true;
                }

                return s_bIsRepainterRunning;
            }
            else
            {
                return IsPluginRunning(sMOD_ID);
            }
        }

        public static bool IsAdvancedBuildingLevelRunning()
        {
            const string sMOD_ID = "2133705267";

            // Only cache result once map is loaded
            if (TransferManagerLoader.IsLoaded())
            {
                if (!s_bHasAdvancedBuildingLevelBeenCheckedRunning)
                {
                    s_bIsAdvancedBuildingLevelRunning = IsPluginRunning(sMOD_ID);
                    s_bHasAdvancedBuildingLevelBeenCheckedRunning = true;
                }

                return s_bIsAdvancedBuildingLevelRunning;
            }
            else
            {
                return IsPluginRunning(sMOD_ID);
            }
        }
    }
}
