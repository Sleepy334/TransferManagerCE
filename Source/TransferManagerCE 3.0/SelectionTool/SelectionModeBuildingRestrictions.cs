using System.Collections.Generic;
using static TransferManagerCE.SelectionTool;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    public class SelectionModeBuildingRestrictions : SelectionModeBase
    {
        private bool m_bIncomingMode = true;
        private HashSet<ushort> m_buildingRestrictions = new HashSet<ushort>();

        public SelectionModeBuildingRestrictions(SelectionTool tool) : 
            base(tool) 
        {
        }

        public override void Enable(SelectionToolMode mode)
        {
            m_bIncomingMode = (mode == SelectionToolMode.BuildingRestrictionIncoming);
            UpdateBuildingSelection();
        }

        public override void Highlight(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            // Now highlight buildings
            foreach (ushort buildingId in m_buildingRestrictions)
            {
                RendererUtils.HighlightBuilding(BuildingBuffer, buildingId, cameraInfo, Color.green);
            }
        }

        public override void OnToolUpdate()
        {
            string sText = $"{Localization.Get("btnBuildingRestrictionsSelected")}\n";
            sText += "\n";
            sText += $"<color #00AA00>{Localization.Get("txtAllowedBuildings")}: {m_buildingRestrictions.Count}</color>\n";

            // Now describe buildings
            int iCount = 0;
            foreach (ushort buildingId in m_buildingRestrictions)
            {
                if (iCount < 10)
                {
                    sText += $"#{buildingId} {CitiesUtils.GetBuildingName(buildingId)}\n";
                }
                else if (iCount == 10)
                {
                    sText += "...";
                }

                iCount++;
            }

            m_tool.ShowToolInfo(sText);
        }

        public override void HandleLeftClick()
        {
            if (GetHoverInstance().Building != 0)
            {
                ushort buildingId = BuildingPanel.Instance.GetBuildingId();
                if (buildingId != 0 && buildingId != GetHoverInstance().Building)
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
                        if (allowedBuildings.Contains(GetHoverInstance().Building))
                        {
                            allowedBuildings.Remove(GetHoverInstance().Building);
                        }
                        else
                        {
                            allowedBuildings.Add(GetHoverInstance().Building);
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
                        if (BuildingPanel.Instance is not null && BuildingPanel.Instance.isVisible)
                        {
                            BuildingPanel.Instance.UpdateTabs();
                        }

                        // Update selection array
                        m_buildingRestrictions = allowedBuildings;
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
                        m_buildingRestrictions = restrictions.m_incomingBuildingSettings.GetBuildingRestrictionsCopy();
                    }
                    else
                    {
                        m_buildingRestrictions = restrictions.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                    }
                }
            }
        }
    }
}
