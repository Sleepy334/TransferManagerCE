using ColossalFramework.UI;
using SleepyCommon;
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
        private UIButton? m_btnClear = null;

        // Copy paste support
        private BuildingType m_eCopyPasteBuildingType = BuildingType.None;
        private BuildingSubType m_eCopyPasteSubBuildingType = BuildingSubType.None;
        private BuildingSettings? m_eCopyPasteSettings = null;

        public static UIApplyToAll Create(UIComponent parent)
        {
            UIApplyToAll applyToAll = parent.AddUIComponent<UIApplyToAll>();
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
            autoLayoutPadding = new RectOffset(10, 0, 0, 0);

            // Copy/Paste
            UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, this, "CopyButtonIcon", Localization.Get("tooltipCopySettings"), TransferManagerMod.Instance.LoadResources(), 30, 30, OnCopyClicked);
            m_btnPaste = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, this, "PasteButtonIcon", Localization.Get("tooltipPasteSettings"), TransferManagerMod.Instance.LoadResources(), 30, 30, OnPasteClicked);

            // Clear
            m_btnClear = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, this, "Niet", Localization.Get("tooltipClearSettings"), atlas, 30, 30, OnClearClicked);

            // Separator Panel
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.width = 40;// 210;

            // Label
            m_labelApplyToAll = AddUIComponent<UILabel>();
            m_labelApplyToAll.verticalAlignment = UIVerticalAlignment.Middle;
            m_labelApplyToAll.textAlignment = UIHorizontalAlignment.Left;
            m_labelApplyToAll.text = Localization.Get("txtApplyToBuilding");
            m_labelApplyToAll.textScale = 0.9f;
            m_labelApplyToAll.autoSize = false;
            m_labelApplyToAll.height = 30;
            m_labelApplyToAll.width = 150;

            // Buttons
            m_btnApplyToAllDistrict = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, this, Localization.Get("btnDistrict"), "", 100, 30, OnApplyToAllDistrictClicked);
            m_btnApplyToAllPark = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, this, Localization.Get("btnPark"), "", 100, 30, OnApplyToAllParkClicked);
            UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, this, Localization.Get("btnMap"), "", 60, 30, OnApplyToAllWholeMapClicked);
        }

        private ushort GetBuildingId()
        {
            if (BuildingPanel.Exists)
            {
                return BuildingPanel.Instance.Building;
            }
            return 0;
        }


        public void OnApplyToAllDistrictClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (CitiesUtils.IsSameDistrict(buildingId, (ushort)i) && IsSameType(buildingId, (ushort)i))
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
                if (CitiesUtils.IsSamePark(buildingId, (ushort)i) && IsSameType(buildingId, (ushort)i))
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
                if (IsSameType(buildingId, (ushort)i))
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
                m_eCopyPasteBuildingType = GetBuildingType(buildingId);
                m_eCopyPasteSubBuildingType = GetBuildingSubType(buildingId);
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

                if (BuildingPanel.IsVisible())
                {
                    BuildingPanel.Instance.InvalidatePanel();
                }
            }
        }

        public void OnClearClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            if (buildingId != 0)
            {
                BuildingSettingsStorage.ClearSettings(buildingId);

                if (BuildingPanel.IsVisible())
                {
                    BuildingPanel.Instance.InvalidatePanel();
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

                m_labelApplyToAll.tooltip = Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL") + ": " + sTypeDescription;
            }

            if (m_btnPaste != null)
            {
                m_btnPaste.isEnabled = eMainType == m_eCopyPasteBuildingType && m_eCopyPasteSubBuildingType == eSubType;
            }

            if (m_btnClear != null)
            {
                m_btnClear.isEnabled = BuildingSettingsStorage.HasSettings(buildingId);
            }
        }
    }
}