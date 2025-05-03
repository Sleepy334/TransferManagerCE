using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIRoadAccessRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblId = null;
        private UITruncateLabel? m_lblOwner = null;
        private UILabel? m_lblSourceFailCount = null;

        private RoadAccessData? m_data = null;

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
                m_lblId.width = TransferIssuePanel.iCOLUMN_WIDTH_VALUE;
                m_lblId.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblId.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblSourceFailCount = AddUIComponent<UILabel>();
            if (m_lblSourceFailCount is not null)
            {
                m_lblSourceFailCount.name = "m_lblSourceFailCount";
                m_lblSourceFailCount.text = "";
                m_lblSourceFailCount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblSourceFailCount.tooltip = "";
                m_lblSourceFailCount.textAlignment = UIHorizontalAlignment.Center;
                m_lblSourceFailCount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblSourceFailCount.autoSize = false;
                m_lblSourceFailCount.height = height;
                m_lblSourceFailCount.width = TransferIssuePanel.iCOLUMN_WIDTH_PATH_FAIL;
                m_lblSourceFailCount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblSourceFailCount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOwner = AddUIComponent<UITruncateLabel>();
            if (m_lblOwner is not null)
            {
                m_lblOwner.name = "m_lblOwner";
                m_lblOwner.text = "";
                m_lblOwner.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOwner.tooltip = "";
                m_lblOwner.textAlignment = UIHorizontalAlignment.Left;
                m_lblOwner.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOwner.autoSize = false;
                m_lblOwner.height = height;
                m_lblOwner.width = TransferIssuePanel.iCOLUMN_WIDTH_DESCRIPTION;
                m_lblOwner.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwner.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (RoadAccessData?) data;

            if (m_lblOwner is null)
            {
                return;
            }

            if (m_data is not null)
            {
                m_lblId.text = m_data.m_source.Building.ToString();
                m_lblOwner.text = m_data.GetDescription();
                m_lblSourceFailCount.text = m_data.m_iCount.ToString();
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

            if (m_lblId is not null)
            {
                m_lblId.text = "";
                m_lblOwner.text = "";
                m_lblSourceFailCount.text = "";

                m_lblId.tooltip = "";
                m_lblOwner.tooltip = "";
                m_lblSourceFailCount.tooltip = "";
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
                if (component == m_lblOwner)
                {
                    InstanceHelper.ShowInstanceSetBuildingPanel(m_data.m_source);
                }
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
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