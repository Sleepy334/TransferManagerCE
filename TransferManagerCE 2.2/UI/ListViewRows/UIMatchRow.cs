using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIMatchRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblTime = null;
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblInOut = null;
        private UILabel? m_lblActive = null;
        private UILabel? m_lblAmount = null;
        private UILabel? m_lblDistance = null;
        private UILabel? m_lblPriority = null;
        private UILabel? m_lblPark = null;
        private UILabel? m_lblDescription = null;

        private BuildingMatchData? m_data = null;

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
            eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            eventMouseLeave += new MouseEventHandler(OnMouseLeave);

            m_lblTime = AddUIComponent<UILabel>();
            if (m_lblTime != null)
            {
                m_lblTime.name = "m_lblTime";
                m_lblTime.text = "";
                m_lblTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTime.tooltip = "";
                m_lblTime.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTime.autoSize = false;
                m_lblTime.height = height;
                m_lblTime.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblTime.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTime.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblMaterial = AddUIComponent<UILabel>();
            if (m_lblMaterial != null)
            {
                m_lblMaterial.name = "lblMaterial";
                m_lblMaterial.text = "";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = BuildingPanel.iCOLUMN_WIDTH_LARGE;
                m_lblMaterial.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblInOut = AddUIComponent<UILabel>();
            if (m_lblInOut != null)
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

            m_lblActive = AddUIComponent<UILabel>();
            if (m_lblActive != null)
            {
                m_lblActive.name = "m_lblActive";
                m_lblActive.text = "";
                m_lblActive.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblActive.tooltip = "";
                m_lblActive.textAlignment = UIHorizontalAlignment.Center;
                m_lblActive.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblActive.autoSize = false;
                m_lblActive.height = height;
                m_lblActive.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblActive.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblActive.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblAmount = AddUIComponent<UILabel>();
            if (m_lblAmount != null)
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

            m_lblDistance = AddUIComponent<UILabel>();
            if (m_lblDistance != null)
            {
                m_lblDistance.name = "m_lblDistance";
                m_lblDistance.text = "";
                m_lblDistance.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDistance.tooltip = "";
                m_lblDistance.textAlignment = UIHorizontalAlignment.Center;
                m_lblDistance.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDistance.autoSize = false;
                m_lblDistance.height = height;
                m_lblDistance.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblDistance.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblDistance.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblPriority = AddUIComponent<UILabel>();
            if (m_lblPriority != null)
            {
                m_lblPriority.name = "m_lblPriority";
                m_lblPriority.text = "";
                m_lblPriority.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblPriority.tooltip = "";
                m_lblPriority.textAlignment = UIHorizontalAlignment.Center;
                m_lblPriority.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblPriority.autoSize = false;
                m_lblPriority.height = height;
                m_lblPriority.width = BuildingPanel.iCOLUMN_WIDTH_TINY;
                m_lblPriority.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblPriority.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblPark = AddUIComponent<UILabel>();
            if (m_lblPriority != null)
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
            if (m_lblDescription != null)
            {
                m_lblDescription.name = "m_lblDescription";
                m_lblDescription.text = "";
                m_lblDescription.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription.tooltip = "";
                m_lblDescription.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription.autoSize = false;
                m_lblDescription.height = height;
                m_lblDescription.width = BuildingPanel.iCOLUMN_WIDTH_250;
                m_lblDescription.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblDescription.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            if (m_data != null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            BuildingMatchData? rowData = (BuildingMatchData?)data;
            if (rowData != null)
            {
                m_data = rowData;
                if (m_lblTime != null)
                {
                    m_lblTime.text = rowData.Time();
                }
                if (m_lblMaterial != null)
                {
                    m_lblMaterial.text = rowData.m_material.ToString();
                }
                if (m_lblInOut != null)
                {
                    m_lblInOut.text = rowData.GetInOutStatus();
                }
                if (m_lblActive != null)
                {
                    m_lblActive.text = rowData.GetActiveStatus();
                }
                if (m_lblAmount != null)
                {
                    m_lblAmount.text = rowData.GetAmount();
                }
                if (m_lblDistance != null)
                {
                    m_lblDistance.text = rowData.GetDistance().ToString("0.00");
                }
                if (m_lblPriority != null)
                {
                    m_lblPriority.text = rowData.GetPriority();
                }
                if (m_lblPark != null)
                {
                    m_lblPark.text = rowData.GetPark();
                }
                if (m_lblDescription != null)
                {
                    m_lblDescription.text = rowData.DisplayMatch();
                }
            }
            else
            {
                m_data = null;
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
            if (m_data != null)
            {
                m_data.Show();
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UILabel? label in components)
            {
                if (label != null)
                {
                    label.textColor = Color.yellow;
                }
            }

        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UILabel? label in components)
            {
                if (label != null)
                {
                    label.textColor = Color.white;
                }
            }
        }
    }
}