using ColossalFramework.UI;
using SleepyCommon;

namespace TransferManagerCE.UI
{
    public class UIRoadAccessRow : UIListRow<RoadAccessData>
    {
        private UILabel? m_lblId = null;
        private UITruncateLabel? m_lblOwner = null;
        private UILabel? m_lblSourceFailCount = null;

        public static float[] ColumnWidths =
        {
            70, // Value
            120, // Fail Count
            400, // Description
        };

        // ----------------------------------------------------------------------------------------
        public override void Start()
        {
            base.Start();

            fullRowSelect = true;

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
                m_lblId.width = ColumnWidths[0];
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
                m_lblSourceFailCount.width = ColumnWidths[1];
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
                m_lblOwner.width = ColumnWidths[2];
            }

            base.AfterStart();
        }

        protected override void Display()
        {
            if (data is not null)
            {
                m_lblId.text = data.m_source.Building.ToString();
                m_lblOwner.text = data.GetDescription();
                m_lblSourceFailCount.text = data.m_iCount.ToString();
            }
        }

        protected override void Clear()
        {
            m_lblOwner.text = "";
            m_lblSourceFailCount.text = "";
        }

        protected override void ClearTooltips()
        {
            tooltip = "";
            m_lblOwner.tooltip = "";
            m_lblSourceFailCount.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (data is not null)
            {
                BuildingUtils.ShowInstanceSetBuildingPanel(data.m_source);
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            if (data is not null)
            {
                return InstanceHelper.DescribeInstance(data.m_source, InstanceID.Empty, true);
            }

            return "";
        }
    }
}