using ColossalFramework.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIGeneralStatsRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblDescription = null;
        private UILabel? m_lblValue = null;
        private StatsBase? m_data = null;
        private UIComponent? m_MouseEnterComponent = null;
        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
            height = ListView.iROW_HEIGHT;
            //backgroundSprite = "InfoviewPanel";
            //color = new Color32(255, 0, 0, 225);
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;

            m_lblDescription = AddUIComponent<UILabel>();
            if (m_lblDescription is not null)
            {
                m_lblDescription.name = "m_lblDescription";
                m_lblDescription.text = "";
                m_lblDescription.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription.tooltip = "";
                m_lblDescription.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription.autoSize = false;
                m_lblDescription.height = height;
                m_lblDescription.width = StatsPanel.iCOLUMN_WIDTH_DESCRIPTION;
                m_lblDescription.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblDescription.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblValue = AddUIComponent<UILabel>();
            if (m_lblValue is not null)
            {
                m_lblValue.name = "m_lblValue";
                m_lblValue.text = "0";
                m_lblValue.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblValue.tooltip = "";
                m_lblValue.textAlignment = UIHorizontalAlignment.Left;
                m_lblValue.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblValue.autoSize = false;
                m_lblValue.height = height;
                m_lblValue.width = StatsPanel.iCOLUMN_WIDTH_VALUE;
                m_lblValue.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblValue.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (StatsBase?)data;

            if (m_lblDescription is null)
            {
                return;
            }

            if (m_data is not null)
            {
                m_lblDescription.text = m_data.m_description;
                m_lblValue.text = m_data.m_value;

                // Update text color
                foreach (UIComponent component in components)
                {
                    if (component is UILabel label)
                    {
                        label.textColor = GetTextColor(label);
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

            if (m_lblDescription is not null)
            {
                m_lblDescription.text = "";
                m_lblValue.text = "";

                tooltip = "";
                m_lblDescription.tooltip = "";
                m_lblValue.tooltip = "";
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
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
        }

        protected void OnMouseHover(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_MouseEnterComponent = component;

            UILabel? txtLabel = component as UILabel;
            if (txtLabel is not null)
            {
                txtLabel.textColor = Color.yellow;
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_MouseEnterComponent = null;

            UILabel? txtLabel = component as UILabel;
            if (txtLabel is not null)
            {
                txtLabel.textColor = m_data.GetColor();
            }
        }

        public virtual Color GetTextColor(UIComponent component)
        {
            if (m_MouseEnterComponent == component)
            {
                return Color.yellow;
            }
            else if (m_data is not null)
            {
                return m_data.GetColor();
            }
            else
            {
                return Color.white;
            }
        }
    }
}