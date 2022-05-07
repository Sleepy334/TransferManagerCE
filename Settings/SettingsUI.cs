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

            UIHelperBase group0 = helper.AddGroup(Localization.Get("DEBUGPROFILE"));
            group0.AddCheckbox(Localization.Get("optionEnableNewTransferManager"), oSettings.optionEnableNewTransferManager, (index) => setOptionEnableNewTransferManager(index));

            UIHelperBase groupGeneral = helper.AddGroup(Localization.Get("GROUP_GENERAL"));
            groupGeneral.AddCheckbox(Localization.Get("optionPathfindChirper"), oSettings.optionPathfindChirper, (index) => setOptionPathfindChirper(index));
            UIPanel txtPanel0 = (groupGeneral as UIHelper).self as UIPanel;
            UILabel txtLabel0 = AddDescription(txtPanel0, "optionPathfindChirper_txt", txtPanel0, 1.0f, Localization.Get("optionPathfindChirper_txt"));


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
        }

        /* 
         * Code adapted from PropAnarchy under MIT license
         */
        private static readonly Color32 m_greyColor = new Color32(0xe6, 0xe6, 0xe6, 0xee);
        private static UILabel AddDescription(UIPanel panel, string name, UIComponent alignTo, float fontScale, string text)
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


        public static void setOptionEnableNewTransferManager(bool index)
        {
            ModSettings oSettings = ModSettings.GetSettings(); 
            oSettings.optionEnableNewTransferManager = index;
            oSettings.Save();
            //DebugLog.LogDebug(DebugLog.REASON_ALL, $"** OPTION ENABLE/DISABLE: {optionEnableNewTransferManager} **");
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
