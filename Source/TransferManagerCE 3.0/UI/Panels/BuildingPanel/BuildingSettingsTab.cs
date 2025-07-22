using ColossalFramework.UI;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using System.Linq;
using static TransferManagerCE.UITabStrip;
using static TransferManagerCE.UI.BuildingPanel;
using TransferManagerCE.Settings;
using TransferManagerCE.CustomManager;
using SleepyCommon;
using TransferManagerCE.Util;

namespace TransferManagerCE.UI
{
    public class BuildingSettingsTab : BuildingTab
    {
        const float fTEXT_SCALE = 0.9f;
        const int iButtonHeight = 28;

        // Settings tab
        private UITabStrip? m_tabStripTransferReason = null;
        private UIPanel? m_pnlMain = null;
        private UIPanel? m_panelTabPanel = null;

        private UIGroup? m_grpDistrictRestrictions = null;

        private UIDistrictRestrictionsPanel? m_incomingDistrictPanel = null;
        private UIDistrictRestrictionsPanel? m_outgoingDistrictPanel = null;

        // Distance restriction
        private UIGroup? m_grpServiceDistance = null;
        private UIDistancePanel? m_sliderIncomingServiceDistance = null;
        private UIDistancePanel? m_sliderOutgoingServiceDistance = null;
        private UILabel? m_lblDistanceGlobal = null;
        private UIPanel? m_globalDistancePanel = null;

        private UIGroup? m_panelImportExport = null;
        private UICheckBox? m_chkAllowImport = null;
        private UICheckBox? m_chkAllowExport = null;
        private UIToggleButton? m_btnExcludeOutsideConnecions = null;
        private UIButton? m_btnOutsideClear = null;


        private UIPanel? m_otherSettingsPanel = null;
        private UIGroup? m_grpOutsidePriority = null;
        private SettingsSlider? m_sliderOutsideCargoPriority = null;
        private SettingsSlider? m_sliderOutsideCitizenPriority = null;

        // Warehouse options
        private UIGroup? m_panelGoodsDelivery = null;
        private SettingsSlider? m_sliderReserveCargoTrucks = null;

        private UIGroup? m_buildingRestrictionGroup = null;

        private UIBuildingRestrictionsPanel m_pnlBuildingRestrictionsIncoming = null;
        private UIBuildingRestrictionsPanel m_pnlBuildingRestrictionsOutgoing = null;

        // Apply to all
        private UIApplyToAll? m_applyToAll = null;

        private int m_iRestrictionTabIndex = 0;
        private bool m_bInSetup = false;

        private static OutsideConnectionCurve s_connectionCurve = new OutsideConnectionCurve();

        // ----------------------------------------------------------------------------------------
        public BuildingSettingsTab() :
            base()
        {
        }

        public int RestrictionTabIndex
        {
            get 
            { 
                return m_iRestrictionTabIndex; 
            }
            set
            {
                m_iRestrictionTabIndex = Mathf.Max(0, value);
            }
        }

        public int GetRestrictionId()
        {
            if (m_tabStripTransferReason is not null && m_tabStripTransferReason.Count > RestrictionTabIndex)
            {
                return m_tabStripTransferReason.GetTabId(RestrictionTabIndex);
            }
            return -1;
        }

        public override void SetTabBuilding(ushort buildingId, BuildingType buildingType, List<ushort> subBuildingIds)
        {
            if (buildingId != m_buildingId)
            {
                RestrictionTabIndex = 0;

                if (DistrictSelectionPanel.IsVisible())
                {
                    DistrictSelectionPanel.Instance.Hide();
                }

                if (OutsideConnectionSelectionPanel.IsVisible())
                {
                    OutsideConnectionSelectionPanel.Instance.Hide();
                }
            }

            base.SetTabBuilding(buildingId, buildingType, subBuildingIds);
        }

        public override bool ShowTab()
        {
            if (m_buildingId == 0)
            {
                return false;
            }

            return BuildingRuleSets.GetRules(m_eBuildingType, m_buildingId).Count > 0;
        }

