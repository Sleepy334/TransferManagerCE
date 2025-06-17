using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.CustomManager.Stats;
using TransferManagerCE.Data.StatsPanel;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.CustomTransferReason;
using static TransferManagerCE.UITabStrip;

namespace TransferManagerCE
{
    public class StatsPanel : UIMainPanel<StatsPanel>
    {
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;

        private UITitleBar? m_title = null;
        private UITabStrip? m_tabStrip = null;
        private ListView? m_generalStats = null;
        private ListView? m_transferManagerStats = null;
        private ListView? m_gameStats = null;
        private ListView? m_panelStats = null;
        private ListView? m_listStats = null;

        private enum Tabs
        {
            TAB_GAME = 0,
            TAB_GENERAL = 1,
            TAB_TRANSFER_MANAGER = 2,
            TAB_MATCHES = 3,
            TAB_PANELS = 4,
        }

        public StatsPanel() : base()
        {
            PanelUpdateRate = 1000; // 1 second
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
            m_title = UITitleBar.Create(this, Localization.Get("titleTransferStatsPanel"), "Transfer", TransferManagerMod.Instance.LoadResources(), OnCloseClick);
            if (m_title != null)
            {
                m_title.SetupButtons();
            }

            m_tabStrip = UITabStrip.Create(TabStyle.Generic, this, width - 20f, height - m_title.height - 10, OnTabChanged);
            m_tabStrip.padding = new RectOffset(iMARGIN, iMARGIN, 4, iMARGIN);

            // Game
            UIPanel? tabGame = m_tabStrip.AddTab(Localization.Get("tabGame"));
            if (tabGame is not null)
            {
                // Offer list
                m_gameStats = ListView.Create<UIGeneralStatsRow>(tabGame, "ScrollbarTrack", 0.7f, width - 20f, tabGame.height);
                if (m_gameStats is not null)
                {
                    m_gameStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", UIGeneralStatsRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_gameStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", UIGeneralStatsRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                }
            }

            // General
            UIPanel? tabGeneral = m_tabStrip.AddTab(Localization.Get("tabGeneral"));
            if (tabGeneral is not null)
            {
                // Offer list
                m_generalStats = ListView.Create<UIGeneralStatsRow>(tabGeneral, "ScrollbarTrack", 0.7f, width - 20f, tabGeneral.height);
                if (m_generalStats is not null)
                {
                    m_generalStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", UIGeneralStatsRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_generalStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", UIGeneralStatsRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
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
                    m_transferManagerStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", UIGeneralStatsRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_transferManagerStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", UIGeneralStatsRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
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
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, "Material", "Material", UIMatchStatsRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);

                    // Job stats
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_JOB_AVG, "Job Avg", "Average Job Time (ms)", UIMatchStatsRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_JOB_LAST, "Job Last", "Last Job Time (ms)", UIMatchStatsRow.ColumnWidths[2], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_JOB_MAX, "Job Max", "Max Job Time (ms)", UIMatchStatsRow.ColumnWidths[3], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);

                    // Match stats
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_AMOUNT, "Match Amount", "", UIMatchStatsRow.ColumnWidths[4], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_AMOUNT, "OUT Amount", "", UIMatchStatsRow.ColumnWidths[5], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_AMOUNT, "IN Amount", "", UIMatchStatsRow.ColumnWidths[6], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_DISTANCE, "Avg Dist.", "Average Match Distance (km)", UIMatchStatsRow.ColumnWidths[7], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_OUTSIDE, "Outside", "", UIMatchStatsRow.ColumnWidths[8], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);

                    m_listStats.Header.ResizeLastColumn();

                    // Show sort by match amount descending
                    m_listStats.HandleSort(ListViewRowComparer.Columns.COLUMN_JOB_AVG);
                    m_listStats.HandleSort(ListViewRowComparer.Columns.COLUMN_JOB_AVG);
                }

