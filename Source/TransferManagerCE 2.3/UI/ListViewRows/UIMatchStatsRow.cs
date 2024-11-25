using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIMatchStatsRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblOutCount = null;
        private UILabel? m_lblOutAmount = null;
        private UILabel? m_lblInCount = null;
        private UILabel? m_lblInAmount = null;
        private UILabel? m_lblMatchCount = null;
        private UILabel? m_lblMatchAmount = null;
        private UILabel? m_lblMatchDistance = null;
        private UILabel? m_lblMatchOutside = null;
        private UILabel? m_lblOutPercent = null;
        private UILabel? m_lblInPercent = null;

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

            m_lblMaterial = AddUIComponent<UILabel>();
            if (m_lblMaterial is not null)
            {
                m_lblMaterial.name = "m_lblMaterial";
                m_lblMaterial.text = "";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = StatsPanel.iCOLUMN_MATERIAL_WIDTH;
                m_lblMaterial.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOutCount = AddUIComponent<UILabel>();
            if (m_lblOutCount is not null)
            {
                m_lblOutCount.name = "m_lblOutCount";
                m_lblOutCount.text = "0";
                m_lblOutCount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOutCount.tooltip = "";
                m_lblOutCount.textAlignment = UIHorizontalAlignment.Center;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblOutCount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOutCount.autoSize = false;
                m_lblOutCount.height = height;
                m_lblOutCount.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblOutCount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOutCount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOutAmount = AddUIComponent<UILabel>();
            if (m_lblOutAmount is not null)
            {
                m_lblOutAmount.name = "m_lblOutAmount";
                m_lblOutAmount.text = "m_lblOutAmount";
                m_lblOutAmount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOutAmount.tooltip = "";
                m_lblOutAmount.textAlignment = UIHorizontalAlignment.Center;
                m_lblOutAmount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOutAmount.autoSize = false;
                m_lblOutAmount.height = height;
                m_lblOutAmount.width = StatsPanel.iCOLUMN_BIGGER_WIDTH;
                m_lblOutAmount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOutAmount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblInCount = AddUIComponent<UILabel>();
            if (m_lblInCount is not null)
            {
                m_lblInCount.name = "m_lblInCount";
                m_lblInCount.text = "m_lblInCount";
                m_lblInCount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblInCount.tooltip = "";
                m_lblInCount.textAlignment = UIHorizontalAlignment.Center;
                m_lblInCount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblInCount.autoSize = false;
                m_lblInCount.height = height;
                m_lblInCount.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblInCount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblInCount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblInAmount = AddUIComponent<UILabel>();
            if (m_lblInAmount is not null)
            {
                m_lblInAmount.name = "m_lblInAmount";
                m_lblInAmount.text = "m_lblInAmount";
                m_lblInAmount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblInAmount.tooltip = "";
                m_lblInAmount.textAlignment = UIHorizontalAlignment.Center;
                m_lblInAmount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblInAmount.autoSize = false;
                m_lblInAmount.height = height;
                m_lblInAmount.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblInAmount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblInAmount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }


            m_lblMatchCount = AddUIComponent<UILabel>();
            if (m_lblMatchCount is not null)
            {
                m_lblMatchCount.name = "m_lblMatchCount";
                m_lblMatchCount.text = "m_lblMatchCount";
                m_lblMatchCount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMatchCount.tooltip = "";
                m_lblMatchCount.textAlignment = UIHorizontalAlignment.Center;
                m_lblMatchCount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMatchCount.autoSize = false;
                m_lblMatchCount.height = height;
                m_lblMatchCount.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblMatchCount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMatchCount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblMatchAmount = AddUIComponent<UILabel>();
            if (m_lblMatchAmount is not null)
            {
                m_lblMatchAmount.name = "m_lblMatchAmount";
                m_lblMatchAmount.text = "m_lblMatchAmount";
                m_lblMatchAmount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMatchAmount.tooltip = "";
                m_lblMatchAmount.textAlignment = UIHorizontalAlignment.Center;
                m_lblMatchAmount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMatchAmount.autoSize = false;
                m_lblMatchAmount.height = height;
                m_lblMatchAmount.width = StatsPanel.iCOLUMN_BIGGER_WIDTH;
                m_lblMatchAmount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMatchAmount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblMatchDistance = AddUIComponent<UILabel>();
            if (m_lblMatchDistance is not null)
            {
                m_lblMatchDistance.name = "m_lblMatchDistance";
                m_lblMatchDistance.text = "m_lblMatchDistance";
                m_lblMatchDistance.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMatchDistance.tooltip = "";
                m_lblMatchDistance.textAlignment = UIHorizontalAlignment.Center;
                m_lblMatchDistance.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMatchDistance.autoSize = false;
                m_lblMatchDistance.height = height;
                m_lblMatchDistance.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblMatchDistance.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMatchDistance.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblMatchOutside = AddUIComponent<UILabel>();
            if (m_lblMatchOutside is not null)
            {
                m_lblMatchOutside.name = "m_lblMatchOutside";
                m_lblMatchOutside.text = "m_lblMatchOutside";
                m_lblMatchOutside.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMatchOutside.tooltip = "";
                m_lblMatchOutside.textAlignment = UIHorizontalAlignment.Center;
                m_lblMatchOutside.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMatchOutside.autoSize = false;
                m_lblMatchOutside.height = height;
                m_lblMatchOutside.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblMatchOutside.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMatchOutside.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOutPercent = AddUIComponent<UILabel>();
            if (m_lblOutPercent is not null)
            {
                m_lblOutPercent.name = "m_lblOutPercent";
                m_lblOutPercent.text = "m_lblOutPercent";
                m_lblOutPercent.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOutPercent.tooltip = "";
                m_lblOutPercent.textAlignment = UIHorizontalAlignment.Center;
                m_lblOutPercent.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOutPercent.autoSize = false;
                m_lblOutPercent.height = height;
                m_lblOutPercent.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblOutPercent.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOutPercent.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblInPercent = AddUIComponent<UILabel>();
            if (m_lblInPercent is not null)
            {
                m_lblInPercent.name = "m_lblInPercent";
                m_lblInPercent.text = "m_lblInPercent";
                m_lblInPercent.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblInPercent.tooltip = "";
                m_lblInPercent.textAlignment = UIHorizontalAlignment.Center;
                m_lblInPercent.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblInPercent.autoSize = false;
                m_lblInPercent.height = height;
                m_lblInPercent.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblInPercent.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblInPercent.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            StatsContainer? rowData = (StatsContainer?)data;
            if (rowData is not null)
            {
                if (m_lblMaterial is not null)
                {
                    m_lblMaterial.text = rowData.GetMaterialDescription();
                }
                if (m_lblOutCount is not null)
                {
                    m_lblOutCount.text = rowData.TotalOutgoingCount.ToString();
                }
                if (m_lblOutAmount is not null)
                {
                    m_lblOutAmount.text = rowData.TotalOutgoingAmount.ToString();
                }
                if (m_lblInCount is not null)
                {
                    m_lblInCount.text = rowData.TotalIncomingCount.ToString();
                }
                if (m_lblInAmount is not null)
                {
                    m_lblInAmount.text = rowData.TotalIncomingAmount.ToString();
                }
                if (m_lblMatchCount is not null)
                {
                    m_lblMatchCount.text = rowData.TotalMatches.ToString();
                }
                if (m_lblMatchAmount is not null)
                {
                    m_lblMatchAmount.text = rowData.TotalMatchAmount.ToString();
                }
                if (m_lblMatchDistance is not null)
                {
                    m_lblMatchDistance.text = rowData.GetAverageDistance();
                }
                if (m_lblMatchOutside is not null)
                {
                    m_lblMatchOutside.text = rowData.TotalOutside.ToString();
                }
                if (m_lblOutPercent is not null)
                {
                    m_lblOutPercent.text = (SafeDiv(rowData.TotalMatchAmount, rowData.TotalIncomingAmount) * 100f).ToString("0.00");
                }
                if (m_lblInPercent is not null)
                {
                    m_lblInPercent.text = (SafeDiv(rowData.TotalMatchAmount, rowData.TotalOutgoingAmount) * 100f).ToString("0.00");
                }
            }
        }

        public float SafeDiv(float fNumerator, float fDenominator)
        {
            if (fNumerator == 0 || fDenominator == 0)
            {
                return 0f;
            }
            else
            {
                return fNumerator / fDenominator;
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
            UILabel? txtLabel = component as UILabel;
            if (txtLabel is not null)
            {
                txtLabel.textColor = Color.yellow;
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel? txtLabel = component as UILabel;
            if (txtLabel is not null)
            {
                txtLabel.textColor = Color.white;
            }
        }
    }
}