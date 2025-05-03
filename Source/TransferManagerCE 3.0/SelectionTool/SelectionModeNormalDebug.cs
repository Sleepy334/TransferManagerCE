using ColossalFramework;
using ColossalFramework.Math;
using System;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;
using static ToolBase;

namespace TransferManagerCE
{
    public class SelectionModeNormalDebug : SelectionModeNormal
    {
        public SelectionModeNormalDebug(SelectionTool tool) :
           base(tool)
        {
        }

        public override RaycastInput GetRayCastInput(Ray ray, float rayCastLength)
        {
            // We change the ray cast input so we can highlight nodes and segments.
            // But it makes selecting buildings harder when the segments overlap the building.
            RaycastInput input = new RaycastInput(ray, rayCastLength);
            input.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.None);
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreTerrain = true;
            return input;
        }

        public override void Highlight(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            base.Highlight(toolManager, cameraInfo);

            // Highlight net nodes
            ushort usSourceBuildingId = BuildingPanel.Instance.GetBuildingId();
            if (usSourceBuildingId != 0)
            {
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

                Building building = BuildingBuffer[usSourceBuildingId];
                if (building.m_flags != 0)
                {
                    int iLoopCount = 0;
                    ushort nodeId = building.m_netNode;
                    while (nodeId != 0)
                    {
                        NetNode node = NetManager.instance.m_nodes.m_buffer[nodeId];
                        HighlightNode(toolManager, cameraInfo, node, Color.green);

                        nodeId = node.m_nextBuildingNode;

                        if (++iLoopCount > 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            // Highlight hovered node/segment
            if (GetHoverInstance().NetNode != 0)
            {
                HighlightNode(toolManager, cameraInfo, NetManager.instance.m_nodes.m_buffer[GetHoverInstance().NetNode], Color.blue);
            }
            else if (GetHoverInstance().NetSegment != 0)
            {
                HighlightSegment(cameraInfo, ref NetManager.instance.m_segments.m_buffer[GetHoverInstance().NetSegment], Color.blue, Color.blue);
            }
        }

        public override void OnToolGUI(Event e)
        {
            base.OnToolGUI(e);

            DisplayNodeInformation();
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            switch (GetHoverInstance().Type)
            {
                case InstanceType.Building:
                    {
                        m_tool.SelectBuilding(GetHoverInstance().Building);
                        break;
                    }
                case InstanceType.NetNode:
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[GetHoverInstance().NetNode];
                        if (oNode.m_building != 0)
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[oNode.m_building];
                            if (building.Info?.GetAI() is OutsideConnectionAI)
                            {
                                m_tool.SelectBuilding(oNode.m_building);
                            }
                        }
                        break;
                    }
            }
        }

        private static void HighlightNode(ToolManager toolManager, CameraInfo cameraInfo, NetNode oNode, Color color)
        {
            RenderManager.instance.OverlayEffect.DrawCircle(
                                            cameraInfo,
                                            color,
                                            oNode.m_position,
                                            oNode.m_bounds.size.magnitude,
                                            oNode.m_position.y - 1f,
                                            oNode.m_position.y + 1f,
                                            true,
                                            true);
            
            toolManager.m_drawCallData.m_overlayCalls++;
        }

        public static void HighlightSegment(RenderManager.CameraInfo cameraInfo, ref NetSegment segment, Color importantColor, Color nonImportantColor)
        {
            NetInfo info = segment.Info;
            if (!(info == null) && ((segment.m_flags & NetSegment.Flags.Untouchable) == 0 || info.m_overlayVisible))
            {
                Bezier3 bezier = default(Bezier3);
                bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[segment.m_startNode].m_position;
                bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[segment.m_endNode].m_position;
                NetSegment.CalculateMiddlePoints(bezier.a, segment.m_startDirection, bezier.d, segment.m_endDirection, smoothStart: false, smoothEnd: false, out bezier.b, out bezier.c);
                bool flag = false;
                bool flag2 = false;
                int privateServiceIndex = ItemClass.GetPrivateServiceIndex(info.m_class.m_service);
                Color color = (((privateServiceIndex == -1 && !info.m_autoRemove) || (segment.m_flags & NetSegment.Flags.Untouchable) != 0) ? importantColor : nonImportantColor);
                Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, color, bezier, info.m_halfWidth * 2f, (!flag) ? (-100000f) : info.m_halfWidth, (!flag2) ? (-100000f) : info.m_halfWidth, -1f, 1280f, renderLimits: false, alphaBlend: false);
            }
        }

        private void DisplayNodeInformation()
        {
            // If node connection information is on then display information about node under mouse.
            switch (GetHoverInstance().Type)
            {
                case InstanceType.NetNode:
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[GetHoverInstance().NetNode];
                        if (oNode.m_flags != 0)
                        {
                            var text = $"Node:{GetHoverInstance().NetNode}";
                            text += $"\nClass:{oNode.Info.GetService()} | {oNode.Info.GetSubService()} | {oNode.Info.GetClassLevel()}";
                            if (oNode.Info.m_intersectClass is not null)
                            {
                                text += $"\nIntersectClass:{oNode.Info.m_intersectClass.m_service} | {oNode.Info.m_intersectClass.m_subService} | {oNode.Info.m_intersectClass.m_level}";
                            }
                            if (oNode.Info.m_connectionClass is not null)
                            {
                                text += $"\nConnectionClass:{oNode.Info.m_connectionClass.m_service} | {oNode.Info.m_connectionClass.m_subService} | {oNode.Info.m_connectionClass.m_level}";
                            }
                            text += $"\nLaneTypes:{oNode.Info.m_laneTypes}\nNetAI:{oNode.Info.GetAI()}";

                            var screenPoint = SelectionTool.MousePosition;
                            screenPoint.y = screenPoint.y - 40f;
                            var color = GUI.color;
                            GUI.color = Color.white;
                            DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
                            GUI.color = color;
                        }
                        break;
                    }
                case InstanceType.NetSegment:
                    {
                        NetSegment segment = NetManager.instance.m_segments.m_buffer[GetHoverInstance().NetSegment];
                        if (segment.m_flags != 0)
                        {
                            var text = $"Segment:{GetHoverInstance().NetSegment}";
                            text += $"\nClass:{segment.Info.GetService()} | {segment.Info.GetSubService()} | {segment.Info.GetClassLevel()}";
                            if (segment.Info.m_intersectClass is not null)
                            {
                                text += $"\nIntersectClass:{segment.Info.m_intersectClass.m_service} | {segment.Info.m_intersectClass.m_subService} | {segment.Info.m_intersectClass.m_level}";
                            }
                            if (segment.Info.m_connectionClass is not null)
                            {
                                text += $"\nConnectionClass:{segment.Info.m_connectionClass.m_service} | {segment.Info.m_connectionClass.m_subService} | {segment.Info.m_connectionClass.m_level}";
                            }
                            text += $"\nLaneTypes:{segment.Info.m_laneTypes}\nNetAI:{segment.Info.GetAI()}";

                            var screenPoint = SelectionTool.MousePosition;
                            screenPoint.y = screenPoint.y - 40f;
                            var color = GUI.color;
                            GUI.color = Color.white;
                            DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
                            GUI.color = color;
                        }
                        break;
                    }
            }
        }
    }
}
