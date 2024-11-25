using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.Data;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class UIIssueRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblTime = null;
        private UILabel? m_lblOwner = null;
        private UILabel? m_lblTarget = null;
        private UILabel? m_lblVehicle = null;

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
            if (m_lblMaterial != null)
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
            if (m_lblTime != null)
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
            if (m_lblOwner != null)
            {
                m_lblOwner.name = "m_lblOwner";
                m_lblOwner.text = "";
                m_lblOwner.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOwner.tooltip = "";
                m_lblOwner.textAlignment = UIHorizontalAlignment.Left;
                m_lblOwner.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOwner.autoSize = false;
                m_lblOwner.height = height;
                m_lblOwner.width = TransferIssuePanel.iCOLUMN_VEHICLE_WIDTH;
                m_lblOwner.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwner.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblOwner.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblOwner.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                m_lblTarget.width = TransferIssuePanel.iCOLUMN_VEHICLE_WIDTH;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblVehicle = AddUIComponent<UILabel>();
            if (m_lblVehicle != null)
            {
                m_lblVehicle.name = "m_lblVehicle";
                m_lblVehicle.text = "";
                m_lblVehicle.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblVehicle.tooltip = "";
                m_lblVehicle.textAlignment = UIHorizontalAlignment.Left;
                m_lblVehicle.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblVehicle.autoSize = false;
                m_lblVehicle.height = height;
                m_lblVehicle.width = TransferIssuePanel.iCOLUMN_VEHICLE_WIDTH;
                m_lblVehicle.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblVehicle.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblVehicle.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblVehicle.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            if (m_data != null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            TransferIssueContainer? rowData = (TransferIssueContainer?)data;
            if (rowData != null)
            {
                m_data = rowData;
                if (m_lblMaterial != null)
                {
                    m_lblMaterial.text = rowData.m_value.ToString();
                }
                if (m_lblTime != null)
                {
                    m_lblTime.text = rowData.m_timer.ToString();
                }
                if (m_lblOwner != null)
                {
                    m_lblOwner.text = CitiesUtils.GetBuildingName(rowData.m_sourceBuildingId).ToString();
                }
                if (m_lblTarget != null)
                {
                    m_lblTarget.text = CitiesUtils.GetBuildingName(rowData.m_targetBuildingId).ToString();
                }
                if (m_lblVehicle != null)
                {
                    m_lblVehicle.text = CitiesUtils.GetVehicleName(rowData.m_vehicleId).ToString();
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
                else if (component == m_lblTarget)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_data.m_targetBuildingId];
                    if (building.m_flags != Building.Flags.None)
                    {
                        CitiesUtils.ShowBuilding(m_data.m_targetBuildingId);
                    }
                    else
                    {
                        CitiesUtils.ShowPosition(m_data.m_targetPostion);
                    }
                }
                else if (component == m_lblVehicle)
                {
                    CitiesUtils.ShowVehicle(m_data.m_vehicleId);
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