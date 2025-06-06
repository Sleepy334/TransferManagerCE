using System;
using System.Collections.Generic;
using static TransferManagerCE.NetworkModeHelper;

namespace TransferManagerCE
{
    public class PathDistanceTest
    {
        private PathDistance m_pathDistance = null;
        private ushort m_startNodeId = 0;
        private ushort m_chosenNodeId = 0;
        private int m_chosenBuildingId = 0;
        private Dictionary<ushort, PathData> m_nodesExamined = new Dictionary<ushort, PathData>();
        private long m_ticks = 0;
        private float m_travelTime = 0.0f;

        // ----------------------------------------------------------------------------------------
        public PathDistanceTest()
        {
            // Force path calculation even for 1 candidate
            m_pathDistance = new PathDistance(true, false); 
        }

        public ushort StartNodeId
        {
            get { return m_startNodeId; }
        }

        public int ChosenBuildingId
        {
            get { return m_chosenBuildingId; }
        }

        public long ChosenNodeId
        {
            get { return m_chosenNodeId; }
        }

        public long Ticks
        {
            get { return m_ticks; }
        }

        public float TravelTime
        {
            get { return m_travelTime; }
        }

        public void FindNearestNeighbour(NetworkMode mode, bool bStartActive, ushort buildingId, ushort[] candidates)
        {
            m_nodesExamined.Clear();
            m_chosenBuildingId = 0;
            m_chosenNodeId = 0;

            // Set up lane requirements
            m_pathDistance.SetNetworkMode(mode);

            // Check it is still valid
            PathDistanceCache.UpdateCache(mode);

            ushort uiStartNode = FindBuildingNode(CustomTransferReason.Reason.Goods, buildingId, bStartActive);
            if (uiStartNode != 0)
            {
                m_startNodeId = uiStartNode;

                // Create a list of node candidates to send to the path distance algorithm
                m_pathDistance.Candidates.Clear();
                foreach (ushort candidateId in candidates)
                {
                    ushort nodeId = FindBuildingNode(CustomTransferReason.Reason.Goods, candidateId, !bStartActive);
                    if (nodeId != 0 && PathConnectedCache.IsConnected(mode, m_startNodeId, nodeId))
                    {
                        m_pathDistance.Candidates.Add(nodeId, candidateId);
                    }
                }

                // Calculate travel time
                int iResult = m_pathDistance.FindNearestNeighborId(bStartActive, uiStartNode, out m_chosenNodeId, out m_travelTime, out m_ticks, out int iNodesExamined);
                if (iResult > 0)
                {
                    m_chosenBuildingId = iResult;
                }

                // Store nodes
                m_nodesExamined = m_pathDistance.GetExaminedNodes();
            }
        }

        public void Clear()
        {
            m_pathDistance.Candidates.Clear();
            m_nodesExamined.Clear();
            m_chosenNodeId = 0;
            m_startNodeId = 0;
            m_travelTime = 0.0f;
            m_ticks = 0;
            m_chosenBuildingId = 0;
        }

        public Dictionary<ushort, PathData> GetExaminedNodes()
        {
            return m_nodesExamined;
        }

        public HashSet<ushort> GetChosenPathNodes()
        {
            HashSet<ushort> chosenPath = new HashSet<ushort>();

            chosenPath.Add(m_startNodeId);
            chosenPath.Add(m_chosenNodeId);

            ushort nodeId = m_chosenNodeId;
            while (nodeId != 0)
            {
                chosenPath.Add(nodeId);

                PathData node = m_nodesExamined[nodeId];
                nodeId = node.prevId;
            }

            return chosenPath;
        }

        private static ushort FindBuildingNode(CustomTransferReason.Reason material, ushort buildingId, bool bActive)
        {
            ushort segmentId = PathNode.FindStartSegmentBuilding(buildingId, material);
            if (segmentId != 0)
            {
                return PathNode.FindNearestNode(buildingId, segmentId, bActive);
            }

            return 0;
        }
    }
}
