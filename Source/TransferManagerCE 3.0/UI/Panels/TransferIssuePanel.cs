using AlgernonCommons.UI;
using ColossalFramework.UI;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using System;
using System.Linq;
using static TransferManagerCE.UITabStrip;
using static TransferManagerCE.PathingContainer;
using static TransferIssueContainer;
using SleepyCommon;
using static RenderManager;

namespace TransferManagerCE.UI
{
    public class TransferIssuePanel : UIMainPanel<TransferIssuePanel>
    {
        public enum TabOrder
        {
            TAB_ISSUES,
            TAB_PATHING,
            TAB_ROAD_ACCESS,
        }

        const int iMARGIN = 8;

        // Issue list
        public const int iCOLUMN_WIDTH_ISSUE = 65;
        public const int iCOLUMN_WIDTH_PRIORITY = 65;
        public const int iCOLUMN_WIDTH_TIMER = 60;
        public const int iCOLUMN_WIDTH_VALUE = 70;
        public const int iCOLUMN_WIDTH_VEHICLE = 190;

        // Pathing
        public const int iCOLUMN_WIDTH_LOCATION = 95;
        public const int iCOLUMN_WIDTH_TIME = 70;
        public const int iCOLUMN_WIDTH_PATH_FAIL = 120;
        public const int iCOLUMN_WIDTH_PATHING_BUILDING = 240;
        public const int iCOLUMN_WIDTH_DESCRIPTION = 400;

        private UITitleBar? m_title = null;
        private ListView? m_listPathing = null;
        private ListView? m_listRoadAccess = null;
        private ListView? m_listIssues = null;

        private UITabStrip? m_tabStrip = null;
        
        private UICheckBox? m_chkShowIssuesWithVehicles = null;
        private UIButton? m_btnResetPathingLocal = null;
        private UIButton? m_btnResetRoadAccess = null;
        
        private Coroutine? m_coroutineUpdateHelper = null;
        private TransferIssueHelper m_issueHelper = new TransferIssueHelper();

        public TransferIssuePanel() : base()
        {
            PanelUpdateRate = 5000; // 5 seconds
            m_coroutineUpdateHelper = StartCoroutine(UpdateHelperCoroutine(1));
            IssueRenderer.RegisterRenderer();
        }

        public TransferIssueHelper GetIssueHelper()
        {
            return m_issueHelper;
        }

        public override void Start()
        {
            base.Start();

            name = "TransferIssuePanel";
            width = 920;
            height = 670;
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
            eventVisibilityChanged += OnVisibilityChanged;

            CenterTo(parent);

            // Title Bar
            m_title = UITitleBar.Create(this, Localization.Get("titleTransferIssuesPanel"), "Transfer", TransferManagerMod.Instance.LoadResources(), OnCloseClick);
            if (m_title != null)
            {
                m_title.AddButton("btnHighlight", atlas, "InfoIconLevel", "Highlight Matches", OnHighlightIssuesClick);
                m_title.SetupButtons();
            }

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height - m_title.height;
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.autoLayoutPadding = new RectOffset(iMARGIN, iMARGIN, 4, 4);

            m_tabStrip = UITabStrip.Create(TabStyle.Generic, mainPanel, mainPanel.width - 2 * iMARGIN, mainPanel.height - 12, OnTabChanged);
            //m_tabStrip.backgroundSprite = "InfoviewPanel";
            //m_tabStrip.color = Color.blue;

            CreateIssueTab();
            CreatePathingTab();
            CreateNoRoadAccessTab();
            
            m_tabStrip.SelectTabIndex(0);
            isVisible = true;
            UpdateHighlightButtonIcon();
            UpdatePanel();
        }

