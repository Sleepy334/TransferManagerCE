using ColossalFramework.UI;
using SleepyCommon;

namespace TransferManagerCE.UI
{
    public class UIMatchRow : UIListRow<BuildingMatchData>
    {
        private UILabel? m_lblTime = null;
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblInOut = null;
        private UILabel? m_lblActive = null;
        private UILabel? m_lblAmount = null;
        private UILabel? m_lblDistance = null;
        private UILabel? m_lblPriority = null;
        private UILabel? m_lblPark = null;
        private UITruncateLabel? m_lblDescription = null;

        public static float[] ColumnWidths =
        {
            60, // Time
            120, // Material
            60, // In/Out
            60, // Active
            60, // Amount
            60, // Distance
            40, // Priority
            60, // Park
            250, // Description
        };

        // ----------------------------------------------------------------------------------------
        public override void Start()
        {
            base.Start();

            fullRowSelect = true;

            m_lblTime = AddUIComponent<UILabel>();
            if (m_lblTime is not null)
            {
                m_lblTime.name = "m_lblTime";
                m_lblTime.text = "";
                m_lblTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTime.tooltip = "";
                m_lblTime.textAlignment = UIHorizontalAlignment.Left;
                m_lblTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTime.autoSize = false;
                m_lblTime.height = height;
                m_lblTime.width = ColumnWidths[0];
            }

            m_lblMaterial = AddUIComponent<UILabel>();
            if (m_lblMaterial is not null)
            {
                m_lblMaterial.name = "lblMaterial";
                m_lblMaterial.text = "";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = ColumnWidths[1];
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
                m_lblInOut.width = ColumnWidths[2];
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
                m_lblActive.width = ColumnWidths[3];
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
                m_lblAmount.width = ColumnWidths[4];
            }

            m_lblDistance = AddUIComponent<UILabel>();
            if (m_lblDistance is not null)
            {
                m_lblDistance.name = "m_lblDistance";
                m_lblDistance.text = "";
                m_lblDistance.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDistance.tooltip = "";
                m_lblDistance.textAlignment = UIHorizontalAlignment.Center;
                m_lblDistance.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDistance.autoSize = false;
                m_lblDistance.height = height;
                m_lblDistance.width = ColumnWidths[5];
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
                m_lblPriority.width = ColumnWidths[6];
            }

            m_lblPark = AddUIComponent<UILabel>();
            if (m_lblPriority is not null)
            {
                m_lblPark.name = "m_lblPark";
                m_lblPark.text = "";
                m_lblPark.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblPark.tooltip = "";
                m_lblPark.textAlignment = UIHorizontalAlignment.Center;
                m_lblPark.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblPark.autoSize = false;
                m_lblPark.height = height;
                m_lblPark.width = ColumnWidths[7];
            }

            m_lblDescription = AddUIComponent<UITruncateLabel>();
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
                m_lblDescription.width = ColumnWidths[8];
            }

            AfterStart();
        }

        protected override void Display()
        {
            m_lblTime.text = data.Time();
            m_lblMaterial.text = data.m_material.ToString();
            m_lblInOut.text = data.GetInOutStatus();
            m_lblActive.text = data.GetActiveStatus();
            m_lblAmount.text = data.GetAmount();
            m_lblDistance.text = data.GetDistance().ToString("0.00");
            m_lblPriority.text = data.GetPriority();
            m_lblPark.text = data.GetParkId();
            m_lblDescription.text = data.DisplayMatch();
        }

        protected override void Clear()
        {
            m_lblTime.text = "";
            m_lblMaterial.text = "";
            m_lblInOut.text = "";
            m_lblActive.text = "";
            m_lblAmount.text = "";
            m_lblDistance.text = "";
            m_lblPriority.text = "";
            m_lblPark.text = "";
            m_lblDescription.text = "";
        }

        protected override void ClearTooltips()
        {
            m_lblTime.tooltip = "";
            m_lblMaterial.tooltip = "";
            m_lblInOut.tooltip = "";
            m_lblActive.tooltip = "";
            m_lblAmount.tooltip = "";
            m_lblDistance.tooltip = "";
            m_lblPriority.tooltip = "";
            m_lblPark.tooltip = "";
            m_lblDescription.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (data is not null)
            {
                data.Show();
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            return data.GetTooltipText();
        }
    }
}