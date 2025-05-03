using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIMatchStatsRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblMatchAmount = null;
        private UILabel? m_lblOutAmount = null;
        private UILabel? m_lblInAmount = null;
        private UILabel? m_lblMatchDistance = null;
        private UILabel? m_lblMatchOutside = null;
        
        private UILabel? m_lblJobLastTime = null;
        private UILabel? m_lblJobMaxTime = null; 
        private UILabel? m_lblJobAvgTime = null;

        private MatchStatsData? m_data = null;

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

            m_lblJobAvgTime = AddUIComponent<UILabel>();
            if (m_lblJobAvgTime is not null)
            {
                m_lblJobAvgTime.name = "m_lblJobAvgTime";
                m_lblJobAvgTime.text = "";
                m_lblJobAvgTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblJobAvgTime.tooltip = "";
                m_lblJobAvgTime.textAlignment = UIHorizontalAlignment.Center;
                m_lblJobAvgTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblJobAvgTime.autoSize = false;
                m_lblJobAvgTime.height = height;
                m_lblJobAvgTime.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblJobAvgTime.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblJobAvgTime.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblJobLastTime = AddUIComponent<UILabel>();
            if (m_lblJobLastTime is not null)
            {
                m_lblJobLastTime.name = "m_lblJobLastTime";
                m_lblJobLastTime.text = "";
                m_lblJobLastTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblJobLastTime.tooltip = "";
                m_lblJobLastTime.textAlignment = UIHorizontalAlignment.Center;
                m_lblJobLastTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblJobLastTime.autoSize = false;
                m_lblJobLastTime.height = height;
                m_lblJobLastTime.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblJobLastTime.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblJobLastTime.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblJobMaxTime = AddUIComponent<UILabel>();
            if (m_lblJobMaxTime is not null)
            {
                m_lblJobMaxTime.name = "m_lblJobMaxTime";
                m_lblJobMaxTime.text = "";
                m_lblJobMaxTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblJobMaxTime.tooltip = "";
                m_lblJobMaxTime.textAlignment = UIHorizontalAlignment.Center;
                m_lblJobMaxTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblJobMaxTime.autoSize = false;
                m_lblJobMaxTime.height = height;
                m_lblJobMaxTime.width = StatsPanel.iCOLUMN_WIDTH;
                m_lblJobMaxTime.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblJobMaxTime.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblMatchAmount = AddUIComponent<UILabel>();
            if (m_lblMatchAmount is not null)
            {
                m_lblMatchAmount.name = "m_lblMatchAmount";
                m_lblMatchAmount.text = "";
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

            m_lblOutAmount = AddUIComponent<UILabel>();
            if (m_lblOutAmount is not null)
            {
                m_lblOutAmount.name = "m_lblOutAmount";
                m_lblOutAmount.text = "";
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

            m_lblInAmount = AddUIComponent<UILabel>();
            if (m_lblInAmount is not null)
            {
                m_lblInAmount.name = "m_lblInAmount";
                m_lblInAmount.text = "";
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

            m_lblMatchDistance = AddUIComponent<UILabel>();
            if (m_lblMatchDistance is not null)
            {
                m_lblMatchDistance.name = "m_lblMatchDistance";
                m_lblMatchDistance.text = "";
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
                m_lblMatchOutside.text = "";
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

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (MatchStatsData?) data;

            if (m_lblMaterial is null)
            {
                return;
            }

            if (m_data is not null)
            {
                if (!m_data.IsSeparator())
                {
                    m_lblMaterial.text = m_data.GetMaterialDescription();

                    m_lblJobAvgTime.text = Utils.DisplayTicks(m_data.GetJobAverageTicks());
                    m_lblJobLastTime.text = Utils.DisplayTicks(m_data.JobTimeLastTicks);
                    m_lblJobMaxTime.text = Utils.DisplayTicks(m_data.JobTimeMaxTicks);
                    
                    m_lblMatchAmount.text = m_data.TotalMatchAmount.ToString();
                    m_lblOutAmount.text = m_data.TotalOutgoingAmount.ToString();
                    m_lblInAmount.text = m_data.TotalIncomingAmount.ToString();
                    m_lblMatchDistance.text = m_data.GetAverageDistance();
                    m_lblMatchOutside.text = m_data.TotalOutside.ToString();
                }
                else
                {
                    Clear();
                }

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

        public void Clear()
        {
            m_data = null;

            if (m_lblMaterial is not null)
            {
                m_lblMaterial.text = "";
                m_lblJobLastTime.text = "";
                m_lblJobMaxTime.text = "";
                m_lblJobAvgTime.text = "";
                m_lblOutAmount.text = "";
                m_lblInAmount.text = "";
                m_lblMatchAmount.text = "";
                m_lblMatchDistance.text = "";
                m_lblMatchOutside.text = "";

                m_lblMaterial.tooltip = "";
                m_lblJobLastTime.tooltip = "";
                m_lblJobMaxTime.tooltip = "";
                m_lblJobAvgTime.tooltip = "";
                m_lblOutAmount.tooltip = "";
                m_lblInAmount.tooltip = "";
                m_lblMatchAmount.tooltip = "";
                m_lblMatchDistance.tooltip = "";
                m_lblMatchOutside.tooltip = "";
            }
        }

        public void Disabled()
        {
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

        public virtual Color GetTextColor(UIComponent component)
        {
            if (m_data is not null)
            {
                return m_data.GetTextColor();
            }
            else
            {
                return Color.white;
            }
        }
    }
}