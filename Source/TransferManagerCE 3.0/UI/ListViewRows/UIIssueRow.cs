using ColossalFramework.UI;
using SleepyCommon;

namespace TransferManagerCE.UI
{
    public class UIIssueRow : UIListRow<TransferIssueContainer>
    {

        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblPriority = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UITruncateLabel? m_lblSource = null;
        private UITruncateLabel? m_lblResponder = null;
        private UITruncateLabel? m_lblVehicle = null;

        public override void Start()
        {
            base.Start();

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
                m_lblVehicle.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblVehicle.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblResponder = AddUIComponent<UITruncateLabel>();
            if (m_lblResponder is not null)
            {
                m_lblResponder.name = "m_lblResponder";
                m_lblResponder.text = "";
                m_lblResponder.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblResponder.tooltip = "";
                m_lblResponder.textAlignment = UIHorizontalAlignment.Left;
                m_lblResponder.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblResponder.autoSize = false;
                m_lblResponder.height = height;
                m_lblResponder.width = 100; // This gets updated in AfterStart
                m_lblResponder.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblResponder.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            base.AfterStart();
        }

        protected override void Display()
        {
            if (data is not null)
            {
                m_lblMaterial.text = data.m_issue.ToString();
                m_lblPriority.text = data.GetPriority().ToString();
                m_lblValue.text = data.m_value.ToString();
                m_lblTimer.text = data.GetTimer();
                m_lblSource.text = data.GetSource();
                m_lblResponder.text = data.GetResponder();
                m_lblVehicle.text = data.GetVehicle();
            }
        }

        protected override void Clear()
        {
            m_lblMaterial.text = "";

            m_lblPriority.text = "";
            m_lblValue.text = "";
            m_lblTimer.text = "";

            m_lblSource.text = "";
            m_lblVehicle.text = "";
            m_lblResponder.text = "";
        }

        protected override void ClearTooltips()
        {
            tooltip = "";

            m_lblMaterial.tooltip = "";

            m_lblPriority.tooltip = "";
            m_lblValue.tooltip = "";
            m_lblTimer.tooltip = "";
            
            m_lblSource.tooltip = "";
            m_lblVehicle.tooltip = "";
            m_lblResponder.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (component == m_lblSource)
            {
                data.ShowSource();
            }
            else if (component == m_lblResponder)
            {
                data.ShowTarget();
            }
            else if (component == m_lblVehicle)
            {
                data.ShowVehicle();
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            if (component == this)
            {
                return "";
            }
            else if (component == m_lblValue)
            {
                return data.GetValueTooltip();
            }
            if (component == m_lblPriority)
            {
                return "";
            }
            else if (component == m_lblTimer)
            {
                return data.GetTimerTooltip();
            }
            else if (component == m_lblSource)
            {
                return data.GetSourceTooltip();
            }
            else if (component == m_lblVehicle)
            {
                return data.GetVehicleTooltip();
            }
            else if (component == m_lblResponder)
            {
                return data.GetResponderTooltip();
            }

            return "";
        }
    }
}