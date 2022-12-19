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
            m_btnApplyToAllDistrict = UIUtils.AddButton(this, Localization.Get("btnDistrict"), 100, 30, OnApplyToAllDistrictClicked);
            m_btnApplyToAllPark = UIUtils.AddButton(this, Localization.Get("btnPark"), 100, 30, OnApplyToAllParkClicked);
            UIUtils.AddButton(this, Localization.Get("btnMap"), 60, 30, OnApplyToAllWholeMapClicked);
        }

        private ushort GetBuildingId()
        {
            if (BuildingPanel.Instance != null)
            {
                return BuildingPanel.Instance.GetBuildingId();
            }
            return 0;
        }


        public void OnApplyToAllDistrictClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort buildingId = GetBuildingId();
            BuildingSettings settings = BuildingSettingsStorage.GetSettings(buildingId);

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
            BuildingSettings settings = BuildingSettingsStorage.GetSettings(buildingId);

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
            BuildingSettings settings = BuildingSettingsStorage.GetSettings(buildingId);

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; ++i)
            {
                if (BuildingTypeHelper.IsSameType(buildingId, (ushort)i))
                {
                    BuildingSettingsStorage.SetSettings((ushort)i, settings);
                }
            }
        }

        public void UpdatePanel()
        {
            ushort buildingId = GetBuildingId();

            // Apply to all buttons
            if (m_btnApplyToAllDistrict != null)
            {
                m_btnApplyToAllDistrict.isEnabled = CitiesUtils.IsInDistrict(buildingId);
            }
            if (m_btnApplyToAllPark != null)
            {
                m_btnApplyToAllPark.isEnabled = CitiesUtils.IsInPark(buildingId);
            }

            if (m_labelApplyToAll != null)
            {
                string sTypeDescription;

                BuildingSubType eSubType = GetBuildingSubType(buildingId);
                if (eSubType != BuildingSubType.None)
                {
                    sTypeDescription = eSubType.ToString();
                }
                else
                {
                    BuildingType eMainType = GetBuildingType(buildingId); 
                    sTypeDescription = eMainType.ToString();
                }

                m_labelApplyToAll.text = Localization.Get("GROUP_BUILDINGPANEL_APPLYTOALL") + ": " + sTypeDescription;
            }
        }
    }
}