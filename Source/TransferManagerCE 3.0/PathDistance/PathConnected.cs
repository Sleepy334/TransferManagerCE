using System.Collections.Generic;

namespace TransferManagerCE
{
    public class PathConnected : PathBase
    {
        // Connected graph storage
        private ConnectedStorage m_nodes;
        private Queue<ushort> m_connectedNodes;

        // Save state values so when know when to update graph
        private int m_iGameNodeCount;
        private int m_iGameSegmentCount;
        private int m_iGameLaneCount;

        public PathConnected() : base()
        {
            m_nodes = new ConnectedStorage();
            m_connectedNodes = new Queue<ushort>();

            // Use these to track updating connections
            m_iGameNodeCount = 0;
            m_iGameSegmentCount = 0;
            m_iGameLaneCount = 0;
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

            // Reset storage
            m_nodes.Clear();

            for (int i = 0; i < NetNodes.Length; i++)
            {
                if (!HasVisited((ushort)i))
                {
                    NetNode node = NetNodes[i];

                    // We check the service types match for the node but don't do a full check against a node due to pedestrian nodes
                    if (node.m_flags != 0 && node.Info is not null && IsServiceValid(node.Info))
                    {
                        // Found an unvisited node, start exploring this nodes tree
                        m_nodes.Colors++;
                        int iColor = m_nodes.Colors;
                        UpdateNode((ushort)i, 0.0f);

                        // Now process all nodes connected to this node and mark them with the same color.
                        while (m_connectedNodes.Count > 0)
                        {
                            ushort nodeId = m_connectedNodes.Dequeue();
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
                                ProcessNode(nodeId, iColor);
                            }
                        }
                    }
                }
            }
        }

        protected override void UpdateNode(ushort nodeId, float fCurrentTravelTime)
        {
            m_connectedNodes.Enqueue(nodeId);
        }

        private bool HasVisited(ushort nodeId)
        {
            return m_nodes.HasVisited(nodeId);
        }

        protected void ProcessNode(ushort nodeId, int iColor)
        {
            NetNode node = NetNodes[nodeId];
            if (node.m_flags != 0)
            {
                // We can reach this node, add to graph
                m_nodes.SetColor(nodeId, iColor);

                // Call trough to base ProcessNode
                ProcessNode(nodeId, node, 0.0f);
            }
        }

        protected override void ProcessSegment(ushort segmentId, ushort uiCurrentNodeId, float fCurrentTravelTime)
        {
            if (segmentId != 0)
            {
                NetSegment segment = NetSegments[segmentId];

                // Check segment is valid for this service type
                if (segment.m_flags != 0 && IsNetInfoValid(segment.Info))
                {
                    // Add nodes from this segment
                    UpdateNode(segment.m_startNode, 0.0f);
                    UpdateNode(segment.m_endNode, 0.0f);

                    // Loop through all sub nodes for this segments lanes
                    ProcessLaneNodes(segment.m_lanes, 0.0f);
                }
            }
        }
    }
}
