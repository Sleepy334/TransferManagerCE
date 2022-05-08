using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    public class TransferManagerLoader : LoadingExtensionBase
    {
        private static bool s_loaded = false;
        private const int iHARMONY_PATCH_COUNT = 7;

        public static bool IsLoaded() { return s_loaded; }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log("OnLevelLoaded");
            if (TransferToolMain.IsEnabled && (mode == LoadMode.LoadGame || mode == LoadMode.NewGame))
            {
                s_loaded = true;

                if (!IsHarmonyValid())
                {
                    s_loaded = false;
                    if (HarmonyHelper.IsHarmonyInstalled)
                    {
                        Patcher.UnpatchAll();
                    }
                    string strMessage = "Harmony patching failed\r\n";
                    strMessage += "\r\n";
                    strMessage += "If you could send TransferManagerCE.log to the author that would be great\r\n";
                    strMessage += "\r\n";
                    strMessage += "You could try Compatibility Report to check for mod compatibility or use Load Order Mod to ensure your mods are loaded in the correct order.";
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Transfer Manager CE Error", strMessage, true);
                    
                    return;
                }

                // Create TransferJobPool and initialize
                TransferJobPool.Instance.Initialize();

                // Create TransferDispatcher and initialize
                string strError;
                if (CustomTransferDispatcher.Instance.Initialize(out strError))
                {
                    // Create TransferManager background thread and start
                    CustomTransferDispatcher._transferThread = new System.Threading.Thread(CustomTransferManager.MatchOffersThread);
                    CustomTransferDispatcher._transferThread.IsBackground = true;
                    CustomTransferDispatcher._transferThread.Start();
                }
                else
                {
                    s_loaded = false;
                    if (HarmonyHelper.IsHarmonyInstalled)
                    {
                        Patcher.UnpatchAll();
                    }

                    string strMessage = "CustomTransferDispatcher Initialization failed\r\n";
                    strMessage += "\r\n";
                    strMessage += "If you could send TransferManagerCE.log to the author that would be great\r\n";
                    strMessage += "\r\n";
                    strMessage += strError;
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Transfer Manager CE Error", strMessage, true);
                    
                    return;
                }

                // Display warning about fire fighting patches
                if (DependencyUtilities.IsSmarterFireFightersRunning())
                {
                    string strMessage = "Smarter Firefighters: Improved AI detected\r\n";
                    strMessage += "\r\n";
                    strMessage += "The following harmony patches have not been applied due to mod conflict:\r\n";
                    strMessage += "FireTruckAI\r\n";
                    strMessage += "FireCopterAI\r\n";
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Transfer Manager CE", strMessage, true);
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (s_loaded)
            {
                // Unload
            }
        }

        public bool IsHarmonyValid()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                int iHarmonyPatches = Patcher.GetPatchCount();
                Debug.Log("Harmony patches: " + iHarmonyPatches);
                return iHarmonyPatches == Patcher.GetHarmonyPatchCount();
            }

            return false;
        }
    }
}
