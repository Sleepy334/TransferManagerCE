using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIGoodsOutIssueRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblTime = null;
        private UILabel? m_lblOwner = null;

        private TransferIssueContainer? m_data = null;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
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
                m_lblMaterial.text = "m_lblMaterial";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Center;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = TransferIssuePanel.iCOLUMN_WIDTH_VALUE;
                m_lblMaterial.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblTime = AddUIComponent<UILabel>();
            if (m_lblTime is not null)
            {
                m_lblTime.name = "m_lblTime";
                m_lblTime.text = "m_lblTime";
                m_lblTime.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTime.tooltip = "";
                m_lblTime.textAlignment = UIHorizontalAlignment.Center;
                m_lblTime.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTime.autoSize = false;
                m_lblTime.height = height;
                m_lblTime.width = TransferIssuePanel.iCOLUMN_WIDTH_VALUE;
                m_lblTime.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTime.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblOwner = AddUIComponent<UILabel>();
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
                m_lblOwner.width = TransferIssuePanel.iCOLUMN_DESCRIPTION_WIDTH;
                m_lblOwner.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwner.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblOwner.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblOwner.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            if (m_data is not null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            TransferIssueContainer? rowData = (TransferIssueContainer?)data;
            if (rowData is not null)
            {
                m_data = rowData;
                if (m_lblMaterial is not null)
                {
                    m_lblMaterial.text = rowData.m_value.ToString();
                }
                if (m_lblTime is not null)
                {
                    m_lblTime.text = rowData.m_timer.ToString();
                }
                if (m_lblOwner is not null)
                {
                    m_lblOwner.text = CitiesUtils.GetBuildingName(rowData.m_sourceBuildingId).ToString();
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
            if (m_data is not null)
            {
                if (component == m_lblOwner)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_data.m_sourceBuildingId];
                    if (building.m_flags != Building.Flags.None)
                    {
                        CitiesUtils.ShowBuilding(m_data.m_sourceBuildingId);
                    }
                    else
                    {
                        CitiesUtils.ShowPosition(m_data.m_sourcePostion);
                    }
                }
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
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