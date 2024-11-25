using ColossalFramework.UI;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace TransferManagerCE.UI
{
    public class UIOfferRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblInOut = null;
        private UILabel? m_lblPriority = null;
        private UILabel? m_lblActive = null;
        private UILabel? m_lblAmount = null;
        private UILabel? m_lblPark = null;
        private UILabel? m_lblDescription = null;

        private OfferData? m_data = null;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
            height = ListView.iROW_HEIGHT;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;
            eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            eventMouseLeave += new MouseEventHandler(OnMouseLeave);

            m_lblMaterial = AddUIComponent<UILabel>();
            if (m_lblMaterial is not null)
            {
                m_lblMaterial.name = "lblMaterial";
                m_lblMaterial.text = "";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = BuildingPanel.iCOLUMN_WIDTH_LARGE;
                m_lblMaterial.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblInOut = AddUIComponent<UILabel>();
            if (m_lblInOut is not null)
            {
                m_lblInOut.name = "m_lblInOut";
                m_lblInOut.text = "";
                m_lblInOut.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblInOut.tooltip = "";
                m_lblInOut.textAlignment = UIHorizontalAlignment.Center;
                m_lblInOut.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblInOut.autoSize = false;
                m_lblInOut.height = height;
                m_lblInOut.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblInOut.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblInOut.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblPriority = AddUIComponent<UILabel>();
            if (m_lblPriority is not null)
            {
                m_lblPriority.name = "m_lblPriority";
                m_lblPriority.text = "";
                m_lblPriority.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblPriority.tooltip = "";
                m_lblPriority.textAlignment = UIHorizontalAlignment.Center;
                m_lblPriority.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblPriority.autoSize = false;
                m_lblPriority.height = height;
                m_lblPriority.width = BuildingPanel.iCOLUMN_WIDTH_NORMAL;
                m_lblPriority.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblPriority.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblActive = AddUIComponent<UILabel>();
            if (m_lblActive is not null)
            {
                m_lblActive.name = "m_lblActive";
                m_lblActive.text = "";
                m_lblActive.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblActive.tooltip = "";
                m_lblActive.textAlignment = UIHorizontalAlignment.Center;
                m_lblActive.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblActive.autoSize = false;
                m_lblActive.height = height;
                m_lblActive.width = BuildingPanel.iCOLUMN_WIDTH_NORMAL;
                m_lblActive.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblActive.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblAmount = AddUIComponent<UILabel>();
            if (m_lblAmount is not null)
            {
                m_lblAmount.name = "m_lblAmount";
                m_lblAmount.text = "";
                m_lblAmount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblAmount.tooltip = "";
                m_lblAmount.textAlignment = UIHorizontalAlignment.Center;
                m_lblAmount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblAmount.autoSize = false;
                m_lblAmount.height = height;
                m_lblAmount.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblAmount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblAmount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblPark = AddUIComponent<UILabel>();
            if (m_lblPriority is not null)
            {
                m_lblPark.name = "m_lblPriority";
                m_lblPark.text = "";
                m_lblPark.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblPark.tooltip = "";
                m_lblPark.textAlignment = UIHorizontalAlignment.Center;
                m_lblPark.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblPark.autoSize = false;
                m_lblPark.height = height;
                m_lblPark.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblPark.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblPark.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

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
                m_lblDescription.width = BuildingPanel.iCOLUMN_WIDTH_300;
                m_lblDescription.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblDescription.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            if (m_data is not null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            m_data = (OfferData?)data;

            if (m_data is not null)
            {
                if (m_lblMaterial is not null)
                {
                    m_lblMaterial.text = m_data.m_material.ToString();
                }
                if (m_lblInOut is not null)
                {
                    m_lblInOut.text = m_data.m_bIncoming ? "IN" : "OUT";
                }
                if (m_lblActive is not null)
                {
                    m_lblActive.text = m_data.m_bActive ? "Active" : "Passive";
                }
                if (m_lblAmount is not null)
                {
                    m_lblAmount.text = m_data.m_offer.Amount.ToString();
                    if (m_data.m_offer.Unlimited)
                    {
                        m_lblAmount.text += "*";
                    }
                }
                if (m_lblPriority is not null)
                {
                    m_lblPriority.text = m_data.m_iPrioirty.ToString();
                }
                if (m_lblPark is not null)
                {
                    m_lblPark.text = m_data.m_byLocalPark.ToString();
                }
                if (m_lblDescription is not null)
                {
                    m_lblDescription.text = m_data.DisplayOffer();
                }
            }
        }

        public void Disabled()
        {
            if (m_data is not null)
            {
                m_data = null;

                m_lblMaterial.tooltip = "";
                m_lblInOut.tooltip = "";
                m_lblActive.tooltip = "";
                m_lblAmount.tooltip = "";
                m_lblPriority.tooltip = "";
                m_lblPark.tooltip = "";
                m_lblDescription.tooltip = "";
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
                m_data.Show();
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lblMaterial is null)
            {
                return;
            }

            if (enabled && m_data is not null)
            {
                m_lblMaterial.tooltip = m_data.m_material.ToString();
                m_lblDescription.tooltip = m_data.DisplayOffer();
            }
            else
            {
                m_lblMaterial.tooltip = "";
                m_lblDescription.tooltip = "";
            }
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UILabel? label in components)
            {
                if (label is not null)
                {
                    label.textColor = Color.yellow;
                }
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UILabel? label in components)
            {
                if (label is not null)
                {
                    label.textColor = Color.white;
                }
            }
        }
    }
}