using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public class BuildingPanelThread : ThreadingExtensionBase
    {
        const int iBUILDING_PANEL_UPDATE_RATE = 4000;

        private bool _processed = false;

        private long m_LastBuildingPanelElapsedTime = 0;
        private Stopwatch? m_watch = null;
        private static bool s_bUpdatePanel = false;

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

            if (!SelectionTool.HasUnifiedUIButtonBeenAdded() && ModSettings.SelectionToolHotkey.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed)
                {
                    return;
                }
                _processed = true;

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
                        BuildingPanel.Instance.SetPanelBuilding(WorldInfoPanel.GetCurrentInstanceID().Building);
                    }

                    // Open panel
                    BuildingPanel.Instance.TogglePanel();
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
                    m_LastBuildingPanelElapsedTime = 0;
                }
            }
            else
            {
                if (!m_watch.IsRunning)
                {
                    m_watch.Start();
                    m_LastBuildingPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // s_bUpdatePanel gets set when an offer comes in for the current building
                if (s_bUpdatePanel || (m_watch.ElapsedMilliseconds - m_LastBuildingPanelElapsedTime) > iBUILDING_PANEL_UPDATE_RATE)
                {
                    if (BuildingPanel.Instance != null && BuildingPanel.Instance.isVisible)
                    {
#if DEBUG
                        long lStartTime = m_watch.ElapsedMilliseconds;
#endif
                        BuildingPanel.Instance.UpdatePanel();
                        s_bUpdatePanel = false;
#if DEBUG
                        long lStopTime = m_watch.ElapsedMilliseconds;
                        Debug.Log("Building panel update - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
                    }

                    m_LastBuildingPanelElapsedTime = m_watch.ElapsedMilliseconds;
                }
            }
        }

        public static bool HandleEscape()
        {
            if (BuildingPanel.IsVisible())
            {
                SelectionTool.Instance?.Disable(); 
                BuildingPanel.Instance?.Hide();
                return true;
            }
            return false;
        }

        public static void HandleOffer(TransferOffer offer)
        {
            if (BuildingPanel.Instance != null && 
                BuildingPanel.Instance.isVisible &&
                BuildingPanel.Instance.IsTransferTabActive() && 
                InstanceHelper.GetBuildings(offer.m_object).Contains(BuildingPanel.Instance.m_buildingId))
            {
                s_bUpdatePanel = true;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
