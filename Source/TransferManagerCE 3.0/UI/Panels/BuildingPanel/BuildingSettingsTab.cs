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

namespace TransferManagerCE.UI
{
    public class BuildingSettingsTab : BuildingTab
    {
        const float fTEXT_SCALE = 0.9f;

        // Settings tab
        private UITabStrip? m_tabStripTransferReason = null;
        private UIPanel? m_pnlMain = null;
        private UIScrollablePanel? m_panelTabPanel = null;

        private UIGroup? m_grpDistrictRestrictions = null;

        private UIDistrictRestrictionsPanel? m_incomingDistrictPanel = null;
        private UIDistrictRestrictionsPanel? m_outgoingDistrictPanel = null;

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

        private UIBuildingRestrictionsPanel m_pnlBuildingRestrictionsIncoming = null;
        private UIBuildingRestrictionsPanel m_pnlBuildingRestrictionsOutgoing = null;

        // Apply to all
        private UIApplyToAll? m_applyToAll = null;

        private int m_iRestrictionTabIndex = 0;
        private bool m_bInSetup = false;
        private bool m_bScrollbarVisibleLayoutState = false;

        // ----------------------------------------------------------------------------------------
        public BuildingSettingsTab() :
            base()
        {
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
            return -1;
        }

        public override void SetTabBuilding(ushort buildingId, BuildingType buildingType, List<ushort> subBuildingIds)
        {
            if (buildingId != m_buildingId)
            {
                m_iRestrictionTabIndex = 0;

                if (DistrictSelectionPanel.IsVisible())
                {
                    DistrictSelectionPanel.Instance.Hide();
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
                m_panelTabPanel.scrollPadding.bottom = 12;

                // Add scroll bar
                UIScrollbars.AddScrollbar(m_pnlMain, m_panelTabPanel);

                m_panelTabPanel.eventSizeChanged += (c, value) =>
                {
                    m_panelTabPanel.scrollPadding.top = 1;
                };

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
                    const int iButtonHeight = 28;

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
                    m_sliderServiceDistance = SettingsSlider.Create(m_grpServiceDistance.m_content, LayoutDirection.Horizontal, Localization.Get("sliderDistanceRestriction"), UIFonts.Regular, fTEXT_SCALE, 400, 280, 0f, 20f, 0.5f, 0f, 1, OnServiceDistanceChanged);
                    m_sliderServiceDistance.SetTooltip(Localization.Get("sliderDistanceRestrictionTooltip"));

                    m_lblDistanceGlobal = m_grpServiceDistance.m_content.AddUIComponent<UILabel>();
                    m_lblDistanceGlobal.text = Localization.Get("txtGlobalDistanceRestriction");
                    m_lblDistanceGlobal.textScale = fTEXT_SCALE;
                    m_lblDistanceGlobal.font = UIFonts.Regular;
                    m_lblDistanceGlobal.autoSize = false;// true;
                    m_lblDistanceGlobal.height = 26;
                    m_lblDistanceGlobal.width = m_grpServiceDistance.m_content.width;
                    m_lblDistanceGlobal.verticalAlignment = UIVerticalAlignment.Middle;
                }

                // ------------------------------------------------------------
                // Outside connections
                m_panelImportExport = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS"), fTEXT_SCALE, m_panelTabPanel.width - 20, 70);
                if (m_panelImportExport is not null && m_panelImportExport.m_content is not null)
                {
                    m_chkAllowImport = UIMyUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowImport"), UIFonts.Regular, fTEXT_SCALE, true, OnAllowImportChanged);
                    m_chkAllowExport = UIMyUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowExport"), UIFonts.Regular, fTEXT_SCALE, true, OnAllowExportChanged);
                }

                // ------------------------------------------------------------
                // Goods delivery
                m_panelGoodsDelivery = UIGroup.AddGroup(m_panelTabPanel, Localization.Get("GROUP_BUILDINGPANEL_GOODS_DELIVERY"), fTEXT_SCALE, m_panelTabPanel.width - 20, 95);
                if (m_panelGoodsDelivery is not null)
                {
                    m_chkWarehouseOverride = UIMyUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionWarehouseOverride"), UIFonts.Regular, fTEXT_SCALE, false, OnWarehouseOverrideChanged);
                    m_chkImprovedWarehouseMatching = UIMyUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionImprovedWarehouseTransfer"), UIFonts.Regular, fTEXT_SCALE, false, OnImprovedWarehouseMatchingChanged);
                    m_sliderReserveCargoTrucks = SettingsSlider.Create(m_panelGoodsDelivery.m_content, LayoutDirection.Horizontal, Localization.Get("sliderWarehouseReservePercentLocal"), UIFonts.Regular, fTEXT_SCALE, 400, 280, 0f, 100f, 1f, 0f, 0, OnReserveCargoTrucksChanged);
                }

                // ------------------------------------------------------------
                m_grpOutsideDistanceMultiplier = UIGroup.AddGroup(m_pnlMain, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_DISTANCE_MULTIPLIER"), fTEXT_SCALE, m_pnlMain.width - 20, 60);
                if (m_grpOutsideDistanceMultiplier is not null)
                {
                    // Clear the group background
                    m_grpOutsideDistanceMultiplier.backgroundSprite = "";
                    m_grpOutsideDistanceMultiplier.relativePosition = new Vector2(0, m_tabStripTransferReason.height + m_panelTabPanel.height + 12);
                    m_sliderOutsideDistanceMultiplier = SettingsSlider.Create(m_grpOutsideDistanceMultiplier.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideDistanceMultiplier"), UIFonts.Regular, fTEXT_SCALE, 420, 280, 0f, 10, 1f, 0f, 0, OnOutsideDistanceMultiplierChanged);
                    m_sliderOutsideDistanceMultiplier.SetTooltip(Localization.Get("sliderOutsideDistanceMultiplierTooltip"));
                    m_grpOutsideDistanceMultiplier.isVisible = false; // It seems to not be hidden correctly the first time so hide it here
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
                        // Update settings reason tabs
                        UpdateReasonTabVisibility(m_eBuildingType, buildingRules);

                        // check index is in range
                        int iRestrictionTabIndex = GetRestrictionTabIndex();
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
                            else if (IsWarehouse(m_eBuildingType))
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
                            m_chkAllowImport.isEnabled = currentRule.m_import;
                            m_chkAllowImport.isChecked = restrictionSettings.m_bAllowImport;
                            m_chkAllowExport.isEnabled = currentRule.m_export;
                            m_chkAllowExport.isChecked = restrictionSettings.m_bAllowExport;
                        }
                        else
                        {
                            m_panelImportExport.Hide();
                        }

                        // ------------------------------------------------------------------------
                        // Warehouse settings
                        if (IsWarehouse(m_eBuildingType))
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

                        // ------------------------------------------------------------------------
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


                        // Apply to all buttons
                        if (m_applyToAll is not null)
                        {
                            m_applyToAll.UpdatePanel();
                        }

                        // Update the panel height depending on building type.
                        if (IsOutsideConnection(m_buildingId))
                        {
                            m_panelTabPanel.height = m_panelImportExport.height + 20;
                            m_grpOutsideDistanceMultiplier.relativePosition = new Vector2(10, m_tabStripTransferReason.height + m_panelTabPanel.height + 12);
                        }
                        else
                        {
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

                        m_tabStripTransferReason.PerformLayout();

                        // ------------------------------------------------------------------------
                        if (DistrictSelectionPanel.IsVisible())
                        {
                            DistrictSelectionPanel.Instance.SetPanelBuilding(m_buildingId, GetRestrictionId());
                            DistrictSelectionPanel.Instance.InvalidatePanel();
                        }

                        UpdateDistrictButtonTooltip(true);
                        UpdateDistrictButtonTooltip(false);
                        UpdateDistrictButtonToggle();
                        UpdateScrollbarChanged();
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
                            CustomTransferReason.Reason actualTransferReason = BuildingTypeHelper.GetWarehouseActualTransferReason(m_buildingId);
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

        private void OnReasonTabChanged(int index)
        {
            if (m_bInSetup) { return; }

            m_iRestrictionTabIndex = index;

            // Close district panel
            if (DistrictSelectionPanel.IsVisible())
            {
                DistrictSelectionPanel.Instance.Hide();
            }

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
        public void OnServiceDistanceChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(GetRestrictionId());

            restrictions.m_iServiceDistanceMeters = (int)(Value * 1000.0f); // eg 3.5km = 3500

            settings.SetRestrictions(GetRestrictionId(), restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(true);
        }

        public void OnOutsideDistanceMultiplierChanged(float Value)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_iOutsideMultiplier = (int)Value;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            // Invalidate outside connection path cache now we have updated a modifier
            OutsideConnectionCache.Invalidate();

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

        public void OnWarehouseOverrideChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_bWarehouseOverride = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdateTab(true);
        }

        public void OnImprovedWarehouseMatchingChanged(bool bChecked)
        {
            if (m_bInSetup) { return; }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            settings.m_bImprovedWarehouseMatching = bChecked;
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

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
