using ColossalFramework.UI;
using ICities;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.IO;
using TransferManagerCE.CustomManager;
using TransferManagerCE.CustomManager.Stats;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using TransferManagerCE.Util;
using static TransferManagerCE.TransportUtils;

namespace TransferManagerCE
{
    public class SettingsUI
    {
        const int iSEPARATOR_HEIGHT = 10;

        private UICheckBox? m_chkEnableTransferManager = null;
        
        // Outside connection tab
        private SettingsSlider? m_sliderShipPriority = null;
        private SettingsSlider? m_sliderPlanePriority = null;
        private SettingsSlider? m_sliderTrainPriority = null;
        private SettingsSlider? m_sliderRoadPriority = null;
        private SettingsSlider? m_sliderExportVehicleLimitPercent = null;

        // Services tab
        private UICheckBox? m_chkPreferLocal = null;
        private UICheckBox? m_chkDeathcareExperimental = null;
        private UICheckBox? m_chkGarbageExperimental = null;
        private UICheckBox? m_chkPoliceExperimental = null;
        private UICheckBox? m_chkImprovedMailTransfers = null;

        private UICheckBox? m_chkPoliceToughOnCrime = null;

        // Collect sick
        private UICheckBox? m_chkOverrideSickCollection = null;
        private SettingsSlider? m_sliderSickWalkRate = null;
        private SettingsSlider? m_sliderSickHelicopterRate = null;
        private SettingsSlider? m_sliderSickGenerationRate = null;
        private UICheckBox? m_chkDisplaySickNotification = null;

        // Mail
        private SettingsSlider? m_sliderMainBuildingMaxMail = null;
        private UICheckBox? m_chkMainBuildingPostTruck = null;

        // Taxi Stand
        private UICheckBox? m_chkTaxiMove = null;
        private SettingsSlider? m_sliderTaxiStandDelay = null;

        // Warehouse
        private UICheckBox? m_chkFactoryFirst = null;
        private UICheckBox? m_chkOverrideGenericIndustriesHandler = null;

        private UICheckBox? m_chkWarehouseFirst = null;
        private SettingsSlider? m_sliderWarehouseReservePercent = null;
        private UICheckBox? m_chkImprovedWarehouseMatching = null;
        private UICheckBox? m_chkWarehouseSmarterImportExport = null;
        private UICheckBox? m_chkNewInterWarehouseMatching = null;

        // General tab
        private UICheckBox? m_chkEnablePathFailExclusion = null;
        private UIDropDown? m_dropdownPathDistanceServices = null;
        private UIDropDown? m_dropdownPathDistanceGoods = null;
        private SettingsSlider? m_sliderPathDistanceHeuristic = null;
        private SettingsSlider? m_sliderCargoStationDelay = null;
        private UIDropDown? m_dropdownBalanced = null;
        private UICheckBox? m_chkDisableDummyTraffic = null;
        private UICheckBox? m_chkApplyUnlimited = null;
        private UICheckBox? m_chkEmployOvereducatedWorkers = null;

        // Maintenance tab
        private UILabel? m_lblGhostVehicleCount = null;
        private UILabel? m_lblPathUnitCount = null;
        private UIDropDown? m_dropdownLogReason = null;
        private UIDropDown? m_dropdownCandidates = null;
        private UIButton? m_btnResetTransferManagerSettings = null;

        private Dictionary<CustomTransferReason.Reason, SettingsSlider> m_sliderLimits = new Dictionary<CustomTransferReason.Reason, SettingsSlider>();
        private Dictionary<CustomTransferReason.Reason, UICheckBox> m_chkImport = new Dictionary<CustomTransferReason.Reason, UICheckBox>();
        private Dictionary<CustomTransferReason.Reason, UICheckBox> m_chkImportWarehouses = new Dictionary<CustomTransferReason.Reason, UICheckBox>();

        private List<string> m_reasonNames = new List<string>();
        private List<UIComponent> m_saveGameSettings = new List<UIComponent>();

        public SettingsUI()
        {
        }