        public override void SetupInternal()
        {
            m_bInSetup = true;

            UIPanel? tabSettings = m_tabStrip.AddTabIcon("Options", Localization.Get("tabBuildingPanelSettings"), "", 110f);
            if (tabSettings is not null)
            {
                tabSettings.autoLayout = true;
                tabSettings.autoLayoutDirection = LayoutDirection.Vertical;
                tabSettings.padding = new RectOffset(10, 10, 0, 10);

                m_pnlMain = tabSettings.AddUIComponent<UIPanel>();
                m_pnlMain.width = tabSettings.width - 20;
                m_pnlMain.height = 400; // This gets adjusted at the end
                //m_pnlMain.backgroundSprite = "InfoviewPanel";
                //m_pnlMain.color = new Color32(150, 0, 0, 255);

                // Color the tabs differently so it's easier to distingush between multuiple tab levels
                m_tabStripTransferReason = UITabStrip.Create(TabStyle.SubBar, m_pnlMain, tabSettings.width - 20f, 18f, null);
                m_tabStripTransferReason.relativePosition = new Vector2(0, 0);

                m_panelTabPanel = m_pnlMain.AddUIComponent<UIPanel>();
                m_panelTabPanel.relativePosition = new Vector2(0, m_tabStripTransferReason.height + 6);
                m_panelTabPanel.width = m_tabStripTransferReason.width;
                m_panelTabPanel.height = 400; // This gets adjusted at the end
                m_panelTabPanel.backgroundSprite = "InfoviewPanel";
                m_panelTabPanel.color = new Color32(150, 150, 150, 255);
                m_panelTabPanel.autoLayout = true;
                m_panelTabPanel.autoLayoutDirection = LayoutDirection.Vertical;
                m_panelTabPanel.autoLayoutPadding = new RectOffset(10, 0, 6, 0);
                m_panelTabPanel.clipChildren = true;

                // ------------------------------------------------------------
                // Prefer local services
                m_grpDistrictRestrictions = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_DISTRICT"), fTEXT_SCALE, m_panelTabPanel.width - 20, 140);
                if (m_grpDistrictRestrictions is not null)
                {
                    // Incoming restrictions
                    m_incomingDistrictPanel = UIDistrictRestrictionsPanel.Create(m_grpDistrictRestrictions.m_content, true, fTEXT_SCALE);
                    m_incomingDistrictPanel.eventSelectedIndexChanged += OnPreferLocalServices;
                    m_incomingDistrictPanel.eventOnDistrictClicked += OnSelectDistrictClicked;
                    m_incomingDistrictPanel.eventOnDistrictTooltipEnter += UpdateDistrictButtonTooltip;
                    m_incomingDistrictPanel.eventOnDistrictClearClicked += OnDistrictRestrictionsClearClicked;

                    // Incoming restrictions
                    m_outgoingDistrictPanel = UIDistrictRestrictionsPanel.Create(m_grpDistrictRestrictions.m_content, false, fTEXT_SCALE);
                    m_outgoingDistrictPanel.eventSelectedIndexChanged += OnPreferLocalServices;
                    m_outgoingDistrictPanel.eventOnDistrictClicked += OnSelectDistrictClicked;
                    m_outgoingDistrictPanel.eventOnDistrictTooltipEnter += UpdateDistrictButtonTooltip;
                    m_outgoingDistrictPanel.eventOnDistrictClearClicked += OnDistrictRestrictionsClearClicked;
                }

                // ------------------------------------------------------------
                // Building restrictions
                m_buildingRestrictionGroup = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_BUILDING_RESTRICTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 100);
                if (m_buildingRestrictionGroup is not null)
                {
                    // Incoming
                    m_pnlBuildingRestrictionsIncoming = UIBuildingRestrictionsPanel.Create(m_buildingRestrictionGroup.m_content, true, fTEXT_SCALE);
                    m_pnlBuildingRestrictionsIncoming.eventOnBuildingRestrictionsClicked += OnBuildingRestrictionsClicked;
                    m_pnlBuildingRestrictionsIncoming.eventOnBuildingRestrictionsClearClicked += OnBuildingRestrictionsClearClicked;

                    // Outgoing
                    m_pnlBuildingRestrictionsOutgoing = UIBuildingRestrictionsPanel.Create(m_buildingRestrictionGroup.m_content, false, fTEXT_SCALE);
                    m_pnlBuildingRestrictionsOutgoing.eventOnBuildingRestrictionsClicked += OnBuildingRestrictionsClicked;
                    m_pnlBuildingRestrictionsOutgoing.eventOnBuildingRestrictionsClearClicked += OnBuildingRestrictionsClearClicked;
                }

                // ------------------------------------------------------------
                // Service distance
                m_grpServiceDistance = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 80);
                if (m_grpServiceDistance is not null)
                {
                    m_sliderIncomingServiceDistance = UIDistancePanel.Create(m_grpServiceDistance.m_content, true, SelectionModeBase.GetKnownColor(true), fTEXT_SCALE, (value) => OnServiceDistanceChanged(true, value));
                    m_sliderOutgoingServiceDistance = UIDistancePanel.Create(m_grpServiceDistance.m_content, false, SelectionModeBase.GetKnownColor(false), fTEXT_SCALE, (value) => OnServiceDistanceChanged(false, value));

                    m_globalDistancePanel = m_grpServiceDistance.m_content.AddUIComponent<UIPanel>();
                    m_globalDistancePanel.width = 400;
                    m_globalDistancePanel.height = 26;
                    m_globalDistancePanel.autoLayout = true;
                    m_globalDistancePanel.autoLayoutDirection = LayoutDirection.Horizontal;
                    m_globalDistancePanel.autoLayoutPadding = new RectOffset(4, 4, 4, 4);
                    //globalLayout.backgroundSprite = "InfoviewPanel";
                    //globalLayout.color = SleepyCommon.KnownColor.red;

                    m_lblDistanceGlobal = m_globalDistancePanel.AddUIComponent<UILabel>();
                    m_lblDistanceGlobal.text = Localization.Get("txtGlobalDistanceRestriction");
                    m_lblDistanceGlobal.textScale = fTEXT_SCALE;
                    m_lblDistanceGlobal.font = UIFonts.Regular;
                    m_lblDistanceGlobal.autoSize = false;// true;
                    m_lblDistanceGlobal.height = m_globalDistancePanel.height;
                    m_lblDistanceGlobal.width = 294;
                    m_lblDistanceGlobal.verticalAlignment = UIVerticalAlignment.Middle;

                    UIPanel spacer = m_globalDistancePanel.AddUIComponent<UIPanel>();
                    spacer.width = m_globalDistancePanel.height;
                    spacer.height = m_globalDistancePanel.height;

                    UIPanel colorPanel = spacer.AddUIComponent<UIPanel>();
                    colorPanel.width = 15;
                    colorPanel.height = 15;
                    colorPanel.backgroundSprite = "InfoviewPanel";
                    colorPanel.color = SleepyCommon.KnownColor.yellow;
                    colorPanel.CenterToParent();
                }

