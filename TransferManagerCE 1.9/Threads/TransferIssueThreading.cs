using ColossalFramework.UI;
using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class TransferIssueThread : ThreadingExtensionBase
    {
        const int iPANEL_UPDATE_RATE = 4000;

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
            if (ModSettings.TransferIssueHotkey.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed)
                {
                    return;
                }
                _processed = true;

                if (TransferIssuePanel.Instance == null)
                {
                    TransferIssuePanel.Init();
                }

                if (TransferIssuePanel.Instance != null)
                {
                    ToggleTransferIssuePanel();
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
                    if (TransferIssuePanel.Instance != null && TransferIssuePanel.Instance.isVisible)
                    {
                        TransferIssuePanel.Instance.UpdatePanel();
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
            if (TransferIssuePanel.Instance != null && TransferIssuePanel.Instance.isVisible)
            {
                TransferIssuePanel.Instance.Hide();
                return true;
            }
            return false;
        }

        public static void ToggleTransferIssuePanel()
        {
            if (TransferIssuePanel.Instance == null)
            {
                TransferIssuePanel.Init();
            }

            if (TransferIssuePanel.Instance != null)
            {
                if (TransferIssuePanel.Instance.isVisible)
                {
                    TransferIssuePanel.Instance.Hide();
                }
                else
                {
                    TransferIssuePanel.Instance.Show();
                }
            } 
            else
            {
                Debug.Log("ToggleTransferIssuePanel is null");
            }
        }

        public static void ShowTransferIssuePanel()
        {
            if (TransferIssuePanel.Instance == null)
            {
                TransferIssuePanel.Init();
            }
            
            if (TransferIssuePanel.Instance != null)
            {
                if (!TransferIssuePanel.Instance.isVisible)
                {
                    TransferIssuePanel.Instance.Show();
                    TransferIssuePanel.Instance.BringToFront();
                }
            }
        }

        public static void HideTransferIssuePanel()
        {
            if (TransferIssuePanel.Instance != null)
            {
                if (TransferIssuePanel.Instance.isVisible)
                {
                    TransferIssuePanel.Instance.Hide();
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
