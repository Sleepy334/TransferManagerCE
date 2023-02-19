using ColossalFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Common;
using UnityEngine;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    public class PathDistance
    {
        const int iMAX_CANDIDATE_POSITIONS = 30;

        // Lane requirements for this path calculation
        public ItemClass.Service m_service1;
        public ItemClass.Service m_service2;
        public ItemClass.Service m_service3;
        public NetInfo.LaneType m_laneTypes;
        public VehicleInfo.VehicleType m_vehicleTypes;
        public bool m_bPedestrianZone;

        // Arrays
        private Dictionary<ushort, int> m_candidateData = new Dictionary<ushort, int>(); // Path candidate (node, id)
        private Dictionary<ushort, QueueData> m_nodeQueueData; // A quick lookup of the current data
        private BitArray m_visited; // A flag for whether node has been visited yet
        private PathQueue m_sortedNodes; // A rpiority queue to help us choose next node.

        // A* Heuristic
        private Vector3[] m_candidatePositions = new Vector3[iMAX_CANDIDATE_POSITIONS];
        private int m_iCandidatePositionCount = 0;

        private static bool s_bInitNeeded = true;
        private static Array16<NetNode>? NetNodes = null;
        private static Array16<NetSegment>? NetSegments = null;
        private static Array32<NetLane>? NetLanes = null;
        
        private static Stopwatch s_watch = Stopwatch.StartNew();

        private static void Init()
        {
            if (s_bInitNeeded)
            {
                s_bInitNeeded = false;
                NetNodes = Singleton<NetManager>.instance.m_nodes;
                NetSegments = Singleton<NetManager>.instance.m_segments;
                NetLanes = Singleton<NetManager>.instance.m_lanes;
                PathQueue.QueueData.UpdateHeuristicScale();
            }
        }

        public PathDistance()
        {
            Init();
            m_sortedNodes = new PathQueue();
            m_nodeQueueData = new Dictionary<ushort, QueueData>(Singleton<NetManager>.instance.m_nodeCount);
            m_visited = new BitArray(NetManager.MAX_NODE_COUNT);
        }

        private void ResetArrays()
        {
            // Clear graph arrays
            m_sortedNodes.Clear();
            m_nodeQueueData.Clear();
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
            fTravelTime = 0.0f;
            iNodesExamined = 0;

            long startTime = s_watch.ElapsedTicks;
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
                m_nodeQueueData[startNodeId] = nodeData;
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
                    if (visitedNodes != null)
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

                    ProcessNode(usMinNodeId, bstartActive, minNode.TravelTime());

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

        private void ProcessNode(ushort nodeId, bool bStartActive, float fCurrentTravelTime)
        {
            // For each segment from this node mark distance
            NetNode node = NetNodes.m_buffer[nodeId];
            if (node.m_flags != 0 && IsNetInfoValid(node.Info))
            {
                // Loop through segments to find neighboring roads
                for (int i = 0; i < 8; ++i)
                {
                    ushort segmentId = node.GetSegment(i);
                    if (segmentId != 0)
                    {
                        ProcessSegment(segmentId, nodeId, bStartActive, fCurrentTravelTime);
                    }
                }

                // Loop through lanes to if see there are any extra connections segments
                int iLaneCount = 0;
                uint laneId = node.m_lane;
                while (laneId != 0)
                {
                    NetLane lane = NetLanes.m_buffer[laneId];
                    if (lane.m_flags != 0)
                    {
                        if (lane.m_segment != 0)
                        {
                            ProcessSegment(lane.m_segment, nodeId, bStartActive, fCurrentTravelTime);
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

        private void ProcessSegment(ushort segmentId, uint uiCurrentNodeId, bool bStartActive, float fCurrentTravelTime)
        {
            if (segmentId != 0)
            {
                NetSegment segment = NetSegments.m_buffer[segmentId];
                if (segment.m_flags != 0)
                {      
                    ushort segmentNode1Id = 0;
                    ushort segmentNode2Id = 0;

                    // Find end node of segment, depending on which way we are going.
                    NetInfo.Direction direction;
                    if (segment.m_startNode == uiCurrentNodeId)
                    {
                        segmentNode1Id = segment.m_endNode;
                        direction = bStartActive ? NetInfo.Direction.Forward : NetInfo.Direction.Backward;
                    }
                    else if (segment.m_endNode == uiCurrentNodeId)
                    {
                        segmentNode1Id = segment.m_startNode;
                        direction = bStartActive ? NetInfo.Direction.Backward : NetInfo.Direction.Forward;
                    }
                    else
                    {
                        // This is a join segment, we can go either way
                        direction = NetInfo.Direction.Both;
                        segmentNode1Id = segment.m_startNode;
                        segmentNode2Id = segment.m_endNode;
                    }

                    // Some segments have the Inverted flag set which means we have to go the other way
                    if ((segment.m_flags & NetSegment.Flags.Invert) != 0)
                    {
                        direction = NetInfo.InvertDirection(direction);
                    }

                    // Check we have a lane available for this direction
                    float fTravelTime = UncongestedTravelTime(segmentId, segment, direction);
                    if (fTravelTime > 0)
                    {
                        float fNewTravelTime = fCurrentTravelTime + fTravelTime;

                        // Update node distance
                        UpdateNode(segmentNode1Id, fNewTravelTime);

                        // If it is a join segment (ie. Bus station to road) add both nodes
                        UpdateNode(segmentNode2Id, fNewTravelTime);

                        // Loop through all sub nodes for this lane
                        int iLaneLoopCount = 0;
                        uint laneId = segment.m_lanes;
                        while (laneId != 0)
                        {
                            NetLane lane = NetLanes.m_buffer[laneId];
                            if (lane.m_flags != 0)
                            {
                                int iNodeLoopCount = 0;
                                ushort nodeId = lane.m_nodes;
                                while (nodeId != 0)
                                {
                                    NetNode node = NetNodes.m_buffer[nodeId];
                                    if (node.m_flags != 0)
                                    {
                                        UpdateNode(nodeId, fCurrentTravelTime);
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
                            if (iLaneLoopCount++ > NetManager.MAX_LANE_COUNT)
                            {
                                Debug.Log("Invalid lane loop detected");
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateNode(ushort nodeId, float fNewTravelTime)
        {
            // Update node distance
            // NOTE: We currently dont remove node from m_visited here if a shorter time
            // It is clearly less accurate but way faster as we don't have to re-examine nodes
            // Speed is more important than absolute accuracy for transfer matching
            if (nodeId != 0 && fNewTravelTime > 0 && !m_visited[nodeId])
            {
                if (m_nodeQueueData.TryGetValue(nodeId, out QueueData nodeData))
                {
                    // We have already seen this node. Check if new distance is smaller than current distance (Shorter path)
                    if (fNewTravelTime < nodeData.TravelTime())
                    {
                        // Update travel time
                        nodeData.UpdateTravelTime(fNewTravelTime);

                        // Update node data
                        m_nodeQueueData[nodeId] = nodeData;
                        
                        // Add node to priority queue with new metric
                        m_sortedNodes.Push(nodeData);
                    }
                }
                else
                {
                    // Create a new one
                    QueueData newNodeData = new QueueData(nodeId, fNewTravelTime, GetNodeEstimateToCandidates(nodeId));

                    // Add it to out data stoe
                    m_nodeQueueData[nodeId] = newNodeData;

                    // Add node to priority queue so we can determine next node to visit
                    m_sortedNodes.Push(newNodeData);
                }
            }
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
                    NetNode node = NetNodes.m_buffer[candidate.Key];
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

                NetNode node = NetNodes.m_buffer[nodeId];
                if (node.m_flags != 0)
                {
                    Vector3 nodePosition = node.m_position;
                    for (int i = 0; i < m_iCandidatePositionCount; ++i)
                    {
                        // Need to make sure we apply outside connection multipliers here as well
                        float fDistance = (float)Vector3.SqrMagnitude(nodePosition - m_candidatePositions[i]) * PathNodeCache.GetOutsideNodeMultiplier(nodeId);
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
                if (info != null && IsNetInfoValid(info))
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

        private bool IsNetInfoValid(NetInfo info)
        {
            return IsNetInfoValid(info, m_laneTypes, m_vehicleTypes, m_service1, m_service2, m_service3, m_bPedestrianZone);
        }

        public static bool IsNetInfoValid(NetInfo info, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, ItemClass.Service service1, ItemClass.Service service2, ItemClass.Service service3, bool bPedestrianZone)
        {
            return 
                (info.m_laneTypes & laneTypes) != 0 &&
                IsNetInfoTypesValid(info, vehicleTypes) &&
                (info.m_class.m_service == service1 || info.m_class.m_service == service2 || info.m_class.m_service == service3) &&
                (bPedestrianZone || !info.IsPedestrianZoneRoad());
        }

        public static bool IsNetInfoTypesValid(NetInfo info, VehicleInfo.VehicleType vehicleTypes)
        {
            if (info.m_class.m_service == ItemClass.Service.Beautification && info.m_class.m_subService == ItemClass.SubService.None)
            {
                // Cargo stations seem to label their connector nodes as Beautification for some reason
                // and annoyingly their vehicle types are set to None so we need to handle this separately.
                return true;
            }
            else
            {
                return (info.m_vehicleTypes & vehicleTypes) != 0;
            }
        }
    }
}