        private void CreateIssueTab()
        {
            UIPanel? tabIssues = m_tabStrip.AddTabIcon("IconWarning", Localization.Get("tabTransferIssues"), "", 200f);
            if (tabIssues is not null)
            {
                tabIssues.autoLayout = true;
                tabIssues.autoLayoutDirection = LayoutDirection.Vertical;
                tabIssues.autoLayoutPadding = new RectOffset(0, 0, 6, 0);
                //tabIssues.backgroundSprite = "InfoviewPanel";
                //tabIssues.color = Color.red;

                // Issue list
                m_listIssues = ListView.Create<UIIssueRow>(tabIssues, "ScrollbarTrack", 0.7f, tabIssues.width, tabIssues.height);
                if (m_listIssues is not null)
                {
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("txtIssue"), "", iCOLUMN_WIDTH_ISSUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listBuildingPanelOffersColumn3"), "", iCOLUMN_WIDTH_PRIORITY, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listBuildingPanelStatusColumn5"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listDeadColumn3"), "Source for issue", iCOLUMN_WIDTH_VEHICLE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listDeadColumn5"), "Vehicle on route", iCOLUMN_WIDTH_VEHICLE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listIssues.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listDeadColumn4"), "Target for issue", iCOLUMN_WIDTH_VEHICLE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listIssues.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }

                UIPanel pnlButtons = tabIssues.AddUIComponent<UIPanel>();
                pnlButtons.width = tabIssues.width;
                pnlButtons.height = 30;
                pnlButtons.autoLayout = true;
                pnlButtons.autoLayoutDirection = LayoutDirection.Horizontal;
                pnlButtons.autoLayoutPadding = new RectOffset(6, 0, 6, 0);

                ModSettings settings = ModSettings.GetSettings();

                // Add issue filter buttons

                // Dead
                UIToggleButton btnDead = UIMyUtils.AddSpriteToggleButton(settings.ShowDeadIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "Dead", Localization.Get("tabTransferIssuesDead"), TransferManagerMod.Instance.LoadResources(), 30, 30, null);
                btnDead.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowDeadIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnDead.eventTooltipEnter += (s, e) =>
                {
                    btnDead.tooltip = $"Dead Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Dead)}";
                };

                // Sick
                UIToggleButton btnSick = UIMyUtils.AddSpriteToggleButton(settings.ShowSickIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "ToolbarIconHealthcare", Localization.Get("tabTransferIssuesSick"), atlas, 30, 30, null);
                btnSick.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowSickIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnSick.eventTooltipEnter += (s, e) =>
                {
                    btnSick.tooltip = $"Sick Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Sick)}";
                };

                // Garbage
                UIToggleButton btnGarbage = UIMyUtils.AddSpriteToggleButton(settings.ShowGarbageIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "InfoIconGarbage", Localization.Get("issueGarbage"), atlas, 30, 30, null);
                btnGarbage.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowGarbageIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnGarbage.eventTooltipEnter += (s, e) =>
                {
                    btnGarbage.tooltip = $"Garbage Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Garbage)}";
                };

                // Fire
                UIToggleButton btnFire = UIMyUtils.AddSpriteToggleButton(settings.ShowFireIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "ToolbarIconFireDepartment", Localization.Get("reasonFire"), atlas, 30, 30, null);
                btnFire.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowFireIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnFire.eventTooltipEnter += (s, e) =>
                {
                    btnFire.tooltip = $"Fire Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Fire)}";
                };

                // Crime
                UIToggleButton btnCrime = UIMyUtils.AddSpriteToggleButton(settings.ShowCrimeIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "ToolbarIconPolice", Localization.Get("reasonCrime"), atlas, 30, 30, null);
                btnCrime.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowCrimeIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnCrime.eventTooltipEnter += (s, e) =>
                {
                    btnCrime.tooltip = $"Crime Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Crime)}";
                };

                // Mail
                UIToggleButton btnMail = UIMyUtils.AddSpriteToggleButton(settings.ShowMailIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "InfoIconPost", Localization.Get("reasonMail"), atlas, 30, 30, null);
                btnMail.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowMailIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnMail.eventTooltipEnter += (s, e) =>
                {
                    btnMail.tooltip = $"Mail Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Mail)}";
                };

                // Incoming
                UIToggleButton btnIncoming = UIMyUtils.AddSpriteToggleButton(settings.ShowIncomingIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "IconPolicyIndustrySpace", Localization.Get("issueIncoming"), atlas, 30, 30, null);
                btnIncoming.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowIncomingIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnIncoming.eventTooltipEnter += (s, e) =>
                {
                    btnIncoming.tooltip = $"Incoming Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Incoming)}";
                };

                // Outgoing
                UIToggleButton btnOutgoing = UIMyUtils.AddSpriteToggleButton(settings.ShowOutgoingIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "InfoIconOutsideConnections", Localization.Get("issueOutgoing"), atlas, 30, 30, null);
                btnOutgoing.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowOutgoingIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnOutgoing.eventTooltipEnter += (s, e) =>
                {
                    btnOutgoing.tooltip = $"Outgoing Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Outgoing)}";
                };

                // No service
                UIToggleButton btnNoService = UIMyUtils.AddSpriteToggleButton(settings.ShowServiceIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "InfoIconMaintenance", Localization.Get("issueNoServices"), atlas, 30, 30, null);
                btnNoService.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowServiceIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnNoService.eventTooltipEnter += (s, e) =>
                {
                    btnNoService.tooltip = $"Service Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Services)}";
                };

                // Workers
                UIToggleButton btnWorkers = UIMyUtils.AddSpriteToggleButton(settings.ShowWorkerIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "IconPolicyWorkersUnion", Localization.Get("issueNoWorkers"), atlas, 30, 30, null);
                btnWorkers.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowWorkerIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };
                btnWorkers.eventTooltipEnter += (s, e) =>
                {
                    btnWorkers.tooltip = $"Worker Issues: {m_issueHelper.GetAllIssues().Count(x => x.m_issue == IssueType.Worker)}";
                };

                // Separate buttons from checkbox
                UIPanel pnlButtonSeparator = pnlButtons.AddUIComponent<UIPanel>();
                pnlButtonSeparator.width = 230;
                pnlButtonSeparator.height = pnlButtons.height;

                m_chkShowIssuesWithVehicles = UIMyUtils.AddCheckbox(pnlButtons, Localization.Get("optionShowIssuesWithVehiclesOnRoute"), UIFonts.Regular, 0.9f, ModSettings.GetSettings().ShowWithVehiclesOnRouteIssues, OnShowIssuesClicked);
                m_chkShowIssuesWithVehicles.autoSize = false;
                m_chkShowIssuesWithVehicles.width = 200;
                m_chkShowIssuesWithVehicles.height = pnlButtons.height;
                m_chkShowIssuesWithVehicles.PerformLayout();

                // Set height now they have all been created.    
                m_listIssues.height = tabIssues.height - pnlButtons.height - 12;
            }
        }

