using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UISettingsRow : UIListRow<SettingsData>
    {
        private UILabel? m_lblId = null;
        private UILabel? m_lblRestrictions = null;
        private UITruncateLabel? m_lblDescription = null;
        private UITruncateLabel? m_lblType = null;
        private UIButton? m_btnDelete = null;

        public override void Start()
        {
            base.Start();
        
            // Select whole row
            fullRowSelect = true;

            m_lblId = AddUIComponent<UILabel>();
            if (m_lblId is not null)
            {
                m_lblId.name = "m_lblId";
                m_lblId.text = "";
                m_lblId.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblId.tooltip = "";
                m_lblId.textAlignment = UIHorizontalAlignment.Left;
                m_lblId.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblId.autoSize = false;
                m_lblId.height = height;
                m_lblId.width = SettingsPanel.iCOLUMN_WIDTH_ID;
            }

            m_lblRestrictions = AddUIComponent<UILabel>();
            if (m_lblRestrictions is not null)
            {
                m_lblRestrictions.name = "m_lblRestrictions";
                m_lblRestrictions.text = "";
                m_lblRestrictions.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblRestrictions.tooltip = "";
                m_lblRestrictions.textAlignment = UIHorizontalAlignment.Center;
                m_lblRestrictions.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblRestrictions.autoSize = false;
                m_lblRestrictions.height = height;
                m_lblRestrictions.width = SettingsPanel.iCOLUMN_WIDTH_RESTRICTIONS;
            }

            m_lblDescription = AddUIComponent<UITruncateLabel>();
            if (m_lblDescription is not null)
            {
                m_lblDescription.name = "m_lblOwner";
                m_lblDescription.text = "";
                m_lblDescription.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription.tooltip = "";
                m_lblDescription.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription.autoSize = false;
                m_lblDescription.height = height;
                m_lblDescription.width = SettingsPanel.iCOLUMN_WIDTH_DESCRIPTION;
            }

            m_lblType = AddUIComponent<UITruncateLabel>();
            if (m_lblType is not null)
            {
                m_lblType.name = "m_lblOwner";
                m_lblType.text = "";
                m_lblType.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblType.tooltip = "";
                m_lblType.textAlignment = UIHorizontalAlignment.Left;
                m_lblType.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblType.autoSize = false;
                m_lblType.height = height;
                m_lblType.width = SettingsPanel.iCOLUMN_WIDTH_DESCRIPTION;
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
                m_btnDelete.eventClick += (component, param) =>
                {
                    if (data is not null)
                    {
                        // Clear tooltip
                        if (m_btnDelete.tooltipBox is not null)
                        {
                            m_btnDelete.tooltip = "";
                            m_btnDelete.tooltipBox.Hide();
                        }

                        BuildingSettingsStorage.ClearSettings(data.GetBuildingId());
                        SettingsPanel.Instance.InvalidatePanel();
                    }
                };
            }

            AfterStart();
        }

        protected override void Display()
        {
            tooltip = data.DescribeSettings();
            m_lblId.text = data.GetBuildingId().ToString();
            m_lblRestrictions.text = data.GetRestrctionCount().ToString();
            m_lblDescription.text = data.GetDescription();
            m_lblType.text = data.DescribeDistricts();
            m_btnDelete.tooltip = $"Delete settings for building: {data.GetBuildingId()}";
        }

        protected override void Clear()
        {
            m_lblId.text = "";
            m_lblRestrictions.text = "";
            m_lblDescription.text = "";
            m_lblType.text = "";
        }

        protected override void ClearTooltips()
        {
            tooltip = "";

            m_lblId.tooltip = "";
            m_lblRestrictions.tooltip = "";
            m_lblDescription.tooltip = "";
            m_lblType.tooltip = "";
            m_btnDelete.tooltip = "";
        }

        protected override void OnClicked(UIComponent component)
        {
            if (data is not null)
            {
                data.ShowInstance();

                if (SettingsPanel.Exists)
                {
                    SettingsPanel.Instance.InvalidatePanel();
                }
            }
        }

        protected override string GetTooltipText(UIComponent component)
        {
            if (data is not null)
            {
                return data.DescribeSettings();
            }
            return "";
        }

        protected override Color GetTextColor(UIComponent component, bool hightlightRow)
        {
            if (data is not null)
            {
                if (!hightlightRow && BuildingUtils.GetSelectedBuilding() == data.GetBuildingId())
                {
                    return KnownColor.lightBlue;
                }
            }

            return base.GetTextColor(component, hightlightRow);
        }
    }
}