using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.Util;

namespace TransferManagerCE.UI
{
    public class UIPathRow : UIListRow<PathingContainer>
    {
        private UILabel? m_lblTime = null;
        private UILabel? m_lblLocation = null;
        private UITruncateLabel? m_lblOwner = null;
        private UILabel? m_lblSourceFailCount = null;
        private UITruncateLabel? m_lblTarget = null;
        private UILabel? m_lblTargetFailCount = null;

        public static float[] ColumnWidths =
        {
            70, // Time
            95, // Location
            240, // Source building
            60, // Fail count
            240, // Target building
            60, // Fail count
        };

        public override void Start()
        {
            base.Start();

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
                m_lblTime.width = ColumnWidths[0];
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
                m_lblLocation.width = ColumnWidths[1];
            }

            m_lblOwner = AddUIComponent<UITruncateLabel>();
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
                m_lblOwner.width = ColumnWidths[2];
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
                m_lblSourceFailCount.width = ColumnWidths[3];
            }

            m_lblTarget = AddUIComponent<UITruncateLabel>();
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
                m_lblTarget.width = ColumnWidths[4];
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
                m_lblTargetFailCount.width = ColumnWidths[5];
            }

            base.AfterStart();
        }

        protected override void Display()
        {
            if (data is not null)
            {
                m_lblTime.text = data.GetTimeDescription();
                m_lblLocation.text = data.m_locationType.ToString();

                string sText = "";
                if (data.m_eSourceOrTarget == PathingContainer.SourceOrTarget.Source)
                {
                    sText += "* ";
                }
                m_lblOwner.text = sText + InstanceHelper.DescribeInstance(data.m_source, InstanceID.Empty);
                m_lblSourceFailCount.text = PathFindFailure.GetTotalPathFailures(data.m_source).ToString();

                sText = "";
                if (data.m_eSourceOrTarget == PathingContainer.SourceOrTarget.Target)
                {
                    sText += "* ";
                }
                m_lblTarget.text = sText + InstanceHelper.DescribeInstance(data.m_target, InstanceID.Empty);
                m_lblTargetFailCount.text = PathFindFailure.GetTotalPathFailures(data.m_target).ToString();
            }
        }

        protected override void Clear()
        {
            m_lblTime.text = "";
            m_lblLocation.text = "";
            m_lblOwner.text = "";
            m_lblSourceFailCount.text = "";
            m_lblTarget.text = "";
            m_lblTargetFailCount.text = "";
        }

        protected override void ClearTooltips()
        {
            tooltip = "";

            m_lblTime.tooltip = "";
            m_lblLocation.tooltip = "";
            m_lblOwner.tooltip = "";
            m_lblSourceFailCount.tooltip = "";
            m_lblTarget.tooltip = "";
            m_lblTargetFailCount.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (component == m_lblOwner)
            {
                BuildingUtils.ShowInstanceSetBuildingPanel(data.m_source);
            }
            else if (component == m_lblTarget)
            {
                BuildingUtils.ShowInstanceSetBuildingPanel(data.m_target);
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            if (component == this)
            {
                return "";
            }
            else if (component == m_lblTime)
            {
                return data.GetTimeDescription();
            }
            if (component == m_lblLocation)
            {
                return data.m_locationType.ToString();
            }
            else if (component == m_lblOwner)
            {
                return InstanceHelper.DescribeInstance(data.m_source, InstanceID.Empty, true);
            }
            else if (component == m_lblTarget)
            {
                return InstanceHelper.DescribeInstance(data.m_target, InstanceID.Empty, true);
            }

            return "";
        }
    }
}