        public void OnSettingsUI(UIHelper helper)
        {
            // Title
            UIComponent pnlMain = (UIComponent)helper.self;
            UILabel txtTitle = UISettings.AddDescription(pnlMain, "title", pnlMain, 1.0f, TransferManagerMod.Instance.Name);
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

        private void AddSaveGameSetting(UIComponent component)
        {
            m_saveGameSettings.Add(component);
        }

        public void SetupGeneralTab(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // ----------------------------------------------------------------
            UIHelper groupLocalisation = (UIHelper)helper.AddGroup(Localization.Get("GROUP_LOCALISATION"));
            groupLocalisation.AddDropdown(Localization.Get("dropdownLocalization"), Localization.GetLoadedLanguages(), Localization.GetLanguageIndexFromCode(oSettings.PreferredLanguage), OnLocalizationDropDownChanged);

            // ----------------------------------------------------------------
            // Behaviour
            UIHelper groupVisuals = (UIHelper)helper.AddGroup(Localization.Get("GROUP_VISUALS"));
            groupVisuals.AddCheckbox(Localization.Get("optionEnablePanelTransparency"), oSettings.EnablePanelTransparency, OnEnablePanelTransparencyChanged);
            groupVisuals.AddCheckbox(Localization.Get("optionAddUnifiedUIbutton"), oSettings.AddUnifiedUIButton, (value) =>
            {
                ModSettings.GetSettings().AddUnifiedUIButton = value;
                ModSettings.GetSettings().Save();

                if (value)
                {
                    UnifiedUIButton.Add();
                }
                else
                {
                    UnifiedUIButton.Remove();
                }
            });

            // ----------------------------------------------------------------
            // Transfer Issue Panel
            UIHelper groupTransferIssue = (UIHelper)helper.AddGroup(Localization.Get("GROUP_TRANSFERISSUE_PANEL"));
            UIPanel panel = (UIPanel)groupTransferIssue.self;
            UIKeymappingsPanel keymappingsTransferIssue = panel.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsTransferIssue.AddKeymapping(Localization.Get("keyOpenTransferIssuePanel"), ModSettings.GetSettings().TransferIssueHotkey, OnShortcutKeyChanged);
            SettingsSlider.CreateSettingsStyle(groupTransferIssue, LayoutDirection.Horizontal, Localization.Get("sliderTransferIssueDeadTimerValue"), 400, 200, 0f, 255f, 1f, (float)oSettings.DeadTimerValue, 0, OnDeadValueChanged);
            SettingsSlider.CreateSettingsStyle(groupTransferIssue, LayoutDirection.Horizontal, Localization.Get("sliderTransferIssueSickTimerValue"), 400, 200, 0f, 255f, 1f, (float)oSettings.SickTimerValue, 0, OnSickValueChanged);
            SettingsSlider.CreateSettingsStyle(groupTransferIssue, LayoutDirection.Horizontal, Localization.Get("sliderTransferIssueGoodsTimerValue"), 400, 200, 0f, 255f, 1f, (float)oSettings.GoodsTimerValue, 0, OnGoodsValueChanged);

            // ----------------------------------------------------------------
            // Building Panel
            UIHelper groupBuildingPanel = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BUILDING_PANEL"));
            UIPanel panelBuilding = (UIPanel)groupBuildingPanel.self;
            UIKeymappingsPanel keymappingsBuildingPanel = panelBuilding.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsBuildingPanel.AddKeymapping(Localization.Get("keyOpenBuildingPanel"), ModSettings.GetSettings().SelectionToolHotkey, OnShortcutKeyChanged); // Automatically saved

            // ----------------------------------------------------------------
            // Show id's in building title
            groupBuildingPanel.AddSpace(iSEPARATOR_HEIGHT);
            UICheckBox check = (UICheckBox) groupBuildingPanel.AddCheckbox(Localization.Get("optionShowBuildingId"), oSettings.ShowBuildingId, OnShowBuildingId);

            // ----------------------------------------------------------------
            // Building Panel - Status tab
            groupBuildingPanel.AddSpace(iSEPARATOR_HEIGHT);
            UISettings.AddDescription(groupBuildingPanel, "txtStatusHideVehicleReason", 1.0f, Localization.Get("txtStatusHideVehicleReason"));
            groupBuildingPanel.AddCheckbox(Localization.Get("optionStatusHideVehicleReason"), oSettings.StatusHideVehicleReason, (bChecked) =>
            {
                ModSettings.GetSettings().StatusHideVehicleReason = bChecked;
                ModSettings.GetSettings().Save();
            });

            // ----------------------------------------------------------------
            // Reset panel location
            groupBuildingPanel.AddSpace(iSEPARATOR_HEIGHT);
            groupBuildingPanel.AddButton(Localization.Get("buttonResetPanelPosition"), () =>
            {
                // Move panel if created
                if (BuildingPanel.Exists)
                {
                    // Center panel to screen
                    BuildingPanel.Instance.CenterTo(null);

                    // Save new positions
                    oSettings.BuildingPanelPosX = BuildingPanel.Instance.absolutePosition.x;
                    oSettings.BuildingPanelPosY = BuildingPanel.Instance.absolutePosition.y;
                    oSettings.Save();
                }
            });

            // ----------------------------------------------------------------
            // Statistics group
            UIHelper groupStats = (UIHelper)helper.AddGroup(Localization.Get("GROUP_STATISTICS_PANEL"));
            groupStats.AddCheckbox(Localization.Get("StatisticsPanelEnabled"), oSettings.StatisticsEnabled, OnStatisticsEnabledChanged);
            UIPanel panelStats = (UIPanel)groupStats.self;
            UIKeymappingsPanel keymappingsStatsPanel = panelStats.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsStatsPanel.AddKeymapping(Localization.Get("keyOpenStatisticsPanel"), ModSettings.GetSettings().StatsPanelHotkey, OnShortcutKeyChanged); // Automatically saved

            // ----------------------------------------------------------------
            // Outside Connections group
            UIHelper groupOutside = (UIHelper)helper.AddGroup(Localization.Get("GROUP_OUTSIDE_CONNECTIONS_PANEL"));
            UIPanel panelOutside = (UIPanel)groupOutside.self;
            UIKeymappingsPanel keymappingsOutsidePanel = panelOutside.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsOutsidePanel.AddKeymapping(Localization.Get("keyOpenOutsidePanel"), ModSettings.GetSettings().OutsideConnectionPanelHotkey, OnShortcutKeyChanged); // Automatically saved

            // ----------------------------------------------------------------
            // Settings panel
            UIHelper groupSettings = (UIHelper)helper.AddGroup(Localization.Get("GROUP_SETTINGS_PANEL"));
            UIPanel panelSettings = (UIPanel)groupSettings.self;
            UIKeymappingsPanel keymappingsSettingsPanel = panelSettings.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsSettingsPanel.AddKeymapping(Localization.Get("keyOpenSettingsPanel"), ModSettings.GetSettings().SettingsPanelHotkey, OnShortcutKeyChanged); // Automatically saved

            // Reset panel location
            groupSettings.AddSpace(iSEPARATOR_HEIGHT);
            groupSettings.AddButton(Localization.Get("buttonResetPanelPosition"), () =>
            {
                // Move panel if created
                if (SettingsPanel.Exists)
                {
                    // Center panel to screen
                    SettingsPanel.Instance.CenterTo(null);

                    // Save new positions
                    oSettings.SettingsPanelPosX = SettingsPanel.Instance.absolutePosition.x;
                    oSettings.SettingsPanelPosY = SettingsPanel.Instance.absolutePosition.y;
                    oSettings.Save();
                }
            });

            // ----------------------------------------------------------------
            // Path Distance Panel panel
            UIHelper groupPathDistance = (UIHelper)helper.AddGroup(Localization.Get("GROUP_PATH_DISTANCE_PANEL"));
            UIPanel panelPathDistance = (UIPanel)groupPathDistance.self;
            UIKeymappingsPanel keymappingsPathDistancePanel = panelPathDistance.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsPathDistancePanel.AddKeymapping(Localization.Get("keyOpenPathDistancePanel"), ModSettings.GetSettings().PathDistancePanelHotkey, OnShortcutKeyChanged); // Automatically saved
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

        // ----------------------------------------------------------------------------------------
        public void SetupTransferGeneralTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // ----------------------------------------------------------------
            // Path Failure Exclusion
            UIHelper groupPathFailure = (UIHelper)helper.AddGroup(Localization.Get("GROUP_PATH_FAIL_EXCLUSION"));
            UISettings.AddDescription(groupPathFailure, "txtPathFailExclusion", 1.0f, Localization.Get("txtPathFailExclusion"));
            m_chkEnablePathFailExclusion = (UICheckBox)groupPathFailure.AddCheckbox(Localization.Get("optionPathFailExclusion"), oSettings.EnablePathFailExclusion, OnPathFailExclusion);
            AddSaveGameSetting(m_chkEnablePathFailExclusion);

            // ----------------------------------------------------------------
            // Path distance
            UIHelper groupPathDistance = (UIHelper)helper.AddGroup(Localization.Get("GROUP_PATH_DISTANCE"));
            UISettings.AddDescription(groupPathDistance, "txtPathDistance", 1.0f, Localization.Get("txtPathDistance"));
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
            AddSaveGameSetting(m_dropdownPathDistanceGoods);
            groupPathDistance.AddSpace(iSEPARATOR_HEIGHT);

            // Accuracy slider
            UISettings.AddDescription(groupPathDistance, "txtPathDistanceHeuristic", 1.0f, Localization.Get("txtPathDistanceHeuristic"));
            m_sliderPathDistanceHeuristic = SettingsSlider.CreateSettingsStyle(groupPathDistance, LayoutDirection.Horizontal, Localization.Get("sliderPathDistanceHeuristic"), 400, 200, 0f, 100f, 10f, (float)oSettings.PathDistanceHeuristic, 0, OnPathDistanceHeuristicChanged);
            m_sliderPathDistanceHeuristic.Percent = true;
            AddSaveGameSetting(m_sliderPathDistanceHeuristic);
            UISettings.AddDescription(groupPathDistance, "txtPathDistanceHeuristicKey", 1.0f, Localization.Get("txtPathDistanceHeuristicKey"));
            groupPathDistance.AddSpace(iSEPARATOR_HEIGHT);

            // Cargo Station Travel Time Delay
            UISettings.AddDescription(groupPathDistance, "txtPathDistanceCargoStationDelay", 1.0f, Localization.Get("txtPathDistanceCargoStationDelay"));
            m_sliderCargoStationDelay = SettingsSlider.CreateSettingsStyle(groupPathDistance, LayoutDirection.Horizontal, Localization.Get("sliderCargoStationDelay"), 400, 200, 0f, 2000f, 100f, (float)oSettings.PathDistanceCargoStationDelay, 0, OnPathDistanceCargoDelay);
            AddSaveGameSetting(m_sliderCargoStationDelay);
            groupPathDistance.AddSpace(iSEPARATOR_HEIGHT);

            // ----------------------------------------------------------------
            // Balanced match mode setting
            string[] itemsBalancedMode = {
                Localization.Get("dropdownBalancedModeIncomingFirst"),
                Localization.Get("dropdownBalancedModeLeastFirst"),
                Localization.Get("dropdownBalancedModePassiveFirst"),
            };

            UIHelper groupBalanced = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BALANCED_MATCH_MODE"));
            m_dropdownBalanced = (UIDropDown)groupBalanced.AddDropdown(Localization.Get("dropdownBalancedTitle"), itemsBalancedMode, (int)oSettings.BalancedMatchMode, OnBalancedMatchModeChanged);
            m_dropdownBalanced.width = 400;
            AddSaveGameSetting(m_dropdownBalanced);
            UISettings.AddDescription(groupBalanced, "txtBalancedMatch", 1.0f, Localization.Get("txtBalancedMatch"));

            // ----------------------------------------------------------------
            // Employ over-educated workers
            UIHelper groupOveredcuatedWorkers = (UIHelper)helper.AddGroup(Localization.Get("GROUP_OVEREDUCATED_WORKERS"));
            UISettings.AddDescription(groupOveredcuatedWorkers, "txtEmployOvereducatedWorkers", 1.0f, Localization.Get("txtEmployOvereducatedWorkers"));
            m_chkEmployOvereducatedWorkers = (UICheckBox)groupOveredcuatedWorkers.AddCheckbox(Localization.Get("optionEmployOverEducatedWorkers"), oSettings.EmployOverEducatedWorkers, OnEmployOvereducatedWOrkers);
            AddSaveGameSetting(m_chkEmployOvereducatedWorkers);

            // ----------------------------------------------------------------
            // Unlimited flag
            UIHelper groupUnlimited = (UIHelper)helper.AddGroup(Localization.Get("GROUP_UNLIMITED"));
            UISettings.AddDescription(groupUnlimited, "txtApplyUnlimited", 1.0f, Localization.Get("txtApplyUnlimited"));
            m_chkApplyUnlimited = (UICheckBox)groupUnlimited.AddCheckbox(Localization.Get("optionApplyUnlimited"), oSettings.ApplyUnlimited, (bCheck) => SaveGameSettings.GetSettings().ApplyUnlimited = bCheck);
            AddSaveGameSetting(m_chkApplyUnlimited);
            groupUnlimited.AddSpace(iSEPARATOR_HEIGHT);
            UISettings.AddDescription(groupUnlimited, "txtApplyUnlimitedWarning", 1.0f, Localization.Get("txtApplyUnlimitedWarning"));

            // ----------------------------------------------------------------
            // Dummy traffic
            UIHelper groupDummyTraffic = (UIHelper)helper.AddGroup(Localization.Get("GROUP_DUMMY_TRAFFIC"));
            UISettings.AddDescription(groupDummyTraffic, "txtDummyTraffic", 1.0f, Localization.Get("txtDummyTraffic"));
            m_chkDisableDummyTraffic = (UICheckBox)groupDummyTraffic.AddCheckbox(Localization.Get("optionDummyTraffic"), oSettings.DisableDummyTraffic, OnDisableDummyTraffic);
            AddSaveGameSetting(m_chkDisableDummyTraffic);
        }

        // ----------------------------------------------------------------------------------------
        public void SetupWarehousesTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // FACTORY GROUP
            UIHelper groupFactory = (UIHelper)helper.AddGroup(Localization.Get("GROUP_FACTORY_OPTIONS"));
            UIPanel panelFactory = (groupFactory as UIHelper).self as UIPanel;

            // Factory First
            UISettings.AddDescription(panelFactory, "optionFactoryFirstText", panelFactory, 1.0f, Localization.Get("optionFactoryFirstText"));
            m_chkFactoryFirst = (UICheckBox)groupFactory.AddCheckbox(Localization.Get("optionFactoryFirst"), oSettings.FactoryFirst, (index) => setOptionFactoryFirst(index));
            AddSaveGameSetting(m_chkFactoryFirst);
            groupFactory.AddSpace(iSEPARATOR_HEIGHT);

            // Override generic industries handler
            UISettings.AddDescription(panelFactory, "txtOverrideGenericIndustriesHandler", panelFactory, 1.0f, Localization.Get("txtOverrideGenericIndustriesHandler"));
            m_chkOverrideGenericIndustriesHandler = (UICheckBox)groupFactory.AddCheckbox(Localization.Get("optionOverrideGenericIndustriesHandler"), oSettings.OverrideGenericIndustriesHandler, OnOverrideGenericIndustriesHandlerChanged);
            AddSaveGameSetting(m_chkOverrideGenericIndustriesHandler);

            // WAREHOUSE GROUP
            UIHelper groupWarehouse = (UIHelper) helper.AddGroup(Localization.Get("GROUP_WAREHOUSE_OPTIONS"));
            UIPanel panelGroupWarehouse = (groupWarehouse as UIHelper).self as UIPanel;

            // Warehouse first
            UISettings.AddDescription(panelGroupWarehouse, "optionWarehouseFirst_txt", panelGroupWarehouse, 1.0f, Localization.Get("optionWarehouseFirst_txt"));
            m_chkWarehouseFirst = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionWarehouseFirst"), oSettings.WarehouseFirst, (index) => setOptionWarehouseFirst(index));
            AddSaveGameSetting(m_chkWarehouseFirst);
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // Smarter Import / Export
            UISettings.AddDescription(panelGroupWarehouse, "txtWarehouseSmartImportExport", panelGroupWarehouse, 1.0f, Localization.Get("txtWarehouseSmartImportExport"));
            m_chkWarehouseSmarterImportExport = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionWarehouseSmartImportExport"), oSettings.WarehouseSmartImportExport, OnWarehouseSmarterImportExport);
            AddSaveGameSetting(m_chkWarehouseSmarterImportExport);
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // Improved Warehouse Matching
            UISettings.AddDescription(panelGroupWarehouse, "txtImprovedWarehouseMatching", panelGroupWarehouse, 1.0f, Localization.Get("txtImprovedWarehouseMatching"));
            m_chkImprovedWarehouseMatching = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionImprovedWarehouseMatching"), oSettings.ImprovedWarehouseMatching, (index) => setOptionImprovedWarehouseMatching(index));
            AddSaveGameSetting(m_chkImprovedWarehouseMatching);
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // Improved inter-warehouse matching
            UISettings.AddDescription(panelGroupWarehouse, "txtNewInterWarehouseMatching", panelGroupWarehouse, 1.0f, Localization.Get("txtNewInterWarehouseMatching"));
            m_chkNewInterWarehouseMatching = (UICheckBox)groupWarehouse.AddCheckbox(Localization.Get("optionNewWarehouseTransfer"), oSettings.NewInterWarehouseTransfer, OnNewWarehouseTransferChanged);
            AddSaveGameSetting(m_chkNewInterWarehouseMatching);
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);

