using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using static TransferManager;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    internal class PathDistanceTest
    {
#if DEBUG
        public const bool PATH_TESTING_ENABLED = true;
#else 
        public const bool PATH_TESTING_ENABLED = false;
#endif

        public static List<QueueData> s_nodesExamined = new List<QueueData>();

        public static void FindNearestNeighbour(TransferReason material, ushort buildingId, ushort[] candidates)
        {
            s_nodesExamined.Clear();

            PathDistance pathDistance = new PathDistance();

            // Set up lane requirements
            PathDistanceTypes.GetService(material, out pathDistance.m_service1, out pathDistance.m_service2, out pathDistance.m_service3);
            pathDistance.m_laneTypes = PathDistanceTypes.GetLaneTypes(material);
            pathDistance.m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(material);
            pathDistance.m_bPedestrianZone = PathDistanceTypes.IsPedestrianZoneService(material);

            ushort uiStartNode = FindBuildingNode(material, buildingId, true);
            if (uiStartNode != 0)
            {
                // Create a list of node candidates to send to the path distance algorithm
                foreach (ushort candidateId in candidates)
                {
                    ushort nodeId = FindBuildingNode(material, candidateId, false);
                    if (nodeId != 0)
                    {
                        pathDistance.AddCandidate(nodeId, candidateId);
                    }
                }

                // Calculate travel time
                int iChosenBuilding = pathDistance.FindNearestNeighborId(true, uiStartNode, out float fTravelTime, out long ticks, out int iNodesExamined, s_nodesExamined);
                
                Debug.Log($"ChosenBuilding:{iChosenBuilding} NodesExamined: {iNodesExamined} Ticks: {ticks}");
            }
        }

        private static ushort FindBuildingNode(CustomTransferReason material, ushort building, bool bActive)
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
