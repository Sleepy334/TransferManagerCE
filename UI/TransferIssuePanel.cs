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
        public const int iCOLUMN_WIDTH_VALUE = 40;
        public const int iCOLUMN_WIDTH_TIME = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_MATERIAL_WIDTH = 150;
        public const int iCOLUMN_VEHICLE_WIDTH = 180;
        public const int iCOLUMN_WIDTH_PATHING_BUILDING = 240;
        public const int iCOLUMN_DESCRIPTION_WIDTH = 300;

        private UITitleBar? m_title = null;
        private ListView m_listPathing = null;
        private ListView m_listOutside = null;
        private ListView m_listSick = null;
        private ListView m_listGoods = null;
        private ListView m_listDead = null;
        private UITabStrip? m_tabStrip = null;
        TransferIssueHelper? m_issueHelper = null;
        private UICheckBox? m_chkShowIssuesWithVehicles = null;

        public TransferIssuePanel() : base()
        {
            m_issueHelper = new TransferIssueHelper();
        }

        public override void Start()
        {
            base.Start();
            name = "TransferIssuePanel";
            width = 600;
            height = 500;
            padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventPositionChanged += (sender, e) =>
            {
                ModSettings settings = ModSettings.GetSettings();
                settings.TransferIssueLocationSaved = true;
                settings.TransferIssueLocation = absolutePosition;
                settings.Save();
            };

            if (ModSettings.GetSettings().TransferIssueLocationSaved)
            {
                absolutePosition = ModSettings.GetSettings().TransferIssueLocation;
                FitToScreen();
            } 
            else
            {
                CenterToParent();
            }

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = Localization.Get("titleTransferIssuesPanel");

            m_tabStrip = UITabStrip.Create(this, width - 20f, height - m_title.height - 10 - 20 , OnTabChanged);

            // Pathing
            UIPanel? tabPathing = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesPathing"));
            if (tabPathing != null)
            {
                tabPathing.autoLayout = true;
                tabPathing.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listPathing = ListView.Create(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height - 10);
                if (m_listPathing != null)
                {
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", iCOLUMN_WIDTH_TIME, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", iCOLUMN_WIDTH_PATHING_BUILDING, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", iCOLUMN_WIDTH_PATHING_BUILDING, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
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
                m_listOutside = ListView.Create(tabOutside, "ScrollbarTrack", 0.8f, tabOutside.width, tabOutside.height - 10);
                if (m_listOutside != null)
                {
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", iCOLUMN_WIDTH_TIME, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", iCOLUMN_WIDTH_PATHING_BUILDING, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOutside.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", iCOLUMN_WIDTH_PATHING_BUILDING, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                }
            }

            // Dead
            UIPanel? tabDead = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesDead"));
            if (tabDead != null)
            {
                tabDead.autoLayout = true;
                tabDead.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listDead = ListView.Create(tabDead, "ScrollbarTrack", 0.7f, tabDead.width, tabDead.height - 10);
                if (m_listDead != null)
                {
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listDeadColumn1"), "No. of Dead", iCOLUMN_WIDTH_VALUE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "Death timer", iCOLUMN_WIDTH_VALUE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listDead.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listDead.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Sick
            UIPanel? tabSick = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesSick"));
            if (tabSick != null)
            {
                tabSick.autoLayout = true;
                tabSick.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listSick = ListView.Create(tabSick, "ScrollbarTrack", 0.7f, tabSick.width, tabSick.height - 10);
                if (m_listSick != null)
                {
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listSickColumn1"), "No. of Sick", iCOLUMN_WIDTH_VALUE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "Sick timer", iCOLUMN_WIDTH_VALUE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listSick.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listSick.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Sick
            UIPanel? tabGoods = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesGoods"));
            if (tabGoods != null)
            {
                tabGoods.autoLayout = true;
                tabGoods.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listGoods = ListView.Create(tabGoods, "ScrollbarTrack", 0.7f, tabGoods.width, tabGoods.height - 10);
                if (m_listGoods != null)
                {
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("tabTransferIssuesGoods"), "", iCOLUMN_WIDTH_VALUE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listDeadColumn2"), "", iCOLUMN_WIDTH_VALUE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listGoods.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listGoods.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            UIHelper helper = new UIHelper(this);
            m_chkShowIssuesWithVehicles = (UICheckBox)helper.AddCheckbox(Localization.Get("optionShowIssuesWithVehiclesOnRoute"), ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute, OnShowIssuesClicked);

            isVisible = true;
            UpdatePanel();
        }

        private void OnShowIssuesClicked(bool bEnabled)
        {
            ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute = bEnabled;
            ModSettings.GetSettings().Save();
        }

        private void FitToScreen()
        {
            Vector2 oScreenVector = UIView.GetAView().GetScreenResolution();
            float fX = Math.Max(0.0f, Math.Min(absolutePosition.x, oScreenVector.x - width));
            float fY = Math.Max(0.0f, Math.Min(absolutePosition.y, oScreenVector.y - height));
            Vector3 oFitPosition = new Vector3(fX, fY, absolutePosition.z);
            absolutePosition = oFitPosition;
        }

        new public void Show()
        {
            base.Show();
            UpdatePanel();
        }

        new public void Hide()
        {
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
            base.Hide();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransferManagerCEThreading.HideTransferIssuePanel();
        }

        public List<PathingContainer> GetPathingIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            Dictionary<Util.PATHFINDPAIR, long> failures = Util.PathFindFailure.GetPathFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in failures)
            {
                list.Add(new PathingContainer(kvp.Value, kvp.Key.sourceBuilding, kvp.Key.targetBuilding));
            }

            return list;
        }

        public List<PathingContainer> GetOutsideIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            Dictionary<Util.PATHFINDPAIR, long> outsideFailures = Util.PathFindFailure.GetOutsideFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in outsideFailures)
            {
                list.Add(new PathingContainer(kvp.Value, kvp.Key.sourceBuilding, kvp.Key.targetBuilding));
            }

            return list;
        }

        public void OnTabChanged(int index)
        {
            UpdatePanel();
        }

        public void UpdatePanel()
        {
            if (m_tabStrip != null && m_tabStrip.Count >= 3 && m_issueHelper != null) {
                m_issueHelper.UpdateIssues();

                switch ((TabOrder) m_tabStrip.GetSelectTabIndex())
                {
                    case TabOrder.TAB_PATHING:
                        {
                            if (m_listPathing != null)
                            {
                                List<PathingContainer> list = GetPathingIssues();
                                list.Sort();
                                m_listPathing.SetItems(list.Take(100).ToArray());

                                // Update tab to show count
                                m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + list.Count + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_DEAD, Localization.Get("tabTransferIssuesDead") + " (" + m_issueHelper.GetIssues(TransferReason.Dead).Count() + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_SICK, Localization.Get("tabTransferIssuesSick") + " (" + m_issueHelper.GetIssues(TransferReason.Sick).Count() + ")");
                            }
                        }
                        break;
                    case TabOrder.TAB_OUTSIDE:
                        {
                            if (m_listOutside != null)
                            {
                                List<PathingContainer> list = GetOutsideIssues();
                                list.Sort();
                                m_listOutside.SetItems(list.Take(100).ToArray());

                                // Update tab to show count
                                m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + list.Count + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_DEAD, Localization.Get("tabTransferIssuesDead") + " (" + m_issueHelper.GetIssues(TransferReason.Dead).Count() + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_SICK, Localization.Get("tabTransferIssuesSick") + " (" + m_issueHelper.GetIssues(TransferReason.Sick).Count() + ")");
                                m_tabStrip.SetTabText((int)TabOrder.TAB_GOODS, Localization.Get("tabTransferIssuesGoods") + " (" + m_issueHelper.GetIssues(TransferReason.Goods).Count() + ")");
                            }
                        }
                        break;
                    case TabOrder.TAB_DEAD:
                        {
                            List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferReason.Dead);
                            list.Sort();
                            m_listDead.SetItems(list.Take(100).ToArray());

                            // Update tab to show count
                            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_DEAD, Localization.Get("tabTransferIssuesDead") + " (" + m_issueHelper.GetIssues(TransferReason.Dead).Count() + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_SICK, Localization.Get("tabTransferIssuesSick") + " (" + m_issueHelper.GetIssues(TransferReason.Sick).Count() + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_GOODS, Localization.Get("tabTransferIssuesGoods") + " (" + m_issueHelper.GetIssues(TransferReason.Goods).Count() + ")");
                        }
                        break;
                    case TabOrder.TAB_SICK:
                        {
                            List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferReason.Sick);
                            list.Sort();
                            m_listSick.SetItems(list.Take(100).ToArray());

                            // Update tab to show count
                            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_DEAD, Localization.Get("tabTransferIssuesDead") + " (" + m_issueHelper.GetIssues(TransferReason.Dead).Count() + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_SICK, Localization.Get("tabTransferIssuesSick") + " (" + m_issueHelper.GetIssues(TransferReason.Sick).Count() + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_GOODS, Localization.Get("tabTransferIssuesGoods") + " (" + m_issueHelper.GetIssues(TransferReason.Goods).Count() + ")");
                        }
                        break;
                    case TabOrder.TAB_GOODS:
                        {
                            List<TransferIssueContainer> list = m_issueHelper.GetIssues(TransferReason.Goods);
                            list.Sort();
                            m_listGoods.SetItems(list.Take(100).ToArray());

                            // Update tab to show count
                            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + " (" + GetPathingIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_OUTSIDE, Localization.Get("tabTransferIssuesOutside") + " (" + GetOutsideIssues().Count + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_DEAD, Localization.Get("tabTransferIssuesDead") + " (" + m_issueHelper.GetIssues(TransferReason.Dead).Count() + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_SICK, Localization.Get("tabTransferIssuesSick") + " (" + m_issueHelper.GetIssues(TransferReason.Sick).Count() + ")");
                            m_tabStrip.SetTabText((int)TabOrder.TAB_GOODS, Localization.Get("tabTransferIssuesGoods") + " (" + m_issueHelper.GetIssues(TransferReason.Goods).Count() + ")");
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
            if (m_title != null)
            {
                Destroy(m_title.gameObject);
            }
        }
    }
}
