using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIDistrictRestrictionsPanel : UIPanel
    {
        const int iDropDownWidth = 300;
        const int iDropDownWidthOffset = 226;

        static string[] s_itemsPreferLocal = 
        {
            Localization.Get("dropdownBuildingPanelPreferLocal1"),
            Localization.Get("dropdownBuildingPanelPreferLocal2"),
            Localization.Get("dropdownBuildingPanelPreferLocal3"),
            Localization.Get("dropdownBuildingPanelPreferLocal4"),
        };

        private UIMyDropDown? m_dropPreferLocal = null;
        private UIToggleButton? m_btnSelectDistrict = null;
        private UIButton? m_btnClear = null;
        private bool m_bIncoming;

        // ----------------------------------------------------------------------------------------
        public delegate void DistrictMouseEventHandler(bool bIncoming);
        public delegate void DistrictDropDownSelectionChanged(bool bIncoming, int index);

        public event DistrictDropDownSelectionChanged eventSelectedIndexChanged;
        public event DistrictMouseEventHandler eventOnDistrictClicked;
        public event DistrictMouseEventHandler eventOnDistrictTooltipEnter;
        public event DistrictMouseEventHandler eventOnDistrictClearClicked;

        // ----------------------------------------------------------------------------------------
        // District Restrictions Panel
        public static UIDistrictRestrictionsPanel Create(UIPanel parent, bool bIncoming, float fTextScale)
        {
            UIDistrictRestrictionsPanel panel = parent.AddUIComponent<UIDistrictRestrictionsPanel>();
            panel.SetupInternal(bIncoming, fTextScale);
            return panel;
        }

        public UIMyDropDown DropDown
        {
            get
            {
                return m_dropPreferLocal;
            }
        }

        public UIToggleButton SelectDistrict
        {
            get
            {
                return m_btnSelectDistrict;
            }
        }

        public UIButton Clear
        {
            get
            {
                return m_btnClear;
            }
        }

        private void SetupInternal(bool bIncoming, float fTextScale)
        {
            m_bIncoming = bIncoming;

            width = parent.width;
            height = 35;

            string sLabel;
            if (bIncoming) 
            {
                sLabel = Localization.Get("dropdownBuildingPanelIncomingPreferLocalLabel");
            }
            else
            {
                sLabel = Localization.Get("dropdownBuildingPanelOutgoingPreferLocalLabel");
            }

            m_dropPreferLocal = UIMyDropDown.Create(this, sLabel, fTextScale, s_itemsPreferLocal, OnPreferLocalServices, 0, iDropDownWidth);
            if (m_dropPreferLocal is not null)
            {
                m_dropPreferLocal.Panel.relativePosition = new Vector3(0, 0);
                m_dropPreferLocal.SetPanelWidth(width - iDropDownWidthOffset);
                m_dropPreferLocal.DropDown.textScale = 0.9f;
            }
            m_btnSelectDistrict = UIMyUtils.AddToggleButton(UIMyUtils.ButtonStyle.DropDown, this, Localization.Get("btnDistricts") + "...", "", 120, m_dropPreferLocal.DropDown.height, OnSelectDistrictClicked);
            if (m_btnSelectDistrict is not null)
            {
                m_btnSelectDistrict.onColor = KnownColor.lightBlue; 
                m_btnSelectDistrict.offColor = KnownColor.white;
                m_btnSelectDistrict.StateOn = false; // start off

                m_btnSelectDistrict.relativePosition = new Vector3(m_dropPreferLocal.Panel.width + 6, 2);
                m_btnSelectDistrict.eventTooltipEnter += OnSelectDistrictTooltipEnter;
            }

            m_btnClear = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, this, "Niet", m_dropPreferLocal.DropDown.height, m_dropPreferLocal.DropDown.height, OnSelectDistrictClearClicked);
            if (m_btnClear is not null)
            {
                m_btnClear.relativePosition = new Vector3(m_dropPreferLocal.Panel.width + m_btnSelectDistrict.width + 12, 2);
                m_btnClear.tooltip = Localization.Get("btnClear");
            }
        }

        public void OnPreferLocalServices(UIComponent component, int Value)
        {
            if (eventSelectedIndexChanged != null)
            {
                eventSelectedIndexChanged(m_bIncoming, Value);
            }
        }

        public void OnSelectDistrictClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToggleButtonState();

            if (eventOnDistrictClicked != null)
            {
                eventOnDistrictClicked(m_bIncoming);
            }
        }

        public void OnSelectDistrictTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (eventOnDistrictTooltipEnter != null)
            {
                eventOnDistrictTooltipEnter(m_bIncoming);
            }
        }

        public void OnSelectDistrictClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            SelectDistrict.StateOn = false;

            if (eventOnDistrictClicked != null)
            {
                eventOnDistrictClearClicked(m_bIncoming);
            }
        }

        public void SetButtonState(bool bPressed)
        {
            SelectDistrict.StateOn = bPressed;
        }

        public void ToggleButtonState()
        {
            SelectDistrict.Toggle();
        }

        public void Destroy()
        {
            if (m_dropPreferLocal is not null)
            {
                m_dropPreferLocal.OnDestroy();
                m_dropPreferLocal = null;
            }
            if (m_btnSelectDistrict is not null)
            {
                m_btnSelectDistrict.OnDestroy();
                m_btnSelectDistrict = null;
            }
            if (m_btnClear is not null)
            {
                m_btnClear.OnDestroy();
                m_btnClear = null;
            }
        }
    }
}