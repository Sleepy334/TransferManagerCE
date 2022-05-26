using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransferManagerCE.Settings
{
    public class SettingsTextEN
    {
        public static Dictionary<string, string> s_SettingText = new Dictionary<string, string>();

        public static void Init()
        {
            if (s_SettingText.Count == 0)
            {
                // Tabs
                s_SettingText["tabTransferManager"] = "Transfer Manager";
                s_SettingText["tabCallAgain"] = "CallAgain";
                s_SettingText["tabMisc"] = "Misc.";

                // Transfer Manager
                s_SettingText["DEBUGPROFILE"] = "Custom Transfer Manager";
                s_SettingText["optionEnableNewTransferManager"] = "Enable Custom Transfer Manager";
                s_SettingText["GROUP_SERVICE_OPTIONS"] = "Service Options";
                s_SettingText["optionPreferLocalService"] = "Prefer local district services";
                s_SettingText["optionPreferLocalService_txt"] = "Further improve locality of services by matching low priority requests only with service buildings within the same district (or outside any district). Once an unfulfilled requests becomes high priority, it will be served by any nearby service offer though. Affects: garbage, police, health care, maintenance, mail, taxi.";
                s_SettingText["GROUP_WAREHOUSE_OPTIONS"] = "Goods Delivery Options";
                s_SettingText["optionWarehouseFirst"] = "Warehouse First";
                s_SettingText["optionWarehouseFirst_txt"] = "Prefer to conduct all goods delivery (inbound and outbound) through warehouses, if available. Will increase warehouse traffic, so your warehouses better be situated strategically well.";
                s_SettingText["optionWarehouseReserveTrucks"] = "Reserve warehouse cargo trucks for local transfers";
                s_SettingText["optionWarehouseReserveTrucks_txt"] = "Recommended to enable with Warehouse First: reserve 25% of warehouse truck capacity for satisfying city demand, prevents all trucks being used for export.";
                s_SettingText["optionWarehouseNewBalanced"] = "Warehouse improved locality";
                s_SettingText["optionWarehouseNewBalanced_txt"] = "Warehouses will wait to buy from local warehouses or sell to local warehouse before deciding to import/export respectively. This should reduce unnecessary import/export and make intra-city warehouse transfers more reliable. Note that imports/exports will still happen.";
                s_SettingText["GROUP_EXPORTIMPORT_OPTIONS"] = "Export/Import Options";
                s_SettingText["optionPreferExportShipPlaneTrain"] = "Import/Export prefer to use ship/plane/train instead of highway";
                s_SettingText["optionPreferExportShipPlaneTrain_txt"] = "All import/exports attempt to prioritize usage of available ship, train or plane connections instead of roads, even if a road connection is closer.";
                s_SettingText["GROUP_EXPERIMENTAL_DEATHCARE"] = "Experimental";
                s_SettingText["optionDeathcareExperimental"] = "Improved death care transfers"; 
                s_SettingText["txtDeathcareExperimental"] = "By default Cemeteries send out a transfer offer for one hearse at a time.This option changes the offer to be the number of available hearses, which can result in improved deathcare responsiveness.";
                
                // Call again
                s_SettingText["CallAgainEnabled"] = "Enable Call Again";
                s_SettingText["sliderCallAgainUpdateRate"] = "Call Again update rate";
                s_SettingText["CallAgainDescriptionThreshold"] = "Threshold: Lower thresholds start calling sooner.";
                s_SettingText["CallAgainDescriptionRate"] = "Rate: Lower rates produce callbacks more often until a response is received.";
                s_SettingText["GROUP_CALLAGAIN_HEALTHCARE"] = "Health Care";
                s_SettingText["CallAgainHealthcareThreshold"] = "Healthcare Threshold";
                s_SettingText["CallAgainHealthcareRate"] = "Healthcare Callback Rate";
                s_SettingText["GROUP_CALLAGAIN_DEATHCARE"] = "Death Care";
                s_SettingText["CallAgainDeathcareThreshold"] = "Deathcare Threshold";
                s_SettingText["CallAgainDeathcareRate"] = "Deathcare Callback Rate";

                // Misc Tab
                s_SettingText["GROUP_TRANSFERISSUE_PANEL"] = "Transfer Issue Panel";
                s_SettingText["keyOpenTransferIssuePanel"] = "Open Transfer Issue Panel";
                s_SettingText["optionShowIssuesWithVehiclesOnRoute"] = "Show issues with vehicles on route";
                s_SettingText["sliderTransferIssueDeleteResolvedDelay"] = "Delete resolved delay";

                // Statistics
                s_SettingText["GROUP_STATISTICS_PANEL"] = "Statistics Panel";
                s_SettingText["keyOpenStatisticsPanel"] = "Open Statistics Panel";
                s_SettingText["StatisticsPanelEnabled"] = "Enable Statistics";
                s_SettingText["btnResetPathingStatistics"] = "Reset Pathing Statistics";
                s_SettingText["buttonResetTransferStatistics"] = "Reset Transfer Statistics";

                // Building Panel
                s_SettingText["tabBuildingPanelSettings"] = "Settings";
                s_SettingText["GROUP_BUILDINGPANEL_SERVICES"] = "Service Options";
                s_SettingText["chkBuildingPanelPreferLocalServices"] = "Prefer local district services";
                s_SettingText["GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS"] = "Outside Connections";
                s_SettingText["dropdownBuildingPanelImportExport"] = "Import/Export";
                s_SettingText["GROUP_BUILDINGPANEL_GOODS_DELIVERY"] = "Goods Delivery Options";
                s_SettingText["chkBuildingPanelReserveWarehouseTrucks"] = "Reserve warehouse cargo trucks for local transfers";
                s_SettingText["dropBuildingPanelImportExport1"] = "Allow Import and Export";
                s_SettingText["dropBuildingPanelImportExport2"] = "Allow Import Only";
                s_SettingText["dropBuildingPanelImportExport3"] = "Allow Export Only";
                s_SettingText["dropBuildingPanelImportExport4"] = "Neither Allowed";

                s_SettingText["tabBuildingPanelStatus"] = "Status";
                s_SettingText["listBuildingPanelStatusColumn1"] = "Material";
                s_SettingText["listBuildingPanelStatusColumn2"] = "Value";
                s_SettingText["listBuildingPanelStatusColumn3"] = "Responder";
                s_SettingText["listBuildingPanelStatusColumn4"] = "Vehicle";
                s_SettingText["listBuildingPanelStatusColumn5"] = "Description";

                s_SettingText["tabBuildingPanelTransfers"] = "Transfers";
                s_SettingText["labelBuildingPanelOffers"] = "Transfer Offers:";
                s_SettingText["labelBuildingPanelMatchOffers"] = "Match Offers";

                s_SettingText["listBuildingPanelOffersColumn1"] = "In/Out";
                s_SettingText["listBuildingPanelOffersColumn2"] = "Material";
                s_SettingText["listBuildingPanelOffersColumn3"] = "Priority";
                s_SettingText["listBuildingPanelOffersColumn4"] = "Active";
                s_SettingText["listBuildingPanelOffersColumn5"] = "Amount";
                s_SettingText["listBuildingPanelOffersColumn6"] = "Description";

                s_SettingText["listBuildingPanelMatchesColumn1"] = "Time";
                s_SettingText["listBuildingPanelMatchesColumn2"] = "Material";
                s_SettingText["listBuildingPanelMatchesColumn3"] = "Active";
                s_SettingText["listBuildingPanelMatchesColumn4"] = "Amount";
                s_SettingText["listBuildingPanelMatchesColumn5"] = "Delta";
                s_SettingText["listBuildingPanelMatchesColumn6"] = "Description";

                // Transfer issues panel
                s_SettingText["titleTransferIssuesPanel"] = "Transfer Issues";

                s_SettingText["tabTransferIssuesPathing"] = "Pathing";
                s_SettingText["listPathingColumn1"] = "Time";
                s_SettingText["listPathingColumn2"] = "Source";
                s_SettingText["listPathingColumn3"] = "Target";

                s_SettingText["tabTransferIssuesDead"] = "Dead";
                s_SettingText["listDeadColumn1"] = "Dead";
                s_SettingText["listDeadColumn2"] = "Timer";
                s_SettingText["listDeadColumn3"] = "Source";
                s_SettingText["listDeadColumn4"] = "Target";
                s_SettingText["listDeadColumn5"] = "Vehicle";

                s_SettingText["tabTransferIssuesSick"] = "Sick";
                s_SettingText["listSickColumn1"] = "Sick";
            }
        }

        public static string GetSettingText(string sSetting)
        {
            Init();
            if (s_SettingText.ContainsKey(sSetting))
            {
                return s_SettingText[sSetting];
            }
            else
            {
                return sSetting;
            }
        }
    }
}
