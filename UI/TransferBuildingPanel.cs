using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
{
    public class TransferBuildingPanel : UIPanel
    {
        enum TabIndex
        {
            TAB_SETTINGS,
            TAB_STATUS,
            TAB_VEHICLES,
            TAB_TRANSFERS,
        }
        
        public static TransferBuildingPanel? Instance = null;

        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;

        public const int iLISTVIEW_MATCHES_HEIGHT = 250;
        public const int iLISTVIEW_OFFERS_HEIGHT = 196;

        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 200;
        public const int iCOLUMN_WIDTH_250 = 250;
        public const int iCOLUMN_WIDTH_300 = 300;
        

        private UITitleBar? m_title = null;
        private UITabStrip? m_tabStrip = null;

        // Settings tab
        private UIDropDown? m_dropPreferLocalIncoming = null;
        private UIDropDown? m_dropPreferLocalOutgoing = null;
        private UIPanel? m_panelImportExport = null;
        private UICheckBox? m_chkAllowImport = null;
        private UICheckBox? m_chkAllowExport = null;
        private UIPanel? m_panelGoodsDelivery = null;
        private UICheckBox? m_chkReserveCargoTrucks = null;
        private UILabel? m_lblCustomTransferManagerWarning = null;
        private UILabel? m_lblDetectedDistricts = null;
        private UIButton? m_btnApplyToAllDistrict = null;
        private UIButton? m_btnApplyToAllPark = null;
        private UIButton? m_btnApplyToAllMap = null;

        // Status tab
        private ListView m_listStatus = null;
        private ListView m_listVehicles = null;

        // Transfers tab
        private UILabel? m_lblSource = null;
        private UILabel? m_lblOffers = null;
        private UILabel? m_lblMatches = null;
        private ListView m_listOffers = null;
        private ListView m_listMatches = null;

        public ushort m_buildingId = 0;

        private List<OfferData> m_TransferOffers = null;

        public TransferBuildingPanel() : base()
        {
            m_TransferOffers = new List<OfferData>();
        }

        public static bool IsVisible()
        {
            return Instance != null && Instance.isVisible;
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(TransferBuildingPanel)) as TransferBuildingPanel;
                if (Instance == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Transfer Building Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "TransferBuildingPanel";
            width = 700;
            height = 600;
            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            clipChildren = true;
            eventPositionChanged += (sender, e) =>
            {
                ModSettings settings = ModSettings.GetSettings();
                settings.TransferIssueLocationSaved = true;
                settings.TransferIssueLocation = absolutePosition;
                settings.Save();
            };

            if (ModSettings.GetSettings().TransferBuildingLocationSaved)
            {
                absolutePosition = ModSettings.GetSettings().TransferBuildingLocation;
                FitToScreen();
            }
            else
            {
                CenterToParent();
            }
            
            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.SetStatsHandler(OnStatsClick);
            m_title.title = "Transfer Manager CE";

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height;
            mainPanel.padding = new RectOffset(iMARGIN, iMARGIN, iMARGIN, iMARGIN);
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;

            // Object label
            m_lblSource = mainPanel.AddUIComponent<UILabel>();
            m_lblSource.autoSize = true;
            m_lblSource.padding = new RectOffset(4, 4, 4, 4);
            m_lblSource.text = "Select Building";
            m_lblSource.textAlignment = UIHorizontalAlignment.Center;
            m_lblSource.eventMouseEnter += (c, e) =>
            {
                m_lblSource.textColor = new Color32(13, 183, 255, 255);
            };
            m_lblSource.eventMouseLeave += (c, e) =>
            {
                m_lblSource.textColor = Color.white;
            };
            m_lblSource.eventClick += (c, e) =>
            {
                if (m_buildingId != 0)
                {
                    InstanceID oInstanceId = new InstanceID { Building = m_buildingId };
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                    Vector3 oPosition = building.m_position;
                    ToolsModifierControl.cameraController.SetTarget(oInstanceId, oPosition, false);
                }
            };

            m_tabStrip = UITabStrip.Create(mainPanel, width - 20f, height - m_lblSource.height - m_title.height - 10, OnTabChanged);

            UIPanel tabSettings = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelSettings"));
            if (tabSettings != null)
            {
                tabSettings.autoLayout = true;
                tabSettings.autoLayoutDirection = LayoutDirection.Vertical;
                tabSettings.padding = new RectOffset(10, 10, 10, 10);

                m_lblCustomTransferManagerWarning = tabSettings.AddUIComponent<UILabel>();
                if (m_lblCustomTransferManagerWarning != null)
                {
                    m_lblCustomTransferManagerWarning.text = Localization.Get("txtBuildingPanelTransferManagerWarning");
                    m_lblCustomTransferManagerWarning.textColor = Color.red;
                }

                UIHelper helper = new UIHelper(tabSettings);

                // Prefer local services
                UIHelper helperServiceOptions = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BUILDINGPANEL_DISTRICT"));
                string[] itemsPreferLocal = {
                    Localization.Get("dropdownBuildingPanelPreferLocal1"),
                    Localization.Get("dropdownBuildingPanelPreferLocal2"),
                    Localization.Get("dropdownBuildingPanelPreferLocal3"),
                };
                m_dropPreferLocalIncoming = UIUtils.AddDropDown(helperServiceOptions, Localization.Get("dropdownBuildingPanelIncomingPreferLocalLabel"), itemsPreferLocal, (int)BuildingSettings.PreferLocalDistrictServicesIncoming(m_buildingId));
                if (m_dropPreferLocalIncoming != null)
                {
                    m_dropPreferLocalIncoming.eventSelectedIndexChanged += OnIncomingPreferLocalServices;
                }
                m_dropPreferLocalOutgoing = UIUtils.AddDropDown(helperServiceOptions, Localization.Get("dropdownBuildingPanelOutgoingPreferLocalLabel"), itemsPreferLocal, (int)BuildingSettings.PreferLocalDistrictServicesOutgoing(m_buildingId));
                if (m_dropPreferLocalOutgoing != null)
                {
                    m_dropPreferLocalOutgoing.eventSelectedIndexChanged += OnOutgoingPreferLocalServices;
                }

                m_lblDetectedDistricts = ((UIPanel)helperServiceOptions.self).AddUIComponent<UILabel>();
                if (m_lblDetectedDistricts != null)
                {
                    m_lblDetectedDistricts.text = "Detected Districts: " + CitiesUtils.GetDetectedDistricts(m_buildingId);
                    m_lblDetectedDistricts.autoSize = false;
                    m_lblDetectedDistricts.width = tabSettings.width - 40;
                    m_lblDetectedDistricts.height = 25;
                    m_lblDetectedDistricts.textAlignment = UIHorizontalAlignment.Center;
                    m_lblDetectedDistricts.verticalAlignment = UIVerticalAlignment.Middle;
                    //m_lblDetectedDistricts.backgroundSprite = "InfoviewPanel";
                    //m_lblDetectedDistricts.color = Color.red;
                }

                // Outside connections
                m_panelImportExport = tabSettings.AddUIComponent<UIPanel>();
                if (m_panelImportExport != null)
                {
                    m_panelImportExport.width = width;
                    m_panelImportExport.height = 120;
                    m_panelImportExport.autoLayout = true;
                    m_panelImportExport.autoLayoutDirection = LayoutDirection.Vertical;
                    UIHelper helperImportExport = new UIHelper(m_panelImportExport);
                    UIHelper helperOutsideConnections = (UIHelper)helperImportExport.AddGroup(Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS"));
                    m_chkAllowImport = (UICheckBox)helperOutsideConnections.AddCheckbox(Localization.Get("chkAllowImport"), BuildingSettings.IsReserveCargoTrucks(m_buildingId), OnAllowImportChanged);
                    m_chkAllowExport = (UICheckBox)helperOutsideConnections.AddCheckbox(Localization.Get("chkAllowExport"), BuildingSettings.IsReserveCargoTrucks(m_buildingId), OnAllowExportChanged);
                }

                // Good delivery
                m_panelGoodsDelivery = tabSettings.AddUIComponent<UIPanel>();
                if (m_panelGoodsDelivery != null)
                {
                    m_panelGoodsDelivery.width = width;
                    m_panelGoodsDelivery.height = 100;
                    m_panelGoodsDelivery.autoLayout = true;
                    m_panelGoodsDelivery.autoLayoutDirection = LayoutDirection.Vertical;
                    UIHelper helperGoodsDelivery = new UIHelper(m_panelGoodsDelivery);
                    UIHelper helperGoodsDeliveryOptions = (UIHelper)helperGoodsDelivery.AddGroup(Localization.Get("GROUP_BUILDINGPANEL_GOODS_DELIVERY"));
                    m_chkReserveCargoTrucks = (UICheckBox)helperGoodsDeliveryOptions.AddCheckbox(Localization.Get("chkBuildingPanelReserveWarehouseTrucks"), BuildingSettings.IsReserveCargoTrucks(m_buildingId), OnReserveCargoTrucksChanged);
                }

                // Apply to all
                UIHelper helperApplyToAll = (UIHelper)helper.AddGroup(Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL"));
                UIPanel panelApplyToAll = ((UIPanel)helperApplyToAll.self).AddUIComponent<UIPanel>();
                panelApplyToAll.width = tabSettings.width;
                panelApplyToAll.height = 25;
                panelApplyToAll.autoLayout = true;
                panelApplyToAll.autoLayoutDirection = LayoutDirection.Horizontal;
                panelApplyToAll.autoFitChildrenHorizontally = true;
                panelApplyToAll.autoLayoutPadding = new RectOffset(0, 20, 0, 0);

                m_btnApplyToAllDistrict = panelApplyToAll.AddUIComponent<UIButton>();
                m_btnApplyToAllDistrict.text = "District";
                m_btnApplyToAllDistrict.eventClick += OnApplyToAllDistrictClicked;
                m_btnApplyToAllDistrict.autoSize = false;
                m_btnApplyToAllDistrict.width = 100;
                m_btnApplyToAllDistrict.height = 30;
                m_btnApplyToAllDistrict.normalBgSprite = "ButtonMenu";
                m_btnApplyToAllDistrict.hoveredBgSprite = "ButtonMenuHovered";
                m_btnApplyToAllDistrict.disabledBgSprite = "ButtonMenuDisabled";
                m_btnApplyToAllDistrict.pressedBgSprite = "ButtonMenuPressed";

                m_btnApplyToAllPark = panelApplyToAll.AddUIComponent<UIButton>();
                m_btnApplyToAllPark.text = "Park/Industry Area";
                m_btnApplyToAllPark.eventClick += OnApplyToAllParkClicked;
                m_btnApplyToAllPark.autoSize = false;
                m_btnApplyToAllPark.width = 200;
                m_btnApplyToAllPark.height = 30;
                m_btnApplyToAllPark.normalBgSprite = "ButtonMenu";
                m_btnApplyToAllPark.hoveredBgSprite = "ButtonMenuHovered";
                m_btnApplyToAllPark.disabledBgSprite = "ButtonMenuDisabled";
                m_btnApplyToAllPark.pressedBgSprite = "ButtonMenuPressed";

                m_btnApplyToAllMap = panelApplyToAll.AddUIComponent<UIButton>();
                m_btnApplyToAllMap.text = "Map";
                m_btnApplyToAllMap.eventClick += OnApplyToAllWholeMapClicked;
                m_btnApplyToAllMap.autoSize = false;
                m_btnApplyToAllMap.width = 60;
                m_btnApplyToAllMap.height = 30;
                m_btnApplyToAllMap.normalBgSprite = "ButtonMenu";
                m_btnApplyToAllMap.hoveredBgSprite = "ButtonMenuHovered";
                m_btnApplyToAllMap.disabledBgSprite = "ButtonMenuDisabled";
                m_btnApplyToAllMap.pressedBgSprite = "ButtonMenuPressed";
            }

            UIPanel? tabStatus = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelStatus"));
            if (tabStatus != null)
            {
                tabStatus.autoLayout = true;
                tabStatus.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listStatus = ListView.Create(tabStatus, "ScrollbarTrack", 0.8f, tabStatus.width, tabStatus.height - 10);
                if (m_listStatus != null)
                {
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "Timer", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", iCOLUMN_WIDTH_250, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_250, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }
            }

            UIPanel? tabVehicles = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelVehicles"));
            if (tabVehicles != null)
            {
                tabVehicles.autoLayout = true;
                tabVehicles.autoLayoutDirection = LayoutDirection.Vertical;

                // Vehicles list
                m_listVehicles = ListView.Create(tabVehicles, "ScrollbarTrack", 0.8f, tabVehicles.width, tabVehicles.height - 10);
                if (m_listVehicles != null)
                {
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Vehicle transfer value", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_250, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE_TARGET, Localization.Get("listBuildingPanelVehicleTarget"), "Target", iCOLUMN_WIDTH_250, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }
            }

            UIPanel? tabTransfers = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelTransfers"));
            if (tabTransfers != null)
            {
                tabTransfers.autoLayout = true;
                tabTransfers.autoLayoutDirection = LayoutDirection.Vertical;

                // Object label
                m_lblOffers = tabTransfers.AddUIComponent<UILabel>();
                m_lblOffers.width = tabTransfers.width;
                m_lblOffers.height = 20;
                m_lblOffers.padding = new RectOffset(4, 4, 4, 4);
                m_lblOffers.text = Localization.Get("labelBuildingPanelOffers");

                // Offer list
                m_listOffers = ListView.Create(tabTransfers, "ScrollbarTrack", 0.8f, tabTransfers.width, iLISTVIEW_OFFERS_HEIGHT);
                if (m_listOffers != null)
                {
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "Transfer Direction", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelOffersColumn2"), "Reason for transfer request", iCOLUMN_WIDTH_LARGE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listBuildingPanelOffersColumn3"), "Transfer offer priority", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelOffersColumn4"), "Transfer offer Active/Passive", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelOffersColumn5"), "Transfer Offer Amount", iCOLUMN_WIDTH_NORMAL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelOffersColumn6"), "Offer description", iCOLUMN_WIDTH_250, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listOffers.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }

                m_lblMatches = tabTransfers.AddUIComponent<UILabel>();
                m_lblMatches.width = tabTransfers.width;
                m_lblMatches.height = 20;
                m_lblMatches.padding = new RectOffset(4, 4, 4, 4);
                m_lblMatches.text = Localization.Get("labelBuildingPanelMatchOffers");

                // Offer list
                m_listMatches = ListView.Create(tabTransfers, "ScrollbarTrack", 0.7f, tabTransfers.width, iLISTVIEW_MATCHES_HEIGHT);
                if (m_listMatches != null)
                {
                    //m_listMatches.width = width - 20;
                    //m_listMatches.height = iLISTVIEW_MATCHES_HEIGHT;
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listBuildingPanelMatchesColumn1"), "Time of match", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelMatchesColumn2"), "Reason for transfer request", iCOLUMN_WIDTH_LARGE, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelMatchesColumn3"), "Active or Passive for this match", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelMatchesColumn4"), "Transfer match amount", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Transfer distance", iCOLUMN_WIDTH_SMALL, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, "P", "In priority / Out priority", iCOLUMN_WIDTH_XS, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelMatchesColumn6"), "Match description", iCOLUMN_WIDTH_250, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            m_tabStrip.SelectTabIndex(0);

            isVisible = true;
            UpdateSettingsTab();
            UpdateVehicleTab();
            UpdatePanel();
        }

        public void OnAllowImportChanged(bool bChecked)
        {
            BuildingSettings.SetImport(m_buildingId, bChecked);
        }

        public void OnAllowExportChanged(bool bChecked)
        {
            BuildingSettings.SetExport(m_buildingId, bChecked);
        }

        public void OnIncomingPreferLocalServices(UIComponent component, int Value)
        {
            BuildingSettings.PreferLocalDistrictServicesIncoming(m_buildingId, (BuildingSettings.PreferLocal)Value);
        }

        public void OnOutgoingPreferLocalServices(UIComponent component, int Value)
        {
            BuildingSettings.PreferLocalDistrictServicesOutgoing(m_buildingId, (BuildingSettings.PreferLocal)Value);
        }

        public void OnReserveCargoTrucksChanged(bool isChecked)
        {
            BuildingSettings.ReserveCargoTrucks(m_buildingId, isChecked);
        }

        private void FitToScreen()
        {
            Vector2 oScreenVector = UIView.GetAView().GetScreenResolution();
            float fX = Math.Max(0.0f, Math.Min(absolutePosition.x, oScreenVector.x - width));
            float fY = Math.Max(0.0f, Math.Min(absolutePosition.y, oScreenVector.y - height));
            Vector3 oFitPosition = new Vector3(fX, fY, absolutePosition.z);
            absolutePosition = oFitPosition;
        }

        public bool CanShowSettingsTab()
        {
            return BuildingTypeHelper.CanRestrictDistrict(m_buildingId) ||
                    BuildingTypeHelper.CanImport(m_buildingId) ||
                    BuildingTypeHelper.CanExport(m_buildingId);
        }

        public void UpdateSettingsTab()
        {
            if (m_tabStrip != null)
            {
                BuildingType eType = GetBuildingType(m_buildingId);
                if (CanShowSettingsTab()) 
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, true);

                    if (m_lblCustomTransferManagerWarning != null)
                    {
                        m_lblCustomTransferManagerWarning.isVisible = !ModSettings.GetSettings().optionEnableNewTransferManager;
                    }

                    if (m_dropPreferLocalIncoming != null)
                    {
                        m_dropPreferLocalIncoming.selectedIndex = (int) BuildingSettings.PreferLocalDistrictServicesIncoming(m_buildingId);
                    }
                    if (m_dropPreferLocalOutgoing != null)
                    {
                        m_dropPreferLocalOutgoing.selectedIndex = (int)BuildingSettings.PreferLocalDistrictServicesOutgoing(m_buildingId);
                    }
                    if (m_lblDetectedDistricts != null)
                    {
                        m_lblDetectedDistricts.text = "Detected Districts: " + CitiesUtils.GetDetectedDistricts(m_buildingId);
                        m_lblDetectedDistricts.textAlignment = UIHorizontalAlignment.Center;
                    }

                    if (m_panelImportExport != null)
                    {
                        bool bCanImport = CanImport(m_buildingId);
                        bool bCanExport = CanExport(m_buildingId);
                        if (bCanImport || bCanExport)
                        {
                            m_panelImportExport.Show();

                            if (m_chkAllowImport != null)
                            {
                                m_chkAllowImport.isEnabled = bCanImport;
                                m_chkAllowImport.isChecked = BuildingSettings.GetImport(m_buildingId);
                            }
                            if (m_chkAllowExport != null)
                            {
                                m_chkAllowExport.isEnabled = bCanExport;
                                m_chkAllowExport.isChecked = BuildingSettings.GetExport(m_buildingId);
                            }
                        }
                        else
                        {
                            m_panelImportExport.Hide();
                        }
                    }

                    if (m_panelGoodsDelivery != null)
                    {
                        if (BuildingTypeHelper.IsWarehouse(m_buildingId))
                        {
                            m_panelGoodsDelivery.Show();

                            if (m_chkReserveCargoTrucks != null)
                            {
                                // If the global setting is on then disable this button.
                                if (ModSettings.GetSettings().optionWarehouseReserveTrucks)
                                {
                                    m_chkReserveCargoTrucks.isEnabled = false;
                                    m_chkReserveCargoTrucks.isChecked = true;
                                }
                                else
                                {
                                    m_chkReserveCargoTrucks.isEnabled = true;
                                    m_chkReserveCargoTrucks.isChecked = BuildingSettings.IsReserveCargoTrucks(m_buildingId);
                                }
                            }
                        }
                        else
                        {
                            m_panelGoodsDelivery.Hide();
                        }
                    }

                    if (m_btnApplyToAllDistrict != null)
                    {
                        m_btnApplyToAllDistrict.isEnabled = CitiesUtils.IsInDistrict(m_buildingId);
                    }
                    if (m_btnApplyToAllPark != null)
                    {
                        m_btnApplyToAllPark.isEnabled = CitiesUtils.IsInPark(m_buildingId);
                    }
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);
                }
            }
        }

        public void UpdateVehicleTab()
        {
            if (m_tabStrip != null)
            {
                if (HasVehicles(m_buildingId))
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, true);
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, false);
                }
            }
        }

        public void SetPanelBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
            m_TransferOffers.Clear();
            UpdateSettingsTab();
            UpdateVehicleTab();
            UpdatePanel();
        }

        public void ShowPanel(ushort buildingId)
        {
            SetPanelBuilding(buildingId);
            ShowPanel();
        }

        public void ShowPanel()
        {
            UpdatePanel();
            Show(true);
        }

        public void HidePanel()
        {
            if (m_listStatus != null)
            {
                m_listStatus.Clear();
            }
            if (m_listOffers != null)
            {
                m_listOffers.Clear();
            }
            if (m_listMatches != null)
            {
                m_listMatches.Clear();
            }
            if (SelectionTool.Instance != null)
            {
                SelectionTool.Instance.Disable();
            }
            Hide();
        }

        public void TogglePanel()
        {
            if (isVisible)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            HidePanel();
        }

        public void OnStatsClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransferManagerCEThreading.ToggleStatsPanel();
        }

        public void OnTabChanged(int index)
        {
            UpdatePanel();
        }

        public List<VehicleData> GetVehicles()
        {
            List<VehicleData> list = new List<VehicleData>();

            List<ushort> vehicles = CitiesUtils.GetOwnVehiclesForBuilding(m_buildingId);
            foreach(ushort vehicleId in vehicles)
            {
                list.Add(new VehicleData(vehicleId));
            }

            return list;
        }

        public void OnApplyToAllDistrictClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (CitiesUtils.IsSameDistrict(m_buildingId, (ushort)i) && BuildingTypeHelper.IsSameType(m_buildingId, (ushort) i))
                {
                    BuildingSettings.SetSettings((ushort) i, settings);
                }
            }
            
        }

        public void OnApplyToAllParkClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (CitiesUtils.IsSamePark(m_buildingId, (ushort)i) && BuildingTypeHelper.IsSameType(m_buildingId, (ushort)i))
                {
                    BuildingSettings.SetSettings((ushort)i, settings);
                }
            }
        }


        public void OnApplyToAllWholeMapClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (BuildingTypeHelper.IsSameType(m_buildingId, (ushort)i))
                {
                    BuildingSettings.SetSettings((ushort)i, settings);
                }
            }

        }

        public void UpdatePanel()
        {
            if (m_buildingId != 0)
            {
                if (m_lblSource != null)
                {
                    m_lblSource.text = CitiesUtils.GetBuildingName(m_buildingId);
                }
                if (m_tabStrip != null)
                {
                    switch ((TabIndex) m_tabStrip.GetSelectTabIndex())
                    {
                        case TabIndex.TAB_SETTINGS: // Settings
                            {
                                break;
                            }
                        case TabIndex.TAB_STATUS: // Status
                            {
                                if (m_listStatus != null)
                                {
                                    List<StatusContainer> list = StatusHelper.GetStatusList(m_buildingId);
                                    list.Sort();
                                    m_listStatus.SetItems(list.ToArray());
                                }
                                break;
                            }
                        case TabIndex.TAB_VEHICLES: // Vehicles
                            {
                                if (m_listVehicles != null)
                                {
                                    List<VehicleData> vehicles = GetVehicles();
                                    m_listVehicles.SetItems(vehicles.ToArray());
                                }
                                break;
                            }
                        case TabIndex.TAB_TRANSFERS: // Transfers
                            {
                                if (m_listOffers != null)
                                {
                                    List<OfferData> offers = TransferManagerUtils.GetOffersForBuilding(m_buildingId);
                                    offers.Sort();
                                    m_listOffers.SetItems(offers.Take(30).ToArray());
                                }
                                if (m_listMatches != null && MatchLogging.instance != null)
                                {
                                    List<MatchData> list = MatchLogging.instance.GetMatchesForBuilding(m_buildingId);
                                    if (list != null)
                                    {
                                        list.Sort();
                                        m_listMatches.SetItems(list.Take(30).ToArray());
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}