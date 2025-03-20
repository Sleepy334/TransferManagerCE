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
                                NetTool.RenderOverlay(cameraInfo, ref segment, s_accessSegmentColor, s_accessSegmentColor);
                            }
                        }

                        BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                        HashSet<CustomTransferReason.Reason> localReasons = new HashSet<CustomTransferReason.Reason>();

                        // Highlight currently selected building
                        if (BuildingSettingsStorage.HasSettings(usSourceBuildingId))
                        {
                            HighlightBuilding(toolManager, BuildingBuffer, usSourceBuildingId, cameraInfo, Color.red);

                            // If building has distance restriction then draw the distance as a circle
                            BuildingSettings settings = BuildingSettingsStorage.GetSettings(usSourceBuildingId);
                            if (settings is not null)
                            {
                                foreach (KeyValuePair<int, RestrictionSettings> kvp in settings.m_restrictions)
                                {
                                    if (kvp.Value.m_iServiceDistanceMeters > 0)
                                    {
                                        // Add reasons for restriction
                                        localReasons.UnionWith(BuildingRuleSets.GetRestrictionReasons(eType, kvp.Key));

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
                        else
                        {
                            HighlightBuilding(toolManager, BuildingBuffer, usSourceBuildingId, cameraInfo, Color.white);
                        }

                        // Draw global distance setting if any
                        if (SaveGameSettings.GetSettings().GetDistanceRestrictionCount() > 0)
                        {
                            CustomTransferReason.Reason reason = BuildingTypeHelper.GetGlobalDistanceReason(eType);

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

                    Highlight(toolManager, cameraInfo);
                }
            }
        }

        public static void HighlightBuilding(ToolManager toolManager, Building[] BuildingBuffer, ushort usBuildingId, RenderManager.CameraInfo cameraInfo, UnityEngine.Color color)
        {
            ref Building building = ref BuildingBuffer[usBuildingId];
            if (building.m_flags != 0)
            {
                // Highlight building path
                if (building.Info is not null)
                {
                    building.Info.m_buildingAI.RenderBuildOverlay(cameraInfo, color, building.m_position, building.m_angle, default(Segment3));
                }

                // Highlight building
                BuildingTool.RenderOverlay(cameraInfo, ref building, color, color);

                // Also highlight any sub buildings
                float m_angle = building.m_angle * 57.29578f;
                BuildingInfo info3 = building.Info;
                if (info3 is not null && info3.m_subBuildings is not null && info3.m_subBuildings.Length != 0)
                {
                    // Render sub buildings
                    Matrix4x4 matrix4x = default(Matrix4x4);
                    matrix4x.SetTRS(building.m_position, Quaternion.AngleAxis(m_angle, Vector3.down), Vector3.one);
                    for (int i = 0; i < info3.m_subBuildings.Length; i++)
                    {
                        BuildingInfo buildingInfo = info3.m_subBuildings[i].m_buildingInfo;
                        Vector3 position = matrix4x.MultiplyPoint(info3.m_subBuildings[i].m_position);
                        float angle = (info3.m_subBuildings[i].m_angle + m_angle) * ((float)Math.PI / 180f);
                        BuildingTool.RenderOverlay(cameraInfo, buildingInfo, 0, position, angle, color, radius: false);
                    }
                }
            }
        }
    }
}
