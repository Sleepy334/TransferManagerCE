﻿using System;
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
                if (m_shortcutCoroutine == null)
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
                if (m_shortcutCoroutine != null)
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
            return  ModSettings.TransferIssueHotkey.IsPressed() ||
                    ModSettings.SelectionToolHotkey.IsPressed() ||
                    ModSettings.StatsPanelHotkey.IsPressed() ||
                    ModSettings.OutsideConnectionPanelHotkey.IsPressed();
        }

        private IEnumerator WaitForShortcutHotkeyCoroutine()
        {
            while (true)
            {
                // Wait for key to be released
                yield return new WaitUntil(() => !IsShortcutPressed());

                // Now wait for it to be pressed again
                yield return new WaitUntil(() => IsShortcutPressed());

                if (ModSettings.TransferIssueHotkey.IsPressed())
                {
                    // Create panel if needed
                    TransferIssuePanel.Init();
                    if (TransferIssuePanel.Instance != null)
                    {
                        TransferIssuePanel.Instance.TogglePanel();
                    }
                }
                else if (!SelectionTool.HasUnifiedUIButtonBeenAdded() && ModSettings.SelectionToolHotkey.IsPressed())
                {
                    // Create panel if needed
                    if (BuildingPanel.Instance == null)
                    {
                        BuildingPanel.Init();
                    }

                    if (SelectionTool.Instance != null)
                    {
                        SelectionTool.Instance.ToogleSelectionTool();
                    }
                    else if (BuildingPanel.Instance != null)
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
                else if (ModSettings.StatsPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    StatsPanel.Init();
                    if (StatsPanel.Instance != null)
                    {
                        StatsPanel.Instance.TogglePanel();
                    }
                }
                else if (ModSettings.OutsideConnectionPanelHotkey.IsPressed())
                {
                    // Create panel if needed
                    OutsideConnectionPanel.Init();
                    if (OutsideConnectionPanel.Instance != null)
                    {
                        // Open panel
                        OutsideConnectionPanel.Instance.TogglePanel();
                    }
                }
            }
        }
    }
}
