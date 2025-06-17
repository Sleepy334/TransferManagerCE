using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class OutsideConnectionSelectionPanel : UIMainPanel<OutsideConnectionSelectionPanel>
    {
        const int iMARGIN = 8;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 150;

        public ushort m_buildingId;
        public int m_iRestrictionId;

        private UITitleBar? m_title = null;
        private UIPanel m_mainPanel = null;
        private CheckListView? m_chkListView = null;
        private UICheckBox? m_checkUncheckAll = null;

        // Panel sync
        private UIPanel m_parent = null;
        private bool m_bPanelSync = true;

        // ----------------------------------------------------------------------------------------
        public OutsideConnectionSelectionPanel() : 
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

                name = "OutsideConnectionSelectionPanel";
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

                m_checkUncheckAll = UIMyUtils.AddCheckbox(m_mainPanel, "Check / Uncheck All", UIFonts.Regular, 0.8f, true, CheckUncheckAll);

                m_chkListView = CheckListView.Create(m_mainPanel, "ScrollbarTrack", 1.0f, m_mainPanel.width - 36, m_mainPanel.height - m_checkUncheckAll.height - 8);
                m_chkListView.RowHeight = 20;

                isVisible = true;
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

        public void ShowPanel(ushort buildingId, int iRestrictionId)
        {
            Create(BuildingPanel.Instance);

            m_buildingId = buildingId;
            m_iRestrictionId = iRestrictionId;

            Show();

            if (m_title is not null)
            {
                m_title.title = GetTitle();
            }

            UpdatePanel();
            BringToFront();
        }

        public string GetTitle()
        {
            return Localization.Get("GROUP_BUILDINGPANEL_OUTSIDE_CONNECTIONS");
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

                if (DistrictSelectionPanel.IsVisible())
                {
                    DistrictSelectionPanel.Instance.Hide();
                }
            }
            else 
            { 
                if (BuildingPanel.IsVisible()) 
                {
                    BuildingPanel.Instance.InvalidatePanel();
                }
            }
        }

        protected override void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            if (m_buildingId != 0)
            {
                if (m_chkListView is not null)
                {
                    m_chkListView.SetItems(GetOutsideConnections().ToArray());
                }
            }
            else
            {
                m_chkListView.Clear();
            }
        }

        public override void OnDestroy()
        {
            if (m_chkListView is not null)
            {
                Destroy(m_chkListView.gameObject);
                m_chkListView = null;
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

        public void ToggleOutsideConnection(ushort outsideConnectionBuildingId)
        {
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(m_iRestrictionId);

            if (restrictions.m_excludedOutsideConnections.Contains(outsideConnectionBuildingId))
            {
                restrictions.m_excludedOutsideConnections.Remove(outsideConnectionBuildingId);
            }
            else
            {
                restrictions.m_excludedOutsideConnections.Add(outsideConnectionBuildingId);
            }

            settings.SetRestrictions(m_iRestrictionId, restrictions);
            BuildingSettingsStorage.SetSettings(m_buildingId, settings);

            UpdatePanel();
        }

        public List<OutsideCheckListData> GetOutsideConnections()
        {
            List<OutsideCheckListData> result = new List<OutsideCheckListData>();

            FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
            foreach (ushort connection in connections)
            {
                result.Add(new OutsideCheckListData(m_buildingId, m_iRestrictionId, connection));
            }

            result.Sort();

            return result;
        }

        private void CheckUncheckAll(bool bChecked)
        {
            // Select all OC's
            if (m_buildingId != 0 && m_iRestrictionId != -1)
            {
                BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(m_iRestrictionId);

                if (bChecked)
                {
                    // An empty exclusion list means all are ON
                    restrictions.m_excludedOutsideConnections.Clear();
                }
                else
                {
                    // An full exclusion list means all are OFF
                    FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
                    foreach (ushort connection in connections)
                    {
                        restrictions.m_excludedOutsideConnections.Add(connection);
                    }
                }

                settings.SetRestrictions(m_iRestrictionId, restrictions);
                BuildingSettingsStorage.SetSettings(m_buildingId, settings);

                UpdatePanel();

                if (BuildingPanel.IsVisible())
                {
                    BuildingPanel.Instance.InvalidatePanel();
                }
            }
        }
    }
}