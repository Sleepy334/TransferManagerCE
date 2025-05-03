using ColossalFramework.UI;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIPathRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblTime = null;
        private UILabel? m_lblLocation = null;
        private UILabel? m_lblOwner = null;
        private UILabel? m_lblSourceFailCount = null;
        private UILabel? m_lblTarget = null;
        private UILabel? m_lblTargetFailCount = null;

        private PathingContainer? m_data = null;

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

            m_lblTime = AddUIComponent<UILabel>();
            if (m_lblTime is not null)
            {
                m_lblTime.name = "m_lblTime";
                m_lblTime.text = "";
                m_lblTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTime.tooltip = "";
                m_lblTime.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTime.autoSize = false;
                m_lblTime.height = height;
                m_lblTime.width = TransferIssuePanel.iCOLUMN_WIDTH_TIME;
                m_lblTime.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTime.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblLocation = AddUIComponent<UILabel>();
            if (m_lblLocation is not null)
            {
                m_lblLocation.name = "m_lblLocation";
                m_lblLocation.text = "";
                m_lblLocation.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblLocation.tooltip = "";
                m_lblLocation.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblLocation.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblLocation.autoSize = false;
                m_lblLocation.height = height;
                m_lblLocation.width = TransferIssuePanel.iCOLUMN_WIDTH_LOCATION;
                m_lblLocation.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblLocation.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblLocation.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblLocation.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblOwner = AddUIComponent<UILabel>();
            if (m_lblOwner is not null)
            {
                m_lblOwner.name = "m_lblOwner";
                m_lblOwner.text = "";
                m_lblOwner.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOwner.tooltip = "";
                m_lblOwner.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblOwner.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOwner.autoSize = false;
                m_lblOwner.height = height;
                m_lblOwner.width = BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING;
                m_lblOwner.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwner.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblOwner.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblOwner.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                m_lblSourceFailCount.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblSourceFailCount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblSourceFailCount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblTarget = AddUIComponent<UILabel>();
            if (m_lblTarget is not null)
            {
                m_lblTarget.name = "m_lblActive";
                m_lblTarget.text = "";
                m_lblTarget.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTarget.tooltip = "";
                m_lblTarget.textAlignment = UIHorizontalAlignment.Left;
                m_lblTarget.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTarget.autoSize = false;
                m_lblTarget.height = height;
                m_lblTarget.width = BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblTargetFailCount = AddUIComponent<UILabel>();
            if (m_lblTargetFailCount is not null)
            {
                m_lblTargetFailCount.name = "m_lblAmount";
                m_lblTargetFailCount.text = "";
                m_lblTargetFailCount.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTargetFailCount.tooltip = "";
                m_lblTargetFailCount.textAlignment = UIHorizontalAlignment.Center;
                m_lblTargetFailCount.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTargetFailCount.autoSize = false;
                m_lblTargetFailCount.height = height;
                m_lblTargetFailCount.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblTargetFailCount.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTargetFailCount.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (PathingContainer?) data;

            if (m_lblTime is null)
            {
                return;
            }

            if (m_data is not null)
            {
                m_lblTime.text = m_data.GetTimeDescription();
                m_lblLocation.text = m_data.m_locationType.ToString();

                string sText = "";
                if (m_data.m_eSourceOrTarget == PathingContainer.SourceOrTarget.Source)
                {
                    sText += "* ";
                }
                m_lblOwner.text = sText + InstanceHelper.DescribeInstance(m_data.m_source);
                m_lblSourceFailCount.text = PathFindFailure.GetTotalPathFailures(m_data.m_source).ToString();

                sText = "";
                if (m_data.m_eSourceOrTarget == PathingContainer.SourceOrTarget.Target)
                {
                    sText += "* ";
                }
                m_lblTarget.text = sText + InstanceHelper.DescribeInstance(m_data.m_target);
                m_lblTargetFailCount.text = PathFindFailure.GetTotalPathFailures(m_data.m_target).ToString();
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

            if (m_lblTime is not null)
            {
                m_lblTime.text = "";
                m_lblLocation.text = "";
                m_lblOwner.text = "";
                m_lblSourceFailCount.text = "";
                m_lblTarget.text = "";
                m_lblTargetFailCount.text = "";

                m_lblTime.tooltip = "";
                m_lblLocation.tooltip = "";
                m_lblOwner.tooltip = "";
                m_lblSourceFailCount.tooltip = "";
                m_lblTarget.tooltip = "";
                m_lblTargetFailCount.tooltip = "";
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
                else if (component == m_lblTarget)
                {
                    InstanceHelper.ShowInstanceSetBuildingPanel(m_data.m_target);
                }
            }

        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
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