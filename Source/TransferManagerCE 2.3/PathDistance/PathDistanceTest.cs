using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    internal class PathDistanceTest
    {
#if DEBUG
        public const bool PATH_TESTING_ENABLED = false;
#else
        public const bool PATH_TESTING_ENABLED = false;
#endif

        public static List<QueueData> s_nodesExamined = new List<QueueData>();

        private PathDistance m_pathDistance = null;

        public PathDistanceTest()
        {
            m_pathDistance = new PathDistance();
        }

        public void FindNearestNeighbour(CustomTransferReason.Reason material, ushort buildingId, ushort[] candidates)
        {
            s_nodesExamined.Clear();

            // Set up lane requirements
            m_pathDistance.SetMaterial(material);

            ushort uiStartNode = FindBuildingNode(material, buildingId, true);
            if (uiStartNode != 0)
            {
                // Create a list of node candidates to send to the path distance algorithm
                foreach (ushort candidateId in candidates)
                {
                    ushort nodeId = FindBuildingNode(material, candidateId, false);
                    if (nodeId != 0)
                    {
                        m_pathDistance.AddCandidate(nodeId, candidateId);
                    }
                }

                // Calculate travel time
                int iChosenBuilding = m_pathDistance.FindNearestNeighborId(true, uiStartNode, out float fTravelTime, out long ticks, out int iNodesExamined, s_nodesExamined);
                
                Debug.Log($"ChosenBuilding:{iChosenBuilding} TravelTime:{fTravelTime} NodesExamined: {iNodesExamined} Ticks: {ticks}");
            }
        }

        private static ushort FindBuildingNode(CustomTransferReason.Reason material, ushort building, bool bActive)
        {
            ushort uiNearestNodeId = 0;

            ushort segmentId = PathNode.FindStartSegmentBuilding(building, material);
            if (segmentId != 0)
            {
                NetSegment segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentId];
                uiNearestNodeId = PathNode.GetStartNode(segmentId, segment, bActive);
            }

            return uiNearestNodeId;
        }
    }
}
