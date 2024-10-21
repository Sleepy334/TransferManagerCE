using ColossalFramework.UI;
using System;
using TransferManagerCE;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class ListViewHeaderColumnIcon : ListViewHeaderColumnBase
    {
        private UIPanel? m_pnlIcon = null;
        private UILabel? m_lblIcon = null;
        private UILabel? m_lblIconSort = null;

        public ListViewHeaderColumnIcon(ListViewRowComparer.Columns eColumn, UIComponent parent, string sText, string sTooltip, int iWidth, int iHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, OnListViewColumnClick eventCallback) :
                base(eColumn, sText, eventCallback)
        {
            m_eColumn = eColumn;
            m_eventClickCallback = eventCallback;
            m_sText = sText;

            m_pnlIcon = parent.AddUIComponent<UIPanel>();
            m_pnlIcon.name = eColumn.ToString() + "Panel"; ;
            m_pnlIcon.autoSize = false;
            m_pnlIcon.tooltip = sTooltip;
            //m_pnlVehicles.backgroundSprite = "InfoviewPanel";
            //m_pnlVehicles.color = Color.red;
            m_pnlIcon.height = BuildingPanel.iHEADER_HEIGHT;
            m_pnlIcon.width = 100;
            m_pnlIcon.autoLayoutDirection = LayoutDirection.Horizontal;
            m_pnlIcon.autoLayout = true;
            m_pnlIcon.eventMouseEnter += OnMouseHover;
            m_pnlIcon.eventMouseLeave += OnMouseLeave;
            m_pnlIcon.eventClick += new MouseEventHandler(OnItemClicked);

            m_lblIconSort = m_pnlIcon.AddUIComponent<UILabel>();
            m_lblIconSort.name = eColumn.ToString() + "Sort";
            m_lblIconSort.text = "";
            //m_lblVehiclesSort.backgroundSprite = "InfoviewPanel";
            //m_lblVehiclesSort.color = Color.green;
            m_lblIconSort.textAlignment = UIHorizontalAlignment.Center;
            m_lblIconSort.autoSize = false;
            m_lblIconSort.height = BuildingPanel.iHEADER_HEIGHT;
            m_lblIconSort.width = 0;// This gets resized when sorting is enabled.
            m_lblIconSort.AlignTo(m_pnlIcon, UIAlignAnchor.TopLeft);
            m_lblIconSort.eventMouseEnter += OnMouseHover;
            m_lblIconSort.eventMouseLeave += OnMouseLeave;

            m_lblIcon = m_pnlIcon.AddUIComponent<UILabel>();
            m_lblIcon.name = eColumn.ToString() + "Icon";
            m_lblIcon.text = "";
            //m_lblVehicles.backgroundSprite = "InfoviewPanel";
            //m_lblVehicles.color = Color.blue;
            m_lblIcon.backgroundSprite = sText;
            m_lblIcon.textAlignment = UIHorizontalAlignment.Center;
            m_lblIcon.autoSize = false;
            m_lblIcon.height = BuildingPanel.iHEADER_HEIGHT;
            m_lblIcon.width = 30;
            m_lblIcon.AlignTo(m_pnlIcon, UIAlignAnchor.TopRight);
            m_lblIcon.eventMouseEnter += OnMouseHover;
            m_lblIcon.eventMouseLeave += OnMouseLeave;
        }

        public override void Sort(ListViewRowComparer.Columns eColumn, bool bDescending)
        {

        }

        public override bool IsHit(UIComponent component)
        {
            return component == m_pnlIcon || component == m_lblIconSort || component == m_lblIcon;
        }

        protected void OnMouseHover(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lblIconSort is not null)
            {
                m_lblIconSort.textColor = Color.yellow;
            }
            if (m_lblIcon is not null)
            {
                m_lblIcon.backgroundSprite = m_sText + "Hovered";
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lblIconSort is not null)
            {
                m_lblIconSort.textColor = Color.white;
            }
            if (m_lblIcon is not null)
            {
                m_lblIcon.backgroundSprite = m_sText;
            }
        }

        new public void SetText(string sText)
        {
            // Do nothing we use an icon instead
            throw new Exception("No text to set for an icon column");
        }

        new public void SetTooltip(string sText)
        {
            if (m_pnlIcon is not null)
            {
                m_pnlIcon.tooltip = sText;
            }
        }

        private void HideTooltipBox()
        {
            if (m_pnlIcon is not null && m_pnlIcon.tooltipBox is not null)
            {
                m_pnlIcon.tooltipBox.isVisible = false;
            }
        }

        public override void Destroy()
        {
            HideTooltipBox();
            if (m_lblIconSort is not null)
            {
                UnityEngine.Object.Destroy(m_lblIconSort.gameObject);
            }
            if (m_lblIcon is not null)
            {
                UnityEngine.Object.Destroy(m_lblIcon.gameObject);
            }
            if (m_pnlIcon is not null)
            {
                UnityEngine.Object.Destroy(m_pnlIcon.gameObject);
            }
        }
    }
}
