using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIMatchStatsRow : UIListRow<MatchStatsData>
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

        public static float[] ColumnWidths =
        {
            130, // Material
            80, // JobAvgTime
            80, // JobLastTime
            80, // JobMaxTime
            100, // Match Amount
            100, // Out Amount
            80, // In Amount
            80, // Distance
            80, // Match Outside
        };

        // ----------------------------------------------------------------------------------------
        public override void Start()
        {
            base.Start();

            fullRowSelect = true;

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
                m_lblMaterial.width = ColumnWidths[0];
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
                m_lblJobAvgTime.width = ColumnWidths[1];
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
                m_lblJobLastTime.width = ColumnWidths[2];
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
                m_lblJobMaxTime.width = ColumnWidths[3];
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
                m_lblMatchAmount.width = ColumnWidths[4];
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
                m_lblOutAmount.width = ColumnWidths[5];
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
                m_lblInAmount.width = ColumnWidths[6];
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
                m_lblMatchDistance.width = ColumnWidths[7];
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
                m_lblMatchOutside.width = ColumnWidths[8];
            }

            AfterStart();
        }

        protected override void Display()
        {
            if (!data.IsSeparator())
            {
                m_lblMaterial.text = data.GetMaterialDescription();

                m_lblJobAvgTime.text = Utils.DisplayTicks(data.GetJobAverageTicks());
                m_lblJobLastTime.text = Utils.DisplayTicks(data.JobTimeLastTicks);
                m_lblJobMaxTime.text = Utils.DisplayTicks(data.JobTimeMaxTicks);

                m_lblMatchAmount.text = data.TotalMatchAmount.ToString();
                m_lblOutAmount.text = data.TotalOutgoingAmount.ToString();
                m_lblInAmount.text = data.TotalIncomingAmount.ToString();
                m_lblMatchDistance.text = data.GetAverageDistance();
                m_lblMatchOutside.text = data.TotalOutside.ToString();
            }
            else
            {
                Clear();
            }
        }

        protected override void Clear()
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
        }

        protected override void ClearTooltips()
        {
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

        protected override void OnClicked(UIComponent component)
        {
        }

        protected override string GetTooltipText(UIComponent component)
        {
            return "";
        }

        protected override Color GetTextColor(UIComponent component, bool highlightRow)
        {
            if (highlightRow)
            {
                return KnownColor.yellow;
            }
            else if (data is not null)
            {
                return data.GetTextColor();
            }
            else
            {
                return KnownColor.white;
            }
        }
    }
}