            // Reserve trucks
            UISettings.AddDescription(panelGroupWarehouse, "txtNewWarehouseReserveTrucks", panelGroupWarehouse, 1.0f, Localization.Get("txtNewWarehouseReserveTrucks"));
            m_sliderWarehouseReservePercent = SettingsSlider.CreateSettingsStyle(groupWarehouse, LayoutDirection.Horizontal, Localization.Get("sliderWarehouseReservePercent"), 400, 200, 0f, 100f, 5f, (float)oSettings.WarehouseReserveTrucksPercent, 0, OnWarehouseFirstPercentChanged);
            m_sliderWarehouseReservePercent.Percent = true;
            AddSaveGameSetting(m_sliderWarehouseReservePercent);
            groupWarehouse.AddSpace(iSEPARATOR_HEIGHT);
        }

        // ----------------------------------------------------------------------------------------
        public void SetupImportExportTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Outside Multipliers
            UIHelper groupImportExport = (UIHelper) helper.AddGroup(Localization.Get("GROUP_EXPORTIMPORT_OPTIONS"));
            UIPanel txtPanel3 = groupImportExport.self as UIPanel;

            // Priority
            UISettings.AddDescription(txtPanel3, "OutsidePriorityDescription", txtPanel3, 1.0f, Localization.Get("OutsidePriorityDescription"));
            m_sliderPlanePriority = SettingsSlider.CreateSettingsStyle(groupImportExport, LayoutDirection.Horizontal, "Plane Priority", 400, 200, 0f, 100f, 1f, (float)oSettings.OutsidePlanePriority, 0, (value) => OnOutsideConnectionPriority(TransportType.Plane, value));
            m_sliderPlanePriority.Percent = true;
            AddSaveGameSetting(m_sliderPlanePriority);
            m_sliderTrainPriority = SettingsSlider.CreateSettingsStyle(groupImportExport, LayoutDirection.Horizontal, "Train Priority", 400, 200, 0f, 100f, 1f, (float)oSettings.OutsideTrainPriority, 0, (value) => OnOutsideConnectionPriority(TransportType.Train, value));
            m_sliderTrainPriority.Percent = true;
            AddSaveGameSetting(m_sliderTrainPriority);
            m_sliderShipPriority = SettingsSlider.CreateSettingsStyle(groupImportExport, LayoutDirection.Horizontal, "Ship Priority", 400, 200, 0f, 100f, 1f, (float)oSettings.OutsideShipPriority, 0, (value) => OnOutsideConnectionPriority(TransportType.Ship, value));
            m_sliderShipPriority.Percent = true;
            AddSaveGameSetting(m_sliderShipPriority);
            m_sliderRoadPriority = SettingsSlider.CreateSettingsStyle(groupImportExport, LayoutDirection.Horizontal, "Road Priority", 400, 200, 0f, 100f, 1f, (float)oSettings.OutsideRoadPriority, 0, (value) => OnOutsideConnectionPriority(TransportType.Road, value));
            m_sliderRoadPriority.Percent = true;
            AddSaveGameSetting(m_sliderRoadPriority);

