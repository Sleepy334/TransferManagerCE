using ColossalFramework.UI;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.BuildingPanel;
using TransferManagerCE.Settings;
using System.Collections.Generic;
using static TransferManager;
using TransferManagerCE.CustomManager;
using TransferManagerCE.TransferRules;
using TransferManagerCE.Common;

namespace TransferManagerCE.UI
{
    public class BuildingSettingsTab
    {
        const float fTEXT_SCALE = 0.9f;
        const float fTAB_PANEL_HEIGHT = 470f;

        // Settings tab
        private UITabStrip? m_tabStripTransferReason = null;
        private UIPanel? m_pnlMain = null;
        private UIPanel? m_panelTabPanel = null;

        private UIGroup? m_grpDistrictRestrictions = null;
        private UIPanel? m_panelIncomingDistrict = null;
        private UIMyDropDown? m_dropPreferLocalIncoming = null;
        private UIButton? m_btnIncomingSelectDistrict = null;
        private UIButton? m_btnDistrictRestrictionsIncomingClear = null;
        private UIPanel? m_panelOutgoingDistrict = null;
        private UIMyDropDown? m_dropPreferLocalOutgoing = null;
        private UIButton? m_btnOutgoingSelectDistrict = null;
        private UIButton? m_btnDistrictRestrictionsOutgoingClear = null;

        private UIGroup? m_panelServiceDistance = null;
        private SettingsSlider? m_sliderServiceDistance = null;

        private UIGroup? m_panelImportExport = null;
        private UICheckBox? m_chkAllowImport = null;
        private UICheckBox? m_chkAllowExport = null;

        private UIGroup? m_panelOutsideDistanceMultiplier = null;
        private SettingsSlider? m_sliderOutsideDistanceMultiplier = null;

        // Warehouse options
        private UIGroup? m_panelGoodsDelivery = null;
        private UICheckBox? m_chkWarehouseOverride = null;
        private UICheckBox? m_chkWarehouseFirst = null;
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

            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.Hide();
            }
            
