using System.Collections.Generic;
using static TransferManagerCE.SelectionTool;
using TransferManagerCE.UI;
using UnityEngine;
using SleepyCommon;

namespace TransferManagerCE
{
    public class SelectionModeBuildingRestrictions : SelectionModeSelectBuildings
    {
        private bool m_bIncomingMode = true;

        public SelectionModeBuildingRestrictions(SelectionTool tool) : 
            base(tool) 
        {
        }

        // ----------------------------------------------------------------------------------------
        public override void Enable()
        {
            m_bIncomingMode = (Tool.GetCurrentMode() == SelectionToolMode.BuildingRestrictionIncoming);
            UpdateBuildingSelection();
        }

        public override void Disable()
        {
            base.Disable();

            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.HideInfo();
                BuildingPanel.Instance.InvalidatePanel();
            }
        }

        protected override Color GetColor()
        {
            return Color.green;
        }

        public override void HandleLeftClick()
        {
            if (HoverInstance.Building != 0)
            {
                ushort buildingId = BuildingPanel.Instance.GetBuildingId();
                if (buildingId != 0 && buildingId != HoverInstance.Building)
                {
                    int restrictionId = BuildingPanel.Instance.GetRestrictionId();
                    if (restrictionId != -1)
                    {
                        BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
                        RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(restrictionId);

                        // Get correct array
                        HashSet<ushort> allowedBuildings;
                        if (m_bIncomingMode)
                        {
                            allowedBuildings = restrictions.m_incomingBuildingSettings.GetBuildingRestrictionsCopy();
                        }
                        else
                        {
                            allowedBuildings = restrictions.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                        }

                        // Add or remove building
                        if (allowedBuildings.Contains(HoverInstance.Building))
                        {
                            allowedBuildings.Remove(HoverInstance.Building);
                        }
                        else
                        {
                            allowedBuildings.Add(HoverInstance.Building);
                        }

                        // Update settings
                        if (m_bIncomingMode)
                        {
                            restrictions.m_incomingBuildingSettings.SetBuildingRestrictions(allowedBuildings);
                        }
                        else
                        {
                            restrictions.m_outgoingBuildingSettings.SetBuildingRestrictions(allowedBuildings);
                        }

                        // Now update settings
                        settings.SetRestrictions(restrictionId, restrictions);
                        BuildingSettingsStorage.SetSettings(buildingId, settings);

                        // Update tab to reflect selected building
                        if (BuildingPanel.IsVisible())
                        {
                            BuildingPanel.Instance.UpdateTabs();
                        }

                        // Update selection array
                        m_buildings = allowedBuildings;
                    }
                }
            }
        }

        private void UpdateBuildingSelection()
        {
            ushort buildingId = BuildingPanel.Instance.GetBuildingId();
            if (buildingId != 0 && buildingId != m_tool.GetHoverInstance().Building)
            {
                int restrictionId = BuildingPanel.Instance.GetRestrictionId();
                if (restrictionId != -1)
                {
                    BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
                    RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(restrictionId);

                    // Get correct array
                    if (m_bIncomingMode)
                    {
                        m_buildings = restrictions.m_incomingBuildingSettings.GetBuildingRestrictionsCopy();
                    }
                    else
                    {
                        m_buildings = restrictions.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                    }
                }
            }
        }

        public override string GetTooltipText2()
        {
            string sText = "";
            sText += $"<color #FFFFFF>{Localization.Get("txtSelectBuildings")}</color>\n";
            sText += "\n";
            sText += $"<color #FFFFFF>{Localization.Get("txtAllowedBuildings")}: {m_buildings.Count}</color>\n";
            return sText + base.GetTooltipText2();
        }

        public override void OnToolLateUpdate()
        {
            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.ShowInfo(GetTooltipText2());
            }
        }
    }
}
