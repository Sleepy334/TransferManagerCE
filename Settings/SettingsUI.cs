using ColossalFramework.UI;
using ICities;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class SettingsUI
    {
		public SettingsUI()
        {
        }

        public void OnSettingsUI(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Title
            UIComponent pnlMain = (UIComponent)helper.self;
            UILabel txtTitle = AddDescription(pnlMain, "title", pnlMain, 1.0f, TransferManagerMain.Title);
            txtTitle.textScale = 1.2f;

            // Add tabstrip.
            ExtUITabstrip tabStrip = ExtUITabstrip.Create(helper);
            UIHelper tabTransferManager = tabStrip.AddTabPage(Localization.Get("tabTransferManager"), true);
            UIHelper tabCallAgain = tabStrip.AddTabPage(Localization.Get("tabCallAgain"), true);
            UIHelper tabMisc = tabStrip.AddTabPage(Localization.Get("tabMisc"), true);

            // Tranfer Manager tab
            SetupTransferManagerTab(tabTransferManager);

            // Call Again tab
            SetupCallAgainTab(tabCallAgain);

            SetupMiscTab(tabMisc);
        }

        public void SetupTransferManagerTab(UIHelper helper)
        {
			ModSettings oSettings = ModSettings.GetSettings();

            UIHelperBase group0 = helper.AddGroup(Localization.Get("DEBUGPROFILE"));
            group0.AddCheckbox(Localization.Get("optionEnableNewTransferManager"), oSettings.optionEnableNewTransferManager, (index) => setOptionEnableNewTransferManager(index));

            UIHelperBase group1 = helper.AddGroup(Localization.Get("GROUP_SERVICE_OPTIONS"));
            group1.AddCheckbox(Localization.Get("optionPreferLocalService"), oSettings.optionPreferLocalService, (index) => setOptionPreferLocalService(index));
            UIPanel txtPanel1 = (group1 as UIHelper).self as UIPanel;
            UILabel txtLabel1 = AddDescription(txtPanel1, "optionPreferLocalService_txt", txtPanel1, 1.0f, Localization.Get("optionPreferLocalService_txt"));


            UIHelperBase group2 = helper.AddGroup(Localization.Get("GROUP_WAREHOUSE_OPTIONS"));
            group2.AddCheckbox(Localization.Get("optionWarehouseFirst"), oSettings.optionWarehouseFirst, (index) => setOptionWarehouseFirst(index));
            UIPanel txtPanel2 = (group2 as UIHelper).self as UIPanel;
            UILabel txtLabel21 = AddDescription(txtPanel2, "optionWarehouseFirst_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseFirst_txt"));
            UILabel txtLabel21_spacer = AddDescription(txtPanel2, "txtLabel21_spacer", txtPanel2, 1.0f, "");

            group2.AddCheckbox(Localization.Get("optionWarehouseReserveTrucks"), oSettings.optionWarehouseReserveTrucks, (index) => setOptionWarehouseReserveTrucks(index));
            UILabel txtLabel22 = AddDescription(txtPanel2, "optionWarehouseReserveTrucks_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseReserveTrucks_txt"));
            UILabel txtLabel22_spacer = AddDescription(txtPanel2, "txtLabel22_spacer", txtPanel2, 1.0f, "");

            group2.AddCheckbox(Localization.Get("optionWarehouseNewBalanced"), oSettings.optionWarehouseNewBalanced, (index) => setOptionWarehouseNewBalanced(index));
            UILabel txtLabel23 = AddDescription(txtPanel2, "optionWarehouseNewBalanced_txt", txtPanel2, 1.0f, Localization.Get("optionWarehouseNewBalanced_txt"));


            UIHelperBase group3 = helper.AddGroup(Localization.Get("GROUP_EXPORTIMPORT_OPTIONS"));
            group3.AddCheckbox(Localization.Get("optionPreferExportShipPlaneTrain"), oSettings.optionPreferExportShipPlaneTrain, (index) => setOptionPreferExportShipPlaneTrain(index));
            UIPanel txtPanel3 = (group3 as UIHelper).self as UIPanel;
            UILabel txtLabel3 = AddDescription(txtPanel3, "optionPreferExportShipPlaneTrain_txt", txtPanel3, 1.0f, Localization.Get("optionPreferExportShipPlaneTrain_txt"));

            // Experimental section
            UIHelperBase groupExperimental = helper.AddGroup(Localization.Get("GROUP_EXPERIMENTAL_DEATHCARE"));
            UIPanel panelExperimental = (groupExperimental as UIHelper).self as UIPanel;
            groupExperimental.AddCheckbox(Localization.Get("optionDeathcareExperimental"), oSettings.TransferManagerExperimentalDeathcare, OnExperimentalDeathcare);
            UILabel txtDeathcareExperimental = AddDescription(panelExperimental, "txtDeathcareExperimental", panelExperimental, 1.0f, Localization.Get("txtDeathcareExperimental"));
        }

        public void SetupCallAgainTab(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            helper.AddCheckbox(Localization.Get("CallAgainEnabled"), oSettings.CallAgainEnabled, OnCallAgainChanged);
            SettingsSlider.Create(helper, Localization.Get("sliderCallAgainUpdateRate"), 2f, 10f, 1f, (float)oSettings.CallAgainUpdateRate, OnCallAgainUpdateRateValueChanged);
            UIScrollablePanel pnlPanel3 = (UIScrollablePanel) helper.self;
            UILabel txtLabel1 = AddDescription(pnlPanel3, "CallAgainDescriptionThreshold", pnlPanel3, 1.0f, Localization.Get("CallAgainDescriptionThreshold"));
            UILabel txtLabel2 = AddDescription(pnlPanel3, "CallAgainDescriptionRate", pnlPanel3, 1.0f, Localization.Get("CallAgainDescriptionRate"));
            
            // Health care threshold Slider
            UIHelper oHealthcareGroup = (UIHelper)helper.AddGroup(Localization.Get("GROUP_CALLAGAIN_HEALTHCARE"));
            SettingsSlider oHealthcareThresholdSlider = SettingsSlider.Create(oHealthcareGroup, Localization.Get("CallAgainHealthcareThreshold"), 0f, 255f, 1f, (float)oSettings.HealthcareThreshold, OnHealthcareThresholdValueChanged);
            SettingsSlider oHealthcareRateSlider = SettingsSlider.Create(oHealthcareGroup, Localization.Get("CallAgainHealthcareRate"), 1f, 30f, 1f, (float)oSettings.HealthcareRate, OnHealthcareRateValueChanged);

            // Health care threshold Slider
            UIHelper oDeathcareGroup = (UIHelper)helper.AddGroup(Localization.Get("GROUP_CALLAGAIN_DEATHCARE"));
            SettingsSlider oDeathcareThresholdSlider = SettingsSlider.Create(oDeathcareGroup, Localization.Get("CallAgainDeathcareThreshold"), 0f, 255f, 1f, (float)oSettings.DeathcareThreshold, OnDeathcareThresholdValueChanged);
            SettingsSlider oDeathcareRateSlider = SettingsSlider.Create(oDeathcareGroup, Localization.Get("CallAgainDeathcareRate"), 1f, 30f, 1f, (float)oSettings.DeathcareRate, OnDeathcareRateValueChanged);
        }

        public void SetupMiscTab(UIHelper helper)
        {
            ModSettings oSettings = ModSettings.GetSettings();

            // Transfer Issue Panel
            UIHelper groupTransferIssue = (UIHelper)helper.AddGroup(Localization.Get("GROUP_TRANSFERISSUE_PANEL"));
            UIPanel panel = (UIPanel)groupTransferIssue.self;
            UIKeymappingsPanel keymappingsTransferIssue = panel.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsTransferIssue.AddKeymapping(Localization.Get("keyOpenTransferIssuePanel"), ModSettings.TransferIssueHotkey); // Automatically saved
            groupTransferIssue.AddCheckbox(Localization.Get("optionShowIssuesWithVehiclesOnRoute"), oSettings.TransferIssueShowWithVehiclesOnRoute, OnVehiclesOnRouteChanged);

            // Statistics group
            UIHelper groupStats = (UIHelper)helper.AddGroup(Localization.Get("GROUP_STATISTICS_PANEL"));
            groupStats.AddCheckbox(Localization.Get("StatisticsPanelEnabled"), oSettings.StatisticsEnabled, OnStatisticsEnabledChanged);

            UIPanel panelStats = (UIPanel)groupStats.self;
            UIKeymappingsPanel keymappingsStatsPanel = panelStats.gameObject.AddComponent<UIKeymappingsPanel>();
            keymappingsStatsPanel.AddKeymapping(Localization.Get("keyOpenStatisticsPanel"), ModSettings.StatsPanelHotkey); // Automatically saved
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

        public static void OnVehiclesOnRouteChanged(bool value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.TransferIssueShowWithVehiclesOnRoute = value;
            oSettings.Save();
        }

        public static void OnStatisticsEnabledChanged(bool value)
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

        public static void OnCallAgainUpdateRateValueChanged(float value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.CallAgainUpdateRate = (int)value;
            oSettings.Save();
        }

        public static void OnExperimentalDeathcare(bool enabled)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.TransferManagerExperimentalDeathcare = enabled;
            oSettings.Save();
        }

        public static void OnDeathcareThresholdValueChanged(float value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.DeathcareThreshold = (int)value;
            oSettings.Save();
        }

        public static void OnDeathcareRateValueChanged(float value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.DeathcareRate = (int)value;
            oSettings.Save();
        }

        public static void OnCallAgainChanged(bool enabled)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.CallAgainEnabled = enabled;
            oSettings.Save();
        }

        public static void OnHealthcareThresholdValueChanged(float value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.HealthcareThreshold = (int)value;
            oSettings.Save();
        }

        public static void OnHealthcareRateValueChanged(float value)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.HealthcareRate = (int)value;
            oSettings.Save();
        }

        public static void setOptionEnableNewTransferManager(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings(); 
            oSettings.optionEnableNewTransferManager = index;
            oSettings.Save();

            // Reset the stats as we have changed Transfer Manager.
            TransferManagerStats.Init();
        }

        public static void setOptionPreferLocalService(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionPreferLocalService = index;
            oSettings.Save();
        }

        public static void setOptionWarehouseFirst(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionWarehouseFirst = index;
            oSettings.Save();
        }

        public static void setOptionWarehouseReserveTrucks(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionWarehouseReserveTrucks = index;
            oSettings.Save();
        }

        public static void setOptionPreferExportShipPlaneTrain(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionPreferExportShipPlaneTrain = index;
            oSettings.Save();
        }

        public static void setOptionWarehouseNewBalanced(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionWarehouseNewBalanced = index;
            oSettings.Save();
        }

        public static void setOptionPathfindChirper(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings();
            oSettings.optionPathfindChirper = index;
            oSettings.Save();
        }
    }
}
