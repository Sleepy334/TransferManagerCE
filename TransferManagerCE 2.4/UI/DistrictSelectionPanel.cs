using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class DistrictSelectionPanel : UIPanel
    {
        const int iMARGIN = 8;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 150;

        public static DistrictSelectionPanel? Instance = null;

        public ushort m_buildingId;
        public int m_iRestrictionId;

        public bool m_bIncoming;
        private UITitleBar? m_title = null;
        private UILabel? m_lblSource = null;
        private UICheckBox? m_chkCurrentDistrict = null;
        private UICheckBox? m_chkCurrentPark = null;
        private UILabel? m_lblAdditional = null;
        private CheckListView? m_chkListView = null;

        public DistrictSelectionPanel() : base()
        {
        }

        public ushort GetBuildingid()
        {
            return m_buildingId;
        }

        public int GetRestrictionid()
        {
            return m_iRestrictionId;
        }

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(DistrictSelectionPanel)) as DistrictSelectionPanel;
                if (Instance is null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating District Selection Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "DistrictPanel";
            width = 350;
            height = 540;
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "SubcategoriesPanel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventVisibilityChanged += OnVisibilityChanged;
            CenterTo(parent);

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = GetTitle();

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height - m_title.height;
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;

            // Object label
            m_lblSource = mainPanel.AddUIComponent<UILabel>();
            m_lblSource.height = 30;
            m_lblSource.width = width;
            m_lblSource.padding = new RectOffset(4, 4, 4, 4);
            m_lblSource.text = "Building";
            m_lblSource.textAlignment = UIHorizontalAlignment.Center;
            m_lblSource.textScale = 1.0f;

            m_chkCurrentDistrict = UIUtils.AddCheckbox(mainPanel, Localization.Get("txtCurrentDistrict") + ": ", 1.0f, true, OnDistrictChanged);
            m_chkCurrentPark = UIUtils.AddCheckbox(mainPanel, Localization.Get("txtCurrentPark") + ": ", 1.0f, true, OnParkChanged);

            // Object label
            m_lblAdditional = mainPanel.AddUIComponent<UILabel>();
            m_lblAdditional.height = 30;
            m_lblAdditional.width = width;
            m_lblAdditional.padding = new RectOffset(4, 4, 4, 4);
            m_lblAdditional.text = "Additional Districts:";
            m_lblAdditional.textAlignment = UIHorizontalAlignment.Center;
            m_lblAdditional.textScale = 1.0f;

            m_chkListView = CheckListView.Create(mainPanel, "ScrollbarTrack", 1.0f, width - 36, 400);

            isVisible = true;
            UpdatePanel();
        }

        public void SetPanelBuilding(ushort buildingId, int iRestrictionId)
        {
            m_buildingId = buildingId;
            m_iRestrictionId = iRestrictionId;

            UpdatePanel();
        }

        public void ShowPanel(ushort buildingId, int iRestrictionId, bool bIncoming)
        {
            m_buildingId = buildingId;
            m_iRestrictionId = iRestrictionId;

            m_bIncoming = bIncoming;
            Show();

            if (m_title is not null)
            {
                m_title.title = GetTitle();
            }

            UpdatePanel();
            BringToFront();

            // Update selection tool mode
            if (bIncoming)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.DistrictRestrictionIncoming);
            }
            else
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.DistrictRestrictionOutgoing);
            }
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

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (!bVisible)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
            }
        }

        public void OnDistrictChanged(bool bChecked)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            if (settings is not null)
            {
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(m_iRestrictionId);

                if (m_bIncoming)
                {
                    restrictions.m_incomingDistrictSettings.m_bAllowLocalDistrict = bChecked;
                }
                else
                {
                    restrictions.m_outgoingDistrictSettings.m_bAllowLocalDistrict = bChecked;
                }

                DistrictSelectionPatches.UpdateDistricts();
            }
        }

        public void OnParkChanged(bool bChecked)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            if (settings is not null)
            {
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(m_iRestrictionId);

                if (m_bIncoming)
                {
                    restrictions.m_incomingDistrictSettings.m_bAllowLocalPark = bChecked;
                }
                else
                {
                    restrictions.m_outgoingDistrictSettings.m_bAllowLocalPark = bChecked;
                }

                DistrictSelectionPatches.UpdateDistricts();
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

        public List<CheckListData> GetAdditionalDistricts()
        {
            List<CheckListData> list = new List<CheckListData>();

            // Exclude "current" districts from additional list.
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            byte currentDistrict = DistrictManager.instance.GetDistrict(building.m_position);
            byte currentPark = DistrictManager.instance.GetPark(building.m_position);

            // Districts
            for (int i = 1; i < DistrictManager.instance.m_districts.m_buffer.Length; ++i)
            {
                if (i != currentDistrict)
                {
                    District district = DistrictManager.instance.m_districts.m_buffer[i];
                    if (district.m_flags != 0)
                    {
                        list.Add(new CheckListData(m_bIncoming, DistrictData.DistrictType.District, (byte)i));
                    }
                }
            }

            // Parks
            for (int i = 1; i < DistrictManager.instance.m_parks.m_buffer.Length; ++i)
            {
                if (i != currentPark)
                {
                    DistrictPark park = DistrictManager.instance.m_parks.m_buffer[i];
                    if (park.m_flags != 0)
                    {
                        list.Add(new CheckListData(m_bIncoming, DistrictData.DistrictType.Park, (byte)i));
                    }
                }
            }

            list.Sort();
            return list;
        }

        public void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            if (m_lblSource is not null)
            {
                string sText = CitiesUtils.GetBuildingName(m_buildingId);
                if (BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                {
                    sText += ":" + m_buildingId + " (Outside Connection)";
                }
                m_lblSource.text = sText;
            }

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictionSettings = settings.GetRestrictionsOrDefault(m_iRestrictionId);

            // Are settings allowed for this building/Direction
            bool bDisable = false;
            if (m_bIncoming && restrictionSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts == DistrictRestrictionSettings.PreferLocal.AllDistricts)
            {
                bDisable = true;
            }
            else if (!m_bIncoming && restrictionSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts == DistrictRestrictionSettings.PreferLocal.AllDistricts)
            {
                bDisable = true;
            }

            if (m_chkCurrentDistrict is not null)
            {
                m_chkCurrentDistrict.text = Localization.Get("txtCurrentDistrict") + ": " + CitiesUtils.GetDistrictName(m_buildingId);
                if (m_bIncoming)
                {
                    m_chkCurrentDistrict.isChecked = restrictionSettings.m_incomingDistrictSettings.m_bAllowLocalDistrict;
                }
                else
                {
                    m_chkCurrentDistrict.isChecked = restrictionSettings.m_outgoingDistrictSettings.m_bAllowLocalDistrict;
                }
                m_chkCurrentDistrict.isEnabled = !bDisable;
            }
            if (m_chkCurrentPark is not null)
            {
                m_chkCurrentPark.text = Localization.Get("txtCurrentPark") + ": " + CitiesUtils.GetParkName(m_buildingId);
                if (m_bIncoming)
                {
                    m_chkCurrentPark.isChecked = restrictionSettings.m_incomingDistrictSettings.m_bAllowLocalPark;
                }
                else
                {
                    m_chkCurrentPark.isChecked = restrictionSettings.m_outgoingDistrictSettings.m_bAllowLocalPark;
                }
                m_chkCurrentPark.isEnabled = !bDisable;
            }
            if (m_chkListView is not null)
            {
                m_chkListView.SetItems(GetAdditionalDistricts().ToArray());
                m_chkListView.isEnabled = !bDisable;
            }

            DistrictSelectionPatches.UpdateDistricts();
        }

        public override void OnDestroy()
        {
            if (m_lblSource is not null)
            {
                Destroy(m_lblSource.gameObject);
                m_lblSource = null;
            }
            if (m_chkListView is not null)
            {
                Destroy(m_chkListView.gameObject);
                m_chkListView = null;
            }
            if (m_lblAdditional is not null)
            {
                Destroy(m_lblAdditional.gameObject);
                m_lblAdditional = null;
            }
            if (Instance is not null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}