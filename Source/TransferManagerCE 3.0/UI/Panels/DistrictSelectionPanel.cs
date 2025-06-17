using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class DistrictSelectionPanel : UIMainPanel<DistrictSelectionPanel>
    {
        const int iMARGIN = 8;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 150;

        public bool m_bIncoming;
        public ushort m_buildingId;
        public int m_iRestrictionId;
        static HashSet<DistrictData> s_emptyDistricts = new HashSet<DistrictData>();

        private UITitleBar? m_title = null;
        private UIPanel m_mainPanel = null;
        private UILabel? m_lblSource = null;
        private UICheckBox? m_chkCurrentDistrict = null;
        private UICheckBox? m_chkCurrentPark = null;

        private ListView? m_selectedDistricts = null;
        private CheckListView? m_chkListView = null;

        private UICheckBox? m_chkShowAllDistricts = null;

        // Panel sync
        private UIPanel m_parent = null;
        private bool m_bPanelSync = true;

        // ----------------------------------------------------------------------------------------
        public DistrictSelectionPanel() : 
            base()
        {
        }

        public ushort GetBuildingId()
        {
            return m_buildingId;
        }

        public int GetRestrictionId()
        {
            return m_iRestrictionId;
        }

        public bool IsIncoming()
        {
            return m_bIncoming;
        }

        public override void Start()
        {
            base.Start();
        }

        public void Create(UIPanel parent)
        {
            if (m_parent is null)
            {
                m_parent = parent;

                // Hook into parents events
                parent.eventVisibilityChanged += OnParentVisibilityChanged;
                parent.eventPositionChanged += OnParentPostitionChanged;

                name = "DistrictPanel";
                width = 350;
                height = parent.height;
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
                m_title = UITitleBar.Create(this, GetTitle(), "Transfer", TransferManagerMod.Instance.LoadResources(), OnCloseClick);
                if (m_title != null)
                {
                    m_title.SetupButtons(OnDragStart);
                }

                m_mainPanel = AddUIComponent<UIPanel>();
                m_mainPanel.width = width;
                m_mainPanel.height = height - m_title.height;
                m_mainPanel.relativePosition = new Vector3(0f, m_title.height);
                m_mainPanel.padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
                m_mainPanel.autoLayout = true;
                m_mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
                //m_mainPanel.backgroundSprite = "InfoviewPanel";
                //m_mainPanel.color = Color.blue;

                // Object label
                m_lblSource = m_mainPanel.AddUIComponent<UILabel>();
                m_lblSource.height = 30;
                m_lblSource.width = m_mainPanel.width;
                m_lblSource.padding = new RectOffset(4, 4, 4, 4);
                m_lblSource.text = "Building";
                m_lblSource.textAlignment = UIHorizontalAlignment.Center;
                m_lblSource.textScale = 1.0f;

                float fTextScale = 0.7f;
                m_chkCurrentDistrict = UIMyUtils.AddCheckbox(m_mainPanel, Localization.Get("txtCurrentDistrict") + ": ", UIFonts.Regular, fTextScale, true, OnDistrictChanged);
                m_chkCurrentPark = UIMyUtils.AddCheckbox(m_mainPanel, Localization.Get("txtCurrentPark") + ": ", UIFonts.Regular, fTextScale, true, OnParkChanged);

                float fShowAllDistrictsHeight = 20;
                float fListHeight = m_mainPanel.height - m_lblSource.height - m_chkCurrentDistrict.height - m_chkCurrentPark.height - 20 - fShowAllDistrictsHeight;
                m_selectedDistricts = ListView.Create<UIDistrictRow>(m_mainPanel, "ScrollbarTrack", 1.0f, m_mainPanel.width - 20, fListHeight);
                if (m_selectedDistricts is not null)
                {
                    m_selectedDistricts.padding = new RectOffset(0, 0, iMARGIN, 0);
                    m_selectedDistricts.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("txtAdditionalDistricts"), "", (int)m_selectedDistricts.width - 50, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                }

                m_chkListView = CheckListView.Create(m_mainPanel, "ScrollbarTrack", 1.0f, m_mainPanel.width - 36, fListHeight);
                m_chkListView.RowHeight = 20;

                // Add a panel for postioning
                UIPanel pnlShowAllDistricts = m_mainPanel.AddUIComponent<UIPanel>();
                pnlShowAllDistricts.width = m_mainPanel.width;
                pnlShowAllDistricts.height = 30;

                m_chkShowAllDistricts = UIMyUtils.AddCheckbox(pnlShowAllDistricts, Localization.Get("optionShowAllDistricts"), UIFonts.Regular, 0.8f, ModSettings.GetSettings().ShowAllDistricts, OnShowAllDistrictsClicked);
                m_chkShowAllDistricts.relativePosition = new Vector2(10, 10);
                m_chkShowAllDistricts.autoSize = false;
                m_chkShowAllDistricts.width = pnlShowAllDistricts.width;
                m_chkShowAllDistricts.height = fShowAllDistrictsHeight;
                m_chkShowAllDistricts.PerformLayout();

                isVisible = true;
                UpdateAdditionalDistrictsPanelVisibility();
                UpdatePanel();
            }
        }

        public void SetPanelBuilding(ushort buildingId, int iRestrictionId)
        {
            if (buildingId != m_buildingId || m_iRestrictionId != iRestrictionId) 
            {
                m_buildingId = buildingId;
                m_iRestrictionId = iRestrictionId;
                UpdatePanel();
            }
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

            SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.DistrictRestriction);
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
            if (bVisible)
            {
                m_bPanelSync = true;
                PostitionPanel();

                if (OutsideConnectionSelectionPanel.IsVisible())
                {
                    OutsideConnectionSelectionPanel.Instance.Hide();
                }
            }
            else 
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

        private void OnShowAllDistrictsClicked(bool bEnabled)
        {
            ModSettings.GetSettings().ShowAllDistricts = bEnabled;
            ModSettings.GetSettings().Save();
            UpdatePanel();
        }

        public List<DistrictData> GetAdditionalDistricts()
        {
            List<DistrictData> districts = null;

            // Exclude "current" districts from additional list.
            Building building = BuildingManager.instance.m_buildings.m_buffer[GetBuildingId()];
            if (building.m_flags != 0)
            {
                // Districts
                BuildingSettings? settings = BuildingSettingsStorage.GetSettings(GetBuildingId());
                if (settings is not null)
                {
                    RestrictionSettings? restrictions = settings.GetRestrictions(GetRestrictionId());
                    if (restrictions is not null)
                    {
                        if (m_bIncoming)
                        {
                            districts = restrictions.m_incomingDistrictSettings.m_allowedDistricts.ToList();
                        }
                        else
                        {
                            districts = restrictions.m_outgoingDistrictSettings.m_allowedDistricts.ToList();
                        }
                    }
                }
            }

            if (districts is not null)
            {
                districts.Sort();
                return districts;
            }
            else
            {
                return new List<DistrictData>();
            }
        }

        protected override void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictionSettings = settings.GetRestrictionsOrDefault(m_iRestrictionId);

            // Are settings allowed for this building/Direction
            if (m_bIncoming && restrictionSettings.m_incomingDistrictSettings.m_iPreferLocalDistricts == DistrictRestrictionSettings.PreferLocal.AllDistricts)
            {
                Hide();
                return;
            }
            else if (!m_bIncoming && restrictionSettings.m_outgoingDistrictSettings.m_iPreferLocalDistricts == DistrictRestrictionSettings.PreferLocal.AllDistricts)
            {
                Hide();
                return;
            }

            if (m_lblSource is not null)
            {
                string sText = CitiesUtils.GetBuildingName(m_buildingId, InstanceID.Empty);
                if (BuildingTypeHelper.IsOutsideConnection(m_buildingId))
                {
                    sText += ":" + m_buildingId + " (Outside Connection)";
                }
                m_lblSource.text = sText;
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
            }

            // Load additional district data
            if (ModSettings.GetSettings().ShowAllDistricts)
            {
                if (m_chkListView is not null)
                {
                    m_chkListView.SetItems(GetAllAdditionalDistricts().ToArray());
                }
            }
            else
            {
                if (m_selectedDistricts is not null)
                {
                    List<DistrictData> additionalDistricts = GetAdditionalDistricts();
                    m_selectedDistricts.GetList().rowsData = new FastList<object>
                    {
                        m_buffer = additionalDistricts.ToArray(),
                        m_size = additionalDistricts.Count,
                    };
                }
            }

            UpdateAdditionalDistrictsPanelVisibility();
            DistrictSelectionPatches.UpdateDistricts();
            PostitionPanel();
        }

        private void UpdateAdditionalDistrictsPanelVisibility()
        {
            if (ModSettings.GetSettings().ShowAllDistricts)
            {
                if (m_selectedDistricts is not null)
                {
                    m_selectedDistricts.isVisible = false;
                }

                if (m_chkListView is not null)
                {
                    m_chkListView.isVisible = true;
                }
            }
            else
            {
                if (m_chkListView is not null)
                {
                    m_chkListView.isVisible = false;
                }

                if (m_selectedDistricts is not null)
                {
                    m_selectedDistricts.isVisible = true;
                }
            }
        }

        public override void OnDestroy()
        {
            if (m_lblSource is not null)
            {
                Destroy(m_lblSource.gameObject);
                m_lblSource = null;
            }
            if (m_selectedDistricts is not null)
            {
                Destroy(m_selectedDistricts.gameObject);
                m_selectedDistricts = null;
            }
        }

        protected void OnParentVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                PostitionPanel();
            }
            else
            {
                Hide();
            }
        }

        protected void OnParentPostitionChanged(UIComponent component, Vector2 position)
        {
            PostitionPanel();
        }

        protected void OnDragStart(UIComponent component, UIDragEventParameter eventParam)
        {
            m_bPanelSync = false;
        }

        private void PostitionPanel()
        {
            if (m_bPanelSync && m_parent is not null)
            {
                // Display tooltip next to main panel
                UIView uiview = GetUIView();

                Vector2 vector = (ToolBase.fullscreenContainer is null) ? uiview.GetScreenResolution() : ToolBase.fullscreenContainer.size;

                float fScreenMiddle = vector.x * 0.5f;
                float fParentPanelMiddle = m_parent.relativePosition.x + (m_parent.width * 0.5f);

                Vector3 newRelativePosition;
                if (fParentPanelMiddle < fScreenMiddle)
                {
                    // To the right
                    newRelativePosition = m_parent.relativePosition + new Vector3(m_parent.width + 6, 0.0f);
                }
                else
                {
                    // To the left
                    newRelativePosition = m_parent.relativePosition - new Vector3(width + 6, 0.0f);
                }

                // Check its within screen
                if (newRelativePosition.x < 0f)
                {
                    newRelativePosition.x = 0f;
                }
                if (newRelativePosition.x + width > vector.x)
                {
                    newRelativePosition.x = vector.x - width;
                }

                relativePosition = newRelativePosition;
            }
        }

        public void ToggleDistrictAllowed(DistrictData.DistrictType eType, byte district)
        {
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(m_iRestrictionId);

            if (m_bIncoming)
            {
                restrictions.m_incomingDistrictSettings.ToggleDistrictAllowed(m_buildingId, eType, district);
            }
            else
            {
                restrictions.m_outgoingDistrictSettings.ToggleDistrictAllowed(m_buildingId, eType, district);
            }

            settings.SetRestrictions(m_iRestrictionId, restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdatePanel();
        }

        public HashSet<DistrictData> GetAllowedDistricts()
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            if (settings is not null)
            {
                RestrictionSettings? restrictions = settings.GetRestrictions(m_iRestrictionId);
                if (restrictions is not null)
                {
                    if (IsIncoming())
                    {
                        return restrictions.m_incomingDistrictSettings.GetAllowedDistricts(m_buildingId, null, null);
                    }
                    else
                    {
                        return restrictions.m_outgoingDistrictSettings.GetAllowedDistricts(m_buildingId, null, null);
                    }
                }
            }

            return s_emptyDistricts;
        }

        public List<CheckListData> GetAllAdditionalDistricts()
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
                        list.Add(new DistrictCheckListData(m_bIncoming, DistrictData.DistrictType.District, (byte)i));
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
                        list.Add(new DistrictCheckListData(m_bIncoming, DistrictData.DistrictType.Park, (byte)i));
                    }
                }
            }

            list.Sort();
            return list;
        }

    }
}