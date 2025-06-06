using ColossalFramework;
using SleepyCommon;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static TransferManagerCE.NodeLinkData;

namespace TransferManagerCE
{
    public class NodeLinkGraph
    {
        protected readonly NetNode[] NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

        // Lane requirements for this path calculation
        private NetworkModeHelper.NetworkMode m_mode = NetworkModeHelper.NetworkMode.None;

        NodeLinkGenerator m_nodeLinkGenerator;
        private object m_lock = new object();
        private object m_regenerationLock = new object();

        // Cache to store found nodes for later matches.
        private Dictionary<ushort, NodeLinkData> m_allNodeLinks = new Dictionary<ushort, NodeLinkData>();
        private static NodeLinkData s_emptyNodeLinkData = new NodeLinkData();

        // Save state values so when know when to update graph
        private int m_iGameNodeCount;
        private int m_iGameSegmentCount;
        private int m_iGameLaneCount;
        private bool m_invalidateCache = false;

        private static Stopwatch s_stopwatch = Stopwatch.StartNew();
        public static long s_totalGenerationTicks = 0;
        public static int s_totalGenerations = 0;

        // ----------------------------------------------------------------------------------------
        public NodeLinkGraph(NetworkModeHelper.NetworkMode mode)
        {
            m_mode = mode;
            m_nodeLinkGenerator = new NodeLinkGenerator(mode);

            // Use these to track updating connections
            m_iGameNodeCount = 0;
            m_iGameSegmentCount = 0;
            m_iGameLaneCount = 0;
        }

        // ----------------------------------------------------------------------------------------
        public bool IsValid()
        {
            return !m_invalidateCache &&
                   m_iGameNodeCount > 0 &&
                   m_iGameNodeCount == NetManager.instance.m_nodeCount &&
                   m_iGameSegmentCount == NetManager.instance.m_segmentCount &&
                   m_iGameLaneCount == NetManager.instance.m_laneCount;
        }

        public void Invalidate()
        {
            m_invalidateCache = true;
        }

        // ----------------------------------------------------------------------------------------
        public void Update()
        {
            // We only want one thread to enter and regenerate the data
            if (Monitor.TryEnter(m_regenerationLock, 0))
            {
                try
                {
                    if (!IsValid())
                    {
                        long startTicks = s_stopwatch.ElapsedTicks;

                        // Only allow 1 thread to generate the cache at a time
                        Dictionary<ushort, NodeLinkData> newLinks = GenerateGraph();

                        long stopTicks = s_stopwatch.ElapsedTicks;
                        s_totalGenerationTicks += stopTicks - startTicks;
                        s_totalGenerations++;

                        lock (m_lock)
                        {
                            // Replace old cached data
                            m_allNodeLinks = newLinks;

                            // Store the game graph counts when we made this connection graph so we can invalidate it when this changes
                            m_invalidateCache = false;
                            m_iGameNodeCount = NetManager.instance.m_nodeCount;
                            m_iGameSegmentCount = NetManager.instance.m_segmentCount;
                            m_iGameLaneCount = NetManager.instance.m_laneCount;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(m_regenerationLock);
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        public bool HasNodeLinks(ushort nodeId)
        {
            lock (m_lock)
            {
                return m_allNodeLinks.ContainsKey(nodeId);
            }
        }

        // ----------------------------------------------------------------------------------------
        public bool TryGetNodeLinks(ushort nodeId, out NodeLinkData data)
        {
            lock (m_lock)
            {
                if (m_allNodeLinks.TryGetValue(nodeId, out data))
                {
                    return true;
                }
                else
                {
                    // The node hasnt passed the service valid check so just return empty.
                    return false;
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        public NodeLinkData GetNodeLinks(ushort nodeId)
        {
            lock (m_lock)
            {
                if (m_allNodeLinks.TryGetValue(nodeId, out NodeLinkData data))
                {
                    return data;
                }
                else
                {
                    // The node hasnt passed the service valid check so just return empty.
                    return s_emptyNodeLinkData;
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        public Dictionary<ushort, NodeLinkData> GenerateGraph()
        {
            Dictionary<ushort, NodeLinkData> allNodeLinks = new Dictionary<ushort, NodeLinkData>();

            for (int i = 0; i < NetNodes.Length; i++)
            {
                ushort nodeId = (ushort)i;
                NodeLinkData nodeLinkData = m_nodeLinkGenerator.GetNodeLinks(nodeId);
                if (nodeLinkData.Count > 0)
                {
                    allNodeLinks[nodeId] = new NodeLinkData(nodeLinkData); // Store a copy
                }
            }

            // Add bypass links to speed up path finding.
            MiddleNodeBypass middleNodeBypass = new MiddleNodeBypass(allNodeLinks);
            middleNodeBypass.Bypass();

#if DEBUG
            if (!ValidateLinks(allNodeLinks))
            {
                CDebug.Log($"ERROR: Node link graph not valid. {m_mode}");
            }
#endif

            return allNodeLinks;
        }

        // ----------------------------------------------------------------------------------------
        public void Clear()
        {
            lock (m_lock)
            {
                m_allNodeLinks.Clear();
            }
        }

        // ----------------------------------------------------------------------------------------
        private static bool ValidateLinks(Dictionary<ushort, NodeLinkData> newLinks)
        {
            foreach (KeyValuePair<ushort, NodeLinkData> kvp in newLinks)
            {
                foreach (NodeLink link in kvp.Value.items)
                {
                    if (!newLinks.ContainsKey(link.m_nodeId))
                    {
                        NetNode node = NetManager.instance.m_nodes.m_buffer[link.m_nodeId];
                        CDebug.Log($"ERROR: Node: {link.m_nodeId} not found in graph. Flags: {node.m_flags} Service: {node.Info.GetService()} AI: {node.Info.GetAI()}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
