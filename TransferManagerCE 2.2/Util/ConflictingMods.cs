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
            int iTransferManagerCount = 0;

            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin != null && plugin.isEnabled)
                {
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
                            case "EmployOvereducatedWorkers":
                                {
                                    sConflictingMods += "Employ Overeducated Workers V2\r\n";
                                    break;
                                }
                            case "TransferManagerCE":
                                {
                                    iTransferManagerCount++;
                                    if (iTransferManagerCount > 1)
                                    {
                                        sConflictingMods += "Multiple Transfer Manager CE mods running\r\n";
                                    }

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