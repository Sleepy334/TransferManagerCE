using ColossalFramework.Plugins;
using System.Reflection;

namespace TransferManagerCE
{
    public class ConflictingMods
    {
        const string sTransferManagerStable = "2804719780";
        const string sTransferManagerTest = "2810557345";

        public static bool ConflictingModsFound()
        {
            string sConflictingMods = "";
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin != null && plugin.isEnabled)
                {
                    switch (plugin.name)
                    {
                        case sTransferManagerStable:
                            {
#if TEST_RELEASE || TEST_DEBUG
                                sConflictingMods += "Transfer Manager Community Edition [STABLE]\r\n";
#endif
                                break;
                            }
                        case sTransferManagerTest:
                            {
#if !(TEST_RELEASE || TEST_DEBUG)
                                sConflictingMods += "Transfer Manager Community Edition [TEST]\r\n";
#endif
                                break;
                            }
                    }
                    
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        switch (assembly.GetName().Name)
                        {
                            case "TransferController":
                                {
                                    sConflictingMods += "Transfer Controller\r\n";
                                    break;
                                }
                            case "MoreEffectiveTransfer":
                                {
                                    sConflictingMods += "More Effective Transfer Manager\r\n";
                                    break;
                                }
                            case "EnhancedDistrictServices":
                                {
                                    sConflictingMods += "Enhanced District Services\r\n";
                                    break;
                                }
                            case "ConfigureOutsideConnectionsLimits":
                                {
                                    sConflictingMods += "Configure Outside Connections' Limits\r\n";
                                    break;
                                }
                            default:
                                {
                                    //Debug.Log("Assembly: " + assembly.GetName().Name);
                                    break;
                                }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(sConflictingMods))
            {
                return false;
            }
            else
            {
                string sMessage = "Conflicting Mods Found:\r\n";
                sMessage += "\r\n";
                sMessage += sConflictingMods;
                sMessage += "\r\n";
                sMessage += "Mod disabled until conflicts resolved, please remove these mods.";
                Prompt.WarningFormat("Transfer Manager CE", sMessage);
                return true;
            }
        }
    }
}