            // Export vehicle limit
            UIHelper groupExportLimits = (UIHelper)helper.AddGroup(Localization.Get("GROUP_EXPORT_LIMITS"));
            UIPanel panelExportLimit = groupExportLimits.self as UIPanel;
            m_sliderExportVehicleLimitPercent = SettingsSlider.CreateSettingsStyle(groupExportLimits, LayoutDirection.Horizontal, Localization.Get("sliderExportVehicleLimit"), 400, 200, 0f, 100f, 1f, (float)oSettings.ExportVehicleLimit, 0, OnExportVehicleLimit);
            m_sliderExportVehicleLimitPercent.Percent = true;
            AddSaveGameSetting(m_sliderExportVehicleLimitPercent);
            UISettings.AddDescription(panelExportLimit, "txtExportVehicleLimit", panelExportLimit, 1.0f, Localization.Get("txtExportVehicleLimit"));

            // Import restrictions
            UIHelperBase groupImportRestrict = helper.AddGroup(Localization.Get("GROUP_IMPORT_RESTRICTIONS"));
            UIPanel panelImportRestrictions = (groupImportRestrict as UIHelper).self as UIPanel;
            UISettings.AddDescription(panelImportRestrictions, "txtImportRestrictions", panelImportRestrictions, 1.0f, Localization.Get("txtImportRestrictions"));

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

