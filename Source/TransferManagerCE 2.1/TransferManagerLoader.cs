using ColossalFramework.UI;
using ICities;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class TransferManagerLoader : LoadingExtensionBase
    {
        private static bool s_loaded = false;
        private static UITextureAtlas? s_atlas = null;

        public static bool IsLoaded() { return s_loaded; }

        private static bool ActiveInMode(LoadMode mode)
        {
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.NewGameFromScenario:
                case LoadMode.LoadGame:
                    return true;

                default:
                    return false;
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (TransferManagerMain.IsEnabled && ActiveInMode(mode))
            {
                s_loaded = true;

                // Check for mod conflicts
                if (ConflictingMods.ConflictingModsFound()) 
                {
                    s_loaded = false;
                    return;
                }

                PathFindFailure.Init();

                // Make sure the outside connection cache is cleared.
                PathNodeCache.InvalidateOutsideConnections();

                // Create TransferJobPool and initialize
                TransferJobPool.Instance.Initialize();

                // Initialise stats
                MatchStats.Init();

                // Hook into building release event to handle removal of buildings from settings.
                BuildingManager.instance.EventBuildingReleased += BuildingReleasedHandler;

                // Create TransferDispatcher and initialize
                TransferManagerThread.StartThreads();

                // Patch game using Harmony
                if (ApplyHarmonyPatches())
                {
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
                } 
                else
                {
                    s_loaded = false;
                    return;
                }

                InfoPanelButtons.AddInfoPanelButtons();
                SelectionTool.AddSelectionTool();
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            // Remove patches first so object aren't called after being destroyed
            RemoveHarmonyPathes();

            // Delete mod objects
            TransferManagerThread.StopThreads();
            TransferJobPool.Instance.Delete();
            TransferResultQueue.Instance.Delete();
            PathFindFailure.Delete();
            MatchStats.Destroy();
            PathFindFailure.Reset();
            MatchLoggingThread.StopThread();

            if (BuildingPanel.Instance != null)
            {
                BuildingPanel.Instance.OnDestroy();
                BuildingPanel.Instance = null;
            }

            if (DistrictPanel.Instance != null)
            {
                DistrictPanel.Instance.OnDestroy();
                DistrictPanel.Instance = null;
            }

            if (OutsideConnectionPanel.Instance != null)
            {
                OutsideConnectionPanel.Instance.OnDestroy();
                OutsideConnectionPanel.Instance = null;
            }

            if (TransferIssuePanel.Instance != null)
            {
                TransferIssuePanel.Instance.OnDestroy();
                TransferIssuePanel.Instance = null;
            }

            if (StatsPanel.Instance != null)
            {
                StatsPanel.Instance.OnDestroy();
                StatsPanel.Instance = null;
            }

            s_loaded = false;
        }

        public bool ApplyHarmonyPatches()
        {
            // Harmony
            if (DependencyUtilities.IsHarmonyRunning())
            {
                Patcher.PatchAll();
            }
            else
            {
                string sMessage = "Mod Dependency Error:\r\n";
                sMessage += "\r\n";
                sMessage += "Harmony not found.\r\n";
                sMessage += "\r\n";
                sMessage += "Mod disabled until dependencies resolved, please subscribe to Harmony.";
                Prompt.ErrorFormat("Transfer Manager CE", sMessage);
                return false;
            }

            if (!IsHarmonyValid())
            {
                RemoveHarmonyPathes();

                string strMessage = "Harmony patching failed\r\n";
                strMessage += "\r\n";
                strMessage += "If you could send TransferManagerCE.log to the author that would be great\r\n";
                strMessage += "\r\n";
                strMessage += "You could try Compatibility Report to check for mod compatibility or use Load Order Mod to ensure your mods are loaded in the correct order.";
                Prompt.ErrorFormat("Transfer Manager CE", strMessage);
                return false;
            }

            return true;
        }

        public void RemoveHarmonyPathes()
        {
            if (s_loaded && DependencyUtilities.IsHarmonyRunning())
            {
                Patcher.UnpatchAll();
            }
        }

        public bool IsHarmonyValid()
        {
            if (DependencyUtilities.IsHarmonyRunning())
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
                BuildingSettingsStorage.ReleaseBuilding(building);
            }
        }
    }
}