                // Panels
                UIPanel? tabPanels = m_tabStrip.AddTab(Localization.Get("tabPanels"));
                if (tabPanels is not null)
                {
                    // Offer list
                    m_panelStats = ListView.Create<UIGeneralStatsRow>(tabPanels, "ScrollbarTrack", 0.7f, width - 20f, tabPanels.height);
                    if (m_panelStats is not null)
                    {
                        m_panelStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "", UIGeneralStatsRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                        m_panelStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "", UIGeneralStatsRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    }
                }
            }
            
            m_tabStrip.SelectTabIndex(0);
            isVisible = true;
            UpdatePanel();
        }

        public void OnTabChanged(int index)
        {
            UpdatePanel();
        }

        new public void Show()
        {
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

        private List<StatsBase> GetGameStats()
        {
            List<StatsBase> list = new List<StatsBase>();

            // Game resources
            list.Add(new StatsHeader("Game resources"));
            list.Add(new StatsGroup("Building Count", $"{BuildingManager.instance.m_buildingCount} / {BuildingManager.instance.m_buildings.m_size}"));
            list.Add(new StatsGroup("Vehicle Count", $"{VehicleManager.instance.m_vehicleCount} / {VehicleManager.instance.m_vehicles.m_size}"));
            list.Add(new StatsGroup("Citizen Count", $"{CitizenManager.instance.m_citizenCount} / {CitizenManager.instance.m_citizens.m_size}"));
            list.Add(new StatsGroup("CitizenUnit Count", $"{CitizenManager.instance.m_unitCount} / {CitizenManager.instance.m_units.m_size}"));
            list.Add(new StatsGroup("CitizenInstance Count", $"{CitizenManager.instance.m_instanceCount} / {CitizenManager.instance.m_instances.m_size}"));
            list.Add(new StatsGroup("Path Units", $"{PathManager.instance.m_pathUnitCount} / {PathManager.instance.m_pathUnits.m_size}"));
            list.Add(new StatsGroup("Node Count", $"{NetManager.instance.m_nodeCount} / {NetManager.instance.m_nodes.m_size}"));
            list.Add(new StatsGroup("Segment Count", $"{NetManager.instance.m_segmentCount} / {NetManager.instance.m_segments.m_size}"));

            // Add separator
            list.Add(new StatsSeparator());

            list.Add(new StatsHeader("Simulation"));
            list.Add(new StatsGroup("Step Average", $"{Utils.DisplayTicks(SimulationManager.instance.m_simulationProfiler.m_averageStepDuration)}ms"));
            list.Add(new StatsGroup("Step Last ", $"{Utils.DisplayTicks(SimulationManager.instance.m_simulationProfiler.m_lastStepDuration)}ms"));
            list.Add(new StatsGroup("Step Peak", $"{Utils.DisplayTicks(SimulationManager.instance.m_simulationProfiler.m_peakStepDuration)}ms"));

            return list;
        }

        private List<StatsBase> GetGeneralStats()
        {
            List<StatsBase> list = new List<StatsBase>();

            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Match time
                bool bPathDistance = SaveGameSettings.GetSettings().PathDistanceServices == (int)SaveGameSettings.PathDistanceAlgorithm.PathDistance ||
                                     SaveGameSettings.GetSettings().PathDistanceGoods == (int)SaveGameSettings.PathDistanceAlgorithm.PathDistance;

                // Transfer manager match statistics
                list.Add(new StatsHeader("Match Totals"));
                list.Add(new StatsGroup("Total Match Jobs", $"{TransferManagerStats.s_TotalMatchJobs}"));
                list.Add(new StatsGroup("Total Match Time", $"{Utils.DisplayTicks(TransferManagerStats.s_TotalMatchTimeTicks)} ms"));
                if (bPathDistance)
                {
                    list.Add(new StatsGroup("Total Path Distance Match Jobs", $"{TransferManagerStats.s_TotalPathDistanceMatchJobs}"));
                }
                if (ModSettings.GetSettings().StatisticsEnabled)
                {
                    list.Add(new StatsGroup("Total Matches", MatchStats.GetTotalMatches().ToString()));
                    list.Add(new StatsGroup("Matches / Second", MatchStats.GetMatchesPerSecond().ToString("N0")));
                    list.Add(new StatsGroup("Average Distance", MatchStats.GetAverageDistance()));
                }
                list.Add(new StatsSeparator());

                // Cycle
                CycleJobData cycleData = TransferManagerStats.CycleData.GetLatestCompletedCopy();

                list.Add(new StatsHeader("Latest Match Cycle"));
                list.Add(new StatsGroup("Cycle Number", $"{cycleData.m_cycle}"));
                list.Add(new StatsGroup("Cycle Match Job Count", $"{cycleData.m_jobsCompleted}"));
                list.Add(new StatsGroup("Cycle Simulation Time", $"{Utils.DisplayTicks(cycleData.DurationTicks())} ms"));
                list.Add(new StatsGroup("Cycle Calculation Time", $"{Utils.DisplayTicks(cycleData.m_totalTicks)} ms"));
                list.Add(new StatsGroup($"Longest Cycle Match Job Time", $"{Utils.DisplayTicks(cycleData.m_ticks)}ms ({cycleData.m_material})"));
                list.Add(new StatsSeparator());

                // Job stats
                list.Add(new StatsHeader("Job Statistics"));
                list.Add(new StatsGroup("Average Match Job Time", $"{TransferManagerStats.GetAverageMatchTime().ToString("F")} ms"));
                if (bPathDistance)
                {
                    list.Add(new StatsGroup("Average Path Distance Match Job Time", $"{TransferManagerStats.GetAveragePathDistanceMatchTime().ToString("F")} ms"));
                }                
                list.Add(new StatsGroup("Longest Match Job Time", $"{((double)TransferManagerStats.s_longestMatchTicks * 0.0001).ToString("F")}ms ({TransferManagerStats.s_longestMaterial})"));
                list.Add(new StatsGroup("Largest Match Job", $"IN: {TransferManagerStats.s_largestIncoming} OUT: {TransferManagerStats.s_largestOutgoing} ({TransferManagerStats.s_largestMaterial})"));
                list.Add(new StatsSeparator());

                // Pathing
                list.Add(new StatsHeader("Pathing"));
                list.Add(new StatsGroup("Total Citizen Path Fail Count", HumanAIPathfindFailure.s_pathFailCount.ToString()));
                list.Add(new StatsGroup("Total Vehicle Path Fail Count", CarAIPathfindFailurePatch.s_pathFailCount.ToString()));
                list.Add(new StatsGroup("Current Path Fail Count", PathFindFailure.GetPathFailureCount().ToString()));
                list.Add(new StatsGroup("Current Outside Path Fail Count", PathFindFailure.GetOutsidePathFailureCount().ToString()));
                if (bPathDistance)
                {
                    list.Add(new StatsGroup("No Road Access Fail Count", RoadAccessStorage.Count.ToString()));
                }
            }
            else
            {
                list.Add(new StatsBase("Transfer Manager Disabled", "Please enable transfer manager to view statistics"));
            }

            return list;
        }

        private List<StatsBase> GetTransferManagerStats() 
        {
            List<StatsBase> list = new List<StatsBase>();

            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // ------------------------------------------------------------
                // Dropped reasons indicate performance issues.
                list.Add(new StatsHeader("General"));
                list.Add(new StatsGroup("Dropped Reason Count", CustomTransferDispatcher.Instance.DroppedReasons.ToString()));
                list.Add(new StatsGroup("Invalid Building Objects", $"{TransferManagerStats.s_iInvalidBuildingObjects}"));
                list.Add(new StatsGroup("Invalid Vehicle Objects", $"{TransferManagerStats.s_iInvalidVehicleObjects}"));
                list.Add(new StatsGroup("Invalid Citizen Objects", $"{TransferManagerStats.s_iInvalidCitizenObjects}"));
                list.Add(new StatsSeparator());

                // ------------------------------------------------------------
                // Threads
                list.Add(new StatsHeader("Threads"));
                list.Add(new StatsGroup("Thread Count", $"{TransferManagerThread.ThreadCount}"));
                list.Add(new StatsGroup("Running Threads", $"{TransferManagerThread.RunningThreads()}"));
                list.Add(new StatsGroup("Max Running Threads", $"{TransferManagerThread.MaxRunningThreads()}"));
                list.Add(new StatsSeparator());

                // ------------------------------------------------------------
                // Job queue
                list.Add(new StatsHeader("Job Queue"));
                list.Add(new StatsGroup("Current Job Queue Depth", TransferJobQueue.Instance.Count().ToString()));
                list.Add(new StatsGroup("Max Job Queue Depth", $"{TransferJobQueue.Instance.GetMaxUsageCount()}"));
                list.Add(new StatsGroup("Max Job Pool Usage", TransferJobPool.Instance.GetMaxUsageCount().ToString()));
                list.Add(new StatsSeparator());

                // ------------------------------------------------------------
                // Result queue
                list.Add(new StatsHeader("Result Queue"));
                list.Add(new StatsGroup("Current Transfer Result Queue Depth", CustomTransferDispatcher.Instance.GetResultQueue().GetCount().ToString()));
                list.Add(new StatsGroup("Max Transfer Result Queue Depth", CustomTransferDispatcher.Instance.GetResultQueue().GetMaxUsageCount().ToString()));
                list.Add(new StatsSeparator());

                // ------------------------------------------------------------
                // Cache
                list.Add(new StatsHeader("Cache"));

                // Node Links
                list.Add(new StatsGroup("Graph Generation Count", NodeLinkGraph.s_totalGenerations.ToString()));
                if (NodeLinkGraph.s_totalGenerations > 0)
                {
                    list.Add(new StatsGroup("Graph Average Time", $"{Utils.DisplayTicks(NodeLinkGraph.s_totalGenerationTicks / NodeLinkGraph.s_totalGenerations)}ms"));
                }

                // Path connected
                list.Add(new StatsGroup("Path Connection Generation Count", PathConnected.s_totalGenerations.ToString()));
                if (PathConnected.s_totalGenerations > 0)
                {
                    list.Add(new StatsGroup("Path Connection Average Time", $"{Utils.DisplayTicks(PathConnected.s_totalGenerationTicks / PathConnected.s_totalGenerations)}ms"));
                }
            }
            else
            {
                list.Add(new StatsBase("Transfer Manager Disabled", "Please enable transfer manager to view statistics"));
            }

            return list;
        }

        private List<StatsBase> GetPanelStats()
        {
            List<StatsBase> list = new List<StatsBase>();

            // Issue Panel
            list.Add(new StatsHeader("Issue Detector"));
            list.Add(new StatsGroup("Update Count", $"{TransferIssueHelper.s_totalUpdates}"));
            list.Add(new StatsGroup("Update Last", $"{(TransferIssueHelper.s_lastUpdateTicks * 0.0001).ToString("F")}ms"));
            list.Add(new StatsGroup("Update Peak", $"{(TransferIssueHelper.s_maxUpdateTicks * 0.0001).ToString("F")}ms"));
            if (TransferIssueHelper.s_totalUpdates > 0)
            {
                list.Add(new StatsGroup("Update Average", $"{((TransferIssueHelper.s_totalUpdateTicks / TransferIssueHelper.s_totalUpdates) * 0.0001).ToString("F")}ms"));
            }
            else
            {
                list.Add(new StatsGroup("Update Average", $"0.00ms"));
            }

            // Add separator
            list.Add(new StatsSeparator());

            // Building Panel
            list.Add(new StatsHeader("Building Panel")); 
            list.Add(new StatsGroup("Update Count", $"{BuildingPanel.s_totalUpdates}"));
            list.Add(new StatsGroup("Update Last", $"{(BuildingPanel.s_lastUpdateTicks * 0.0001).ToString("F")}ms"));
            list.Add(new StatsGroup("Update Peak", $"{(BuildingPanel.s_maxUpdateTicks * 0.0001).ToString("F")}ms"));
            if (BuildingPanel.s_totalUpdates > 0)
            {
                list.Add(new StatsGroup("Update Average", $"{((BuildingPanel.s_totalUpdateTicks / BuildingPanel.s_totalUpdates) * 0.0001).ToString("F")}ms"));
            }
            else
            {
                list.Add(new StatsGroup("Update Average", $"0.00ms"));
            }

            return list;
        }

        protected override void UpdatePanel()
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
                            List<StatsBase> list = GetGeneralStats();
                            m_generalStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }

                    case Tabs.TAB_TRANSFER_MANAGER:
                        {
                            List<StatsBase> list = GetTransferManagerStats();
                            m_transferManagerStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }
                        
                    case Tabs.TAB_GAME:
                        {
                            List<StatsBase> list = GetGameStats();
                            m_gameStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }
                    case Tabs.TAB_MATCHES:
                        {
                            if (ModSettings.GetSettings().StatisticsEnabled && m_listStats is not null)
                            {
                                // Currently only reason up to Biofuel bus are used.
                                List<MatchStatsData> matches = new List<MatchStatsData>();

                                // Now add rest of materials in order
                                foreach (CustomTransferReason.Reason reason in Enum.GetValues(typeof(CustomTransferReason.Reason)))
                                {
                                    if (reason != Reason.None)
                                    {
                                        matches.Add(MatchStats.s_Stats[(int)reason]);
                                    }
                                }

                                matches.Sort();

                                // Add a blank line
                                MatchStatsSeparator separator = new MatchStatsSeparator();
                                matches.Insert(0, separator);

                                // Totals first
                                MatchStatsData totals = MatchStats.s_Stats[MatchStats.iMATERIAL_TOTAL_LOCATION];
                                totals.m_material = TransferReason.None;
                                matches.Insert(0, totals);

                                m_listStats.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = matches.ToArray(),
                                    m_size = matches.Count,
                                };
                            }
                            break;
                        }
                    case Tabs.TAB_PANELS:
                        {
                            List<StatsBase> list = GetPanelStats();
                            m_panelStats.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = list.ToArray(),
                                m_size = list.Count,
                            };
                            break;
                        }
                }

                // Clear the match stats as there are lots of them.
                if ((Tabs)m_tabStrip.GetSelectTabIndex() != Tabs.TAB_MATCHES)
                {
                    m_listStats.Clear();
                }
            }        
        }

        public override void OnDestroy()
        {
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
        }
    }
}