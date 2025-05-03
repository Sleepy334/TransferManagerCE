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
using AlgernonCommons.UI;

namespace TransferManagerCE.UI
{
    public class BuildingSettingsTab
    {
        const float fTEXT_SCALE = 0.9f;

        // Settings tab
        private UITabStrip? m_tabStripTransferReason = null;
        private UIPanel? m_pnlMain = null;
        private UIScrollablePanel? m_panelTabPanel = null;

        private UIGroup? m_grpDistrictRestrictions = null;
        private UIPanel? m_panelIncomingDistrict = null;
        private UIMyDropDown? m_dropPreferLocalIncoming = null;
        private UIButton? m_btnIncomingSelectDistrict = null;
        private UIButton? m_btnDistrictRestrictionsIncomingClear = null;
        private UIPanel? m_panelOutgoingDistrict = null;
        private UIMyDropDown? m_dropPreferLocalOutgoing = null;
        private UIButton? m_btnOutgoingSelectDistrict = null;
        private UIButton? m_btnDistrictRestrictionsOutgoingClear = null;

        // Distance restriction
        private UIGroup? m_grpServiceDistance = null;
        private SettingsSlider? m_sliderServiceDistance = null;
        private UILabel? m_lblDistanceGlobal = null;

        private UIGroup? m_panelImportExport = null;
        private UICheckBox? m_chkAllowImport = null;
        private UICheckBox? m_chkAllowExport = null;

        private UIGroup? m_grpOutsideDistanceMultiplier = null;
        private SettingsSlider? m_sliderOutsideDistanceMultiplier = null;

        // Warehouse options
        private UIGroup? m_panelGoodsDelivery = null;
        private UICheckBox? m_chkWarehouseOverride = null;
        private UICheckBox? m_chkImprovedWarehouseMatching = null;
        private SettingsSlider? m_sliderReserveCargoTrucks = null;

        private UIGroup? m_buildingRestrictionGroup = null;
        private UIPanel? m_pnlBuildingRestrictionsIncoming = null;
        private UILabel? m_lblBuildingRestrictionsIncoming = null;
        private UIButton? m_btnBuildingRestrictionsIncoming = null;
        private UIButton? m_btnBuildingRestrictionsIncomingClear = null;
        private UIPanel? m_pnlBuildingRestrictionsOutgoing = null;
        private UILabel? m_lblBuildingRestrictionsOutgoing = null;
        private UIButton? m_btnBuildingRestrictionsOutgoing = null;
        private UIButton? m_btnBuildingRestrictionsOutgoingClear = null;

        // Apply to all
        private UIApplyToAll? m_applyToAll = null;

        private ushort m_buildingId;
        private int m_iRestrictionTabIndex = 0;

        private UITabStrip? m_tabStrip;
        private bool m_bInSetup = false;
        private bool m_bScrollbarVisibleLayoutState = false;

        public BuildingSettingsTab()
        {
            m_buildingId = 0;
            m_tabStrip = null;
        }

        public void SetTabBuilding(ushort buildingId)
        {
            if (buildingId != m_buildingId)
            {
                m_buildingId = buildingId;
                m_iRestrictionTabIndex = 0;
            }

            if (DistrictSelectionPanel.Instance is not null && DistrictSelectionPanel.Instance.isVisible)
            {
                DistrictSelectionPanel.Instance.Hide();
            }
            
            UpdateTab(m_tabStrip);
        }

        public int GetRestrictionTabIndex()
        {
            return m_iRestrictionTabIndex;
        }

        public int GetRestrictionId()
        {
            if (m_tabStripTransferReason is not null && m_tabStripTransferReason.Count > GetRestrictionTabIndex())
            {
                return m_tabStripTransferReason.GetTabId(GetRestrictionTabIndex());                
            }
            return 0;
        }

