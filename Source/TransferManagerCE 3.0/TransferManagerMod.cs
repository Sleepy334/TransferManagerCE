using SleepyCommon;
using System.Reflection;
using TransferManagerCore.Util;
using UnityEngine;
using TransferManagerCore.CustomManager;
using ColossalFramework.UI;
using TransferManagerCore.Settings;
using TransferManagerCore.UI;
using ColossalFramework;

namespace TransferManagerCore
{
    public class TransferManagerMod : UserModBase
    {
#if TEST_RELEASE || TEST_DEBUG
        private static string Edition => " TEST";
#else
		private static string Edition => "";
#endif

#if DEBUG
        private static string Config => $" [DEBUG]";
#else
        private static string Config => "";
#endif
        private GameObject? m_modManagerGameObject = null;
        private GameObject? m_keyboardShortcutGameObject = null;
        private UITextureAtlas? m_atlas = null;

        // ----------------------------------------------------------------------------------------
        public static TransferManagerMod Instance
        {
            get
            {
                return (TransferManagerMod)BaseInstance;
            }
        }

        public override string BaseModName
        {
            get
            {
#if TRANSFER_MANAGER_EXTENDED
                return $"Transfer Manager Extended";
#else
                return $"Transfer Manager CE";
#endif
            }
        }

        public override string ModName
        {
            get
            {
                return $"{BaseModName} {Edition}{Config}" ;
            }
        }

		public override string Description
		{
			get { return "More realistic response to service requests."; }
		}

        public override void OnEnabled()
        {
            base.OnEnabled();

#if TRANSFER_MANAGER_EXTENDED
            Patcher.Patch(typeof(TransferManagerAwakePatch));

            // Add our patch call to pre load level function just in case TransferManager.Awake has already been called
            Singleton<LoadingManager>.instance.m_levelPreLoaded += OnPreLoadLevel;
#endif
        }

        public void OnPreLoadLevel()
        {
#if TRANSFER_MANAGER_EXTENDED
            if (!TransferManagerAwakePatch.IsTransferReasonArraysPatched())
            {
                // Force patching of arrays
                TransferManagerAwakePatch.PatchTransferArraysManually();
            }
#endif
        }

        public override void OnLevelLoaded()
        {
            DependencyUtils.LogPlugins();

            // Check for mod conflicts
            if (ConflictingMods.ConflictingModsFound())
            {
                IsLoaded = false;
                return;
            }

#if TRANSFER_MANAGER_EXTENDED
            // Check arrays have actually been extended
            if (!TransferManagerAwakePatch.IsTransferReasonArraysPatched())
            {
                string strMessage = "Error: Transfer reason arrays unpatched.\r\n";
                strMessage += "\r\n";
                strMessage += "\r\nThis can happen if you enable the Transfer Manager Extended mod after the transfer reason arrays have already been created. Please restart Cities Skylines.";
                strMessage += "\r\n";
                strMessage += "\r\nMod disabled for this load.";
                Prompt.Info(ModName, strMessage);
                IsLoaded = false;
                return;
            }
#endif

            if (m_modManagerGameObject is null)
            {
                m_modManagerGameObject = new GameObject("TransferManagerModManager");
                m_modManagerGameObject.AddComponent<ModManager>();
            }

            if (m_keyboardShortcutGameObject is null)
            {
                m_keyboardShortcutGameObject = new GameObject("KeyboardShortcuts");
                m_keyboardShortcutGameObject.AddComponent<KeyboardShortcuts>();
            }

            PathFindFailure.Init();

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
                if (DependencyUtils.IsSmarterFireFightersRunning())
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
                IsLoaded = false;
                return;
            }

            InfoPanelButtons.AddInfoPanelButtons();
            SelectionTool.AddSelectionTool();
            UnifiedUIButton.Add();

            // We need to clean up any sick timers that may have been running before the sick handler was changed
            SickHandler.ClearSickTimerForNonResidential();

            // Check the settings are still consistent now we have loaded map.
            BuildingSettingsStorage.CheckBuildingSettings();

            // Generate path distance cache if needed
            PathDistanceCache.UpdateCache();


