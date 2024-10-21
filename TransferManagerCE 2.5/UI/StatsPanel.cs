using ColossalFramework.UI;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.UITabStrip;

namespace TransferManagerCE
{
    public class StatsPanel : UIPanel
    {
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;

        // General and Game tabs
        public const int iCOLUMN_WIDTH_DESCRIPTION = 420;
        public const int iCOLUMN_WIDTH_VALUE = 420;

        // Match stats widths
        public const int iCOLUMN_MATERIAL_WIDTH = 120;
        public const int iCOLUMN_WIDTH = 60;
        public const int iCOLUMN_BIGGER_WIDTH = 95;

        public static StatsPanel? Instance = null;

        private UITitleBar? m_title = null;
        private UITabStrip? m_tabStrip = null;
        private ListView? m_generalStats = null;
        private ListView? m_transferManagerStats = null;
        private ListView? m_gameStats = null;
        private ListView? m_listStats = null;
        private Coroutine? m_coroutine = null;

        private enum Tabs
        {
            TAB_GENERAL = 0,
            TAB_TRANSFER_MANAGER = 1,
            TAB_GAME = 2,
            TAB_STATS = 3,
        }

        public StatsPanel() : base()
        {
            m_coroutine = StartCoroutine(UpdatePanelCoroutine(1));
        }

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(StatsPanel)) as StatsPanel;
                if (Instance is null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Statistics Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "StatsPanel";
            width = 900;
            height = 730;
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "SubcategoriesPanel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            CenterTo(parent);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = Localization.Get("titleTransferStatsPanel"); ;

            m_tabStrip = UITabStrip.Create(TabStyle.Generic, this, width - 20f, height - m_title.height - 10, OnTabChanged);
            m_tabStrip.padding = new RectOffset(iMARGIN, iMARGIN, 4, iMARGIN);

            // General
            UIPanel? tabGeneral = m_tabStrip.AddTab(Localization.Get("tabGeneral"));
            if (tabGeneral is not null)
            {
                // Offer list
                m_generalStats = ListView.Create<UIGeneralStatsRow>(tabGeneral, "ScrollbarTrack", 0.7f, width - 20f, tabGeneral.height);
                if (m_generalStats is not null)
                {
                    m_generalStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", iCOLUMN_WIDTH_DESCRIPTION, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_generalStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", iCOLUMN_WIDTH_VALUE, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                }
            }

            // Transfer Manager
            UIPanel? tabTransferManager = m_tabStrip.AddTab(Localization.Get("tabTransferManager"), 200f);
            if (tabTransferManager is not null)
            {
                // Offer list
                m_transferManagerStats = ListView.Create<UIGeneralStatsRow>(tabTransferManager, "ScrollbarTrack", 0.7f, width - 20f, tabTransferManager.height);
                if (m_transferManagerStats is not null)
                {
                    m_transferManagerStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", iCOLUMN_WIDTH_DESCRIPTION, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_transferManagerStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", iCOLUMN_WIDTH_VALUE, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                }
            }

            // Game
            UIPanel? tabGame = m_tabStrip.AddTab(Localization.Get("tabGame"));
            if (tabGame is not null)
            {
                // Offer list
                m_gameStats = ListView.Create<UIGeneralStatsRow>(tabGame, "ScrollbarTrack", 0.7f, width - 20f, tabGame.height);
                if (m_gameStats is not null)
                {
                    m_gameStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", iCOLUMN_WIDTH_DESCRIPTION, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_gameStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", iCOLUMN_WIDTH_VALUE, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                }
            }

            // Matches
            UIPanel? tabMatchStats = m_tabStrip.AddTab(Localization.Get("tabMatches"));
            if (tabMatchStats is not null)
            {
                // Offer list
                m_listStats = ListView.Create<UIMatchStatsRow>(tabMatchStats, "ScrollbarTrack", 0.7f, width - 20f, tabMatchStats.height);
                if (m_listStats is not null)
                {
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, "Material", "Material", iCOLUMN_MATERIAL_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_COUNT, "OUT #", "Transfer offer priority", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_AMOUNT, "OUT Amount", "Transfer Offer Amount", iCOLUMN_BIGGER_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_COUNT, "IN #", "IN Count", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_AMOUNT, "IN Amount", "Reason for transfer request", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_COUNT, "Matches", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_AMOUNT, "Match Amount", "Offer description", iCOLUMN_BIGGER_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_DISTANCE, "Avg Dist.", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_OUTSIDE, "Outside", "Outside connection count", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_PERCENT, "OUT%", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_PERCENT, "IN%", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                }
            }
            
            m_tabStrip.SelectTabIndex(0);
            isVisible = true;
            UpdatePanel();
        }

        public bool HandleEscape()
        {
            if (isVisible)
            {
                Hide();
                return true;
            }
            return false;
        }

        public void TogglePanel()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void OnTabChanged(int index)
        {
            UpdatePanel();
        }

        new public void Show()
        {
            UpdatePanel();
            base.Show();
            UpdatePanel();
        }

