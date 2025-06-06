using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.Data;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIStatusRow : UIListRow<StatusData>
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UILabelLiveTooltip? m_lblVehicle = null;
        private UILabel? m_lblDistance = null;
        private UITruncateLabel? m_lblResponder = null;
        private UIButton? m_btnDelete = null;

        public override void Start()
        {
            base.Start();

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
            }

            m_lblVehicle = AddUIComponent<UILabelLiveTooltip>();
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
                m_lblVehicle.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblVehicle.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                    if (data is not null && data.HasVehicle())
                    {
                        ushort vehicleId = data.GetVehicleId();
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

            AfterStart();
        }


        protected override void Display()
        {
            // Update row
            m_lblMaterial.text = data.GetMaterialDisplay();
            m_lblValue.text = data.GetValue();
            m_lblTimer.text = data.GetTimer();
            m_lblVehicle.text = data.GetVehicle();
            m_lblDistance.text = data.GetDistanceAsString();
            m_lblResponder.text = data.GetResponder();

            if (data.GetVehicleId() != 0)
            {
                m_btnDelete.isVisible = true;
                m_btnDelete.tooltip = $"{Localization.Get("btnDeleteVehicle")} #{data.GetVehicleId()}";
            }
            else
            {
                m_btnDelete.isVisible = false;
            }
        }

        protected override void Clear()
        {
            m_lblMaterial.text = "";
            m_lblValue.text = "";
            m_lblTimer.text = "";
            m_lblVehicle.text = "";
            m_lblDistance.text = "";
            m_lblResponder.text = "";
            m_btnDelete.text = "";
        }

        protected override void ClearTooltips()
        {
            m_lblMaterial.tooltip = "";
            m_lblValue.tooltip = "";
            m_lblTimer.tooltip = "";
            m_lblVehicle.tooltip = "";
            m_lblDistance.tooltip = "";
            m_lblResponder.tooltip = "";
            m_btnDelete.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (component == m_lblResponder)
            {
                data.OnClickResponder();
            }
            else if (component == m_lblVehicle)
            {
                data.OnClickTarget();
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            string sTooltip = data.GetTooltip();
            if (sTooltip.Length == 0)
            {
                if (component == m_lblValue)
                {
                    sTooltip = data.GetValueTooltip();
                }
                else if (component == m_lblTimer)
                {
                    sTooltip = data.GetTimerTooltip();
                }
                else if (component == m_lblVehicle)
                {
                    sTooltip = data.GetVehicleTooltip();
                }
                else if (component == m_lblResponder)
                {
                    sTooltip = data.GetResponderTooltip();
                }
            }

            return sTooltip;
        }

        protected override Color GetTextColor(UIComponent component, bool hightlightRow)
        {
            if (m_MouseEnterComponent == component)
            {
                return Color.yellow;
            }
            else if (data is not null)
            {
                return data.GetTextColor();
            }
            else
            {
                return Color.white;
            }
        }
    }
}