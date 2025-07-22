using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIOutsideRow : UIListRow<OutsideContainer>
    {
        private UILabel? m_lblName = null;
        private UILabel? m_lblType = null;
        private UILabel? m_lblInOut = null;
        private UILabel? m_lblCargoPriority = null;
        private UILabel? m_lblCitizenPriority = null;
        private UILabel? m_lblUsage = null;
        private UILabel? m_lblOwn = null;
        private UILabel? m_lblStuck = null;
        private UILabel? m_lblGuest = null;

        public static float[] ColumnWidths =
        {
            200, // Name
            60, // Type
            60, // In / Out
            60, // Prioriity
            60, // Prioriity
            80, // Usage
            100, // Own
            100, // Guest
            100, // Stuck
        };

        // ----------------------------------------------------------------------------------------
        public override void Start()
        {
            base.Start();
            fullRowSelect = true;

            int iColumnIndex = 0;

            m_lblName = AddUIComponent<UILabel>();
            if (m_lblName is not null)
            {
                m_lblName.name = "m_lblName";
                m_lblName.text = "";
                m_lblName.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblName.tooltip = "";
                m_lblName.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblName.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblName.autoSize = false;
                m_lblName.height = height;
                m_lblName.width = ColumnWidths[iColumnIndex++];
            }

            m_lblType = AddUIComponent<UILabel>();
            if (m_lblType is not null)
            {
                m_lblType.name = "m_lblType";
                m_lblType.text = "";
                m_lblType.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblType.tooltip = "";
                m_lblType.textAlignment = UIHorizontalAlignment.Center;
                m_lblType.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblType.autoSize = false;
                m_lblType.height = height;
                m_lblType.width = ColumnWidths[iColumnIndex++];
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
                m_lblInOut.width = ColumnWidths[iColumnIndex++];
            }

            m_lblCargoPriority = AddUIComponent<UILabel>();
            if (m_lblCargoPriority is not null)
            {
                m_lblCargoPriority.name = "m_lblCargoPriority";
                m_lblCargoPriority.text = "";
                m_lblCargoPriority.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblCargoPriority.tooltip = "";
                m_lblCargoPriority.textAlignment = UIHorizontalAlignment.Center;
                m_lblCargoPriority.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblCargoPriority.autoSize = false;
                m_lblCargoPriority.height = height;
                m_lblCargoPriority.width = ColumnWidths[iColumnIndex++];
            }

            m_lblCitizenPriority = AddUIComponent<UILabel>();
            if (m_lblCitizenPriority is not null)
            {
                m_lblCitizenPriority.name = "m_lblCitizenPriority";
                m_lblCitizenPriority.text = "";
                m_lblCitizenPriority.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblCitizenPriority.tooltip = "";
                m_lblCitizenPriority.textAlignment = UIHorizontalAlignment.Center;
                m_lblCitizenPriority.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblCitizenPriority.autoSize = false;
                m_lblCitizenPriority.height = height;
                m_lblCitizenPriority.width = ColumnWidths[iColumnIndex++];
            }

            m_lblUsage = AddUIComponent<UILabel>();
            if (m_lblUsage is not null)
            {
                m_lblUsage.name = "m_lblUsage";
                m_lblUsage.text = "";
                m_lblUsage.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblUsage.tooltip = "";
                m_lblUsage.textAlignment = UIHorizontalAlignment.Center;
                m_lblUsage.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblUsage.autoSize = false;
                m_lblUsage.height = height;
                m_lblUsage.width = ColumnWidths[iColumnIndex++];
            }

            m_lblOwn = AddUIComponent<UILabel>();
            if (m_lblOwn is not null)
            {
                m_lblOwn.name = "m_lblOwn";
                m_lblOwn.text = "";
                m_lblOwn.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOwn.tooltip = "";
                m_lblOwn.textAlignment = UIHorizontalAlignment.Center;
                m_lblOwn.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOwn.autoSize = false;
                m_lblOwn.height = height;
                m_lblOwn.width = ColumnWidths[iColumnIndex++];
            }

            m_lblGuest = AddUIComponent<UILabel>();
            if (m_lblGuest is not null)
            {
                m_lblGuest.name = "m_lblGuest";
                m_lblGuest.text = "";
                m_lblGuest.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblGuest.tooltip = "";
                m_lblGuest.textAlignment = UIHorizontalAlignment.Center;
                m_lblGuest.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblGuest.autoSize = false;
                m_lblGuest.height = height;
                m_lblGuest.width = ColumnWidths[iColumnIndex++];
            }

            m_lblStuck = AddUIComponent<UILabel>();
            if (m_lblStuck is not null)
            {
                m_lblStuck.name = "m_lblStuck";
                m_lblStuck.text = "";
                m_lblStuck.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblStuck.tooltip = "";
                m_lblStuck.textAlignment = UIHorizontalAlignment.Center;
                m_lblStuck.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblStuck.autoSize = false;
                m_lblStuck.height = height;
                m_lblStuck.width = ColumnWidths[iColumnIndex++];
            }

            AfterStart();
        }

        protected override void Display()
        {
            m_lblName.text = data.GetName();
            m_lblType.text = data.m_eType.ToString();
            m_lblInOut.text = data.GetDirection();
            m_lblCargoPriority.text = $"{BuildingSettingsFast.GetEffectiveOutsideCargoPriority(data.m_buildingId)}%";
            m_lblCitizenPriority.text = $"{BuildingSettingsFast.GetEffectiveOutsideCitizenPriority(data.m_buildingId)}%";
            m_lblUsage.text = data.GetUsage();
            m_lblOwn.text = data.m_ownCount.ToString();
            m_lblGuest.text = data.m_guestCount.ToString();
            m_lblStuck.text = data.m_stuckCount.ToString();
        }

        protected override void Clear()
        {
            foreach (UIComponent component in components)
            {
                if (component is UILabel label)
                {
                    label.text = "";
                }
            }
        }

        protected override void ClearTooltips()
        {
            foreach (UIComponent component in components)
            {
                if (component is UILabel label)
                {
                    label.tooltip = "";
                }
            }
        }

        protected override void OnClicked(UIComponent component)
        {
            if (data is not null)
            {
                data.Show();

                if (OutsideConnectionPanel.Exists)
                {
                    OutsideConnectionPanel.Instance.InvalidatePanel();
                }
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            return $"Connection #{GetIndex(data.m_buildingId)} | {CitiesUtils.GetBuildingName(data.m_buildingId, true, true)}";
        }

        protected override Color GetTextColor(UIComponent component, bool hightlightRow)
        {
            if (data is not null)
            {
                if (!hightlightRow && BuildingUtils.GetSelectedBuilding() == data.m_buildingId)
                {
                    return KnownColor.lightBlue;
                }
                else if (data.GetTotal() == 0)
                {
                    return KnownColor.orange;
                }
            }

            return base.GetTextColor(component, hightlightRow);
        }

        public static int GetIndex(ushort buildingId)
        {
            int iPosition = 0;
            foreach (ushort outsideId in BuildingManager.instance.GetOutsideConnections())
            {
                iPosition++;
                if (buildingId == outsideId)
                {
                    break;
                }
            }

            return iPosition;
        }
    }
}