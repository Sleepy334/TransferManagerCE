using ColossalFramework.UI;
using ICities;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIDistancePanel : UIPanel
    {
        private bool m_bIncoming;
        private SettingsSlider? m_sliderServiceDistance = null;
        private UIPanel? m_colorPanel = null;

        // ----------------------------------------------------------------------------------------
        // Distance Restrictions Panel

        public float Value
        {
            get
            {
                return m_sliderServiceDistance.Value;
            }
            set
            {
                m_sliderServiceDistance.Value = value;
            }
        }

        public KnownColor Color
        {
            set
            {
                m_colorPanel.color = value;
            }
        }

        public static UIDistancePanel Create(UIPanel parent, bool bIncoming, KnownColor color, float fTextScale, ICities.OnValueChanged eventCallback)
        {
            UIDistancePanel panel = parent.AddUIComponent<UIDistancePanel>();
            panel.Setup(bIncoming, color, fTextScale, eventCallback);
            return panel;
        }

        private void Setup(bool bIncoming, KnownColor color, float fTextScale, ICities.OnValueChanged eventCallback)
        {
            m_bIncoming = bIncoming;
            width = parent.width;
            height = 35;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(4, 4, 4, 4);

            string sLabel = string.Empty;
            if (bIncoming)
            {
                sLabel = $"{Localization.Get("txtBuildingRestrictionsIncoming")} {Localization.Get("sliderDistanceRestriction")}";
            }
            else
            {
                sLabel = $"{Localization.Get("txtBuildingRestrictionsOutgoing")} {Localization.Get("sliderDistanceRestriction")}";
            }

            m_sliderServiceDistance = SettingsSlider.Create(this, LayoutDirection.Horizontal, sLabel, UIFonts.Regular, fTextScale, 400, 260, 0f, 20f, 0.5f, 0f, 1, eventCallback);
            m_sliderServiceDistance.SetTooltip(Localization.Get("sliderDistanceRestrictionTooltip"));

            UIPanel spacer = AddUIComponent<UIPanel>();
            spacer.width = height;
            spacer.height = height;

            m_colorPanel = spacer.AddUIComponent<UIPanel>();
            m_colorPanel.width = 15;
            m_colorPanel.height = 15;
            m_colorPanel.backgroundSprite = "InfoviewPanel";
            m_colorPanel.color = color;
            m_colorPanel.CenterToParent();
        }
    }
}
