using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.UITabStrip;

namespace TransferManagerCE.UI
{
    public class BuildingPanel : UIMainPanel<BuildingPanel>
    {
        public enum TabIndex
        {
            TAB_SETTINGS = 0,
            TAB_CAPACITY = 1,
            TAB_STATUS = 2,
            TAB_VEHICLES = 3,
            TAB_PATHING = 4,
            TAB_TRANSFERS = 5,
        }

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

        private UILabel? m_lblSource = null;
        private UITextField? m_txtSource = null;
        private UIPanel? m_labelPanel = null;

        // Update panel
        private bool m_bUpdatePanel = false;
        private Coroutine? m_coroutine = null;
        private bool m_bAfterStart = false;

        private List<BuildingTab> m_buildingTabs = new List<BuildingTab>();
        private UIInfoLabel? m_infoLabel = null;

        // ----------------------------------------------------------------------------------------
        public BuildingPanel() : 
            base()
        {
            m_coroutine = StartCoroutine(UpdatePanelCoroutine(4));

            // Add tabs
            m_buildingTabs.Add(new BuildingSettingsTab());
            m_buildingTabs.Add(new BuildingCapacityTab());
            m_buildingTabs.Add(new BuildingStatusTab());
            m_buildingTabs.Add(new BuildingVehicleTab());
            m_buildingTabs.Add(new BuildingPathingTab());
            m_buildingTabs.Add(new BuildingTransfersTab());
        }

        public BuildingSettingsTab GetSettingsTab()
        {
            return (BuildingSettingsTab)m_buildingTabs[(int)TabIndex.TAB_SETTINGS];
        }

        public BuildingTransfersTab GetTransfersTab()
        {
            return (BuildingTransfersTab)m_buildingTabs[(int)TabIndex.TAB_TRANSFERS];
        }

        public BuildingStatusTab GetStatusTab()
        {
            return (BuildingStatusTab)m_buildingTabs[(int)TabIndex.TAB_STATUS];

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
            if (GetSettingsTab() is not null)
            {
                return GetSettingsTab().GetRestrictionId();
            }
            return 0;
        }

        public BuildingMatches GetBuildingMatches()
        {
            return GetTransfersTab().GetBuildingMatches();
        }

        
        public StatusHelper GetStatusHelper()
        {
            return GetStatusTab().m_statusHelper;
        }

        public override void Start()
        {
            base.Start();
            name = "BuildingPanel";
            width = 820;
            height = 710;
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
            m_title = UITitleBar.Create(this, TransferManagerMod.Instance.Name, "Transfer", TransferManagerMod.Instance.LoadResources(), OnTitleCloseClick);
            if (m_title != null)
            {
                m_title.AddButton("btnStats", atlas, "ThumbStatistics", "Show Statistics Panel", OnStatsClick);
                m_title.AddButton("btnSettings", atlas, "Options", "Show list of buildings with TMCE settings", OnSettingsClick);
                m_title.AddButton("btnIssues", atlas, "IconWarning", "Show Issues Panel", OnIssuesClick);
                m_title.AddButton("btnOutside", atlas, "InfoIconOutsideConnections", "Show Outside Connections Panel", OnOutsideClick);
                m_title.AddButton("btnHighlight", atlas, "InfoIconLevel", "Highlight Matches", OnHighlightClick);
                m_title.SetupButtons();
                UpdateHighlightButtonIcon();
            }

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
                UIButton btnClearBuilding = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.BigRound, m_labelPanel, "Niet", fBUTTON_HEIGHT, fBUTTON_HEIGHT, (component, param) =>
                {
                    ToolsModifierControl.cameraController.ClearTarget();
                    SetBuilding(0);
                });
                btnClearBuilding.tooltip = Localization.Get("btnClear");
            }
            
            // Object label
            m_txtSource = UIMyUtils.CreateTextField(UIMyUtils.ButtonStyle.DropDown, mainPanel, "txtSource", 1.0f, mainPanel.width - 20, 30);
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

            // Setup tabs
            for (int i = 0; i < m_buildingTabs.Count; ++i)
            {
                m_buildingTabs[i].Setup(m_tabStrip);
            }

            m_bAfterStart = true;

            m_infoLabel = new UIInfoLabel(this);

            // Select first tab
            m_tabStrip.SelectFirstVisibleTab();
            isVisible = true;

