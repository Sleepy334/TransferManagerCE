using Epic.OnlineServices.Presence;
using ICities;
using SleepyCommon;
using TransferManagerCE.UI;
using UnityEngine;
using UnityEngine.Networking.Types;
using static RenderManager;
using static TransferManagerCE.NodeLinkData;

namespace TransferManagerCE
{
    public class SelectionModePathDistance : SelectionModeBase
    {
        // ----------------------------------------------------------------------------------------
        public SelectionModePathDistance(SelectionTool tool) :
           base(tool)
        {
        }

        // ----------------------------------------------------------------------------------------
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.None;
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.None;
        }
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.All;

        // ----------------------------------------------------------------------------------------
        public override void Enable()
        {
            base.Enable();
            PathDistancePanel.Instance.InvalidatePanel();
        }

        public override void Disable()
        {
            base.Disable();

            if (PathDistancePanel.IsVisible())
            {
                PathDistancePanel.Instance.ShowInfo(string.Empty);
                PathDistancePanel.Instance.InvalidatePanel();
            }
        }

        public override void RenderOverlay(CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);

            if (!ToolController.IsInsideUI)
            {
                // Highlight path
                HighlightNodeLinks(cameraInfo);

                if (Tool.m_nodeId != 0)
                {
                    RendererUtils.HighlightNode(cameraInfo, NetManager.instance.m_nodes.m_buffer[Tool.m_nodeId], Color.blue);
                }
            }
        }

        public override void OnSelectBuilding(ushort buildingId) 
        {
            if (BuildingTypeHelper.GetBuildingType(buildingId) == BuildingTypeHelper.BuildingType.CargoWarehouse)
            {
                // Toggle building / sub building
                if (PathDistancePanel.Instance.Building == buildingId)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                    if (building.m_subBuilding != 0)
                    {
                        PathDistancePanel.Instance.Building = building.m_subBuilding;
                    }
                }
                else
                {
                    PathDistancePanel.Instance.Building = buildingId;
                }
            }
            else
            {
                PathDistancePanel.Instance.SetBuilding(buildingId);
            }
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            switch (HoverInstance.Type)
            {
                case InstanceType.Building:
                    {
                        m_tool.OnSelectBuilding(HoverInstance.Building);
                        break;
                    }
            }
        }

        private void HighlightNodeLinks(RenderManager.CameraInfo cameraInfo)
        {
            if (Tool.m_nodeId != 0)
            {
                NodeLinkGraph nodeLinks = PathDistanceCache.GetLoader(NetworkModeHelper.NetworkMode.Goods, true);

                // Highlight node links
                if (nodeLinks.TryGetNodeLinks(Tool.m_nodeId, out NodeLinkData nodeLinkData))
                {
                    foreach (NodeLink nodeLink in nodeLinkData.items)
                    {
                        Color color = KnownColor.cyan;
                        if (nodeLink.IsBypassNode())
                        {
                            color = KnownColor.darkGreen;
                        }

                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeLink.m_nodeId];

                        RenderManager.instance.OverlayEffect.DrawCircle(
                        cameraInfo,
                                color,
                                oNode.m_position,
                                oNode.m_bounds.size.magnitude,
                                oNode.m_position.y - 1f,
                                oNode.m_position.y + 1f,
                                true,
                                true);
                    }
                }
            }
        }

        public override string GetTooltipText()
        {
            return "";
        }

        public string GetTooltipText2()
        {
            string sTooltip = "";

            if (Tool.m_nodeId != 0)
            {
                NetNode node = NetManager.instance.m_nodes.m_buffer[Tool.m_nodeId];

                sTooltip += $"<color #FFFFFF>Node: {Tool.m_nodeId}</color>";
                sTooltip += $"\nService: {node.Info.GetService()}";
                sTooltip += $"\nLane Types: {node.Info.m_laneTypes}";
                sTooltip += $"\nCross Lanes: {((node.Info.m_canCrossLanes) ? "Yes" : "No")}";

                // Add connection group
                int iColor = PathConnectedCache.GetNodeColor(PathDistancePanel.Instance.Algorithm, Tool.m_nodeId);
                if (iColor != 0)
                {
                    sTooltip += $"\nConnection Group: {iColor}";
                }

                // Add node links to tooltip
                NodeLinkGraph nodeLinks = PathDistanceCache.GetLoader(PathDistancePanel.Instance.Algorithm, true); // Force an update if needed
                NodeLinkData data = nodeLinks.GetNodeLinks(Tool.m_nodeId);
                if (data.Count > 0)
                {
                    sTooltip += $"{data}";
                }

                // Is it in node path
                if (Tool.m_nodeId == PathDistancePanel.Instance.Test.StartNodeId)
                {
                    sTooltip += $"\n\nStart Node: {Tool.m_nodeId}";
                }
                else if (PathDistancePanel.Instance.Test.GetExaminedNodes().TryGetValue(Tool.m_nodeId, out PathData nodeData))
                {
                    NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeData.nodeId];
                    sTooltip += "\n\nPath Distance:";
                    if (nodeData.visited)
                    {
                        sTooltip += $"\nVisited";
                    }
                    else
                    {
                        sTooltip += $"\nDiscovered";
                    }
                    sTooltip += $"\nTravel Time: {nodeData.TravelTime().ToString("F")}";
                    sTooltip += $"\nHeuristic:{nodeData.Heuristic().ToString("F")}";
                    sTooltip += $"\nPriority: {nodeData.Priority}";

                    // Node and segment to get here
                    sTooltip += $"\nPrevious Node: {nodeData.prevId}";
                    ushort actualNodeId = data.GetActualNode(nodeData.prevId);
                    if (actualNodeId != nodeData.prevId)
                    {
                        sTooltip += $"\nBypass Node: {actualNodeId}";
                    }

#if DEBUG
                    ushort segmentId = GetTravelSegment(Tool.m_nodeId, actualNodeId);
                    if (segmentId != 0)
                    {
                        NetSegment segment = NetManager.instance.m_segments.m_buffer[segmentId];
                        sTooltip += $"\n\nSegment: {segmentId}";
                        sTooltip += $"\nStart Node: {segment.m_startNode}";
                        sTooltip += $"\nEnd Node: {segment.m_endNode}";
                        sTooltip += $"\nStartLeftSegment: {segment.m_startLeftSegment}";
                        sTooltip += $"\nStartRightSegment: {segment.m_startRightSegment}";
                        sTooltip += $"\nEndLeftSegment: {segment.m_endLeftSegment}";
                        sTooltip += $"\nEndRightSegment: {segment.m_endRightSegment}";

                        sTooltip += $"\n\nLanes:";
                        uint laneId = segment.m_lanes;
                        int iLoopCount = 0;
                        while (laneId != 0)
                        {
                            NetLane lane = NetManager.instance.m_lanes.m_buffer[laneId];
                            sTooltip += $"\nLane: {laneId} Flags: #{lane.m_flags} : {DecodeLaneFlags(lane.m_flags)}";

                            laneId = lane.m_nextLane;

                            // Safety check in case we get caught in an infinite loop somehow
                            if (iLoopCount++ > NetManager.MAX_LANE_COUNT)
                            {
                                Debug.Log("Invalid segment lane loop detected");
                                break;
                            }
                        }
                    }
#endif
                }
            }

            return sTooltip;
        }

        public override void OnToolLateUpdate()
        {
            if (PathDistancePanel.IsVisible())
            {
                PathDistancePanel.Instance.ShowInfo(GetTooltipText2());
            }
        }

        private ushort GetTravelSegment(int startNode,  int endNode)
        {
            NetNode node = NetManager.instance.m_nodes.m_buffer[endNode];

            // Loop through segments to find neighboring roads
            for (int i = 0; i < 8; ++i)
            {
                ushort segmentId = node.GetSegment(i);
                if (segmentId != 0)
                {
                    NetSegment segment = NetManager.instance.m_segments.m_buffer[segmentId];
                    if ((segment.m_startNode == startNode || segment.m_endNode == startNode) &&
                        (segment.m_startNode == endNode || segment.m_endNode == endNode))
                    {
                        return segmentId;
                    }
                }
            }

            return 0;
        }

        private string DecodeLaneFlags(ushort flags)
        {
            string text = string.Empty;

            if ((flags & (ushort) NetLane.Flags.Left) == (ushort) NetLane.Flags.Left)
            {
                text += "Left, ";
            }

            if ((flags & (ushort)NetLane.Flags.Right) == (ushort)NetLane.Flags.Right)
            {
                text += "Right, ";
            }

            if ((flags & (ushort)NetLane.Flags.Forward) == (ushort)NetLane.Flags.Forward)
            {
                text += "Forward, ";
            }

            return text;
        }
    }
}
