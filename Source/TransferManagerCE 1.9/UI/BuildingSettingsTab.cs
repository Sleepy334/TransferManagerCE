using ColossalFramework.UI;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.BuildingPanel;
using TransferManagerCE.Settings;

namespace TransferManagerCE.UI
{
    public class BuildingSettingsTab
    {
        const float fTEXT_SCALE = 0.9f;

        // Settings tab
        private UILabel? m_lblCustomTransferManagerWarning = null;

        private UIGroup? m_grpDistrictRestrictions = null;
        private UIPanel? m_panelIncomingDistrict = null;
        private UIMyDropDown? m_dropPreferLocalIncoming = null;
        private UIButton? m_btnIncomingSelectDistrict = null;
        private UIPanel? m_panelOutgoingDistrict = null;
        private UIMyDropDown? m_dropPreferLocalOutgoing = null;
        private UIButton? m_btnOutgoingSelectDistrict = null;
        private UICheckBox? m_chkDistrictAllowServices = null;

        private UIGroup? m_panelServiceDistance = null;
        private SettingsSlider? m_sliderServiceDistance = null;

        private UIGroup? m_panelImportExport = null;
        private UICheckBox? m_chkAllowImport = null;
        private UICheckBox? m_chkAllowExport = null;

        private UIGroup? m_panelOutsideDistanceMultiplier = null;
        private SettingsSlider? m_sliderOutsideDistanceMultiplier = null;

        private UIGroup? m_panelOutsideSettings = null;
        private SettingsSlider? m_sliderOutsideCargoCapacity = null;
        private SettingsSlider? m_sliderOutsideResidentCapacity = null;
        private SettingsSlider? m_sliderOutsideTouristFactor0 = null;
        private SettingsSlider? m_sliderOutsideTouristFactor1 = null;
        private SettingsSlider? m_sliderOutsideTouristFactor2 = null;
        private SettingsSlider? m_sliderOutsideDummyTrafficFactor = null;

        private UIGroup? m_panelGoodsDelivery = null;
        private UICheckBox? m_chkWarehouseOverride = null;
        private UICheckBox? m_chkWarehouseFirst = null;
        private SettingsSlider? m_sliderReserveCargoTrucks = null;

        private UIGroup? m_buttonGroup = null;
        private UIButton? m_btnApplyToAllDistrict = null;
        private UIButton? m_btnApplyToAllPark = null;
        private UIButton? m_btnApplyToAllMap = null;

        private ushort m_buildingId;
        private UITabStrip? m_tabStrip;

        public BuildingSettingsTab()
        {
            m_buildingId = 0;
            m_tabStrip = null;
        }

