using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCore.Data;
using UnityEngine;

namespace TransferManagerCore.UI
{
    public class UIStatusRow : UIListRow<StatusData>
    {
        private UILabel? m_lblMaterial = null;
        private UILabel? m_lblValue = null;
        private UILabel? m_lblTimer = null;
        private UILabel? m_lblDistance = null;
        private UILabelLiveTooltip? m_lblDescription1 = null;
        private UILabelLiveTooltip? m_lblDescription2 = null;
        private UIButton? m_btnDelete = null;

        public static float[] ColumnWidths =
        {
            120, // Material
            80, // Value
            80, // Timer
            60, // Distance
            200, // Description1
            200, // Description2
        };

        // ----------------------------------------------------------------------------------------
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
                m_lblMaterial.textAlignment = UIHorizontalAlignment.Left;
                m_lblMaterial.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblMaterial.autoSize = false;
                m_lblMaterial.height = height;
                m_lblMaterial.width = ColumnWidths[0];
                m_lblMaterial.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblMaterial.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                m_lblValue.width = ColumnWidths[1];
                m_lblValue.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblValue.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                m_lblTimer.width = ColumnWidths[2];
                m_lblTimer.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblTimer.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                m_lblDistance.width = ColumnWidths[3];
                m_lblDistance.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblDistance.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblDescription1 = AddUIComponent<UILabelLiveTooltip>();
            if (m_lblDescription1 is not null)
            {
                m_lblDescription1.name = "m_lblDescription1";
                m_lblDescription1.text = "";
                m_lblDescription1.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription1.tooltip = "";
                m_lblDescription1.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription1.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription1.autoSize = false;
                m_lblDescription1.height = height;
                m_lblDescription1.width = ColumnWidths[4];
                m_lblDescription1.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblDescription1.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            }

            m_lblDescription2 = AddUIComponent<UILabelLiveTooltip>();
            if (m_lblDescription2 is not null)
            {
                m_lblDescription2.name = "m_lblDescription2";
                m_lblDescription2.text = "";
                m_lblDescription2.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription2.tooltip = "";
                m_lblDescription2.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription2.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription2.autoSize = false;
                m_lblDescription2.height = height;
                m_lblDescription2.width = ColumnWidths[5];
                m_lblDescription2.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                m_lblDescription2.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
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
                m_btnDelete.tooltip = "";
                m_btnDelete.eventClick += (component, param) =>
                {
                    if (data is not null && data.CanDelete())
                    {
                        // Clear tooltip
                        if (m_btnDelete.tooltipBox is not null)
                        {
                            m_btnDelete.tooltip = "";
                            m_btnDelete.tooltipBox.Hide();
                        }

                        data.OnClickDelete();
                    }
                };
            }

            AfterStart();
        }


        protected override void Display()
        {
            // Update row
            if (data.IsHeader())
            {
                // Make first column full width
                m_lblMaterial.width = width;
            }
            else
            {
                m_lblMaterial.width = ColumnWidths[0];
            }

            m_lblMaterial.text = data.GetMaterialDisplay();
            m_lblValue.text = data.GetValue();
            m_lblTimer.text = data.GetTimer();
            m_lblDistance.text = data.GetDistanceAsString();
            m_lblDescription1.text = data.GetDescription1();
            m_lblDescription2.text = data.GetDescription2();

            if (data.CanDelete())
            {
                m_btnDelete.isVisible = true;
                m_btnDelete.tooltip = data.GetDeleteTooltip();
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
            m_lblDistance.text = "";
            m_lblDescription1.text = "";
            m_lblDescription2.text = "";
            m_btnDelete.text = "";
        }

        protected override void ClearTooltips()
        {
            m_lblMaterial.tooltip = "";
            m_lblValue.tooltip = "";
            m_lblTimer.tooltip = "";
            m_lblDistance.tooltip = "";
            m_lblDescription1.tooltip = "";
            m_lblDescription2.tooltip = "";
            m_btnDelete.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (component == m_lblDescription1)
            {
                data.OnClickDescription1();
            }
            else if (component == m_lblDescription2)
            {
                data.OnClickDescription2();
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
                else if (component == m_lblDescription1)
                {
                    sTooltip = data.GetDescription1Tooltip();
                }
                else if (component == m_lblDescription2)
                {
                    sTooltip = data.GetDescription2Tooltip();
                }
                else if (component == m_btnDelete)
                {
                    sTooltip = data.GetDeleteTooltip();
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