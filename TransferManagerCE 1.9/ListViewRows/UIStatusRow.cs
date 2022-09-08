using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.Data;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class UIStatusRow : UIPanel, IUIFastListRow
    {
        /*
        m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
        m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
        m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "S = Sick\r\nD = Dead\r\nI = Incoming\r\nW = Waiting\r\nB = Blocked", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
        m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_LARGER, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
        m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_LOAD, Localization.Get("txtLoad"), "Load", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
        m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", iCOLUMN_WIDTH_LARGER, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
        */

        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UILabel? m_lblTarget = null;
        private UILabel? m_lblLoad = null;
        private UILabel? m_lblOwner = null;

        private StatusData? m_data = null;

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

            m_lblMaterial = AddUIComponent<UILabel>();
            if (m_lblMaterial != null)
            {
                m_lblMaterial.name = "lblMaterial";
                m_lblMaterial.text = "";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;// oTextAlignment;// UIHorizontalAlignment.Center;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = BuildingPanel.iCOLUMN_WIDTH_LARGE;
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblValue = AddUIComponent<UILabel>();
            if (m_lblValue != null)
            {
                m_lblValue.name = "m_lblValue";
                m_lblValue.text = "";
                m_lblValue.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblValue.tooltip = "";
                m_lblValue.textAlignment = UIHorizontalAlignment.Center;
                m_lblValue.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblValue.autoSize = false;
                m_lblValue.height = height;
                m_lblValue.width = BuildingPanel.iCOLUMN_WIDTH_NORMAL;
                m_lblValue.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblTimer = AddUIComponent<UILabel>();
            if (m_lblTimer != null)
            {
                m_lblTimer.name = "m_lblTimer";
                m_lblTimer.text = "";
                m_lblTimer.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTimer.tooltip = "";
                m_lblTimer.textAlignment = UIHorizontalAlignment.Center;
                m_lblTimer.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTimer.autoSize = false;
                m_lblTimer.height = height;
                m_lblTimer.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblTimer.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblTarget = AddUIComponent<UILabel>();
            if (m_lblTarget != null)
            {
                m_lblTarget.name = "m_lblTarget";
                m_lblTarget.text = "";
                m_lblTarget.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTarget.tooltip = "";
                m_lblTarget.textAlignment = UIHorizontalAlignment.Left;
                m_lblTarget.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTarget.autoSize = false;
                m_lblTarget.height = height;
                m_lblTarget.width = BuildingPanel.iCOLUMN_WIDTH_LARGER;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblLoad = AddUIComponent<UILabel>();
            if (m_lblLoad != null)
            {
                m_lblLoad.name = "m_lblLoad";
                m_lblLoad.text = "";
                m_lblLoad.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblLoad.tooltip = "";
                m_lblLoad.textAlignment = UIHorizontalAlignment.Center;
                m_lblLoad.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblLoad.autoSize = false;
                m_lblLoad.height = height;
                m_lblLoad.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblLoad.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOwner = AddUIComponent<UILabel>();
            if (m_lblOwner != null)
            {
                m_lblOwner.name = "m_lblLoad";
                m_lblOwner.text = "";
                m_lblOwner.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOwner.tooltip = "";
                m_lblOwner.textAlignment = UIHorizontalAlignment.Left;
                m_lblOwner.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOwner.autoSize = false;
                m_lblOwner.height = height;
                m_lblOwner.width = BuildingPanel.iCOLUMN_WIDTH_LARGER;
                m_lblOwner.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwner.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblOwner.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblOwner.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            if (m_data != null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            StatusData? rowData = (StatusData?)data;
            if (rowData != null)
            {
                m_data = rowData;
                if (m_lblMaterial != null)
                {
                    m_lblMaterial.text = rowData.GetMaterial().ToString();
                }
                if (m_lblValue != null)
                {
                    m_lblValue.text = rowData.GetValue();
                }
                if (m_lblTimer != null)
                {
                    m_lblTimer.text = rowData.GetTimer();
                }
                if (m_lblTarget != null)
                {
                    m_lblTarget.text = rowData.GetTarget();
                }
                if (m_lblLoad != null)
                {
                    m_lblLoad.text = rowData.GetLoad();
                }
                if (m_lblOwner != null)
                {
                    m_lblOwner.text = rowData.GetResponder();
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
                if (component == m_lblOwner)
                {
                    m_data.OnClickResponder();
                }
                else if (component == m_lblTarget)
                {
                    m_data.OnClickTarget();
                }
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel? txtLabel = component as UILabel;
            if (txtLabel != null)
            {
                txtLabel.textColor = Color.yellow;
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            UILabel? txtLabel = component as UILabel;
            if (txtLabel != null)
            {
                txtLabel.textColor = Color.white;
            }
        }
    }
}