using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferIssuePanel : UIPanel
    {
        enum TabOrder
        {
            TAB_PATHING,
            TAB_OUTSIDE,
            TAB_DEAD,
            TAB_SICK,
            TAB_GOODS,
        }
        
        const int iMARGIN = 8;

        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_VALUE = 40;
        public const int iCOLUMN_WIDTH_TIME = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_MATERIAL_WIDTH = 150;
        public const int iCOLUMN_VEHICLE_WIDTH = 180;
        public const int iCOLUMN_WIDTH_PATHING_BUILDING = 240;
        public const int iCOLUMN_DESCRIPTION_WIDTH = 300;

        public static TransferIssuePanel? Instance = null;

        private UITitleBar? m_title = null;
        private ListView? m_listPathing = null;
        private ListView? m_listOutside = null;
        private ListView? m_listSick = null;
        private ListView? m_listGoods = null;
        private ListView? m_listDead = null;
        private UITabStrip? m_tabStrip = null;
        TransferIssueHelper? m_issueHelper = null;
        private UICheckBox? m_chkShowIssuesWithVehicles = null;
        private UIButton? m_btnReset = null;
        public TransferIssuePanel() : base()
        {
            m_issueHelper = new TransferIssueHelper();
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(TransferIssuePanel)) as TransferIssuePanel;
                if (Instance == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Issue Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "TransferIssuePanel";
            width = 700;
            height = 500;
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
            m_title.title = Localization.Get("titleTransferIssuesPanel");

            m_tabStrip = UITabStrip.Create(this, width - 20f, height - m_title.height - 40 , OnTabChanged);
            
            // Pathing
            UIPanel? tabPathing = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesPathing"));
            if (tabPathing != null)
            {
                tabPathing.autoLayout = true;
                tabPathing.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listPathing = ListView.Create<UIPathRow>(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height - 10);
                if (m_listPathing != null)
                {
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }

                m_tabStrip.SelectTabIndex(0);
            }

            // Pathing
            UIPanel? tabOutside = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesOutside"));
            if (tabOutside != null)
            {
                tabOutside.autoLayout = true;
                tabOutside.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listOutside = ListView.Create<UIPathRow>(tabOutside, "ScrollbarTrack", 0.8f, tabOutside.width, tabOutside.height - 10);
                if (m_listOutside != null)
                {
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }
            }
            
            // Dead
            UIPanel? tabDead = m_tabStrip.AddTabIcon("NotificationIconVerySick", Localization.Get("tabTransferIssuesDead"));
            if (tabDead != null)
            {
                tabDead.autoLayout = true;
                tabDead.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listDead = ListView.Create<UIIssueRow>(tabDead, "ScrollbarTrack", 0.7f, tabDead.width, tabDead.height - 10);
                if (m_listDead != null)
                {
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listDeadColumn1"), "No. of Dead", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "Death timer", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listDead.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Sick
            UIPanel? tabSick = m_tabStrip.AddTabIcon("ToolbarIconHealthcare", Localization.Get("tabTransferIssuesSick"));
            if (tabSick != null)
            {
                tabSick.autoLayout = true;
                tabSick.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listSick = ListView.Create<UIIssueRow>(tabSick, "ScrollbarTrack", 0.7f, tabSick.width, tabSick.height - 10);
                if (m_listSick != null)
                {
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listSickColumn1"), "No. of Sick", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "Sick timer", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listSick.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Sick
            UIPanel? tabGoods = m_tabStrip.AddTabIcon("IconPolicyIndustrySpace", Localization.Get("tabTransferIssuesGoods"));
            if (tabGoods != null)
            {
                tabGoods.autoLayout = true;
                tabGoods.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listGoods = ListView.Create<UIIssueRow>(tabGoods, "ScrollbarTrack", 0.7f, tabGoods.width, tabGoods.height - 10);
                if (m_listGoods != null)
                {
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("tabTransferIssuesGoods"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listGoods.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }
            
            UIHelper helper = new UIHelper(this);
            m_chkShowIssuesWithVehicles = (UICheckBox)helper.AddCheckbox(Localization.Get("optionShowIssuesWithVehiclesOnRoute"), ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute, OnShowIssuesClicked);
            m_chkShowIssuesWithVehicles.isVisible = false;

            m_btnReset = UIUtils.AddButton(this, Localization.Get("btnResetPathingStatistics"), 200, 30);
            m_btnReset.eventClick += OnReset;

            isVisible = true;
            UpdatePanel();
        }

        private void OnShowIssuesClicked(bool bEnabled)
        {
            ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute = bEnabled;
            ModSettings.GetSettings().Save();
        }

        private void OnReset(UIComponent component, UIMouseEventParameter eventParam)
        {
            PathFindFailure.Reset();
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
            if (m_listPathing != null) 
            {
                m_listPathing.Clear();
            }
            if (m_listOutside != null)
            {
                m_listOutside.Clear();
            }
            if (m_listDead != null)
            {
                m_listDead.Clear();
            }
            if (m_listSick != null)
            {
                m_listSick.Clear();
            }
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransferIssueThread.HideTransferIssuePanel();
        }

        public List<PathingContainer> GetPathingIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            Dictionary<Util.PATHFINDPAIR, long> failures = Util.PathFindFailure.GetPathFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in failures)
            {
                list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target));
            }

            return list;
        }

        public List<PathingContainer> GetOutsideIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            Dictionary<Util.PATHFINDPAIR, long> outsideFailures = Util.PathFindFailure.GetOutsideFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in outsideFailures)
            {
                list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target));
            }

            return list;
        }

        public void OnTabChanged(int index)
        {
            UpdatePanel();
        }

        public void UpdatePanel()
        {
            if (m_tabStrip != null) {
                if (m_chkShowIssuesWithVehicles != null && m_btnReset != null)
                {
                    switch ((TabOrder)m_tabStrip.GetSelectTabIndex())
                    {
                        case TabOrder.TAB_PATHING:
                        case TabOrder.TAB_OUTSIDE:
                            {
                                m_chkShowIssuesWithVehicles.isVisible = false;
                                m_btnReset.isVisible = true;
                                break;
                            }
                        default:
                            {
                                m_chkShowIssuesWithVehicles.isVisible = true;
                                m_btnReset.isVisible = false;
                                break;
                            }
                    }
                }

                switch ((TabOrder) m_tabStrip.GetSelectTabIndex())
                {
                    case TabOrder.TAB_PATHING:
                        {
                            if (m_listPathing != null)
                            {
                                List<PathingContainer> list = GetPathingIssues();
                                list.Sort();
                                m_listPathing.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };

                                // Update tab to show count
                                m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + list.Count + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                            }
                        }
                        break;
                    case TabOrder.TAB_OUTSIDE:
                        {
                            if (m_listOutside != null)
                            {
                                List<PathingContainer> list = GetOutsideIssues();
                                list.Sort();
                                m_listOutside.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };

                                // Update tab to show count
                                m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + list.Count + ")");
                            }
                        }
                        break;
                    case TabOrder.TAB_DEAD:
                        {
                            if (m_issueHelper != null)
                            {
                                m_issueHelper.UpdateIssues();
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferReason.Dead);
                                list.Sort();
                                m_listDead.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };
                            }

                            // Update tab to show count
                            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                        }
                        break;
                    case TabOrder.TAB_SICK:
                        {
                            if (m_issueHelper != null)
                            {
                                m_issueHelper.UpdateIssues();
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferReason.Sick);
                                list.Sort();
                                m_listSick.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };
                            }

                            // Update tab to show count
                            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                        }
                        break;
                    case TabOrder.TAB_GOODS:
                        {
                            if (m_issueHelper != null)
                            {
                                m_issueHelper.UpdateIssues();
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferReason.Goods);
                                list.Sort();
                                m_listGoods.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };
                            }

                            // Update tab to show count
                            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                        }
                        break;
                }
            }
        }

        public override void OnDestroy()
        {
            if (m_listPathing != null)
            {
                Destroy(m_listPathing.gameObject);
            }
            if (m_listDead != null)
            {
                Destroy(m_listDead.gameObject);
            }
            if (m_listSick != null)
            {
                Destroy(m_listSick.gameObject);
            }
            if (m_listGoods != null)
            {
                Destroy(m_listGoods.gameObject);
            }
            if (m_chkShowIssuesWithVehicles != null)
            {
                Destroy(m_chkShowIssuesWithVehicles.gameObject);
            }
            if (m_title != null)
            {
                Destroy(m_title.gameObject);
            }
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}
