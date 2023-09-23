using ColossalFramework;
using ColossalFramework.UI;
using TransferManagerCE.Common;
using TransferManagerCE.Data;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIStatusRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UILabel? m_lblTarget = null;
        private UILabel? m_lblLoad = null;
        private UILabel? m_lblDistance = null;
        private UILabel? m_lblOwner = null;
        private UIButton? m_btnDelete = null;
        private StatusData? m_data = null;
        private UIComponent? m_MouseEnterComponent = null;

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
                m_lblMaterial.width = BuildingPanel.iCOLUMN_WIDTH_LARGE;
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
                m_lblTarget.width = BuildingPanel.iCOLUMN_WIDTH_LARGER;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblLoad = AddUIComponent<UILabel>();
            if (m_lblLoad is not null)
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
            if (m_lblOwner is not null)
            {
                m_lblOwner.name = "m_lblLoad";
                m_lblOwner.text = "";
                m_lblOwner.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblOwner.tooltip = "";
                m_lblOwner.textAlignment = UIHorizontalAlignment.Left;
                m_lblOwner.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblOwner.autoSize = false;
                m_lblOwner.height = height;
                m_lblOwner.width = 180.0f;// BuildingPanel.iCOLUMN_WIDTH_250;
                m_lblOwner.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblOwner.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblOwner.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblOwner.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                    if (m_data is not null && m_data.m_targetVehicle != 0)
                    {
                        // Remove vehicle
                        InstanceID vehicleInstace = new InstanceID { Vehicle = m_data.m_targetVehicle };
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
            StatusData? rowData = (StatusData?)data;
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
                if (m_lblTarget is not null)
                {
                    m_lblTarget.text = rowData.GetTarget();
                }
                if (m_lblLoad is not null)
                {
                    m_lblLoad.text = rowData.GetLoad();
                }
                if (m_lblDistance is not null)
                {
                    m_lblDistance.text = rowData.GetDistance();
                }
                if (m_lblOwner is not null)
                {
                    m_lblOwner.text = rowData.GetResponder();
                }
                if (m_btnDelete is not null)
                {
                    m_btnDelete.isVisible = m_data.m_targetVehicle != 0;
                }

                // Update text color
                foreach (UIComponent component in components)
                {
                    if (component is UILabel label)
                    {
                        label.textColor = GetTextColor(label);
                    }
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
                m_lblTarget.tooltip = "";
                m_lblLoad.tooltip = "";
                m_lblDistance.tooltip = "";
                m_lblOwner.tooltip = "";
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
            if (m_lblValue is null || m_lblOwner is null)
            {
                return;
            }

            if (enabled && m_data is not null)
            {
                m_lblValue.tooltip = m_data.GetValueTooltip();
                m_lblOwner.tooltip = m_data.GetResponderTooltip();
            }
            else
            {
                m_lblValue.tooltip = "";
                m_lblOwner.tooltip = "";
            }
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_MouseEnterComponent = component;

            UILabel? txtLabel = component as UILabel;
            if (txtLabel is not null)
            {
                txtLabel.textColor = GetTextColor(component);
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_MouseEnterComponent = null;

            UILabel? txtLabel = component as UILabel;
            if (txtLabel is not null)
            {
                txtLabel.textColor = GetTextColor(component); 
            }
        }

        public virtual Color GetTextColor(UIComponent component)
        {
            if (m_MouseEnterComponent == component)
            {
                return Color.yellow;
            }
            else if (m_data is not null)
            {
                return m_data.GetTextColor();
            }
            else
            {
                return Color.white;
            }
        }
    }
}