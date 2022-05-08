using ColossalFramework.Plugins;

namespace TransferManagerCE
{
    public static class DependencyUtilities
    {
        private static bool s_bHasSmarterFireFightersBeenCheckedRunning = false;
        private static bool s_bIsSmarterFireFightersRunning = false;

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
    }
}
