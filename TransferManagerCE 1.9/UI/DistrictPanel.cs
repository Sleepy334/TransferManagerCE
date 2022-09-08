using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class DistrictPanel : UIPanel
    {
        const int iMARGIN = 8;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 150;

        public static DistrictPanel? Instance = null;

        public ushort m_buildingId;
        public bool m_bIncoming;
        private UITitleBar? m_title = null;
        private UILabel? m_lblSource = null;
        private UICheckBox? m_chkCurrentDistrict = null;
        private UICheckBox? m_chkCurrentPark = null;
        private UILabel? m_lblAdditional = null;
        private CheckListView? m_chkListView = null;

        public DistrictPanel() : base()
        {
        }

        public static void Init(UIPanel parent)
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(DistrictPanel)) as DistrictPanel;
                if (Instance == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating District Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "DistrictPanel";
            width = 350;
            height = 540;
            opacity = 0.95f;
            padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            CenterToParent();

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = GetTitle();

            // Object label
            m_lblSource = AddUIComponent<UILabel>();
            m_lblSource.height = 30;
            m_lblSource.width = width;
            m_lblSource.padding = new RectOffset(4, 4, 4, 4);
            m_lblSource.text = "Building";
            m_lblSource.textAlignment = UIHorizontalAlignment.Center;
            m_lblSource.textScale = 1.0f;

            m_chkCurrentDistrict = UIUtils.AddCheckbox(this, Localization.Get("txtCurrentDistrict") + ": ", 1.0f, true, OnDistrictChanged);
            m_chkCurrentPark = UIUtils.AddCheckbox(this, Localization.Get("txtCurrentPark") + ": ", 1.0f, true, OnParkChanged);

            // Object label
            m_lblAdditional = AddUIComponent<UILabel>();
            m_lblAdditional.height = 30;
            m_lblAdditional.width = width;
            m_lblAdditional.padding = new RectOffset(4, 4, 4, 4);
            m_lblAdditional.text = "Additional Districts:";
            m_lblAdditional.textAlignment = UIHorizontalAlignment.Center;
            m_lblAdditional.textScale = 1.0f;

            m_chkListView = CheckListView.Create(this, "ScrollbarTrack", 1.0f, width - 36, 400);

            isVisible = true;
            UpdatePanel();
        }

        public void SetPanelBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
            UpdatePanel();
        }

        public void ShowPanel(ushort buildingId, bool bIncoming)
        {
            m_buildingId = buildingId;
            m_bIncoming = bIncoming;
            Show();

            if (m_title != null)
            {
                m_title.title = GetTitle();
            }

            UpdatePanel();
            BringToFront();
        }

        public string GetTitle()
        {
            if (m_bIncoming)
            {
                return Localization.Get("titleDistrictPanel") + " (" + Localization.Get("dropdownBuildingPanelIncomingPreferLocalLabel") + ")";
            }
            else
            {
                return Localization.Get("titleDistrictPanel") + " (" + Localization.Get("dropdownBuildingPanelOutgoingPreferLocalLabel") + ")";
            }
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        public void OnDistrictChanged(bool bChecked)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);
            if (m_bIncoming)
            {
                settings.m_bIncomingAllowLocalDistrict = bChecked;
            }
            else
            {
                settings.m_bOutgoingAllowLocalDistrict = bChecked;
            }
        }

        public void OnParkChanged(bool bChecked)
        {
            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);
            if (m_bIncoming)
            {
                settings.m_bIncomingAllowLocalPark = bChecked;
            }
            else
            {
                settings.m_bOutgoingAllowLocalPark = bChecked;
            }
        }

        public void TogglePanel()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public List<CheckListData> GetDistricts()
        {
            List<CheckListData> list = new List<CheckListData>();
            
            for (int i = 1; i < DistrictManager.instance.m_districts.m_buffer.Length; ++i)
            {
                District district = DistrictManager.instance.m_districts.m_buffer[i];
                if (district.m_flags != 0)
                {
                    list.Add(new CheckListData(m_bIncoming, DistrictData.DistrictType.District, (byte)i));
                }
            }

            for (int i = 1; i < DistrictManager.instance.m_parks.m_buffer.Length; ++i)
            {
                DistrictPark park = DistrictManager.instance.m_parks.m_buffer[i];
                if (park.m_flags != 0)
                {
                    list.Add(new CheckListData(m_bIncoming, DistrictData.DistrictType.Park, (byte)i));
                }
            }

            list.Sort();
            return list;
        }

        public void UpdatePanel()
        {
            if (m_lblSource != null)
            {
                string sText = CitiesUtils.GetBuildingName(m_buildingId);
                if (BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                {
                    sText += ":" + m_buildingId + " (Outside Connection)";
                }
                m_lblSource.text = sText;
            }

            BuildingSettings settings = BuildingSettings.GetSettings(m_buildingId);

            // Are settings allowed for this building/Direction
            bool bDisable = false;
            if (m_bIncoming && settings.m_iPreferLocalDistrictsIncoming == BuildingSettings.PreferLocal.ALL_DISTRICTS)
            {
                bDisable = true;
            }
            else if (!m_bIncoming && settings.m_iPreferLocalDistrictsOutgoing == BuildingSettings.PreferLocal.ALL_DISTRICTS)
            {
                bDisable = true;
            }

            if (m_chkCurrentDistrict != null)
            {
                m_chkCurrentDistrict.text = Localization.Get("txtCurrentDistrict") + ": " + CitiesUtils.GetDistrictName(m_buildingId);
                if (m_bIncoming)
                {
                    m_chkCurrentDistrict.isChecked = settings.m_bIncomingAllowLocalDistrict;
                }
                else
                {
                    m_chkCurrentDistrict.isChecked = settings.m_bOutgoingAllowLocalDistrict;
                }
                m_chkCurrentDistrict.isEnabled = !bDisable;
            }
            if (m_chkCurrentPark != null)
            {
                m_chkCurrentPark.text = Localization.Get("txtCurrentPark") + ": " + CitiesUtils.GetParkName(m_buildingId);
                if (m_bIncoming)
                {
                    m_chkCurrentPark.isChecked = settings.m_bIncomingAllowLocalPark;
                }
                else
                {
                    m_chkCurrentPark.isChecked = settings.m_bOutgoingAllowLocalPark;
                }
                m_chkCurrentPark.isEnabled = !bDisable;
            }
            if (m_chkListView != null)
            {
                m_chkListView.SetItems(GetDistricts().ToArray());
                m_chkListView.isEnabled = !bDisable;
            }
        }
    }
}