using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SleepyCommon
{
    public class ListViewRowColumn
    {
        private UILabel? m_lblColumn = null;
        private OnGetColumnTooltip? m_GetColumnTooltip = null;
        private ListViewRowComparer.Columns m_eColumn;
        private OnColumnClick? m_columnClickCallback = null;

        public delegate void OnColumnClick(ListViewRowColumn oColumn);

        public delegate string OnGetColumnTooltip(ListViewRowColumn oColumn);

        public static ListViewRowColumn Create(ListViewRowComparer.Columns eColumn, UIComponent parent, string sText, string sTooltip, float fTextScale, int iWidth, int iHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, OnColumnClick eventClickCallback, OnGetColumnTooltip getColumnTooltip)
        {
            ListViewRowColumn oColumn = new ListViewRowColumn();
            oColumn.Setup(eColumn, parent, sText, sTooltip, fTextScale, iWidth, iHeight, oTextAlignment, oAncor, eventClickCallback, getColumnTooltip);
            return oColumn;
        }

        private void Setup(ListViewRowComparer.Columns eColumn, UIComponent parent, string sText, string sTooltip, float fTextScale, int iWidth, int iHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, OnColumnClick eventClickCallback, OnGetColumnTooltip getColumnTooltip)
        {
            m_eColumn = eColumn;
            m_columnClickCallback = eventClickCallback;
            m_GetColumnTooltip = getColumnTooltip;
            m_lblColumn = parent.AddUIComponent<UILabel>();
            m_lblColumn.backgroundSprite = 
            m_lblColumn.name = eColumn.ToString();
            m_lblColumn.text = sText;
            m_lblColumn.textScale = fTextScale;
            m_lblColumn.tooltip = sTooltip;
            m_lblColumn.textAlignment = oTextAlignment;// UIHorizontalAlignment.Center;
            m_lblColumn.verticalAlignment = UIVerticalAlignment.Middle;
            m_lblColumn.autoSize = false;
            m_lblColumn.height = iHeight;
            m_lblColumn.width = iWidth;
            m_lblColumn.AlignTo(parent, oAncor);
            if (OnItemClicked != null)
            {
                m_lblColumn.eventClick += new MouseEventHandler(OnItemClicked);
                m_lblColumn.eventMouseEnter += OnMouseHover;
                m_lblColumn.eventMouseLeave += OnMouseLeave;
            }
            
            m_lblColumn.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
        }

        public ListViewRowComparer.Columns GetColumn()
        {
            return m_eColumn;
        }

        public UILabel? GetLabel()
        {
            return m_lblColumn;
        }

        public void SetText(string sText)
        {
            if (m_lblColumn != null)
            {
                m_lblColumn.text = sText;
            }
        }

        public void HideTooltipBox()
        {
            if (m_lblColumn != null && m_lblColumn.tooltipBox != null)
            {
                m_lblColumn.tooltipBox.isVisible = false;
            }
        }

        private void OnItemClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_columnClickCallback != null)
            {
                m_columnClickCallback(this);
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lblColumn != null && m_GetColumnTooltip != null)
            {
                string sTooltip = m_GetColumnTooltip(this);
                m_lblColumn.tooltip = sTooltip;
            }
        }

        public void Destroy()
        {
            HideTooltipBox();
            if (m_lblColumn != null)
            {
                UnityEngine.Object.Destroy(m_lblColumn.gameObject);
            }
        }

        protected void OnMouseHover(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel txtLabel = (UILabel)component;
            if (txtLabel != null)
            {
                txtLabel.textColor = Color.yellow;
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel txtLabel = (UILabel)component;
            if (txtLabel != null)
            {
                txtLabel.textColor = Color.white;
            }
        }
    }
}
