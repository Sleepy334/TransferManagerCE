using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;
using static TransferManagerCE.SelectionTool;

namespace TransferManagerCE
{
    public class SelectionModeBase
    {
        protected SelectionTool m_tool;

        public SelectionModeBase(SelectionTool tool) 
        {
            m_tool = tool;
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

        public virtual void Highlight(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {

        }

        public virtual void OnToolGUI(Event e)
        {

        }

        public virtual void OnToolUpdate()
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

                        // Highlight currently selected building
                        if (BuildingSettingsStorage.HasSettings(usSourceBuildingId))
                        {
                            HighlightBuilding(toolManager, BuildingBuffer, usSourceBuildingId, cameraInfo, Color.red);
                        }
                        else
                        {
                            HighlightBuilding(toolManager, BuildingBuffer, usSourceBuildingId, cameraInfo, Color.white);
                        }
                    }

                    Highlight(toolManager, cameraInfo);
                }
            }
        }

        public static void HighlightBuilding(ToolManager toolManager, Building[] BuildingBuffer, ushort usBuildingId, RenderManager.CameraInfo cameraInfo, Color color)
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
