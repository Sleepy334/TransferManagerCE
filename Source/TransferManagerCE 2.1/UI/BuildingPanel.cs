using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class BuildingPanel : UIPanel
    {
        public enum TabIndex
        {
            TAB_SETTINGS,
            TAB_CAPACITY,
            TAB_STATUS,
            TAB_VEHICLES,
            TAB_PATHING,
            TAB_TRANSFERS,
        }
        
        public static BuildingPanel? Instance = null;

        public const float fTEXT_SCALE = 0.8f;
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;

        public const int iLISTVIEW_MATCHES_HEIGHT = 294;
        public const int iLISTVIEW_OFFERS_HEIGHT = 196;

        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_TINY = 40;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 120;
        public const int iCOLUMN_WIDTH_LARGER = 150;
        public const int iCOLUMN_WIDTH_XLARGE = 200;
        public const int iCOLUMN_WIDTH_PATHING_BUILDING = 240;
        public const int iCOLUMN_WIDTH_250 = 250;
        public const int iCOLUMN_WIDTH_300 = 300;

        // Currently viewed building id.
        private ushort m_buildingId = 0;

        private BuildingType m_eBuildingType;

        private UITitleBar? m_title = null;
        private UITabStrip? m_tabStrip = null;
        public BuildingSettingsTab? m_settingsTab = null;
        public OutsideCapacityTab? m_capacityTab = null;

        // Status tab
        private ListView? m_listStatus = null;
        private ListView? m_listVehicles = null;

        // Pathing tab
        private ListView? m_listPathing = null;
        private UIButton? m_btnReset = null;

        // Transfers tab
        private UILabel? m_lblSource = null;
        private UITextField? m_txtSource = null;
        private UILabel? m_lblOffers = null;
        private UILabel? m_lblMatches = null;
        private ListView? m_listOffers = null;
        private ListView? m_listMatches = null;
        private BuildingMatches m_matches;
        private BuildingOffers m_buildingOffers;

        public BuildingPanel() : 
            base()
        {
            m_settingsTab = new BuildingSettingsTab();
            m_capacityTab = new OutsideCapacityTab();
            m_buildingOffers = new BuildingOffers();
            m_matches = new BuildingMatches();
        }

        public ushort GetBuildingId()
        {
            return m_buildingId;
        }

        public int GetRestrictionId()
        {
            if (m_settingsTab != null)
            {
                return m_settingsTab.GetRestrictionId();
            }
            return -1;
        }

        public static bool IsVisible()
        {
            return Instance != null && Instance.isVisible;
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(BuildingPanel)) as BuildingPanel;
                if (Instance == null)
                {
                    Prompt.Info(TransferManagerMain.Title, "Error creating Transfer Building Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "BuildingPanel";
            width = 740;
            height = 652;
            backgroundSprite = "UnlockingPanel2";
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            clipChildren = true;
            CenterToParent();
            
            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.SetStatsHandler(OnStatsClick);
            m_title.SetIssuesHandler(OnIssuesClick);
            m_title.SetOutsideHandler(OnOutsideClick);
            m_title.SetHighlightHandler(OnHighlightClick);
            m_title.title = TransferManagerMain.Title;

            UpdateHighlightButtonIcon();

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height;
            mainPanel.padding = new RectOffset(iMARGIN, iMARGIN, iMARGIN, iMARGIN);
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 4);

            // Object label
            m_lblSource = mainPanel.AddUIComponent<UILabel>();
            if (m_lblSource != null)
            {
                m_lblSource.height = 30;
                m_lblSource.width = mainPanel.width - 10;
                m_lblSource.padding = new RectOffset(4, 4, 4, 4);
                m_lblSource.autoSize = false;
                m_lblSource.text = "Select Building";
                //m_lblSource.textScale = fTEXT_SCALE;
                m_lblSource.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblSource.textAlignment = UIHorizontalAlignment.Center;
                m_lblSource.eventMouseEnter += (c, e) =>
                {
                    m_lblSource.textColor = new Color32(13, 183, 255, 255);
                };
                m_lblSource.eventMouseLeave += (c, e) =>
                {
                    m_lblSource.textColor = Color.white;
                };
                m_lblSource.eventClick += (c, e) =>
                {
                    if (m_buildingId != 0)
                    {
                        CitiesUtils.ShowBuilding(m_buildingId);
                    }
                };
            }

            // Object label
            m_txtSource = UIUtils.CreateTextField(mainPanel, "txtSource", 1.0f, mainPanel.width - 20, 30);
            if (m_txtSource != null)
            {
                m_txtSource.eventTextSubmitted += OnBuildingNameChanged;
                m_txtSource.eventLostFocus += OnBuildingNameLostFocus;
            }

            UpdateBuildingName();

            // Tabs
            m_tabStrip = UITabStrip.Create(mainPanel, width - 20f, height - m_txtSource.height - m_title.height - 10, OnTabChanged);

            // Settings tab
            if (m_settingsTab != null)
            {
                m_settingsTab.SetTabBuilding(m_buildingId); 
                m_settingsTab.Setup(m_tabStrip);
            }

            // Capacity tab
            if (m_capacityTab != null)
            {
                m_capacityTab.SetTabBuilding(m_buildingId);
                m_capacityTab.Setup(m_tabStrip);
            }

            // Status tab
            UIPanel? tabStatus = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelStatus"));
            if (tabStatus != null)
            {
                tabStatus.autoLayout = true;
                tabStatus.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listStatus = ListView.Create<UIStatusRow>(tabStatus, "ScrollbarTrack", 0.8f, tabStatus.width, tabStatus.height - 10);
                if (m_listStatus != null)
                {
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "S = Sick\r\nD = Dead\r\nI = Incoming\r\nW = Waiting\r\nB = Blocked", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_LARGER, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_LOAD, Localization.Get("txtLoad"), "Load", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", iCOLUMN_WIDTH_LARGER, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                }
            }
            
            UIPanel? tabVehicles = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelVehicles"), 150f);
            if (tabVehicles != null)
            {
                tabVehicles.autoLayout = true;
                tabVehicles.autoLayoutDirection = LayoutDirection.Vertical;

                // Vehicles list
                m_listVehicles = ListView.Create<UIVehicleRow>(tabVehicles, "ScrollbarTrack", 0.8f, tabVehicles.width, tabVehicles.height - 10);
                if (m_listVehicles != null)
                {
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("txtLoad"), "Vehicle Load", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "W = Waiting\r\nB = Blocked", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_XLARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE_TARGET, Localization.Get("listBuildingPanelVehicleTarget"), "Target", iCOLUMN_WIDTH_250, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }
            }
            
            UIPanel? tabPathing = m_tabStrip.AddTab(Localization.Get("tabTransferIssuesPathing"));
            if (tabPathing != null)
            {
                const int iButtonHeight = 30;
                // Issue list
                m_listPathing = ListView.Create<UIPathRow>(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height - iButtonHeight - 10);
                if (m_listPathing != null)
                {
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Failure count", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Failure count", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }

                m_btnReset = UIUtils.AddButton(tabPathing, Localization.Get("btnResetPathingStatistics"), 200, iButtonHeight, OnReset);
                if (m_btnReset != null)
                {
                    m_btnReset.tooltip = "Reset pathing for this building.";
                }
            }
            
            UIPanel? tabTransfers = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelTransfers"));
            if (tabTransfers != null)
            {
                tabTransfers.autoLayout = true;
                tabTransfers.autoLayoutDirection = LayoutDirection.Vertical;

                // Object label
                m_lblOffers = tabTransfers.AddUIComponent<UILabel>();
                m_lblOffers.width = tabTransfers.width;
                m_lblOffers.height = 20;
                m_lblOffers.padding = new RectOffset(4, 4, 4, 4);
                m_lblOffers.text = Localization.Get("labelBuildingPanelOffers");

                // Offer list
                m_listOffers = ListView.Create<UIOfferRow>(tabTransfers, "ScrollbarTrack", 0.8f, tabTransfers.width, iLISTVIEW_OFFERS_HEIGHT);
                if (m_listOffers != null)
                {
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelOffersColumn2"), "Reason for transfer request", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "Transfer Direction", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listBuildingPanelOffersColumn3"), "Transfer offer priority", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelOffersColumn4"), "Transfer offer Active/Passive", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelOffersColumn5"), "Transfer Offer Amount", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PARK, Localization.Get("listBuildingPanelOffersPark"), "Offer Park #", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelOffersColumn6"), "Offer description", iCOLUMN_WIDTH_250, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listOffers.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }

                m_lblMatches = tabTransfers.AddUIComponent<UILabel>();
                m_lblMatches.width = tabTransfers.width;
                m_lblMatches.height = 20;
                m_lblMatches.padding = new RectOffset(4, 4, 4, 4);
                m_lblMatches.text = Localization.Get("labelBuildingPanelMatchOffers");

                // Offer list
                m_listMatches = ListView.Create<UIMatchRow>(tabTransfers, "ScrollbarTrack", 0.7f, tabTransfers.width, iLISTVIEW_MATCHES_HEIGHT);
                if (m_listMatches != null)
                {
                    //m_listMatches.width = width - 20;
                    //m_listMatches.height = iLISTVIEW_MATCHES_HEIGHT;
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listBuildingPanelMatchesColumn1"), "Time of match", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelMatchesColumn2"), "Reason for transfer request", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelMatchesColumn3"), "Active or Passive for this match", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelMatchesColumn4"), "Transfer match amount", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Transfer distance", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, "P", "In priority / Out priority", iCOLUMN_WIDTH_TINY, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PARK, Localization.Get("listBuildingPanelOffersPark"), "Offer Park #", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelMatchesColumn6"), "Match description", iCOLUMN_WIDTH_250, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            // Hide till needed.
            m_tabStrip.SetTabVisible((int) TabIndex.TAB_PATHING, false);

            // Select first tab
            m_tabStrip.SelectTabIndex(0);
            isVisible = true;
            UpdateTabs();
            UpdatePanel();
        }

        public BuildingMatches GetBuildingMatches()
        {
            return m_matches;
        }

        public void UpdateTabs()
        {
            if (m_tabStrip != null)
            {
                if (m_settingsTab != null)
                {
                    if (SaveGameSettings.GetSettings().EnableNewTransferManager)
                    {
                        m_settingsTab.SetTabBuilding(m_buildingId);
                        m_settingsTab.UpdateSettingsTab();
                    }
                    else
                    {
                        m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);
                    }
                }
                if (m_capacityTab != null)
                {
                    m_capacityTab.SetTabBuilding(m_buildingId);
                    m_capacityTab.UpdateTab();
                }
                UpdateVehicleTab();

                // Show/Hide pathing tab for this building
                if (m_tabStrip != null)
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, GetPathingIssues().Count > 0);
                }
            }
            
        }

        public void UpdateVehicleTab()
        {
            if (m_tabStrip != null)
            {
                if (HasVehicles(m_buildingId))
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, true);
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, false);
                }
            }
        }

        private void SetPanelBuilding(ushort buildingId)
        {
            if (buildingId != m_buildingId)
            {
                m_buildingId = buildingId;
                m_eBuildingType = BuildingTypeHelper.GetBuildingType(buildingId);

                // Select building with tool
                CitiesUtils.ShowBuilding(buildingId);

                // Match logging
                if (MatchLogging.Instance != null)
                {
                    MatchLogging.Instance.SetBuildingId(buildingId);
                }

                GetBuildingMatches().SetBuildingId(buildingId);
                UpdateBuildingName();
                UpdateTabs();

                UpdatePanel();

                if (SelectionTool.Instance != null)
                {
                    SelectionTool.Instance.UpdateSelection();
                }
            }
        }

        public void ShowPanel(ushort buildingId)
        {
            SetPanelBuilding(buildingId);
            ShowPanel();
        }

        public void ShowPanel()
        {
            // Load the relevant building if not set
            if (m_buildingId == 0)
            {
                InstanceID selectedInstance = ToolsModifierControl.cameraController.GetTarget();
                if (selectedInstance != null && selectedInstance.Building != 0)
                {
                    SetPanelBuilding(selectedInstance.Building);
                }
            }

            UpdateBuildingName();
            UpdateTabs();
            UpdatePanel();
            Show(true);
        }

        public void HidePanel()
        {
            Hide();

            m_buildingId = 0;
            m_eBuildingType = BuildingType.None;

            GetBuildingMatches().InvalidateMatches();

            if (m_listStatus != null)
            {
                m_listStatus.Clear();
            }
            if (m_listOffers != null)
            {
                m_listOffers.Clear();
            }
            if (m_listMatches != null)
            {
                m_listMatches.Clear();
            }
            if (SelectionTool.Instance != null)
            {
                SelectionTool.Instance.Disable();
            }
            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.Hide();
            }
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

        private void OnReset(UIComponent component, UIMouseEventParameter eventParam)
        {
            PathFindFailure.ResetPathingStatistics(m_buildingId);
            UpdatePanel();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            HidePanel();
        }

        public void OnStatsClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StatisticsThread.ToggleStatsPanel();
        }

        public void OnIssuesClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransferIssueThread.ShowTransferIssuePanel();
        }
        
        public void OnOutsideClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            OutsideConnectionPanel.Init();
            if (OutsideConnectionPanel.Instance != null)
            {
                OutsideConnectionPanel.Instance.Show();
            }
        }

        public void OnHighlightClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            int iHighlightMode = (int)ModSettings.GetSettings().HighlightMatchesState;
            ModSettings.GetSettings().HighlightMatchesState = ((iHighlightMode + 1) % 3);
            ModSettings.GetSettings().Save();
            UpdateHighlightButtonIcon();
        }

        public void UpdateHighlightButtonIcon()
        {
            if (m_title != null && m_title.m_btnHighlight != null)
            {
                string sIcon = "";
                string sTooltip = "";
                switch ((ModSettings.HighlightMode)ModSettings.GetSettings().HighlightMatchesState)
                {
                    case ModSettings.HighlightMode.None:
                        {
                            sIcon = "InfoIconLevelPressed";
                            sTooltip = Localization.Get("tooltipHighlightModeOff");
                            break;
                        }
                    case ModSettings.HighlightMode.Matches:
                        {
                            sIcon = "InfoIconLevel";
                            sTooltip = Localization.Get("tooltipHighlightModeMatches");
                            break;
                        }
                    case ModSettings.HighlightMode.Issues:
                        {
                            sIcon = "InfoIconLevelFocused";
                            sTooltip = Localization.Get("tooltipHighlightModeIssues"); ;
                            break;
                        }
                }

                m_title.m_btnHighlight.normalBgSprite = sIcon;
                m_title.m_btnHighlight.tooltip = sTooltip;

                // Update selection
                if (SelectionTool.Instance != null)
                {
                    SelectionTool.Instance.UpdateSelection();
                }
            }
        }

        public void OnTabChanged(int index)
        {
            // Close district panel
            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.Hide();
            }

            // Turn off building selection mode
            if (SelectionTool.Instance != null)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
            }

            UpdatePanel();
        }

        public List<VehicleData> GetVehicles(out int iVehicleCount)
        {
            List<VehicleData> list = new List<VehicleData>();
            iVehicleCount = 0;

            if (m_buildingId != 0)
            {
                List<VehicleData> listInternal = new List<VehicleData>();
                List<VehicleData> listExternal = new List<VehicleData>();
                List<VehicleData> listReturning = new List<VehicleData>();

                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    uint uiSize = VehicleManager.instance.m_vehicles.m_size;
                    int iLoopCount = 0;
                    ushort vehicleId = building.m_ownVehicles;
                    while (vehicleId != 0 && vehicleId < uiSize)
                    {
                        Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                        if (vehicle.m_flags != 0)
                        {
                            if (vehicle.m_targetBuilding == 0)
                            {
                                listReturning.Add(new VehicleData(vehicleId));
                            }
                            else
                            {
                                InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleId, vehicle);
                                switch (target.Type)
                                {
                                    case InstanceType.Building:
                                        {
                                            if (IsOutsideConnection(target.Building))
                                            {
                                                listExternal.Add(new VehicleData(vehicleId));
                                            }
                                            else
                                            {
                                                listInternal.Add(new VehicleData(vehicleId));
                                            }
                                            break;
                                        }
                                    case InstanceType.NetNode:
                                        {
                                            NetNode node = NetManager.instance.m_nodes.m_buffer[target.NetNode];
                                            if ((node.m_flags & NetNode.Flags.Outside) != 0)
                                            {
                                                listExternal.Add(new VehicleData(vehicleId));
                                            }
                                            else
                                            {
                                                listInternal.Add(new VehicleData(vehicleId));
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            listInternal.Add(new VehicleData(vehicleId));
                                            break;
                                        }
                                }
                            }
                        }

                        vehicleId = vehicle.m_nextOwnVehicle;

                        // Check for bad list
                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            return new List<VehicleData>();
                        }
                    }
                }

                // Now produce output list
                // Internal first
                listInternal.Sort();
                foreach (VehicleData vehicleData in listInternal)
                {
                    list.Add(vehicleData);
                }

                if (listExternal.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    // External
                    listExternal.Sort();
                    foreach (VehicleData vehicleData in listExternal)
                    {
                        list.Add(vehicleData);
                    }
                }

                if (listReturning.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }
                    
                    // Returning
                    listReturning.Sort();
                    foreach (VehicleData vehicleData in listReturning)
                    {
                        list.Add(vehicleData);
                    }
                }

                iVehicleCount = listInternal.Count + listExternal.Count + listReturning.Count;
            }

            return list;
        }

        public bool IsTransferTabActive()
        {
            if (m_tabStrip != null)
            {
                return (TabIndex)m_tabStrip.GetSelectTabIndex() == TabIndex.TAB_TRANSFERS;
            }
            return false;
        }

        public List<PathingContainer> GetPathingIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            InstanceID instance = new InstanceID {  Building = m_buildingId };
            Dictionary<Util.PATHFINDPAIR, long> failures = Util.PathFindFailure.GetPathFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in failures)
            {
                if (kvp.Key.m_source == instance || kvp.Key.m_target == instance)
                {
                    list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target));
                }             
            }

            Dictionary<Util.PATHFINDPAIR, long> outside = Util.PathFindFailure.GetOutsideFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in outside)
            {
                if (kvp.Key.m_source == instance || kvp.Key.m_target == instance)
                {
                    list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target));
                }
            }

            return list;
        }

        public void UpdateBuildingName()
        {
            if (m_txtSource != null && m_lblSource != null)
            {
                if (m_buildingId != 0)
                {
                    string sText;
                    if (!DependencyUtilities.IsAdvancedOutsideConnectionsRunning() && IsOutsideConnection(m_buildingId))
                    {
                        m_lblSource.isVisible = false;
                        m_txtSource.isVisible = true;

                        OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
                        sText = settings.GetName(m_buildingId);
                    }
                    else
                    {
                        m_lblSource.isVisible = true;
                        m_txtSource.isVisible = false;

                        sText = CitiesUtils.GetBuildingName(m_buildingId);
                        string sDetectedDistricts = CitiesUtils.GetDetectedDistricts(m_buildingId);
                        if (sDetectedDistricts.Length > 0)
                        {
                            sText += " (" + sDetectedDistricts + ")";
                        }
                    }
                    m_lblSource.text = sText;
                    m_txtSource.text = sText;
                }
                else
                {
                    m_lblSource.text = "Select Building";
                    m_txtSource.text = "Select Building";
                }
            }
        }

        public void OnBuildingNameChanged(UIComponent c, string text)
        {
            if (string.IsNullOrEmpty(m_txtSource.text))
            {
                OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
                settings.m_name = "";
                OutsideConnectionSettings.SetSettings(m_buildingId, settings);

                // Update string to default value
                m_txtSource.text = settings.GetName(m_buildingId);
            }
            else
            {
                OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
                settings.m_name = m_txtSource.text;
                OutsideConnectionSettings.SetSettings(m_buildingId, settings);
            }

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }
        }

        public void OnBuildingNameLostFocus(UIComponent c, UIFocusEventParameter e)
        {
            OnBuildingNameChanged(c, m_txtSource.text);
        }

        public void UpdatePanel()
        {
            if (m_tabStrip != null)
            {
                // Update status tab count
                if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_STATUS))
                {
                    string sMessage = Localization.Get("tabBuildingPanelStatus");

                    if (m_buildingId != 0)
                    {
                        // Add vehicle count if there are any guest vehicles
                        int iCount = CitiesUtils.GetGuestParentVehiclesForBuilding(m_buildingId).Count;
                        if (iCount > 0)
                        {
                            sMessage += " (" + iCount + ")";
                        }
                    }

                    m_tabStrip.SetTabText((int)TabIndex.TAB_STATUS, sMessage);
                }

                // Update vehicle tab count
                List<VehicleData> vehicles;
                if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_VEHICLES))
                {
                    vehicles = GetVehicles(out int iVehicleCount);
                    string sMessage = Localization.Get("tabBuildingPanelVehicles");
                    
                    if (m_buildingId != 0)
                    {
                        sMessage += " (" + iVehicleCount;

                        int maxVehicles = BuildingVehicleCount.GetMaxVehicleCount(m_eBuildingType, m_buildingId);
                        if (maxVehicles > 0)
                        {
                            sMessage += $"/{maxVehicles}";
                        }

                        sMessage += ")";
                    }

                    m_tabStrip.SetTabText((int)TabIndex.TAB_VEHICLES, sMessage);
                }
                else
                {
                    vehicles = new List<VehicleData>();
                }

                if (m_buildingId != 0)
                {
                    // Show pathing tab if there are any pathing uissues, do not hide pathing tab once it is shown for this building.
                    List<PathingContainer> list = GetPathingIssues();
                    if (list.Count > 0 && !m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                    {
                        m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, true);
                    }

                    // Update pathing tab count
                    if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                    {
                        m_tabStrip.SetTabText((int)TabIndex.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + "(" + list.Count + ")");
                    }

                    switch ((TabIndex)m_tabStrip.GetSelectTabIndex())
                    {
                        case TabIndex.TAB_SETTINGS: // Settings
                            {
                                break;
                            }
                        case TabIndex.TAB_STATUS: // Status
                            {
                                if (m_listStatus != null)
                                {
                                    List<StatusData> status = new StatusHelper().GetStatusList(m_buildingId);
                                    
                                    // Services
                                    m_listStatus.GetList().rowsData = new FastList<object>
                                    {
                                        m_buffer = status.ToArray(),
                                        m_size = status.Count,
                                    };
                                }
                                break;
                            }
                        case TabIndex.TAB_VEHICLES: // Vehicles
                            {
                                if (m_listVehicles != null)
                                {
                                    m_listVehicles.GetList().rowsData = new FastList<object>
                                    {
                                        m_buffer = vehicles.ToArray(),
                                        m_size = vehicles.Count,
                                    };
                                }
                                break;
                            }
                        case TabIndex.TAB_PATHING:
                            {
                                if (m_listPathing != null)
                                {
                                    list.Sort(PathingContainer.CompareToTime);
                                    m_listPathing.GetList().rowsData = new FastList<object>
                                    {
                                        m_buffer = list.ToArray(),
                                        m_size = list.Count,
                                    };
                                }
                                break;
                            }
                        case TabIndex.TAB_TRANSFERS: // Transfers
                            {
                                if (m_listOffers != null)
                                {
                                    List<OfferData> offers = m_buildingOffers.GetOffersForBuilding(m_buildingId);
                                    offers.Sort();
                                    m_listOffers.GetList().rowsData = new FastList<object>
                                    {
                                        m_buffer = offers.ToArray(),
                                        m_size = offers.Count,
                                    };
                                }
                                if (m_listMatches != null && MatchLogging.Instance != null)
                                {
                                    List<BuildingMatchData>? listMatches = GetBuildingMatches().GetSortedBuildingMatches();
                                    if (listMatches != null && m_listMatches.GetList() != null)
                                    {
                                        m_listMatches.GetList().rowsData = new FastList<object>
                                        {
                                            m_buffer = listMatches.ToArray(),
                                            m_size = listMatches.Count,
                                        };
                                    }
                                }
                                break;
                            }
                    }
                }
            }

            if (SelectionTool.Instance != null)
            {
                SelectionTool.Instance.UpdateSelection();
            }
        }

        public override void OnDestroy()
        {
            if (m_listStatus != null)
            {
                Destroy(m_listStatus.gameObject);
                m_listStatus = null;
            }
            if (m_listVehicles != null)
            {
                Destroy(m_listVehicles.gameObject);
                m_listVehicles = null;
            }
            if (m_listMatches != null)
            {
                Destroy(m_listMatches.gameObject);
                m_listMatches = null;
            }
            if (m_tabStrip != null)
            {
                Destroy(m_tabStrip.gameObject);
                m_tabStrip = null;
            }
            if (m_txtSource != null)
            {
                Destroy(m_txtSource.gameObject);
                m_txtSource = null;
            }
            if (m_settingsTab != null)
            {
                m_settingsTab.Destroy();
                m_settingsTab = null;
            }
            if (m_capacityTab != null)
            {
                m_capacityTab.Destroy();
                m_capacityTab = null;
            }
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}