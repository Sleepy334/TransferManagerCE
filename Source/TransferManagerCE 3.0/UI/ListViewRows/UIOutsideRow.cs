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
        private UILabel? m_lblMultiplier = null;
        private UILabel? m_lblOwn = null;
        private UILabel? m_lblStuck = null;
        private UILabel? m_lblGuest = null;

        public override void Start()
        {
            base.Start();
            fullRowSelect = true;

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
                m_lblName.width = OutsideConnectionPanel.iCOLUMN_WIDTH_XLARGE;
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
                m_lblType.width = OutsideConnectionPanel.iCOLUMN_WIDTH_SMALL;
            }

            m_lblMultiplier = AddUIComponent<UILabel>();
            if (m_lblMultiplier is not null)
            {
                m_lblMultiplier.name = "m_lblMultiplier";
                m_lblMultiplier.text = "";
                m_lblMultiplier.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMultiplier.tooltip = "";
                m_lblMultiplier.textAlignment = UIHorizontalAlignment.Center;
                m_lblMultiplier.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMultiplier.autoSize = false;
                m_lblMultiplier.height = height;
                m_lblMultiplier.width = OutsideConnectionPanel.iCOLUMN_WIDTH_NORMAL;
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
                m_lblOwn.width = OutsideConnectionPanel.iCOLUMN_WIDTH_LARGE;
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
                m_lblGuest.width = OutsideConnectionPanel.iCOLUMN_WIDTH_LARGE;
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
                m_lblStuck.width = OutsideConnectionPanel.iCOLUMN_WIDTH_LARGE;
            }

            AfterStart();
        }

        protected override void Display()
        {
            m_lblName.text = data.GetName();
            m_lblType.text = data.m_eType.ToString();
            m_lblMultiplier.text = BuildingSettingsFast.GetEffectiveOutsideMultiplier(data.m_buildingId).ToString();

            int iOwnCount = BuildingUtils.GetOwnParentVehiclesForBuilding(data.m_buildingId, out int iOwnStuck).Count;
            if (m_lblOwn is not null)
            {
                m_lblOwn.text = iOwnCount.ToString();
            }

            int iGuestCount = BuildingUtils.GetGuestParentVehiclesForBuilding(data.m_buildingId, out int iGuestStuck).Count;
            if (m_lblGuest is not null)
            {
                m_lblGuest.text = iGuestCount.ToString();
            }

            m_lblStuck.text = (iOwnStuck + iGuestStuck).ToString();
        }

        protected override void Clear()
        {
            m_lblName.text = "";
            m_lblType.text = "";
            m_lblMultiplier.text = "";
            m_lblOwn.text = "";
            m_lblGuest.text = "";
            m_lblStuck.text = "";
        }

        protected override void ClearTooltips()
        {
            m_lblName.tooltip = "";
            m_lblType.tooltip = "";
            m_lblMultiplier.tooltip = "";
            m_lblOwn.tooltip = "";
            m_lblGuest.tooltip = "";
            m_lblStuck.tooltip = "";
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
            return $"Connection #{GetIndex(data.m_buildingId)} | {InstanceHelper.DescribeInstance(new InstanceID() { Building = data.m_buildingId }, InstanceID.Empty, true)}";
        }

        protected override Color GetTextColor(UIComponent component, bool hightlightRow)
        {
            if (data is not null)
            {
                if (!hightlightRow && BuildingUtils.GetSelectedBuilding() == data.m_buildingId)
                {
                    return KnownColor.lightBlue;
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