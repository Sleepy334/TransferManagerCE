using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Settings;
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

        // Performance stats
        public static long s_lastUpdateTicks = 0;
        public static long s_maxUpdateTicks = 0;
        public static long s_totalUpdateTicks = 0;
        public static int s_totalUpdates = 0;

        // Currently viewed building id.
        private ushort m_buildingId = 0;
        private List<ushort> m_subBuildingIds = new List<ushort>();
        private BuildingType m_eBuildingType;

        private UITitleBar? m_title = null;
        private UITabStrip? m_tabStrip = null;

        // Tab objects
        private BuildingSettingsTab m_settingsTab = new BuildingSettingsTab();
        private BuildingCapacityTab m_capacityTab = new BuildingCapacityTab();
        public BuildingStatusTab m_statusTab = new BuildingStatusTab();
        private BuildingVehicleTab m_vehicleTab = new BuildingVehicleTab();
        private BuildingPathingTab m_pathingTab = new BuildingPathingTab();
        private BuildingTransfersTab m_transfersTab = new BuildingTransfersTab();

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

        public StatusHelper GetStatusHelper()
        {
            return m_statusTab.m_statusHelper;
        }

        public override void Start()
        {
            base.Start();
            name = "BuildingPanel";
            width = 820;
            height = 670;
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
            
            // Panel position
            if (ModSettings.GetSettings().BuildingPanelPosX == float.MaxValue || ModSettings.GetSettings().BuildingPanelPosY == float.MaxValue)
            {
                CenterTo(parent);
            }
            else
            {
                absolutePosition = new Vector3(ModSettings.GetSettings().BuildingPanelPosX, ModSettings.GetSettings().BuildingPanelPosY);
            }

            // Save new position
            eventPositionChanged += (component, pos) =>
            {
                ModSettings settings = ModSettings.GetSettings();
                settings.BuildingPanelPosX = absolutePosition.x;
                settings.BuildingPanelPosY = absolutePosition.y;
                settings.Save();
            };


            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.SetStatsHandler(OnStatsClick);
            m_title.SetIssuesHandler(OnIssuesClick);
            m_title.SetOutsideHandler(OnOutsideClick);
            m_title.SetHighlightHandler(OnHighlightClick);
            m_title.SetSettingsHandler(OnSettingsClick);
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
                    SetBuilding(0);
                });
                btnClearBuilding.tooltip = Localization.Get("btnClear");
            }
            
            // Object label
            m_txtSource = UIUtils.CreateTextField(ButtonStyle.DropDown, mainPanel, "txtSource", 1.0f, mainPanel.width - 20, 30);
            if (m_txtSource is not null)
            {
                m_txtSource.text = Localization.Get("txtEnterBuildingId"); // Add a user hint
                m_txtSource.eventTextSubmitted += OnBuildingNameChanged;
                m_txtSource.eventLostFocus += OnBuildingNameLostFocus;

                // Clear hint on focus
                m_txtSource.eventGotFocus += (c, e) =>
                {
                    if (m_txtSource.text == Localization.Get("txtEnterBuildingId"))
                    {
                        m_txtSource.text = "";
                        m_txtSource.Focus();     
                    }
                };
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
            m_statusTab.SetTabBuilding(m_buildingId);
            m_statusTab.Setup(m_tabStrip);

            // Vehicle tab
            m_vehicleTab.SetTabBuilding(m_buildingId);
            m_vehicleTab.Setup(m_tabStrip);

            // Pathing tab
            m_pathingTab.SetTabBuilding(m_buildingId);
            m_pathingTab.Setup(m_tabStrip);

            // Transfers
            m_transfersTab.SetTabBuilding(m_buildingId, GetSubBuildingIds());
            m_transfersTab.Setup(m_tabStrip);

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
            else if (tooltipBox is not null)
            {
                tooltipBox.tooltip = "";
                tooltipBox.tooltipBox.Hide();
            }
        }

        private void SetBuilding(ushort buildingId)
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
                m_statusTab.SetTabBuilding(buildingId);
                m_vehicleTab.SetTabBuilding(buildingId);
                m_pathingTab.SetTabBuilding(buildingId);
                m_transfersTab.SetTabBuilding(m_buildingId, m_subBuildingIds);

                if (buildingId != 0)
                {
                    // Select building with tool
                    InstanceHelper.ShowInstance(new InstanceID { Building = buildingId });

                    if (SelectionTool.Instance is not null)
                    {
                        SelectionTool.Instance.UpdateSelection();
                    } 
                }

                if (SettingsPanel.Instance is not null)
                {
                    SettingsPanel.Instance.UpdatePanel();
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
                // Update tab information
                m_settingsTab.UpdateTab(m_tabStrip);
                m_capacityTab.UpdateTab(m_tabStrip);
                m_statusTab.UpdateTab(m_tabStrip);
                m_vehicleTab.UpdateTab(m_tabStrip, m_eBuildingType);
                m_pathingTab.UpdateTab(m_tabStrip);
                m_transfersTab.UpdateTab(m_tabStrip);
                m_tabStrip.PerformLayout();
            }
        }

        public void ShowPanel(ushort buildingId)
        {
            SetBuilding(buildingId);
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
                    SetBuilding(selectedInstance.Building);
                }
            }

            // Activate selection tool if needed
            if (SelectionTool.Instance is null)
            {
                SelectionTool.AddSelectionTool();
            }
            if (SelectionTool.Instance is not null && ToolsModifierControl.toolController.CurrentTool != SelectionTool.Instance)
            {
                SelectionTool.Instance.Enable();
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
            if (m_statusTab is not null)
            {
                m_statusTab.Clear();
            }
            if (m_vehicleTab is not null)
            {
                m_vehicleTab.Clear();
            }
            if (m_pathingTab is not null)
            {
                m_pathingTab.Clear();
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

        public void OnSettingsClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            // Create panel if needed
            SettingsPanel.Init();
            if (SettingsPanel.Instance is not null)
            {
                // Open panel
                SettingsPanel.Instance.TogglePanel();
            }
        }

        public void UpdateHighlightButtonIcon()
        {
            if (m_title is not null && m_title.m_btnHighlight is not null)
            {
                string sIcon = "";
                string sTooltip = "";
                switch ((ModSettings.BuildingHighlightMode)ModSettings.GetSettings().HighlightMatchesState)
                {
                    case ModSettings.BuildingHighlightMode.None:
                        {
                            sIcon = "InfoIconLevelPressed";
                            sTooltip = Localization.Get("tooltipHighlightModeOff");
                            break;
                        }
                    case ModSettings.BuildingHighlightMode.Matches:
                        {
                            sIcon = "InfoIconLevel";
                            sTooltip = Localization.Get("tooltipHighlightModeMatches");
                            break;
                        }
                    case ModSettings.BuildingHighlightMode.Issues:
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

        public bool IsTransferTabActive()
        {
            if (m_tabStrip is not null)
            {
                return (TabIndex)m_tabStrip.GetSelectTabIndex() == TabIndex.TAB_TRANSFERS;
            }
            return false;
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
                        SetBuilding((ushort)iBuildingId);
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

            Stopwatch stopwatch = Stopwatch.StartNew();
            long startTicks = stopwatch.ElapsedTicks;

            // Check the building is still valid
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags == 0)
            {
                SetBuilding(0);
            }

            if (m_tabStrip is not null)
            {
                m_statusTab.UpdateTab(m_tabStrip);
                m_vehicleTab.UpdateTab(m_tabStrip, m_eBuildingType);
                m_pathingTab.UpdateTab(m_tabStrip);
                m_transfersTab.UpdateTab(m_tabStrip);
            }

            if (SelectionTool.Instance is not null)
            {
                SelectionTool.Instance.UpdateSelection();
            }

            long stopTicks = stopwatch.ElapsedTicks;
            s_lastUpdateTicks = stopTicks - startTicks;
            s_maxUpdateTicks = (long)Mathf.Max(s_lastUpdateTicks, s_maxUpdateTicks);
            s_totalUpdateTicks += s_lastUpdateTicks;
            s_totalUpdates += 1;
        }

        public override void OnDestroy()
        {
            if (m_coroutine is not null)
            {
                StopCoroutine(m_coroutine);
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
            if (m_statusTab is not null)
            {
                m_statusTab.Destroy();
            }
            if (m_vehicleTab is not null)
            {
                m_vehicleTab.Destroy();
            }
            if (m_pathingTab is not null)
            {
                m_pathingTab.Destroy();
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
            if (Instance is not null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}