using ColossalFramework.UI;
using TransferManagerCE.Common;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.UI
{
    public class UIApplyToAll : UIPanel
    {
        // Apply to all
        private UILabel? m_labelApplyToAll = null;
        private UIButton? m_btnApplyToAllDistrict = null;
        private UIButton? m_btnApplyToAllPark = null;
        private UIButton? m_btnPaste = null;

        // Copy paste support
        private BuildingType m_eCopyPasteBuildingType = BuildingType.None;
        private BuildingSubType m_eCopyPasteSubBuildingType = BuildingSubType.None;
        private BuildingSettings? m_eCopyPasteSettings = null;

        public static UIApplyToAll Create(UIComponent parent)
        {
            UIApplyToAll applyToAll =  parent.AddUIComponent<UIApplyToAll>();
            applyToAll.Setup();
            return applyToAll;
        }

        public void Setup()
        {
            // Apply to all
            width = parent.width;
            height = 30;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoFitChildrenHorizontally = true;
            autoLayoutPadding = new RectOffset(0, 10, 0, 0);

            // Label
            m_labelApplyToAll = AddUIComponent<UILabel>();
            m_labelApplyToAll.verticalAlignment = UIVerticalAlignment.Middle;
            m_labelApplyToAll.textAlignment = UIHorizontalAlignment.Right;
            m_labelApplyToAll.text = Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL");
            m_labelApplyToAll.textScale = 0.9f;
            m_labelApplyToAll.autoSize = false;
            m_labelApplyToAll.height = 30;
            m_labelApplyToAll.width = 400;

            // Buttons
            m_btnApplyToAllDistrict = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, this, Localization.Get("btnDistrict"), "", 100, 30, OnApplyToAllDistrictClicked);
            m_btnApplyToAllPark = UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, this, Localization.Get("btnPark"), "", 100, 30, OnApplyToAllParkClicked);
            UIUtils.AddButton(UIUtils.ButtonStyle.DropDown, this, Localization.Get("btnMap"), "", 60, 30, OnApplyToAllWholeMapClicked);

            // Copy/Paste
            UIUtils.AddSpriteButton(UIUtils.ButtonStyle.DropDown, this, "CopyButtonIcon", Localization.Get("tooltipCopySettings"), TransferManagerLoader.LoadResources(), 30, 30, OnCopyClicked);
            m_btnPaste = UIUtils.AddSpriteButton(UIUtils.ButtonStyle.DropDown, this, "PasteButtonIcon", Localization.Get("tooltipPasteSettings"), TransferManagerLoader.LoadResources(), 30, 30, OnPasteClicked);
        }

        private ushort GetBuildingId()
        {
            if (BuildingPanel.Instance is not null)
            {
                return BuildingPanel.Instance.GetBuildingId();
            }
            return 0;
        }


        public void OnApplyToAllDistrictClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (CitiesUtils.IsSameDistrict(buildingId, (ushort)i) && BuildingTypeHelper.IsSameType(buildingId, (ushort)i))
                {
                    BuildingSettingsStorage.SetSettings((ushort)i, settings);
                }
            }
        }

        public void OnApplyToAllParkClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (CitiesUtils.IsSamePark(buildingId, (ushort)i) && BuildingTypeHelper.IsSameType(buildingId, (ushort)i))
                {
                    BuildingSettingsStorage.SetSettings((ushort)i, settings);
                }
            }
        }


        public void OnApplyToAllWholeMapClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (BuildingTypeHelper.IsSameType(buildingId, (ushort)i))
                {
                    BuildingSettingsStorage.SetSettings((ushort)i, settings);
                }
            }
        }

        public void OnCopyClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            if (buildingId != 0)
            {
                m_eCopyPasteBuildingType = BuildingTypeHelper.GetBuildingType(buildingId);
                m_eCopyPasteSubBuildingType = BuildingTypeHelper.GetBuildingSubType(buildingId);
                m_eCopyPasteSettings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
            }
        }

        public void OnPasteClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            if (buildingId != 0 && m_eCopyPasteSettings != null)
            {
                // This function takes a copy so we dont need to do this first
                BuildingSettingsStorage.SetSettings(buildingId, m_eCopyPasteSettings);

                if (BuildingPanel.Instance is not null)
                {
                    BuildingPanel.Instance.UpdateTabs();
                }
            }
        }

        public void UpdatePanel()
        {
            ushort buildingId = GetBuildingId();
            BuildingType eMainType = GetBuildingType(buildingId);
            BuildingSubType eSubType = GetBuildingSubType(buildingId);

            // Apply to all buttons
            if (m_btnApplyToAllDistrict is not null)
            {
                m_btnApplyToAllDistrict.isEnabled = CitiesUtils.IsInDistrict(buildingId);
                if (m_btnApplyToAllDistrict.isEnabled)
                {
                    m_btnApplyToAllDistrict.tooltip = CitiesUtils.GetDistrictName(buildingId);
                }
                else
                {
                    m_btnApplyToAllDistrict.tooltip = "";
                }
            }
            if (m_btnApplyToAllPark is not null)
            {
                m_btnApplyToAllPark.isEnabled = CitiesUtils.IsInPark(buildingId);
                if (m_btnApplyToAllPark.isEnabled)
                {
                    m_btnApplyToAllPark.tooltip = CitiesUtils.GetParkName(buildingId);
                }
                else
                {
                    m_btnApplyToAllPark.tooltip = "";
                }
            }

            if (m_labelApplyToAll is not null)
            {
                string sTypeDescription;

                
                if (eSubType != BuildingSubType.None)
                {
                    sTypeDescription = eSubType.ToString();
                }
                else
                {
                    sTypeDescription = eMainType.ToString();
                }

                m_labelApplyToAll.text = Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL") + ": " + sTypeDescription;
            }

            if (m_btnPaste != null)
            {
                m_btnPaste.isEnabled = (eMainType == m_eCopyPasteBuildingType && m_eCopyPasteSubBuildingType == eSubType);
            }
        }
    }
}