            UpdateTabs();
            UpdatePanel();
        }

        public void HandleOffer(TransferOffer offer)
        {
            if (isVisible && IsTransferTabActive() && GetBuildingId() != 0)
            {
                HashSet<ushort> buildingIds = InstanceHelper.GetBuildings(offer.m_object);
                if (buildingIds.Contains(GetBuildingId()))
                {
                    InvalidatePanel();
                }
                else if (m_subBuildingIds.Count > 0) 
                {
                    for (int i = 0; i < m_subBuildingIds.Count; ++i)
                    {
                        ushort buildingId = m_subBuildingIds[i];
                        if (buildingIds.Contains(buildingId))
                        {
                            InvalidatePanel();
                            break;
                        }
                    }
                }
            }
        }

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
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

                UpdateBuildingName();

                if (m_bAfterStart)
                {
                    UpdateTabs();
                    UpdatePanel();
                }

                // Activate selection tool if needed
                if (SelectionTool.Instance is null)
                {
                    SelectionTool.AddSelectionTool();
                }
                if (!SelectionTool.Active)
                {
                    SelectionTool.Instance.Enable(SelectionTool.SelectionToolMode.Normal);
                }
            }
            else
            {
                m_buildingId = 0;
                m_eBuildingType = BuildingType.None;

                for (int i = 0; i < m_buildingTabs.Count; ++i)
                {
                    m_buildingTabs[i].Clear();
                }

                if (tooltipBox is not null)
                {
                    tooltipBox.tooltip = "";
                    tooltipBox.tooltipBox.Hide();
                }

                if (SelectionTool.Exists)
                {
                    SelectionTool.Instance.Disable();
                }

                if (DistrictSelectionPanel.IsVisible())
                {
                    DistrictSelectionPanel.Instance.Hide();
                }
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
                    CDebug.Log($"Building type: {m_eBuildingType}");
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
                // Setup tabs
                foreach (BuildingTab buildingTab in m_buildingTabs)
                {
                    buildingTab.SetTabBuilding(m_buildingId, m_eBuildingType, m_subBuildingIds);
                }

                if (buildingId != 0)
                {
                    // Select building with tool
                    InstanceHelper.ShowInstance(new InstanceID { Building = buildingId });

                    if (SelectionTool.Exists)
                    {
                        SelectionTool.Instance.UpdateSelection();
                    } 
                }

                if (SettingsPanel.IsVisible())
                {
                    SettingsPanel.Instance.InvalidatePanel();
                }

                UpdateBuildingName();
                UpdateTabs();
                UpdatePanel();
            }
        }

        public void UpdateTabs()
        {
            if (m_bAfterStart)
            {
                // Update tab information
                for (int i = 0; i < m_buildingTabs.Count; ++i)
                {
                    bool bTabVisible = m_buildingTabs[i].ShowTab();
                    m_tabStrip.SetTabVisible(i, bTabVisible);
                    if (bTabVisible)
                    {
                        m_buildingTabs[i].UpdateTab(m_tabStrip.GetSelectTabIndex() == i);
                    }
                }

                m_tabStrip.PerformLayout();
            }
        }

        public void ShowPanel(ushort buildingId)
        {
            SetBuilding(buildingId);
            Show();
        }

        public void OnTitleCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        public void OnStatsClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            StatsPanel.TogglePanel();
        }

        public void OnIssuesClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransferIssuePanel.TogglePanel();
        }
        
        public void OnOutsideClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            OutsideConnectionPanel.TogglePanel();
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
            SettingsPanel.TogglePanel();
        }

        public void UpdateHighlightButtonIcon()
        {
            if (m_title is not null)
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

                m_title.Buttons[4].normalBgSprite = sIcon;
                m_title.Buttons[4].tooltip = sTooltip;
                m_title.Buttons[4].RefreshTooltip();

                // Update selection
                if (SelectionTool.Exists)
                {
                    SelectionTool.Instance.UpdateSelection();
                }
            }
        }

        public void OnTabChanged(int index)
        {
            // Close district panel
            if (DistrictSelectionPanel.IsVisible())
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
                            sText = CitiesUtils.GetBuildingName(m_buildingId, InstanceID.Empty);
                        }
                    }
                    else
                    {
                        m_labelPanel.isVisible = true;
                        m_txtSource.isVisible = false;

                        sText = CitiesUtils.GetBuildingName(m_buildingId, InstanceID.Empty, ModSettings.GetSettings().ShowBuildingId);
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
                if (OutsideConnectionPanel.IsVisible())
                {
                    OutsideConnectionPanel.Instance.InvalidatePanel();
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

        protected override void UpdatePanel()
        {
            if (!m_bAfterStart || !isVisible)
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

            if (m_bAfterStart)
            {
                for (int i = 0; i < m_buildingTabs.Count; ++i)
                {
                    m_buildingTabs[i].UpdateTab(m_tabStrip.GetSelectTabIndex() == i);
                }
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
            for (int i = 0; i < m_buildingTabs.Count; ++i)
            {
                m_buildingTabs[i].Destroy();
                m_buildingTabs[i] = null;
                m_buildingTabs.Clear();
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
        }

        public void ShowInfo(string sText)
        {
            if (m_infoLabel is not null)
            {
                m_infoLabel.text = sText;
                m_infoLabel.Show();
            }
        }

        public void HideInfo()
        {
            if (m_infoLabel is not null)
            {
                m_infoLabel.Hide();
            }
        }
    }
}