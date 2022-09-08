using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using TransferManagerCE.CustomManager;
using UnityEngine;

namespace TransferManagerCE
{
    public class TransferManagerLoader : LoadingExtensionBase
    {
        private static bool s_loaded = false;
        private static UITextureAtlas? s_atlas = null;

        public static bool IsLoaded() { return s_loaded; }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log("OnLevelLoaded");
            if (TransferManagerMain.IsEnabled && (mode == LoadMode.LoadGame || mode == LoadMode.NewGame))
            {
                s_loaded = true;

                if (ConflictingMods.ConflictingModsFound()) 
                {
                    return;
                }

                // Harmony
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Patcher.PatchAll();
                }
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
                    Prompt.Info("Transfer Manager CE", strMessage);
                    return;
                }

                // Create TransferJobPool and initialize
                TransferJobPool.Instance.Initialize();

                // Initialise stats
                MatchStats.Init();

                // Hook into building release event to handle removal of buildings from settings.
                BuildingManager.instance.EventBuildingReleased += BuildingReleasedHandler;

                // Create TransferDispatcher and initialize
                string strError;
                if (CustomTransferDispatcher.Instance.Initialize(out strError))
                {
                    TransferManagerThread.StartThreads();
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
                    Prompt.Info("Transfer Manager CE", strMessage);
                    return;
                }

                // Display warning about fire fighting patches
                if (DependencyUtilities.IsSmarterFireFightersRunning())
                {
                    string strMessage = "Smarter Firefighters: Improved AI mod detected\r\n";
                    strMessage += "\r\n";
                    strMessage += "The following harmony patches have not been applied due to mod conflict:\r\n";
                    strMessage += "FireTruckAI\r\n";
                    strMessage += "FireCopterAI\r\n";
                    Prompt.Info("Transfer Manager CE", strMessage);
                }

                InfoPanelButtons.AddInfoPanelButtons();
                SelectionTool.AddSelectionTool();
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            TransferManagerThread.StopThreads();

            if (s_loaded)
            {
                // Unload
                Patcher.UnpatchAll();
            }

            if (BuildingPanel.Instance != null)
            {
                BuildingPanel.Instance.OnDestroy();
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

        public static UITextureAtlas? LoadResources()
        {
            if (s_atlas == null)
            {
                string[] spriteNames = new string[]
                {
                    "clear",
                    "Transfer"
                };

                s_atlas = ResourceLoader.CreateTextureAtlas("TransferManagerCEAtlas", spriteNames, "TransferManagerCE.Resources.");
                if (s_atlas == null)
                {
                    Debug.Log("Loading of resources failed.");
                }

                UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
                Texture2D[] textures = new Texture2D[]
                {
                    defaultAtlas["ToolbarIconGroup6Focused"].texture,
                    defaultAtlas["ToolbarIconGroup6Hovered"].texture,
                    defaultAtlas["ToolbarIconGroup6Normal"].texture,
                    defaultAtlas["ToolbarIconGroup6Pressed"].texture
                };

                if (s_atlas != null)
                {
                    ResourceLoader.AddTexturesInAtlas(s_atlas, textures);
                }
            }

            return s_atlas;
        }

        public void BuildingReleasedHandler(ushort building)
        {
            if (building != 0)
            {
                BuildingSettings.ReleaseBuilding(building);
            }
        }
    }
}