        new public void Hide()
        {
            base.Hide();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
            if (m_listStats is not null)
            {
                m_listStats.Clear();
            }
        }

        private List<GeneralContainer> GetGeneralStats()
        {
            List<GeneralContainer> list = new List<GeneralContainer>();

            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Match time
                bool bPathDistance = SaveGameSettings.GetSettings().PathDistanceServices == (int)SaveGameSettings.PathDistanceAlgorithm.PathDistance ||
                                     SaveGameSettings.GetSettings().PathDistanceGoods == (int)SaveGameSettings.PathDistanceAlgorithm.PathDistance;

                // Transfer manager match statistics
                list.Add(new GeneralContainer("Match Cycle", $"{CustomTransferDispatcher.Instance.Cycle}"));
                list.Add(new GeneralContainer("Match Cycle Time", $"{CustomTransferDispatcher.Instance.LastCycleMilliseconds} ms"));
                list.Add(new GeneralContainer("Total Match Jobs", $"{TransferManagerStats.s_TotalMatchJobs}"));
                if (bPathDistance)
                {
                    list.Add(new GeneralContainer("Total Path Distance Match Jobs", $"{TransferManagerStats.s_TotalPathDistanceMatchJobs}"));
                }
                if (ModSettings.GetSettings().StatisticsEnabled)
                {
                    list.Add(new GeneralContainer("Total Matches", MatchStats.GetTotalMatches().ToString()));
                    list.Add(new GeneralContainer("Matches / Second", MatchStats.GetMatchesPerSecond().ToString("N0")));
                    list.Add(new GeneralContainer("Average Distance", MatchStats.GetAverageDistance()));
                }

                // Add separator
                list.Add(new GeneralContainer("", ""));

                // Job stats
                list.Add(new GeneralContainer("Average Match Job Time", $"{TransferManagerStats.GetAverageMatchTime().ToString("F")} ms"));       
                if (bPathDistance)
                {
                    list.Add(new GeneralContainer("Average Path Distance Match Job Time", $"{TransferManagerStats.GetAveragePathDistanceMatchTime().ToString("F")} ms"));
                }
                
                list.Add(new GeneralContainer($"Longest Cycle Match Job Time (Cycle {TransferManagerStats.CycleData.m_cycle})", $"{((double)TransferManagerStats.CycleData.m_ticks * 0.0001).ToString("F")}ms ({TransferManagerStats.CycleData.m_material})"));
                list.Add(new GeneralContainer("Longest Match Job Time", $"{((double)TransferManagerStats.s_longestMatchTicks * 0.0001).ToString("F")}ms ({TransferManagerStats.s_longestMaterial})"));
                list.Add(new GeneralContainer("Largest Match Job", $"IN: {TransferManagerStats.s_largestIncoming} OUT: {TransferManagerStats.s_largestOutgoing} ({TransferManagerStats.s_largestMaterial})"));

                // Add separator
                list.Add(new GeneralContainer("", ""));

                // Dropped reasons indicate performance issues.
                list.Add(new GeneralContainer("Dropped Reason Count", CustomTransferDispatcher.Instance.DroppedReasons.ToString()));
                list.Add(new GeneralContainer("Invalid Transfer Objects", $"{TransferManagerStats.GetTotalInvalidObjectCount()} (Building:{TransferManagerStats.s_iInvalidBuildingObjects}, Vehicle:{TransferManagerStats.s_iInvalidVehicleObjects}, Citizen:{TransferManagerStats.s_iInvalidCitizenObjects})"));

                // Pathing
                list.Add(new GeneralContainer("Total Citizen Path Fail Count", HumanAIPathfindFailure.s_pathFailCount.ToString()));
                list.Add(new GeneralContainer("Total Vehicle Path Fail Count", CarAIPathfindFailurePatch.s_pathFailCount.ToString()));
                list.Add(new GeneralContainer("Current Path Fail Count", PathFindFailure.GetPathFailureCount().ToString()));
                list.Add(new GeneralContainer("Current Outside Path Fail Count", PathFindFailure.GetOutsidePathFailureCount().ToString()));
                if (bPathDistance)
                {
                    list.Add(new GeneralContainer("No Road Access Fail Count", RoadAccessData.Count.ToString()));
                }
            }
            else
            {
                list.Add(new GeneralContainer("Transfer Manager Disabled", "Please enable transfer manager to view statistics"));
            }

