using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections;
using TransferManagerCore.Settings;
using TransferManagerCore.UI;
using UnityEngine;

namespace TransferManagerCore.Util
{
    internal class KeyboardShortcuts : MonoBehaviour
    {
        private Coroutine? m_shortcutCoroutine = null;

        public void Start()
        {
            try
            {
                if (m_shortcutCoroutine is null)
                {
                    m_shortcutCoroutine = StartCoroutine(WaitForShortcutHotkeyCoroutine());
                }
            }
            catch (Exception e)
            {
                Log.Info("Exception: " + e.Message);
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (m_shortcutCoroutine is not null)
                {
                    StopCoroutine(m_shortcutCoroutine);
                    m_shortcutCoroutine = null;
                }
            }
            catch (Exception e)
            {
                Log.Info("Exception: " + e.Message);
            }
        }

        private bool IsShortcutPressed()
        {
            return ModSettings.GetSettings().TransferIssueHotkey.IsPressed() ||
                    ModSettings.GetSettings().SelectionToolHotkey.IsPressed() ||
                    ModSettings.GetSettings().StatsPanelHotkey.IsPressed() ||
                    ModSettings.GetSettings().OutsideConnectionPanelHotkey.IsPressed() ||
                    ModSettings.GetSettings().SettingsPanelHotkey.IsPressed() ||
                    ModSettings.GetSettings().PathDistancePanelHotkey.IsPressed() ||
                    ModSettings.GetSettings().BuildingPanelNext.IsPressed() ||
                    ModSettings.GetSettings().BuildingPanelPrev.IsPressed();
        }

        private IEnumerator WaitForShortcutHotkeyCoroutine()
        {
            while (true)
            {
                // Wait for key to be released
                yield return new WaitUntil(() => !IsShortcutPressed());

                // Now wait for it to be pressed again
                yield return new WaitUntil(() => IsShortcutPressed());

                if (UIView.HasModalInput() || UIView.HasInputFocus())
                {
                    continue;
                }

                if (ModSettings.GetSettings().PathDistancePanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    PathDistancePanel.TogglePanel();
                }
                else if (ModSettings.GetSettings().StatsPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    StatsPanel.TogglePanel();
                }
                else if (ModSettings.GetSettings().SelectionToolHotkey.IsPressed())
                {
                    if (!BuildingPanel.IsVisible() && InstanceHelper.GetTargetInstance().Building != 0)
                    {
                        BuildingPanel.Instance.ShowPanel(InstanceHelper.GetTargetInstance().Building);
                    }
                    else
                    {
                        BuildingPanel.TogglePanel();
                    }
                }
                else if (ModSettings.GetSettings().TransferIssueHotkey.IsPressed())
                {
                    // Create panel if needed
                    TransferIssuePanel.TogglePanel();
                }
                else if (ModSettings.GetSettings().OutsideConnectionPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    OutsideConnectionPanel.TogglePanel();
                }
                else if (ModSettings.GetSettings().SettingsPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    SettingsPanel.TogglePanel();
                }
                else if (ModSettings.GetSettings().BuildingPanelNext.IsPressed())
                {
                    if (BuildingPanel.IsVisible())
                    {
                        BuildingPanel.Instance.SelectNextBuilding();
                        continue;
                    }
                }
                else if (ModSettings.GetSettings().BuildingPanelPrev.IsPressed())
                {
                    if (BuildingPanel.IsVisible())
                    {
                        BuildingPanel.Instance.SelectPrevBuilding();
                        continue;
                    }
                }
            }
        }
    }
}
