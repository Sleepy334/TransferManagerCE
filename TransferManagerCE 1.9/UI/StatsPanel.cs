using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Patch;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatsPanel : UIPanel
    {
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;
        public const int iLISTVIEW_STATS_HEIGHT = 300;

        public const int iCOLUMN_MATERIAL_WIDTH = 120;
        public const int iCOLUMN_WIDTH = 60;
        public const int iCOLUMN_BIGGER_WIDTH = 95;
        public const int iCOLUMN_LARGE_WIDTH = 400;

        public static StatsPanel? Instance = null;

        private UITitleBar? m_title = null;

        private UITabStrip? m_tabStrip = null;
        private ListView m_generalStats = null;
        private ListView m_listStats = null;
        

        public StatsPanel() : base()
        {
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(StatsPanel)) as StatsPanel;
                if (Instance == null)
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
            height = 640;
            opacity = 0.95f;
            padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            CenterToParent();

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = Localization.Get("titleTransferStatsPanel"); ;

            m_tabStrip = UITabStrip.Create(this, width - 20f, height - m_title.height - 10 - 20, OnTabChanged);
            
            // General
            UIPanel? tabGeneral = m_tabStrip.AddTab(Localization.Get("tabGeneral"));
            if (tabGeneral != null)
            {
                // Offer list
                m_generalStats = ListView.Create<UIGeneralStatsRow>(tabGeneral, "ScrollbarTrack", 0.7f, width - 20f, height - m_title.height - 12);
                if (m_generalStats != null)
                {
                    m_generalStats.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, "Description", "Material", iCOLUMN_LARGE_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_generalStats.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, "Value", "Transfer offer priority", iCOLUMN_LARGE_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                }
            }

            // Matches
            UIPanel? tabMatchStats = m_tabStrip.AddTab(Localization.Get("tabMatches"));
            if (tabMatchStats != null)
            {
                // Offer list
                m_listStats = ListView.Create<UIMatchStatsRow>(tabMatchStats, "ScrollbarTrack", 0.7f, width - 20f, height - m_title.height - 12);
                if (m_listStats != null)
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
            if (m_listStats != null)
            {
                m_listStats.Clear();
            }
        }

        private List<GeneralContainer> GetGeneralStats()
        {
            List<GeneralContainer> list = new List<GeneralContainer>();

            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Transfer manager match statistics
                if (ModSettings.GetSettings().StatisticsEnabled)
                {
                    list.Add(new GeneralContainer("Total Matches", MatchStats.GetTotalMatches().ToString()));
                    list.Add(new GeneralContainer("Matches / Second", MatchStats.GetMatchesPerSecond().ToString("N0")));
                    list.Add(new GeneralContainer("Average Distance", MatchStats.GetAverageDistance()));
                }
                else
                {
                    list.Add(new GeneralContainer("Match Statistics Disabled", "Please enable statistics in global settings."));
                }

                // Threads
                list.Add(new GeneralContainer("Thread Count", $"{TransferManagerThread.ThreadCount}"));
                list.Add(new GeneralContainer("Max Running Threads", $"{TransferManagerThread.MaxRunningThreads()}"));

                // Match time
                list.Add(new GeneralContainer("Total Match Jobs", $"{TransferManagerStats.s_TotalMatchJobs}"));
                list.Add(new GeneralContainer("Average Match Job Time (ms)", $"{Math.Round(TransferManagerStats.GetAverageMatchTime(), 2)} ms"));
                list.Add(new GeneralContainer("Longest Match Job Time (ms)", $"{TransferManagerStats.s_longestMatch} ms ({TransferManagerStats.s_longestMaterial})"));
                list.Add(new GeneralContainer("Largest Match Job", $"IN: {TransferManagerStats.s_largestIncoming} OUT: {TransferManagerStats.s_largestOutgoing} ({TransferManagerStats.s_largestMaterial})"));

                // Transfer manager queue's
                list.Add(new GeneralContainer("Current Job Queue Depth", TransferJobQueue.Instance.Count().ToString()));
                list.Add(new GeneralContainer("Current Transfer Result Queue Depth", TransferResultQueue.Instance.GetCount().ToString()));
                list.Add(new GeneralContainer("Max Job Queue Depth", $"{TransferJobQueue.Instance.GetMaxUsageCount()}"));
                list.Add(new GeneralContainer("Max Job Pool Usage", TransferJobPool.Instance.GetMaxUsageCount().ToString()));
                list.Add(new GeneralContainer("Max Transfer Result Queue Depth", TransferResultQueue.GetMaxUsageCount().ToString()));

                // Pathing
                list.Add(new GeneralContainer("Path Fail Count", PathFindFailure.GetPathFailureCount().ToString()));
                list.Add(new GeneralContainer("Outside Path Fail Count", PathFindFailure.GetOutsidePathFailureCount().ToString()));

                // Game resources
                list.Add(new GeneralContainer("Game Path Units", $"{PathManager.instance.m_pathUnitCount} / {PathManager.instance.m_pathUnits.m_size}"));
                list.Add(new GeneralContainer("Game Vehicle Count", $"{VehicleManager.instance.m_vehicleCount} / {VehicleManager.instance.m_vehicles.m_size}"));
                list.Add(new GeneralContainer("Game CitizenUnit Count", $"{CitizenManager.instance.m_unitCount} / {CitizenManager.instance.m_units.m_size}"));
                list.Add(new GeneralContainer("Game Citizen Count", $"{CitizenManager.instance.m_citizenCount} / {CitizenManager.instance.m_citizens.m_size}"));
            }
            else
            {
                list.Add(new GeneralContainer("Transfer Manager Disabled", "Please enable transfer manager to view statistics"));
            }

            return list;
        }

        public void UpdatePanel()
        {
            if (m_tabStrip != null)
            {
                switch (m_tabStrip.GetSelectTabIndex())
                {
                    case 0:
                        List<GeneralContainer> list = GetGeneralStats();
                        m_generalStats.GetList().rowsData = new FastList<object>
                        {
                            m_buffer = list.ToArray(),
                            m_size = list.Count,
                        };
                        break;
                    case 1:
                        {
                            if (ModSettings.GetSettings().StatisticsEnabled && m_listStats != null)
                            {
                                // Currently only reason up to Biofuel bus are used.
                                StatsContainer[] statsContainers = new StatsContainer[(int)TransferReason.BiofuelBus + 2];

                                // Totals first
                                statsContainers[0] = MatchStats.s_Stats[MatchStats.iMATERIAL_TOTAL_LOCATION];
                                statsContainers[0].m_material = TransferReason.None;

                                // Now add rest of materials in order
                                for (int i = 0; i <= (int)TransferReason.BiofuelBus; i++)
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
            if (m_listStats != null)
            {
                Destroy(m_listStats.gameObject);
                m_listStats = null;
            }
            if (m_generalStats != null)
            {
                Destroy(m_generalStats.gameObject);
                m_generalStats = null;
            }
            if (m_tabStrip != null)
            {
                Destroy(m_tabStrip.gameObject);
                m_tabStrip = null;
            }
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}