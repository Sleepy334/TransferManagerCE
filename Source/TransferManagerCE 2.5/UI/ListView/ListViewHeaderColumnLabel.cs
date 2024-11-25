using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class ListViewHeaderColumnLabel : ListViewHeaderColumnBase
    {
        private UILabel? m_lblColumn = null;

        public ListViewHeaderColumnLabel(ListViewRowComparer.Columns eColumn, UIComponent parent, string sText, string sTooltip, float textScale, int iWidth, int iHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, OnListViewColumnClick eventClickCallback) :
                base(eColumn, sText, eventClickCallback)
        {
            m_lblColumn = parent.AddUIComponent<UILabel>();
            m_lblColumn.name = eColumn.ToString();
            m_lblColumn.text = sText;
            m_lblColumn.textScale = textScale;
            m_lblColumn.tooltip = sTooltip;
            m_lblColumn.textAlignment = oTextAlignment;
            m_lblColumn.verticalAlignment = UIVerticalAlignment.Middle;
            m_lblColumn.autoSize = false;
            m_lblColumn.height = iHeight;
            m_lblColumn.width = iWidth;
            m_lblColumn.AlignTo(parent, oAncor);
            m_lblColumn.eventMouseEnter += OnMouseHover;
            m_lblColumn.eventMouseLeave += OnMouseLeave;
            m_lblColumn.eventClick += new MouseEventHandler(OnItemClicked);
        }

        public override void Sort(ListViewRowComparer.Columns eColumn, bool bDescending)
        {
            if (m_lblColumn is not null)
            {
                m_lblColumn.text = m_sText;

                if (eColumn == m_eColumn)
                {
                    string sSortCharacter = Utils.GetSortCharacter(bDescending);
                    if (m_lblColumn.textAlignment == UIHorizontalAlignment.Left)
                    {
                        m_lblColumn.text += " " + sSortCharacter;
                    }
                    else
                    {
                        m_lblColumn.text = sSortCharacter + " " + m_lblColumn.text;
                    }
                }
            }
        }

        public override bool IsHit(UIComponent component)
        {
            return component == m_lblColumn;
        }

        public override void SetText(string sText)
        {
            if (m_lblColumn is not null)
            {
                m_lblColumn.text = sText;
            }
        }

        public override void SetTooltip(string sText)
        {
            if (m_lblColumn is not null)
            {
                m_lblColumn.tooltip = sText;
            }
        }

        private void HideTooltipBox()
        {
            if (m_lblColumn is not null && m_lblColumn.tooltipBox is not null)
            {
                m_lblColumn.tooltipBox.isVisible = false;
            }
        }

        public override void Destroy()
        {
            HideTooltipBox();
            if (m_lblColumn is not null)
            {
                Object.Destroy(m_lblColumn.gameObject);
            }
        }

        protected void OnMouseHover(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel txtLabel = (UILabel)component;
            if (txtLabel is not null)
            {
                txtLabel.textColor = Color.yellow;
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel txtLabel = (UILabel)component;
            if (txtLabel is not null)
            {
                txtLabel.textColor = Color.white;
            }
        }
    }
}
