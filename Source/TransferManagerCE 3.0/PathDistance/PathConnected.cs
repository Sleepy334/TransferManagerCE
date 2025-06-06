using ColossalFramework;
using SleepyCommon;
using System.Collections.Generic;
using System.Diagnostics;
using static TransferManagerCE.NetworkModeHelper;
using static TransferManagerCE.NodeLinkData;

namespace TransferManagerCE
{
    public class PathConnected
    {
        // Connected graph storage
        private ConnectedStorage m_nodes;
        private Queue<ushort> m_connectedNodes;

        // Save state values so when know when to update graph
        private int m_iGameNodeCount;
        private int m_iGameSegmentCount;
        private int m_iGameLaneCount;
        private bool m_invalidated = false;
        private NetworkMode m_mode;

        // Statistics
        private static Stopwatch s_stopwatch = Stopwatch.StartNew();
        public static long s_totalGenerationTicks = 0;
        public static int s_totalGenerations = 0;

        // ----------------------------------------------------------------------------------------
        public PathConnected(NetworkMode mode) : 
            base()
        {
            m_mode = mode;
            m_nodes = new ConnectedStorage();
            m_connectedNodes = new Queue<ushort>();

            // Use these to track updating connections
            m_iGameNodeCount = 0;
            m_iGameSegmentCount = 0;
            m_iGameLaneCount = 0;
            m_invalidated = false;
        }

        public int Colors
        {
            get { return m_nodes.Colors; }
        }

        public int GetColor(ushort nodeId)
        {
            return m_nodes.GetColor(nodeId);
        }

        public bool IsValid()
        {
            if (m_invalidated)
            {
                return false;
            }

            return m_iGameNodeCount > 0 && 
                   m_iGameNodeCount == NetManager.instance.m_nodeCount &&
                   m_iGameSegmentCount == NetManager.instance.m_segmentCount &&
                   m_iGameLaneCount == NetManager.instance.m_laneCount;
        }

        public void Invalidate()
        {
            m_invalidated = true;
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
            long startTicks = s_stopwatch.ElapsedTicks;

            // Store the game graph counts when we made this connection graph so we can invalidate it when this changes
            m_iGameNodeCount = NetManager.instance.m_nodeCount;
            m_iGameSegmentCount = NetManager.instance.m_segmentCount;
            m_iGameLaneCount = NetManager.instance.m_laneCount;
            m_invalidated = false;

            NodeLinkGraph nodeLink = PathDistanceCache.GetLoader(m_mode, true); // Generate if needed
            NodeLinkData links;

            // Reset storage
            m_nodes.Clear();

            int iLength = Singleton<NetManager>.instance.m_nodes.m_buffer.Length;
            for (int i = 0; i < iLength; i++)
            {
                ushort startNodeId = (ushort)i;
                if (!HasVisited(startNodeId) && nodeLink.HasNodeLinks(startNodeId))
                {
                    // Found an unvisited node, start exploring this nodes tree
                    m_nodes.Colors++;
                    int iColor = m_nodes.Colors;

                    // Add to queue to start processing
                    UpdateNode(startNodeId, 0.0f);

                    // Now process all nodes connected to this node and mark them with the same color.
                    while (m_connectedNodes.Count > 0)
                    {
                        ushort nodeId = m_connectedNodes.Dequeue();
                        if (HasVisited(nodeId))
                        {
                            // Check the colors match
                            if (iColor != m_nodes.GetColor(nodeId))
                            {
                                CDebug.Log($"ERROR: Found node {nodeId} with different color: {iColor} NodeColor: {m_nodes.GetColor(nodeId)}");
                            }
                        } 
                        else if (nodeLink.TryGetNodeLinks(nodeId, out links))
                        {
                            // We can reach this node, add to graph
                            m_nodes.SetColor(nodeId, iColor);

                            // Process all node links from this node
                            foreach (NodeLink link in links.items)
                            {
                                UpdateNode(link.m_nodeId, 0.0f);
                            }
                        }
                    }
                }
            }

            long stopTicks = s_stopwatch.ElapsedTicks;
            s_totalGenerationTicks += stopTicks - startTicks;
            s_totalGenerations++;
        }

        protected void UpdateNode(ushort nodeId, float fCurrentTravelTime)
        {
            m_connectedNodes.Enqueue(nodeId);
        }

        private bool HasVisited(ushort nodeId)
        {
            return m_nodes.HasVisited(nodeId);
        }
    }
}