                    UICheckBox? chkMaterial = UIMyUtils.AddCheckbox(panelMaterialRestrictions, material.ToString(), UIFonts.SemiBold, 1.0f, !oSettings.IsImportRestricted(material), (index) => OnImportRestrictMaterial(material, index));
                    if (chkMaterial is not null)
                    {
                        chkMaterial.width = 300;
                        m_chkImport.Add(material, chkMaterial);
                    }

                    if (TransferManagerModes.IsWarehouseMaterial(material))
                    {
                        UICheckBox? chkWarehouseMaterial = UIMyUtils.AddCheckbox(panelMaterialRestrictions, material.ToString(), UIFonts.SemiBold, 1.0f, !oSettings.IsImportRestricted(material), (index) => OnImportRestrictMaterialWarehouses(material, index));
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

            // ----------------------------------------------------------------
            // Experimental services options
            UIHelperBase groupExperimental = helper.AddGroup(Localization.Get("GROUP_IMPROVED_SERVICES_MATCHING"));
            UIPanel panelExperimental = (groupExperimental as UIHelper).self as UIPanel;
            UILabel txtDeathcareExperimental = UISettings.AddDescription(panelExperimental, "txtDeathcareExperimental", panelExperimental, 1.0f, Localization.Get("txtDeathcareExperimental"));
            m_chkDeathcareExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionDeathcareExperimental"), oSettings.ImprovedDeathcareMatching, OnExperimentalDeathcare);
            AddSaveGameSetting(m_chkDeathcareExperimental); 
            m_chkGarbageExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionGarbageExperimental"), oSettings.ImprovedGarbageMatching, OnExperimentalGarbage);
            AddSaveGameSetting(m_chkGarbageExperimental); 
            m_chkPoliceExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionPoliceExperimental"), oSettings.ImprovedCrimeMatching, OnExperimentalCrime);
            AddSaveGameSetting(m_chkPoliceExperimental);
            m_chkImprovedMailTransfers = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionImprovedMailTransfers"), oSettings.ImprovedMailTransfers, OnImprovedMailMatching);
            AddSaveGameSetting(m_chkImprovedMailTransfers);

            // ----------------------------------------------------------------
            // Sick Collection
            UIHelper groupSick = (UIHelper)helper.AddGroup(Localization.Get("GROUP_SICK_COLLECTION"));
            UISettings.AddDescription(groupSick, "txtOverrideSickHandler", 1.0f, Localization.Get("txtOverrideSickHandler"));
            m_chkOverrideSickCollection = (UICheckBox)groupSick.AddCheckbox(Localization.Get("optionOverrideSickHandler"), oSettings.OverrideSickHandler, OnOverrideResidentialSick);
            AddSaveGameSetting(m_chkOverrideSickCollection);
            groupSick.AddSpace(iSEPARATOR_HEIGHT);
            
            UISettings.AddDescription(groupSick, "txtSickSadNotification", 1.0f, Localization.Get("txtSickSadNotification"));
            m_chkDisplaySickNotification = (UICheckBox)groupSick.AddCheckbox(Localization.Get("optionDisplaySickNotification"), oSettings.DisplaySickNotification, OnDisplaySadNotification);
            AddSaveGameSetting(m_chkDisplaySickNotification);
            groupSick.AddSpace(iSEPARATOR_HEIGHT);

            UISettings.AddDescription(groupSick, "txtSickHelicopterRate", 1.0f, Localization.Get("txtSickHelicopterRate"));
            m_sliderSickHelicopterRate = SettingsSlider.CreateSettingsStyle(groupSick, LayoutDirection.Horizontal, Localization.Get("optionSickHelicopterRate"), 400, 200, 0f, 100f, 1.0f, (float)oSettings.SickHelicopterRate, 0, OnSickHelicopterRate);
            m_sliderSickHelicopterRate.Percent = true;
            AddSaveGameSetting(m_sliderSickHelicopterRate);

            UISettings.AddDescription(groupSick, "txtSickWalkRate", 1.0f, Localization.Get("txtSickWalkRate"));
            m_sliderSickWalkRate = SettingsSlider.CreateSettingsStyle(groupSick, LayoutDirection.Horizontal, Localization.Get("optionSickWalkRate"), 400, 200, 0f, 100f, 1.0f, (float)oSettings.SickWalkRate, 0, OnSickWalkRate);
            m_sliderSickWalkRate.Percent = true;
            AddSaveGameSetting(m_sliderSickWalkRate);

            // ----------------------------------------------------------------
            // Sick Generation
            UIHelper groupSickGeneration = (UIHelper)helper.AddGroup(Localization.Get("GROUP_SICK_GENERATE")); 
            UISettings.AddDescription(groupSickGeneration, "txtRandomSick", 1.0f, Localization.Get("txtRandomSick"));
            m_sliderSickGenerationRate = SettingsSlider.CreateSettingsStyle(groupSickGeneration, LayoutDirection.Horizontal, Localization.Get("txtRandomSickRate"), 400, 200, 0f, 10000f, 100.0f, (float)oSettings.RandomSickRate, 0, OnRandomSickRate);
            m_sliderSickGenerationRate.OffValue = 0;
            AddSaveGameSetting(m_sliderSickGenerationRate);
            UISettings.AddDescription(groupSickGeneration, "txtRandomSickRateScale", 1.0f, Localization.Get("txtRandomSickRateScale"));

            // ----------------------------------------------------------------
            // Crime
            UIHelper groupCrime = (UIHelper) helper.AddGroup(Localization.Get("GROUP_CRIME"));
            UISettings.AddDescription(groupCrime, "txtToughOnCrime", 1.0f, Localization.Get("txtToughOnCrime"));
            m_chkPoliceToughOnCrime = (UICheckBox)groupCrime.AddCheckbox(Localization.Get("optionPoliceToughOnCrime"), oSettings.PoliceToughOnCrime, OnPoliceToughOnCrime);
            AddSaveGameSetting(m_chkPoliceToughOnCrime);

            // ----------------------------------------------------------------
            // Main area building mail
            UIHelper groupMail = (UIHelper)helper.AddGroup(Localization.Get("reasonMail"));
            UISettings.AddDescription(groupMail, "txtMainBuildingMaxMail", 1.0f, Localization.Get("txtMainBuildingMaxMail"));
            groupMail.AddSpace(iSEPARATOR_HEIGHT);
            m_sliderMainBuildingMaxMail = SettingsSlider.CreateSettingsStyle(groupMail, LayoutDirection.Horizontal, Localization.Get("sliderMainBuildingMaxMail"), 320, 300, 2000f, 50000f, 1000f, (float)oSettings.MainBuildingMaxMail, 0, OnMainBuildingMaxMail);
            AddSaveGameSetting(m_sliderMainBuildingMaxMail);
            groupMail.AddSpace(iSEPARATOR_HEIGHT);

            UISettings.AddDescription(groupMail, "txtMainBuildingPostTruck", 1.0f, Localization.Get("txtMainBuildingPostTruck"));
            groupMail.AddSpace(iSEPARATOR_HEIGHT); 
            m_chkMainBuildingPostTruck = (UICheckBox)groupMail.AddCheckbox(Localization.Get("optionMainBuildingPostTruck"), oSettings.MainBuildingPostTruck, OnMainBuildingPostTruck);
            AddSaveGameSetting(m_chkMainBuildingPostTruck);

            // ----------------------------------------------------------------
            // Taxi Move
            UIHelper groupTaxiMove = (UIHelper)helper.AddGroup(Localization.Get("GROUP_TAXI_MOVE"));
            UISettings.AddDescription(groupTaxiMove, "txtTaxiMove", 1.0f, Localization.Get("txtTaxiMove"));
            m_chkTaxiMove = (UICheckBox)groupTaxiMove.AddCheckbox(Localization.Get("optionTaxiMove"), oSettings.TaxiMove, OnTaxiMove);
            AddSaveGameSetting(m_chkTaxiMove);

            groupTaxiMove.AddSpace(iSEPARATOR_HEIGHT);
            UISettings.AddDescription(groupTaxiMove, "txtTaxiStandDelay", 1.0f, Localization.Get("txtTaxiStandDelay"));
            m_sliderTaxiStandDelay = SettingsSlider.CreateSettingsStyle(groupTaxiMove, LayoutDirection.Horizontal, Localization.Get("sliderTaxiStandDelay"), 400, 200, 0f, 20f, 1.0f, (float)oSettings.TaxiStandDelay, 0, OnTaxiStandDelay);
            AddSaveGameSetting(m_sliderTaxiStandDelay);

            // ----------------------------------------------------------------
            // Prefer local
            UIHelperBase group1 = helper.AddGroup(Localization.Get("GROUP_SERVICE_DISTRICT_OPTIONS"));
            UIPanel txtPanel1 = (group1 as UIHelper).self as UIPanel;
            UISettings.AddDescription(txtPanel1, "txtPreferLocalService", txtPanel1, 1.0f, Localization.Get("txtPreferLocalService"));
            m_chkPreferLocal = (UICheckBox)group1.AddCheckbox(Localization.Get("optionPreferLocalService"), oSettings.PreferLocalService, (index) => setOptionPreferLocalService(index));
            AddSaveGameSetting(m_chkPreferLocal);
            group1.AddSpace(iSEPARATOR_HEIGHT);
            UISettings.AddDescription(txtPanel1, "txtPreferLocalServiceWarning", txtPanel1, 1.0f, Localization.Get("txtPreferLocalServiceWarning"));
        }

