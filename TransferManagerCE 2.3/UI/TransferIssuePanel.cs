using ColossalFramework.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TransferManagerCE.Common;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManagerCE.UITabStrip;

namespace TransferManagerCE.UI
{
    public class TransferIssuePanel : UIPanel
    {
        enum TabOrder
        {
            TAB_PATHING,
            TAB_OUTSIDE,
            TAB_ROAD_ACCESS,
            TAB_DEAD,
            TAB_SICK,
            TAB_GOODS_IN,
            TAB_GOODS_OUT,
        }

        const int iMARGIN = 8;

        public const int iCOLUMN_WIDTH_VALUE = 40;
        public const int iCOLUMN_WIDTH_TIME = 60;
        public const int iCOLUMN_VEHICLE_WIDTH = 210;
        public const int iCOLUMN_WIDTH_PATHING_BUILDING = 240;
        public const int iCOLUMN_DESCRIPTION_WIDTH = 300;

        public static TransferIssuePanel? Instance = null;

        private UITitleBar? m_title = null;
        private ListView? m_listPathing = null;
        private ListView? m_listOutside = null;
        private ListView? m_listRoadAccess = null;
        private ListView? m_listSick = null;
        private ListView? m_listDead = null;
        private ListView? m_listGoodsIn = null;
        private ListView? m_listGoodsOut = null;

        private UITabStrip? m_tabStrip = null;
        
        private UICheckBox? m_chkShowIssuesWithVehicles = null;
        private UIButton? m_btnResetPathing = null;
        private UIButton? m_btnResetRoadAccess = null;
        private Coroutine? m_coroutine = null;

        private TransferIssueHelper m_issueHelper = new TransferIssueHelper();

