using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIGeneralStatsRow : UIListRow<StatsBase>
    {
        private UILabel? m_lblDescription = null;
        private UILabel? m_lblValue = null;

        public override void Start()
        {
            base.Start();

            fullRowSelect = true;

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
            }

            AfterStart();
        }

        protected override void Display()
        {
            m_lblDescription.text = data.m_description;
            m_lblValue.text = data.m_value;
        }

        protected override void Clear()
        {
            m_lblDescription.text = "";
            m_lblValue.text = "";
        }

        protected override void ClearTooltips()
        {
            tooltip = "";
            m_lblDescription.tooltip = "";
            m_lblValue.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
        }

        protected override string GetTooltipText(UIComponent component)
        {
            return "";
        }

        protected override Color GetTextColor(UIComponent component, bool highlightRow)
        {
            if (data is not null)
            {
                if (!data.IsHeader())
                {
                    if (highlightRow)
                    {
                        return Color.yellow;
                    }
                    else
                    {
                        return data.GetColor();
                    }
                }
            }

            return Color.white;
        }
    }
}