        public void SetTabBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.Hide();
            }
            UpdateSettingsTab();
        }

        public void Setup(UITabStrip tabStrip)
        {
            m_tabStrip = tabStrip;

            UIPanel? tabSettings = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelSettings"));
            if (tabSettings != null)
            {
                tabSettings.autoLayout = true;
                tabSettings.autoLayoutDirection = LayoutDirection.Vertical;
                tabSettings.padding = new RectOffset(10, 10, 10, 10);
                tabSettings.autoLayoutPadding = new RectOffset(0, 0, 0, 8);

                m_lblCustomTransferManagerWarning = tabSettings.AddUIComponent<UILabel>();
                if (m_lblCustomTransferManagerWarning != null)
                {
                    m_lblCustomTransferManagerWarning.text = Localization.Get("txtBuildingPanelTransferManagerWarning");
                    m_lblCustomTransferManagerWarning.textColor = Color.red;
                    m_lblCustomTransferManagerWarning.textScale = fTEXT_SCALE;
                }

                UIHelper helper = new UIHelper(tabSettings);

                // Prefer local services
                m_grpDistrictRestrictions = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_BUILDINGPANEL_DISTRICT"), fTEXT_SCALE, tabSettings.width - 20, 140);
                if (m_grpDistrictRestrictions != null)
                {
                    string[] itemsPreferLocal = {
                            Localization.Get("dropdownBuildingPanelPreferLocal1"),
                            Localization.Get("dropdownBuildingPanelPreferLocal2"),
                            Localization.Get("dropdownBuildingPanelPreferLocal3"),
                            Localization.Get("dropdownBuildingPanelPreferLocal4"),
                        };
                    // Incoming restrictions
                    m_panelIncomingDistrict = m_grpDistrictRestrictions.m_content.AddUIComponent<UIPanel>();
                    m_panelIncomingDistrict.width = tabSettings.width;
                    m_panelIncomingDistrict.height = 35;

                    m_dropPreferLocalIncoming = UIMyDropDown.Create(m_panelIncomingDistrict, Localization.Get("dropdownBuildingPanelIncomingPreferLocalLabel"), fTEXT_SCALE, itemsPreferLocal, OnIncomingPreferLocalServices, (int)BuildingSettings.PreferLocalDistrictServicesIncoming(m_buildingId));
                    if (m_dropPreferLocalIncoming != null)
                    {
                        m_dropPreferLocalIncoming.m_panel.relativePosition = new Vector3(0, 0);
                        m_dropPreferLocalIncoming.SetPanelWidth(m_panelIncomingDistrict.width - 200);
                        m_dropPreferLocalIncoming.m_dropdown.textScale = 0.9f;
                    }

                    m_btnIncomingSelectDistrict = UIUtils.AddButton(m_panelIncomingDistrict, Localization.Get("btnDistricts") + "...", 120, m_dropPreferLocalIncoming.m_dropdown.height);
                    if (m_btnIncomingSelectDistrict != null)
                    {
                        m_btnIncomingSelectDistrict.relativePosition = new Vector3(m_dropPreferLocalIncoming.m_panel.width + 20, 2);
                        m_btnIncomingSelectDistrict.eventClick += (c, e) => OnSelectIncomingDistrictClicked();
                        m_btnIncomingSelectDistrict.eventTooltipEnter += (c, e) => UpdateDistrictButtonTooltips();
                    }

                    // Outgoing restrictions
                    m_panelOutgoingDistrict = m_grpDistrictRestrictions.m_content.AddUIComponent<UIPanel>();
                    m_panelOutgoingDistrict.width = tabSettings.width;
                    m_panelOutgoingDistrict.height = 65;

                    m_dropPreferLocalOutgoing = UIMyDropDown.Create(m_panelOutgoingDistrict, Localization.Get("dropdownBuildingPanelOutgoingPreferLocalLabel"), fTEXT_SCALE, itemsPreferLocal, OnOutgoingPreferLocalServices, (int)BuildingSettings.PreferLocalDistrictServicesOutgoing(m_buildingId));
                    if (m_dropPreferLocalOutgoing != null)
                    {
                        m_dropPreferLocalOutgoing.m_panel.relativePosition = new Vector3(0, 0);
                        m_dropPreferLocalOutgoing.SetPanelWidth(m_panelOutgoingDistrict.width - 200);
                        m_dropPreferLocalOutgoing.m_dropdown.textScale = 0.9f;
                    }

                    m_btnOutgoingSelectDistrict = UIUtils.AddButton(m_panelOutgoingDistrict, Localization.Get("btnDistricts") + "...", 120, m_dropPreferLocalOutgoing.m_dropdown.height);
                    if (m_btnOutgoingSelectDistrict != null)
                    {
                        m_btnOutgoingSelectDistrict.relativePosition = new Vector3(m_dropPreferLocalOutgoing.m_panel.width + 20, 2);
                        m_btnOutgoingSelectDistrict.eventClick += (c, e) => OnSelectOutgoingDistrictClicked();
                        m_btnOutgoingSelectDistrict.eventTooltipEnter += (c, e) => UpdateDistrictButtonTooltips();
                    }

                    // Allow Services
                    m_chkDistrictAllowServices = UIUtils.AddCheckbox(m_panelOutgoingDistrict, Localization.Get("chkDistrictAllowServices"), fTEXT_SCALE, BuildingSettings.GetDistrictAllowServices(m_buildingId), OnDistrictAllowServicesChanged);
                    if (m_chkDistrictAllowServices != null) 
                    {
                        m_chkDistrictAllowServices.relativePosition = new Vector3(0, 32);
                    }
                }

                m_panelServiceDistance = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_BUILDINGPANEL_DISTANCE_RESTRICTIONS"), fTEXT_SCALE, tabSettings.width - 20, 46);
                if (m_panelServiceDistance != null)
                {
                    UIHelper helperDistance = new UIHelper(m_panelServiceDistance.m_content);
                    m_sliderServiceDistance = SettingsSlider.Create(helperDistance, LayoutDirection.Horizontal, Localization.Get("sliderDistanceRestriction"), fTEXT_SCALE, 400, 200, 0f, 10, 1f, BuildingSettings.GetSettings(m_buildingId).m_iServiceDistance, OnServiceDistanceChanged);
                    m_sliderServiceDistance.SetTooltip(Localization.Get("sliderDistanceRestrictionTooltip"));
                }

                // Outside connections
                m_panelImportExport = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS"), fTEXT_SCALE, tabSettings.width - 20, 70);
                if (m_panelImportExport != null && m_panelImportExport.m_content != null)
                {
                    m_chkAllowImport = UIUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowImport"), fTEXT_SCALE, !BuildingSettings.IsImportDisabled(m_buildingId), OnAllowImportChanged);
                    m_chkAllowExport = UIUtils.AddCheckbox(m_panelImportExport.m_content, Localization.Get("chkAllowExport"), fTEXT_SCALE, !BuildingSettings.IsExportDisabled(m_buildingId), OnAllowExportChanged);
                }

                m_panelOutsideDistanceMultiplier = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_DISTANCE_MULTIPLIER"), fTEXT_SCALE, tabSettings.width - 20, 60);
                if (m_panelOutsideDistanceMultiplier != null)
                {
                    UIHelper helperDistanceMultiplier = new UIHelper(m_panelOutsideDistanceMultiplier.m_content);
                    m_sliderOutsideDistanceMultiplier = SettingsSlider.Create(helperDistanceMultiplier, LayoutDirection.Horizontal, Localization.Get("sliderOutsideDistanceMultiplier"), fTEXT_SCALE, 400, 200, 0f, 10, 1f, BuildingSettings.GetSettings(m_buildingId).GetOutsideMultiplier(), OnOutsideDistanceMultiplierChanged);
                    m_sliderOutsideDistanceMultiplier.SetTooltip(Localization.Get("sliderOutsideDistanceMultiplierTooltip"));
                }

                m_panelOutsideSettings = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_OUTSIDE_SETTINGS"), fTEXT_SCALE, tabSettings.width - 20, 240);
                if (m_panelOutsideSettings != null)
                {
                    UIHelper helperOutside = new UIHelper(m_panelOutsideSettings.m_content);
                    m_sliderOutsideCargoCapacity = SettingsSlider.Create(helperOutside, LayoutDirection.Horizontal, Localization.Get("sliderOutsideCargoCapacity"), fTEXT_SCALE, 400, 200, 0f, 100, 1f, 20, OnOutsideCargoCapacityChanged);
                    m_sliderOutsideResidentCapacity = SettingsSlider.Create(helperOutside, LayoutDirection.Horizontal, Localization.Get("sliderOutsideResidentCapacity"), fTEXT_SCALE, 400, 200, 0f, 2000f, 1f, 20, OnOutsideResidentCapacityChanged);
                    m_sliderOutsideTouristFactor0 = SettingsSlider.Create(helperOutside, LayoutDirection.Horizontal, Localization.Get("sliderOutsideTouristFactor0"), fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, OnOutsideTouristFactor0Changed);
                    m_sliderOutsideTouristFactor1 = SettingsSlider.Create(helperOutside, LayoutDirection.Horizontal, Localization.Get("sliderOutsideTouristFactor1"), fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, OnOutsideTouristFactor1Changed);
                    m_sliderOutsideTouristFactor2 = SettingsSlider.Create(helperOutside, LayoutDirection.Horizontal, Localization.Get("sliderOutsideTouristFactor2"), fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, OnOutsideTouristFactor2Changed);
                    m_sliderOutsideDummyTrafficFactor = SettingsSlider.Create(helperOutside, LayoutDirection.Horizontal, Localization.Get("sliderOutsideDummyTrafficFactor"), fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, OnOutsideDummyTrafficChanged);
                    UIButton btnReset = UIUtils.AddButton(m_panelOutsideSettings.m_content, Localization.Get("btnOutsideReset"), 100, 30);
                    if (btnReset != null)
                    {
                        btnReset.eventClick += (c, e) =>
                        {
                            OutsideConnectionSettings.Reset(m_buildingId);
                            UpdateSettingsTab();
                        };
                    }
                }

                // Good delivery
                m_panelGoodsDelivery = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_BUILDINGPANEL_GOODS_DELIVERY"), fTEXT_SCALE, tabSettings.width - 20, 120);
                if (m_panelGoodsDelivery != null)
                {
                    m_chkWarehouseOverride = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionWarehouseOverride"), fTEXT_SCALE, BuildingSettings.IsWarehouseOverride(m_buildingId), OnWarehouseOverrideChanged);
                    m_chkWarehouseFirst = UIUtils.AddCheckbox(m_panelGoodsDelivery.m_content, Localization.Get("optionWarehouseFirst"), fTEXT_SCALE, BuildingSettings.IsWarehouseFirst(m_buildingId), OnWarehouseFirstChanged);
                    UIHelper helperGoodsDelivery = new UIHelper(m_panelGoodsDelivery.m_content);
                    m_sliderReserveCargoTrucks = SettingsSlider.Create(helperGoodsDelivery, LayoutDirection.Vertical, Localization.Get("sliderWarehouseReservePercent"), fTEXT_SCALE, 400, 200, 0f, 100f, 1f, BuildingSettings.ReserveCargoTrucksPercent(m_buildingId), OnReserveCargoTrucksChanged);
                }

                // Apply to all
                m_buttonGroup = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL"), fTEXT_SCALE, tabSettings.width - 20, 60);
                if (m_buttonGroup != null)
                {
                    // Apply to all
                    UIPanel panelApplyToAll = m_buttonGroup.m_content.AddUIComponent<UIPanel>();
                    panelApplyToAll.width = tabSettings.width;
                    panelApplyToAll.height = 30;
                    panelApplyToAll.autoLayout = true;
                    panelApplyToAll.autoLayoutDirection = LayoutDirection.Horizontal;
                    panelApplyToAll.autoFitChildrenHorizontally = true;
                    panelApplyToAll.autoLayoutPadding = new RectOffset(0, 20, 0, 0);

                    m_btnApplyToAllDistrict = UIUtils.AddButton(panelApplyToAll, Localization.Get("btnDistrict"), 100, 30);
                    m_btnApplyToAllDistrict.eventClick += OnApplyToAllDistrictClicked;

                    m_btnApplyToAllPark = UIUtils.AddButton(panelApplyToAll, Localization.Get("btnPark"), 200, 30);
                    m_btnApplyToAllPark.eventClick += OnApplyToAllParkClicked;

                    m_btnApplyToAllMap = UIUtils.AddButton(panelApplyToAll, Localization.Get("btnMap"), 60, 30);
                    m_btnApplyToAllMap.eventClick += OnApplyToAllWholeMapClicked;
                }
            }
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

                    BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);

                    if (m_lblCustomTransferManagerWarning != null)
                    {
                        m_lblCustomTransferManagerWarning.isVisible = !SaveGameSettings.GetSettings().EnableNewTransferManager;
                    }

                    if (m_grpDistrictRestrictions != null)
                    {
                        bool bIncoming = CanDistrictRestrictIncoming(eType);
                        bool bOutgoing = CanDistrictRestrictOutgoing(eType, m_buildingId);
                        if (bIncoming || bOutgoing)
                        {
                            m_grpDistrictRestrictions.isVisible = true;

                            // Incoming
                            if (m_panelIncomingDistrict != null)
                            {
                                m_panelIncomingDistrict.isVisible = bIncoming;

                                if (m_dropPreferLocalIncoming != null)
                                {
                                    m_dropPreferLocalIncoming.isVisible = bIncoming;
                                    m_dropPreferLocalIncoming.selectedIndex = (int)settings.m_iPreferLocalDistrictsIncoming;
                                }
                                if (m_btnIncomingSelectDistrict != null)
                                {
                                    m_btnIncomingSelectDistrict.isVisible = bIncoming;
                                    m_btnIncomingSelectDistrict.isEnabled = (settings.m_iPreferLocalDistrictsIncoming != BuildingSettings.PreferLocal.ALL_DISTRICTS);
                                    m_btnIncomingSelectDistrict.tooltip = settings.GetIncomingDistrictTooltip(m_buildingId);
                                }
                            }

                            // Outgoing
                            if (m_panelOutgoingDistrict != null)
                            {
                                m_panelOutgoingDistrict.isVisible = bOutgoing;

                                if (m_dropPreferLocalOutgoing != null)
                                {
                                    m_dropPreferLocalOutgoing.isVisible = bOutgoing;
                                    m_dropPreferLocalOutgoing.selectedIndex = (int)settings.m_iPreferLocalDistrictsOutgoing;
                                }
                                if (m_btnOutgoingSelectDistrict != null)
                                {
                                    m_btnOutgoingSelectDistrict.isVisible = bOutgoing;
                                    m_btnOutgoingSelectDistrict.isEnabled = (settings.m_iPreferLocalDistrictsOutgoing != BuildingSettings.PreferLocal.ALL_DISTRICTS);
                                    
                                }
                                if (m_chkDistrictAllowServices != null)
                                {
                                    m_chkDistrictAllowServices.isVisible = bOutgoing;
                                    m_chkDistrictAllowServices.isChecked = settings.m_bDistrictAllowServices;
                                    m_chkDistrictAllowServices.isEnabled = (settings.m_iPreferLocalDistrictsOutgoing != BuildingSettings.PreferLocal.ALL_DISTRICTS);
                                }
                            }

                            if (bIncoming && bOutgoing)
                            {
                                m_grpDistrictRestrictions.height = 120;
                            }
                            else if (bIncoming)
                            {
                                m_grpDistrictRestrictions.height = 60;
                            }
                            else if (bOutgoing)
                            {
                                m_grpDistrictRestrictions.height = 80;
                            }
                        }
                        else
                        {
                            m_grpDistrictRestrictions.isVisible = false;
                        }
                    }

                    if (m_panelServiceDistance != null)
                    {
                        if (BuildingTypeHelper.IsDistanceRestrictionSupported(m_buildingId))
                        {
                            m_panelServiceDistance.Show();
                            if (m_sliderServiceDistance != null)
                            {
                                m_sliderServiceDistance.SetValue(settings.m_iServiceDistance);
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
                                m_chkAllowImport.isChecked = settings.m_bAllowImport;
                            }
                            if (m_chkAllowExport != null)
                            {
                                m_chkAllowExport.isEnabled = bCanExport;
                                m_chkAllowExport.isChecked = settings.m_bAllowExport;
                            }
                        }
                        else
                        {
                            m_panelImportExport.Hide();
                        }
                    }

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

                    if (m_panelOutsideSettings != null)
                    {
                        if (!DependencyUtilities.IsAdvancedOutsideConnectionsRunning() && IsOutsideConnection(m_buildingId))
                        {
                            m_panelOutsideSettings.Show();
                            OutsideConnectionSettings outsideSettings = OutsideConnectionSettings.GetSettings(m_buildingId);

                            if (m_sliderOutsideCargoCapacity != null)
                            {
                                m_sliderOutsideCargoCapacity.SetValue(outsideSettings.m_cargoCapacity);
                            }
                            if (m_sliderOutsideResidentCapacity != null)
                            {
                                m_sliderOutsideResidentCapacity.SetValue(outsideSettings.m_residentCapacity);
                            }
                            if (m_sliderOutsideTouristFactor0 != null)
                            {
                                m_sliderOutsideTouristFactor0.SetValue(outsideSettings.m_touristFactor0);
                            }
                            if (m_sliderOutsideTouristFactor1 != null)
                            {
                                m_sliderOutsideTouristFactor1.SetValue(outsideSettings.m_touristFactor1);
                            }
                            if (m_sliderOutsideTouristFactor2 != null)
                            {
                                m_sliderOutsideTouristFactor2.SetValue(outsideSettings.m_touristFactor2);
                            }
                            if (m_sliderOutsideDummyTrafficFactor != null)
                            {
                                m_sliderOutsideDummyTrafficFactor.SetValue(outsideSettings.m_dummyTrafficFactor);
                            }
                        }
                        else
                        {
                            m_panelOutsideSettings.Hide();
                        }
                    }
                    else
                    {
                        Debug.Log("m_panelOutsideDistanceMultiplier is null");
                    }

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
                                m_chkWarehouseFirst.isChecked = BuildingSettings.IsWarehouseFirst(m_buildingId);
                                m_chkWarehouseFirst.isEnabled = settings.m_bWarehouseOverride;
                            }

                            if (m_sliderReserveCargoTrucks != null)
                            {
                                
                                int iPercent = BuildingSettings.ReserveCargoTrucksPercent(m_buildingId);
                                m_sliderReserveCargoTrucks.SetValue(iPercent);
                                m_sliderReserveCargoTrucks.Enable(settings.m_bWarehouseOverride);

                                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                                WarehouseAI? warehouse = building.Info.GetAI() as WarehouseAI;
                                if (warehouse != null && m_sliderReserveCargoTrucks.m_label != null)
                                {
                                    int iTrucks = (int)((float)(iPercent * 0.01) * (float)warehouse.m_truckCount);
                                    m_sliderReserveCargoTrucks.m_label.text = Localization.Get("sliderWarehouseReservePercent") + ": " + iPercent + " | Trucks: "  + iTrucks;
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

                    if (m_buttonGroup != null)
                    {
                        string sTypeDescription;
                        BuildingType eMainType = GetBuildingType(m_buildingId);
                        if (eMainType == BuildingType.Warehouse)
                        {
                            BuildingSubType eSubType = GetBuildingSubType(m_buildingId);
                            if (eSubType != BuildingSubType.None)
                            {
                                sTypeDescription = eSubType.ToString();
                            }
                            else
                            {
                                sTypeDescription = eMainType.ToString();
                            }
                        }
                        else
                        {
                            sTypeDescription = eMainType.ToString();
                        }
                        m_buttonGroup.SetText(Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL") + ": " + sTypeDescription);
                    }
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_SETTINGS, false);
                }
            }

            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.SetPanelBuilding(m_buildingId);
                DistrictPanel.Instance.UpdatePanel();
            }

            UpdateDistrictButtonTooltips();
        }

        public void OnOutsideCargoCapacityChanged(float fValue)
        {
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            settings.m_cargoCapacity = (int)fValue;
            OutsideConnectionSettings.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideResidentCapacityChanged(float fValue)
        {
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            settings.m_residentCapacity = (int)fValue;
            OutsideConnectionSettings.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideTouristFactor0Changed(float fValue)
        {
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            settings.m_touristFactor0 = (int)fValue;
            OutsideConnectionSettings.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideTouristFactor1Changed(float fValue)
        {
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            settings.m_touristFactor1 = (int)fValue;
            OutsideConnectionSettings.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideTouristFactor2Changed(float fValue)
        {
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            settings.m_touristFactor2 = (int)fValue;
            OutsideConnectionSettings.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideDummyTrafficChanged(float fValue)
        {
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            settings.m_dummyTrafficFactor = (int)fValue;
            OutsideConnectionSettings.SetSettings(m_buildingId, settings);
        }

        public void OnSelectIncomingDistrictClicked()
        {
            DistrictPanel.Init();
            if (DistrictPanel.Instance != null)
            {
                DistrictPanel.Instance.ShowPanel(m_buildingId, true);
            }
        }

        public void OnSelectOutgoingDistrictClicked()
        {
            DistrictPanel.Init();
            if (DistrictPanel.Instance != null)
            {
                DistrictPanel.Instance.ShowPanel(m_buildingId, false);
            }
        }

        public void UpdateDistrictButtonTooltips()
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId); 
            if (m_btnIncomingSelectDistrict != null)
            {
                m_btnIncomingSelectDistrict.tooltip = settings.GetIncomingDistrictTooltip(m_buildingId);
            }
            if (m_btnOutgoingSelectDistrict != null)
            {
                m_btnOutgoingSelectDistrict.tooltip = settings.GetOutgoingDistrictTooltip(m_buildingId);
            }
        }

        public void OnServiceDistanceChanged(float Value)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);
            settings.m_iServiceDistance = (int)Value;
            BuildingSettings.SetSettings(m_buildingId, settings);
        }

        public void OnOutsideDistanceMultiplierChanged(float Value)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);
            settings.SetOutsideMultiplier((int)Value);
            BuildingSettings.SetSettings(m_buildingId, settings);
        }

        public void OnDistrictAllowServicesChanged(bool bChecked)
        {
            BuildingSettings.SetDistrictAllowServices(m_buildingId, bChecked);
        }

        public void OnAllowImportChanged(bool bChecked)
        {
            BuildingSettings.SetImport(m_buildingId, bChecked);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }
        }

        public void OnAllowExportChanged(bool bChecked)
        {
            BuildingSettings.SetExport(m_buildingId, bChecked);

            // Update Import Export settings on outside connection panel if showing
            if (OutsideConnectionPanel.Instance != null && OutsideConnectionPanel.Instance.isVisible)
            {
                OutsideConnectionPanel.Instance.UpdatePanel();
            }
        }

        public void OnIncomingPreferLocalServices(UIComponent component, int Value)
        {
            BuildingSettings.PreferLocalDistrictServicesIncoming(m_buildingId, (BuildingSettings.PreferLocal)Value);
            UpdateSettingsTab(); // Enable Allow Services option
        }

        public void OnOutgoingPreferLocalServices(UIComponent component, int Value)
        {
            BuildingSettings.PreferLocalDistrictServicesOutgoing(m_buildingId, (BuildingSettings.PreferLocal)Value);
            UpdateSettingsTab(); // Enable Allow Services option
        }

        public void OnWarehouseOverrideChanged(bool bChecked)
        {
            BuildingSettings.WarehouseOverride(m_buildingId, bChecked);
            UpdateSettingsTab();
        }

        public void OnWarehouseFirstChanged(bool bChecked)
        {
            BuildingSettings.WarehouseFirst(m_buildingId, bChecked);
        }

        public void OnReserveCargoTrucksChanged(float fPercent)
        {
            BuildingSettings.ReserveCargoTrucksPercent(m_buildingId, (int)fPercent);
            UpdateSettingsTab();
        }

        public void OnApplyToAllDistrictClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (CitiesUtils.IsSameDistrict(m_buildingId, (ushort)i) && BuildingTypeHelper.IsSameType(m_buildingId, (ushort)i))
                {
                    BuildingSettings.SetSettings((ushort)i, settings);
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
            if (m_chkDistrictAllowServices != null)
            {
                UnityEngine.Object.Destroy(m_chkDistrictAllowServices.gameObject);
                m_chkDistrictAllowServices = null;
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
            if (m_lblCustomTransferManagerWarning != null)
            {
                UnityEngine.Object.Destroy(m_lblCustomTransferManagerWarning.gameObject);
                m_lblCustomTransferManagerWarning = null;
            }
            if (m_buttonGroup != null)
            {
                UnityEngine.Object.Destroy(m_buttonGroup.gameObject);
                m_buttonGroup = null;
            }
            if (m_btnApplyToAllDistrict != null)
            {
                UnityEngine.Object.Destroy(m_btnApplyToAllDistrict.gameObject);
                m_btnApplyToAllDistrict = null;
            }
            if (m_btnApplyToAllPark != null)
            {
                UnityEngine.Object.Destroy(m_btnApplyToAllPark.gameObject);
                m_btnApplyToAllPark = null;
            }
            if (m_btnApplyToAllMap != null)
            {
                UnityEngine.Object.Destroy(m_btnApplyToAllMap.gameObject);
                m_btnApplyToAllMap = null;
            }
            if (m_grpDistrictRestrictions != null)
            {
                UnityEngine.Object.Destroy(m_grpDistrictRestrictions.gameObject);
                m_grpDistrictRestrictions = null;
            }
        }
    }
}