        public TransferIssuePanel() : base()
        {
            m_coroutine = StartCoroutine(UpdatePanelCoroutine(4));
        }

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(TransferIssuePanel)) as TransferIssuePanel;
                if (Instance is null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Issue Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "TransferIssuePanel";
            width = 800;
            height = 520;
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
            m_title.title = Localization.Get("titleTransferIssuesPanel");

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height - m_title.height;
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.autoLayoutPadding = new RectOffset(iMARGIN, iMARGIN, 4, 4);

            m_tabStrip = UITabStrip.Create(TabStyle.Generic, mainPanel, mainPanel.width - 2 * iMARGIN, mainPanel.height - 50, OnTabChanged);
            //m_tabStrip.backgroundSprite = "InfoviewPanel";
            //m_tabStrip.color = Color.red;

            // Pathing
            UIPanel? tabPathing = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesPathing"), 150f);
            if (tabPathing is not null)
            {
                tabPathing.autoLayout = true;
                tabPathing.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listPathing = ListView.Create<UIPathRow>(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height);
                if (m_listPathing is not null)
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
            UIPanel? tabOutside = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesOutside"), 150f);
            if (tabOutside is not null)
            {
                tabOutside.autoLayout = true;
                tabOutside.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listOutside = ListView.Create<UIPathRow>(tabOutside, "ScrollbarTrack", 0.8f, tabOutside.width, tabOutside.height);
                if (m_listOutside is not null)
                {
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }
            }

            // Road Access
            UIPanel? tabRoadAccess = m_tabStrip.AddTabIcon("ToolbarIconRoads", "0", Localization.Get("tabTransferRoadAccess"), 60f);
            if (tabRoadAccess is not null)
            {
                tabRoadAccess.autoLayout = true;
                tabRoadAccess.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listRoadAccess = ListView.Create<UIRoadAccessRow>(tabRoadAccess, "ScrollbarTrack", 0.7f, tabRoadAccess.width, tabRoadAccess.height);
                if (m_listRoadAccess is not null)
                {
                    m_listRoadAccess.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Building with road access issues", BuildingPanel.iCOLUMN_WIDTH_300, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listRoadAccess.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_300, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }
            }

            UIPanel? tabDead = m_tabStrip.AddTabIcon("NotificationIconVerySick", "0", Localization.Get("tabTransferIssuesDead"), 70f);
            if (tabDead is not null)
            {
                tabDead.autoLayout = true;
                tabDead.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listDead = ListView.Create<UIIssueRow>(tabDead, "ScrollbarTrack", 0.7f, tabDead.width, tabDead.height);
                if (m_listDead is not null)
                {
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listDeadColumn1"), "No. of Dead", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "Death timer", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listDead.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Sick
            UIPanel? tabSick = m_tabStrip.AddTabIcon("ToolbarIconHealthcare", "0", Localization.Get("tabTransferIssuesSick"), 70f);
            if (tabSick is not null)
            {
                tabSick.autoLayout = true;
                tabSick.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listSick = ListView.Create<UIIssueRow>(tabSick, "ScrollbarTrack", 0.7f, tabSick.width, tabSick.height);
                if (m_listSick is not null)
                {
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listSickColumn1"), "No. of Sick", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "Sick timer", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listSick.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Goods IN
            UIPanel? tabGoods = m_tabStrip.AddTabIcon("IconPolicyIndustrySpace", "0", Localization.Get("tabTransferIssuesGoods"), 70f);
            if (tabGoods is not null)
            {
                tabGoods.autoLayout = true;
                tabGoods.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listGoodsIn = ListView.Create<UIIssueRow>(tabGoods, "ScrollbarTrack", 0.7f, tabGoods.width, tabGoods.height);
                if (m_listGoodsIn is not null)
                {
                    m_listGoodsIn.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("tabTransferIssuesGoods"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsIn.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsIn.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsIn.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsIn.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "", iCOLUMN_VEHICLE_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsIn.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listGoodsIn.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Goods OUT
            UIPanel? tabGoodsOut = m_tabStrip.AddTabIcon("IconPolicyNone", "0", Localization.Get("tabTransferIssuesGoodsOut"), 70f);
            if (tabGoodsOut is not null)
            {
                tabGoodsOut.autoLayout = true;
                tabGoodsOut.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listGoodsOut = ListView.Create<UIGoodsOutIssueRow>(tabGoodsOut, "ScrollbarTrack", 0.7f, tabGoodsOut.width, tabGoodsOut.height);
                if (m_listGoodsOut is not null)
                {
                    m_listGoodsOut.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("tabTransferIssuesGoods"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsOut.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsOut.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "", iCOLUMN_DESCRIPTION_WIDTH, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoodsOut.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listGoodsOut.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            UIHelper helper = new UIHelper(mainPanel);
            m_chkShowIssuesWithVehicles = (UICheckBox)helper.AddCheckbox(Localization.Get("optionShowIssuesWithVehiclesOnRoute"), ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute, OnShowIssuesClicked);
            m_chkShowIssuesWithVehicles.isVisible = false;

            m_btnResetPathing = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, mainPanel, Localization.Get("btnResetPathingStatistics"), "", 200, 30, OnReset);
            m_btnResetRoadAccess = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, mainPanel, Localization.Get("btnResetRoadAccess"), "", 200, 30, OnResetRoadAccess);

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
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        private void OnShowIssuesClicked(bool bEnabled)
        {
            ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute = bEnabled;
            ModSettings.GetSettings().Save();
            UpdatePanel();
        }

        private void OnReset(UIComponent component, UIMouseEventParameter eventParam)
        {
            PathFindFailure.Reset();
            UpdatePanel();
        }

        private void OnResetRoadAccess(UIComponent component, UIMouseEventParameter eventParam)
        {
            RoadAccessData.Reset();
            UpdatePanel();
        }

        public void ShowPanel()
        {
            Debug.Log("Show");
            base.Show();
            UpdatePanel();
        }

        public void HidePanel()
        {
            base.Hide();

            if (m_listPathing is not null)
            {
                m_listPathing.Clear();
            }
            if (m_listOutside is not null)
            {
                m_listOutside.Clear();
            }
            if (m_listDead is not null)
            {
                m_listDead.Clear();
            }
            if (m_listSick is not null)
            {
                m_listSick.Clear();
            }
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
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

            if (m_tabStrip is not null) {
                if (m_chkShowIssuesWithVehicles is not null && m_btnResetPathing is not null && m_btnResetRoadAccess is not null)
                {
                    switch ((TabOrder)m_tabStrip.GetSelectTabIndex())
                    {
                        case TabOrder.TAB_PATHING:
                        case TabOrder.TAB_OUTSIDE:
                            {
                                m_chkShowIssuesWithVehicles.isVisible = false;
                                m_btnResetPathing.isVisible = true;
                                m_btnResetRoadAccess.isVisible = false;
                                break;
                            }
                        case TabOrder.TAB_ROAD_ACCESS:
                            {
                                
                                m_chkShowIssuesWithVehicles.isVisible = false;
                                m_btnResetPathing.isVisible = false;
                                m_btnResetRoadAccess.isVisible = true;
                                break;
                            }
                        case TabOrder.TAB_GOODS_OUT:
                            {
                                m_chkShowIssuesWithVehicles.isVisible = false;
                                m_btnResetPathing.isVisible = false;
                                m_btnResetRoadAccess.isVisible = false;
                                break;
                            }
                        default:
                            {
                                m_chkShowIssuesWithVehicles.isVisible = true;
                                m_btnResetPathing.isVisible = false;
                                m_btnResetRoadAccess.isVisible = false;
                                break;
                            }
                    }
                }

                // Update tabs to show count
                List<PathingContainer> listPathing = GetPathingIssues();
                List<PathingContainer> listOutside = GetOutsideIssues();
                m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + listPathing.Count + ")");
                m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + listOutside.Count + ")");

                // Road access only used when path distance is enabled.
                bool bRoadAccessVisible = SaveGameSettings.GetSettings().PathDistanceServices != (int)SaveGameSettings.PathDistanceAlgorithm.LineOfSight || 
                                          SaveGameSettings.GetSettings().PathDistanceGoods != (int)SaveGameSettings.PathDistanceAlgorithm.LineOfSight;

                m_tabStrip.SetTabVisible((int)TabOrder.TAB_ROAD_ACCESS, bRoadAccessVisible);
                if (bRoadAccessVisible)
                {
                    m_tabStrip.SetTabText((int)TabOrder.TAB_ROAD_ACCESS, RoadAccessData.GetRoadAccessIssues().Count.ToString());
                }

                if (m_issueHelper is not null)
                {
                    m_issueHelper.UpdateIssues();
                    m_tabStrip.SetTabText((int)TabOrder.TAB_DEAD, m_issueHelper.GetIssues(TransferIssueHelper.IssueType.Dead).Count.ToString());
                    m_tabStrip.SetTabText((int)TabOrder.TAB_SICK, m_issueHelper.GetIssues(TransferIssueHelper.IssueType.Sick).Count.ToString());
                    m_tabStrip.SetTabText((int)TabOrder.TAB_GOODS_IN, m_issueHelper.GetIssues(TransferIssueHelper.IssueType.GoodsIn).Count.ToString());
                    m_tabStrip.SetTabText((int)TabOrder.TAB_GOODS_OUT, m_issueHelper.GetIssues(TransferIssueHelper.IssueType.GoodsOut).Count.ToString());
                }

                switch ((TabOrder) m_tabStrip.GetSelectTabIndex())
                {
                    case TabOrder.TAB_PATHING:
                        {
                            if (m_listPathing is not null)
                            {
                                listPathing.Sort();
                                m_listPathing.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = listPathing.ToArray(),
                                    m_size = listPathing.Count,
                                };

                                
                            }
                        }
                        break;
                    case TabOrder.TAB_OUTSIDE:
                        {
                            if (m_listOutside is not null)
                            {
                                listOutside.Sort();
                                m_listOutside.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = listOutside.ToArray(),
                                    m_size = listOutside.Count,
                                };
                            }
                        }
                        break;
                    case TabOrder.TAB_ROAD_ACCESS:
                        {
                            if (m_listRoadAccess is not null)
                            {
                                List<RoadAccessData> list = RoadAccessData.GetRoadAccessIssues();
                                list.Sort();
                                m_listRoadAccess.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };

                                
                            }
                        }
                        break;
                        
                    case TabOrder.TAB_DEAD:
                        {
                            if (m_issueHelper is not null)
                            {
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferIssueHelper.IssueType.Dead);
                                list.Sort();
                                m_listDead.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };

                                
                            }
                        }
                        break;
                    case TabOrder.TAB_SICK:
                        {
                            if (m_issueHelper is not null)
                            {
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferIssueHelper.IssueType.Sick);
                                list.Sort();
                                m_listSick.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };
                            }
                        }
                        break;
                    case TabOrder.TAB_GOODS_IN:
                        {
                            if (m_issueHelper is not null)
                            {
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferIssueHelper.IssueType.GoodsIn);
                                list.Sort();
                                m_listGoodsIn.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };
                            }
                        }
                        break;
                    case TabOrder.TAB_GOODS_OUT:
                        {
                            if (m_issueHelper is not null)
                            {
                                List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferIssueHelper.IssueType.GoodsOut);
                                list.Sort();
                                m_listGoodsOut.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = list.ToArray(),
                                    m_size = list.Count,
                                };
                            }
                        }
                        break;
                }
            }
        }

        public override void OnDestroy()
        {
            if (m_coroutine is not null)
            {
                StopCoroutine(m_coroutine);
            }
            if (m_listPathing is not null)
            {
                DestroyGameObject(m_listPathing.gameObject);
                m_listPathing = null;
            }
            if (m_listDead is not null)
            {
                DestroyGameObject(m_listDead.gameObject);
                m_listDead = null;
            }
            if (m_listSick is not null)
            {
                DestroyGameObject(m_listSick.gameObject);
                m_listSick = null;
            }
            if (m_listGoodsIn is not null)
            {
                DestroyGameObject(m_listGoodsIn.gameObject);
                m_listGoodsIn = null;
            }
            if (m_listGoodsOut is not null)
            {
                DestroyGameObject(m_listGoodsOut.gameObject);
                m_listGoodsOut = null;
            }
            if (m_chkShowIssuesWithVehicles is not null)
            {
                DestroyGameObject(m_chkShowIssuesWithVehicles.gameObject);
                m_chkShowIssuesWithVehicles = null;
            }
            if (m_title is not null && m_title.gameObject is not null)
            {
                DestroyGameObject(m_title.gameObject);
                m_title = null;
            }
            if (Instance is not null && Instance.gameObject is not null)
            {
                DestroyGameObject(Instance.gameObject);
                Instance = null;
            }
        }

        private void DestroyGameObject(GameObject go)
        {
            if (go is not null)
            {
                Destroy(go);
            }
        }
    }
}
