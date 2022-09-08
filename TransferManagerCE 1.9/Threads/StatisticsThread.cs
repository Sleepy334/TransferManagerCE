using ColossalFramework.UI;
using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class StatisticsThread : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 1000;

        private bool _processed = false;
        private long m_LastElapsedTime = 0;
        private Stopwatch? m_watch = null;
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
            if (ModSettings.StatsPanelHotkey.IsPressed())
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
                    m_watch.Stop();
                    m_LastElapsedTime = 0;
                }
            }
            else
            {
                if (!m_watch.IsRunning)
                {
                    m_watch.Start();
                    m_LastElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // Update other panels
                if (m_watch.ElapsedMilliseconds - m_LastElapsedTime > iPANEL_UPDATE_RATE)
                {
#if DEBUG
                    long lStartTime = m_watch.ElapsedMilliseconds;
#endif
                    if (s_statPanel != null && s_statPanel.isVisible)
                    {
                        s_statPanel.UpdatePanel();
                    }

                    m_LastElapsedTime = m_watch.ElapsedMilliseconds;
#if DEBUG
                    long lStopTime = m_watch.ElapsedMilliseconds;
                    //Debug.Log("Update panels - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
                }
            }
        }

        public static bool HandleEscape()
        {
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

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
