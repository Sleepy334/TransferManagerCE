using ColossalFramework;
using ColossalFramework.UI;
using System;
using TransferManagerCE.Data;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIStatusRow : UIPanel, IUIFastListRow
    {
        enum TooltipElement
        {
            None,
            Full,
            Value,
            Timer,
            Target,
            Responder,
        }

        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UITruncateLabel? m_lblTarget = null;
        private UILabel? m_lblDistance = null;
        private UITruncateLabel? m_lblResponder = null;
        private UIButton? m_btnDelete = null;
        private StatusData? m_data = null;
        private UIComponent? m_MouseEnterComponent = null;

        // Live toltip support
        private TooltipElement m_iTooltipElement = TooltipElement.None;
        private bool m_bShowingTooltip = false;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width - ListView.iSCROLL_BAR_WIDTH;
            height = ListView.iROW_HEIGHT;
            //backgroundSprite = "InfoviewPanel";
            //color = new Color32(255, 0, 0, 225);
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;
            eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            eventTooltipLeave += new MouseEventHandler(OnTooltipLeave); 

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
                m_lblMaterial.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
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
                m_lblValue.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
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
                m_lblTimer.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
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
                m_lblDistance.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
            }

            m_lblTarget = AddUIComponent<UITruncateLabel>();
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
                m_lblTarget.width = BuildingPanel.iCOLUMN_WIDTH_XLARGE;
                m_lblTarget.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblTarget.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblTarget.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
                m_lblTarget.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTarget.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblResponder = AddUIComponent<UITruncateLabel>();
            if (m_lblResponder is not null)
            {
                m_lblResponder.name = "m_lblResponder";
                m_lblResponder.text = "";
                m_lblResponder.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblResponder.tooltip = "";
                m_lblResponder.textAlignment = UIHorizontalAlignment.Left;
                m_lblResponder.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblResponder.autoSize = false;
                m_lblResponder.height = height;
                m_lblResponder.width = BuildingPanel.iCOLUMN_WIDTH_XLARGE;
                m_lblResponder.eventClicked += new MouseEventHandler(OnItemClicked);
                m_lblResponder.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                m_lblResponder.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
                m_lblResponder.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblResponder.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                    if (m_data is not null && m_data.HasVehicle())
                    {
                        ushort vehicleId = m_data.GetVehicleId();
                        if (vehicleId != 0)
                        {
                            // Clear tooltip
                            if (m_btnDelete.tooltipBox is not null)
                            {
                                m_btnDelete.tooltip = "";
                                m_btnDelete.tooltipBox.Hide();
                            }

                            // Remove vehicle
                            InstanceID vehicleInstace = new InstanceID { Vehicle = vehicleId };
                            Singleton<SimulationManager>.instance.AddAction(() =>
                            {
                                // If vehicle is stuck we may need to add Created flag to remove it
                                ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleInstace.Vehicle];
                                vehicle.m_flags |= Vehicle.Flags.Created;

                                // Remove vehicle
                                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleInstace.Vehicle);
                            });
                        }
                    }
                };
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }


        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (StatusData?) data;

            if (m_lblResponder is null)
            {
                return;
            }

            // The components will have valid positions now, set width of last label
            m_lblResponder.width = width - m_lblResponder.position.x - m_btnDelete.width;

            if (m_data is not null)
            {
                // Update row
                m_lblMaterial.text = m_data.GetMaterialDisplay();
                m_lblValue.text = m_data.GetValue();
                m_lblTimer.text = m_data.GetTimer();
                m_lblTarget.text = m_data.GetTarget();
                m_lblDistance.text = m_data.GetDistanceAsString();
                m_lblResponder.text = m_data.GetResponder();

                // Update live tooltips
                if (m_bShowingTooltip)
                {
                    switch (m_iTooltipElement)
                    {
                        case TooltipElement.Value:
                            {
                                m_lblValue.tooltip = m_data.GetValueTooltip();
                                UpdateLiveTooltip(m_lblValue);
                                break;
                            }
                        case TooltipElement.Timer:
                            {
                                m_lblTimer.tooltip = m_data.GetTimerTooltip();
                                UpdateLiveTooltip(m_lblTimer);
                                break;
                            }
                        case TooltipElement.Target:
                            {
                                m_lblTarget.tooltip = m_data.GetTargetTooltip();
                                UpdateLiveTooltip(m_lblTarget);
                                break;
                            }
                        case TooltipElement.Responder:
                            {
                                m_lblResponder.tooltip = m_data.GetResponderTooltip();
                                UpdateLiveTooltip(m_lblResponder);
                                break;
                            }
                    }
                }

                if (m_data.GetVehicleId() != 0)
                {
                    m_btnDelete.isVisible = true;
                    m_btnDelete.tooltip = $"{Localization.Get("btnDeleteVehicle")} #{m_data.GetVehicleId()}";
                }
                else
                {
                    m_btnDelete.isVisible = false;
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
            else
            {
                Clear();
            }
        }

        private void UpdateLiveTooltip(UILabel lbl)
        {
            if (lbl.tooltipBox is not null && 
                lbl.tooltipBox.isVisible)
            {
                if (lbl.tooltip.Length > 0)
                {
                    lbl.RefreshTooltip();
                }
                else
                {
                    lbl.tooltipBox.Hide();
                }
            }
        }

        public void Disabled()
        {
            Clear();
        }

        public void Clear()
        {
            m_data = null;

            if (m_lblMaterial is not null)
            {
                m_lblMaterial.text = "";
                m_lblValue.text = "";
                m_lblTimer.text = "";
                m_lblTarget.text = "";
                m_lblDistance.text = "";
                m_lblResponder.text = "";
                m_btnDelete.text = "";

                m_lblMaterial.tooltip = "";
                m_lblValue.tooltip = "";
                m_lblTimer.tooltip = "";
                m_lblTarget.tooltip = "";
                m_lblDistance.tooltip = "";
                m_lblResponder.tooltip = "";
                m_btnDelete.tooltip = "";

                if (m_bShowingTooltip &&
                    tooltipBox is not null &&
                    tooltipBox.isVisible)
                {
                    tooltipBox.Hide();
                    m_bShowingTooltip = false;
                }
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
                if (component == m_lblResponder)
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
            if (m_lblValue is null || m_lblResponder is null || m_lblTarget is null || m_lblTimer is null)
            {
                return;
            }

            if (enabled && m_data is not null)
            {
                m_bShowingTooltip = true;

                // Don't load other tooltips if we get a global tooltip.
                tooltip = m_data.GetTooltip();
                if (tooltip.Length > 0)
                {
                    m_lblValue.tooltip = "";
                    m_lblTimer.tooltip = "";
                    m_lblResponder.tooltip = "";
                    m_lblTarget.tooltip = "";
                    m_iTooltipElement = TooltipElement.Full;
                }
                else
                {
                    m_lblValue.tooltip = m_data.GetValueTooltip();
                    m_lblTimer.tooltip = m_data.GetTimerTooltip();
                    m_lblResponder.tooltip = m_data.GetResponderTooltip();
                    m_lblTarget.tooltip = m_data.GetTargetTooltip();

                    if (component == m_lblValue)
                    {
                        m_iTooltipElement = TooltipElement.Value;
                    }
                    else if (component == m_lblTimer)
                    {
                        m_iTooltipElement = TooltipElement.Timer;
                    }
                    else if (component == m_lblTarget)
                    {
                        m_iTooltipElement = TooltipElement.Target;
                    }
                    else if (component == m_lblResponder)
                    {
                        m_iTooltipElement = TooltipElement.Responder;
                    }
                    
                }
            }
            else
            {
                tooltip = "";
                m_lblValue.tooltip = "";
                m_lblTimer.tooltip = "";
                m_lblResponder.tooltip = "";
                m_lblTarget.tooltip = "";

                if (m_bShowingTooltip &&
                    tooltipBox is not null &&
                    tooltipBox.isVisible)
                {
                    tooltipBox.Hide();
                    m_bShowingTooltip = false;
                }
            }
        }

        private void OnTooltipLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_bShowingTooltip = false;
            m_iTooltipElement = TooltipElement.None;
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