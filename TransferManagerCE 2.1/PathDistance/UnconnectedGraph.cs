using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TransferManagerCE
{
    public class UnconnectedGraph
    {
        public ItemClass.Service m_service1;
        public ItemClass.Service m_service2;
        public ItemClass.Service m_service3;
        public NetInfo.LaneType m_laneTypes;
        public VehicleInfo.VehicleType m_vehicleTypes;

        // Game instances
        private readonly NetSegment[] NetSegments;
        private readonly NetNode[] Nodes;
        private readonly NetLane[] NetLanes;

        // Connected graph storage
        private ConnectedStorage m_nodes;
        private Queue<ushort> m_connectedNodes;

        // Save state values so when know when to update graph
        private int m_iGameNodeCount;
        private int m_iGameSegmentCount;
        private int m_iGameLaneCount;

        public UnconnectedGraph()
        {
            m_nodes = new ConnectedStorage();
            m_connectedNodes = new Queue<ushort>();

            // Use these to track updating connections
            m_iGameNodeCount = 0;
            m_iGameSegmentCount = 0;
            m_iGameLaneCount = 0;

            // Store local references to buffers for faster access
            Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            NetSegments = Singleton<NetManager>.instance.m_segments.m_buffer;
            NetLanes = Singleton<NetManager>.instance.m_lanes.m_buffer;
        }

        public int Colors
        {
            get { return m_nodes.Colors; }
        }

        public bool IsValid()
        {
            return m_iGameNodeCount > 0 && 
                   m_iGameNodeCount == NetManager.instance.m_nodeCount &&
                   m_iGameSegmentCount == NetManager.instance.m_segmentCount &&
                   m_iGameLaneCount == NetManager.instance.m_laneCount;
        }

        public bool IsConnected(ushort node1, ushort node2)
        {
            return m_nodes.IsConnected(node1, node2);
        }

        public ConnectedStorage GetBuffer()
        {
            return m_nodes;
        }

        public void FloodFill()
        {
            // Store the game graph counts when we made this connection graph so we can invalidate it when this changes
            m_iGameNodeCount = NetManager.instance.m_nodeCount;
            m_iGameSegmentCount = NetManager.instance.m_segmentCount;
            m_iGameLaneCount = NetManager.instance.m_laneCount;

            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

            m_nodes.Colors = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (!HasVisited((ushort)i))
                {
                    NetNode node = Nodes[i];
                    if (node.m_flags != 0 && IsNetInfoValid(node.Info))
                    {
                        // Found an unvisited node, start exploring this nodes tree
                        m_nodes.Colors++;
                        int iColorCount = 0;
                        AddNode((ushort)i);

                        // Now process all nodes connected to this node and mark them with the same color.
                        while (m_connectedNodes.Any())
                        {
                            ushort nodeId = m_connectedNodes.Dequeue();
                            ProcessNode(nodeId, m_nodes.Colors, ref iColorCount);
                        }
                    }
                }
            }
        }

        private void AddNode(ushort nodeId)
        {
            m_connectedNodes.Enqueue(nodeId);
        }

        private bool HasVisited(ushort nodeId)
        {
            return m_nodes.HasVisited(nodeId);
        }

        private void ProcessNode(ushort nodeId, int iColor, ref int iColorCount)
        {
            if (HasVisited((ushort)nodeId))
            {
                // Check the colors match
                if (iColor != m_nodes.GetColor(nodeId))
                {
                    Debug.Log($"Found node {nodeId} with different color: {iColor} NodeColor: {m_nodes.GetColor(nodeId)}");
                }
            }
            else
            {
                // Check node is valid type
                NetNode node = Nodes[nodeId];
                if (IsNetInfoValid(node.Info))
                {
                    // Add to graph
                    m_nodes.SetColor(nodeId, iColor);
                    iColorCount++;

                    // Loop through segments to find neighboring nodes we can reach
                    for (int i = 0; i < 8; ++i)
                    {
                        ushort segmentId = node.GetSegment(i);
                        if (segmentId != 0)
                        {
                            ProcessSegment(segmentId);
                        }
                    }

                    // Loop through lanes to see if there are any extra connections segments
                    int iLaneCount = 0;
                    uint laneId = node.m_lane;
                    while (laneId != 0)
                    {
                        NetLane lane = NetLanes[laneId];
                        if (lane.m_flags != 0)
                        {
                            if (lane.m_segment != 0)
                            {
                                ProcessSegment(lane.m_segment);
                            }
                        }

                        laneId = lane.m_nextLane;

                        if (++iLaneCount >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        private void ProcessSegment(ushort segmentId)
        {
            if (segmentId != 0)
            {
                NetSegment segment = NetSegments[segmentId];
                if (segment.m_flags != 0)
                {
                    AddNode(segment.m_startNode);
                    AddNode(segment.m_endNode);

                    // Loop through all sub nodes for this lane
                    int iNodeLoopCount = 0;
                    int iLaneLoopCount = 0;

                    uint laneId = segment.m_lanes;
                    while (laneId != 0)
                    {
                        NetLane lane = NetLanes[laneId];
                        if (lane.m_flags != 0)
                        {
                            ushort nodeId = lane.m_nodes;
                            while (nodeId != 0)
                            {
                                NetNode node = Nodes[nodeId];
                                if (node.m_flags != 0)
                                {
                                    AddNode(nodeId);
                                }

                                nodeId = node.m_nextLaneNode;

                                // Safety check in case we get caught in an infinite loop somehow
                                if (iNodeLoopCount++ > NetManager.MAX_NODE_COUNT)
                                {
                                    Debug.Log("Invalid node loop detected");
                                    break;
                                }
                            }
                        }

                        // Update laneId
                        laneId = lane.m_nextLane;

                        // Safety check in case we get caught in an infinite loop somehow
                        if (iLaneLoopCount++ > NetManager.MAX_NODE_COUNT)
                        {
                            Debug.Log("Invalid lane loop detected");
                            break;
                        }
                    }
                }
            }
        }

        private bool IsNetInfoValid(NetInfo info)
        {
            return PathDistance.IsNetInfoValid(info, m_laneTypes, m_service1, m_service2, m_service3);
        }
    }
}
