using ColossalFramework.Plugins;
using SleepyCommon;
using System.Reflection;

namespace TransferManagerCore
{
    public class ConflictingMods
    {
        public static bool ConflictingModsFound()
        {
            string sConflictingMods = "";
            int iTransferManagerCount = 0;

            Log.Info("Checking for conflicting mods");

            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin is not null && plugin.isEnabled)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        //Log.Info($"\r\n{assembly.GetName().Name}");

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
                            case "TaxiStandFix":
                                {
                                    sConflictingMods += "Taxi Stand Fix\r\n";
                                    break;
                                }
                            case "OneModFix":
                                {
                                    sConflictingMods += "One Mod Fix\r\n";
                                    break;
                                }
#if TRANSFER_MANAGER_EXTENDED
                            case "TransferManagerCE":
                                {
                                    sConflictingMods += "Transfer Manager CE\r\n";
                                    break;
                                }
                            case "MoreTransferReasons":
                                {
                                    sConflictingMods += "More Transfer Reasons\r\n";
                                    break;
                                }
                            case "TransferManagerExtended":
                                {
                                    iTransferManagerCount++;
                                    if (iTransferManagerCount > 1)
                                    {
                                        sConflictingMods += "Multiple Transfer Manager Extended mods running\r\n";
                                    }

                                    break;
                                }
#else
                            case "PrisonHelicopter":
                                {
                                    sConflictingMods += "Prison Helicopter Mod\r\n";
                                    break;
                                }
                            case "TransferManagerExtended":
                                {
                                    sConflictingMods += "Transfer Manager Extended\r\n";
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
#endif
                            default:
                                {
                                    //Log.Info("Assembly: " + assembly.GetName().Name);
                                    break;
                                }
                        }
                    }
                }
            }

            // Also check for Akira's Employ Overeducated Workers as it completely overrides TMCE.
            if (DependencyUtils.IsEmployOverEducatedWorkersByAkiraRunning())
            {
                sConflictingMods += "Employ Overeducated Workers (By Akira)\r\n";
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
                Prompt.WarningFormat(TransferManagerMod.Instance.Name, sMessage);
                return true;
            }
        }
    }
}