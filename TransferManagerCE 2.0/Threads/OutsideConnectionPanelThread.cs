using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public class OutsideConnectionPanelThread : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 4000;

        private bool _processed = false;
        private long m_LastPanelElapsedTime = 0;
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

            if (ModSettings.OutsideConnectionPanelHotkey.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed)
                {
                    return;
                }
                _processed = true;

                // Create panel if needed
                OutsideConnectionPanel.Init();

                if (OutsideConnectionPanel.Instance != null)
                {
                    // Open panel
                    OutsideConnectionPanel.Instance.TogglePanel();
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
                    m_LastPanelElapsedTime = 0;
                }
            }
            else
            {
                if (!m_watch.IsRunning)
                {
                    m_watch.Start();
                    m_LastPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // s_bUpdatePanel gets set when an offer comes in for the current building
                if ((m_watch.ElapsedMilliseconds - m_LastPanelElapsedTime) > iPANEL_UPDATE_RATE)
                {

                    if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
                    {
#if DEBUG
                        long lStartTime = m_watch.ElapsedMilliseconds;
#endif
                        OutsideConnectionPanel.Instance.UpdatePanel();
#if DEBUG
                        long lStopTime = m_watch.ElapsedMilliseconds;
                        Debug.Log("Outside Connection panel update - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
                    }

                    m_LastPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }
            }
        }

        public static bool HandleEscape()
        {
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance?.Hide();
                return true;
            }
            return false;
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
