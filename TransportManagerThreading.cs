using ColossalFramework.UI;
using ICities;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferManagerCEThreading : ThreadingExtensionBase
    {
        const int iUPDATE_RATE = 3000;

        private bool _processed = false;

        private long m_LastCallAgainElapsedTime = 0;
        private long m_LastElapsedTime = 0;
        private Stopwatch? m_watch = null;

        private static TransferIssuePanel? s_TransferIssuePanel = null;
        private static TransferBuildingPanel s_transferBuildingPanel = null;
        private static StatsPanel s_statPanel = null;

        public static CallAgain s_callAgain = null;

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

                // Call again
                if (ModSettings.GetSettings().CallAgainEnabled && (m_watch.ElapsedMilliseconds - m_LastCallAgainElapsedTime > (ModSettings.GetSettings().CallAgainUpdateRate * 1000)))
                {
                    if (s_callAgain == null)
                    {
                        s_callAgain = new CallAgain();
                    }
                    if (s_callAgain != null)
                    {
                        s_callAgain.Update(m_watch);
                    }

                    m_LastCallAgainElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // Update panels
                if (m_watch.ElapsedMilliseconds - m_LastElapsedTime > iUPDATE_RATE)
                {
                    if (s_TransferIssuePanel != null && s_TransferIssuePanel.isVisible)
                    {
                        s_TransferIssuePanel.UpdatePanel();
                    }

                    if (s_transferBuildingPanel != null && s_transferBuildingPanel.isVisible)
                    {
                        s_transferBuildingPanel.UpdatePanel();
                    }

                    if (s_statPanel != null && s_statPanel.isVisible)
                    {
                        s_statPanel.UpdatePanel();
                    }

                    m_LastElapsedTime = m_watch.ElapsedMilliseconds;
                }
            }
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
            if (s_statPanel != null)
            {
                if (s_statPanel.isVisible)
                {
                    s_statPanel.Hide();
                }
                else
                {
                    s_statPanel.Show();
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

        public static void CreateTransferBuildingPanel()
        {
            if (s_transferBuildingPanel == null)
            {
                s_transferBuildingPanel = UIView.GetAView().AddUIComponent(typeof(TransferBuildingPanel)) as TransferBuildingPanel;
                if (s_transferBuildingPanel == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Transfer Building Panel.");
                }
            }
        }

        public static void StartTransfer(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {
            if (s_transferBuildingPanel != null && s_transferBuildingPanel.isVisible)
            {
                s_transferBuildingPanel.StartTransfer(material, outgoingOffer, incomingOffer, deltaamount);
            }

            if (ModSettings.GetSettings().StatisticsEnabled && TransferManagerStats.s_Stats != null)
            {
                TransferManagerStats.s_Stats[(int)material].m_stats.TotalMatches++;
                TransferManagerStats.s_Stats[(int)material].m_stats.TotalMatchAmount += deltaamount;

                TransferManagerStats.s_Stats[255].m_stats.TotalMatches++;
                TransferManagerStats.s_Stats[255].m_stats.TotalMatchAmount += deltaamount;
            }
        }

        public static void ShowTransferBuildingPanel(ushort buildingId)
        {
            CreateTransferBuildingPanel();

            if (s_transferBuildingPanel != null)
            {
                s_transferBuildingPanel.SetPanelBuilding(buildingId);

                if (!s_transferBuildingPanel.isVisible)
                {    
                    s_transferBuildingPanel.Show();
                }
            }
        }

        public static void HideTransferBuildingPanel()
        {
            if (s_transferBuildingPanel != null)
            {
                if (s_transferBuildingPanel.isVisible)
                {
                    s_transferBuildingPanel.Hide();
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
