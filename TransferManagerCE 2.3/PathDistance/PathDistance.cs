using ColossalFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Common;
using UnityEngine;
using UnityEngine.Networking.Types;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    public class PathDistance : PathBase
    {
        const int iMAX_CANDIDATE_POSITIONS = 30;

        // Arrays
        private Dictionary<ushort, int> m_candidateData = new Dictionary<ushort, int>(); // Path candidate (node, id)
        private BitArray m_visited; // A flag for whether node has been visited yet
        private PathQueue m_sortedNodes; // An indexed priority queue to help us choose next node.

        // A* Heuristic
        private Vector3[] m_candidatePositions = new Vector3[iMAX_CANDIDATE_POSITIONS];
        private int m_iCandidatePositionCount = 0;

        // Which direction is the vehicle going?
        private bool m_bStartActive = true;

        private static Stopwatch s_watch = Stopwatch.StartNew();

        public PathDistance() : base()
        {
            QueueData.UpdateHeuristicScale();
            m_sortedNodes = new PathQueue();
            m_visited = new BitArray(NetManager.MAX_NODE_COUNT);
        }

        private void ResetArrays()
        {
            // Clear graph arrays
            m_sortedNodes.Clear();
            m_visited.SetAll(false);
        }

        public void AddCandidate(ushort nodeId, int id)
        {
            m_candidateData[nodeId] = id;
        }

        public bool ContainsNode(ushort nodeId)
        {
            return m_candidateData.ContainsKey(nodeId);
        }

        public int CandidateCount()
        {
            return m_candidateData.Count;
        }

        public void ClearCandidates()
        {
            m_candidateData.Clear();
        }

        // Dijkstra's algorithm, returns -1 if not found, otherwise returns candidate id
        public int FindNearestNeighborId(bool bstartActive, ushort startNodeId, out float fTravelTime, out long ticks, out int iNodesExamined, List<QueueData>? visitedNodes = null)
        {
            long startTime = s_watch.ElapsedTicks;

            m_bStartActive = bstartActive;
            fTravelTime = 0.0f;
            iNodesExamined = 0;
            int iChosenId = -1;
            int iNodeCount = 0;

            if (m_candidateData.Count > 0)
            {
                if (m_candidateData.Count == 1)
                {
                    // 1 option just return it's id.
                    foreach (var candidate in m_candidateData)
                    {
                        ticks = s_watch.ElapsedTicks - startTime;
                        return candidate.Value;
                    }
                }

                // Check if start node and candidate node are the same
                // no need to search further, return candidate id
                if (m_candidateData.ContainsKey(startNodeId))
                {
                    ticks = s_watch.ElapsedTicks - startTime;
                    return m_candidateData[startNodeId];
                }

                // Load candidate positions for LOS heuristic
                LoadCandidatePositions(m_candidateData);

                // Clear graph arrays
                ResetArrays();

                // Mark start node as distance 0.
                QueueData nodeData = new QueueData(startNodeId, 0.0f, float.MaxValue);
                m_sortedNodes.Push(nodeData);

                // Given a starting node, traverse the maps nodes to find the nearest candidate
                int iLoopCount = 0;
                while (m_sortedNodes.Count > 0)
                {
                    // Next node to evaluate is the minimum distance "unvisited" node.
                    QueueData minNode = m_sortedNodes.Pop();
                    ushort usMinNodeId = minNode.Node();
                    
                    if (m_visited[usMinNodeId])
                    {
                        continue;
                    }

                    // Add to visisted list if requested
                    if (visitedNodes is not null)
                    {
                        visitedNodes.Add(minNode);
                    }
                    iNodesExamined++;

                    // Set the node as visited now
                    m_visited[usMinNodeId] = true;
                    iNodeCount++;

                    // If a candidate is the lowest value
                    // unvisited node then we don't need to search anymore
                    if (m_candidateData.ContainsKey(usMinNodeId))
                    {
                        iChosenId = m_candidateData[usMinNodeId];
                        fTravelTime = minNode.TravelTime();
                        break;
                    }

                    ProcessNode(usMinNodeId, NetNodes[usMinNodeId], minNode.TravelTime());

                    // Safety check in case we get caught in an infinite loop somehow
                    if (iLoopCount++ > NetManager.MAX_NODE_COUNT)
                    {
                        //RoadAccessData.AddInstance(new InstanceID { NetNode = (ushort) startNodeId });
                        string sCandidates = "";
                        foreach (var candidate in m_candidateData)
                        {
                            sCandidates += $"{candidate}, ";
                            //RoadAccessData.AddInstance(new InstanceID { NetNode = (ushort)candidate });
                        }
                        Debug.Log($"Invalid loop detected StartNode:{startNodeId} Candidates:{sCandidates}");
                        
                        break;
                    }
                } // End while
            }

            ticks = s_watch.ElapsedTicks - startTime;
            return iChosenId;
        }

        protected override void ProcessSegment(ushort segmentId, ushort usCurrentNodeId, float fCurrentTravelTime)
        {
            if (segmentId != 0)
            {
                NetSegment segment = NetSegments[segmentId];
                if (segment.m_flags != 0 && IsNetInfoValid(segment.Info))
                {      
                    // Find direction of segment
                    NetInfo.Direction direction = GetSegmentDirection(segment, usCurrentNodeId);

                    // Check we have a lane available for this direction
                    float fTravelTime = UncongestedTravelTime(segmentId, segment, direction);
                    if (fTravelTime > 0)
                    {
                        float fNewTravelTime = fCurrentTravelTime + fTravelTime;

                        // Add nodes from this segment
                        if (segment.m_startNode != usCurrentNodeId)
                        {
                            UpdateNode(segment.m_startNode, fNewTravelTime);
                        }

                        if (segment.m_endNode != usCurrentNodeId)
                        {
                            UpdateNode(segment.m_endNode, fNewTravelTime);
                        }

                        // Loop through all sub nodes for this segments lanes
                        ProcessLaneNodes(segment.m_lanes, fNewTravelTime);
                    }
                }
            }
        }

        protected override void UpdateNode(ushort nodeId, float fNewTravelTime)
        {
            // Update node distance
            // NOTE: We currently dont remove node from m_visited here if a shorter time
            // It is clearly less accurate but way faster as we don't have to re-examine nodes
            // Speed is more important than absolute accuracy for transfer matching
            if (nodeId != 0 && fNewTravelTime > 0 && !m_visited[nodeId])
            {
                if (m_sortedNodes.TryGetValue(nodeId, out QueueData nodeData))
                {
                    // We have already seen this node. Check if new distance is smaller than current distance (Shorter path)
                    if (fNewTravelTime < nodeData.TravelTime())
                    {
                        // Update travel time
                        nodeData.UpdateTravelTime(fNewTravelTime);

                        // Update node in priority queue
                        m_sortedNodes.Update(nodeData);
                    }
                }
                else
                {
                    // Create a new one
                    QueueData newNodeData = new QueueData(nodeId, fNewTravelTime, GetNodeEstimateToCandidates(nodeId));

                    // Add node to priority queue so we can determine next node to visit
                    m_sortedNodes.Push(newNodeData);
                }
            }
        }

        private NetInfo.Direction GetSegmentDirection(NetSegment segment, ushort usCurrentNodeId)
        {
            // Find end node of segment, depending on which way we are going.
            NetInfo.Direction direction;
            if (segment.m_startNode == usCurrentNodeId)
            {
                direction = m_bStartActive ? NetInfo.Direction.Forward : NetInfo.Direction.Backward;
            }
            else if (segment.m_endNode == usCurrentNodeId)
            {
                direction = m_bStartActive ? NetInfo.Direction.Backward : NetInfo.Direction.Forward;
            }
            else
            {
                // This is a join segment, we can go either way
                direction = NetInfo.Direction.Both;
            }

            // Some segments have the Inverted flag set which means we have to go the other way
            if ((segment.m_flags & NetSegment.Flags.Invert) != 0)
            {
                direction = NetInfo.InvertDirection(direction);
            }

            return direction;
        }

        // Load candidate positions for LOS heuristic,
        private void LoadCandidatePositions(Dictionary<ushort, int> candidates)
        {
            m_iCandidatePositionCount = 0;

            // if the candidate count is too large it is better just to use travel time
            // also dont bother calculating heuristic if set to 0
            if (candidates.Count <= iMAX_CANDIDATE_POSITIONS && 
                SaveGameSettings.GetSettings().PathDistanceHeuristic > 0)
            {
                int iIndex = 0;
                foreach (var candidate in candidates)
                {
                    NetNode node = NetNodes[candidate.Key];
                    if (node.m_flags != 0)
                    {
                        m_candidatePositions[iIndex++] = node.m_position;
                        m_iCandidatePositionCount++;
                    }
                }
            }
        }

        // A* Hueristic, find smallest LOS from node to candidate positions
        // Ensures we are expanding nodes in the right direction instead of
        // heading in the wrong direction wasting time.
        private float GetNodeEstimateToCandidates(ushort nodeId)
        {
            if (m_iCandidatePositionCount > 0)
            {
                float fMinDistance = float.MaxValue;

                NetNode node = NetNodes[nodeId];
                if (node.m_flags != 0)
                {
                    Vector3 nodePosition = node.m_position;
                    for (int i = 0; i < m_iCandidatePositionCount; ++i)
                    {
                        // Need to make sure we apply outside connection multipliers here as well
                        float fDistance = Vector3.SqrMagnitude(nodePosition - m_candidatePositions[i]) * PathNodeCache.GetOutsideNodeMultiplier(nodeId);
                        if (fDistance < fMinDistance)
                        {
                            fMinDistance = fDistance;
                        }
                    }
                }

                if (fMinDistance != float.MaxValue)
                {
                    // We scale distance so it has a bigger impact
                    return (float)Math.Sqrt(fMinDistance);
                }
                else
                {
                    return float.MaxValue;
                }
            }

            return 0f;
        }

        private float UncongestedTravelTime(ushort segmentId, NetSegment segment, NetInfo.Direction direction)
        {
            // This code assumes all valid lanes have same speed.
            if (segment.m_flags != 0)
            {
                NetInfo info = segment.Info;
                if (info is not null && IsNetInfoValid(info))
                {
                    for (int i = info.m_lanes.Length - 1; i >= 0; --i)
                    {
                        NetInfo.Lane lane = info.m_lanes[i];
                        if ((lane.m_laneType & m_laneTypes) != 0 &&
                            (direction == NetInfo.Direction.Both || (lane.m_finalDirection & direction) == direction) &&
                            lane.m_speedLimit > 0)
                        {
                            float fEffectiveTravelTime;
                            if (PathNodeCache.GetOutsideSegmentTravelTime(segmentId, out fEffectiveTravelTime))
                            {
                                return fEffectiveTravelTime;
                            }
                            else
                            {
                                return segment.m_averageLength / lane.m_speedLimit;
                            }
                        }
                    }
                }   
            }
            
            return 0f;
        }
    }
}
