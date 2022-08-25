using ColossalFramework.UI;
using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class TransferManagerCEThreading : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 4000;

        private bool _processed = false;
        private long m_LastElapsedTime = 0;
        private Stopwatch? m_watch = null;

        private static TransferIssuePanel? s_TransferIssuePanel = null;
        private static StatsPanel s_statPanel = null;
        private static bool s_bShownPathUnitWarning = false;

        private static int s_iDistrictCount = 0;
        private static int s_iParkCount = 0;

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
                    //Debug.Log("Update panels - Execution Time: " + (lStopTime - lStartTime) + "ms");
                    //Debug.Log("Path units: " + PathManager.instance.m_pathUnitCount + " Size: " + PathManager.instance.m_pathUnits.m_size);

#endif
                    CheckPathUnits();
                    CheckDistricts();
                }
            }
        }

        public static bool HandleEscape()
        {
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
            CreateTransferIssuePanel();
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

        private void CheckPathUnits()
        {
            const double dWarningPercent = 0.95;
            if (!s_bShownPathUnitWarning && PathManager.instance.m_pathUnits.m_size * dWarningPercent <= PathManager.instance.m_pathUnitCount)
            {
                string sMessage = "Path Units used has hit " + (dWarningPercent * 100) + "% of available amount.\r\n";
                sMessage += "Current: " + PathManager.instance.m_pathUnitCount + "\r\n";
                sMessage += "Available: " + PathManager.instance.m_pathUnits.m_size + "\r\n";
                sMessage += "This is a game limit, not a mod limit, once you run out of path units vehicles will no longer be able to find paths.\r\n";
                sMessage += "You could try running the \"More Path Units\" mod by algernon to increase the available path units for the game.";
                Prompt.WarningFormat("Transfer Manager CE", sMessage);
                s_bShownPathUnitWarning = true;
            }
        }

        private void CheckDistricts()
        {
            // Check if districts changed
            int iNewDistrictCount = DistrictManager.instance.m_districtCount;
            int iNewParkCount = DistrictManager.instance.m_parkCount;
            if (iNewDistrictCount != s_iDistrictCount || s_iParkCount != iNewParkCount)
            {
                s_iDistrictCount = iNewDistrictCount;
                s_iParkCount = iNewParkCount;
                BuildingSettings.ValidateSettings();
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
