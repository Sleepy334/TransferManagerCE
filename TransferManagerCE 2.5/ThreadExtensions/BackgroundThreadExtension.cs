using ICities;

namespace TransferManagerCE
{
    public class BackgroundThreadExtension : ThreadingExtensionBase
    {
        private bool m_cacheInvalidated = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (!TransferManagerLoader.IsLoaded())
            {
                return;
            }

            // Update panel
            if (SimulationManager.instance.SimulationPaused)
            {
                if (!m_cacheInvalidated)
                {
                    // Invalidate these when paused as the user is likely to modify the network during this time.
                    PathNodeCache.InvalidateOutsideConnections();
                    m_cacheInvalidated = true;
                }
            }
            else
            {
                m_cacheInvalidated = false;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
