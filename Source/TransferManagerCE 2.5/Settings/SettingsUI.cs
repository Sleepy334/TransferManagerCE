﻿using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    public class SettingsUI
    {
        const int iSEPARATOR_HEIGHT = 10;

        private UICheckBox? m_chkEnableTransferManager = null;
        
        // Outside connection tab
        private SettingsSlider? m_sliderShipMultiplier = null;
        private SettingsSlider? m_sliderPlaneMultiplier = null;
        private SettingsSlider? m_sliderTrainMultiplier = null;
        private SettingsSlider? m_sliderRoadMultiplier = null;
        private SettingsSlider? m_sliderExportVehicleLimitPercent = null;

        // Services tab
        private UICheckBox? m_chkPreferLocal = null;
        private UICheckBox? m_chkDeathcareExperimental = null;
        private UICheckBox? m_chkGarbageExperimental = null;
        private UICheckBox? m_chkPoliceExperimental = null;
        private UICheckBox? m_chkImprovedMailTransfers = null;

        // Collect sick
        private UICheckBox? m_chkOverrideSickCollection = null;
        private SettingsSlider? m_sliderSickWalkRate = null;
        private SettingsSlider? m_sliderSickHelicopterRate = null;
        private SettingsSlider? m_sliderSickGenerationRate = null;
        private UICheckBox? m_chkDisplaySickNotification = null;

        //Taxi Stand
        private UICheckBox? m_chkTaxiMove = null;
        private SettingsSlider? m_sliderTaxiStandDelay = null;

        // Warehouse
        private UICheckBox? m_chkFactoryFirst = null;
        private UICheckBox? m_chkOverrideGenericIndustriesHandler = null;

        private UICheckBox? m_chkWarehouseFirst = null;
        private SettingsSlider? m_sliderWarehouseReservePercent = null;
        private UICheckBox? m_chkImprovedWarehouseMatching = null;
        private UICheckBox? m_chkNewInterWarehouseMatching = null;

        // vehicle AI
        private UICheckBox? m_chkFireTruckAI = null;
        private UICheckBox? m_chkFireCopterAI = null;
        private UICheckBox? m_chkGarbageTruckAI = null;
        private UICheckBox? m_chkPoliceCarAI = null;
        private UICheckBox? m_chkPoliceCopterAI = null;

        // General tab
        private UICheckBox? m_chkEnablePathFailExclusion = null;
        private UIDropDown? m_dropdownPathDistanceServices = null;
        private UIDropDown? m_dropdownPathDistanceGoods = null;
        private SettingsSlider? m_sliderPathDistanceHeuristic = null;
        private SettingsSlider? m_sliderPathDistanceShift = null;
        private UIDropDown? m_dropdownBalanced = null;
        private UICheckBox? m_chkDisableDummyTraffic = null;
        private UICheckBox? m_chkApplyUnlimited = null;
        private UICheckBox? m_chkEmployOvereducatedWorkers = null;

        // Maintenance tab
        private UILabel? m_lblGhostVehicleCount = null;
        private UILabel? m_lblPathUnitCount = null;
        private UIDropDown? m_dropdownConnectionGraph = null;
        private UIDropDown? m_dropdownLogReason = null;
        private UIDropDown? m_dropdownCandidates = null;
        private UIButton? m_btnResetTransferManagerSettings = null;

        private Dictionary<CustomTransferReason.Reason, SettingsSlider> m_sliderLimits = new Dictionary<CustomTransferReason.Reason, SettingsSlider>();
        private Dictionary<CustomTransferReason.Reason, UICheckBox> m_chkImport = new Dictionary<CustomTransferReason.Reason, UICheckBox>();
        private Dictionary<CustomTransferReason.Reason, UICheckBox> m_chkImportWarehouses = new Dictionary<CustomTransferReason.Reason, UICheckBox>();

        private List<string> m_reasonNames = new List<string>();

        public SettingsUI()
        {
        }

        public void OnSettingsUI(UIHelper helper)
        {
            // Title
            UIComponent pnlMain = (UIComponent)helper.self;
            UILabel txtTitle = AddDescription(pnlMain, "title", pnlMain, 1.0f, TransferManagerMain.Title);
            txtTitle.textScale = 1.2f;

            // Add tabstrip.
            ExtUITabstrip tabStrip = ExtUITabstrip.Create(helper);
            tabStrip.eventVisibilityChanged += OnTabVisibilityChanged;
            UIHelper tabGeneral = tabStrip.AddTabPage(Localization.Get("tabGeneral"), true);
            UIHelper tabTransferManager = tabStrip.AddTabPage(Localization.Get("tabTransferManager"), true);
            UIHelper tabVehicleAI = tabStrip.AddTabPage(Localization.Get("tabVehicleAI"), true);
            UIHelper tabAdvanced = tabStrip.AddTabPage(Localization.Get("tabAdvanced"), true);
            UIHelper tabMaintenance = tabStrip.AddTabPage(Localization.Get("tabMaintenance"), true);

            // Setup tabs
            SetupGeneralTab(tabGeneral);
            SetupTransferManagerTab(tabTransferManager);
            SetupVehicleAITab(tabVehicleAI);
            SetupAdvanced(tabAdvanced);
            SetupMaintenance(tabMaintenance);

            UpdateTransferManagerEnabled();
        }

        public void SetupGeneralTab(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            UIHelper groupLocalisation = (UIHelper)helper.AddGroup(Localization.Get("GROUP_LOCALISATION"));
            groupLocalisation.AddDropdown(Localization.Get("dropdownLocalization"), Localization.GetLoadedLanguages(), Localization.GetLanguageIndexFromCode(oSettings.PreferredLanguage), OnLocalizationDropDownChanged);

            // Behaviour
            UIHelper groupVisuals = (UIHelper)helper.AddGroup(Localization.Get("GROUP_VISUALS"));
            groupVisuals.AddCheckbox(Localization.Get("optionEnablePanelTransparency"), oSettings.EnablePanelTransparency, OnEnablePanelTransparencyChanged);

            // Transfer Issue Panel
            UIHelper groupTransferIssue = (UIHelper)helper.AddGroup(Localization.Get("GROUP_TRANSFERISSUE_PANEL"));
            UIPanel panel = (UIPanel)groupTransferIssue.self;
            UIKeymappingsPanel keymappingsTransferIssue = panel.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsTransferIssue.AddKeymapping(Localization.Get("keyOpenTransferIssuePanel"), ModSettings.GetSettings().TransferIssueHotkey, OnShortcutKeyChanged);
            SettingsSlider.Create(groupTransferIssue, LayoutDirection.Horizontal, Localization.Get("sliderTransferIssueDeadTimerValue"), 1.0f, 400, 200, 0f, 255f, 1f, (float)oSettings.DeadTimerValue, 0, OnDeadValueChanged);
            SettingsSlider.Create(groupTransferIssue, LayoutDirection.Horizontal, Localization.Get("sliderTransferIssueSickTimerValue"), 1.0f, 400, 200, 0f, 255f, 1f, (float)oSettings.SickTimerValue, 0, OnSickValueChanged);
            SettingsSlider.Create(groupTransferIssue, LayoutDirection.Horizontal, Localization.Get("sliderTransferIssueGoodsTimerValue"), 1.0f, 400, 200, 0f, 255f, 1f, (float)oSettings.GoodsTimerValue, 0, OnGoodsValueChanged);

            // Building Panel
            UIHelper groupBuildingPanel = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BUILDING_PANEL"));
            UIPanel panelBuilding = (UIPanel)groupBuildingPanel.self;
            UIKeymappingsPanel keymappingsBuildingPanel = panelBuilding.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsBuildingPanel.AddKeymapping(Localization.Get("keyOpenBuildingPanel"), ModSettings.GetSettings().SelectionToolHotkey, OnShortcutKeyChanged); // Automatically saved

            // Statistics group
            UIHelper groupStats = (UIHelper)helper.AddGroup(Localization.Get("GROUP_STATISTICS_PANEL"));
            groupStats.AddCheckbox(Localization.Get("StatisticsPanelEnabled"), oSettings.StatisticsEnabled, OnStatisticsEnabledChanged);
            UIPanel panelStats = (UIPanel)groupStats.self;
            UIKeymappingsPanel keymappingsStatsPanel = panelStats.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsStatsPanel.AddKeymapping(Localization.Get("keyOpenStatisticsPanel"), ModSettings.GetSettings().StatsPanelHotkey, OnShortcutKeyChanged); // Automatically saved

            // Outside Connections group
            UIHelper groupOutside = (UIHelper)helper.AddGroup(Localization.Get("GROUP_OUTSIDE_CONNECTIONS_PANEL"));
            UIPanel panelOutside = (UIPanel)groupOutside.self;
            UIKeymappingsPanel keymappingsOutsidePanel = panelOutside.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsOutsidePanel.AddKeymapping(Localization.Get("keyOpenOutsidePanel"), ModSettings.GetSettings().OutsideConnectionPanelHotkey, OnShortcutKeyChanged); // Automatically saved
        }

        public void SetupTransferManagerTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            UIHelperBase group0 = helper.AddGroup(Localization.Get("DEBUGPROFILE"));
            m_chkEnableTransferManager = (UICheckBox)group0.AddCheckbox(Localization.Get("optionEnableNewTransferManager"), oSettings.EnableNewTransferManager, (index) => setOptionEnableNewTransferManager(index));

            UIScrollablePanel panelHelper = (UIScrollablePanel)helper.self;

            // Create a panel to sit the sub tabstrip on.
            UIPanel panelSubTab = panelHelper.AddUIComponent<UIPanel>();
            panelSubTab.autoLayout = true;
            panelSubTab.autoLayoutDirection = LayoutDirection.Vertical;
            panelSubTab.width = panelHelper.width;
            panelSubTab.height = panelHelper.height - 130;

            ExtUITabstrip tabStripTransferManager = ExtUITabstrip.Create(panelSubTab);
            UIPanel panel = (group0 as UIHelper).self as UIPanel;
            if (panel is not null)
            {
                tabStripTransferManager.eventVisibilityChanged += OnTabVisibilityChanged;
                UIHelper tabGeneral = tabStripTransferManager.AddTabPage(Localization.Get("tabGeneral"), true);
                UIHelper tabWarehouses = tabStripTransferManager.AddTabPage(Localization.Get("tabWarehouses"), true);
                UIHelper tabImportExport = tabStripTransferManager.AddTabPage(Localization.Get("tabImportExport"), true);
                UIHelper tabServices = tabStripTransferManager.AddTabPage(Localization.Get("tabServices"), true);
                UIHelper tabDistances = tabStripTransferManager.AddTabPage(Localization.Get("tabDistanceRestrictions"), true);

                SetupTransferGeneralTab(tabGeneral);
                SetupWarehousesTab(tabWarehouses);
                SetupImportExportTab(tabImportExport);
                SetupTransferServicesTab(tabServices);
                SetupTransferDistancesTab(tabDistances);
            }
        }

        public void SetupTransferGeneralTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Path Failure Exclusion
            UIHelper groupPathFailure = (UIHelper)helper.AddGroup(Localization.Get("GROUP_PATH_FAIL_EXCLUSION"));
            AddDescription(groupPathFailure, "txtPathFailExclusion", 1.0f, Localization.Get("txtPathFailExclusion"));
            m_chkEnablePathFailExclusion = (UICheckBox)groupPathFailure.AddCheckbox(Localization.Get("optionPathFailExclusion"), oSettings.EnablePathFailExclusion, OnPathFailExclusion);

            // Path distance
            UIHelper groupPathDistance = (UIHelper)helper.AddGroup(Localization.Get("GROUP_PATH_DISTANCE"));
            AddDescription(groupPathDistance, "txtPathDistance", 1.0f, Localization.Get("txtPathDistance"));
            // Balanced match mode setting
            string[] itemsPathDistance = {
                Localization.Get("dropdownPathDistanceLOS"),
                Localization.Get("dropdownPathDistanceConnectedLOS"),
                Localization.Get("dropdownPathDistance"),
            };
            m_dropdownPathDistanceServices = (UIDropDown)groupPathDistance.AddDropdown(Localization.Get("dropdownPathDistanceAlgorithmServices"), itemsPathDistance, (int)oSettings.PathDistanceServices, OnPathDistanceServices);
            m_dropdownPathDistanceServices.width = 400;
            m_dropdownPathDistanceGoods = (UIDropDown)groupPathDistance.AddDropdown(Localization.Get("dropdownPathDistanceAlgorithmGoods"), itemsPathDistance, (int)oSettings.PathDistanceGoods, OnPathDistanceGoods);
            m_dropdownPathDistanceGoods.width = 400;

            groupPathDistance.AddSpace(iSEPARATOR_HEIGHT);

            // Accuracy slider
            AddDescription(groupPathDistance, "txtPathDistanceHeuristic", 1.0f, Localization.Get("txtPathDistanceHeuristic"));
            m_sliderPathDistanceHeuristic = SettingsSlider.Create(groupPathDistance, LayoutDirection.Horizontal, Localization.Get("sliderPathDistanceHeuristic"), 1.0f, 400, 200, 0f, 100f, 1f, (float)oSettings.PathDistanceHeuristic, 0, OnPathDistanceHeuristicChanged);
            m_sliderPathDistanceHeuristic.Percent = true;
            AddDescription(groupPathDistance, "txtPathDistanceHeuristicKey", 1.0f, Localization.Get("txtPathDistanceHeuristicKey"));

            groupPathDistance.AddSpace(iSEPARATOR_HEIGHT);

            // Travel Time shift
            AddDescription(groupPathDistance, "txtPathDistanceShift", 1.0f, Localization.Get("txtPathDistanceShift"));
            m_sliderPathDistanceShift = SettingsSlider.Create(groupPathDistance, LayoutDirection.Horizontal, Localization.Get("sliderPathDistanceShift"), 1.0f, 400, 200, 1000f, 20000f, 100f, (float)oSettings.PathDistanceTravelTimeBaseValue, 0, OnPathDistanceShiftChanged);
           
            // Balanced match mode setting
            string[] itemsBalancedMode = {
                Localization.Get("dropdownBalancedModeIncomingFirst"),
                Localization.Get("dropdownBalancedModeLeastFirst"),
                Localization.Get("dropdownBalancedModePassiveFirst"),
            };

            UIHelper groupBalanced = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BALANCED_MATCH_MODE"));
            m_dropdownBalanced = (UIDropDown)groupBalanced.AddDropdown(Localization.Get("dropdownBalancedTitle"), itemsBalancedMode, (int)oSettings.BalancedMatchMode, OnBalancedMatchModeChanged);
            m_dropdownBalanced.width = 400;
            AddDescription(groupBalanced, "txtBalancedMatch", 1.0f, Localization.Get("txtBalancedMatch"));

            // Unlimited flag
            UIHelper groupUnlimited = (UIHelper)helper.AddGroup(Localization.Get("GROUP_UNLIMITED"));
            AddDescription(groupUnlimited, "txtApplyUnlimited", 1.0f, Localization.Get("txtApplyUnlimited"));
            m_chkApplyUnlimited = (UICheckBox)groupUnlimited.AddCheckbox(Localization.Get("optionApplyUnlimited"), oSettings.ApplyUnlimited, (bCheck) => SaveGameSettings.GetSettings().ApplyUnlimited = bCheck);
            groupUnlimited.AddSpace(iSEPARATOR_HEIGHT);
            AddDescription(groupUnlimited, "txtApplyUnlimitedWarning", 1.0f, Localization.Get("txtApplyUnlimitedWarning"));

            // Dummy traffic
            UIHelper groupDummyTraffic = (UIHelper)helper.AddGroup(Localization.Get("GROUP_DUMMY_TRAFFIC"));
            AddDescription(groupDummyTraffic, "txtDummyTraffic", 1.0f, Localization.Get("txtDummyTraffic"));
            m_chkDisableDummyTraffic = (UICheckBox)groupDummyTraffic.AddCheckbox(Localization.Get("optionDummyTraffic"), oSettings.DisableDummyTraffic, OnDisableDummyTraffic);

            // Employ over-educated workers
            UIHelper groupOveredcuatedWorkers = (UIHelper)helper.AddGroup(Localization.Get("GROUP_OVEREDUCATED_WORKERS"));
            AddDescription(groupOveredcuatedWorkers, "txtEmployOvereducatedWorkers", 1.0f, Localization.Get("txtEmployOvereducatedWorkers"));
            m_chkEmployOvereducatedWorkers = (UICheckBox)groupOveredcuatedWorkers.AddCheckbox(Localization.Get("optionEmployOverEducatedWorkers"), oSettings.EmployOverEducatedWorkers, OnEmployOvereducatedWOrkers);

        }

        public void SetupWarehousesTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // FACTORY GROUP
            UIHelper groupFactory = (UIHelper)helper.AddGroup(Localization.Get("GROUP_FACTORY_OPTIONS"));
            UIPanel panelFactory = (groupFactory as UIHelper).self as UIPanel;

            // Factory First
            AddDescription(panelFactory, "optionFactoryFirstText", panelFactory, 1.0f, Localization.Get("optionFactoryFirstText"));
            m_chkFactoryFirst = (UICheckBox)groupFactory.AddCheckbox(Localization.Get("optionFactoryFirst"), oSettings.FactoryFirst, (index) => setOptionFactoryFirst(index));
            groupFactory.AddSpace(iSEPARATOR_HEIGHT);

            // Override generic industries handler
            AddDescription(panelFactory, "txtOverrideGenericIndustriesHandler", panelFactory, 1.0f, Localization.Get("txtOverrideGenericIndustriesHandler"));
            m_chkOverrideGenericIndustriesHandler = (UICheckBox)groupFactory.AddCheckbox(Localization.Get("optionOverrideGenericIndustriesHandler"), oSettings.OverrideGenericIndustriesHandler, OnOverrideGenericIndustriesHandlerChanged);

            // WAREHOUSE GROUP
            UIHelper groupWarehouse = (UIHelper) helper.AddGroup(Localization.Get("GROUP_WAREHOUSE_OPTIONS"));
            UIPanel panelGroupWarehouse = (groupWarehouse as UIHelper).self as UIPanel;

            // Warehouse first
            AddDescription(panelGroupWarehouse, "optionWarehouseFirst_txt", panelGroupWarehouse, 1.0f, Localization.Get("optionWarehouseFirst_txt"));
            m_chkWarehouseFirst = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionWarehouseFirst"), oSettings.WarehouseFirst, (index) => setOptionWarehouseFirst(index));
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // Reserve trucks
            AddDescription(panelGroupWarehouse, "txtNewWarehouseReserveTrucks", panelGroupWarehouse, 1.0f, Localization.Get("txtNewWarehouseReserveTrucks"));
            m_sliderWarehouseReservePercent = SettingsSlider.Create(groupWarehouse, LayoutDirection.Horizontal, Localization.Get("sliderWarehouseReservePercent"), 1.0f, 400, 200, 0f, 100f, 5f, (float)oSettings.WarehouseReserveTrucksPercent, 0, OnWarehouseFirstPercentChanged);
            m_sliderWarehouseReservePercent.Percent = true;
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // Improved Warehouse Matching
            AddDescription(panelGroupWarehouse, "txtImprovedWarehouseMatching", panelGroupWarehouse, 1.0f, Localization.Get("txtImprovedWarehouseMatching"));
            m_chkImprovedWarehouseMatching = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionImprovedWarehouseMatching"), oSettings.ImprovedWarehouseMatching, (index) => setOptionImprovedWarehouseMatching(index));
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // New warehouse matching
            AddDescription(panelGroupWarehouse, "txtNewInterWarehouseMatching", panelGroupWarehouse, 1.0f, Localization.Get("txtNewInterWarehouseMatching"));
            m_chkNewInterWarehouseMatching = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionNewWarehouseTransfer"), oSettings.NewInterWarehouseTransfer, OnNewWarehouseTransferChanged);
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);
        }

        public void SetupImportExportTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Outside Multipliers
            UIHelper groupImportExport = (UIHelper) helper.AddGroup(Localization.Get("GROUP_EXPORTIMPORT_OPTIONS"));
            UIPanel txtPanel3 = groupImportExport.self as UIPanel;
            AddDescription(txtPanel3, "OutsideMultiplierDescription1", txtPanel3, 1.0f, Localization.Get("OutsideMultiplierDescription1"));
            m_sliderShipMultiplier = SettingsSlider.Create(groupImportExport, LayoutDirection.Horizontal, Localization.Get("sliderShipMultiplier"), 1.0f, 400, 200, 1f, 10f, 1f, (float)oSettings.OutsideShipMultiplier, 0, OnOutsideShipMultiplier);
            m_sliderPlaneMultiplier = SettingsSlider.Create(groupImportExport, LayoutDirection.Horizontal, Localization.Get("sliderPlaneMultiplier"), 1.0f, 400, 200, 1f, 10f, 1f, (float)oSettings.OutsidePlaneMultiplier, 0, OnOutsidePlaneMultiplier);
            m_sliderTrainMultiplier = SettingsSlider.Create(groupImportExport, LayoutDirection.Horizontal, Localization.Get("sliderTrainMultiplier"), 1.0f, 400, 200, 1f, 10f, 1f, (float)oSettings.OutsideTrainMultiplier, 0, OnOutsideTrainMultiplier);
            m_sliderRoadMultiplier = SettingsSlider.Create(groupImportExport, LayoutDirection.Horizontal, Localization.Get("sliderRoadMultiplier"), 1.0f, 400, 200, 1f, 10f, 1f, (float)oSettings.OutsideRoadMultiplier, 0, OnOutsideRoadMultiplier);
            AddDescription(txtPanel3, "OutsideMultiplierDescription2", txtPanel3, 1.0f, Localization.Get("OutsideMultiplierDescription2"));
            AddDescription(txtPanel3, "OutsideMultiplierDescription3", txtPanel3, 1.0f, Localization.Get("OutsideMultiplierDescription3"));

            // Export vehicle limit
            UIHelper groupExportLimits = (UIHelper)helper.AddGroup(Localization.Get("GROUP_EXPORT_LIMITS"));
            UIPanel panelExportLimit = groupExportLimits.self as UIPanel;
            m_sliderExportVehicleLimitPercent = SettingsSlider.Create(groupExportLimits, LayoutDirection.Horizontal, Localization.Get("sliderExportVehicleLimit"), 1.0f, 400, 200, 0f, 100f, 1f, (float)oSettings.ExportVehicleLimit, 0, OnExportVehicleLimit);
            m_sliderExportVehicleLimitPercent.Percent = true;
            AddDescription(panelExportLimit, "txtExportVehicleLimit", panelExportLimit, 1.0f, Localization.Get("txtExportVehicleLimit"));

            // Import restrictions
            UIHelperBase groupImportRestrict = helper.AddGroup(Localization.Get("GROUP_IMPORT_RESTRICTIONS"));
            UIPanel panelImportRestrictions = (groupImportRestrict as UIHelper).self as UIPanel;
            AddDescription(panelImportRestrictions, "txtImportRestrictions", panelImportRestrictions, 1.0f, Localization.Get("txtImportRestrictions"));

            UIPanel paneRestrictionsHeadings = panelImportRestrictions.AddUIComponent<UIPanel>();
            paneRestrictionsHeadings.autoLayout = true;
            paneRestrictionsHeadings.autoLayoutDirection = LayoutDirection.Horizontal;
            paneRestrictionsHeadings.width = panelImportRestrictions.width;
            paneRestrictionsHeadings.height = 20;
            UILabel label = paneRestrictionsHeadings.AddUIComponent<UILabel>();
            label.autoSize = false;
            label.width = 300;
            label.height = 20;
            label.text = "Buildings";
            UILabel label2 = paneRestrictionsHeadings.AddUIComponent<UILabel>();
            label2.autoSize = false;
            label2.width = 300;
            label2.height = 20;
            label2.text = "Warehouses";

            for (int i = 0; i < (int)CustomTransferReason.iLAST_REASON; ++i)
            {
                CustomTransferReason.Reason material = (CustomTransferReason.Reason)i;
                if (TransferManagerModes.IsImportRestrictionsSupported(material))
                {
                    UIPanel panelMaterialRestrictions = panelImportRestrictions.AddUIComponent<UIPanel>();
                    panelMaterialRestrictions.autoLayout = true;
                    panelMaterialRestrictions.autoLayoutDirection = LayoutDirection.Horizontal;
                    panelMaterialRestrictions.width = panelImportRestrictions.width;
                    panelMaterialRestrictions.height = 20;

                    UICheckBox? chkMaterial = UIUtils.AddCheckbox(panelMaterialRestrictions, material.ToString(), 1.0f, !oSettings.IsImportRestricted(material), (index) => OnImportRestrictMaterial(material, index));
                    if (chkMaterial is not null)
                    {
                        chkMaterial.width = 300;
                        m_chkImport.Add(material, chkMaterial);
                    }

                    if (TransferManagerModes.IsWarehouseMaterial(material))
                    {
                        UICheckBox? chkWarehouseMaterial = UIUtils.AddCheckbox(panelMaterialRestrictions, material.ToString(), 1.0f, !oSettings.IsImportRestricted(material), (index) => OnImportRestrictMaterialWarehouses(material, index));
                        if (chkWarehouseMaterial is not null)
                        {
                            chkWarehouseMaterial.width = 300;
                            m_chkImportWarehouses.Add(material, chkWarehouseMaterial);
                        }
                    }
                }
            }  
        }

        public void SetupTransferServicesTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Experimental services options
            UIHelperBase groupExperimental = helper.AddGroup(Localization.Get("GROUP_IMPROVED_SERVICES_MATCHING"));
            UIPanel panelExperimental = (groupExperimental as UIHelper).self as UIPanel;
            UILabel txtDeathcareExperimental = AddDescription(panelExperimental, "txtDeathcareExperimental", panelExperimental, 1.0f, Localization.Get("txtDeathcareExperimental"));
            m_chkDeathcareExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionDeathcareExperimental"), oSettings.ImprovedDeathcareMatching, OnExperimentalDeathcare);
            m_chkGarbageExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionGarbageExperimental"), oSettings.ImprovedGarbageMatching, OnExperimentalGarbage);
            m_chkPoliceExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionPoliceExperimental"), oSettings.ImprovedCrimeMatching, OnExperimentalCrime);
            m_chkImprovedMailTransfers = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionImprovedMailTransfers"), oSettings.ImprovedMailTransfers, OnImprovedMailMatching);

            // Sick Collection
            UIHelper groupSick = (UIHelper)helper.AddGroup(Localization.Get("GROUP_SICK_COLLECTION"));
            AddDescription(groupSick, "txtOverrideSickHandler", 1.0f, Localization.Get("txtOverrideSickHandler"));
            m_chkOverrideSickCollection = (UICheckBox)groupSick.AddCheckbox(Localization.Get("optionOverrideSickHandler"), oSettings.OverrideSickHandler, OnOverrideResidentialSick);
            groupSick.AddSpace(iSEPARATOR_HEIGHT);
            
            AddDescription(groupSick, "txtSickSadNotification", 1.0f, Localization.Get("txtSickSadNotification"));
            m_chkDisplaySickNotification = (UICheckBox)groupSick.AddCheckbox(Localization.Get("optionDisplaySickNotification"), oSettings.DisplaySickNotification, OnDisplaySadNotification);
            groupSick.AddSpace(iSEPARATOR_HEIGHT);

            AddDescription(groupSick, "txtSickHelicopterRate", 1.0f, Localization.Get("txtSickHelicopterRate"));
            m_sliderSickHelicopterRate = SettingsSlider.Create(groupSick, LayoutDirection.Horizontal, Localization.Get("optionSickHelicopterRate"), 1.0f, 400, 200, 0f, 100f, 1.0f, (float)oSettings.SickHelicopterRate, 0, OnSickHelicopterRate);
            m_sliderSickHelicopterRate.Percent = true;

            AddDescription(groupSick, "txtSickWalkRate", 1.0f, Localization.Get("txtSickWalkRate"));
            m_sliderSickWalkRate = SettingsSlider.Create(groupSick, LayoutDirection.Horizontal, Localization.Get("optionSickWalkRate"), 1.0f, 400, 200, 0f, 100f, 1.0f, (float)oSettings.SickWalkRate, 0, OnSickWalkRate);
            m_sliderSickWalkRate.Percent = true;

            // Sick Generation
            UIHelper groupSickGeneration = (UIHelper)helper.AddGroup(Localization.Get("GROUP_SICK_GENERATE")); 
            AddDescription(groupSickGeneration, "txtRandomSick", 1.0f, Localization.Get("txtRandomSick"));
            m_sliderSickGenerationRate = SettingsSlider.Create(groupSickGeneration, LayoutDirection.Horizontal, Localization.Get("txtRandomSickRate"), 1.0f, 400, 200, 0f, 10000f, 100.0f, (float)oSettings.RandomSickRate, 0, OnRandomSickRate);
            AddDescription(groupSickGeneration, "txtRandomSickRateScale", 1.0f, Localization.Get("txtRandomSickRateScale"));

            // Taxi Move
            UIHelper groupTaxiMove = (UIHelper)helper.AddGroup(Localization.Get("GROUP_TAXI_MOVE"));
            AddDescription(groupTaxiMove, "txtTaxiMove", 1.0f, Localization.Get("txtTaxiMove"));
            m_chkTaxiMove = (UICheckBox)groupTaxiMove.AddCheckbox(Localization.Get("optionTaxiMove"), oSettings.TaxiMove, OnTaxiMove);
            groupTaxiMove.AddSpace(iSEPARATOR_HEIGHT);
            AddDescription(groupTaxiMove, "txtTaxiStandDelay", 1.0f, Localization.Get("txtTaxiStandDelay"));
            m_sliderTaxiStandDelay = SettingsSlider.Create(groupTaxiMove, LayoutDirection.Horizontal, Localization.Get("sliderTaxiStandDelay"), 1.0f, 400, 200, 0f, 20f, 1.0f, (float)oSettings.TaxiStandDelay, 0, OnTaxiStandDelay);

            // Prefer local
            UIHelperBase group1 = helper.AddGroup(Localization.Get("GROUP_SERVICE_DISTRICT_OPTIONS"));
            UIPanel txtPanel1 = (group1 as UIHelper).self as UIPanel;
            AddDescription(txtPanel1, "txtPreferLocalService", txtPanel1, 1.0f, Localization.Get("txtPreferLocalService"));
            m_chkPreferLocal = (UICheckBox)group1.AddCheckbox(Localization.Get("optionPreferLocalService"), oSettings.PreferLocalService, (index) => setOptionPreferLocalService(index));
            group1.AddSpace(iSEPARATOR_HEIGHT);
            AddDescription(txtPanel1, "txtPreferLocalServiceWarning", txtPanel1, 1.0f, Localization.Get("txtPreferLocalServiceWarning"));
        }

        public void SetupTransferDistancesTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Distance limits section
            UIHelper groupDistanceLimits = (UIHelper)helper.AddGroup(Localization.Get("GROUP_DISTANCE_LIMITS"));
            UIPanel panelDistanceLimits = (groupDistanceLimits as UIHelper).self as UIPanel;
            AddDescription(panelDistanceLimits, "txtDistanceLimits", panelDistanceLimits, 1.0f, Localization.Get("txtDistanceLimits"));
            groupDistanceLimits.AddSpace(iSEPARATOR_HEIGHT);

            // Services
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Cash);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Crime);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Dead);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Fire);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Garbage);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Mail);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.ParkMaintenance);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.RoadMaintenance);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Sick);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Snow);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Taxi);
            groupDistanceLimits.AddSpace(iSEPARATOR_HEIGHT);

            // School
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.StudentES);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.StudentHS);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.StudentUni);
            groupDistanceLimits.AddSpace(iSEPARATOR_HEIGHT);

            // Workers
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Worker0);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Worker1);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Worker2);
            AddDistanceSlider(groupDistanceLimits, CustomTransferReason.Reason.Worker3);
            groupDistanceLimits.AddSpace(iSEPARATOR_HEIGHT);
        }

        private void AddDistanceSlider(UIHelper helper, CustomTransferReason.Reason reason)
        {
            AddDistanceSlider(helper, reason, $"{reason} (km)");
        }

        private void AddDistanceSlider(UIHelper helper, CustomTransferReason.Reason reason, string strLabel)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            m_sliderLimits[reason] = SettingsSlider.Create(helper, LayoutDirection.Horizontal, strLabel, 1.0f, 400, 200, 0f, 20f, 0.5f, (float)oSettings.GetActiveDistanceRestrictionKm(reason), 1, (float value) => OnDistanceLimit(reason, value));
        }

        public void SetupVehicleAITab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Experimental section
            UIHelperBase group = helper.AddGroup(Localization.Get("GROUP_VEHICLE_AI"));
            UIPanel panel = (group as UIHelper).self as UIPanel;
            AddDescription(panel, "txtVehicleAIDescription", panel, 1.0f, Localization.Get("txtVehicleAIDescription"));
            m_chkFireTruckAI = (UICheckBox)group.AddCheckbox(Localization.Get("optionFireTruckAI"), oSettings.FireTruckAI, OnFireTruckAI);
            m_chkFireCopterAI = (UICheckBox)group.AddCheckbox(Localization.Get("optionFireCopterAI"), oSettings.FireCopterAI, OnFireCopterAI);
            m_chkGarbageTruckAI = (UICheckBox)group.AddCheckbox(Localization.Get("optionGarbageTruckAI"), oSettings.GarbageTruckAI, OnGarbageTruckAI);
            m_chkPoliceCarAI = (UICheckBox)group.AddCheckbox(Localization.Get("optionPoliceCarAI"), oSettings.PoliceCarAI, OnPoliceCarAI);
            m_chkPoliceCopterAI = (UICheckBox)group.AddCheckbox(Localization.Get("optionPoliceCopterAI"), oSettings.PoliceCopterAI, OnPoliceCopterAI);
        }

        public void SetupAdvanced(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Experimental section
            UIHelperBase group = helper.AddGroup(Localization.Get("GROUP_PATCHES"));
            UIPanel panel = (group as UIHelper).self as UIPanel;
            AddDescription(panel, "txtAdvancedDescription", panel, 1.0f, Localization.Get("txtAdvancedDescription"));

            UIHelper groupGeneral = (UIHelper)helper.AddGroup(Localization.Get("tabGeneral"));
            groupGeneral.AddCheckbox(Localization.Get("optionFixFindHospital"), oSettings.FixFindHospital, (bChecked) => { oSettings.FixFindHospital = bChecked; oSettings.Save(); });
            AddDescription((groupGeneral as UIHelper).self as UIPanel, "txtFindHospital", panel, 1.0f, Localization.Get("txtFindHospital"));

            UIHelper groupIntercityStops = (UIHelper) helper.AddGroup(Localization.Get("GROUP_INTERCITY_STOPS"));
            AddDescription((groupIntercityStops as UIHelper).self as UIPanel, "txtIntercityStopSpawnAtCount", panel, 1.0f, Localization.Get("txtIntercityStopSpawnAtCount"));
            groupIntercityStops.AddSpace(6);
            SettingsSlider sliderForceTrainSpawnAtCount = SettingsSlider.Create(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForceTrainSpawnAtCount"), 1.0f, 320, 300, 0f, 500f, 1f, (float)oSettings.ForceTrainSpawnAtCount, 0, (float value) => { oSettings.ForceTrainSpawnAtCount = (int)value; oSettings.Save(); });
            SettingsSlider sliderForceShipSpawnAtCount = SettingsSlider.Create(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForceShipSpawnAtCount"), 1.0f, 320, 300, 0f, 500f, 1f, (float)oSettings.ForceShipSpawnAtCount, 0, (float value) => { oSettings.ForceShipSpawnAtCount = (int)value; oSettings.Save(); });
            SettingsSlider sliderForcePlaneSpawnAtCount = SettingsSlider.Create(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForcePlaneSpawnAtCount"), 1.0f, 320, 300, 0f, 500f, 1f, (float)oSettings.ForcePlaneSpawnAtCount, 0, (float value) => { oSettings.ForcePlaneSpawnAtCount = (int)value; oSettings.Save(); });
            SettingsSlider sliderForceBusSpawnAtCount = SettingsSlider.Create(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForceBusSpawnAtCount"), 1.0f, 320, 300, 0f, 500f, 1f, (float)oSettings.ForceBusSpawnAtCount, 0, (float value) => { oSettings.ForceBusSpawnAtCount = (int)value; oSettings.Save(); });
            
            // Reset button
            groupIntercityStops.AddButton(Localization.Get("btnOutsideReset"), () =>
            {
                ModSettings defaultSettings = new ModSettings();
                sliderForceTrainSpawnAtCount.SetValue(defaultSettings.ForceTrainSpawnAtCount);
                sliderForceShipSpawnAtCount.SetValue(defaultSettings.ForceShipSpawnAtCount);
                sliderForcePlaneSpawnAtCount.SetValue(defaultSettings.ForcePlaneSpawnAtCount);
                sliderForceBusSpawnAtCount.SetValue(defaultSettings.ForceBusSpawnAtCount);
            });

            UIHelperBase groupSpawnPatches = helper.AddGroup(Localization.Get("GROUP_SPAWN_PATCHES"));
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForceCargoShipSpawn"), oSettings.ForceCargoShipSpawn, (bChecked) => { oSettings.ForceCargoShipSpawn = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForcePassengerShipSpawn"), oSettings.ForcePassengerShipSpawn, (bChecked) => { oSettings.ForcePassengerShipSpawn = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForceCargoPlaneSpawn"), oSettings.ForceCargoPlaneSpawn, (bChecked) => { oSettings.ForceCargoPlaneSpawn = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForcePassengerPlaneSpawn"), oSettings.ForcePassengerPlaneSpawn, (bChecked) => { oSettings.ForcePassengerPlaneSpawn = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForceAirportGateToSpawn"), oSettings.ForcePassengerPlaneSpawnAtGate, (bChecked) => { oSettings.ForcePassengerPlaneSpawnAtGate = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddSpace(6);
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForceCargoTrainDespawnOutsideConnections"), oSettings.ForceCargoTrainDespawnOutsideConnections, (bChecked) => { oSettings.ForceCargoTrainDespawnOutsideConnections = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForceCargoShipDespawnOutsideConnections"), oSettings.ForceCargoShipDespawnOutsideConnections, (bChecked) => { oSettings.ForceCargoShipDespawnOutsideConnections = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionForceCargoPlaneDespawnOutsideConnections"), oSettings.ForceCargoPlaneDespawnOutsideConnections, (bChecked) => { oSettings.ForceCargoPlaneDespawnOutsideConnections = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddSpace(6);
            groupSpawnPatches.AddCheckbox(Localization.Get("optionResetStopMaxWaitTimeWhenNoPasengers"), oSettings.ResetStopMaxWaitTimeWhenNoPasengers, (bChecked) => { oSettings.ResetStopMaxWaitTimeWhenNoPasengers = bChecked; oSettings.Save(); });
            groupSpawnPatches.AddCheckbox(Localization.Get("optionFixCargoTrucksDisappearingOutsideConnections"), oSettings.ForceCargoPlaneSpawn, (bChecked) => { oSettings.FixCargoTrucksDisappearingOutsideConnections = bChecked; oSettings.Save(); });

            UIHelperBase groupVehiclePatches = helper.AddGroup(Localization.Get("GROUP_VEHICLE_PATCHES"));
            groupVehiclePatches.AddCheckbox(Localization.Get("optionFixBankVansStuckCargoStations"), oSettings.FixBankVansStuckCargoStations, (bChecked) => { oSettings.FixBankVansStuckCargoStations = bChecked; oSettings.Save(); });
            groupVehiclePatches.AddCheckbox(Localization.Get("optionFixPostVansStuckCargoStations"), oSettings.FixPostVansStuckCargoStations, (bChecked) => { oSettings.FixPostVansStuckCargoStations = bChecked; oSettings.Save(); });
            groupVehiclePatches.AddCheckbox(Localization.Get("optionFixPostTruckCollectingMail"), oSettings.FixPostTruckCollectingMail, (bChecked) => { oSettings.FixPostTruckCollectingMail = bChecked; oSettings.Save(); });

            UIHelperBase groupWarehousePatches = helper.AddGroup(Localization.Get("GROUP_WAREHOUSE_PATCHES"));
            groupWarehousePatches.AddCheckbox(Localization.Get("optionFixFishWarehouses"), oSettings.FixFishWarehouses, (bChecked) => { oSettings.FixFishWarehouses = bChecked; oSettings.Save();});
            groupWarehousePatches.AddCheckbox(Localization.Get("optionFixCargoWarehouseAccessSegment"), oSettings.FixCargoWarehouseAccessSegment, (bChecked) => { oSettings.FixCargoWarehouseAccessSegment = bChecked; oSettings.Save(); });
            groupWarehousePatches.AddCheckbox(Localization.Get("optionFixCargoWarehouseExcludeFlag"), oSettings.FixCargoWarehouseExcludeFlag, (bChecked) => { oSettings.FixCargoWarehouseExcludeFlag = bChecked; oSettings.Save(); });
            groupWarehousePatches.AddCheckbox(Localization.Get("optionFixCargoWarehouseOfferRatio"), oSettings.FixCargoWarehouseOfferRatio, (bChecked) => { oSettings.FixCargoWarehouseOfferRatio = bChecked; oSettings.Save(); });
            groupWarehousePatches.AddCheckbox(Localization.Get("optionRemoveEmptyWarehouseLimit"), oSettings.RemoveEmptyWarehouseLimit, (bChecked) => { oSettings.RemoveEmptyWarehouseLimit = bChecked; oSettings.Save(); });
        }

        public void SetupMaintenance(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            ModSettings oModSettings = ModSettings.GetSettings();

            // Maintenance section
            UIHelperBase groupMaintenance = helper.AddGroup(Localization.Get("GROUP_Maintenance"));
            UIPanel panelMaintenance = (groupMaintenance as UIHelper).self as UIPanel;

            // Release Broken pathing
            AddDescription(panelMaintenance, "txtBrokenPathUnits", panelMaintenance, 1.0f, Localization.Get("txtBrokenPathUnits"));
            groupMaintenance.AddButton(Localization.Get("btnReleaseBrokenPathing"), () =>
            {
                int iReleased = PathUnitMaintenance.ReleaseBrokenPathUnits();
                if (m_lblPathUnitCount is not null)
                {
                    m_lblPathUnitCount.text = Localization.Get("txtPathUnitCount") + ": " + iReleased;
                }
            });
            m_lblPathUnitCount = AddDescription(panelMaintenance, "txtPathUnitCount", panelMaintenance, 1.0f, Localization.Get("txtPathUnitCount") + ": 0");

            groupMaintenance.AddSpace(iSEPARATOR_HEIGHT);

            // Release Ghost vehicles
            AddDescription(panelMaintenance, "txtReleaseGhostVehicles", panelMaintenance, 1.0f, Localization.Get("txtReleaseGhostVehicles"));
            groupMaintenance.AddButton(Localization.Get("btnReleaseGhostVehicles"), () => 
            {
                int iReleased = StuckVehicles.ReleaseGhostVehicles();
                if (m_lblGhostVehicleCount is not null)
                {
                    m_lblGhostVehicleCount.text = Localization.Get("txtGhostVehiclesCount") + ": " + iReleased;
                }
            });
            m_lblGhostVehicleCount = AddDescription(panelMaintenance, "txtGhostVehiclesCount", panelMaintenance, 1.0f, Localization.Get("txtGhostVehiclesCount") + ": 0");
            
            // Match Set Logging
            UIHelper groupLogging = (UIHelper)helper.AddGroup(Localization.Get("GROUP_MAINTENANCE_LOGGING"));

            // Log Material Dropdown
            LoadReasons();
            int iCurrentReason = GetReasonArrayIndex(oModSettings.MatchLogReason);
            AddDescription(groupLogging, "txtMatchLogging", 1.0f, Localization.Get("txtMatchLogging"));
            m_dropdownLogReason = (UIDropDown)groupLogging.AddDropdown(Localization.Get("dropdownLoggingReason"), m_reasonNames.ToArray(), iCurrentReason, OnLogReasonChanged);
            m_dropdownLogReason.width = 400;

            // Candidate logging
            string[] itemsCandidates = {
                Localization.Get("dropdownAllCandidates"),
                Localization.Get("dropdownValidCandidates"),
                Localization.Get("dropdownExcludedCandidates"),
                Localization.Get("dropdownNoCandidates"),
            };
            m_dropdownCandidates = (UIDropDown)groupLogging.AddDropdown(Localization.Get("dropdownCandidates"), itemsCandidates, ModSettings.GetSettings().MatchLogCandidates, OnLogCandidatesChanged);
            m_dropdownCandidates.width = 400;

            // Show id's in building title
            groupLogging.AddCheckbox(Localization.Get("optionShowBuildingId"), oModSettings.ShowBuildingId, OnShowBuildingId);

            // Log file path
            groupLogging.AddSpace(iSEPARATOR_HEIGHT);
            AddDescription(groupLogging, "txtMatchLoggingPath", 1.0f, Localization.Get("txtMatchLoggingPath") + " " + Path.Combine(ModSettings.UserSettingsDir, "TransferManagerCE"));

            // Pathing connection graph
            string[] itemsConnectionGraph = {
                Localization.Get("dropdownConnectionGraphNone"),
                Localization.Get("dropdownConnectionGraphGoods"),
                Localization.Get("dropdownConnectionGraphPedestrianZoneServices"),
                Localization.Get("dropdownConnectionGraphOtherServices"),
            };
            UIHelper groupPathing = (UIHelper)helper.AddGroup(Localization.Get("GROUP_MAINTENANCE_PATHING"));
            m_dropdownConnectionGraph = (UIDropDown)groupPathing.AddDropdown(Localization.Get("dropdownConnectionGraph"), itemsConnectionGraph, oModSettings.ShowConnectionGraph, OnShowConnectionGraphChanged);
            m_dropdownConnectionGraph.width = 400;
            AddDescription(groupPathing, "txtShowConnectionGraph", 1.0f, Localization.Get("txtShowConnectionGraph"));
            groupPathing.AddSpace(iSEPARATOR_HEIGHT); 
            groupPathing.AddCheckbox(Localization.Get("optionLogCitizenPathFailures"), oModSettings.LogCitizenPathFailures, OnLogCitizenPathFailuresChanged);
            AddDescription(groupPathing, "txtLogCitizenPathFailureWarning", 1.0f, Localization.Get("txtLogCitizenPathFailureWarning"));

            UIHelperBase groupTransferManager = helper.AddGroup(Localization.Get("tabTransferManager"));

            // Reset settings
            m_btnResetTransferManagerSettings = (UIButton)groupTransferManager.AddButton(Localization.Get("btnResetTransferManagerSettings"), OnResetTransferManagerSettingsClicked);
            groupTransferManager.AddSpace(iSEPARATOR_HEIGHT);

            // Reset pathing
            groupTransferManager.AddButton(Localization.Get("btnResetPathingStatistics"), OnResetPathingStatisticsClicked);
            groupTransferManager.AddSpace(iSEPARATOR_HEIGHT);

            // Reset statistics
            groupTransferManager.AddButton(Localization.Get("buttonResetTransferStatistics"), OnResetTransferStatisticsClicked);
        }

        public void OnTabVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                UpdateTransferManagerSettings();
            }
        }

        /* 
         * Code adapted from PropAnarchy under MIT license
         */
        private static readonly Color32 m_greyColor = new Color32(0xe6, 0xe6, 0xe6, 0xee);
        private static UILabel AddDescription(UIHelper parent, string name, float fontScale, string text)
        {
            return AddDescription(parent.self as UIPanel, name, parent.self as UIPanel, fontScale, text);
        }
        private static UILabel AddDescription(UIComponent panel, string name, UIComponent alignTo, float fontScale, string text)
        {
            UILabel desc = panel.AddUIComponent<UILabel>();
            desc.name = name;
            desc.width = panel.width - 80;
            desc.wordWrap = true;
            desc.autoHeight = true;
            desc.textScale = fontScale;
            desc.textColor = m_greyColor;
            desc.text = text;
            desc.relativePosition = new Vector3(alignTo.relativePosition.x + 26f, alignTo.relativePosition.y + alignTo.height + 10);
            return desc;
        }

        public void OnShortcutKeyChanged(float mode)
        {
            ModSettings.GetSettings().Save();
        }

        public void OnShowConnectionGraphChanged(int mode)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.ShowConnectionGraph = mode;
            oSettings.Save();

            // Register renderer if needed.
            PathConnectionRenderer.RegisterRenderer();
        }

        public void OnLogCitizenPathFailuresChanged(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.LogCitizenPathFailures = bChecked;
            oSettings.Save();
        }

        public void OnLogReasonChanged(int iSelectedIndex)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Shift log reason back to actual reasons
            oSettings.MatchLogReason = GetReasonId(m_reasonNames[iSelectedIndex]);
            oSettings.Save();
        }

        public void OnLogCandidatesChanged(int iSelectedIndex)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Shift log reason back to actual reasons
            oSettings.MatchLogCandidates = iSelectedIndex;
            oSettings.Save();
        }

        public void OnShowBuildingId(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.ShowBuildingId = bChecked;
            oSettings.Save();
        }

        public void OnPathDistanceServices(int index)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PathDistanceServices = index;
        }

        public void OnPathDistanceGoods(int index)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PathDistanceGoods = index;
        }
        public void OnPathDistanceHeuristicChanged(float value)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PathDistanceHeuristic = (int) value;

            // Update scale values for this new setting
            QueueData.UpdateHeuristicScale();
        }
        public void OnPathDistanceShiftChanged(float value)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PathDistanceTravelTimeBaseValue = (int)Math.Round(value);

            // Update outside connection shift
            PathNodeCache.InvalidateOutsideConnections();
        }
        public void OnPathFailExclusion(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.EnablePathFailExclusion = bChecked;
        }

        public void OnBalancedMatchModeChanged(int mode)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.BalancedMatchMode = (CustomTransferManager.BalancedMatchModeOption) mode;
        }

        public void OnOverrideResidentialSick(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.OverrideSickHandler = bChecked;

            // Update rate fields
            m_sliderSickHelicopterRate.Enable(bChecked);
            m_sliderSickWalkRate.Enable(bChecked);
            m_chkDisplaySickNotification.isEnabled = bChecked;

            // Clear sick timers for non residential buildings when transistioning to vanilla handler.
            if (!bChecked)
            {
                SickHandler.ClearSickTimerForNonResidential();
            }
        }

        public void OnSickWalkRate(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.SickWalkRate = (uint)fValue;
        }

        public void OnSickHelicopterRate(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.SickHelicopterRate = (uint)fValue;
        }

        public void OnDisplaySadNotification(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.DisplaySickNotification = bChecked;
        }

        public void OnRandomSickRate(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.RandomSickRate = (uint) fValue;
        }

        public void OnTaxiMove(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.TaxiMove = bChecked;

            Patcher.PatchTaxiStandHandler();
        }

        public void OnTaxiStandDelay(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.TaxiStandDelay = (int)fValue;
        }

        public void OnDisableDummyTraffic(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.DisableDummyTraffic = bChecked;
        }

        public void OnEmployOvereducatedWOrkers(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.EmployOverEducatedWorkers = bChecked;
        }

        public void OnOverrideGenericIndustriesHandlerChanged(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.OverrideGenericIndustriesHandler = bChecked;

            IndustrialBuildingAISimulationStepActive.PatchGenericIndustriesHandler();
        }

        public void OnWarehouseFirstPercentChanged(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.WarehouseReserveTrucksPercent = (int)fValue;
        }

        public void OnImportRestrictMaterial(CustomTransferReason.Reason material, bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.SetImportRestriction(material, !bChecked);
        }

        public void OnImportRestrictMaterialWarehouses(CustomTransferReason.Reason material, bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.SetWarehouseImportRestriction(material, !bChecked);
        }

        public void OnFireTruckAI(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.FireTruckAI = bChecked;
        }

        public void OnFireCopterAI(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.FireCopterAI = bChecked;
        }

        public void OnGarbageTruckAI(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.GarbageTruckAI = bChecked;
        }

        public void OnPoliceCarAI(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PoliceCarAI = bChecked;
        }

        public void OnPoliceCopterAI(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PoliceCopterAI = bChecked;
        }


        public void OnDistanceLimit(CustomTransferReason.Reason material, float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.SetActiveDistanceRestrictionKm(material, fValue);

            // If updating mail restrictions, we also update global Mail2 restrictions
            if (material == CustomTransferReason.Reason.Mail)
            {
                oSettings.SetActiveDistanceRestrictionKm(CustomTransferReason.Reason.Mail2, fValue);
            }
        }

        public void OnLocalizationDropDownChanged(int value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.PreferredLanguage = Localization.GetLoadedCodes()[value];
            oSettings.Save();
        }

        public void OnDeadValueChanged(float fValue)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.DeadTimerValue = (int)fValue;
            oSettings.Save();
        }

        public void OnSickValueChanged(float fValue)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.SickTimerValue = (int)fValue;
            oSettings.Save();
        }

        public void OnGoodsValueChanged(float fValue)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.GoodsTimerValue = (int)fValue;
            oSettings.Save();
        }

        public void OnResetTransferStatisticsClicked()
        {
            MatchStats.Init();

            TransferManagerStats.Init();

            // Reset dropped reason count
            CustomTransferDispatcher.Instance.ResetStatistics();
        }

        public void OnResetTransferManagerSettingsClicked()
        {
            TransferManagerLoader.ClearSettings();

            // Update global settings
            UpdateTransferManagerSettings();
        }

        public void OnResetPathingStatisticsClicked()
        {
            PathFindFailure.Reset();
            HumanAIPathfindFailure.s_pathFailCount = 0;
            CarAIPathfindFailurePatch.s_pathFailCount = 0;
        }

        public void OnVehiclesOnRouteChanged(bool value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.TransferIssueShowWithVehiclesOnRoute = value;
            oSettings.Save();
        }

        public void OnEnablePanelTransparencyChanged(bool value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.EnablePanelTransparency = value;
            oSettings.Save();
        }
            
        public void OnStatisticsEnabledChanged(bool value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.StatisticsEnabled = value;
            oSettings.Save();

            // Initialise stats now
            if (value)
            {
                MatchStats.Init();
            }
        }

        public void OnExperimentalDeathcare(bool enabled)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ImprovedDeathcareMatching = enabled;
        }

        public void OnExperimentalGarbage(bool enabled)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ImprovedGarbageMatching = enabled;
        }

        public void OnExperimentalCrime(bool enabled)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ImprovedCrimeMatching = enabled;
        }
        public void OnImprovedMailMatching(bool enabled)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ImprovedMailTransfers = enabled;
        }

        public void setOptionEnableNewTransferManager(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings(); 
            oSettings.EnableNewTransferManager = bChecked;

            // Reset the stats as we have changed Transfer Manager.
            MatchStats.Init();
            UpdateTransferManagerEnabled();

            // Remove transpiler patches
            IndustrialBuildingAISimulationStepActive.PatchGenericIndustriesHandler();
            CommonBuildingAIHandleCrime.PatchCrime2Handler();
            Patcher.PatchTaxiStandHandler();
        }

        public void setOptionPreferLocalService(bool index)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PreferLocalService = index;
        }

        public void setOptionWarehouseFirst(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.WarehouseFirst = bChecked;
        }

        public void setOptionImprovedWarehouseMatching(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ImprovedWarehouseMatching = bChecked;
        }

        public void setOptionFactoryFirst(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.FactoryFirst = bChecked;
        }

        public void setOptionWarehouseReserveTrucks(float fPercent)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.WarehouseReserveTrucksPercent = (int)fPercent;
        }

        public void OnNewWarehouseTransferChanged(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.NewInterWarehouseTransfer = bChecked;
        }

        public void OnOutsideShipMultiplier(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.OutsideShipMultiplier = (int)fValue;
            PathNodeCache.InvalidateOutsideConnections();
        }
        public void OnOutsidePlaneMultiplier(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.OutsidePlaneMultiplier = (int)fValue;
            PathNodeCache.InvalidateOutsideConnections();
        }
        public void OnOutsideTrainMultiplier(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.OutsideTrainMultiplier = (int)fValue;
            PathNodeCache.InvalidateOutsideConnections();
        }
        public void OnOutsideRoadMultiplier(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.OutsideRoadMultiplier = (int)fValue;
        }
        public void OnExportVehicleLimit(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ExportVehicleLimit = (int)fValue;
        }
        public void UpdateTransferManagerSettings()
        {
            if (TransferManagerLoader.IsLoaded())
            {
                SaveGameSettings oSettings = SaveGameSettings.GetSettings();
                m_chkEnableTransferManager.isChecked = oSettings.EnableNewTransferManager;

                // General tab
                m_dropdownBalanced.selectedIndex = (int)oSettings.BalancedMatchMode;
                m_dropdownPathDistanceServices.selectedIndex = oSettings.PathDistanceServices;
                m_dropdownPathDistanceGoods.selectedIndex = oSettings.PathDistanceGoods;
                m_chkEnablePathFailExclusion.isChecked = oSettings.EnablePathFailExclusion;
                m_sliderPathDistanceHeuristic.SetValue(oSettings.PathDistanceHeuristic);
                m_sliderPathDistanceShift.SetValue(oSettings.PathDistanceTravelTimeBaseValue);
                m_chkDisableDummyTraffic.isChecked = oSettings.DisableDummyTraffic;
                m_chkApplyUnlimited.isChecked = oSettings.ApplyUnlimited;
                m_chkEmployOvereducatedWorkers.isChecked = oSettings.EmployOverEducatedWorkers;

                // Goods delivery
                m_chkFactoryFirst.isChecked = oSettings.FactoryFirst;
                m_chkOverrideGenericIndustriesHandler.isChecked = oSettings.OverrideGenericIndustriesHandler;

                m_chkImprovedWarehouseMatching.isChecked = oSettings.ImprovedWarehouseMatching;
                m_chkNewInterWarehouseMatching.isChecked = oSettings.NewInterWarehouseTransfer;
                m_chkWarehouseFirst.isChecked = oSettings.WarehouseFirst;
                m_sliderWarehouseReservePercent.SetValue(oSettings.WarehouseReserveTrucksPercent);

                // Import/Export
                m_sliderShipMultiplier.SetValue(oSettings.OutsideShipMultiplier);
                m_sliderPlaneMultiplier.SetValue(oSettings.OutsidePlaneMultiplier);
                m_sliderTrainMultiplier.SetValue(oSettings.OutsideTrainMultiplier);
                m_sliderRoadMultiplier.SetValue(oSettings.OutsideRoadMultiplier);
                m_sliderExportVehicleLimitPercent.SetValue(oSettings.ExportVehicleLimit);

                // Services
                m_chkPreferLocal.isChecked = oSettings.PreferLocalService;
                m_chkDeathcareExperimental.isChecked = oSettings.ImprovedDeathcareMatching;
                m_chkGarbageExperimental.isChecked = oSettings.ImprovedGarbageMatching;
                m_chkPoliceExperimental.isChecked = oSettings.ImprovedCrimeMatching;
                m_chkImprovedMailTransfers.isChecked = oSettings.ImprovedMailTransfers;
                
                // Sick collection
                m_chkOverrideSickCollection.isChecked = oSettings.OverrideSickHandler;
                m_sliderSickGenerationRate.SetValue(oSettings.RandomSickRate);
                m_sliderSickHelicopterRate.SetValue(oSettings.SickHelicopterRate);
                m_chkDisplaySickNotification.isChecked = oSettings.DisplaySickNotification;
                m_sliderSickWalkRate.SetValue(oSettings.SickWalkRate);

                //Taxi Move
                m_chkTaxiMove.isChecked = oSettings.TaxiMove;
                m_sliderTaxiStandDelay.SetValue(oSettings.TaxiStandDelay);
                
                // VehicleAI
                m_chkFireTruckAI.isChecked = oSettings.FireTruckAI;
                m_chkFireCopterAI.isChecked = oSettings.FireCopterAI;
                m_chkGarbageTruckAI.isChecked = oSettings.GarbageTruckAI;
                m_chkPoliceCarAI.isChecked = oSettings.PoliceCarAI;
                m_chkPoliceCopterAI.isChecked = oSettings.PoliceCopterAI;

                foreach (KeyValuePair<CustomTransferReason.Reason, SettingsSlider> kvp in m_sliderLimits)
                {
                    if (kvp.Value is not null)
                    {
                        kvp.Value.SetValue(oSettings.GetActiveDistanceRestrictionKm(kvp.Key));
                    }
                }

                foreach (KeyValuePair<CustomTransferReason.Reason, UICheckBox> kvp in m_chkImport)
                {
                    if (kvp.Value is not null)
                    {
                        kvp.Value.isChecked = !oSettings.IsImportRestricted(kvp.Key);
                    }
                }

                foreach (KeyValuePair<CustomTransferReason.Reason, UICheckBox> kvp in m_chkImportWarehouses)
                {
                    if (kvp.Value is not null)
                    {
                        kvp.Value.isChecked = !oSettings.IsWarehouseImportRestricted(kvp.Key);
                    }
                }

                if (m_lblGhostVehicleCount is not null)
                {
                    m_lblGhostVehicleCount.text = Localization.Get("txtGhostVehiclesCount") + ": 0";
                }
                if (m_lblPathUnitCount is not null)
                {
                    m_lblPathUnitCount.text = Localization.Get("txtPathUnitCount") + ": 0";
                }

                UpdateTransferManagerEnabled();
            }
        }

        public void UpdateTransferManagerEnabled()
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            bool bLoaded = TransferManagerLoader.IsLoaded();

            EnableCheckbox(m_chkEnableTransferManager, bLoaded);

            // General
            m_sliderPathDistanceHeuristic.Enable(bLoaded && oSettings.EnableNewTransferManager);
            m_sliderPathDistanceShift.Enable(bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkEnablePathFailExclusion, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkDisableDummyTraffic, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkApplyUnlimited, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkEmployOvereducatedWorkers, bLoaded && oSettings.EnableNewTransferManager); 

            if (bLoaded && oSettings.EnableNewTransferManager)
            {
                m_dropdownBalanced.Enable();
                m_dropdownPathDistanceServices.Enable();
                m_dropdownPathDistanceGoods.Enable();
            }
            else
            {
                m_dropdownBalanced.Disable();
                m_dropdownPathDistanceServices.Disable();
                m_dropdownPathDistanceGoods.Disable();
            }

            // Goods Delivery
            EnableCheckbox(m_chkFactoryFirst, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkOverrideGenericIndustriesHandler, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkImprovedWarehouseMatching, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkNewInterWarehouseMatching, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkWarehouseFirst, bLoaded && oSettings.EnableNewTransferManager);
            m_sliderWarehouseReservePercent.Enable(bLoaded && oSettings.EnableNewTransferManager);

            // Services
            EnableCheckbox(m_chkPreferLocal, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkDeathcareExperimental, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkGarbageExperimental, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkPoliceExperimental, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkImprovedMailTransfers, bLoaded && oSettings.EnableNewTransferManager);


            // Sick collection
            EnableCheckbox(m_chkOverrideSickCollection, bLoaded && oSettings.EnableNewTransferManager);
            m_sliderSickHelicopterRate.Enable(m_chkOverrideSickCollection.isEnabled && m_chkOverrideSickCollection.isChecked);
            m_sliderSickWalkRate.Enable(m_chkOverrideSickCollection.isEnabled && m_chkOverrideSickCollection.isChecked);
            EnableCheckbox(m_chkDisplaySickNotification, m_chkOverrideSickCollection.isChecked);
            m_sliderSickGenerationRate.Enable(bLoaded && oSettings.EnableNewTransferManager);
            
            // Taxi Move
            EnableCheckbox(m_chkTaxiMove, bLoaded && oSettings.EnableNewTransferManager);
            m_sliderTaxiStandDelay.Enable(bLoaded && oSettings.EnableNewTransferManager);

            // Import / Export
            m_sliderShipMultiplier.Enable(bLoaded && oSettings.EnableNewTransferManager);
            m_sliderPlaneMultiplier.Enable(bLoaded && oSettings.EnableNewTransferManager);
            m_sliderTrainMultiplier.Enable(bLoaded && oSettings.EnableNewTransferManager);
            m_sliderRoadMultiplier.Enable(bLoaded && oSettings.EnableNewTransferManager);
            m_sliderExportVehicleLimitPercent.Enable(bLoaded && oSettings.EnableNewTransferManager);

            // Vehcile AI
            EnableCheckbox(m_chkFireTruckAI, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkFireCopterAI, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkGarbageTruckAI, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkPoliceCarAI, bLoaded && oSettings.EnableNewTransferManager);
            EnableCheckbox(m_chkPoliceCopterAI, bLoaded && oSettings.EnableNewTransferManager);
                
            foreach (KeyValuePair<CustomTransferReason.Reason, SettingsSlider> kvp in m_sliderLimits)
            {
                if (kvp.Value is not null)
                {
                    kvp.Value.Enable(bLoaded && oSettings.EnableNewTransferManager);
                }
            }

            foreach (KeyValuePair<CustomTransferReason.Reason, UICheckBox> kvp in m_chkImport)
            {
                if (kvp.Value is not null)
                {
                    if (bLoaded && oSettings.EnableNewTransferManager)
                    {
                        kvp.Value.Enable();
                    }
                    else
                    {
                        kvp.Value.Disable();
                    }
                }
            }

            foreach (KeyValuePair<CustomTransferReason.Reason, UICheckBox> kvp in m_chkImportWarehouses)
            {
                if (kvp.Value is not null)
                {
                    if (bLoaded && oSettings.EnableNewTransferManager)
                    {
                        kvp.Value.Enable();
                    }
                    else
                    {
                        kvp.Value.Disable();
                    }
                }
            }

            // Maintenance
            if (m_btnResetTransferManagerSettings is not null)
            {
                m_btnResetTransferManagerSettings.isEnabled = bLoaded && oSettings.EnableNewTransferManager;
            }
        }

        private void EnableCheckbox(UICheckBox checkbox, bool value)
        {
            if (value)
            {
                checkbox.Enable();
            }
            else
            {
                checkbox.Disable();
            }
        }

        private void LoadReasons()
        {
            m_reasonNames.Clear();

            foreach (CustomTransferReason.Reason reason in (CustomTransferReason.Reason[])Enum.GetValues(typeof(CustomTransferReason.Reason)))
            {
                if (reason != CustomTransferReason.Reason.None)
                {
                    m_reasonNames.Add(reason.ToString());
                }
            }

            m_reasonNames.Sort();

            // Add None to start of list
            m_reasonNames.Insert(0, CustomTransferReason.Reason.None.ToString());
        }

        private int GetReasonArrayIndex(int iReason)
        {
            CustomTransferReason.Reason reason = (CustomTransferReason.Reason)iReason;

            // Add the transfer reasons in enum order
            int iIndex = 0;
            foreach (string sReason in m_reasonNames)
            {
                if (reason.ToString().Equals(sReason))
                {
                    return iIndex;
                }
                iIndex++;
            }

            return 0;
        }

        private int GetReasonId(string sReason)
        {
            // Add the transfer reasons in enum order
            foreach (CustomTransferReason.Reason reason in (CustomTransferReason.Reason[])Enum.GetValues(typeof(CustomTransferReason.Reason)))
            {
                if (reason.ToString().Equals(sReason))
                {
                    return (int) reason;
                }
            }

            return (int) CustomTransferReason.Reason.None;
        }
    }
}