        public void Setup(UITabStrip tabStrip)
        {
            m_bInSetup = true;
            m_tabStrip = tabStrip;

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
                m_tabStripTransferReason = UITabStrip.Create(TabStyle.SubBar, m_pnlMain, tabSettings.width - 20f, 18f, OnReasonTabChanged);
                m_tabStripTransferReason.relativePosition = new Vector2(0, 0);
                //m_tabStripTransferReason.backgroundSprite = "InfoviewPanel";
                //m_tabStripTransferReason.color = new Color32(0, 150, 0, 255);

                m_panelTabPanel = m_pnlMain.AddUIComponent<UIScrollablePanel>();
                m_panelTabPanel.relativePosition = new Vector2(0, m_tabStripTransferReason.height + 6);
                m_panelTabPanel.width = m_tabStripTransferReason.width;
                m_panelTabPanel.height = 400; // This gets adjusted at the end
                m_panelTabPanel.backgroundSprite = "InfoviewPanel";
                m_panelTabPanel.color = new Color32(150, 150, 150, 255);
                m_panelTabPanel.autoLayout = true;
                m_panelTabPanel.autoLayoutDirection = LayoutDirection.Vertical;
                m_panelTabPanel.autoLayoutPadding = new RectOffset(10, 0, 6, 0);
                m_panelTabPanel.clipChildren = true;
                m_panelTabPanel.scrollWheelDirection = UIOrientation.Vertical;
                //m_panelTabPanel.backgroundSprite = "InfoviewPanel";
                //m_panelTabPanel.color = new Color32(0, 0, 150, 255);

                // Add scroll bar
                UIScrollbars.AddScrollbar(m_pnlMain, m_panelTabPanel);

                m_panelTabPanel.eventSizeChanged += (c, value) =>
                {
                    m_panelTabPanel.scrollPadding.top = 1;
                };

                // Prefer local services
                m_grpDistrictRestrictions = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_DISTRICT"), fTEXT_SCALE, m_panelTabPanel.width - 20, 140);
                if (m_grpDistrictRestrictions is not null)
                {
                    string[] itemsPreferLocal = {
                        Localization.Get("dropdownBuildingPanelPreferLocal1"),
                        Localization.Get("dropdownBuildingPanelPreferLocal2"),
                        Localization.Get("dropdownBuildingPanelPreferLocal3"),
                        Localization.Get("dropdownBuildingPanelPreferLocal4"),
                    };

                    const int iDropDownWidth = 300;
                    const int iDropDownWidthOffset = 226;

                    // Incoming restrictions
                    m_panelIncomingDistrict = m_grpDistrictRestrictions.m_content.AddUIComponent<UIPanel>();
                    m_panelIncomingDistrict.width = m_grpDistrictRestrictions.width;
                    m_panelIncomingDistrict.height = 35;
                    m_dropPreferLocalIncoming = UIMyDropDown.Create(m_panelIncomingDistrict, Localization.Get("dropdownBuildingPanelIncomingPreferLocalLabel"), fTEXT_SCALE, itemsPreferLocal, OnIncomingPreferLocalServices, 0, iDropDownWidth);
                    if (m_dropPreferLocalIncoming is not null)
                    {
                        m_dropPreferLocalIncoming.m_panel.relativePosition = new Vector3(0, 0);
                        m_dropPreferLocalIncoming.SetPanelWidth(m_panelIncomingDistrict.width - iDropDownWidthOffset);
                        m_dropPreferLocalIncoming.m_dropdown.textScale = 0.9f;
                    }
                    m_btnIncomingSelectDistrict = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, m_panelIncomingDistrict, Localization.Get("btnDistricts") + "...", "", 120, m_dropPreferLocalIncoming.m_dropdown.height, (c, e) => OnSelectIncomingDistrictClicked());
                    if (m_btnIncomingSelectDistrict is not null)
                    {
                        m_btnIncomingSelectDistrict.relativePosition = new Vector3(m_dropPreferLocalIncoming.m_panel.width + 6, 2);
                        m_btnIncomingSelectDistrict.eventTooltipEnter += (c, e) => UpdateDistrictButtonTooltips();
                    }

                    m_btnDistrictRestrictionsIncomingClear = UIUtils.AddSpriteButton(UIUtils.ButtonStyle.DropDown, m_panelIncomingDistrict, "Niet", m_dropPreferLocalIncoming.m_dropdown.height, m_dropPreferLocalIncoming.m_dropdown.height, OnDistrictRestrictionsIncomingClearClicked);
                    if (m_btnDistrictRestrictionsIncomingClear is not null)
                    {
                        m_btnDistrictRestrictionsIncomingClear.relativePosition = new Vector3(m_dropPreferLocalIncoming.m_panel.width + m_btnIncomingSelectDistrict.width + 12, 2);
                        m_btnDistrictRestrictionsIncomingClear.tooltip = Localization.Get("btnClear");
                    }

                    // Outgoing restrictions
                    m_panelOutgoingDistrict = m_grpDistrictRestrictions.m_content.AddUIComponent<UIPanel>();
                    m_panelOutgoingDistrict.width = m_grpDistrictRestrictions.width;
                    m_panelOutgoingDistrict.height = 35;
                    m_dropPreferLocalOutgoing = UIMyDropDown.Create(m_panelOutgoingDistrict, Localization.Get("dropdownBuildingPanelOutgoingPreferLocalLabel"), fTEXT_SCALE, itemsPreferLocal, OnOutgoingPreferLocalServices, 0, iDropDownWidth);
                    if (m_dropPreferLocalOutgoing is not null)
                    {
                        m_dropPreferLocalOutgoing.m_panel.relativePosition = new Vector3(0, 0);
                        m_dropPreferLocalOutgoing.SetPanelWidth(m_panelOutgoingDistrict.width - iDropDownWidthOffset);
                        m_dropPreferLocalOutgoing.m_dropdown.textScale = 0.9f;
                    }
                    m_btnOutgoingSelectDistrict = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, m_panelOutgoingDistrict, Localization.Get("btnDistricts") + "...", "", 120, m_dropPreferLocalOutgoing.m_dropdown.height, (c, e) => OnSelectOutgoingDistrictClicked());
                    if (m_btnOutgoingSelectDistrict is not null)
                    {
                        m_btnOutgoingSelectDistrict.relativePosition = new Vector3(m_dropPreferLocalOutgoing.m_panel.width + 6, 2);
                        m_btnOutgoingSelectDistrict.eventTooltipEnter += (c, e) => UpdateDistrictButtonTooltips();
                    }

