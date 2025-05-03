using ColossalFramework.UI;
using System;
using System.Collections;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using UnityEngine;

namespace TransferManagerCE.Util
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
                Debug.Log("Exception: " + e.Message);
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
                Debug.Log("Exception: " + e.Message);
            }
        }

        private bool IsShortcutPressed()
        {
            return ModSettings.GetSettings().TransferIssueHotkey.IsPressed() ||
                    ModSettings.GetSettings().SelectionToolHotkey.IsPressed() ||
                    ModSettings.GetSettings().StatsPanelHotkey.IsPressed() ||
                    ModSettings.GetSettings().OutsideConnectionPanelHotkey.IsPressed() ||
                    ModSettings.GetSettings().SettingsPanelHotkey.IsPressed();
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

                if (ModSettings.GetSettings().TransferIssueHotkey.IsPressed())
                {
                    // Create panel if needed
                    TransferIssuePanel.Init();
                    if (TransferIssuePanel.Instance is not null)
                    {
                        TransferIssuePanel.Instance.TogglePanel();
                    }
                }
                else if (!SelectionTool.HasUnifiedUIButtonBeenAdded() && ModSettings.GetSettings().SelectionToolHotkey.IsPressed())
                {
                    // Create panel if needed
                    if (BuildingPanel.Instance is null)
                    {
                        BuildingPanel.Init();
                    }

                    if (SelectionTool.Instance is not null)
                    {
                        SelectionTool.Instance.ToogleSelectionTool();
                    }
                    else if (BuildingPanel.Instance is not null)
                    {
                        // Select current building in the building details panel and show.
                        if (WorldInfoPanel.GetCurrentInstanceID().Building != 0)
                        {
                            BuildingPanel.Instance.ShowPanel(WorldInfoPanel.GetCurrentInstanceID().Building);
                        }

                        // Open panel
                        BuildingPanel.Instance.TogglePanel();
                    }
                }
                else if (ModSettings.GetSettings().StatsPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    StatsPanel.Init();
                    if (StatsPanel.Instance is not null)
                    {
                        StatsPanel.Instance.TogglePanel();
                    }
                }
                else if (ModSettings.GetSettings().OutsideConnectionPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    OutsideConnectionPanel.Init();
                    if (OutsideConnectionPanel.Instance is not null)
                    {
                        // Open panel
                        OutsideConnectionPanel.Instance.TogglePanel();
                    }
                    
                }
                else if (ModSettings.GetSettings().SettingsPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    SettingsPanel.Init();
                    if (SettingsPanel.Instance is not null)
                    {
                        // Open panel
                        SettingsPanel.Instance.TogglePanel();
                    }

                }
            }
        }
    }
}