        public void SetupTransferDistancesTab(UIHelper helper)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();

            // Distance limits section
            UIHelper groupDistanceLimits = (UIHelper)helper.AddGroup(Localization.Get("GROUP_DISTANCE_LIMITS"));
            UIPanel panelDistanceLimits = (groupDistanceLimits as UIHelper).self as UIPanel;
            UISettings.AddDescription(panelDistanceLimits, "txtDistanceLimits", panelDistanceLimits, 1.0f, Localization.Get("txtDistanceLimits"));
            groupDistanceLimits.AddSpace(iSEPARATOR_HEIGHT);

            // Load distance reasons
            List<CustomTransferReason.Reason> reasons = new List<CustomTransferReason.Reason>();
            foreach (CustomTransferReason.Reason reason in Enum.GetValues(typeof(CustomTransferReason.Reason)))
            {
                if (TransferManagerModes.IsGlobalDistanceRestrictionsSupported(reason))
                {
                    reasons.Add(reason);
                }
            }

            reasons.Sort(SortBytName);

            foreach (CustomTransferReason.Reason reason in reasons)
            {
                AddDistanceSlider(groupDistanceLimits, reason);
            }
        }

        private static int SortBytName(CustomTransferReason.Reason x, CustomTransferReason.Reason y)
        {
            return x.ToString().CompareTo(y.ToString());
        }

        private void AddDistanceSlider(UIHelper helper, CustomTransferReason.Reason reason)
        {
            AddDistanceSlider(helper, reason, $"{reason} (km)");
        }

        private void AddDistanceSlider(UIHelper helper, CustomTransferReason.Reason reason, string strLabel)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            m_sliderLimits[reason] = SettingsSlider.CreateSettingsStyle(helper, LayoutDirection.Horizontal, strLabel, 400, 200, 0f, 20f, 0.5f, (float)oSettings.GetActiveDistanceRestrictionKm(reason), 1, (float value) => OnDistanceLimit(reason, value));
        }

        public void SetupVehicleAITab(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Experimental section
            UIHelper group = (UIHelper) helper.AddGroup(Localization.Get("GROUP_VEHICLE_AI"));
            UISettings.AddDescription(group, "txtVehicleAIDescription", 1.0f, Localization.Get("txtVehicleAIDescription"));
            group.AddCheckbox(Localization.Get("optionFireTruckAI"), oSettings.FireTruckAI, OnFireTruckAI);
            group.AddCheckbox(Localization.Get("optionFireCopterAI"), oSettings.FireCopterAI, OnFireCopterAI);
            group.AddCheckbox(Localization.Get("optionPostVanAI"), oSettings.PostVanAI, OnPostVanAI); 
            group.AddCheckbox(Localization.Get("optionGarbageTruckAI"), oSettings.GarbageTruckAI, OnGarbageTruckAI);
            group.AddCheckbox(Localization.Get("optionPoliceCarAI"), oSettings.PoliceCarAI, OnPoliceCarAI);
            group.AddCheckbox(Localization.Get("optionPoliceCopterAI"), oSettings.PoliceCopterAI, OnPoliceCopterAI);
            group.AddSpace(iSEPARATOR_HEIGHT);

            // Fire Truck Extinguish Nearby Trees
            UIHelper groupFiretruckExtinguishTrees = (UIHelper)helper.AddGroup(Localization.Get("GROUP_FIRETRUCK"));
            UISettings.AddDescription(groupFiretruckExtinguishTrees, "txtFiretruckExtinguishTrees", 1.0f, Localization.Get("txtFiretruckExtinguishTrees"));
            groupFiretruckExtinguishTrees.AddCheckbox(Localization.Get("optionFiretruckExtinguishTrees"), oSettings.FireTruckExtinguishTrees, (bool bChecked) => 
            { 
                oSettings.FireTruckExtinguishTrees = bChecked; 
                oSettings.Save(); 
            });
            group.AddSpace(iSEPARATOR_HEIGHT);
        }

        public void SetupAdvanced(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Experimental section
            UIHelperBase group = helper.AddGroup(Localization.Get("GROUP_PATCHES"));
            UIPanel panel = (group as UIHelper).self as UIPanel;
            UISettings.AddDescription(panel, "txtAdvancedDescription", panel, 1.0f, Localization.Get("txtAdvancedDescription"));

            UIHelper groupGeneral = (UIHelper)helper.AddGroup(Localization.Get("tabGeneral"));
            groupGeneral.AddCheckbox(Localization.Get("optionFixFindHospital"), oSettings.FixFindHospital, (bChecked) => { oSettings.FixFindHospital = bChecked; oSettings.Save(); });
            UISettings.AddDescription((groupGeneral as UIHelper).self as UIPanel, "txtFindHospital", panel, 1.0f, Localization.Get("txtFindHospital"));

            UIHelper groupIntercityStops = (UIHelper) helper.AddGroup(Localization.Get("GROUP_INTERCITY_STOPS"));
            UISettings.AddDescription((groupIntercityStops as UIHelper).self as UIPanel, "txtIntercityStopSpawnAtCount", panel, 1.0f, Localization.Get("txtIntercityStopSpawnAtCount"));
            groupIntercityStops.AddSpace(6);
            SettingsSlider sliderForceTrainSpawnAtCount = SettingsSlider.CreateSettingsStyle(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForceTrainSpawnAtCount"), 320, 300, 0f, 500f, 1f, (float)oSettings.ForceTrainSpawnAtCount, 0, (float value) => { oSettings.ForceTrainSpawnAtCount = (int)value; oSettings.Save(); });
            SettingsSlider sliderForceShipSpawnAtCount = SettingsSlider.CreateSettingsStyle(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForceShipSpawnAtCount"), 320, 300, 0f, 500f, 1f, (float)oSettings.ForceShipSpawnAtCount, 0, (float value) => { oSettings.ForceShipSpawnAtCount = (int)value; oSettings.Save(); });
            SettingsSlider sliderForcePlaneSpawnAtCount = SettingsSlider.CreateSettingsStyle(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForcePlaneSpawnAtCount"), 320, 300, 0f, 500f, 1f, (float)oSettings.ForcePlaneSpawnAtCount, 0, (float value) => { oSettings.ForcePlaneSpawnAtCount = (int)value; oSettings.Save(); });
            SettingsSlider sliderForceBusSpawnAtCount = SettingsSlider.CreateSettingsStyle(groupIntercityStops, LayoutDirection.Horizontal, Localization.Get("sliderForceBusSpawnAtCount"), 320, 300, 0f, 500f, 1f, (float)oSettings.ForceBusSpawnAtCount, 0, (float value) => { oSettings.ForceBusSpawnAtCount = (int)value; oSettings.Save(); });
            
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

            // ----------------------------------------------------------------
            // Maintenance section
            UIHelperBase groupMaintenance = helper.AddGroup(Localization.Get("GROUP_Maintenance"));
            UIPanel panelMaintenance = (groupMaintenance as UIHelper).self as UIPanel;

            // Release Broken pathing
            UISettings.AddDescription(panelMaintenance, "txtBrokenPathUnits", panelMaintenance, 1.0f, Localization.Get("txtBrokenPathUnits"));
            groupMaintenance.AddButton(Localization.Get("btnReleaseBrokenPathing"), () =>
            {
                int iReleased = PathUnitMaintenance.ReleaseBrokenPathUnits();
                if (m_lblPathUnitCount is not null)
                {
                    m_lblPathUnitCount.text = Localization.Get("txtPathUnitCount") + ": " + iReleased;
                }
            });
            m_lblPathUnitCount = UISettings.AddDescription(panelMaintenance, "txtPathUnitCount", panelMaintenance, 1.0f, Localization.Get("txtPathUnitCount") + ": 0");

            groupMaintenance.AddSpace(iSEPARATOR_HEIGHT);

            // Release Ghost vehicles
            UISettings.AddDescription(panelMaintenance, "txtReleaseGhostVehicles", panelMaintenance, 1.0f, Localization.Get("txtReleaseGhostVehicles"));
            groupMaintenance.AddButton(Localization.Get("btnReleaseGhostVehicles"), () => 
            {
                int iReleased = StuckVehicles.ReleaseGhostVehicles();
                if (m_lblGhostVehicleCount is not null)
                {
                    m_lblGhostVehicleCount.text = Localization.Get("txtGhostVehiclesCount") + ": " + iReleased;
                }
            });
            m_lblGhostVehicleCount = UISettings.AddDescription(panelMaintenance, "txtGhostVehiclesCount", panelMaintenance, 1.0f, Localization.Get("txtGhostVehiclesCount") + ": 0");

            // ----------------------------------------------------------------
            // Match Set Logging
            UIHelper groupLogging = (UIHelper)helper.AddGroup(Localization.Get("GROUP_MAINTENANCE_LOGGING"));

            // Log Material Dropdown
            LoadReasons();
            int iCurrentReason = GetReasonArrayIndex(oModSettings.MatchLogReason);
            UISettings.AddDescription(groupLogging, "txtMatchLogging", 1.0f, Localization.Get("txtMatchLogging"));
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


            // Log file path
            groupLogging.AddSpace(iSEPARATOR_HEIGHT);
            UISettings.AddDescription(groupLogging, "txtMatchLoggingPath", 1.0f, Localization.Get("txtMatchLoggingPath") + " " + Path.Combine(ModSettings.UserSettingsDir, "TransferManagerCE"));

            // ----------------------------------------------------------------
            UIHelper groupPathing = (UIHelper)helper.AddGroup(Localization.Get("GROUP_MAINTENANCE_PATHING"));
            groupPathing.AddCheckbox(Localization.Get("optionLogCitizenPathFailures"), oModSettings.LogCitizenPathFailures, OnLogCitizenPathFailuresChanged);
            UISettings.AddDescription(groupPathing, "txtLogCitizenPathFailureWarning", 1.0f, Localization.Get("txtLogCitizenPathFailureWarning"));

            // ----------------------------------------------------------------
            UIHelper groupTransferManager = (UIHelper) helper.AddGroup(Localization.Get("tabTransferManager"));

            // Reset settings
            m_btnResetTransferManagerSettings = (UIButton)groupTransferManager.AddButton(Localization.Get("btnResetTransferManagerSettings"), OnResetTransferManagerSettingsClicked);
            groupTransferManager.AddSpace(iSEPARATOR_HEIGHT);

            // Reset pathing
            groupTransferManager.AddButton(Localization.Get("btnResetPathingStatistics"), OnResetPathingStatisticsClicked);
            groupTransferManager.AddSpace(iSEPARATOR_HEIGHT);

            // Reset statistics
            groupTransferManager.AddButton(Localization.Get("buttonResetTransferStatistics"), OnResetTransferStatisticsClicked);
            groupTransferManager.AddSpace(iSEPARATOR_HEIGHT);
        }

        public void OnTabVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                UpdateTransferManagerSettings();
            }
        }

        public void OnShortcutKeyChanged(float mode)
        {
            ModSettings.GetSettings().Save();
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
            PathData.UpdateHeuristicScale();
            PathDistanceCache.Invalidate();
        }
        public void OnPathDistanceCargoDelay(float value)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PathDistanceCargoStationDelay = (int)Math.Round(value);

            // Invalidate Cache
            PathDistanceCache.Invalidate();
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

        public void OnPoliceToughOnCrime(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.PoliceToughOnCrime = bChecked;
        }

        public void OnMainBuildingMaxMail(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.MainBuildingMaxMail = (int) fValue;
        }

        public void OnMainBuildingPostTruck(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.MainBuildingPostTruck = bChecked;
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

            IndustrialBuildingAIGoodsPatch.PatchGenericIndustriesHandler();
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
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.FireTruckAI = bChecked;
            oSettings.Save();
        }

        public void OnFireCopterAI(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.FireCopterAI = bChecked;
            oSettings.Save();
        }
        public void OnPostVanAI(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.PostVanAI = bChecked;
            oSettings.Save();
        }

        public void OnGarbageTruckAI(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.GarbageTruckAI = bChecked;
            oSettings.Save();
        }

        public void OnPoliceCarAI(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.PoliceCarAI = bChecked;
            oSettings.Save();
        }

        public void OnPoliceCopterAI(bool bChecked)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.PoliceCopterAI = bChecked;
            oSettings.Save();
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
            TransferManagerMod.Instance.ClearSettings();

            // Update global settings
            UpdateTransferManagerSettings();
        }

        public void OnResetPathingStatisticsClicked()
        {
            PathFindFailure.Reset();
            HumanAIPathfindFailure.s_pathFailCount = 0;
            CarAIPathfindFailurePatch.s_pathFailCount = 0;
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
            Patcher.PatchReversibleTranspilers();
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

        public void OnWarehouseSmarterImportExport(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.WarehouseSmartImportExport = bChecked;
        }

        public void OnNewWarehouseTransferChanged(bool bChecked)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.NewInterWarehouseTransfer = bChecked;
        }

        public void OnOutsideConnectionPriority(TransportType type, float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            switch (type)
            {
                case TransportType.Road:
                    {
                        oSettings.OutsideRoadPriority = (int)fValue;
                        break;
                    }
                case TransportType.Plane:
                    {
                        oSettings.OutsidePlanePriority = (int)fValue;
                        break;
                    }
                case TransportType.Train:
                    {
                        oSettings.OutsideTrainPriority = (int)fValue;
                        break;
                    }
                case TransportType.Ship:
                    {
                        oSettings.OutsideShipPriority = (int)fValue;
                        break;
                    }
            }
        }
        public void OnExportVehicleLimit(float fValue)
        {
            SaveGameSettings oSettings = SaveGameSettings.GetSettings();
            oSettings.ExportVehicleLimit = (int)fValue;
        }
        public void UpdateTransferManagerSettings()
        {
            if (TransferManagerMod.Instance.IsLoaded)
            {
                SaveGameSettings oSettings = SaveGameSettings.GetSettings();
                m_chkEnableTransferManager.isChecked = oSettings.EnableNewTransferManager;

                // General tab
                m_dropdownBalanced.selectedIndex = (int)oSettings.BalancedMatchMode;
                m_dropdownPathDistanceServices.selectedIndex = oSettings.PathDistanceServices;
                m_dropdownPathDistanceGoods.selectedIndex = oSettings.PathDistanceGoods;
                m_chkEnablePathFailExclusion.isChecked = oSettings.EnablePathFailExclusion;
                m_sliderPathDistanceHeuristic.SetValue(oSettings.PathDistanceHeuristic);
                m_sliderCargoStationDelay.SetValue(oSettings.PathDistanceCargoStationDelay);
                m_chkDisableDummyTraffic.isChecked = oSettings.DisableDummyTraffic;
                m_chkApplyUnlimited.isChecked = oSettings.ApplyUnlimited;
                m_chkEmployOvereducatedWorkers.isChecked = oSettings.EmployOverEducatedWorkers;

                // Goods delivery
                m_chkFactoryFirst.isChecked = oSettings.FactoryFirst;
                m_chkOverrideGenericIndustriesHandler.isChecked = oSettings.OverrideGenericIndustriesHandler;

                m_chkImprovedWarehouseMatching.isChecked = oSettings.ImprovedWarehouseMatching;
                m_chkWarehouseSmarterImportExport.isChecked = oSettings.WarehouseSmartImportExport;
                m_chkNewInterWarehouseMatching.isChecked = oSettings.NewInterWarehouseTransfer;
                m_chkWarehouseFirst.isChecked = oSettings.WarehouseFirst;
                m_sliderWarehouseReservePercent.SetValue(oSettings.WarehouseReserveTrucksPercent);

                // Import/Export
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

                // Crime
                m_chkPoliceToughOnCrime.isChecked = oSettings.PoliceToughOnCrime;

                // Mail
                m_sliderMainBuildingMaxMail.SetValue(oSettings.MainBuildingMaxMail);
                m_chkMainBuildingPostTruck.isChecked = oSettings.MainBuildingPostTruck;

                //Taxi Move
                m_chkTaxiMove.isChecked = oSettings.TaxiMove;
                m_sliderTaxiStandDelay.SetValue(oSettings.TaxiStandDelay);

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

            bool bLoaded = TransferManagerMod.Instance.IsLoaded;

            EnableCheckbox(m_chkEnableTransferManager, bLoaded);

            // Enable / Disable each save game setting
            foreach (UIComponent component in m_saveGameSettings) 
            {
                if (component is UICheckBox checkbox)
                {
                    EnableCheckbox(checkbox, bLoaded && oSettings.EnableNewTransferManager);
                }
                else if (component is SettingsSlider slider)
                {
                    slider.Enable(bLoaded && oSettings.EnableNewTransferManager);
                }
                else if (component is UIDropDown dropDown)
                {
                    if (bLoaded && oSettings.EnableNewTransferManager)
                    {
                        dropDown.Enable();
                    }
                    else
                    {
                        dropDown.Disable();
                    }
                }
            }
                
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
