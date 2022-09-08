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
        private static TransferIssuePanel? s_TransferIssuePanel = null;

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
                    if (s_TransferIssuePanel != null && s_TransferIssuePanel.isVisible)
                    {
                        s_TransferIssuePanel.UpdatePanel();
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
            if (s_TransferIssuePanel != null && s_TransferIssuePanel.isVisible)
            {
                s_TransferIssuePanel.Hide();
                return true;
            }
            return false;
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
                    s_TransferIssuePanel.BringToFront();
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

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