                // ------------------------------------------------------------
                // Outside connections
                m_panelImportExport = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_EXPORTIMPORT_OPTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 70);
                if (m_panelImportExport is not null && m_panelImportExport.m_content is not null)
                {
                    const int checkboxWidth = 400;

                    UIPanel panel = m_panelImportExport.m_content.AddUIComponent<UIPanel>();
                    panel.autoLayout = true;
                    panel.autoLayoutDirection = LayoutDirection.Horizontal;
                    panel.autoLayoutPadding.right = 4;
                    panel.width = m_panelImportExport.m_content.width;
                    
                    m_chkAllowImport = UIMyUtils.AddCheckbox(panel, Localization.Get("chkAllowImport"), UIFonts.Regular, fTEXT_SCALE, true, OnAllowImportChanged);
                    m_chkAllowImport.width = checkboxWidth;
                    panel.height = m_chkAllowImport.height;

                    m_btnExcludeOutsideConnecions = UIMyUtils.AddToggleButton(UIMyUtils.ButtonStyle.DropDown, panel, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS") + "...", "", 260, iButtonHeight, OnSelectExcludeOutsideConnectionsClicked);
                    if (m_btnExcludeOutsideConnecions is not null)
                    {
                        m_btnExcludeOutsideConnecions.onColor = KnownColor.lightBlue;
                        m_btnExcludeOutsideConnecions.offColor = KnownColor.white;
                        m_btnExcludeOutsideConnecions.StateOn = false; // start off
                    }

                    m_btnOutsideClear = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, panel, "Niet", m_btnExcludeOutsideConnecions.height, m_btnExcludeOutsideConnecions.height, OnExcludeOutsideClearClicked);
                    if (m_btnOutsideClear is not null)
                    {
                        m_btnOutsideClear.tooltip = Localization.Get("btnClear");
                    }

                    m_chkAllowExport = UIMyUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowExport"), UIFonts.Regular, fTEXT_SCALE, true, OnAllowExportChanged);
                    m_chkAllowExport.width = checkboxWidth;
                }

                // ------------------------------------------------------------
                // Goods delivery
                m_panelGoodsDelivery = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_GOODS_DELIVERY"), fTEXT_SCALE, m_panelTabPanel.width - 20, 95);
                if (m_panelGoodsDelivery is not null)
                {
                    m_sliderReserveCargoTrucks = SettingsSlider.Create(m_panelGoodsDelivery.m_content, LayoutDirection.Horizontal, Localization.Get("sliderWarehouseReservePercentLocal"), UIFonts.Regular, fTEXT_SCALE, 400, 280, -1f, 100f, 1f, 0f, 0, OnReserveCargoTrucksChanged);
                    m_sliderReserveCargoTrucks.OffValue = -1;
                }

                // ------------------------------------------------------------
                m_otherSettingsPanel = m_pnlMain.AddUIComponent<UIPanel>();
                m_otherSettingsPanel.relativePosition = new Vector2(0, m_tabStripTransferReason.height + 6);
                m_otherSettingsPanel.width = m_tabStripTransferReason.width;
                m_otherSettingsPanel.height = 200; // This gets adjusted at the end
                m_otherSettingsPanel.backgroundSprite = "InfoviewPanel";
                m_otherSettingsPanel.color = new Color32(150, 150, 150, 255);
                m_otherSettingsPanel.autoLayout = true;
                m_otherSettingsPanel.autoLayoutDirection = LayoutDirection.Vertical;
                m_otherSettingsPanel.autoLayoutPadding = new RectOffset(10, 0, 6, 0);
                m_otherSettingsPanel.clipChildren = true;

                m_grpOutsidePriority = UIGroup.AddGroup(m_otherSettingsPanel, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_PRIORITY"), fTEXT_SCALE, m_pnlMain.width - 20, 120);
                if (m_grpOutsidePriority is not null)
                {
                    // Clear the group background
                    //m_grpOutsidePriority.backgroundSprite = "";
                    m_grpOutsidePriority.relativePosition = new Vector2(0, m_tabStripTransferReason.height + m_panelTabPanel.height + 10);

                    // Priority
                    m_sliderOutsideCargoPriority = SettingsSlider.Create(m_grpOutsidePriority.m_content, LayoutDirection.Horizontal, Localization.Get("txtCargoPriority"), UIFonts.Regular, fTEXT_SCALE, 420, 280, -1f, 100, 1f, 0f, 0, OnOutsideCargoPriorityChanged);
                    m_sliderOutsideCargoPriority.OffValue = -1;
                    m_sliderOutsideCargoPriority.Percent = true;

                    m_sliderOutsideCitizenPriority = SettingsSlider.Create(m_grpOutsidePriority.m_content, LayoutDirection.Horizontal, Localization.Get("txtCitizenPriority"), UIFonts.Regular, fTEXT_SCALE, 420, 280, -1f, 100, 1f, 0f, 0, OnOutsideCitizenPriorityChanged);
                    m_sliderOutsideCitizenPriority.OffValue = -1;
                    m_sliderOutsideCitizenPriority.Percent = true;

                    m_grpOutsidePriority.AdjustHeight();
                    m_grpOutsidePriority.isVisible = false; // It seems to not be hidden correctly the first time so hide it here
                }

                // ------------------------------------------------------------
                // Apply to all
                m_applyToAll = UIApplyToAll.Create(tabSettings);

                // Change panel height now we have added buttons
                m_pnlMain.height = tabSettings.height - m_applyToAll.height - 10;
                m_panelTabPanel.height = m_pnlMain.height - m_tabStripTransferReason.height - 10;
            }

            // ----------------------------------------------------------------
            m_tabStripTransferReason.eventTabChanged = OnReasonTabChanged;