        private void CreatePathingTab()
        {
            // Pathing Local
            UIPanel? tabPathing = m_tabStrip.AddTabIcon("RoadOptionFreeform", Localization.Get("tabTransferIssuesPathing"), "", 160f);
            if (tabPathing is not null)
            {
                tabPathing.autoLayout = true;
                tabPathing.autoLayoutDirection = LayoutDirection.Vertical;
                tabPathing.autoLayoutPadding = new RectOffset(0, 0, 6, 0);

                // Issue list
                m_listPathing = ListView.Create<UIPathRow>(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height);
                if (m_listPathing is not null)
                {
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "", iCOLUMN_WIDTH_TIME, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("columnLocation"), "", iCOLUMN_WIDTH_LOCATION, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }

                UIPanel pnlButtons = tabPathing.AddUIComponent<UIPanel>();
                pnlButtons.width = tabPathing.width;
                pnlButtons.height = 30;
                pnlButtons.autoLayout = true;
                pnlButtons.autoLayoutDirection = LayoutDirection.Horizontal;
                pnlButtons.autoLayoutPadding = new RectOffset(6, 0, 6, 0);

                // Local
                UIToggleButton btnLocal = UIMyUtils.AddSpriteToggleButton(ModSettings.GetSettings().ShowLocalPathingIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "RoadOptionFreeform", Localization.Get("pathingLocal"), atlas, 30, 30, null);
                btnLocal.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowLocalPathingIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };

                // Outside
                UIToggleButton btnOutside = UIMyUtils.AddSpriteToggleButton(ModSettings.GetSettings().ShowOutsidePathingIssues, UIMyUtils.ButtonStyle.DropDown, pnlButtons, "InfoIconOutsideConnections", Localization.Get("tabTransferIssuesOutside"), atlas, 30, 30, null);
                btnOutside.eventClick += (s, e) =>
                {
                    UIToggleButton toggleButton = (UIToggleButton)s;
                    ModSettings.GetSettings().ShowOutsidePathingIssues = toggleButton.StateOn;
                    ModSettings.GetSettings().Save();
                    UpdatePanel();
                };

                UIPanel pnlSeparator = pnlButtons.AddUIComponent<UIPanel>();
                pnlSeparator.width = 200; // set below
                pnlSeparator.height = pnlButtons.height;

                // Reset pathing button
                m_btnResetPathingLocal = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, pnlButtons, Localization.Get("btnResetPathingStatistics"), "", 200, 30, OnReset);
                pnlSeparator.width = pnlButtons.width - btnLocal.width - btnOutside.width - m_btnResetPathingLocal.width - 24;

