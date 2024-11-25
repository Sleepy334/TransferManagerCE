using ColossalFramework.UI;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIOutsideRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblName = null;
        private UILabel? m_lblType = null;
        private UILabel? m_lblMultiplier = null;
        private UILabel? m_lblOwn = null;
        private UILabel? m_lblStuck = null;
        private UILabel? m_lblGuest = null;

        private OutsideContainer? m_data = null;

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
            eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            eventMouseLeave += new MouseEventHandler(OnMouseLeave);

            m_lblName = AddUIComponent<UILabel>();
            if (m_lblName != null)
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
                m_lblName.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblName.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblType = AddUIComponent<UILabel>();
            if (m_lblType != null)
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
                m_lblType.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblType.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblMultiplier = AddUIComponent<UILabel>();
            if (m_lblMultiplier != null)
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
                m_lblMultiplier.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMultiplier.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOwn = AddUIComponent<UILabel>();
            if (m_lblOwn != null)
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
                m_lblOwn.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwn.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblGuest = AddUIComponent<UILabel>();
            if (m_lblGuest != null)
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
                m_lblGuest.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblGuest.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblStuck = AddUIComponent<UILabel>();
            if (m_lblStuck != null)
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
                m_lblStuck.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblStuck.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            if (m_data != null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            OutsideContainer? rowData = (OutsideContainer?)data;
            if (rowData != null)
            {
                m_data = rowData;
                if (m_lblName != null)
                {
                    m_lblName.text = rowData.GetName();
                }
                if (m_lblType != null)
                {
                    m_lblType.text = rowData.m_eType.ToString();
                }
                if (m_lblMultiplier != null)
                {
                    m_lblMultiplier.text = BuildingSettingsFast.GetEffectiveOutsideMultiplier(rowData.m_buildingId).ToString();
                }
                int iOwnCount = BuildingUtils.GetOwnParentVehiclesForBuilding(rowData.m_buildingId, out int iOwnStuck).Count;
                if (m_lblOwn != null)
                {
                    m_lblOwn.text = iOwnCount.ToString();
                }
                int iGuestCount = BuildingUtils.GetGuestParentVehiclesForBuilding(rowData.m_buildingId, out int iGuestStuck).Count;
                if (m_lblGuest != null)
                {
                    m_lblGuest.text = iGuestCount.ToString();
                }
                if (m_lblStuck != null)
                {
                    m_lblStuck.text = (iOwnStuck + iGuestStuck).ToString();
                }
            }
            else
            {
                m_data = null;
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
            if (m_data != null)
            {
                m_data.Show();
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UILabel? label in components)
            {
                if (label != null)
                {
                    label.textColor = Color.yellow;
                }
            }

        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UILabel? label in components)
            {
                if (label != null)
                {
                    label.textColor = Color.white;
                }
            }
        }
    }
}