using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.UITabStrip;
using static TransferManagerCE.UIUtils;

namespace TransferManagerCE.UI
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
        public const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;
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
        private List<ushort> m_subBuildingIds = new List<ushort>();

        private BuildingType m_eBuildingType;

        private UITitleBar? m_title = null;
        private UITabStrip? m_tabStrip = null;
        private BuildingSettingsTab m_settingsTab = new BuildingSettingsTab();
        private BuildingCapacityTab m_capacityTab = new BuildingCapacityTab();
        private BuildingTransfersTab m_transfersTab = new BuildingTransfersTab();

        // Status tab
        private ListView? m_listStatus = null;
        private ListView? m_listVehicles = null;

        // Pathing tab
        private ListView? m_listPathing = null;
        private UIButton? m_btnReset = null;

        // Transfers tab
        private UILabel? m_lblSource = null;
        private UITextField? m_txtSource = null;
        private UIPanel? m_labelPanel = null;

        // Update panel
        private bool m_bUpdatePanel = false;
        private Coroutine? m_coroutine = null;

        public static bool IsVisible()
        {
            return Instance is not null && Instance.isVisible;
        }

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(BuildingPanel)) as BuildingPanel;
                if (Instance is null)
                {
                    Prompt.Info(TransferManagerMain.Title, "Error creating Transfer Building Panel.");
                }
            }
        }

        public BuildingPanel() : 
            base()
        {
            m_coroutine = StartCoroutine(UpdatePanelCoroutine(4));
        }

        public ushort GetBuildingId()
        {
            return m_buildingId;
        }

        public List<ushort> GetSubBuildingIds()
        {
            return m_subBuildingIds;
        }

        public int GetRestrictionId()
        {
            if (m_settingsTab is not null)
            {
                return m_settingsTab.GetRestrictionId();
            }
            return -1;
        }

        public BuildingMatches GetBuildingMatches()
        {
            return m_transfersTab.GetBuildingMatches();
        }

        public override void Start()
        {
            base.Start();
            name = "BuildingPanel";
            width = 820;
            height = 652;
            backgroundSprite = "SubcategoriesPanel";
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            clipChildren = true;
            eventVisibilityChanged += OnVisibilityChanged;
            CenterTo(parent);

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
            m_labelPanel = mainPanel.AddUIComponent<UIPanel>();
            if (m_labelPanel is not null)
            {
                m_labelPanel.width = mainPanel.width;
                m_labelPanel.height = 30;
                m_labelPanel.autoLayout = true;
                m_labelPanel.autoLayoutDirection = LayoutDirection.Horizontal;

                m_lblSource = m_labelPanel.AddUIComponent<UILabel>();
                if (m_lblSource is not null)
                {
                    m_lblSource.height = 30;
                    m_lblSource.width = m_labelPanel.width - 10 - (m_labelPanel.height - 6);
                    m_lblSource.padding = new RectOffset(4, 4, 4, 4);
                    m_lblSource.autoSize = false;
                    m_lblSource.text = "";
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
                            InstanceHelper.ShowInstance(new InstanceID { Building = m_buildingId });
                        }
                    };
                }

                // Add clear building button
                float fBUTTON_HEIGHT = m_labelPanel.height - 6;
                UIButton btnClearBuilding = UIUtils.AddSpriteButton(UIUtils.ButtonStyle.BigRound, m_labelPanel, "Niet", fBUTTON_HEIGHT, fBUTTON_HEIGHT, (component, param) =>
                {
                    ToolsModifierControl.cameraController.ClearTarget();
                    SetPanelBuilding(0);
                });
                btnClearBuilding.tooltip = Localization.Get("btnClear");
            }
            
            // Object label
            m_txtSource = UIUtils.CreateTextField(ButtonStyle.DropDown, mainPanel, "txtSource", 1.0f, mainPanel.width - 20, 30);
            if (m_txtSource is not null)
            {
                m_txtSource.text = Localization.Get("txtEnterBuildingId");
                m_txtSource.eventTextSubmitted += OnBuildingNameChanged;
                m_txtSource.eventLostFocus += OnBuildingNameLostFocus;
            }

            UpdateBuildingName();

            // Tabs
            m_tabStrip = UITabStrip.Create(TabStyle.Generic, mainPanel, width - 20f, height - m_txtSource.height - m_title.height - 10, OnTabChanged);

            // Settings tab
            m_settingsTab.SetTabBuilding(m_buildingId); 
            m_settingsTab.Setup(m_tabStrip);

            // Capacity tab
            m_capacityTab.SetTabBuilding(m_buildingId);
            m_capacityTab.Setup(m_tabStrip);

            // Status tab
            UIPanel? tabStatus = m_tabStrip.AddTabIcon("Information", Localization.Get("tabBuildingPanelStatus"), TransferManagerLoader.LoadResources(), "", 150f);
            if (tabStatus is not null)
            {
                tabStatus.autoLayout = true;
                tabStatus.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listStatus = ListView.Create<UIStatusRow>(tabStatus, "ScrollbarTrack", 0.8f, tabStatus.width, tabStatus.height - 10);
                if (m_listStatus is not null)
                {
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "S = Sick\r\nD = Dead\r\nI = Incoming\r\nW = Waiting\r\nB = Blocked", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Distance (km)", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_LARGER, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_LOAD, Localization.Get("txtLoad"), "Load", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", iCOLUMN_WIDTH_LARGER, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                }
            }
            
            UIPanel? tabVehicles = m_tabStrip.AddTabIcon("InfoIconTrafficCongestion", Localization.Get("tabBuildingPanelVehicles"), "", 230);
            if (tabVehicles is not null)
            {
                tabVehicles.autoLayout = true;
                tabVehicles.autoLayoutDirection = LayoutDirection.Vertical;

                // Vehicles list
                m_listVehicles = ListView.Create<UIVehicleRow>(tabVehicles, "ScrollbarTrack", 0.8f, tabVehicles.width, tabVehicles.height - 10);
                if (m_listVehicles is not null)
                {
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("txtLoad"), "Vehicle Load", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "W = Waiting\r\nB = Blocked", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Distance (km)", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_XLARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE_TARGET, Localization.Get("listBuildingPanelVehicleTarget"), "Target", iCOLUMN_WIDTH_250, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }
            }
            
            UIPanel? tabPathing = m_tabStrip.AddTabIcon("ToolbarIconRoads", Localization.Get("tabTransferIssuesPathing"), "", 140f);
            if (tabPathing is not null)
            {
                tabPathing.autoLayout = true;
                tabPathing.autoLayoutDirection = LayoutDirection.Vertical;

                const int iButtonHeight = 30;
                // Issue list
                m_listPathing = ListView.Create<UIPathRow>(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height - iButtonHeight - 10);
                if (m_listPathing is not null)
                {
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "Issue type", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Failure count", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Failure count", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }

                m_btnReset = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, tabPathing, Localization.Get("btnResetPathingStatistics"), "", 200, iButtonHeight, OnReset);
                if (m_btnReset is not null)
                {
                    m_btnReset.tooltip = "Reset pathing for this building.";
                }
            }

            // Transfers
            m_transfersTab.Setup(m_tabStrip);

            // Hide till needed.
            m_tabStrip.SetTabVisible((int) TabIndex.TAB_PATHING, false);

            // Select first tab
            m_tabStrip.SelectTabIndex(0);
            isVisible = true;
            UpdateTabs();
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

        public void HandleOffer(TransferOffer offer)
        {
            if (isVisible && IsTransferTabActive())
            {
                if (InstanceHelper.GetBuildings(offer.m_object).Contains(GetBuildingId()))
                {
                    InvalidatePanel();
                }
                else if (m_subBuildingIds.Count > 0) 
                {
                    foreach (ushort subBuilding in m_subBuildingIds)
                    {
                        if (InstanceHelper.GetBuildings(offer.m_object).Contains(subBuilding))
                        {
                            InvalidatePanel();
                            break;
                        }
                    }
                }
            }
        }

        public void InvalidatePanel()
        {
            m_bUpdatePanel = true;
        }

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                InvalidatePanel();
            }
        }

        private void SetPanelBuilding(ushort buildingId)
        {
            if (buildingId != m_buildingId)
            {
                m_buildingId = buildingId;
                m_subBuildingIds.Clear();

                if (buildingId != 0)
                {
                    m_eBuildingType = BuildingTypeHelper.GetBuildingType(buildingId);
#if DEBUG
                    Debug.Log($"Building type: {m_eBuildingType}");
#endif
                    // Update sub buildings (if any)
                    Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                    if (building.m_flags != 0)
                    {
                        int iLoopCount = 0;
                        ushort subBuildingId = building.m_subBuilding;
                        while (subBuildingId != 0)
                        {
                            Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                            if (subBuilding.m_flags != 0)
                            {
                                m_subBuildingIds.Add(subBuildingId);
                            }

                            // Setup for next sub building
                            subBuildingId = subBuilding.m_subBuilding;

                            if (++iLoopCount > 16384)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                    }
                } 
                else
                {
                    m_eBuildingType = BuildingType.None;
                }

                // Update tabs building id's as well
                m_settingsTab.SetTabBuilding(buildingId);
                m_capacityTab.SetTabBuilding(buildingId);
                m_transfersTab.SetTabBuilding(m_buildingId, m_subBuildingIds);

                // Match logging
                if (MatchLogging.Instance is not null)
                {
                    MatchLogging.Instance.SetBuildingId(m_buildingId, m_subBuildingIds);
                }

                if (buildingId != 0)
                {
                    // Select building with tool
                    InstanceHelper.ShowInstance(new InstanceID { Building = buildingId });

                    if (SelectionTool.Instance is not null)
                    {
                        SelectionTool.Instance.UpdateSelection();
                    } 
                }

                UpdateBuildingName();
                UpdateTabs();
                UpdatePanel();
            }
        }

        public void UpdateTabs()
        {
            if (m_tabStrip is not null)
            {
                TabIndex tab = (TabIndex) m_tabStrip.GetSelectTabIndex();

                if (SaveGameSettings.GetSettings().EnableNewTransferManager)
                {
                    m_settingsTab.UpdateSettingsTab();
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);
                }

                m_capacityTab.UpdateTab();
                m_transfersTab.UpdateTab(tab == TabIndex.TAB_TRANSFERS);

                // Vehicle tab
                int iVehicleTypes = BuildingVehicleCount.GetVehicleTypeCount(m_eBuildingType, m_buildingId);
                if (iVehicleTypes > 0)
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, true);

                    // Set tab button width based on vehicle types
                    
                    if (iVehicleTypes > 1)
                    {
                        m_tabStrip.SetTabWidth((int)TabIndex.TAB_VEHICLES, 230);
                    }
                    else
                    {
                        m_tabStrip.SetTabWidth((int)TabIndex.TAB_VEHICLES, 175f); 
                    }
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, false);
                }

                // Show/Hide pathing tab for this building
                m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, GetPathingIssues().Count > 0);

                // Make "Transfers" tab compact if Capacity or Pathing are displayed
                m_tabStrip.SetCompactMode((int)TabIndex.TAB_TRANSFERS, m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING) && m_tabStrip.IsTabVisible((int)TabIndex.TAB_CAPACITY));
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
                if (selectedInstance.Building != 0)
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

            if (m_transfersTab is not null)
            {
                m_transfersTab.GetBuildingMatches().InvalidateMatches();
            }

            if (m_listStatus is not null)
            {
                m_listStatus.Clear();
            }
            if (m_transfersTab is not null)
            {
                m_transfersTab.Clear();
            }
            if (SelectionTool.Instance is not null)
            {
                SelectionTool.Instance.Disable();
            }
            if (DistrictSelectionPanel.Instance is not null && DistrictSelectionPanel.Instance.isVisible)
            {
                DistrictSelectionPanel.Instance.Hide();
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
            if (m_subBuildingIds.Count > 0)
            {
                foreach (ushort  subBuildingId in m_subBuildingIds)
                {
                    PathFindFailure.ResetPathingStatistics(subBuildingId);
                }
            }
            UpdatePanel();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            HidePanel();
        }

        public void OnStatsClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StatsPanel.Init();
            if (StatsPanel.Instance is not null)
            {
                StatsPanel.Instance.Show();
            }
        }

        public void OnIssuesClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransferIssuePanel.Init();
            if (TransferIssuePanel.Instance is not null)
            {
                TransferIssuePanel.Instance.Show();
            }
        }
        
        public void OnOutsideClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            OutsideConnectionPanel.Init();
            if (OutsideConnectionPanel.Instance is not null)
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
            if (m_title is not null && m_title.m_btnHighlight is not null)
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
                if (SelectionTool.Instance is not null)
                {
                    SelectionTool.Instance.UpdateSelection();
                }
            }
        }

        public void OnTabChanged(int index)
        {
            // Close district panel
            if (DistrictSelectionPanel.Instance is not null && DistrictSelectionPanel.Instance.isVisible)
            {
                DistrictSelectionPanel.Instance.Hide();
            }

            // Turn off building selection mode
            if (SelectionTool.Instance is not null)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
            }

            InvalidatePanel();
        }

        public List<VehicleData> GetVehicles()
        {
            return new BuildingOwnVehicles().GetVehicles(m_buildingId);
        }

        public bool IsTransferTabActive()
        {
            if (m_tabStrip is not null)
            {
                return (TabIndex)m_tabStrip.GetSelectTabIndex() == TabIndex.TAB_TRANSFERS;
            }
            return false;
        }

        public List<PathingContainer> GetPathingIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            InstanceID instance = new InstanceID {  Building = m_buildingId };

            // Local path failes
            Dictionary<Util.PATHFINDPAIR, long> failures = Util.PathFindFailure.GetPathFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in failures)
            {
                if (kvp.Key.m_source == instance)
                {
                    list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Source, false));
                }
                else if (kvp.Key.m_target == instance)
                {
                    list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Target, false));
                }
                // Check sub building as well
                else if (m_subBuildingIds.Count > 0)
                {
                    foreach (ushort subBuildingId in m_subBuildingIds)
                    {
                        if (kvp.Key.m_source.Building != 0 && kvp.Key.m_source.Building == subBuildingId)
                        {
                            list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Source, true));
                        }
                        else if (kvp.Key.m_target.Building != 0 && kvp.Key.m_target.Building == subBuildingId)
                        {
                            list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Target, true));
                        }
                    }
                    
                }
            }

            // Outside path fails
            Dictionary<Util.PATHFINDPAIR, long> outside = Util.PathFindFailure.GetOutsideFailsCopy();
            foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in outside)
            {
                if (kvp.Key.m_source == instance)
                {
                    list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Source, false));
                }
                else if (kvp.Key.m_target == instance)
                {
                    list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Target, false));
                }
                // Check sub building as well
                else if (m_subBuildingIds.Count > 0)
                {
                    foreach (ushort subBuildingId in m_subBuildingIds)
                    {
                        if (kvp.Key.m_source.Building != 0 && kvp.Key.m_source.Building == subBuildingId)
                        {
                            list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Source, true));
                        }
                        else if (kvp.Key.m_target.Building != 0 && kvp.Key.m_target.Building == subBuildingId)
                        {
                            list.Add(new PathingContainer(kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Target, true));
                        }
                    }
                }
            }

            return list;
        }

        public void UpdateBuildingName()
        {
            if (m_txtSource is not null && m_labelPanel is not null)
            {
                if (m_buildingId != 0)
                {
                    string sText;
                    if (!DependencyUtils.IsAdvancedOutsideConnectionsRunning() && IsOutsideConnection(m_buildingId))
                    {
                        m_labelPanel.isVisible = false;
                        m_txtSource.isVisible = true;

                        if (OutsideConnectionSettings.HasSettings(m_buildingId))
                        {
                            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
                            sText = settings.GetName(m_buildingId);
                        }
                        else
                        {
                            sText = CitiesUtils.GetBuildingName(m_buildingId);
                        }
                    }
                    else
                    {
                        m_labelPanel.isVisible = true;
                        m_txtSource.isVisible = false;

                        sText = CitiesUtils.GetBuildingName(m_buildingId, ModSettings.GetSettings().ShowBuildingId);
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
                    m_txtSource.text = Localization.Get("txtEnterBuildingId");
                    m_txtSource.isVisible = true;
                    m_labelPanel.isVisible = false;
                }
            }
        }

        public void OnBuildingNameChanged(UIComponent c, string text)
        {
            if (m_buildingId == 0)
            {
                // Try and decode text into id of building
                if (Int32.TryParse(text, out int iBuildingId) && iBuildingId > 0 && iBuildingId < BuildingManager.instance.m_buildings.m_size)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[iBuildingId];
                    if (building.m_flags != 0)
                    {
                        SetPanelBuilding((ushort)iBuildingId);
                    }
                    else
                    {
                        m_txtSource.text = Localization.Get("txtEnterBuildingId");
                    }
                }
                else
                {
                    m_txtSource.text = Localization.Get("txtEnterBuildingId");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(m_txtSource.text))
                {
                    // Clear existing connection name (if any)
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
                if (OutsideConnectionPanel.Instance is not null && OutsideConnectionPanel.Instance.isVisible)
                {
                    OutsideConnectionPanel.Instance.UpdatePanel();
                }
            }
        }

        public void OnBuildingNameLostFocus(UIComponent c, UIFocusEventParameter e)
        {
            OnBuildingNameChanged(c, m_txtSource.text);
        }

        public override void Update()
        {
            if (m_bUpdatePanel)
            {
                UpdatePanel();
                m_bUpdatePanel = false;
            }
            base.Update();
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
            if (m_tabStrip is null || !isVisible)
            {
                return;
            }

#if DEBUG
            if (m_title is not null)
            {
                m_title.title = $"{TransferManagerMain.Title} Avg:{SimulationManager.instance.m_simulationProfiler.m_averageStepDuration}";
            }
#endif
            // Check the building is still valid
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags == 0)
            {
                SetPanelBuilding(0);
            }

            if (m_tabStrip is not null)
            {
                List<StatusData>? statusList = null;
                int iStatusCount = 0;

                // Update status tab count
                if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_STATUS))
                {
                    string sMessage = Localization.Get("tabBuildingPanelStatus");
                    if (m_buildingId != 0)
                    {
                        // Add vehicle count if there are any guest vehicles
                        statusList = new StatusHelper().GetStatusList(m_buildingId, out iStatusCount);
                        if (iStatusCount > 0)
                        {
                            sMessage += " (" + iStatusCount + ")";
                        }
                    }

                    m_tabStrip.SetTabText((int)TabIndex.TAB_STATUS, sMessage);
                }

                // Update vehicle tab count
                if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_VEHICLES))
                {
                    m_tabStrip.SetTabText((int)TabIndex.TAB_VEHICLES, BuildingVehicleCount.GetVehicleTabText(m_eBuildingType, m_buildingId, building));
                }

                // Show pathing tab if there are any pathing uissues, do not hide pathing tab once it is shown for this building.
                List<PathingContainer> listPathing = GetPathingIssues();

                if (listPathing.Count > 0 && !m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, true);
                }
                else if (m_buildingId == 0)
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, false);
                }

                // Update pathing tab count
                if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                {
                    m_tabStrip.SetTabText((int)TabIndex.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + "(" + listPathing.Count + ")");
                }

                switch ((TabIndex)m_tabStrip.GetSelectTabIndex())
                {
                    case TabIndex.TAB_SETTINGS: // Settings
                        {
                            break;
                        }
                    case TabIndex.TAB_STATUS: // Status
                        {
                            if (m_listStatus is not null && statusList is not null)
                            {
                                // Services
                                m_listStatus.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = statusList.ToArray(),
                                    m_size = statusList.Count,
                                };
                            }
                            break;
                        }
                    case TabIndex.TAB_VEHICLES: // Vehicles
                        {
                            if (m_listVehicles is not null)
                            {
                                List<VehicleData> vehicles = GetVehicles();

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
                            if (m_listPathing is not null && m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                            {
                                listPathing.Sort(PathingContainer.CompareToTime);
                                m_listPathing.GetList().rowsData = new FastList<object>
                                {
                                    m_buffer = listPathing.ToArray(),
                                    m_size = listPathing.Count,
                                };
                            }
                            break;
                        }
                    case TabIndex.TAB_TRANSFERS: // Transfers
                        {
                            if (m_transfersTab is not null)
                            {
                                m_transfersTab.UpdateTab(true);
                            }
                            break;
                        }
                }
            }

            if (SelectionTool.Instance is not null)
            {
                SelectionTool.Instance.UpdateSelection();
            }
        }

        public override void OnDestroy()
        {
            if (m_coroutine is not null)
            {
                StopCoroutine(m_coroutine);
            }
            if (m_listStatus is not null)
            {
                Destroy(m_listStatus.gameObject);
                m_listStatus = null;
            }
            if (m_listVehicles is not null)
            {
                Destroy(m_listVehicles.gameObject);
                m_listVehicles = null;
            }
            if (m_transfersTab is not null)
            {
                m_transfersTab.Destroy();
            }
            if (m_tabStrip is not null)
            {
                Destroy(m_tabStrip.gameObject);
                m_tabStrip = null;
            }
            if (m_txtSource is not null)
            {
                Destroy(m_txtSource.gameObject);
                m_txtSource = null;
            }
            if (m_settingsTab is not null)
            {
                m_settingsTab.Destroy();
                m_settingsTab = null;
            }
            if (m_capacityTab is not null)
            {
                m_capacityTab.Destroy();
                m_capacityTab = null;
            }
            if (Instance is not null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}