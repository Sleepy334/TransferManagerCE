using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public class GhostVehicleThread : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 5 * 60000; // 5 minutes

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
                    // Periodically Check for "Ghost" vehicles
                    CitiesUtils.ReleaseGhostVehicles();

                    m_watch.Start();
                    m_LastPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // s_bUpdatePanel gets set when an offer comes in for the current building
                if ((m_watch.ElapsedMilliseconds - m_LastPanelElapsedTime) > iPANEL_UPDATE_RATE)
                {
#if DEBUG
                    long lStartTime = m_watch.ElapsedMilliseconds;
#endif
                    // Periodically Check for "Ghost" vehicles
                    CitiesUtils.ReleaseGhostVehicles();
#if DEBUG
                    long lStopTime = m_watch.ElapsedMilliseconds;
                    Debug.Log("Ghost vehicle thread - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
                    m_LastPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
