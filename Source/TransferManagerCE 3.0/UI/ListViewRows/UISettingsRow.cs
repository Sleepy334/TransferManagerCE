using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UISettingsRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblId = null;
        private UILabel? m_lblRestrictions = null;
        private UITruncateLabel? m_lblDescription = null;
        private UITruncateLabel? m_lblType = null;
        private UIButton? m_btnDelete = null;

        private SettingsData? m_data = null;
        private Color m_color = Color.white;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width - ListView.iSCROLL_BAR_WIDTH;
            height = ListView.iROW_HEIGHT;
            //backgroundSprite = "InfoviewPanel";
            //color = new Color32(255, 0, 0, 225);
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;
            eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);

            m_lblId = AddUIComponent<UILabel>();
            if (m_lblId is not null)
            {
                m_lblId.name = "m_lblId";
                m_lblId.text = "";
                m_lblId.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblId.tooltip = "";
                m_lblId.textAlignment = UIHorizontalAlignment.Left;
                m_lblId.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblId.autoSize = false;
                m_lblId.height = height;
                m_lblId.width = SettingsPanel.iCOLUMN_WIDTH_ID;
                m_lblId.eventClicked += new MouseEventHandler(OnItemClicked);
            }

            m_lblRestrictions = AddUIComponent<UILabel>();
            if (m_lblRestrictions is not null)
            {
                m_lblRestrictions.name = "m_lblRestrictions";
                m_lblRestrictions.text = "";
                m_lblRestrictions.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblRestrictions.tooltip = "";
                m_lblRestrictions.textAlignment = UIHorizontalAlignment.Center;
                m_lblRestrictions.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblRestrictions.autoSize = false;
                m_lblRestrictions.height = height;
                m_lblRestrictions.width = SettingsPanel.iCOLUMN_WIDTH_RESTRICTIONS;
                m_lblRestrictions.eventClicked += new MouseEventHandler(OnItemClicked);
            }

            m_lblDescription = AddUIComponent<UITruncateLabel>();
            if (m_lblDescription is not null)
            {
                m_lblDescription.name = "m_lblOwner";
                m_lblDescription.text = "";
                m_lblDescription.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription.tooltip = "";
                m_lblDescription.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription.autoSize = false;
                m_lblDescription.height = height;
                m_lblDescription.width = SettingsPanel.iCOLUMN_WIDTH_DESCRIPTION;
                m_lblDescription.eventClicked += new MouseEventHandler(OnItemClicked);
            }

            m_lblType = AddUIComponent<UITruncateLabel>();
            if (m_lblType is not null)
            {
                m_lblType.name = "m_lblOwner";
                m_lblType.text = "";
                m_lblType.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblType.tooltip = "";
                m_lblType.textAlignment = UIHorizontalAlignment.Left;
                m_lblType.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblType.autoSize = false;
                m_lblType.height = height;
                m_lblType.width = SettingsPanel.iCOLUMN_WIDTH_DESCRIPTION;
                m_lblType.eventClicked += new MouseEventHandler(OnItemClicked);
            }

            m_btnDelete = AddUIComponent<UIButton>();
            if (m_btnDelete is not null)
            {
                float fBUTTON_HEIGHT = height - 6;

                m_btnDelete.height = fBUTTON_HEIGHT;
                m_btnDelete.width = fBUTTON_HEIGHT;
                m_btnDelete.normalBgSprite = "buttonclose";
                m_btnDelete.hoveredBgSprite = "buttonclosehover";
                m_btnDelete.pressedBgSprite = "buttonclosepressed";
                m_btnDelete.eventClick += (component, param) =>
                {
                    if (m_data is not null)
                    {
                        // Clear tooltip
                        if (m_btnDelete.tooltipBox is not null)
                        {
                            m_btnDelete.tooltip = "";
                            m_btnDelete.tooltipBox.Hide();
                        }

                        BuildingSettingsStorage.ClearSettings(m_data.GetBuildingId());
                        SettingsPanel.Instance.UpdatePanel();
                    }
                };
            }

            // Auto size last column
            m_lblType.width = width - m_lblId.width - m_lblRestrictions.width - m_lblDescription.width - m_btnDelete.width - 20;

            // Update row if we already have data
            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (SettingsData?) data;

            if (m_lblDescription is null)
            {
                return;
            }

            if (m_data is not null)
            {
                tooltip = m_data.DescribeSettings();
                m_lblId.text = m_data.GetBuildingId().ToString();
                m_lblRestrictions.text = m_data.GetRestrctionCount().ToString();
                m_lblDescription.text = m_data.GetDescription();
                m_lblType.text = m_data.DescribeDistricts();
                m_btnDelete.tooltip = $"Delete settings for building: {m_data.GetBuildingId()}";

                // Update text color
                foreach (UIComponent component in components)
                {
                    if (component is UILabel label)
                    {
                        label.textColor = GetTextColor();
                    }
                }
            }
            else
            {
                Clear();
            }
        }

        public void Disabled()
        {
            Clear();
        }

        public void Clear()
        {
            m_data = null;
            tooltip = "";

            if (m_lblId is not null)
            {
                m_lblId.text = "";
                m_lblRestrictions.text = "";
                m_lblDescription.text = "";
                m_lblType.text = "";

                m_lblId.tooltip = "";
                m_lblRestrictions.tooltip = "";
                m_lblDescription.tooltip = "";
                m_lblType.tooltip = "";
                m_btnDelete.tooltip = "";
            }
        }

        public void Select(bool isRowOdd)
        {
        }

        public void Deselect(bool isRowOdd)
        {
        }

        private void OnItemClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_data is not null)
            {
                m_data.ShowInstance();

                if (SettingsPanel.Instance != null)
                {
                    SettingsPanel.Instance.InvalidatePanel();
                }
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_data is not null)
            {
                tooltip = m_data.DescribeSettings();
            }
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UIComponent? c in components)
            {
                if (c is UILabel label)
                {
                    label.textColor = Color.yellow;
                }
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UIComponent? c in components)
            {
                if (c is UILabel label)
                {
                    label.textColor = GetTextColor();
                }
            }
        }

        public Color GetTextColor()
        {
            if (m_data is not null &&
                GetSelectedBuilding() == m_data.GetBuildingId())
            {
                m_color = KnownColor.orange;
            }
            else
            {
                m_color = Color.white;
            }
            
            return m_color;
        }

        private ushort GetSelectedBuilding()
        {
            InstanceID selectedId = InstanceHelper.GetTargetInstance();
            if (selectedId.Building != 0)
            {
                return selectedId.Building;
            }
            else if (BuildingPanel.Instance is not null)
            {
                return BuildingPanel.Instance.GetBuildingId();
            }
            else
            {
                return 0;
            }   
        }
    }
}