using ColossalFramework.UI;
using ICities;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class SettingsUI
    {
        private SettingsSlider? m_CallAgainUpdateRateSlider = null;
        private SettingsSlider? m_oHealthcareThresholdSlider = null;
        private SettingsSlider? m_oHealthcareRateSlider = null;
        private SettingsSlider? m_oDeathcareThresholdSlider = null;
        private SettingsSlider? m_oDeathcareRateSlider = null;
        private SettingsSlider? m_oGoodsThresholdSlider = null;
        private SettingsSlider? m_oGoodsRateSlider = null;
        private UICheckBox? m_checkDespawnCargoTrucks = null;

        private UICheckBox? m_chkPreferLocal = null;
        private UICheckBox? m_chkWarehouseFirst = null;
        private UICheckBox? m_chkWarehouseReserveTrucks = null;
        private UICheckBox? m_chkPreferShipPlaneTrain = null;
        private UICheckBox? m_chkDeathcareExperimental = null; 

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
            UIHelper tabGeneral = tabStrip.AddTabPage(Localization.Get("tabGeneral"), true);
            UIHelper tabTransferManager = tabStrip.AddTabPage(Localization.Get("tabTransferManager"), true);

            SetupGeneralTab(tabGeneral);

            // Tranfer Manager tab
            SetupTransferManagerTab(tabTransferManager);
        }

        public void SetupGeneralTab(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            UIHelper groupLocalisation = (UIHelper)helper.AddGroup(Localization.Get("GROUP_LOCALISATION"));
            groupLocalisation.AddDropdown(Localization.Get("dropdownLocalization"), Localization.GetLoadedLanguages(), Localization.GetLanguageIndexFromCode(oSettings.PreferredLanguage), OnLocalizationDropDownChanged);

            // Transfer Issue Panel
            UIHelper groupTransferIssue = (UIHelper)helper.AddGroup(Localization.Get("GROUP_TRANSFERISSUE_PANEL"));
            UIPanel panel = (UIPanel)groupTransferIssue.self;
            UIKeymappingsPanel keymappingsTransferIssue = panel.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsTransferIssue.AddKeymapping(Localization.Get("keyOpenTransferIssuePanel"), ModSettings.TransferIssueHotkey); // Automatically saved
            SettingsSlider.Create(groupTransferIssue, Localization.Get("sliderTransferIssueDeleteResolvedDelay"), 0f, 100f, 1f, (float)oSettings.TransferIssueDeleteResolvedDelay, OnIsseuDeleteDelayValueChanged);
            groupTransferIssue.AddButton(Localization.Get("btnResetPathingStatistics"), OnResetPathingStatisticsClicked);

            // Building Panel
            UIHelper groupBuildingPanel = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BUILDING_PANEL"));
            UIPanel panelBuilding = (UIPanel)groupBuildingPanel.self;
            UIKeymappingsPanel keymappingsBuildingPanel = panelBuilding.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsBuildingPanel.AddKeymapping(Localization.Get("keyOpenBuildingPanel"), ModSettings.SelectionToolHotkey); // Automatically saved

            // Statistics group
            UIHelper groupStats = (UIHelper)helper.AddGroup(Localization.Get("GROUP_STATISTICS_PANEL"));
            groupStats.AddCheckbox(Localization.Get("StatisticsPanelEnabled"), oSettings.StatisticsEnabled, OnStatisticsEnabledChanged);
            UIPanel panelStats = (UIPanel)groupStats.self;
            UIKeymappingsPanel keymappingsStatsPanel = panelStats.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsStatsPanel.AddKeymapping(Localization.Get("keyOpenStatisticsPanel"), ModSettings.StatsPanelHotkey); // Automatically saved
            groupStats.AddButton(Localization.Get("buttonResetTransferStatistics"), OnResetTransferStatisticsClicked);
        }

        public void SetupTransferManagerTab(UIHelper helper)
        {
			ModSettings oSettings = ModSettings.GetSettings();

            UIHelperBase group0 = helper.AddGroup(Localization.Get("DEBUGPROFILE"));
            group0.AddCheckbox(Localization.Get("optionEnableNewTransferManager"), oSettings.optionEnableNewTransferManager, (index) => setOptionEnableNewTransferManager(index));
           
            UIHelperBase group1 = helper.AddGroup(Localization.Get("GROUP_SERVICE_OPTIONS"));
            m_chkPreferLocal = (UICheckBox) group1.AddCheckbox(Localization.Get("optionPreferLocalService"), oSettings.optionPreferLocalService, (index) => setOptionPreferLocalService(index));
            UIPanel txtPanel1 = (group1 as UIHelper).self as UIPanel;
            UILabel txtLabel1 = AddDescription(txtPanel1, "optionPreferLocalService_txt", txtPanel1, 1.0f, Localization.Get("optionPreferLocalService_txt"));

            UIHelperBase group2 = helper.AddGroup(Localization.Get("GROUP_WAREHOUSE_OPTIONS"));
            m_chkWarehouseFirst = (UICheckBox)group2.AddCheckbox(Localization.Get("optionWarehouseFirst"), oSettings.optionWarehouseFirst, (index) => setOptionWarehouseFirst(index));
            UIPanel txtPanel2 = (group2 as UIHelper).self as UIPanel;
            UILabel txtLabel21 = AddDescription(txtPanel2, "optionWarehouseFirst_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseFirst_txt"));
            UILabel txtLabel21_spacer = AddDescription(txtPanel2, "txtLabel21_spacer", txtPanel2, 1.0f, "");

            m_chkWarehouseReserveTrucks = (UICheckBox)group2.AddCheckbox(Localization.Get("optionWarehouseReserveTrucks"), oSettings.optionWarehouseReserveTrucks, (index) => setOptionWarehouseReserveTrucks(index));
            UILabel txtLabel22 = AddDescription(txtPanel2, "optionWarehouseReserveTrucks_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseReserveTrucks_txt"));
            UILabel txtLabel22_spacer = AddDescription(txtPanel2, "txtLabel22_spacer", txtPanel2, 1.0f, "");

            UIHelperBase group3 = helper.AddGroup(Localization.Get("GROUP_EXPORTIMPORT_OPTIONS"));
            m_chkPreferShipPlaneTrain = (UICheckBox)group3.AddCheckbox(Localization.Get("optionPreferExportShipPlaneTrain"), oSettings.optionPreferExportShipPlaneTrain, (index) => setOptionPreferExportShipPlaneTrain(index));
            UIPanel txtPanel3 = (group3 as UIHelper).self as UIPanel;
            UILabel txtLabel3 = AddDescription(txtPanel3, "optionPreferExportShipPlaneTrain_txt", txtPanel3, 1.0f, Localization.Get("optionPreferExportShipPlaneTrain_txt"));

            // Experimental section
            UIHelperBase groupExperimental = helper.AddGroup(Localization.Get("GROUP_EXPERIMENTAL_DEATHCARE"));
            UIPanel panelExperimental = (groupExperimental as UIHelper).self as UIPanel;
            m_chkDeathcareExperimental = (UICheckBox)groupExperimental.AddCheckbox(Localization.Get("optionDeathcareExperimental"), oSettings.TransferManagerExperimentalDeathcare, OnExperimentalDeathcare);
            UILabel txtDeathcareExperimental = AddDescription(panelExperimental, "txtDeathcareExperimental", panelExperimental, 1.0f, Localization.Get("txtDeathcareExperimental"));

            UpdateTransferManagerEnabled();
        }

        /* 
         * Code adapted from PropAnarchy under MIT license
         */
        private static readonly Color32 m_greyColor = new Color32(0xe6, 0xe6, 0xe6, 0xee);
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
            desc.relativePosition = new UnityEngine.Vector3(alignTo.relativePosition.x + 26f, alignTo.relativePosition.y + alignTo.height + 10);
            return desc;
        }

        public void OnLocalizationDropDownChanged(int value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.PreferredLanguage = Localization.GetLoadedCodes()[value];
            oSettings.Save();
        }

        public void OnIsseuDeleteDelayValueChanged(float fValue)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.TransferIssueDeleteResolvedDelay = (int)fValue;
            oSettings.Save();
        }

        public void OnResetTransferStatisticsClicked()
        {
            TransferManagerStats.Init();
        }

        public void OnResetPathingStatisticsClicked()
        {
            PathFindFailure.ResetPathingStatistics();
        }

        public void OnVehiclesOnRouteChanged(bool value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.TransferIssueShowWithVehiclesOnRoute = value;
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
                TransferManagerStats.Init();
            }
        }

        public void OnExperimentalDeathcare(bool enabled)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.TransferManagerExperimentalDeathcare = enabled;
            oSettings.Save();
        }

        public void setOptionEnableNewTransferManager(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings(); 
            oSettings.optionEnableNewTransferManager = index;
            oSettings.Save();

            // Reset the stats as we have changed Transfer Manager.
            TransferManagerStats.Init();
            UpdateTransferManagerEnabled();
        }

        public void setOptionPreferLocalService(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionPreferLocalService = index;
            oSettings.Save();
        }

        public void setOptionWarehouseFirst(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionWarehouseFirst = index;
            oSettings.Save();
        }

        public void setOptionWarehouseReserveTrucks(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionWarehouseReserveTrucks = index;
            oSettings.Save();
        }

        public void setOptionPreferExportShipPlaneTrain(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionPreferExportShipPlaneTrain = index;
            oSettings.Save();
        }

        public void UpdateTransferManagerEnabled()
        {
            ModSettings oSettings = ModSettings.GetSettings();

            if (oSettings.optionEnableNewTransferManager)
            {
                m_chkPreferLocal.Enable();
                m_chkWarehouseFirst.Enable();
                m_chkWarehouseReserveTrucks.Enable();
                m_chkPreferShipPlaneTrain.Enable();
                m_chkDeathcareExperimental.Enable();

            }
            else
            {
                m_chkPreferLocal.Disable();
                m_chkWarehouseFirst.Disable();
                m_chkWarehouseReserveTrucks.Disable();
                m_chkPreferShipPlaneTrain.Disable();
                m_chkDeathcareExperimental.Disable();
            }
        }
    }
}
