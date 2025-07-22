using ColossalFramework;
using SleepyCommon;
using System;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    public class SelectionModeNormalDebug : SelectionModeNormal
    {
        // ----------------------------------------------------------------------------------------
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.None;
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.None;
        }
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.None;

        // ----------------------------------------------------------------------------------------
        public SelectionModeNormalDebug(SelectionTool tool) :
           base(tool)
        {
        }

        public override void RenderOverlay(CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);

            // Highlight net nodes
            if (BuildingPanel.IsVisible() && Input.GetKey(KeyCode.LeftControl)) 
            {
                ushort usSourceBuildingId = BuildingPanel.Instance.Building;
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
                            RendererUtils.HighlightNode(cameraInfo, node, Color.green);

                            nodeId = node.m_nextBuildingNode;

                            if (++iLoopCount > 32768)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }

                if (!ToolController.IsInsideUI)
                {
                    // Highlight hovered node and segment
                    if (SegmentId != 0)
                    {
                        RendererUtils.HighlightSegment(cameraInfo, ref NetManager.instance.m_segments.m_buffer[SegmentId], Color.blue, Color.blue);
                    }
                    if (NodeId != 0)
                    {
                        RendererUtils.HighlightNode(cameraInfo, NetManager.instance.m_nodes.m_buffer[NodeId], Color.green);
                    }
                }
            }
        }

        public override void Disable()
        {
            base.Disable();

            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.HideInfo();
            }
        }

        public override string GetTooltipText()
        {
            return string.Empty;
        }

        public string GetTooltipText2()
        {
            string sTooltip = "";

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (HoverInstance.Index != 0)
                {
                    sTooltip += InstanceHelper.DescribeInstance(HoverInstance, true, true);

                    if (HoverInstance.Building != 0)
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[HoverInstance.Building];
                        sTooltip += $"\n{BuildingTypeHelper.GetBuildingType(building)}";
                        sTooltip += $"\n{building.Info.GetAI()}";
                    }
                }

                if (NodeId != 0)
                {
                    NetNode oNode = NetManager.instance.m_nodes.m_buffer[NodeId];
                    if (oNode.m_flags != 0)
                    {
                        if (sTooltip.Length > 0)
                        {
                            sTooltip += "\n\n";
                        }

                        sTooltip += $"Node:{NodeId}";
                        sTooltip += $"\nClass:{oNode.Info.GetService()} | {oNode.Info.GetSubService()} | {oNode.Info.GetClassLevel()}";
                        if (oNode.Info.m_intersectClass is not null)
                        {
                            sTooltip += $"\nIntersectClass:{oNode.Info.m_intersectClass.m_service} | {oNode.Info.m_intersectClass.m_subService} | {oNode.Info.m_intersectClass.m_level}";
                        }
                        if (oNode.Info.m_connectionClass is not null)
                        {
                            sTooltip += $"\nConnectionClass:{oNode.Info.m_connectionClass.m_service} | {oNode.Info.m_connectionClass.m_subService} | {oNode.Info.m_connectionClass.m_level}";
                        }
                        sTooltip += $"\nLaneTypes:{oNode.Info.m_laneTypes}\nNetAI:{oNode.Info.GetAI()}";
                    }
                }
                if (SegmentId != 0)
                {
                    NetSegment segment = NetManager.instance.m_segments.m_buffer[SegmentId];
                    if (segment.m_flags != 0)
                    {
                        if (sTooltip.Length > 0)
                        {
                            sTooltip += "\n\n";
                        }

                        sTooltip += $"Segment:{SegmentId}";
                        sTooltip += $"\nClass:{segment.Info.GetService()} | {segment.Info.GetSubService()} | {segment.Info.GetClassLevel()}";
                        if (segment.Info.m_intersectClass is not null)
                        {
                            sTooltip += $"\nIntersectClass:{segment.Info.m_intersectClass.m_service} | {segment.Info.m_intersectClass.m_subService} | {segment.Info.m_intersectClass.m_level}";
                        }
                        if (segment.Info.m_connectionClass is not null)
                        {
                            sTooltip += $"\nConnectionClass:{segment.Info.m_connectionClass.m_service} | {segment.Info.m_connectionClass.m_subService} | {segment.Info.m_connectionClass.m_level}";
                        }
                        sTooltip += $"\nLaneTypes:{segment.Info.m_laneTypes}\nNetAI:{segment.Info.GetAI()}";
                    }
                }
            }

            return sTooltip;
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
