using ColossalFramework.UI;
using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class StatisticsThreadExtension : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 1000;

        private bool _processed = false;
        private long m_LastElapsedTime = 0;
        private Stopwatch? m_watch = null;

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

                // Create panel if needed
                if (StatsPanel.Instance == null)
                {
                    StatsPanel.Init();
                }

                if (StatsPanel.Instance != null)
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
                    if (StatsPanel.Instance != null && StatsPanel.Instance.isVisible)
                    {
                        StatsPanel.Instance.UpdatePanel();
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
            if (StatsPanel.Instance != null && StatsPanel.Instance.isVisible)
            {
                StatsPanel.Instance.Hide();
                return true;
            }
            return false;
        }

        public static void ToggleStatsPanel()
        {
            if (StatsPanel.Instance == null)
            {
                StatsPanel.Init();
            }

            if (StatsPanel.Instance != null)
            {
                if (StatsPanel.Instance.isVisible)
                {
                    StatsPanel.Instance.Hide();
                }
                else
                {
                    StatsPanel.Instance.Show();
                    StatsPanel.Instance.BringToFront();
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