            return list;
        }

        private List<GeneralContainer> GetTransferManagerStats() 
        {
            List<GeneralContainer> list = new List<GeneralContainer>();

            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Threads
                list.Add(new GeneralContainer("Thread Count", $"{TransferManagerThread.ThreadCount}"));
                list.Add(new GeneralContainer("Running Threads", $"{TransferManagerThread.RunningThreads()}"));
                list.Add(new GeneralContainer("Max Running Threads", $"{TransferManagerThread.MaxRunningThreads()}"));

                // Job queue
                list.Add(new GeneralContainer("Current Job Queue Depth", TransferJobQueue.Instance.Count().ToString()));
                list.Add(new GeneralContainer("Max Job Queue Depth", $"{TransferJobQueue.Instance.GetMaxUsageCount()}"));
                list.Add(new GeneralContainer("Max Job Pool Usage", TransferJobPool.Instance.GetMaxUsageCount().ToString()));

                // Result queue
                list.Add(new GeneralContainer("Current Transfer Result Queue Depth", CustomTransferDispatcher.Instance.GetResultQueue().GetCount().ToString()));
                list.Add(new GeneralContainer("Max Transfer Result Queue Depth", CustomTransferDispatcher.Instance.GetResultQueue().GetMaxUsageCount().ToString()));
            }
            else
            {
                list.Add(new GeneralContainer("Transfer Manager Disabled", "Please enable transfer manager to view statistics"));
            }

            return list;
        }

        private List<GeneralContainer> GetGameStats()
        {
            List<GeneralContainer> list = new List<GeneralContainer>();

            // Game resources
            list.Add(new GeneralContainer("Building Count", $"{BuildingManager.instance.m_buildingCount} / {BuildingManager.instance.m_buildings.m_size}"));
            list.Add(new GeneralContainer("Vehicle Count", $"{VehicleManager.instance.m_vehicleCount} / {VehicleManager.instance.m_vehicles.m_size}"));
            list.Add(new GeneralContainer("Citizen Count", $"{CitizenManager.instance.m_citizenCount} / {CitizenManager.instance.m_citizens.m_size}"));
            list.Add(new GeneralContainer("CitizenUnit Count", $"{CitizenManager.instance.m_unitCount} / {CitizenManager.instance.m_units.m_size}"));
            list.Add(new GeneralContainer("CitizenInstance Count", $"{CitizenManager.instance.m_instanceCount} / {CitizenManager.instance.m_instances.m_size}"));
            list.Add(new GeneralContainer("Path Units", $"{PathManager.instance.m_pathUnitCount} / {PathManager.instance.m_pathUnits.m_size}"));
            list.Add(new GeneralContainer("Node Count", $"{NetManager.instance.m_nodeCount} / {NetManager.instance.m_nodes.m_size}"));
            list.Add(new GeneralContainer("Segment Count", $"{NetManager.instance.m_segmentCount} / {NetManager.instance.m_segments.m_size}"));

            // Add separator
            list.Add(new GeneralContainer("", ""));

            list.Add(new GeneralContainer("SimulationStep Average", $"{(SimulationManager.instance.m_simulationProfiler.m_averageStepDuration * 0.0001).ToString("F")}ms"));
            list.Add(new GeneralContainer("SimulationStep Last ", $"{(SimulationManager.instance.m_simulationProfiler.m_lastStepDuration * 0.0001).ToString("F")}ms"));
            list.Add(new GeneralContainer("SimulationStep Peak", $"{(SimulationManager.instance.m_simulationProfiler.m_peakStepDuration * 0.0001).ToString("F")}ms"));

            return list;
        }

        IEnumerator UpdatePanelCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);
                UpdatePanel();
            }
        }

        public void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            if (m_tabStrip is not null)
            {
                switch ((Tabs)m_tabStrip.GetSelectTabIndex())
                {
                    case Tabs.TAB_GENERAL:
                        {
                            List<GeneralContainer> list = GetGeneralStats();
                            m_generalStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }

                    case Tabs.TAB_TRANSFER_MANAGER:
                        {
                            List<GeneralContainer> list = GetTransferManagerStats();
                            m_transferManagerStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }
                        
                    case Tabs.TAB_GAME:
                        {
                            List<GeneralContainer> list = GetGameStats();
                            m_gameStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }
                    case Tabs.TAB_STATS:
                        {
                            if (ModSettings.GetSettings().StatisticsEnabled && m_listStats is not null)
                            {
                                // Currently only reason up to Biofuel bus are used.
                                StatsContainer[] statsContainers = new StatsContainer[(int)CustomTransferReason.iLAST_REASON + 2];

                                // Totals first
                                statsContainers[0] = MatchStats.s_Stats[MatchStats.iMATERIAL_TOTAL_LOCATION];
                                statsContainers[0].m_material = TransferReason.None;

                                // Now add rest of materials in order
                                for (int i = 0; i <= (int)CustomTransferReason.iLAST_REASON; i++)
                                {
                                    statsContainers[i + 1] = MatchStats.s_Stats[i];
                                }
                                m_listStats.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = statsContainers,
                                    m_size = statsContainers.Length,
                                };
                            }
                            break;
                        }
                }
            }        
        }

        public override void OnDestroy()
        {
            if (m_coroutine is not null)
            {
                StopCoroutine(m_coroutine);
            }
            if (m_listStats is not null)
            {
                Destroy(m_listStats.gameObject);
                m_listStats = null;
            }
            if (m_generalStats is not null)
            {
                Destroy(m_generalStats.gameObject);
                m_generalStats = null;
            }
            if (m_tabStrip is not null)
            {
                Destroy(m_tabStrip.gameObject);
                m_tabStrip = null;
            }
            if (Instance is not null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}