                    m_btnDistrictRestrictionsOutgoingClear = UIUtils.AddSpriteButton(UIUtils.ButtonStyle.DropDown, m_panelOutgoingDistrict, "Niet", m_dropPreferLocalOutgoing.m_dropdown.height, m_dropPreferLocalOutgoing.m_dropdown.height, OnDistrictRestrictionsOutgoingClearClicked);
                    if (m_btnDistrictRestrictionsOutgoingClear is not null)
                    {
                        m_btnDistrictRestrictionsOutgoingClear.relativePosition = new Vector3(m_dropPreferLocalOutgoing.m_panel.width + m_btnOutgoingSelectDistrict.width + 12, 2);
                        m_btnDistrictRestrictionsOutgoingClear.tooltip = Localization.Get("btnClear");
                    }
                }

                // Building restrictions
                m_buildingRestrictionGroup = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_BUILDING_RESTRICTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 100);
                if (m_buildingRestrictionGroup is not null)
                {
                    const int iButtonHeight = 28;

                    // Incoming
                    m_pnlBuildingRestrictionsIncoming = m_buildingRestrictionGroup.m_content.AddUIComponent<UIPanel>();
                    m_pnlBuildingRestrictionsIncoming.width = m_buildingRestrictionGroup.width;
                    m_pnlBuildingRestrictionsIncoming.height = 35;
                    m_pnlBuildingRestrictionsIncoming.autoLayout = true;
                    m_pnlBuildingRestrictionsIncoming.autoLayoutDirection = LayoutDirection.Horizontal;
                    m_pnlBuildingRestrictionsIncoming.autoLayoutPadding = new RectOffset(4, 4, 4, 4);

                    // Label
                    m_lblBuildingRestrictionsIncoming = m_pnlBuildingRestrictionsIncoming.AddUIComponent<UILabel>();
                    m_lblBuildingRestrictionsIncoming.text = Localization.Get("txtBuildingRestrictionsIncoming");
                    m_lblBuildingRestrictionsIncoming.font = UIFonts.Regular;
                    m_lblBuildingRestrictionsIncoming.textScale = fTEXT_SCALE;
                    m_lblBuildingRestrictionsIncoming.autoSize = false;
                    m_lblBuildingRestrictionsIncoming.height = 30;
                    m_lblBuildingRestrictionsIncoming.width = 370;
                    m_lblBuildingRestrictionsIncoming.verticalAlignment = UIVerticalAlignment.Middle;

                    // Buttons
                    m_btnBuildingRestrictionsIncoming = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, m_pnlBuildingRestrictionsIncoming, Localization.Get("btnBuildingRestrictions"), "", 280, iButtonHeight, OnBuildingRestrictionsIncomingClicked);
                    m_btnBuildingRestrictionsIncomingClear = UIUtils.AddSpriteButton(UIUtils.ButtonStyle.DropDown, m_pnlBuildingRestrictionsIncoming, "Niet", iButtonHeight, iButtonHeight, OnBuildingRestrictionsIncomingClearClicked);
                    m_btnBuildingRestrictionsIncomingClear.tooltip = Localization.Get("btnClear");

                    // Outgoing
                    m_pnlBuildingRestrictionsOutgoing = m_buildingRestrictionGroup.m_content.AddUIComponent<UIPanel>();
                    m_pnlBuildingRestrictionsOutgoing.width = m_buildingRestrictionGroup.width;
                    m_pnlBuildingRestrictionsOutgoing.height = 35;
                    m_pnlBuildingRestrictionsOutgoing.autoLayout = true;
                    m_pnlBuildingRestrictionsOutgoing.autoLayoutDirection = LayoutDirection.Horizontal;
                    m_pnlBuildingRestrictionsOutgoing.autoLayoutPadding = new RectOffset(4, 4, 4, 4);

                    // Label
                    m_lblBuildingRestrictionsOutgoing = m_pnlBuildingRestrictionsOutgoing.AddUIComponent<UILabel>();
                    m_lblBuildingRestrictionsOutgoing.text = Localization.Get("txtBuildingRestrictionsOutgoing");
                    m_lblBuildingRestrictionsOutgoing.textScale = fTEXT_SCALE;
                    m_lblBuildingRestrictionsOutgoing.font = UIFonts.Regular;
                    m_lblBuildingRestrictionsOutgoing.autoSize = false;
                    m_lblBuildingRestrictionsOutgoing.height = m_lblBuildingRestrictionsIncoming.height;
                    m_lblBuildingRestrictionsOutgoing.width = m_lblBuildingRestrictionsIncoming.width;
                    m_lblBuildingRestrictionsOutgoing.verticalAlignment = UIVerticalAlignment.Middle;

                    // Buttons
                    m_btnBuildingRestrictionsOutgoing = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, m_pnlBuildingRestrictionsOutgoing, Localization.Get("btnBuildingRestrictions"), "", 280, iButtonHeight, OnBuildingRestrictionsOutgoingClicked);
                    m_btnBuildingRestrictionsOutgoingClear = UIUtils.AddSpriteButton(UIUtils.ButtonStyle.DropDown, m_pnlBuildingRestrictionsOutgoing, "Niet", iButtonHeight, iButtonHeight, OnBuildingRestrictionsOutgoingClearClicked);
                    m_btnBuildingRestrictionsOutgoingClear.tooltip = Localization.Get("btnClear");
                }

                // Service distance
                m_grpServiceDistance = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 80);
                if (m_grpServiceDistance is not null)
                {
                    m_sliderServiceDistance = SettingsSlider.Create(m_grpServiceDistance.m_content, LayoutDirection.Horizontal, Localization.Get("sliderDistanceRestriction"), UIFonts.Regular, fTEXT_SCALE, 400, 280, 0f, 20f, 0.5f, 0f, 1, OnServiceDistanceChanged);
                    m_sliderServiceDistance.SetTooltip(Localization.Get("sliderDistanceRestrictionTooltip"));

                    m_lblDistanceGlobal = m_grpServiceDistance.m_content.AddUIComponent<UILabel>();
                    m_lblDistanceGlobal.text = Localization.Get("txtGlobalDistanceRestriction");
                    m_lblDistanceGlobal.textScale = fTEXT_SCALE;
                    m_lblDistanceGlobal.font = UIFonts.Regular;
                    m_lblDistanceGlobal.autoSize = false;// true;
                    m_lblDistanceGlobal.height = m_lblBuildingRestrictionsIncoming.height;
                    m_lblDistanceGlobal.width = m_grpServiceDistance.m_content.width;
                    m_lblDistanceGlobal.verticalAlignment = UIVerticalAlignment.Middle;
                }

                // Outside connections
                m_panelImportExport = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 70);
                if (m_panelImportExport is not null && m_panelImportExport.m_content is not null)
                {
                    m_chkAllowImport = UIUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowImport"), UIFonts.Regular, fTEXT_SCALE, true, OnAllowImportChanged);
                    m_chkAllowExport = UIUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowExport"), UIFonts.Regular, fTEXT_SCALE, true, OnAllowExportChanged);
                }

                // Good delivery
                m_panelGoodsDelivery = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_GOODS_DELIVERY"), fTEXT_SCALE, m_panelTabPanel.width - 20, 95);
                if (m_panelGoodsDelivery is not null)
                {
                    m_chkWarehouseOverride = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionWarehouseOverride"), UIFonts.Regular, fTEXT_SCALE, false, OnWarehouseOverrideChanged);
                    m_chkImprovedWarehouseMatching = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionImprovedWarehouseTransfer"), UIFonts.Regular, fTEXT_SCALE, false, OnImprovedWarehouseMatchingChanged);
                    m_sliderReserveCargoTrucks = SettingsSlider.Create(m_panelGoodsDelivery.m_content, LayoutDirection.Horizontal, Localization.Get("sliderWarehouseReservePercentLocal"), UIFonts.Regular, fTEXT_SCALE, 400, 280, 0f, 100f, 1f, 0f, 0, OnReserveCargoTrucksChanged);
                }

                m_grpOutsideDistanceMultiplier = UIGroup.AddGroup(m_pnlMain, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_DISTANCE_MULTIPLIER"), fTEXT_SCALE, m_pnlMain.width - 20, 60);
                if (m_grpOutsideDistanceMultiplier is not null)
                {
                    // Clear the group background
                    m_grpOutsideDistanceMultiplier.backgroundSprite = "";
                    m_grpOutsideDistanceMultiplier.relativePosition = new Vector2(0, m_tabStripTransferReason.height + m_panelTabPanel.height + 12);
                    m_sliderOutsideDistanceMultiplier = SettingsSlider.Create(m_grpOutsideDistanceMultiplier.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideDistanceMultiplier"), UIFonts.Regular, fTEXT_SCALE, 420, 280, 0f, 10, 1f, 0f, 0, OnOutsideDistanceMultiplierChanged);
                    m_sliderOutsideDistanceMultiplier.SetTooltip(Localization.Get("sliderOutsideDistanceMultiplierTooltip"));
                }

                // Apply to all
                m_applyToAll = UIApplyToAll.Create(tabSettings);

                // Change panel height now we have added buttons
                m_pnlMain.height = tabSettings.height - m_applyToAll.height - 10;
                m_panelTabPanel.height = m_pnlMain.height - m_tabStripTransferReason.height - 10;
            }

            m_bInSetup = false;
        }

        public void UpdateTab(UITabStrip tabStrip)
        {
            if (m_tabStrip is null || m_tabStripTransferReason is null || m_bInSetup)
            {
                return;
            }

            if (!SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);
                return;
            }

            // Load applicable rule sets for this building
            BuildingType eType = GetBuildingType(m_buildingId);
            List<ReasonRule> buildingRules = BuildingRuleSets.GetRules(eType, m_buildingId);

            // Should settings tab be shown
            m_tabStrip.SetTabVisible((int)BuildingPanel.TabIndex.TAB_SETTINGS, buildingRules.Count > 0);

            bool bActive = (TabIndex) tabStrip.GetSelectTabIndex() == TabIndex.TAB_SETTINGS;
            if (bActive)
            {
                // Check one of the groups is not null and assume the rest
                if (m_grpDistrictRestrictions is null)
                {
                    return;
                }

                m_bInSetup = true;

                // Load applicable rule sets for this building
                if (buildingRules.Count > 0)
                {
                    // Update settings reason tabs
                    UpdateReasonTabVisibility(eType, buildingRules);

                    // Update settings
                    ReasonRule currentRule = buildingRules[GetRestrictionTabIndex()];
                    BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
                    RestrictionSettings restrictionSettings = settings.GetRestrictionsOrDefault(currentRule.m_id);

                    // District restrictions
                    if (currentRule.m_incomingDistrict || currentRule.m_outgoingDistrict)
                    {
                        m_grpDistrictRestrictions.isVisible = true;

                        // Incoming
                        if (m_panelIncomingDistrict is not null)
                        {
                            m_panelIncomingDistrict.isVisible = currentRule.m_incomingDistrict;
                            m_dropPreferLocalIncoming.isVisible = currentRule.m_incomingDistrict;
                            m_dropPreferLocalIncoming.selectedIndex = (int)restrictionSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts;
                            m_btnIncomingSelectDistrict.isVisible = currentRule.m_incomingDistrict;
                            m_btnIncomingSelectDistrict.isEnabled = (restrictionSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts != DistrictRestrictionSettings.PreferLocal.AllDistricts);
                            m_btnDistrictRestrictionsIncomingClear.isEnabled = restrictionSettings.m_incomingDistrictSettings.IsSet();
                        }

                        // Outgoing
                        if (m_panelOutgoingDistrict is not null)
                        {
                            m_panelOutgoingDistrict.isVisible = currentRule.m_outgoingDistrict;
                            m_dropPreferLocalOutgoing.isVisible = currentRule.m_outgoingDistrict;
                            m_dropPreferLocalOutgoing.selectedIndex = (int)restrictionSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts;
                            m_btnOutgoingSelectDistrict.isVisible = currentRule.m_outgoingDistrict;
                            m_btnOutgoingSelectDistrict.isEnabled = (restrictionSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts != DistrictRestrictionSettings.PreferLocal.AllDistricts);
                            m_btnDistrictRestrictionsOutgoingClear.isEnabled = restrictionSettings.m_outgoingDistrictSettings.IsSet();
                        }

                        if (currentRule.m_incomingDistrict && currentRule.m_outgoingDistrict)
                        {
                            m_grpDistrictRestrictions.height = 100;
                        }
                        else
                        {
                            m_grpDistrictRestrictions.height = 60;
                        }
                    }
                    else
                    {
                        m_grpDistrictRestrictions.isVisible = false;
                    }

                    if (currentRule.m_incomingBuilding || currentRule.m_outgoingBuilding)
                    {
                        m_buildingRestrictionGroup.isVisible = true;
                        if (SelectionTool.Instance is not null &&
                            m_pnlBuildingRestrictionsIncoming is not null)
                        {
                            // Does this restriction allow an incoming rule
                            m_pnlBuildingRestrictionsIncoming.isVisible = currentRule.m_incomingBuilding;

                            if (m_btnBuildingRestrictionsIncoming is not null)
                            {
                                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionIncoming)
                                {
                                    m_btnBuildingRestrictionsIncoming.text = Localization.Get("btnBuildingRestrictionsSelected");
                                }
                                else
                                {
                                    m_btnBuildingRestrictionsIncoming.text = Localization.Get("btnBuildingRestrictions");
                                }
                                HashSet<ushort> allowedBuildings = restrictionSettings.m_incomingBuildingSettings.GetBuildingRestrictionsCopy();
                                m_lblBuildingRestrictionsIncoming.text = GetBuildingRestrictionLabel(true, allowedBuildings);
                                m_btnBuildingRestrictionsIncoming.tooltip = restrictionSettings.m_incomingBuildingSettings.Describe(m_buildingId);
                                m_btnBuildingRestrictionsIncomingClear.isEnabled = allowedBuildings.Count > 0;
                            }
                        }
                        if (SelectionTool.Instance is not null &&
                            m_pnlBuildingRestrictionsOutgoing is not null)
                        {
                            // Does this restriction allow an outgoing rule
                            m_pnlBuildingRestrictionsOutgoing.isVisible = currentRule.m_outgoingBuilding;

                            if (m_btnBuildingRestrictionsOutgoing is not null)
                            {
                                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing)
                                {
                                    m_btnBuildingRestrictionsOutgoing.text = Localization.Get("btnBuildingRestrictionsSelected");
                                }
                                else
                                {
                                    m_btnBuildingRestrictionsOutgoing.text = Localization.Get("btnBuildingRestrictions");
                                }

                                // Update label
                                HashSet<ushort> allowedBuildings = restrictionSettings.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                                m_lblBuildingRestrictionsOutgoing.text = GetBuildingRestrictionLabel(false, allowedBuildings);
                                m_btnBuildingRestrictionsOutgoing.tooltip = restrictionSettings.m_outgoingBuildingSettings.Describe(m_buildingId);
                                m_btnBuildingRestrictionsOutgoingClear.isEnabled = allowedBuildings.Count > 0;
                            }
                        }
                        if (currentRule.m_incomingBuilding && currentRule.m_outgoingBuilding)
                        {
                            m_buildingRestrictionGroup.height = 100;
                        }
                        else
                        {
                            m_buildingRestrictionGroup.height = 70;
                        }
                    }
                    else
                    {
                        m_buildingRestrictionGroup.isVisible = false;
                    }

                    // Distance restrictions
                    if (currentRule.m_distance)
                    {
                        m_grpServiceDistance.Show();
                        m_grpServiceDistance.Text = Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS");
                        m_sliderServiceDistance.SetValue(restrictionSettings.m_iServiceDistanceMeters / 1000.0f);

                        // Try and determine current reason and check global distance
                        CustomTransferReason.Reason reason = CustomTransferReason.Reason.None;
                        if (currentRule.m_reasons.Count == 1)
                        {
                            reason = currentRule.m_reasons.Single();
                        }
                        else if (IsWarehouse(eType))
                        {
                            reason = (CustomTransferReason.Reason)GetWarehouseActualTransferReason(m_buildingId);
                        }
                        if (reason != CustomTransferReason.Reason.None && TransferManagerModes.IsGlobalDistanceRestrictionsSupported(reason))
                        {
                            m_lblDistanceGlobal.Show();
                            m_lblDistanceGlobal.text = $"{Localization.Get("txtGlobalDistanceRestriction")}: {SaveGameSettings.GetSettings().GetActiveDistanceRestrictionKm(reason).ToString("N1")}";
                        }
                        else
                        {
                            m_lblDistanceGlobal.Hide();
                        }

                        if (m_lblDistanceGlobal.isVisible)
                        {
                            m_grpServiceDistance.height = 80;
                        }
                        else
                        {
                            m_grpServiceDistance.height = 60;
                        }
                    }
                    else
                    {
                        m_grpServiceDistance.Hide();
                    }

                    // Import / Export
                    if (currentRule.m_import || currentRule.m_export)
                    {
                        m_panelImportExport.Show();
                        m_chkAllowImport.isEnabled = currentRule.m_import;
                        m_chkAllowImport.isChecked = restrictionSettings.m_bAllowImport;
                        m_chkAllowExport.isEnabled = currentRule.m_export;
                        m_chkAllowExport.isChecked = restrictionSettings.m_bAllowExport;
                    }
                    else
                    {
                        m_panelImportExport.Hide();
                    }

                    // Outside connection multiplier
                    if (BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                    {
                        m_grpOutsideDistanceMultiplier.Show();
                        m_sliderOutsideDistanceMultiplier.SetValue(settings.m_iOutsideMultiplier);
                    }
                    else
                    {
                        m_grpOutsideDistanceMultiplier.Hide();
                    }

                    // Warehouse settings
                    if (IsWarehouse(eType))
                    {
                        m_panelGoodsDelivery.Show();
                        m_chkWarehouseOverride.isChecked = settings.m_bWarehouseOverride;
                        m_chkImprovedWarehouseMatching.isChecked = settings.IsImprovedWarehouseMatching();
                        m_chkImprovedWarehouseMatching.isEnabled = settings.m_bWarehouseOverride;

                        int iPercent = settings.ReserveCargoTrucksPercent();
                        m_sliderReserveCargoTrucks.SetValue(iPercent);
                        m_sliderReserveCargoTrucks.Enable(settings.m_bWarehouseOverride);

                        Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                        WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                        if (warehouse is not null && m_sliderReserveCargoTrucks.m_label is not null)
                        {
                            int iTrucks = (int)((float)(iPercent * 0.01) * (float)warehouse.m_truckCount);
                            m_sliderReserveCargoTrucks.m_label.text = Localization.Get("sliderWarehouseReservePercentLocal") + ": " + iPercent + " | Trucks: " + iTrucks;
                        }
                    }
                    else
                    {
                        m_panelGoodsDelivery.Hide();
                    }

                    // Apply to all buttons
                    if (m_applyToAll is not null)
                    {
                        m_applyToAll.UpdatePanel();
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

                if (DistrictSelectionPanel.Instance is not null && DistrictSelectionPanel.Instance.isVisible)
                {
                    DistrictSelectionPanel.Instance.SetPanelBuilding(m_buildingId, GetRestrictionId());
                    DistrictSelectionPanel.Instance.UpdatePanel();
                }

                // Update the panel height depending on building type.
                if (IsOutsideConnection(m_buildingId))
                {
                    m_panelTabPanel.height = m_panelImportExport.height + 12;
                    m_grpOutsideDistanceMultiplier.relativePosition = new Vector2(10, m_tabStripTransferReason.height + m_panelTabPanel.height + 12);
                }
                else
                {
                    m_panelTabPanel.height = m_pnlMain.height - m_tabStripTransferReason.height - 10;
                }

                UpdateDistrictButtonTooltips();

                m_bInSetup = false;

                UpdateScrollbarChanged();
            }
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
                    case BuildingType.Warehouse:
                    case BuildingType.WarehouseStation:
                    case BuildingType.CargoFerryWarehouseHarbor:
                        {
                            string sName = rule.m_name;

                            // For warehouses we add the actual material to the tab name.
                            CustomTransferReason actualTransferReason = BuildingTypeHelper.GetWarehouseActualTransferReason(m_buildingId);
                            if (actualTransferReason != CustomTransferReason.Reason.None && rule.m_reasons.Contains(actualTransferReason))
                            {
                                sName += $" ({actualTransferReason})";
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
                    case BuildingType.MainIndustryBuilding:
                    case BuildingType.AirportMainTerminal:
                    case BuildingType.AirportCargoTerminal:
                    case BuildingType.MainCampusBuilding:
                        {
                            if (SaveGameSettings.GetSettings().MainBuildingPostTruck)
                            {
                                // Turn off mail
                                if (rule.m_reasons.Contains(CustomTransferReason.Reason.Mail))
                                {
                                    bShowTabForRule = false;
                                }
                            }
                            else
                            {
                                // Turn off mail2
                                if (rule.m_reasons.Contains(CustomTransferReason.Reason.Mail2))
                                {
                                    bShowTabForRule = false;
                                }
                            }

                            // Update tab names
                            m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                            m_tabStripTransferReason.SetTabWidth(iTabIndex, GetTabWidth(rule.m_reasons));
                            m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(rule.m_reasons));
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
            m_tabStripTransferReason.SelectTabIndex(GetRestrictionTabIndex());
        }

        public void UpdateScrollbarChanged()
        {
            // Resize panel when scroll bar visible
            if (m_bScrollbarVisibleLayoutState != m_panelTabPanel.verticalScrollbar.isVisible)
            {
                if (m_panelTabPanel.verticalScrollbar.isVisible)
                {
                    m_panelTabPanel.width = m_tabStripTransferReason.width - 10;
                }
                else
                {
                    m_panelTabPanel.width = m_tabStripTransferReason.width;
                }

                m_grpDistrictRestrictions.width = m_panelTabPanel.width - 20;
                m_buildingRestrictionGroup.width = m_panelTabPanel.width - 20;
                m_grpServiceDistance.width = m_panelTabPanel.width - 20;
                m_panelImportExport.width = m_panelTabPanel.width - 20;
                m_panelGoodsDelivery.width = m_panelTabPanel.width - 20;
                m_grpOutsideDistanceMultiplier.width = m_panelTabPanel.width - 20;
            }
            m_bScrollbarVisibleLayoutState = m_panelTabPanel.verticalScrollbar.isVisible;
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

        private string GetBuildingRestrictionLabel(bool bIncoming, HashSet<ushort> restrictions)
        {
            // Update label
            string sText;
            if (bIncoming)
            {
                sText = Localization.Get("txtBuildingRestrictionsIncoming") + ": ";
            }
            else
            {
                sText = Localization.Get("txtBuildingRestrictionsOutgoing") + ": ";
            }

            // Add restriction text
            if (restrictions.Count == 0)
            {
                sText += Localization.Get("txtBuildingRestrictionsAllBuildings");
            }
            else
            {
                sText += $"{Localization.Get("txtBuildingRestrictionsRestricted")} ({restrictions.Count})";
            }

            return sText;
        }

        private string GetBuildingRestrictionTooltip(HashSet<ushort> restrictions)
        {
            string sTooltip = "Allowed Buildings:";
            if (restrictions.Count > 0)
            {
                foreach (ushort id in restrictions)
                {
                    sTooltip += "\n- " + CitiesUtils.GetBuildingName(id);
                }
            }
            else
            {
                sTooltip += "\n- " + Localization.Get("txtBuildingRestrictionsAllBuildings");
            }

            return sTooltip;
        }

        private void OnReasonTabChanged(int index)
        {
            if (!m_bInSetup)
            {
                m_iRestrictionTabIndex = index;

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

                UpdateTab(m_tabStrip);
            }
        }

        public void OnSelectIncomingDistrictClicked()
        {
            if (m_bInSetup) { return; }

            DistrictSelectionPanel.Init();
            if (DistrictSelectionPanel.Instance is not null)
            {
                DistrictSelectionPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId(), true);
            }
        }

        public void OnSelectOutgoingDistrictClicked()
        {
            if (m_bInSetup) { return; }

            DistrictSelectionPanel.Init();
            if (DistrictSelectionPanel.Instance is not null)
            {
                DistrictSelectionPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId(), false);
            }
        }

        public void UpdateDistrictButtonTooltips()
        {
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());
            if (m_btnIncomingSelectDistrict is not null)
            {
                m_btnIncomingSelectDistrict.tooltip = restrictions.m_incomingDistrictSettings.GetTooltip(m_buildingId);
            }
            if (m_btnOutgoingSelectDistrict is not null)
            {
                m_btnOutgoingSelectDistrict.tooltip = restrictions.m_outgoingDistrictSettings.GetTooltip(m_buildingId);
            }
        }

        public void OnServiceDistanceChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_iServiceDistanceMeters = (int) (Value * 1000.0f); // eg 3.5km = 3500

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }

        public void OnOutsideDistanceMultiplierChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_iOutsideMultiplier = (int)Value;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Invalidate outside connection path cache now we have updated a modifier
            PathNodeCache.InvalidateOutsideConnections();

            // Update outside connection panel with this new value
            if (OutsideConnectionPanel.Instance is not null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }

            UpdateTab(m_tabStrip);
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
            if (OutsideConnectionPanel.Instance is not null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }

            UpdateTab(m_tabStrip);
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
            if (OutsideConnectionPanel.Instance is not null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }

            UpdateTab(m_tabStrip);
        }

        public void OnIncomingPreferLocalServices(UIComponent component, int Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_incomingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal) Value;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }

        public void OnOutgoingPreferLocalServices(UIComponent component, int Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_outgoingDistrictSettings.m_iPreferLocalDistricts = (DistrictRestrictionSettings.PreferLocal)Value;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }

        public void OnWarehouseOverrideChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_bWarehouseOverride = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }

        public void OnImprovedWarehouseMatchingChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_bImprovedWarehouseMatching = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }

        public void OnReserveCargoTrucksChanged(float fPercent)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_iWarehouseReserveTrucksPercent = (int)fPercent;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }
        public void OnDistrictRestrictionsIncomingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            // Clear district settings
            restrictions.m_incomingDistrictSettings.Reset();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }
        public void OnDistrictRestrictionsOutgoingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            // Clear district settings
            restrictions.m_outgoingDistrictSettings.Reset();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }
        public void OnBuildingRestrictionsIncomingClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (SelectionTool.Instance is not null)
            {
                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionIncoming)
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                }
                else
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.BuildingRestrictionIncoming);
                }
                UpdateTab(m_tabStrip);
            }
        }

        public void OnBuildingRestrictionsIncomingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            if (PathDistanceTest.PATH_TESTING_ENABLED)
            {
                // DEBUGGING, Allows viewing path nodes visited
                BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

                PathDistanceTest pd = new PathDistanceTest();
                pd.FindNearestNeighbour(CustomTransferReason.Reason.Goods, m_buildingId, restrictions.m_incomingBuildingSettings.GetBuildingRestrictionsCopy().ToArray());
            }
            else
            {
                BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

                restrictions.m_incomingBuildingSettings.ClearBuildingRestrictions();

                settings.SetRestrictions(GetRestrictionId(), restrictions);
                BuildingSettingsStorage.SetSettings(m_buildingId, settings);

                UpdateTab(m_tabStrip);
            }
        }

        public void OnBuildingRestrictionsOutgoingClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (SelectionTool.Instance is not null)
            {
                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing)
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                }
                else
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing);
                }
                UpdateTab(m_tabStrip);
            }
        }

        public void OnBuildingRestrictionsOutgoingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_outgoingBuildingSettings.ClearBuildingRestrictions();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(m_tabStrip);
        }

        public void Destroy()
        {
            if (m_dropPreferLocalIncoming is not null)
            {
                m_dropPreferLocalIncoming.OnDestroy();
                m_dropPreferLocalIncoming = null;
            }
            if (m_dropPreferLocalOutgoing is not null)
            {
                m_dropPreferLocalOutgoing.OnDestroy();
                m_dropPreferLocalOutgoing = null;
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