            UpdateSettingsTab();
        }

        public int GetRestrictionTabIndex()
        {
            return m_iRestrictionTabIndex;
        }

        public int GetRestrictionId()
        {
            if (m_tabStripTransferReason != null && m_tabStripTransferReason.Count > GetRestrictionTabIndex())
            {
                return m_tabStripTransferReason.GetTabId(GetRestrictionTabIndex());                
            }
            return 0;
        }

        public void Setup(UITabStrip tabStrip)
        {
            m_bInSetup = true;
            m_tabStrip = tabStrip;

            UIPanel? tabSettings = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelSettings"));
            if (tabSettings != null)
            {
                tabSettings.autoLayout = true;
                tabSettings.autoLayoutDirection = LayoutDirection.Vertical;
                tabSettings.padding = new RectOffset(10, 10, 10, 10);

                UIHelper helper = new UIHelper(tabSettings);

                m_pnlMain = tabSettings.AddUIComponent<UIPanel>();
                m_pnlMain.width = tabSettings.width - 20;
                m_pnlMain.height = 500f;
                m_pnlMain.autoLayout = true;
                m_pnlMain.autoLayoutDirection = LayoutDirection.Vertical;
                m_pnlMain.autoLayoutPadding = new RectOffset(0, 0, 0, 8);
                //m_pnlMain.backgroundSprite = "InfoviewPanel";
                //m_pnlMain.color = new Color32(150, 0, 0, 255);

                // Color the tabs differently so it's easier to distingush between multuiple tab levels
                m_tabStripTransferReason = UITabStrip.Create(m_pnlMain, tabSettings.width - 20f, 18f, OnReasonTabChanged);
                m_tabStripTransferReason.buttonNormalSprite = "SubBarButtonBase";
                m_tabStripTransferReason.buttonDisabledSprite = "SubBarButtonBaseDisabled";
                m_tabStripTransferReason.buttonFocusedSprite = "SubBarButtonBasePressed";
                m_tabStripTransferReason.buttonHoveredSprite = "SubBarButtonBaseHovered";
                m_tabStripTransferReason.buttonPressedSprite = "SubBarButtonBasePressed";

                m_panelTabPanel = m_pnlMain.AddUIComponent<UIPanel>();
                m_panelTabPanel.width = m_tabStripTransferReason.width;
                m_panelTabPanel.height = fTAB_PANEL_HEIGHT;
                //m_panelTabPanel.backgroundSprite = "UnlockingPanel";// "GenericPanelLight";// "UnlockingPanel";// "InfoPanel";// "TextFieldPanel";
                m_panelTabPanel.backgroundSprite = "InfoviewPanel";
                m_panelTabPanel.color = new Color32(150, 150, 150, 255);
                m_panelTabPanel.padding = new RectOffset(10, 10, 10, 10);
                m_panelTabPanel.autoLayout = true;
                m_panelTabPanel.autoLayoutDirection = LayoutDirection.Vertical;
                m_panelTabPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 4);

                // Prefer local services
                m_grpDistrictRestrictions = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_DISTRICT"), fTEXT_SCALE, m_panelTabPanel.width - 20, 140);
                if (m_grpDistrictRestrictions != null)
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
                    if (m_dropPreferLocalIncoming != null)
                    {
                        m_dropPreferLocalIncoming.m_panel.relativePosition = new Vector3(0, 0);
                        m_dropPreferLocalIncoming.SetPanelWidth(m_panelIncomingDistrict.width - iDropDownWidthOffset);
                        m_dropPreferLocalIncoming.m_dropdown.textScale = 0.9f;
                    }
                    m_btnIncomingSelectDistrict = UIUtils.AddButton(m_panelIncomingDistrict, Localization.Get("btnDistricts") + "...", 120, m_dropPreferLocalIncoming.m_dropdown.height, (c, e) => OnSelectIncomingDistrictClicked());
                    if (m_btnIncomingSelectDistrict != null)
                    {
                        m_btnIncomingSelectDistrict.relativePosition = new Vector3(m_dropPreferLocalIncoming.m_panel.width + 6, 2);
                        m_btnIncomingSelectDistrict.eventTooltipEnter += (c, e) => UpdateDistrictButtonTooltips();
                    }

                    m_btnDistrictRestrictionsIncomingClear = UIUtils.AddSpriteButton(m_panelIncomingDistrict, "Niet", m_dropPreferLocalIncoming.m_dropdown.height, m_dropPreferLocalIncoming.m_dropdown.height, OnDistrictRestrictionsIncomingClearClicked);
                    if (m_btnDistrictRestrictionsIncomingClear != null)
                    {
                        m_btnDistrictRestrictionsIncomingClear.relativePosition = new Vector3(m_dropPreferLocalIncoming.m_panel.width + m_btnIncomingSelectDistrict.width + 12, 2);
                        m_btnDistrictRestrictionsIncomingClear.tooltip = Localization.Get("btnClear");
                    }

                    // Outgoing restrictions
                    m_panelOutgoingDistrict = m_grpDistrictRestrictions.m_content.AddUIComponent<UIPanel>();
                    m_panelOutgoingDistrict.width = m_grpDistrictRestrictions.width;
                    m_panelOutgoingDistrict.height = 35;
                    m_dropPreferLocalOutgoing = UIMyDropDown.Create(m_panelOutgoingDistrict, Localization.Get("dropdownBuildingPanelOutgoingPreferLocalLabel"), fTEXT_SCALE, itemsPreferLocal, OnOutgoingPreferLocalServices, 0, iDropDownWidth);
                    if (m_dropPreferLocalOutgoing != null)
                    {
                        m_dropPreferLocalOutgoing.m_panel.relativePosition = new Vector3(0, 0);
                        m_dropPreferLocalOutgoing.SetPanelWidth(m_panelOutgoingDistrict.width - iDropDownWidthOffset);
                        m_dropPreferLocalOutgoing.m_dropdown.textScale = 0.9f;
                    }
                    m_btnOutgoingSelectDistrict = UIUtils.AddButton(m_panelOutgoingDistrict, Localization.Get("btnDistricts") + "...", 120, m_dropPreferLocalOutgoing.m_dropdown.height, (c, e) => OnSelectOutgoingDistrictClicked());
                    if (m_btnOutgoingSelectDistrict != null)
                    {
                        m_btnOutgoingSelectDistrict.relativePosition = new Vector3(m_dropPreferLocalOutgoing.m_panel.width + 6, 2);
                        m_btnOutgoingSelectDistrict.eventTooltipEnter += (c, e) => UpdateDistrictButtonTooltips();
                    }

                    m_btnDistrictRestrictionsOutgoingClear = UIUtils.AddSpriteButton(m_panelOutgoingDistrict, "Niet", m_dropPreferLocalOutgoing.m_dropdown.height, m_dropPreferLocalOutgoing.m_dropdown.height, OnDistrictRestrictionsOutgoingClearClicked);
                    if (m_btnDistrictRestrictionsOutgoingClear != null)
                    {
                        m_btnDistrictRestrictionsOutgoingClear.relativePosition = new Vector3(m_dropPreferLocalOutgoing.m_panel.width + m_btnOutgoingSelectDistrict.width + 12, 2);
                        m_btnDistrictRestrictionsOutgoingClear.tooltip = Localization.Get("btnClear");
                    }
                }

                // Building restrictions
                m_buildingRestrictionGroup = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_BUILDING_RESTRICTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 100);
                if (m_buildingRestrictionGroup != null)
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
                    m_lblBuildingRestrictionsIncoming.textScale = fTEXT_SCALE;
                    m_lblBuildingRestrictionsIncoming.autoSize = false;
                    m_lblBuildingRestrictionsIncoming.height = 30;
                    m_lblBuildingRestrictionsIncoming.width = 370;
                    m_lblBuildingRestrictionsIncoming.verticalAlignment = UIVerticalAlignment.Middle;

                    // Buttons
                    m_btnBuildingRestrictionsIncoming = UIUtils.AddButton(m_pnlBuildingRestrictionsIncoming, Localization.Get("btnBuildingRestrictions"), 200, iButtonHeight, OnBuildingRestrictionsIncomingClicked);
                    m_btnBuildingRestrictionsIncomingClear = UIUtils.AddSpriteButton(m_pnlBuildingRestrictionsIncoming, "Niet", iButtonHeight, iButtonHeight, OnBuildingRestrictionsIncomingClearClicked);
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
                    m_lblBuildingRestrictionsOutgoing.autoSize = false;
                    m_lblBuildingRestrictionsOutgoing.height = m_lblBuildingRestrictionsIncoming.height;
                    m_lblBuildingRestrictionsOutgoing.width = m_lblBuildingRestrictionsIncoming.width;
                    m_lblBuildingRestrictionsOutgoing.verticalAlignment = UIVerticalAlignment.Middle;

                    // Buttons
                    m_btnBuildingRestrictionsOutgoing = UIUtils.AddButton(m_pnlBuildingRestrictionsOutgoing, Localization.Get("btnBuildingRestrictions"), 200, iButtonHeight, OnBuildingRestrictionsOutgoingClicked);
                    m_btnBuildingRestrictionsOutgoingClear = UIUtils.AddSpriteButton(m_pnlBuildingRestrictionsOutgoing, "Niet", iButtonHeight, iButtonHeight, OnBuildingRestrictionsOutgoingClearClicked);
                    m_btnBuildingRestrictionsOutgoingClear.tooltip = Localization.Get("btnClear");
                }

                m_panelServiceDistance = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 46);
                if (m_panelServiceDistance != null)
                {
                    UIHelper helperDistance = new UIHelper(m_panelServiceDistance.m_content);
                    m_sliderServiceDistance = SettingsSlider.Create(helperDistance, LayoutDirection.Horizontal, Localization.Get("sliderDistanceRestriction"), fTEXT_SCALE, 400, 200, 0f, 10, 1f, 0f, OnServiceDistanceChanged);
                    m_sliderServiceDistance.SetTooltip(Localization.Get("sliderDistanceRestrictionTooltip"));
                }

                // Outside connections
                m_panelImportExport = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 70);
                if (m_panelImportExport != null && m_panelImportExport.m_content != null)
                {
                    m_chkAllowImport = UIUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowImport"), fTEXT_SCALE, true, OnAllowImportChanged);
                    m_chkAllowExport = UIUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowExport"), fTEXT_SCALE, true, OnAllowExportChanged);
                }

                // Good delivery
                m_panelGoodsDelivery = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_GOODS_DELIVERY"), fTEXT_SCALE, m_panelTabPanel.width - 20, 120);
                if (m_panelGoodsDelivery != null)
                {
                    m_chkWarehouseOverride = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionWarehouseOverride"), fTEXT_SCALE, false, OnWarehouseOverrideChanged);
                    m_chkWarehouseFirst = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionWarehouseFirst"), fTEXT_SCALE, false, OnWarehouseFirstChanged);
                    m_chkImprovedWarehouseMatching = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionImprovedWarehouseTransfer"), fTEXT_SCALE, false, OnImprovedWarehouseMatchingChanged);
                    UIHelper helperGoodsDelivery = new UIHelper(m_panelGoodsDelivery.m_content);
                    m_sliderReserveCargoTrucks = SettingsSlider.Create(helperGoodsDelivery, LayoutDirection.Horizontal, Localization.Get("sliderWarehouseReservePercentLocal"), fTEXT_SCALE, 400, 200, 0f, 100f, 1f, 0f, OnReserveCargoTrucksChanged);
                }

                m_panelOutsideDistanceMultiplier = UIGroup.AddGroup(m_pnlMain, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_DISTANCE_MULTIPLIER"), fTEXT_SCALE, m_pnlMain.width, 60);
                if (m_panelOutsideDistanceMultiplier != null)
                {
                    UIHelper helperDistanceMultiplier = new UIHelper(m_panelOutsideDistanceMultiplier.m_content);
                    m_sliderOutsideDistanceMultiplier = SettingsSlider.Create(helperDistanceMultiplier, LayoutDirection.Horizontal, Localization.Get("sliderOutsideDistanceMultiplier"), fTEXT_SCALE, 460, 200, 0f, 10, 1f, 0f, OnOutsideDistanceMultiplierChanged);
                    m_sliderOutsideDistanceMultiplier.SetTooltip(Localization.Get("sliderOutsideDistanceMultiplierTooltip"));
                }

                // Apply to all
                m_applyToAll = UIApplyToAll.Create(tabSettings);
            }

            m_bInSetup = false;
        }

        public void UpdateSettingsTab()
        {
            if (m_tabStrip == null || m_tabStripTransferReason == null || m_bInSetup)
            {
                return;
            }

            m_bInSetup = true;

            // Load applicable rule sets for this building
            BuildingType eType = GetBuildingType(m_buildingId);

            List<ReasonRule> buildingRules = BuildingRuleSets.GetRules(eType, m_buildingId);
            if (buildingRules.Count > 0)
            {
                m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, true);

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
                    m_tabStripTransferReason.SetTabVisible(iTabIndex, true);

                    // Add 
                    if (eType == BuildingType.Warehouse)
                    {
                        string sName = rule.m_name;

                        // For warehouses we add the actual material to the tab name.
                        Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                        if (building.m_flags != 0)
                        {
                            WarehouseAI? warehouseAI = building.Info.GetAI() as WarehouseAI;
                            if (warehouseAI != null)
                            {
                                
                                CustomTransferReason actualTransferReason = warehouseAI.GetActualTransferReason(m_buildingId, ref building);
                                sName += $" ({actualTransferReason})";
                            }
                        }

                        m_tabStripTransferReason.SetTabText(iTabIndex, sName);
                        m_tabStripTransferReason.SetTabWidth(iTabIndex, 300f);
                    }
                    else
                    {
                        m_tabStripTransferReason.SetTabText(iTabIndex, rule.m_name);
                        m_tabStripTransferReason.SetTabWidth(iTabIndex, 200f);
                    }

                    m_tabStripTransferReason.SetTabId(iTabIndex, rule.m_id);
                    m_tabStripTransferReason.SetTabToolTip(iTabIndex, GetRestrictionReasons(rule.m_reasons));
                    iTabIndex++;
                }

                // Update the panel height depending on building type.
                if (m_panelTabPanel != null)
                {
                    if (IsOutsideConnection(m_buildingId))
                    {
                        m_panelTabPanel.height = 90f;
                    }
                    else
                    {
                        m_panelTabPanel.height = fTAB_PANEL_HEIGHT;
                    }
                }
                
                // Update selected tab
                m_tabStripTransferReason.SelectTabIndex(GetRestrictionTabIndex());

                // Update settings
                ReasonRule currentRule = buildingRules[GetRestrictionTabIndex()];
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
                RestrictionSettings restrictionSettings = settings.GetRestrictions(currentRule.m_id);

                // District restrictions
                if (m_grpDistrictRestrictions != null)
                {
                    if (currentRule.m_incomingDistrict || currentRule.m_outgoingDistrict) 
                    { 
                        m_grpDistrictRestrictions.isVisible = true;

                        // Incoming
                        if (m_panelIncomingDistrict != null)
                        {
                            m_panelIncomingDistrict.isVisible = currentRule.m_incomingDistrict;

                            if (m_dropPreferLocalIncoming != null)
                            {
                                m_dropPreferLocalIncoming.isVisible = currentRule.m_incomingDistrict;
                                m_dropPreferLocalIncoming.selectedIndex = (int)restrictionSettings.m_iPreferLocalDistrictsIncoming;
                            }
                            if (m_btnIncomingSelectDistrict != null)
                            {
                                m_btnIncomingSelectDistrict.isVisible = currentRule.m_incomingDistrict;
                                m_btnIncomingSelectDistrict.isEnabled = (restrictionSettings.m_iPreferLocalDistrictsIncoming != RestrictionSettings.PreferLocal.ALL_DISTRICTS);
                            }
                            if (m_btnDistrictRestrictionsIncomingClear != null)
                            {
                                m_btnDistrictRestrictionsIncomingClear.isEnabled = restrictionSettings.IsDistrictRestrictionsIncomingSet();
                            }
                        }

                        // Outgoing
                        if (m_panelOutgoingDistrict != null)
                        {
                            m_panelOutgoingDistrict.isVisible = currentRule.m_outgoingDistrict;

                            if (m_dropPreferLocalOutgoing != null)
                            {
                                m_dropPreferLocalOutgoing.isVisible = currentRule.m_outgoingDistrict;
                                m_dropPreferLocalOutgoing.selectedIndex = (int)restrictionSettings.m_iPreferLocalDistrictsOutgoing;
                            }
                            if (m_btnOutgoingSelectDistrict != null)
                            {
                                m_btnOutgoingSelectDistrict.isVisible = currentRule.m_outgoingDistrict;
                                m_btnOutgoingSelectDistrict.isEnabled = (restrictionSettings.m_iPreferLocalDistrictsOutgoing != RestrictionSettings.PreferLocal.ALL_DISTRICTS);             
                            }
                            if (m_btnDistrictRestrictionsOutgoingClear != null)
                            {
                                m_btnDistrictRestrictionsOutgoingClear.isEnabled = restrictionSettings.IsDistrictRestrictionsOutgoingSet();
                            }
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
                }

                if (m_buildingRestrictionGroup != null)
                {
                    if (currentRule.m_incomingBuilding || currentRule.m_outgoingBuilding)
                    {
                        m_buildingRestrictionGroup.isVisible = true;
                        if (SelectionTool.Instance != null && 
                            m_pnlBuildingRestrictionsIncoming != null)
                        {
                            // Does this restriction allow an incoming rule
                            m_pnlBuildingRestrictionsIncoming.isVisible = currentRule.m_incomingBuilding;

                            if (m_btnBuildingRestrictionsIncoming != null)
                            {
                                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionIncoming)
                                {
                                    m_btnBuildingRestrictionsIncoming.text = Localization.Get("btnBuildingRestrictionsSelected");
                                }
                                else
                                {
                                    m_btnBuildingRestrictionsIncoming.text = Localization.Get("btnBuildingRestrictions");
                                }
                                m_lblBuildingRestrictionsIncoming.text = GetBuildingRestrictionLabel(true, restrictionSettings.m_incomingBuildingsAllowed);
                                m_btnBuildingRestrictionsIncoming.tooltip = GetBuildingRestrictionTooltip(restrictionSettings.m_incomingBuildingsAllowed);
                                m_btnBuildingRestrictionsIncomingClear.isEnabled = restrictionSettings.m_incomingBuildingsAllowed.Count > 0;
                            }
                        }
                        if (SelectionTool.Instance != null && 
                            m_pnlBuildingRestrictionsOutgoing != null)
                        {
                            // Does this restriction allow an outgoing rule
                            m_pnlBuildingRestrictionsOutgoing.isVisible = currentRule.m_outgoingBuilding;

                            if (m_btnBuildingRestrictionsOutgoing != null)
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
                                m_lblBuildingRestrictionsOutgoing.text = GetBuildingRestrictionLabel(false, restrictionSettings.m_outgoingBuildingsAllowed);
                                m_btnBuildingRestrictionsOutgoing.tooltip = GetBuildingRestrictionTooltip(restrictionSettings.m_outgoingBuildingsAllowed);
                                m_btnBuildingRestrictionsOutgoingClear.isEnabled = restrictionSettings.m_outgoingBuildingsAllowed.Count > 0;
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
                }
                
                // Distance restrictions
                if (m_panelServiceDistance != null)
                {
                    if (currentRule.m_distance)
                    {
                        m_panelServiceDistance.Show();
                        if (m_sliderServiceDistance != null)
                        {
                            m_panelServiceDistance.Text = Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS");
                            m_sliderServiceDistance.SetValue(restrictionSettings.m_iServiceDistance);
                        }
                    }
                    else
                    {
                        m_panelServiceDistance.Hide();
                    }
                }
                else
                {
                    Debug.Log("m_panelServiceDistance is null");
                }

                // Import / Export
                if (m_panelImportExport != null)
                {
                    if (currentRule.m_import || currentRule.m_export)
                    {
                        m_panelImportExport.Show();

                        if (m_chkAllowImport != null)
                        {
                            m_chkAllowImport.isEnabled = currentRule.m_import;
                            m_chkAllowImport.isChecked = restrictionSettings.m_bAllowImport;
                        }
                        if (m_chkAllowExport != null)
                        {
                            m_chkAllowExport.isEnabled = currentRule.m_export;
                            m_chkAllowExport.isChecked = restrictionSettings.m_bAllowExport;
                        }
                    }
                    else
                    {
                        m_panelImportExport.Hide();
                    }
                }

                // Outside connection multiplier
                if (m_panelOutsideDistanceMultiplier != null)
                {
                    if (BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                    {
                        m_panelOutsideDistanceMultiplier.Show();
                        if (m_sliderOutsideDistanceMultiplier != null)
                        {
                            m_sliderOutsideDistanceMultiplier.SetValue(settings.m_iOutsideMultiplier);
                        }
                    }
                    else
                    {
                        m_panelOutsideDistanceMultiplier.Hide();
                    }
                }
                else
                {
                    Debug.Log("m_panelOutsideDistanceMultiplier is null");
                }

                // Warehouse settings
                if (m_panelGoodsDelivery != null)
                {
                    if (eType == BuildingType.Warehouse)
                    {
                        m_panelGoodsDelivery.Show();

                        if (m_chkWarehouseOverride != null)
                        {
                            m_chkWarehouseOverride.isChecked = settings.m_bWarehouseOverride;
                        }

                        if (m_chkWarehouseFirst != null)
                        {
                            m_chkWarehouseFirst.isChecked = settings.IsWarehouseFirst();
                            m_chkWarehouseFirst.isEnabled = settings.m_bWarehouseOverride;
                        }

                        if (m_chkImprovedWarehouseMatching != null)
                        {
                            m_chkImprovedWarehouseMatching.isChecked = settings.IsImprovedWarehouseMatching();
                            m_chkImprovedWarehouseMatching.isEnabled = settings.m_bWarehouseOverride;
                        }

                        if (m_sliderReserveCargoTrucks != null)
                        {
                                
                            int iPercent = settings.ReserveCargoTrucksPercent();
                            m_sliderReserveCargoTrucks.SetValue(iPercent);
                            m_sliderReserveCargoTrucks.Enable(settings.m_bWarehouseOverride);

                            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                            WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                            if (warehouse != null && m_sliderReserveCargoTrucks.m_label != null)
                            {
                                int iTrucks = (int)((float)(iPercent * 0.01) * (float)warehouse.m_truckCount);
                                m_sliderReserveCargoTrucks.m_label.text = Localization.Get("sliderWarehouseReservePercentLocal") + ": " + iPercent + " | Trucks: "  + iTrucks;
                            }
                        }
                    }
                    else
                    {
                        m_panelGoodsDelivery.Hide();
                    }
                }

                // Apply to all buttons
                if (m_applyToAll != null)
                {
                    m_applyToAll.UpdatePanel();
                }
            }
            else
            {
                m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);

                // Hide them all
                for (int i = 0; i<m_tabStripTransferReason.Count; ++i)
                {
                    m_tabStripTransferReason.SetTabVisible(i, false);
                }
            }

            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.SetPanelBuilding(m_buildingId, GetRestrictionId());
                DistrictPanel.Instance.UpdatePanel();
            }

            UpdateDistrictButtonTooltips();

            m_bInSetup = false;
        }

        private string GetRestrictionReasons(HashSet<TransferReason> reasons)
        {
            string sTooltip = "Restriction Reasons:";
            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                WarehouseAI? warehouseAI = building.Info.GetAI() as WarehouseAI;
                if (warehouseAI != null)
                {
                    // Warehouses, just return the actual material they store
                    CustomTransferReason actualTransferReason = warehouseAI.GetActualTransferReason(m_buildingId, ref building);
                    sTooltip += $"\r\n- {actualTransferReason}";
                }
                else
                {
                    foreach (CustomTransferReason reason in reasons)
                    {
                        sTooltip += $"\r\n- {reason}";
                    }
                    
                }
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
                if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
                {
                    DistrictPanel.Instance.Hide();
                }

                // Turn off building selection mode
                if (SelectionTool.Instance != null)
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                }

                UpdateSettingsTab();
            }
        }

        public void OnSelectIncomingDistrictClicked()
        {
            if (m_bInSetup) { return; }

            DistrictPanel.Init();
            if (DistrictPanel.Instance != null)
            {
                DistrictPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId(), true);
            }
        }

        public void OnSelectOutgoingDistrictClicked()
        {
            if (m_bInSetup) { return; }

            DistrictPanel.Init();
            if (DistrictPanel.Instance != null)
            {
                DistrictPanel.Instance.ShowPanel(m_buildingId, GetRestrictionId(), false);
            }
        }

        public void UpdateDistrictButtonTooltips()
        {
            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());
            if (m_btnIncomingSelectDistrict != null)
            {
                m_btnIncomingSelectDistrict.tooltip = restrictions.GetIncomingDistrictTooltip(m_buildingId);
            }
            if (m_btnOutgoingSelectDistrict != null)
            {
                m_btnOutgoingSelectDistrict.tooltip = restrictions.GetOutgoingDistrictTooltip(m_buildingId);
            }
        }

        public void OnServiceDistanceChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            restrictions.m_iServiceDistance = (int)Value;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideDistanceMultiplierChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            settings.m_iOutsideMultiplier = (int)Value;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update outside connection panel with this new value
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }
        }

        public void OnAllowImportChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            restrictions.m_bAllowImport = bChecked;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }
        }

        public void OnAllowExportChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());
            
            restrictions.m_bAllowExport = bChecked;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }
        }

        public void OnIncomingPreferLocalServices(UIComponent component, int Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            restrictions.m_iPreferLocalDistrictsIncoming = (RestrictionSettings.PreferLocal) Value;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }

        public void OnOutgoingPreferLocalServices(UIComponent component, int Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            restrictions.m_iPreferLocalDistrictsOutgoing = (RestrictionSettings.PreferLocal)Value;

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }

        public void OnWarehouseOverrideChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            settings.m_bWarehouseOverride = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);
            UpdateSettingsTab();
        }

        public void OnWarehouseFirstChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            settings.m_bWarehouseFirst = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);
        }

        public void OnImprovedWarehouseMatchingChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            settings.m_bImprovedWarehouseMatching = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);
        }

        public void OnReserveCargoTrucksChanged(float fPercent)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            settings.m_iWarehouseReserveTrucksPercent = (int)fPercent;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }
        public void OnDistrictRestrictionsIncomingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            // Clear district settings
            restrictions.ResetDistrictRestrictionsIncoming();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }
        public void OnDistrictRestrictionsOutgoingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            // Clear district settings
            restrictions.ResetDistrictRestrictionsOutgoing();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }
        public void OnBuildingRestrictionsIncomingClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (SelectionTool.Instance != null)
            {
                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionIncoming)
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                }
                else
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.BuildingRestrictionIncoming);
                }
                UpdateSettingsTab();
            }
        }

        public void OnBuildingRestrictionsIncomingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            restrictions.m_incomingBuildingsAllowed.Clear();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }

        public void OnBuildingRestrictionsOutgoingClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (SelectionTool.Instance != null)
            {
                if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing)
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                }
                else
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing);
                }
                UpdateSettingsTab();
            }
        }

        public void OnBuildingRestrictionsOutgoingClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictions(GetRestrictionId());

            restrictions.m_outgoingBuildingsAllowed.Clear();

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateSettingsTab();
        }

        public void Destroy()
        {
            if (m_dropPreferLocalIncoming != null)
            {
                m_dropPreferLocalIncoming.OnDestroy();
                m_dropPreferLocalIncoming = null;
            }
            if (m_dropPreferLocalOutgoing != null)
            {
                m_dropPreferLocalOutgoing.OnDestroy();
                m_dropPreferLocalOutgoing = null;
            }
            if (m_panelImportExport != null)
            {
                UnityEngine.Object.Destroy(m_panelImportExport.gameObject);
                m_panelImportExport = null;
            }
            if (m_chkAllowImport != null)
            {
                UnityEngine.Object.Destroy(m_chkAllowImport.gameObject);
                m_chkAllowImport = null;
            }
            if (m_chkAllowExport != null)
            {
                UnityEngine.Object.Destroy(m_chkAllowExport.gameObject);
                m_chkAllowExport = null;
            }
            if (m_panelGoodsDelivery != null)
            {
                UnityEngine.Object.Destroy(m_panelGoodsDelivery.gameObject);
                m_panelGoodsDelivery = null;
            }
            if (m_chkWarehouseFirst != null)
            {
                UnityEngine.Object.Destroy(m_chkWarehouseFirst.gameObject);
                m_chkWarehouseFirst = null;
            }
            if (m_sliderReserveCargoTrucks != null)
            {
                m_sliderReserveCargoTrucks.Destroy();
                m_sliderReserveCargoTrucks = null;
            }
            if (m_grpDistrictRestrictions != null)
            {
                UnityEngine.Object.Destroy(m_grpDistrictRestrictions.gameObject);
                m_grpDistrictRestrictions = null;
            }
            
        }
    }
}