            m_bInSetup = false;
        }

        // ----------------------------------------------------------------------------------------
        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(bActive))
            {
                return false;
            }

            if (m_tabStripTransferReason is null)
            {
                return false;
            }

            // Disable notifications while updating tab
            m_tabStripTransferReason.eventTabChanged = null;

            try
            {
                if (!SaveGameSettings.GetSettings().EnableNewTransferManager)
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);
                    return false;
                }

                // Load applicable rule sets for this building
                List<ReasonRule> buildingRules = BuildingRuleSets.GetRules(m_eBuildingType, m_buildingId);

                // Should settings tab be shown
                m_tabStrip.SetTabVisible((int)BuildingPanel.TabIndex.TAB_SETTINGS, buildingRules.Count > 0);

                if (buildingRules.Count > 0 && m_grpDistrictRestrictions is not null)
                {
                    // Dont bother updating till it becomes the active tab
                    if (bActive)
                    {
                        bool bIsWarehouse = IsWarehouse(m_eBuildingType);

                        // Update settings reason tabs
                        UpdateReasonTabVisibility(m_eBuildingType, buildingRules);

                        // check index is in range
                        int iRestrictionTabIndex = RestrictionTabIndex;
                        if (iRestrictionTabIndex < 0 || iRestrictionTabIndex >= buildingRules.Count)
                        {
                            iRestrictionTabIndex = 0;
                        }

                        ReasonRule currentRule = buildingRules[iRestrictionTabIndex];
                        BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
                        RestrictionSettings restrictionSettings = settings.GetRestrictionsOrDefault(currentRule.m_id);

                        // ------------------------------------------------------------------------
                        // District restrictions
                        if (currentRule.m_incomingDistrict || currentRule.m_outgoingDistrict)
                        {
                            m_grpDistrictRestrictions.isVisible = true;

                            // Incoming
                            if (m_incomingDistrictPanel is not null)
                            {
                                m_incomingDistrictPanel.isVisible = currentRule.m_incomingDistrict;
                                m_incomingDistrictPanel.DropDown.selectedIndex = (int)restrictionSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts;
                                m_incomingDistrictPanel.SelectDistrict.isEnabled = (restrictionSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts != DistrictRestrictionSettings.PreferLocal.AllDistricts);
                                m_incomingDistrictPanel.Clear.isEnabled = restrictionSettings.m_incomingDistrictSettings.IsSet();
                            }

                            // Outgoing
                            if (m_outgoingDistrictPanel is not null)
                            {
                                m_outgoingDistrictPanel.isVisible = currentRule.m_outgoingDistrict;
                                m_outgoingDistrictPanel.DropDown.selectedIndex = (int)restrictionSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts;
                                m_outgoingDistrictPanel.SelectDistrict.isEnabled = (restrictionSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts != DistrictRestrictionSettings.PreferLocal.AllDistricts);
                                m_outgoingDistrictPanel.Clear.isEnabled = restrictionSettings.m_outgoingDistrictSettings.IsSet();
                            }
                        }
                        else
                        {
                            m_grpDistrictRestrictions.isVisible = false;
                        }

                        // ------------------------------------------------------------------------
                        // Building restrictions
                        if (currentRule.m_incomingBuilding || currentRule.m_outgoingBuilding)
                        {
                            m_buildingRestrictionGroup.isVisible = true;

                            if (SelectionTool.Exists)
                            {
                                if (m_pnlBuildingRestrictionsIncoming is not null)
                                {
                                    m_pnlBuildingRestrictionsIncoming.UpdatePanel(m_buildingId, currentRule, restrictionSettings);
                                }

                                if (m_pnlBuildingRestrictionsOutgoing is not null)
                                {
                                    m_pnlBuildingRestrictionsOutgoing.UpdatePanel(m_buildingId, currentRule, restrictionSettings);
                                }
                            }
                        }
                        else
                        {
                            m_buildingRestrictionGroup.isVisible = false;
                        }

                        // ------------------------------------------------------------------------
                        // Distance restrictions
                        if (currentRule.m_incomingDistance || currentRule.m_outgoingDistance)
                        {
                            m_grpServiceDistance.Show();
                            m_grpServiceDistance.Text = Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS");

                            m_sliderIncomingServiceDistance.isVisible = currentRule.m_incomingDistance;
                            m_sliderIncomingServiceDistance.Value = restrictionSettings.m_incomingServiceDistanceMeters / 1000.0f;

                            m_sliderOutgoingServiceDistance.isVisible = currentRule.m_outgoingDistance;
                            m_sliderOutgoingServiceDistance.Value = restrictionSettings.m_outgoingServiceDistanceMeters / 1000.0f;

                            // Try and determine current reason and check global distance
                            CustomTransferReason.Reason reason = GetCurrentGlobalDistanceReason(m_eBuildingType, m_buildingId, currentRule);
                            if (reason != CustomTransferReason.Reason.None && TransferManagerModes.IsGlobalDistanceRestrictionsSupported(reason))
                            {
                                m_lblDistanceGlobal.Show();
                                m_lblDistanceGlobal.text = $"{Localization.Get("txtGlobalDistanceRestriction")}: {SaveGameSettings.GetSettings().GetActiveDistanceRestrictionKm(reason).ToString("N1")}";
                                m_globalDistancePanel.Show();
                            }
                            else
                            {
                                m_lblDistanceGlobal.Hide();
                                m_globalDistancePanel.Hide();
                            }
                        }
                        else
                        {
                            m_grpServiceDistance.Hide();
                        }

                        // ------------------------------------------------------------------------
                        // Import / Export
                        if (currentRule.m_import || currentRule.m_export)
                        {
                            m_panelImportExport.Show();

                            m_btnExcludeOutsideConnecions.Show();
                            m_btnOutsideClear.Show();

                            // Disable appropriate import/export option
                            if (bIsWarehouse)
                            {
                                ushort warehouseBuildingId = WarehouseUtils.GetWarehouseBuildingId(m_buildingId);

                                // Disable import / export based on warehouse type as well as rules.
                                switch (WarehouseUtils.GetWarehouseMode(warehouseBuildingId))
                                {
                                    case WarehouseUtils.WarehouseMode.Fill:
                                        {
                                            m_chkAllowImport.isEnabled = currentRule.m_import;
                                            m_chkAllowExport.isEnabled = false; // Fill mode cannot export
                                            break;
                                        }
                                    case WarehouseUtils.WarehouseMode.Empty:
                                        {
                                            m_chkAllowImport.isEnabled = false; // Empty mode cannot import
                                            m_chkAllowExport.isEnabled = currentRule.m_export;
                                            break;
                                        }
                                    default:
                                        {
                                            // Balanced mode can do both
                                            m_chkAllowImport.isEnabled = currentRule.m_import;
                                            m_chkAllowExport.isEnabled = currentRule.m_export;
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                m_chkAllowImport.isEnabled = currentRule.m_import;
                                m_chkAllowExport.isEnabled = currentRule.m_export;
                            }

                            // Import / Export Show outside connections button
                            m_btnExcludeOutsideConnecions.isEnabled = m_chkAllowImport.isEnabled || m_chkAllowExport.isEnabled;
                            m_btnOutsideClear.isEnabled = m_btnExcludeOutsideConnecions.isEnabled;

                            m_chkAllowImport.isChecked = restrictionSettings.m_bAllowImport;
                            m_chkAllowExport.isChecked = restrictionSettings.m_bAllowExport;

                            m_btnExcludeOutsideConnecions.StateOn = OutsideConnectionSelectionPanel.IsVisible();

                            m_btnOutsideClear.isEnabled = restrictionSettings.m_excludedOutsideConnections.Count > 0;
                        }
                        else
                        {
                            m_panelImportExport.Hide();
                        }

                        // ------------------------------------------------------------------------
                        // Warehouse settings
                        if (bIsWarehouse)
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                            if (TransportUtils.GetTransportType(building) == TransportUtils.TransportType.Road)
                            {
                                m_panelGoodsDelivery.Show();

                                // Reserve cargo trucks override
                                m_sliderReserveCargoTrucks.Value = settings.m_iWarehouseReserveTrucksPercent;

                                // Update label
                                int iPercent = settings.ReserveCargoTrucksPercent();
                                WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                                if (warehouse is not null && m_sliderReserveCargoTrucks.m_label is not null)
                                {
                                    int iTrucks = (int)((float)(iPercent * 0.01) * (float)warehouse.m_truckCount);
                                    if (settings.m_iWarehouseReserveTrucksPercent == m_sliderReserveCargoTrucks.OffValue)
                                    {
                                        m_sliderReserveCargoTrucks.m_label.text = $"{Localization.Get("sliderWarehouseReservePercentLocal")}: Off ({iPercent}% | Trucks: {iTrucks})";
                                    }
                                    else
                                    {
                                        m_sliderReserveCargoTrucks.m_label.text = $"{Localization.Get("sliderWarehouseReservePercentLocal")}: {iPercent}% | Trucks: {iTrucks}";
                                    }
                                }
                            }
                            else
                            {
                                m_panelGoodsDelivery.Hide();
                            }
                        }
                        else
                        {
                            m_panelGoodsDelivery.Hide();
                        }

                        // ------------------------------------------------------------------------
                        // Outside connection multiplier
                        if (BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                        {
                            m_grpOutsidePriority.Show();

                            // Import / Export Hide outside connections button
                            m_btnExcludeOutsideConnecions.Hide();
                            m_btnOutsideClear.Hide();

                            // Cargo priority
                            m_sliderOutsideCargoPriority.Value = settings.m_iCargoOutsidePriority;
                            if (m_sliderOutsideCargoPriority.Value == m_sliderOutsideCargoPriority.OffValue)
                            {
                                m_sliderOutsideCargoPriority.Text = $"{Localization.Get("txtCargoPriority")} ({BuildingSettingsFast.GetEffectiveOutsideCargoPriority(m_buildingId)}%)";
                            }
                            else
                            {
                                m_sliderOutsideCargoPriority.Text = $"{Localization.Get("txtCargoPriority")}";
                            }

                            // Citizen priority
                            m_sliderOutsideCitizenPriority.Value = settings.m_iCitizenOutsidePriority;
                            if (m_sliderOutsideCitizenPriority.Value == m_sliderOutsideCitizenPriority.OffValue)
                            {
                                m_sliderOutsideCitizenPriority.Text = $"{Localization.Get("txtCitizenPriority")} ({BuildingSettingsFast.GetEffectiveOutsideCitizenPriority(m_buildingId)}%)";
                            }
                            else
                            {
                                m_sliderOutsideCitizenPriority.Text = $"{Localization.Get("txtCitizenPriority")}";
                            }
                        }
                        else
                        {
                            m_grpOutsidePriority.Hide();
                        }


                        // Apply to all buttons
                        if (m_applyToAll is not null)
                        {
                            m_applyToAll.UpdatePanel();
                        }

                        // Update the panel height depending on building type.
                        if (IsOutsideConnection(m_buildingId))
                        {
                            // Reduce tab panel
                            m_panelTabPanel.height = m_panelImportExport.height + 12;
                            
                            // Show other settings panel
                            m_otherSettingsPanel.isVisible = true;
                            float fTabHeight = m_tabStripTransferReason.height + m_panelTabPanel.height + 10;
                            m_otherSettingsPanel.relativePosition = new Vector2(0, fTabHeight);
                            m_otherSettingsPanel.height = m_pnlMain.height - fTabHeight - 6;
                        }
                        else
                        {
                            m_otherSettingsPanel.isVisible = false;
                            m_panelTabPanel.height = m_pnlMain.height - m_tabStripTransferReason.height - 10;
                        }

                        // Adjust group heights
                        foreach (UIComponent component in m_panelTabPanel.components)
                        {
                            if (component is UIGroup group && component.isVisible)
                            {
                                group.AdjustHeight();
                            }
                        }

                        foreach (UIComponent component in m_otherSettingsPanel.components)
                        {
                            if (component is UIGroup group && component.isVisible)
                            {
                                group.AdjustHeight();
                            }
                        }

                        m_tabStripTransferReason.PerformLayout();

                        // ------------------------------------------------------------------------
                        if (DistrictSelectionPanel.IsVisible())
                        {
                            DistrictSelectionPanel.Instance.SetPanelBuilding(m_buildingId, GetRestrictionId());
                            DistrictSelectionPanel.Instance.InvalidatePanel();
                        }

                        if (OutsideConnectionSelectionPanel.IsVisible())
                        {
                            OutsideConnectionSelectionPanel.Instance.SetPanelBuilding(m_buildingId, GetRestrictionId());
                            OutsideConnectionSelectionPanel.Instance.InvalidatePanel();
                        }

                        UpdateDistrictButtonTooltip(true);
                        UpdateDistrictButtonTooltip(false);
                        UpdateDistrictButtonToggle();
                    }
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)BuildingPanel.TabIndex.TAB_SETTINGS, false);

                    // Hide them all
                    for (int i = 0; i < m_tabStripTransferReason.Count; ++i)
                    {
                        m_tabStripTransferReason.SetTabVisible(i, false);
                    }
                }
            }
            finally
            {
                // Re-enable notifcations now we have finished updating tabs
                m_tabStripTransferReason.eventTabChanged = OnReasonTabChanged;
            }

            return true;
        }

        public CustomTransferReason.Reason GetCurrentGlobalDistanceReason(BuildingType buildingType, ushort buildingId, ReasonRule rule)
        {
            // Try and determine current reason and check global distance
            CustomTransferReason.Reason reason = CustomTransferReason.Reason.None;

            if ((rule.m_incomingDistance || rule.m_outgoingDistance) && rule.m_reasons.Count > 0)
            {
                if (rule.m_reasons.Count == 1)
                {
                    reason = rule.m_reasons.Single();
                }
                else if (IsWarehouse(m_eBuildingType))
                {
                    ushort warehouseId = WarehouseUtils.GetWarehouseBuildingId(buildingId);
                    reason = (CustomTransferReason.Reason)GetWarehouseActualTransferReason(warehouseId);
                }
            }

            return reason;
        }

        public void UpdateReasonTabVisibility(BuildingType eType, List<ReasonRule> buildingRules)
        {
            while (m_tabStripTransferReason.Count < buildingRules.Count)
            {
                m_tabStripTransferReason.AddTab("Tab", 200f);
            }

            // Hide them all
            for (int i = 0; i < m_tabStripTransferReason.Count; ++i)
            {
                m_tabStripTransferReason.SetTabVisible(i, false);
            }

            // Set tab names and show them
            int iTabIndex = 0;
            foreach (var rule in buildingRules)
            {
                bool bShowTabForRule = true;

                // Add 
                switch (eType)
                {
                    case BuildingType.CargoWarehouse:
                    case BuildingType.Warehouse:
                    case BuildingType.CargoFerryWarehouseHarbor:
                        {
                            ushort warehouseBuildingId = WarehouseUtils.GetWarehouseBuildingId(m_buildingId);

                            string sName = string.Empty;
                            string sWarehouseMode = WarehouseUtils.GetLocalisedWarehouseMode(WarehouseUtils.GetWarehouseMode(warehouseBuildingId));

                            // For warehouses we add the actual material to the tab name.
                            CustomTransferReason.Reason actualTransferReason = BuildingTypeHelper.GetWarehouseActualTransferReason(warehouseBuildingId);
                            if (actualTransferReason != CustomTransferReason.Reason.None && rule.m_reasons.Contains(actualTransferReason))
                            {
                                sName += $"{actualTransferReason} | {sWarehouseMode}";
                            }
                            else
                            {
                                sName += $"{sWarehouseMode}";
                            }

                            m_tabStripTransferReason.SetTabText(iTabIndex, sName);
                            m_tabStripTransferReason.SetTabWidth(iTabIndex, 300f);

                            if (rule.m_reasons.Contains(actualTransferReason))
                            {
                                m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(actualTransferReason));
                            }
                            break;
                        }
                    case BuildingType.GenericExtractor:
                    case BuildingType.ExtractionFacility:
                        {
                            switch (rule.m_id)
                            {
                                case 0:
                                    {
                                        // Outgoing
                                        CustomTransferReason.Reason reason = BuildingTypeHelper.GetOutgoingTransferReason(m_buildingId);
                                        if (reason != CustomTransferReason.Reason.None)
                                        {
                                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(reason));
                                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(reason));
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case BuildingType.UniqueFactory:
                    case BuildingType.ProcessingFacility:
                        {
                            // Handle Industries Remastered assets
                            switch (rule.m_id)
                            {
                                case 0:
                                    {
                                        // Incoming
                                        HashSet<CustomTransferReason.Reason> reasons = BuildingTypeHelper.GetIncomingTransferReasons(m_buildingId);
                                        reasons.IntersectWith(rule.m_reasons);
                                        if (reasons.Count > 0)
                                        {
                                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(reasons));
                                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(reasons));
                                        }
                                        break;
                                    }
                                case 1:
                                    {
                                        // Outgoing
                                        CustomTransferReason.Reason reason = BuildingTypeHelper.GetOutgoingTransferReason(m_buildingId);
                                        if (rule.m_reasons.Contains(reason))
                                        {
                                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(reason));
                                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(reason));
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case BuildingType.GenericProcessing:
                    case BuildingType.GenericFactory:
                        {
                            // Handle Industries Remastered assets
                            switch (rule.m_id)
                            {
                                case 0: // Incoming 1
                                    {
                                        CustomTransferReason.Reason reason = BuildingTypeHelper.GetPrimaryIncomingTransferReason(m_buildingId);
                                        if (reason != CustomTransferReason.Reason.None)
                                        {
                                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(reason));
                                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(reason));
                                        }
                                        break;
                                    }
                                case 2: // Incoming 2
                                    {
                                        CustomTransferReason.Reason reason = BuildingTypeHelper.GetSecondaryIncomingTransferReason(m_buildingId);
                                        if (reason != CustomTransferReason.Reason.None)
                                        {
                                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(reason));
                                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(reason));
                                        }
                                        break;
                                    }
                                case 1: // Outgoing
                                    {
                                        CustomTransferReason.Reason reason = BuildingTypeHelper.GetOutgoingTransferReason(m_buildingId);
                                        if (rule.m_reasons.Contains(reason))
                                        {
                                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(reason));
                                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(reason));
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(rule.m_reasons));
                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(rule.m_reasons));
                            break;
                        }
                }

                m_tabStripTransferReason.SetTabVisible(iTabIndex, bShowTabForRule);
                m_tabStripTransferReason.SetTabId(iTabIndex, rule.m_id);
                iTabIndex++;
            }

            // Update selected tab
            if (0 <= RestrictionTabIndex && RestrictionTabIndex < m_tabStripTransferReason.Count)
            {
                m_tabStripTransferReason.SelectTabIndex(RestrictionTabIndex);
            }
            else if (m_tabStripTransferReason.Count > 0)
            {
                RestrictionTabIndex = 0;
                m_tabStripTransferReason.SelectTabIndex(RestrictionTabIndex);
            }
        }

        private string GetRestrictionReasons(CustomTransferReason.Reason reason)
        {
            string sTooltip = "Restriction Reasons:";
            sTooltip += $"\r\n- {reason}";
            return sTooltip;
        }

        private string GetRestrictionReasons(HashSet<CustomTransferReason.Reason> reasons)
        {
            string sTooltip = "Restriction Reasons:";

            foreach (CustomTransferReason.Reason reason in reasons)
            {
                sTooltip += $"\r\n- {reason}";
            }

            return sTooltip;
        }

        private void OnReasonTabChanged(int index)
        {
            if (m_bInSetup) { return; }

            RestrictionTabIndex = index;

            BuildingPanel.Instance.HideSecondaryPanels();

            // Turn off building selection mode
            if (SelectionTool.Exists)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
            }

            UpdateTab(true);
        }

        // ----------------------------------------------------------------------------------------
        public void OnPreferLocalServices(bool bIncoming, int Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            if (bIncoming)
            {
                restrictions.m_incomingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)Value;
            }
            else
            {
                restrictions.m_outgoingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)Value;
            }

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(true);
        }

        private void OnSelectDistrictClicked(bool bIncoming)
        {
            if (m_bInSetup) { return; }

            if (DistrictSelectionPanel.IsVisible())
            {
                if (DistrictSelectionPanel.Instance.IsIncoming() != bIncoming)
                {
                    DistrictSelectionPanel.Instance.Create(BuildingPanel.Instance);
                    DistrictSelectionPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId(), bIncoming);
                }
                else
                {
                    DistrictSelectionPanel.Instance.Hide();
                }
            }
            else
            {
                DistrictSelectionPanel.Instance.Create(BuildingPanel.Instance);
                DistrictSelectionPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId(), bIncoming);
            }

            UpdateDistrictButtonToggle();
        }

        public void UpdateDistrictButtonTooltip(bool bIncoming)
        {
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            if (bIncoming && m_incomingDistrictPanel is not null)
            {
                m_incomingDistrictPanel.SelectDistrict.tooltip = restrictions.m_incomingDistrictSettings.GetTooltip(m_buildingId);
            }
            if (!bIncoming && m_outgoingDistrictPanel is not null)
            {
                m_outgoingDistrictPanel.SelectDistrict.tooltip = restrictions.m_outgoingDistrictSettings.GetTooltip(m_buildingId);
            }
        }

        public void UpdateDistrictButtonToggle()
        {
            if (m_incomingDistrictPanel is not null && m_incomingDistrictPanel.isVisible)
            {
                m_incomingDistrictPanel.SetButtonState(DistrictSelectionPanel.IsVisible() && DistrictSelectionPanel.Instance.IsIncoming());
            }
            if (m_outgoingDistrictPanel is not null && m_outgoingDistrictPanel.isVisible)
            {
                m_outgoingDistrictPanel.SetButtonState(DistrictSelectionPanel.IsVisible() && !DistrictSelectionPanel.Instance.IsIncoming());
            }
        }

        private void OnDistrictRestrictionsClearClicked(bool bIncoming)
        {
            if (m_bInSetup) { return; }

            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            if (settings is not null)
            {
                RestrictionSettings? restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());
                if (restrictions is not null)
                {
                    // Clear district settings
                    if (bIncoming)
                    {
                        restrictions.m_incomingDistrictSettings.Reset();
                    }
                    else
                    {
                        restrictions.m_outgoingDistrictSettings.Reset();
                    }

                    settings.SetRestrictions(GetRestrictionId(), restrictions);
                    BuildingSettingsStorage.SetSettings(m_buildingId, settings);

                    UpdateTab(true);

                    if (DistrictSelectionPanel.IsVisible())
                    {
                        DistrictSelectionPanel.Instance.Hide();
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        public void OnServiceDistanceChanged(bool bIncoming, float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            if (bIncoming)
            {
                restrictions.m_incomingServiceDistanceMeters = (int)(Value * 1000.0f); // eg 3.5km = 3500
            }
            else
            {
                restrictions.m_outgoingServiceDistanceMeters = (int)(Value * 1000.0f); // eg 3.5km = 3500
            }

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(true);
        }

        public void OnOutsideCargoPriorityChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_iCargoOutsidePriority = (int)Value;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update outside connection panel with this new value
            if (OutsideConnectionPanel.IsVisible())
            {
                OutsideConnectionPanel.Instance.InvalidatePanel();
            }

            UpdateTab(true);
        }

        public void OnOutsideCitizenPriorityChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_iCitizenOutsidePriority = (int)Value;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update outside connection panel with this new value
            if (OutsideConnectionPanel.IsVisible())
            {
                OutsideConnectionPanel.Instance.InvalidatePanel();
            }

            UpdateTab(true);
        }

        public void OnAllowImportChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_bAllowImport = bChecked;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.IsVisible())
            {
                OutsideConnectionPanel.Instance.InvalidatePanel();
            }

            UpdateTab(true);
        }

        public void OnAllowExportChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_bAllowExport = bChecked;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.IsVisible())
            {
                OutsideConnectionPanel.Instance.InvalidatePanel();
            }

            UpdateTab(true);
        }

        public void OnSelectExcludeOutsideConnectionsClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_btnExcludeOutsideConnecions.Toggle();

            if (OutsideConnectionSelectionPanel.IsVisible())
            {
                OutsideConnectionSelectionPanel.Instance.Hide();
            }
            else 
            {
                OutsideConnectionSelectionPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId());
            }

            UpdateTab(true);
        }

        public void OnExcludeOutsideClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_excludedOutsideConnections.Clear();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.IsVisible())
            {
                OutsideConnectionPanel.Instance.InvalidatePanel();
            }

            UpdateTab(true);
        }

        public void OnReserveCargoTrucksChanged(float fPercent)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_iWarehouseReserveTrucksPercent = (int)fPercent;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(true);
        }

        public void OnBuildingRestrictionsClicked(bool bIncoming)
        {
            if (SelectionTool.Exists)
            {
                if (bIncoming)
                {
                    if (SelectionTool.Instance.GetNewMode() == SelectionTool.SelectionToolMode.BuildingRestrictionIncoming)
                    {
                        SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                    }
                    else
                    {
                        SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.BuildingRestrictionIncoming);
                    }
                }
                else
                {
                    if (SelectionTool.Instance.GetNewMode() == SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing)
                    {
                        SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                    }
                    else
                    {
                        SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing);
                    }
                }

                BuildingPanel.Instance.HideSecondaryPanels();
                UpdateTab(true);
            }
        }

        private void OnBuildingRestrictionsClearClicked(bool bIncoming)
        {
            if (m_bInSetup) { return; }

            SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            if (bIncoming)
            {
                restrictions.m_incomingBuildingSettings.ClearBuildingRestrictions();
            }
            else
            {
                restrictions.m_outgoingBuildingSettings.ClearBuildingRestrictions();
            }
                
            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(true);
        }

        new public void Destroy()
        {
            if (m_incomingDistrictPanel is not null)
            {
                m_incomingDistrictPanel.Destroy();
                m_incomingDistrictPanel = null;
            }
            if (m_outgoingDistrictPanel is not null)
            {
                m_outgoingDistrictPanel.Destroy();
                m_outgoingDistrictPanel = null;
            }
            if (m_panelImportExport is not null)
            {
                UnityEngine.Object.Destroy(m_panelImportExport.gameObject);
                m_panelImportExport = null;
            }
            if (m_chkAllowImport is not null)
            {
                UnityEngine.Object.Destroy(m_chkAllowImport.gameObject);
                m_chkAllowImport = null;
            }
            if (m_chkAllowExport is not null)
            {
                UnityEngine.Object.Destroy(m_chkAllowExport.gameObject);
                m_chkAllowExport = null;
            }
            if (m_panelGoodsDelivery is not null)
            {
                UnityEngine.Object.Destroy(m_panelGoodsDelivery.gameObject);
                m_panelGoodsDelivery = null;
            }
            if (m_sliderReserveCargoTrucks is not null)
            {
                m_sliderReserveCargoTrucks.Destroy();
                m_sliderReserveCargoTrucks = null;
            }
            if (m_grpDistrictRestrictions is not null)
            {
                UnityEngine.Object.Destroy(m_grpDistrictRestrictions.gameObject);
                m_grpDistrictRestrictions = null;
            }

            base.Destroy();
        }

        private float GetTabWidth(CustomTransferReason.Reason reason)
        {
            switch (reason)
            {
                case CustomTransferReason.Reason.Mail: return 120f;
                case CustomTransferReason.Reason.Mail2: return 140f;
                default: return 200f;
            }
        }

        private float GetTabWidth(HashSet<CustomTransferReason.Reason> reasons)
        {
            if (reasons.Count == 1)
            {
                return GetTabWidth(reasons.Single());
            }

            return 200f;
        }
    }
}
