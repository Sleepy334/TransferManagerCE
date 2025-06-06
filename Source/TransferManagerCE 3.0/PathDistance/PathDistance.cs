using SleepyCommon;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static TransferManagerCE.NetworkModeHelper;
using static TransferManagerCE.NodeLinkData;

namespace TransferManagerCE
{
    public class PathDistance
    {
        private NetworkMode m_mode = NetworkMode.None;
        private bool m_bAutoSelectSingleCandidate = true;
        // Arrays
        PathCandidates m_candidates = new PathCandidates();
        private BitArray m_visited; // A flag for whether node has been visited yet
        private PathQueue m_sortedNodes; // An indexed priority queue to help us choose next node.

        private static Stopwatch s_watch = Stopwatch.StartNew();

        // ----------------------------------------------------------------------------------------
        public PathDistance(bool bStoreNodes, bool bAutoSelectSingleCandidate) : 
            base()
        {
            m_sortedNodes = new PathQueue(bStoreNodes);
            m_visited = new BitArray(NetManager.MAX_NODE_COUNT);
            m_bAutoSelectSingleCandidate = bAutoSelectSingleCandidate;
        }

        public void SetNetworkMode(NetworkMode mode)
        {
            m_mode = mode;

            // Dont bother calculating heuristic if set to 0
            PathData.UpdateHeuristicScale();
        }

        public Dictionary<ushort, PathData>? GetExaminedNodes()
        {
            return m_sortedNodes.GetExaminedNodes();
        }

        public PathCandidates Candidates
        {
            get { return m_candidates; }
        }

        // Dijkstra's algorithm, returns -1 if not found, otherwise returns candidate id
        public int FindNearestNeighborId(bool bStartActive, ushort startNodeId, out ushort nodeId, out float fTravelTime, out long ticks, out int iNodesExamined)
        {
            long startTime = s_watch.ElapsedTicks;
            fTravelTime = 0.0f;
            iNodesExamined = 0;

            nodeId = 0;
            int iCandidateId = -1;

            if (Candidates.Count > 0)
            {
                if (m_bAutoSelectSingleCandidate && Candidates.Count == 1)
                {
                    // 1 option just return it's id.
                    KeyValuePair<ushort, int> kvp = Candidates.Items.First();
                    ticks = s_watch.ElapsedTicks - startTime;
                    nodeId = kvp.Key;
                    return kvp.Value;
                }

                NodeLinkGraph nodeLinkLoader = PathDistanceCache.GetLoader(m_mode, false); // Don't update cache here, just use current values

                // Check if start node and candidate node are the same
                // no need to search further, return candidate id
                if (Candidates.Contains(startNodeId, out int candidateId))
                {
                    ticks = s_watch.ElapsedTicks - startTime;
                    nodeId = startNodeId;
                    return candidateId;
                }

                // Clear graph arrays
                ResetArrays();

                // Mark start node as distance 0.
                PathData nodeData = new PathData(startNodeId, 0, 0.0f, float.MaxValue);
                m_sortedNodes.Push(nodeData);

                // Determine direction vehicle must travel
                NetInfo.Direction direction = bStartActive ? NetInfo.Direction.Forward : NetInfo.Direction.Backward;

                // Given a starting node, traverse the maps nodes to find the nearest candidate
                int iLoopCount = 0;
                while (m_sortedNodes.Count > 0)
                {
                    // Next node to evaluate is the minimum distance "unvisited" node.
                    PathData minNode = m_sortedNodes.Pop();
                    
                    ushort usMinNodeId = minNode.nodeId;
                    if (m_visited[usMinNodeId])
                    {
                        continue;
                    }

                    // Set the node as visited now
                    iNodesExamined++;
                    minNode.visited = true;
                    m_visited[usMinNodeId] = true;

                    // If a candidate is the lowest value
                    // unvisited node then we don't need to search anymore
                    if (Candidates.Contains(usMinNodeId, out candidateId))
                    {
                        nodeId = usMinNodeId;
                        iCandidateId = candidateId;
                        fTravelTime = minNode.TravelTime();
                        break;
                    }

                    // Process all node links from this node
                    NodeLinkData links = nodeLinkLoader.GetNodeLinks(usMinNodeId);
                    List<NodeLink> linkData = links.items;
                    for (int i = 0; i < linkData.Count; ++i)
                    {
                        NodeLink link = linkData[i];

                        // Check direction of node as well
                        if (link.m_direction == NetInfo.Direction.Both || link.m_direction == direction)
                        {
                            UpdateNode(link.m_nodeId, usMinNodeId, minNode.TravelTime() + link.m_fTravelTime);
                        }
                    }

                    // Safety check in case we get caught in an infinite loop somehow
                    if (iLoopCount++ > NetManager.MAX_NODE_COUNT)
                    {
                        CDebug.Log($"Invalid loop detected.");
                        break;
                    }
                } // End while
            }

            ticks = s_watch.ElapsedTicks - startTime;
            return iCandidateId;
        }

        private void ResetArrays()
        {
            // Clear graph arrays
            m_sortedNodes.Clear();
            m_visited.SetAll(false);
        }

        protected void UpdateNode(ushort nodeId, ushort prevNode, float fNewTravelTime)
        {
            // Update node distance
            if (nodeId != 0 && fNewTravelTime > 0 && !m_visited[nodeId])
            {
                if (m_sortedNodes.TryGetValue(nodeId, out PathData nodeData))
                {
                    // We have already seen this node. Check if new distance is smaller than current distance (Shorter path)
                    if (fNewTravelTime < nodeData.TravelTime())
                    {
                        // Update travel time
                        nodeData.UpdateTravelTime(fNewTravelTime);
                        nodeData.prevId = prevNode;

                        // Reset visit flag
                        //nodeData.visited = false;
                        //m_visited[nodeId] = false;

                        // Update node in priority queue
                        m_sortedNodes.Update(nodeData);
                    }
                }
                else
                {
                    // Create a new one
                    PathData newNodeData = new PathData(nodeId, prevNode, fNewTravelTime, Candidates.GetNodeEstimateToCandidates(nodeId));

                    // Add node to priority queue so we can determine next node to visit
                    m_sortedNodes.Push(newNodeData);
                }
            }
        }
    }
}