            // Update passenger capacities for spawn patch
            TransportStationAIPatches.UpdatePassengerCapacities();

#if !TRANSFER_MANAGER_EXTENDED
            // We run this if Transfer Manager Extended data detected.
            if (TransferManager.TRANSFER_REASON_COUNT == 128 &&
                TransferManagerSerializer.instance.CheckTransferManagerExtendedDataExists())
            {
                ClearExtendedReasons.Clear();
            }
#endif
        }

        public override void OnLevelUnloading()
        {
            if (m_modManagerGameObject is not null)
            {
                UnityEngine.Object.Destroy(m_modManagerGameObject.gameObject);
                m_modManagerGameObject = null;
            }

            // Remove patches first so objects aren't called after being destroyed
            RemoveHarmonyPatches();

            // Delete mod objects
            TransferManagerThread.StopThreads();
            MatchLoggingThread.StopThread();
            CustomTransferDispatcher.Instance.Delete();
            TransferJobQueue.Instance.Destroy();
            TransferJobPool.Instance.Delete();
            PathFindFailure.Delete();
            MatchStats.Destroy();

            // Clear settings
            ClearSettings();

            // Destroy Panels
            TransferIssuePanel.Destroy();
            BuildingPanel.Destroy();
            DistrictSelectionPanel.Destroy();
            StatsPanel.Destroy();
            OutsideConnectionPanel.Destroy();
            SettingsPanel.Destroy();
            PathDistancePanel.Destroy();

            base.OnLevelUnloading();
        }

        // called when unloading finished
        public override void OnReleased()
        {
            base.OnReleased();
            Patcher.UnpatchAll();
        }

        // Sets up a settings user interface
        public void OnSettingsUI(UIHelper helper)
        {
            SettingsUI settingsUI = new SettingsUI();
            settingsUI.OnSettingsUI(helper);
        }

        public bool ApplyHarmonyPatches()
        {
            // Harmony
            if (DependencyUtils.IsHarmonyRunning())
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
                Prompt.ErrorFormat(BaseModName, sMessage);
                return false;
            }

            return true;
        }

        public void RemoveHarmonyPatches()
        {
            if (IsLoaded && DependencyUtils.IsHarmonyRunning())
            {
                Patcher.UnpatchAll();
            }
        }

        public UITextureAtlas? LoadResources()
        {
            if (m_atlas is null)
            {
                string[] spriteNames = new string[]
                {
                    "clear",
                    "Transfer",
                    "Information",
                    "CopyButtonIcon",
                    "PasteButtonIcon",
                    "Dead",
                };

#if TRANSFER_MANAGER_EXTENDED
                m_atlas = ResourceLoader.CreateTextureAtlas($"TransferManagerExtendedAtlas", spriteNames, Assembly.GetExecutingAssembly(), $"TransferManagerExtended.Resources.");
#else
                m_atlas = ResourceLoader.CreateTextureAtlas("TransferManagerCEAtlas", spriteNames, Assembly.GetExecutingAssembly(), "TransferManagerCE.Resources.");
#endif

                if (m_atlas is null)
                {
                    Log.Error("Loading of resources failed.");
                }

                UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
                Texture2D[] textures = new Texture2D[]
                {
                    defaultAtlas["ToolbarIconGroup6Focused"].texture,
                    defaultAtlas["ToolbarIconGroup6Hovered"].texture,
                    defaultAtlas["ToolbarIconGroup6Normal"].texture,
                    defaultAtlas["ToolbarIconGroup6Pressed"].texture
                };

                if (m_atlas is not null)
                {
                    ResourceLoader.AddTexturesInAtlas(m_atlas, textures);
                }
            }

            return m_atlas;
        }

        public void BuildingReleasedHandler(ushort building)
        {
            if (building != 0)
            {
                BuildingSettingsStorage.ReleaseBuilding(building);
                CrimeCitizenCountStorage.ReleaseBuilding(building);
            }
        }

        public void ClearSettings()
        {
            // Reset global settings
            SaveGameSettings.SetSettings(new SaveGameSettings());

            // Reset Building Settings
            BuildingSettingsStorage.ClearAllSettings();

            // Reset Outside Connection Settings
            OutsideConnectionSettings.ClearSettings();
        }
    }
}