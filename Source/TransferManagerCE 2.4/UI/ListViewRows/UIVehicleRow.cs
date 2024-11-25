using ColossalFramework;
using ColossalFramework.UI;
using TransferManagerCE.Common;
using TransferManagerCE.Data;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIVehicleRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UILabel? m_lblVehicle = null;
        private UILabel? m_lblDistance = null;
        private UILabel? m_lblTarget = null;
        private UIButton? m_btnDelete = null;

        private VehicleData? m_data = null;

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
                m_lblMaterial.name = "lblMaterial";
                m_lblMaterial.text = "";
                m_lblMaterial.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblMaterial.tooltip = "";
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = BuildingPanel.iCOLUMN_WIDTH_LARGE;
                m_lblMaterial.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblMaterial.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblValue = AddUIComponent<UILabel>();
            if (m_lblValue is not null)
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
            if (m_lblTimer is not null)
            {
                m_lblTimer.name = "m_lblTimer";
                m_lblTimer.text = "";
                m_lblTimer.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTimer.tooltip = "";
                m_lblTimer.textAlignment = UIHorizontalAlignment.Center;
                m_lblTimer.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTimer.autoSize = false;
                m_lblTimer.height = height;
                m_lblTimer.width = BuildingPanel.iCOLUMN_WIDTH_NORMAL;
                m_lblTimer.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
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
                m_lblDistance.width = BuildingPanel.iCOLUMN_WIDTH_SMALL;
                m_lblDistance.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            }

            m_lblVehicle = AddUIComponent<UILabel>();
            if (m_lblVehicle is not null)
            {
                m_lblVehicle.name = "m_lblVehicle";
                m_lblVehicle.text = "";
                m_lblVehicle.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblVehicle.tooltip = "";
                m_lblVehicle.textAlignment = UIHorizontalAlignment.Left;
                m_lblVehicle.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblVehicle.autoSize = false;
                m_lblVehicle.height = height;
                m_lblVehicle.width = BuildingPanel.iCOLUMN_WIDTH_XLARGE;
                m_lblVehicle.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblVehicle.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblVehicle.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblVehicle.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblTarget = AddUIComponent<UILabel>();
            if (m_lblTarget is not null)
            {
                m_lblTarget.name = "m_lblTarget";
                m_lblTarget.text = "";
                m_lblTarget.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblTarget.tooltip = "";
                m_lblTarget.textAlignment = UIHorizontalAlignment.Left;
                m_lblTarget.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblTarget.autoSize = false;
                m_lblTarget.height = height;
                m_lblTarget.width = 200;// BuildingPanel.iCOLUMN_WIDTH_250;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_btnDelete = AddUIComponent<UIButton>();
            if (m_btnDelete is not null)
            {
                float fBUTTON_HEIGHT = height - 6;

                m_btnDelete.height = fBUTTON_HEIGHT;
                m_btnDelete.width = fBUTTON_HEIGHT;
                m_btnDelete.normalBgSprite = "buttonclose";
                m_btnDelete.hoveredBgSprite = "buttonclosehover";
                m_btnDelete.pressedBgSprite = "buttonclosepressed";
                m_btnDelete.tooltip = Localization.Get("btnDeleteVehicle");
                m_btnDelete.eventClick += (component, param) =>
                {
                    if (m_data is not null && m_data.m_vehicleId != 0)
                    {
                        // Remove vehicle
                        InstanceID vehicleInstace = new InstanceID { Vehicle = m_data.m_vehicleId };
                        Singleton<SimulationManager>.instance.AddAction(() =>
                        {
                            // If vehicle is stuck we may need to add Created flag to remove it
                            ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleInstace.Vehicle];
                            vehicle.m_flags |= Vehicle.Flags.Created;

                            // Remove vehicle
                            Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleInstace.Vehicle);
                        });
                    }
                };
            }

            if (m_data is not null)
            {
                Display(m_data, false);
            }
        }

        public void Display(object data, bool isRowOdd)
        {
            VehicleData? rowData = (VehicleData?)data;
            if (rowData is not null)
            {
                m_data = rowData;
                if (m_lblMaterial is not null)
                {
                    m_lblMaterial.text = rowData.GetMaterialDescription();
                }
                if (m_lblValue is not null)
                {
                    m_lblValue.text = rowData.GetValue();
                }
                if (m_lblTimer is not null)
                {
                    m_lblTimer.text = rowData.GetTimer();
                }
                if (m_lblDistance is not null)
                {
                    m_lblDistance.text = rowData.GetDistance();
                }
                if (m_lblVehicle is not null)
                {
                    m_lblVehicle.text = rowData.GetVehicle();
                }
                if (m_lblTarget is not null)
                {
                    m_lblTarget.text = rowData.GetTarget();
                }
                if (m_btnDelete is not null)
                {
                    m_btnDelete.isVisible = rowData.m_vehicleId != 0;
                }
            }
        }

        public void Disabled()
        {
            if (m_data is not null)
            {
                m_data = null;
                m_lblMaterial.tooltip = "";
                m_lblValue.tooltip = "";
                m_lblTimer.tooltip = "";
                m_lblDistance.tooltip = "";
                m_lblVehicle.tooltip = "";
                m_lblTarget.tooltip = "";
                m_btnDelete.tooltip = "";
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
                if (component == m_lblTarget)
                {
                    InstanceID target = VehicleTypeHelper.GetVehicleTarget(m_data.m_vehicleId, m_data.m_vehicle);
                    if (!target.IsEmpty)
                    {
                        InstanceHelper.ShowInstance(target);
                    }
                }
                else if (component == m_lblVehicle)
                {
                    if (m_data.m_vehicleId != 0)
                    {
                        InstanceHelper.ShowInstance(new InstanceID { Vehicle = m_data.m_vehicleId });
                    }
                }
            }
        }

        private void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lblVehicle is null || m_lblTarget is null)
            {
                return;
            }

            if (enabled && m_data is not null)
            {
                m_lblVehicle.tooltip = m_data.GetVehicle();
                m_lblTarget.tooltip = m_data.GetTarget();
            }
            else
            {
                m_lblVehicle.tooltip = "";
                m_lblTarget.tooltip = "";
            }
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