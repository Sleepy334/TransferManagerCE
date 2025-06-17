using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using TransferManagerCE.TransferRules;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIBuildingRestrictionsPanel : UIPanel
    {
        const int iButtonHeight = 28;

        private UILabel? m_lblBuildingRestrictions = null;
        private UIToggleButton? m_btnBuildingRestrictions = null;
        private UIButton? m_btnBuildingRestrictionsClear = null;
        private bool m_bIncoming;

        // ----------------------------------------------------------------------------------------
        public delegate void BuildingRestrictionsMouseEventHandler(bool bIncoming);
        public delegate void DistrictDropDownSelectionChanged(bool bIncoming, int index);

        public event BuildingRestrictionsMouseEventHandler eventOnBuildingRestrictionsClicked;
        public event BuildingRestrictionsMouseEventHandler eventOnBuildingRestrictionsClearClicked;

        // ----------------------------------------------------------------------------------------
        // District Restrictions Panel
        public static UIBuildingRestrictionsPanel Create(UIPanel parent, bool bIncoming, float fTextScale)
        {
            UIBuildingRestrictionsPanel panel = parent.AddUIComponent<UIBuildingRestrictionsPanel>();
            panel.Setup(bIncoming, fTextScale);
            return panel;
        }

        private void Setup(bool bIncoming, float fTextScale)
        {
            m_bIncoming = bIncoming;
            width = parent.width;
            height = 35;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(4, 4, 4, 4);

            string sLabel;
            if (bIncoming)
            {
                sLabel = Localization.Get("txtBuildingRestrictionsIncoming");
            }
            else
            {
                sLabel = Localization.Get("txtBuildingRestrictionsOutgoing");
            }

            // Label
            m_lblBuildingRestrictions = AddUIComponent<UILabel>();
            m_lblBuildingRestrictions.text = sLabel;
            m_lblBuildingRestrictions.font = UIFonts.Regular;
            m_lblBuildingRestrictions.textScale = fTextScale;
            m_lblBuildingRestrictions.autoSize = false;
            m_lblBuildingRestrictions.height = 30;
            m_lblBuildingRestrictions.width = 370;
            m_lblBuildingRestrictions.verticalAlignment = UIVerticalAlignment.Middle;

            // Buttons
            m_btnBuildingRestrictions = UIMyUtils.AddToggleButton(UIMyUtils.ButtonStyle.DropDown, this, Localization.Get("btnBuildingRestrictions"), "", 280, iButtonHeight, OnBuildingRestrictionsClicked);
            m_btnBuildingRestrictions.onColor = KnownColor.lightBlue;
            m_btnBuildingRestrictions.offColor = KnownColor.white;
            m_btnBuildingRestrictions.StateOn = false; // Start off

            m_btnBuildingRestrictionsClear = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, this, "Niet", iButtonHeight, iButtonHeight, OnBuildingRestrictionsClearClicked);
            m_btnBuildingRestrictionsClear.tooltip = Localization.Get("btnClear");
        }

        public UILabel Label
        {
            get
            {
                return m_lblBuildingRestrictions;
            }
        }

        public UIToggleButton SelectButton
        {
            get
            {
                return m_btnBuildingRestrictions;
            }
        }

        public UIButton ClearButton
        {
            get
            {
                return m_btnBuildingRestrictionsClear;
            }
        }

        public void UpdatePanel(ushort buildingId, ReasonRule currentRule, RestrictionSettings settings)
        {
            SelectionTool.SelectionToolMode mode;
            BuildingRestrictionSettings buildingSettings;

            if (m_bIncoming)
            {
                isVisible = currentRule.m_incomingBuilding;
                mode = SelectionTool.SelectionToolMode.BuildingRestrictionIncoming;
                buildingSettings = settings.m_incomingBuildingSettings;
            }
            else
            {
                isVisible = currentRule.m_outgoingBuilding;
                mode = SelectionTool.SelectionToolMode.BuildingRestrictionOutgoing;
                buildingSettings = settings.m_outgoingBuildingSettings;
            }

            // Update button state and text
            HashSet<ushort> allowedBuildings = buildingSettings.GetBuildingRestrictionsCopy();
            Label.text = GetBuildingRestrictionLabel(m_bIncoming, allowedBuildings);

            SelectButton.tooltip = buildingSettings.Describe(buildingId);
            UpdateSelectButton(SelectionTool.Instance.GetNewMode() == mode);

            ClearButton.isEnabled = allowedBuildings.Count > 0;
        }

        private void UpdateSelectButton(bool bOn)
        {
            // Update button state and text
            if (bOn)
            {
                SelectButton.text = Localization.Get("btnBuildingRestrictionsSelected");
                SelectButton.StateOn = true;
            }
            else
            {
                SelectButton.text = Localization.Get("btnBuildingRestrictions");
                SelectButton.StateOn = false;
            }
        }

        private void OnBuildingRestrictionsClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_btnBuildingRestrictions.Toggle();

            if (eventOnBuildingRestrictionsClicked != null)
            {
                eventOnBuildingRestrictionsClicked(m_bIncoming);
            }
        }

        private void OnBuildingRestrictionsClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_btnBuildingRestrictions.Toggle();

            if (eventOnBuildingRestrictionsClearClicked != null)
            {
                eventOnBuildingRestrictionsClearClicked(m_bIncoming);
            }
        }

        private string GetBuildingRestrictionLabel(bool bIncoming, HashSet<ushort> restrictions)
        {
            // Update label
            string sText;
            if (bIncoming)
            {
                sText = Localization.Get("txtBuildingRestrictionsIncoming") + ": ";
            }
            else
            {
                sText = Localization.Get("txtBuildingRestrictionsOutgoing") + ": ";
            }

            // Add restriction text
            if (restrictions.Count == 0)
            {
                sText += Localization.Get("txtBuildingRestrictionsAllBuildings");
            }
            else
            {
                sText += $"{Localization.Get("txtBuildingRestrictionsRestricted")} ({restrictions.Count})";
            }

            return sText;
        }
    }
}

