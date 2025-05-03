using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using TransferManagerCE.UI;
using static RenderManager;
using static ToolBase;
using static TransferManagerCE.SelectionTool;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using ICities;
using static TransferManagerCE.BuildingTypeHelper;
using System.Data;

namespace TransferManagerCE
{
    public class SelectionModeBase
    {
        private Color[] s_distanceColors = 
        { 
            Color.green, 
            Color.magenta, 
            Color.blue,
            Color.red,
            Color.white,
            Color.cyan,
        };

        protected SelectionTool m_tool;

        public SelectionModeBase(SelectionTool tool)
        {
            m_tool = tool;
        }

        public SelectionTool Tool
        {
            get { return m_tool; }
        }

        public ToolController ToolController
        {
            get { return m_tool.GetToolController(); }
        }

        public virtual void Enable(SelectionToolMode mode) { }

        public virtual void Disable() { }

        public virtual void RenderOverlay(CameraInfo cameraInfo)
        {
            HighlightSelectedBuilding(Singleton<ToolManager>.instance, cameraInfo);
        }

        public virtual bool RenderBuildingSelection()
        {
            return true;
        }

        public virtual RaycastInput GetRayCastInput(Ray ray, float rayCastLength)
        {
            return new RaycastInput(ray, rayCastLength);
        }

        public virtual void Highlight(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {

        }

        public virtual void OnToolGUI(Event e)
        {

        }

        public virtual void OnToolUpdate()
        { 
        }

        public virtual void OnToolLateUpdate()
        {
        }

        public virtual void UpdateSelection()
        {
        }

        public virtual void HandleLeftClick()
        {

        }

        protected InstanceID GetHoverInstance()
        {
            return m_tool.GetHoverInstance();
        }

        private void HighlightSelectedBuilding(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            // Highlight selected building
            if (BuildingPanel.Instance is not null && BuildingPanel.Instance.GetBuildingId() != 0)
            {
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
                Vehicle[] VehicleBuffer = VehicleManager.instance.m_vehicles.m_buffer;

                ushort usSourceBuildingId = BuildingPanel.Instance.GetBuildingId();
                if (usSourceBuildingId != 0)
                {
                    Building building = BuildingBuffer[usSourceBuildingId];
                    if (building.m_flags != 0)
                    {
                        // Highlight accessSegment first so it is underneath
                        if (building.m_accessSegment != 0)
                        {
                            ref NetSegment segment = ref NetManager.instance.m_segments.m_buffer[building.m_accessSegment];
                            if (segment.m_flags != 0)
                            {
                                NetTool.RenderOverlay(cameraInfo, ref segment, KnownColor.grey, KnownColor.grey);
                            }
                        }

                        BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                        HashSet<CustomTransferReason.Reason> localReasons = new HashSet<CustomTransferReason.Reason>();

                        // Highlight currently selected building
                        if (BuildingSettingsStorage.HasSettings(usSourceBuildingId))
                        {
                            RendererUtils.HighlightBuilding(BuildingBuffer, usSourceBuildingId, cameraInfo, Color.red);

                            // If building has distance restriction then draw the distance as a circle
                            DrawLocalDistanceCircle(cameraInfo, eType, usSourceBuildingId, building, ref localReasons);
                        }
                        else
                        {
                            RendererUtils.HighlightBuilding(BuildingBuffer, usSourceBuildingId, cameraInfo, Color.white);
                        }

                        // Draw global distance setting if any
                        DrawGlobalDistanceCircle(cameraInfo, eType, usSourceBuildingId, building, localReasons);

                        // Highlight own vehicles
                        BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                        {
                            RendererUtils.HighlightVehicle(VehicleBuffer, cameraInfo, vehicleId, Color.magenta);
                        });

                        // Highlight guest vehicles
                        BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                        {
                            RendererUtils.HighlightVehicle(VehicleBuffer, cameraInfo, vehicleId, Color.green);
                        });
                    }

                    Highlight(toolManager, cameraInfo);
                }
            }
        }

        public void DrawLocalDistanceCircle(CameraInfo cameraInfo, BuildingType eType, ushort buildingId, Building building, ref HashSet<CustomTransferReason.Reason> localReasons)
        {
            // If building has distance restriction then draw the distance as a circle
            BuildingSettings settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null)
            {
                foreach (KeyValuePair<int, RestrictionSettings> kvp in settings.m_restrictions)
                {
                    if (kvp.Value.m_iServiceDistanceMeters > 0)
                    {
                        HashSet<CustomTransferReason.Reason> reasons = BuildingRuleSets.GetRestrictionReasons(eType, kvp.Key);

                        // We need to exclude mail or mail2 depending on the MainBuildingPostTruck setting.
                        switch (eType)
                        {
                            case BuildingType.MainIndustryBuilding:
                            case BuildingType.AirportMainTerminal:
                            case BuildingType.AirportCargoTerminal:
                            case BuildingType.MainCampusBuilding:
                                {
                                    if (SaveGameSettings.GetSettings().MainBuildingPostTruck)
                                    {
                                        // Skip mail
                                        if (reasons.Contains(CustomTransferReason.Reason.Mail))
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // Skip mail2
                                        if (reasons.Contains(CustomTransferReason.Reason.Mail2))
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                }
                        }

                        // Add reasons for restriction
                        localReasons.UnionWith(reasons);

                        // Change color so we can see them if more than one (eg. Recycling Center).
                        Color color = s_distanceColors[kvp.Key];

                        RenderManager.instance.OverlayEffect.DrawCircle(
                                cameraInfo,
                                color,
                                building.m_position,
                                kvp.Value.m_iServiceDistanceMeters * 2.0f, // Range of service matching, we need diameter not radius
                                building.m_position.y - 1f,
                                building.m_position.y + 1f,
                                true,
                                true);
                    }
                }
            }
        }

        public void DrawGlobalDistanceCircle(CameraInfo cameraInfo, BuildingType eType, ushort buildingId, Building building, HashSet<CustomTransferReason.Reason> localReasons)
        {
            // Draw global distance setting if any
            if (SaveGameSettings.GetSettings().GetDistanceRestrictionCount() > 0)
            {
                CustomTransferReason.Reason reason = BuildingTypeHelper.GetGlobalDistanceReason(eType, buildingId);

                // Don't display global restrictiohn if we have a local one as it overrides the global one.
                if (reason != CustomTransferReason.Reason.None && !localReasons.Contains(reason))
                {
                    double dDistance = SaveGameSettings.GetSettings().GetActiveDistanceRestrictionSquaredMeters(reason);
                    if (dDistance > 0.0)
                    {
                        RenderManager.instance.OverlayEffect.DrawCircle(
                                cameraInfo,
                        Color.yellow,
                                building.m_position,
                                (float)Math.Sqrt(dDistance) * 2.0f, // Range of service matching, we need diameter not radius
                                building.m_position.y - 1f,
                                building.m_position.y + 1f,
                                true,
                                true);
                    }
                }
            }
        }
    }
}
