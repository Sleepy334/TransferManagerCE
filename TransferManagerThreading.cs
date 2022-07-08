using ColossalFramework.UI;
using ICities;
using System;
using System.Diagnostics;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferManagerCEThreading : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 4000;
        const int iBUILDING_PANEL_UPDATE_RATE = 1000;

        private bool _processed = false;

        private long m_LastBuildingPanelElapsedTime = 0;
        private long m_LastElapsedTime = 0;
        private Stopwatch? m_watch = null;

        private static TransferIssuePanel? s_TransferIssuePanel = null;
        private static StatsPanel s_statPanel = null;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (!TransferManagerLoader.IsLoaded())
            {
                return;
            }

            if (m_watch == null)
            {
                m_watch = new Stopwatch();
            }
            if (ModSettings.TransferIssueHotkey.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed)
                {
                    return;
                }
                _processed = true;

                if (s_TransferIssuePanel == null)
                {
                    CreateTransferIssuePanel();
                }
                else
                {
                    ToggleTransferIssuePanel();
                }
            }
            else if (ModSettings.GetSettings().StatisticsEnabled && ModSettings.StatsPanelHotkey.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed)
                {
                    return;
                }
                _processed = true;

                if (s_statPanel == null)
                {
                    CreateStatsPanel();
                }
                else
                {
                    ToggleStatsPanel();
                }
            }
            else if (!SelectionTool.HasUnifiedUIButtonBeenAdded() && ModSettings.SelectionToolHotkey.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed)
                {
                    return;
                }
                _processed = true;

                // Create panel if needed
                TransferBuildingPanel.Init();

                if (SelectionTool.Instance != null)
                {
                    SelectionTool.Instance.ToogleSelectionTool();
                }
                else if (TransferBuildingPanel.Instance != null)
                {
                    // Select current building in the building details panel and show.
                    if (WorldInfoPanel.GetCurrentInstanceID().Building != 0)
                    {
                        TransferBuildingPanel.Instance.SetPanelBuilding(WorldInfoPanel.GetCurrentInstanceID().Building);
                    }

                    // Open panel
                    TransferBuildingPanel.Instance.TogglePanel();
                }
            }
            else
            {
                _processed = false;
            }

            // Update panel
            if (SimulationManager.instance.SimulationPaused)
            {
                if (m_watch.IsRunning)
                {
                    Debug.Log("Simulation stopped");
                    m_watch.Stop();
                    m_LastElapsedTime = 0;
                }
            }
            else
            {
                if (!m_watch.IsRunning)
                {
                    Debug.Log("Simulation started");
                    m_watch.Start();
                    m_LastElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // Update building panel at a faster rate to see transfer offers better
                if (m_watch.ElapsedMilliseconds - m_LastBuildingPanelElapsedTime > iBUILDING_PANEL_UPDATE_RATE)
                {
#if DEBUG
                    long lStartTime = m_watch.ElapsedMilliseconds;
#endif
                    if (TransferBuildingPanel.Instance != null && TransferBuildingPanel.Instance.isVisible)
                    {
                        TransferBuildingPanel.Instance.UpdatePanel();
#if DEBUG
                        long lStopTime = m_watch.ElapsedMilliseconds;
                        Debug.Log("Building panel update - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
                    }

                    m_LastBuildingPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // Update other panels
                if (m_watch.ElapsedMilliseconds - m_LastElapsedTime > iPANEL_UPDATE_RATE)
                {
#if DEBUG
                    long lStartTime = m_watch.ElapsedMilliseconds;
#endif
                    if (s_TransferIssuePanel != null && s_TransferIssuePanel.isVisible)
                    {
                        s_TransferIssuePanel.UpdatePanel();
                    }

                    if (s_statPanel != null && s_statPanel.isVisible)
                    {
                        s_statPanel.UpdatePanel();
                    }

                    m_LastElapsedTime = m_watch.ElapsedMilliseconds;
#if DEBUG
                    long lStopTime = m_watch.ElapsedMilliseconds;
                    Debug.Log("Update panels - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
                }
            }
        }

        public static bool HandleEscape()
        {
            if (TransferBuildingPanel.IsVisible())
            {
                SelectionTool.Instance?.Disable(); 
                TransferBuildingPanel.Instance?.Hide();
                return true;
            }
            if (s_TransferIssuePanel != null && s_TransferIssuePanel.isVisible)
            {
                s_TransferIssuePanel.Hide();
                return true;
            }
            if (s_statPanel != null && s_statPanel.isVisible)
            {
                s_statPanel.Hide();
                return true;
            }
            return false;
        }

        public static void CreateStatsPanel()
        {
            if (s_statPanel == null)
            {
                s_statPanel = UIView.GetAView().AddUIComponent(typeof(StatsPanel)) as StatsPanel;
                if (s_statPanel == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Stats Panel.");
                }
            }
        }

        public static void ToggleStatsPanel()
        {
            CreateStatsPanel();

            if (s_statPanel != null)
            {
                if (s_statPanel.isVisible)
                {
                    s_statPanel.Hide();
                }
                else
                {
                    s_statPanel.Show();
                    s_statPanel.BringToFront();
                }
            }
            else
            {
                Debug.Log("ToggleStatsPanel is null");
            }
        }

        public static void CreateTransferIssuePanel()
        {
            if (s_TransferIssuePanel == null)
            {
                s_TransferIssuePanel = UIView.GetAView().AddUIComponent(typeof(TransferIssuePanel)) as TransferIssuePanel;
                if (s_TransferIssuePanel == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Transfer Issue Panel.");
                }
            }
        }

        public static void ToggleTransferIssuePanel()
        {
            if (s_TransferIssuePanel != null)
            {
                if (s_TransferIssuePanel.isVisible)
                {
                    s_TransferIssuePanel.Hide();
                }
                else
                {
                    s_TransferIssuePanel.Show();
                }
            } 
            else
            {
                Debug.Log("ToggleTransferIssuePanel is null");
            }
        }

        public static void ShowTransferIssuePanel()
        {
            if (s_TransferIssuePanel != null)
            {
                if (!s_TransferIssuePanel.isVisible)
                {
                    s_TransferIssuePanel.Show();
                }
            }
        }

        public static void HideTransferIssuePanel()
        {
            if (s_TransferIssuePanel != null)
            {
                if (s_TransferIssuePanel.isVisible)
                {
                    s_TransferIssuePanel.Hide();
                }
            }
        }

        public static void StartTransfer(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {
            // This gets called by vanilla or custom transfer manager (whichever is running) when a match occurs.
            if (MatchLogging.instance != null)
            {
                MatchLogging.instance.StartTransfer(material, outgoingOffer, incomingOffer, deltaamount);
            }

            if (ModSettings.GetSettings().StatisticsEnabled && TransferManagerStats.s_Stats != null)
            {
                double distance = Math.Sqrt(Vector3.SqrMagnitude(incomingOffer.Position - outgoingOffer.Position));
                TransferManagerStats.s_Stats[(int)material].TotalMatches++;
                TransferManagerStats.s_Stats[(int)material].TotalMatchAmount += deltaamount;
                TransferManagerStats.s_Stats[(int)material].TotalDistance += distance;

                TransferManagerStats.s_Stats[TransferManagerStats.iMATERIAL_TOTAL_LOCATION].TotalMatches++;
                TransferManagerStats.s_Stats[TransferManagerStats.iMATERIAL_TOTAL_LOCATION].TotalMatchAmount += deltaamount;
                TransferManagerStats.s_Stats[TransferManagerStats.iMATERIAL_TOTAL_LOCATION].TotalDistance += distance;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
