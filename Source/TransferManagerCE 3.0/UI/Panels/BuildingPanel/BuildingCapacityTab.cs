using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.Common;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class BuildingCapacityTab : BuildingTab
    {
        const float fTEXT_SCALE = 0.9f;

        private UIGroup? m_panelOutsideSettings = null;
        private SettingsSlider? m_sliderOutsideCargoCapacity = null;
        private SettingsSlider? m_sliderOutsideResidentCapacity = null;
        private SettingsSlider? m_sliderOutsideTouristFactor0 = null;
        private SettingsSlider? m_sliderOutsideTouristFactor1 = null;
        private SettingsSlider? m_sliderOutsideTouristFactor2 = null;
        private SettingsSlider? m_sliderOutsideDummyTrafficFactor = null;

        private bool m_bInSetup = false;

        // ----------------------------------------------------------------------------------------
        public void SetTabBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
        }

        public override bool ShowTab()
        {
            if (m_buildingId == 0)
            {
                return false;
            }

            return m_eBuildingType == BuildingTypeHelper.BuildingType.OutsideConnection;
        }

        public override void SetupInternal()
        {
            m_bInSetup = true;

            UIPanel? tabSettings = m_tabStrip.AddTabIcon("InfoIconOutsideConnections", Localization.Get("tabBuildingPanelCapacity"), "", 120f);
            if (tabSettings is not null)
            {
                tabSettings.autoLayout = true;
                tabSettings.autoLayoutDirection = LayoutDirection.Vertical;
                tabSettings.padding = new RectOffset(10, 10, 10, 10);
                tabSettings.autoLayoutPadding = new RectOffset(0, 0, 0, 8);

                m_panelOutsideSettings = UIGroup.AddGroup(tabSettings, Localization.Get("GROUP_OUTSIDE_SETTINGS"), fTEXT_SCALE, tabSettings.width - 20, 240);
                if (m_panelOutsideSettings is not null)
                {
                    // Clear the group background
                    m_panelOutsideSettings.backgroundSprite = "";

                    m_sliderOutsideCargoCapacity = SettingsSlider.Create(m_panelOutsideSettings.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideCargoCapacity"), UIFonts.SemiBold, fTEXT_SCALE, 400, 200, 0f, 100, 1f, 20, 0, OnOutsideCargoCapacityChanged);
                    m_sliderOutsideResidentCapacity = SettingsSlider.Create(m_panelOutsideSettings.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideResidentCapacity"), UIFonts.SemiBold, fTEXT_SCALE, 400, 200, 0f, 2000f, 1f, 20, 0, OnOutsideResidentCapacityChanged);
                    m_sliderOutsideTouristFactor0 = SettingsSlider.Create(m_panelOutsideSettings.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideTouristFactor0"), UIFonts.SemiBold, fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, 0, OnOutsideTouristFactor0Changed);
                    m_sliderOutsideTouristFactor1 = SettingsSlider.Create(m_panelOutsideSettings.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideTouristFactor1"), UIFonts.SemiBold, fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, 0, OnOutsideTouristFactor1Changed);
                    m_sliderOutsideTouristFactor2 = SettingsSlider.Create(m_panelOutsideSettings.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideTouristFactor2"), UIFonts.SemiBold, fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, 0, OnOutsideTouristFactor2Changed);
                    m_sliderOutsideDummyTrafficFactor = SettingsSlider.Create(m_panelOutsideSettings.m_content, LayoutDirection.Horizontal, Localization.Get("sliderOutsideDummyTrafficFactor"), UIFonts.SemiBold, fTEXT_SCALE, 400, 200, 0f, 1000f, 1f, 20, 0, OnOutsideDummyTrafficChanged);
                    
                    UIButton? btnReset = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, m_panelOutsideSettings.m_content, Localization.Get("btnOutsideReset"), "", 100, 30, null);
                    if (btnReset is not null)
                    {
                        btnReset.eventClick += (c, e) =>
                        {
                            OutsideConnectionSettings.Reset(m_buildingId);
                            UpdateTab(true);
                        };
                    }
                }
            }

            m_bInSetup = false;
        }

        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(true))
            {
                return false;
            }

            if (m_bInSetup)
            {
                return false;
            }

            m_bInSetup = true;

            // Outside connection settings
            if (bActive && m_panelOutsideSettings is not null)
            {
                if (!DependencyUtils.IsAdvancedOutsideConnectionsRunning() && BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                {
                    m_tabStrip.SetTabVisible((int)BuildingPanel.TabIndex.TAB_CAPACITY, true);

                    OutsideConnectionSettings outsideSettings = OutsideConnectionSettings.GetSettings(m_buildingId);

                    if (m_sliderOutsideCargoCapacity is not null)
                    {
                        m_sliderOutsideCargoCapacity.SetValue(outsideSettings.m_cargoCapacity);
                    }
                    if (m_sliderOutsideResidentCapacity is not null)
                    {
                        m_sliderOutsideResidentCapacity.SetValue(outsideSettings.m_residentCapacity);
                    }
                    if (m_sliderOutsideTouristFactor0 is not null)
                    {
                        m_sliderOutsideTouristFactor0.SetValue(outsideSettings.m_touristFactor0);
                    }
                    if (m_sliderOutsideTouristFactor1 is not null)
                    {
                        m_sliderOutsideTouristFactor1.SetValue(outsideSettings.m_touristFactor1);
                    }
                    if (m_sliderOutsideTouristFactor2 is not null)
                    {
                        m_sliderOutsideTouristFactor2.SetValue(outsideSettings.m_touristFactor2);
                    }
                    if (m_sliderOutsideDummyTrafficFactor is not null)
                    {
                        m_sliderOutsideDummyTrafficFactor.SetValue(outsideSettings.m_dummyTrafficFactor);
                    }
                }
                else
                {
                    m_tabStrip.SetTabVisible((int)BuildingPanel.TabIndex.TAB_CAPACITY, false);
                }
            }

            m_bInSetup = false;

            return true;
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

        public override void Destroy()
        {
            if (m_sliderOutsideCargoCapacity is not null)
            {
                m_sliderOutsideCargoCapacity.Destroy();
                m_sliderOutsideCargoCapacity = null;
            }
            if (m_sliderOutsideResidentCapacity is not null)
            {
                m_sliderOutsideResidentCapacity.Destroy();
                m_sliderOutsideResidentCapacity = null;
            }
            if (m_sliderOutsideTouristFactor0 is not null)
            {
                m_sliderOutsideTouristFactor0.Destroy();
                m_sliderOutsideTouristFactor0 = null;
            }
            if (m_sliderOutsideTouristFactor1 is not null)
            {
                m_sliderOutsideTouristFactor1.Destroy();
                m_sliderOutsideTouristFactor1 = null;
            }
            if (m_sliderOutsideTouristFactor2 is not null)
            {
                m_sliderOutsideTouristFactor2.Destroy();
                m_sliderOutsideTouristFactor2 = null;
            }
            if (m_sliderOutsideDummyTrafficFactor is not null)
            {
                m_sliderOutsideDummyTrafficFactor.Destroy();
                m_sliderOutsideDummyTrafficFactor = null;
            }

            base.Destroy();
        }
    }
}