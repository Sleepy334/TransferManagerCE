using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIIssueRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblPriority = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UITruncateLabel? m_lblSource = null;
        private UITruncateLabel? m_lblTarget = null;
        private UITruncateLabel? m_lblVehicle = null;

        private TransferIssueContainer? m_data = null;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width - ListView.iSCROLL_BAR_WIDTH;
            height = ListView.iROW_HEIGHT;
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
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = TransferIssuePanel.iCOLUMN_WIDTH_ISSUE;
                m_lblMaterial.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
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
                m_lblPriority.width = TransferIssuePanel.iCOLUMN_WIDTH_PRIORITY;
                m_lblPriority.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblPriority.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblTimer = AddUIComponent<UILabel>();
            if (m_lblTimer is not null)
            {
                m_lblTimer.name = "m_lblTimer";
                m_lblTimer.text = "";
                m_lblTimer.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTimer.tooltip = "";
                m_lblTimer.textAlignment = UIHorizontalAlignment.Center;
                m_lblTimer.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTimer.autoSize = false;
                m_lblTimer.height = height;
                m_lblTimer.width = TransferIssuePanel.iCOLUMN_WIDTH_VALUE;
                m_lblTimer.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTimer.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblValue = AddUIComponent<UILabel>();
            if (m_lblValue is not null)
            {
                m_lblValue.name = "m_lblValue";
                m_lblValue.text = "";
                m_lblValue.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblValue.tooltip = "";
                m_lblValue.textAlignment = UIHorizontalAlignment.Center;
                m_lblValue.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblValue.autoSize = false;
                m_lblValue.height = height;
                m_lblValue.width = TransferIssuePanel.iCOLUMN_WIDTH_VALUE;
                m_lblValue.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblValue.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblSource = AddUIComponent<UITruncateLabel>();
            if (m_lblSource is not null)
            {
                m_lblSource.name = "m_lblSource";
                m_lblSource.text = "";
                m_lblSource.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblSource.tooltip = "";
                m_lblSource.textAlignment = UIHorizontalAlignment.Left;
                m_lblSource.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblSource.autoSize = false;
                m_lblSource.height = height;
                m_lblSource.width = TransferIssuePanel.iCOLUMN_WIDTH_VEHICLE;
                m_lblSource.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblSource.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblSource.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblSource.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblVehicle = AddUIComponent<UITruncateLabel>();
            if (m_lblVehicle is not null)
            {
                m_lblVehicle.name = "m_lblVehicle";
                m_lblVehicle.text = "";
                m_lblVehicle.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblVehicle.tooltip = "";
                m_lblVehicle.textAlignment = UIHorizontalAlignment.Left;
                m_lblVehicle.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblVehicle.autoSize = false;
                m_lblVehicle.height = height;
                m_lblVehicle.width = TransferIssuePanel.iCOLUMN_WIDTH_VEHICLE;
                m_lblVehicle.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblVehicle.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblVehicle.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblVehicle.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblTarget = AddUIComponent<UITruncateLabel>();
            if (m_lblTarget is not null)
            {
                m_lblTarget.name = "m_lblTarget";
                m_lblTarget.text = "";
                m_lblTarget.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTarget.tooltip = "";
                m_lblTarget.textAlignment = UIHorizontalAlignment.Left;
                m_lblTarget.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTarget.autoSize = false;
                m_lblTarget.height = height;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);

                // Adjust last column
                m_lblTarget.width = width - m_lblMaterial.width - m_lblPriority.width - m_lblTimer.width - m_lblValue.width - m_lblSource.width - m_lblVehicle.width - 18;
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (TransferIssueContainer?) data;

            if (m_lblMaterial is null)
            {
                // Not yet initialised
                return;
            }

            if (m_data is not null)
            {
                m_lblMaterial.text = m_data.m_issue.ToString();
                m_lblPriority.text = m_data.GetPriority().ToString();
                m_lblValue.text = m_data.m_value.ToString();
                m_lblTimer.text = m_data.GetTimer();
                m_lblSource.text = m_data.GetSource();
                m_lblTarget.text = m_data.GetTarget();
                m_lblVehicle.text = m_data.GetVehicle();
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

            if (m_lblMaterial is not null)
            {
                m_lblMaterial.text = "";
                m_lblPriority.text = "";
                m_lblValue.text = "";
                m_lblTimer.text = "";
                m_lblSource.text = "";
                m_lblTarget.text = "";
                m_lblVehicle.text = "";

                tooltip = "";
                m_lblTimer.tooltip = "";
                m_lblPriority.tooltip = "";
                m_lblMaterial.tooltip = "";
                m_lblSource.tooltip = "";
                m_lblTarget.tooltip = "";
                m_lblVehicle.tooltip = "";
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
                if (component == m_lblSource)
                {
                    m_data.ShowSource();
                }
                else if (component == m_lblTarget)
                {
                    m_data.ShowTarget();
                }
                else if (component == m_lblVehicle)
                {
                    m_data.ShowVehicle();
                }
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_data is not null)
            {
                m_lblValue.tooltip = m_data.GetValueTooltip();
                m_lblSource.tooltip = m_data.GetSourceTooltip();
                m_lblTarget.tooltip = m_data.GetTargetTooltip();
                m_lblVehicle.tooltip = m_data.GetVehicleTooltip();
            }
            else
            {
                m_lblValue.tooltip = "";
                m_lblSource.tooltip = "";
                m_lblTarget.tooltip = "";
                m_lblVehicle.tooltip = "";
            }
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