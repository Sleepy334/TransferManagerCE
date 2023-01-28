using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class PathDistance
    {
        // Lane requirements for this path calculation
        public ItemClass.Service m_service1;
        public ItemClass.Service m_service2;
        public ItemClass.Service m_service3;
        public NetInfo.LaneType m_laneTypes;
        public VehicleInfo.VehicleType m_vehicleTypes;
        public bool m_bPedestrianZone;

        //private static Stopwatch s_watch = Stopwatch.StartNew();

        public class QueueData
        {
            public ushort m_nodeId;
            public float m_fTravelTime = float.MaxValue;
            public float m_fNodeFactor = float.MaxValue; // A* heuristic

            public QueueData(ushort nodeId, float fTravelTime, float fNodeFactor)
            {
                m_nodeId = nodeId;
                m_fTravelTime = fTravelTime;
                m_fNodeFactor = fNodeFactor;
            }
        }

        public class QueueDataComparer : IComparer<QueueData>
        {
            int IComparer<QueueData>.Compare(QueueData x, QueueData y)
            {
                // We now run a "Node factor" heuristic to improve path performance
                return ((x.m_fTravelTime + x.m_fNodeFactor) < (y.m_fTravelTime + y.m_fNodeFactor)) ? -1 : 1;
            }
        }

        private Dictionary<ushort, float> m_nodes;
        private HashSet<ushort> m_visited;
        private PriorityQueue<QueueData> m_sortedNodes;
        private HashSet<Vector3>? m_candidatePositions = null;

        private static bool s_bInitNeeded = true;
        private static Array16<NetNode>? NetNodes = null;
        private static Array16<NetSegment>? NetSegments = null;
        private static Array32<NetLane>? NetLanes = null;

        private static void Init()
        {
            if (s_bInitNeeded)
            {
                s_bInitNeeded = false;
                NetNodes = Singleton<NetManager>.instance.m_nodes;
                NetSegments = Singleton<NetManager>.instance.m_segments;
                NetLanes = Singleton<NetManager>.instance.m_lanes;
            }
        }

        public PathDistance()
        {
            Init();
            m_sortedNodes = new PriorityQueue<QueueData>(Singleton<NetManager>.instance.m_nodeCount, new QueueDataComparer());
            m_nodes = new Dictionary<ushort, float>(Singleton<NetManager>.instance.m_nodeCount);
            m_visited = new HashSet<ushort>();
        }

        // Dijkstra's algorithm
        public uint FindNearestNeighbor(bool bstartActive, ushort startNodeId, HashSet<ushort>? candidates, out float fTravelTime)
        {
            //long startTime = s_watch.ElapsedMilliseconds;

            ushort uiChosenCandidate = 0;
            fTravelTime = 0.0f;

            int iNodeCount = 0;
            if (candidates != null && candidates.Count > 0)
            {
                if (candidates == null)
                {
                    return 0;
                }
                if (candidates.Count == 1)
                {
                    foreach (var candidate in candidates)
                    {
                        return candidate;
                    }
                }
                if (candidates.Contains(startNodeId))
                {
                    return startNodeId;
                }

                // Load candidate positions for LOS heuristic,
                // if the candidate count is too large it is better just to use travel time
                if (candidates.Count <= 20)
                {
                    m_candidatePositions = new HashSet<Vector3>();
                    foreach (var candidate in candidates)
                    {
                        NetNode node = NetNodes.m_buffer[candidate];
                        if (node.m_flags != 0)
                        {
                            m_candidatePositions.Add(node.m_position);
                        }
                        else
                        {
                            Debug.Log($"Unable to find node for candidate: {candidate}");
                            m_candidatePositions.Add(Vector3.zero);
                        }
                    }
                }
                else
                {
                    m_candidatePositions = null;
                }

                // Clear graph arrays
                while (m_sortedNodes.Count > 0)
                {
                    m_sortedNodes.Pop();
                }
                m_nodes.Clear();
                m_visited.Clear();

                // Mark start node as distance 0.
                m_nodes[startNodeId] = 0.0f;
                m_sortedNodes.Push(new QueueData(startNodeId, 0.0f, float.MaxValue));

                // Given a starting node, traverse the maps nodes to find the nearest candidate
                int iLoopCount = 0;
                while (m_sortedNodes.Count > 0)
                {
                    // Next node to evaluate is the minimum distance "unvisited" node.
                    ushort usMinNodeId = m_sortedNodes.Top.m_nodeId;
                    m_sortedNodes.Pop();

                    if (m_visited.Contains(usMinNodeId))
                    {
                        // Already visited this node skip
                        continue;
                    }

                    // Set the node as visited now
                    m_visited.Add(usMinNodeId);
                    iNodeCount++;

                    // If a candidate is the lowest value
                    // unvisited node then we don't need to search anymore
                    if (candidates.Contains(usMinNodeId))
                    {
                        
                        uiChosenCandidate = usMinNodeId;
                        fTravelTime = m_nodes[uiChosenCandidate];
                        break;
                    }

                    ProcessNode(usMinNodeId, bstartActive);

                    // Safety check in case we get caught in an infinite loop somehow
                    if (iLoopCount++ > NetManager.MAX_NODE_COUNT)
                    {
                        //RoadAccessData.AddInstance(new InstanceID { NetNode = (ushort) startNodeId });
                        string sCandidates = "";
                        foreach (var candidate in candidates)
                        {
                            sCandidates += $"{candidate}, ";
                            //RoadAccessData.AddInstance(new InstanceID { NetNode = (ushort)candidate });
                        }
                        Debug.Log($"Invalid loop detected StartNode:{startNodeId} Candidates:{sCandidates}");
                        
                        break;
                    }
                } // End while
            }

            // DEBUGGING
            /*
            long runTime = s_watch.ElapsedMilliseconds - startTime;
            if (runTime > 100)
            {
                Vector3 startPosition = Vector3.zero;
                NetNode startNode = NetNodes.m_buffer[startNodeId];
                if (startNode.m_flags != 0)
                {
                    startPosition = startNode.m_position;
                }

                // Determine LOS closest node
                string sDistances = "";
                foreach (var candidate in candidates)
                {
                    NetNode node = NetNodes.m_buffer[candidate];
                    if (node.m_flags != 0)
                    {
                        double dDistance = Math.Sqrt(Vector3.SqrMagnitude(startPosition - node.m_position));
                        sDistances += $"\r\nNode: {candidate} Distance: {dDistance}";
                    }
                }
                sDistances += $"\r\nStartNode:{startNodeId} CandidateCount: {candidates.Count} Chosen:{uiChosenCandidate} NodesExamined:{iNodeCount} TravelTime: {m_nodes[uiChosenCandidate]} Time:{runTime}ms";
                Debug.Log(sDistances);
            }
            */

            return uiChosenCandidate;
        }

        private void ProcessNode(ushort nodeId, bool bStartActive)
        {
            // For each segment from this node mark distance
            NetNode node = NetNodes.m_buffer[nodeId];
            if (node.m_flags != 0 && IsNetInfoValid(node.Info))
            {
                float fCurrentTravelTime = m_nodes[nodeId];

                // Loop through segments to find neighboring roads
                for (int i = 0; i < 8; ++i)
                {
                    ushort segmentId = node.GetSegment(i);
                    if (segmentId != 0)
                    {
                        ProcessSegment(segmentId, nodeId, bStartActive, fCurrentTravelTime);
                    }
                }

                // Loop through lanes to see there are any extra connections segments
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
                        int iNodeLoopCount = 0;
                        int iLaneLoopCount = 0;

                        uint laneId = segment.m_lanes;
                        while (laneId != 0)
                        {
                            NetLane lane = NetLanes.m_buffer[laneId];
                            if (lane.m_flags != 0)
                            {
                                ushort nodeId = lane.m_nodes;
                                while (nodeId != 0)
                                {
                                    NetNode node = NetNodes.m_buffer[nodeId];
                                    if (IsNetInfoValid(node.Info))
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
            }
        }

        private void UpdateNode(ushort nodeId, float fNewTravelTime)
        {
            // Update node distance
            if (m_sortedNodes != null && m_nodes != null && nodeId != 0 && !m_visited.Contains(nodeId) && fNewTravelTime > 0)
            {
                float fTravelTime;
                if (m_nodes.TryGetValue(nodeId, out fTravelTime))
                {
                    // We have already seen this node. Check if new distance is smaller than current distance (Shorter path)
                    if (fNewTravelTime < fTravelTime)
                    {
                        m_nodes[nodeId] = fNewTravelTime;
                    }
                }
                else
                {
                    m_nodes[nodeId] = fNewTravelTime;
                }

                // Add node to priority queue so we can determine next node to visit
                m_sortedNodes.Push(new QueueData(nodeId, fNewTravelTime, GetNodeEstimateToCandidates(nodeId)));
            }
        }

        // A* Hueristic, find smallest LOS from node to cadidate positions
        // Ensures we are expanding nodes in the right direction instead of
        // heading in the wrong direction wasting time.
        private float GetNodeEstimateToCandidates(uint nodeId)
        {
            if (m_candidatePositions != null)
            {
                float fMinDistance = float.MaxValue;

                NetNode node = NetNodes.m_buffer[nodeId];
                if (node.m_flags != 0)
                {
                    Vector3 nodePosition = node.m_position;
                    foreach (Vector3 position in m_candidatePositions)
                    {
                        if (position != Vector3.zero)
                        {
                            float fDistance = (float)Vector3.SqrMagnitude(nodePosition - position);
                            if (fDistance < fMinDistance)
                            {
                                fMinDistance = fDistance;
                            }
                        }
                    }
                }

                if (fMinDistance != float.MaxValue)
                {
                    // We scale distance so it has a bigger impact
                    return (float)Math.Sqrt(fMinDistance) * 2.0f;
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
                            if (PathNodeCache.GetOutsideSegmentDistance(segmentId, out fEffectiveTravelTime))
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
            return IsNetInfoValid(info, m_laneTypes, m_service1, m_service2, m_service3, m_bPedestrianZone);
        }

        public static bool IsNetInfoValid(NetInfo info, NetInfo.LaneType laneTypes, ItemClass.Service service1, ItemClass.Service service2, ItemClass.Service service3, bool bPedestrianZone)
        {
            return (info.m_laneTypes & laneTypes) != 0 &&
                (info.m_class.m_service == service1 || info.m_class.m_service == service2 || info.m_class.m_service == service3) &&
                (bPedestrianZone || !info.IsPedestrianZoneRoad());
        }
    }
}
