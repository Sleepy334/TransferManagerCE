using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class UIDistrictRow : UIPanel, IUIFastListRow
    {
        private UILabel? m_lblDescription = null;
        private UIButton? m_btnDelete = null;
        private DistrictData? m_data = null;

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
            eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            eventMouseLeave += new MouseEventHandler(OnMouseLeave);

            float fBUTTON_HEIGHT = height - 6;

            m_lblDescription = AddUIComponent<UILabel>();
            if (m_lblDescription is not null)
            {
                m_lblDescription.name = "m_lblDescription";
                m_lblDescription.text = "";
                m_lblDescription.textScale = BuildingPanel.fTEXT_SCALE;
                m_lblDescription.tooltip = "";
                m_lblDescription.textAlignment = UIHorizontalAlignment.Left;
                m_lblDescription.verticalAlignment = UIVerticalAlignment.Middle;
                m_lblDescription.autoSize = false;
                m_lblDescription.height = height;
                m_lblDescription.width = width - fBUTTON_HEIGHT - 20;
                m_lblDescription.eventClicked += new MouseEventHandler(OnItemClicked);
            }

            m_btnDelete = AddUIComponent<UIButton>();
            if (m_btnDelete is not null)
            {
                m_btnDelete.height = fBUTTON_HEIGHT;
                m_btnDelete.width = fBUTTON_HEIGHT;
                m_btnDelete.normalBgSprite = "buttonclose";
                m_btnDelete.hoveredBgSprite = "buttonclosehover";
                m_btnDelete.pressedBgSprite = "buttonclosepressed";
                m_btnDelete.tooltip = Localization.Get("btnClear");
                m_btnDelete.eventClick += OnDeleteClicked;
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public void Display(int index, object data, bool isRowOdd)
        {
            m_data = (DistrictData?)data;

            if (m_lblDescription is null)
            {
                return;
            }

            if (m_data is not null)
            {
                m_lblDescription.text = m_data.GetDistrictName();
                m_lblDescription.textColor = GetTextColor();
            }
            else
            {
                Clear();
            }
        }

        public void Disabled()
        {
            Clear();
        }

        public void Clear()
        {
            m_data = null;

            if (m_lblDescription is not null)
            {
                m_lblDescription.text = "";
                tooltip = "";
                m_lblDescription.tooltip = "";
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
                if (m_data.m_eType == DistrictData.DistrictType.District)
                {
                    InstanceHelper.ShowInstance(new InstanceID { District = (byte) m_data.m_iDistrictId }); 
                }
                else
                {
                    InstanceHelper.ShowInstance(new InstanceID { Park = (byte)m_data.m_iDistrictId });
                }
            }
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UIComponent c in components)
            {
                if (c is UILabel label)
                {
                    label.textColor = Color.yellow;
                }
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (UIComponent c in components)
            {
                if (c is UILabel label)
                {
                    label.textColor = GetTextColor();
                }
            }
        }

        public void OnDeleteClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_data is not null)
            {
                ushort buildingId = DistrictSelectionPanel.Instance.m_buildingId;
                int iRestrictionId = DistrictSelectionPanel.Instance.m_iRestrictionId;
                bool bIncoming = DistrictSelectionPanel.Instance.m_bIncoming;

                BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(iRestrictionId);

                if (bIncoming)
                {
                    restrictions.m_incomingDistrictSettings.SetDistrictAllowed(buildingId, m_data.m_eType, m_data.m_iDistrictId, false);
                }
                else
                {
                    restrictions.m_outgoingDistrictSettings.SetDistrictAllowed(buildingId, m_data.m_eType, m_data.m_iDistrictId, false);
                }

                settings.SetRestrictions(iRestrictionId, restrictions);
                BuildingSettingsStorage.SetSettings(buildingId, settings);

                // Update panel to reflect the change
                DistrictSelectionPanel.Instance.InvalidatePanel();
            }
        }

        private Color GetTextColor()
        {
            if (m_data != null)
            {
                if (m_data.IsDistrict())
                {
                    return KnownColor.white;
                }
                else
                {
                    return KnownColor.cyan;
                }
            }

            return KnownColor.white;
        }
    }
}