                // Adjust list height
                m_listPathing.height = tabPathing.height - pnlButtons.height - 12;
            }
        }

        private void CreateNoRoadAccessTab()
        {
            // Road Access
            UIPanel? tabRoadAccess = m_tabStrip.AddTabIcon("ToolbarIconRoads", Localization.Get("tabTransferRoadAccess"), "", 220f);
            if (tabRoadAccess is not null)
            {
                tabRoadAccess.autoLayout = true;
                tabRoadAccess.autoLayoutDirection = LayoutDirection.Vertical;
                tabRoadAccess.autoLayoutPadding = new RectOffset(0, 0, 6, 0);

                // Issue list
                m_listRoadAccess = ListView.Create<UIRoadAccessRow>(tabRoadAccess, "ScrollbarTrack", 0.7f, tabRoadAccess.width, tabRoadAccess.height);
                if (m_listRoadAccess is not null)
                {
                    m_listRoadAccess.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("columnId"), "", iCOLUMN_WIDTH_VALUE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listRoadAccess.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, Localization.Get("columnPathFailureCount"), "", iCOLUMN_WIDTH_PATH_FAIL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listRoadAccess.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Building with road access issues", iCOLUMN_WIDTH_DESCRIPTION, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                }

                m_btnResetRoadAccess = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, tabRoadAccess, Localization.Get("btnResetRoadAccess"), "", 200, 30, OnResetRoadAccess);
                m_listRoadAccess.height = tabRoadAccess.height - m_btnResetRoadAccess.height - 12;
            }
        }

        public void OnHighlightIssuesClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            int iHighlightMode = (int)ModSettings.GetSettings().HighlightIssuesState;
            ModSettings.GetSettings().HighlightIssuesState = ((iHighlightMode + 1) % 2);
            ModSettings.GetSettings().Save();

            UpdateHighlightButtonIcon();
        }

        public void UpdateHighlightButtonIcon()
        {
            if (m_title is not null)
            {
                string sIcon = "";
                string sTooltip = "";

                switch ((ModSettings.IssuesHighlightMode)ModSettings.GetSettings().HighlightIssuesState)
                {
                    case ModSettings.IssuesHighlightMode.None:
                        {
                            sIcon = "InfoIconLevelPressed";
                            sTooltip = Localization.Get("tooltipHighlightModeOff");
                            break;
                        }
                    case ModSettings.IssuesHighlightMode.Issues:
                        {
                            sIcon = "InfoIconLevelFocused";
                            sTooltip = Localization.Get("tooltipHighlightModeIssues2");

                            switch ((TabOrder) GetSelectTabIndex())
                            {
                                case TabOrder.TAB_ISSUES:
                                    {
                                        foreach (IssueType issueType in Enum.GetValues(typeof(IssueType)))
                                        {
                                            KnownColor color = TransferIssueContainer.GetColor(issueType);
                                            sTooltip += $"\n{issueType}: {color.name}";
                                        }
                                        break;
                                    }
                                case TabOrder.TAB_PATHING:
                                    {
                                        sTooltip += $"\nPathing: {KnownColor.magenta.name}";
                                        break;
                                    }
                                case TabOrder.TAB_ROAD_ACCESS:
                                    {
                                        sTooltip += $"\nNoRoadAccess: {KnownColor.maroon.name}";
                                        break;
                                    }
                            }

                            break;
                        }
                }

                m_title.Buttons[0].normalBgSprite = sIcon;
                m_title.Buttons[0].tooltip = sTooltip;
            }
        }

        private void OnShowIssuesClicked(bool bEnabled)
        {
            ModSettings.GetSettings().ShowWithVehiclesOnRouteIssues = bEnabled;
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
            RoadAccessStorage.Reset();
            UpdatePanel();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        public List<PathingContainer> GetPathingIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            if (ModSettings.GetSettings().ShowLocalPathingIssues)
            {
                Dictionary<Util.PATHFINDPAIR, long> failures = Util.PathFindFailure.GetPathFailsCopy();
                foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in failures)
                {
                    list.Add(new PathingContainer(LocationType.Local, kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.None, false));
                }
            }

            if (ModSettings.GetSettings().ShowOutsidePathingIssues)
            {
                Dictionary<Util.PATHFINDPAIR, long> outsideFailures = Util.PathFindFailure.GetOutsideFailsCopy();
                foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in outsideFailures)
                {
                    list.Add(new PathingContainer(LocationType.Outside, kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.None, false));
                }
            } 

            list.Sort();

            return list;
        }

        public void OnTabChanged(int index)
        {
            UpdateHighlightButtonIcon();
            UpdatePanel();
        }

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                // Force an update.
                m_issueHelper.UpdateIssues();
                UpdatePanel();
            }
            else
            {
                if (tooltipBox is not null)
                {
                    tooltipBox.tooltip = "";
                    tooltipBox.tooltipBox.Hide();
                }

                if (m_listIssues is not null)
                {
                    m_listIssues.Clear();
                }
                if (m_listPathing is not null)
                {
                    m_listPathing.Clear();
                }
                if (m_listRoadAccess is not null)
                {
                    m_listRoadAccess.Clear();
                }
            }
        }

        IEnumerator UpdateHelperCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);

                if (isVisible)
                {
                    m_issueHelper.UpdateIssues();
                }
            }
        }

        public int GetSelectTabIndex()
        {
            return m_tabStrip.GetSelectTabIndex();
        }

        protected override void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            if (m_tabStrip is null)
            {
                return;
            }

            // get data
            List<PathingContainer> listPathing = GetPathingIssues();
            List<TransferIssueContainer> filteredIssues = m_issueHelper.GetFilteredIssues();
            List<RoadAccessData> roadAccessIssues = m_issueHelper.GetRoadAccess();

            // Update tabs to show count
            m_tabStrip.SetTabText((int)TabOrder.TAB_ISSUES, $"{Localization.Get("tabTransferIssues")} ({filteredIssues.Count} / {m_issueHelper.GetAllIssues().Count})");
            m_tabStrip.SetTabText((int)TabOrder.TAB_PATHING, $"{Localization.Get("tabTransferIssuesPathing")} ({listPathing.Count})");
            m_tabStrip.SetTabText((int)TabOrder.TAB_ROAD_ACCESS, $"{Localization.Get("tabTransferRoadAccess")} ({roadAccessIssues.Count})");

            switch ((TabOrder) m_tabStrip.GetSelectTabIndex())
            {
                case TabOrder.TAB_ISSUES:
                    {
                        if (m_issueHelper is not null)
                        {
                            filteredIssues.Sort();
                            m_listIssues.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = filteredIssues.ToArray(),
                                m_size = filteredIssues.Count,
                            };

                            m_listPathing.Clear();
                            m_listRoadAccess.Clear();
                        }
                        break;
                    }
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

                            m_listIssues.Clear();
                            m_listRoadAccess.Clear();
                        }
                        break;
                    }
                case TabOrder.TAB_ROAD_ACCESS:
                    {
                        if (m_listRoadAccess is not null)
                        {
                            roadAccessIssues.Sort();
                            m_listRoadAccess.GetList().rowsData = new FastList<object>
                            {
                                m_buffer = roadAccessIssues.ToArray(),
                                m_size = roadAccessIssues.Count,
                            };

                            m_listIssues.Clear();
                            m_listPathing.Clear();
                        }
                        break;
                    }
            }
        }

        public override void OnDestroy()
        {
            if (m_coroutineUpdateHelper is not null)
            {
                StopCoroutine(m_coroutineUpdateHelper);
            }
            if (m_listPathing is not null)
            {
                DestroyGameObject(m_listPathing.gameObject);
                m_listPathing = null;
            }
            if (m_listIssues is not null)
            {
                DestroyGameObject(m_listIssues.gameObject);
                m_listIssues = null;
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
