using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TransferManagerCE
{
    public class PathCandidates
    {
        public struct CandidateData {
            public int m_candidateId;
            public Vector3 m_position;
            public float m_multiplier;
        }

        const int iMAX_CANDIDATE_POSITIONS = 30;
        
        private readonly NetNode[] NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

        // Path candidate (nodeId, candidateId)
        private Dictionary<ushort, int> m_candidateData = new Dictionary<ushort, int>();

        // A* Heuristic
        private float[] m_candidatePositionX = new float[iMAX_CANDIDATE_POSITIONS];
        private float[] m_candidatePositionZ = new float[iMAX_CANDIDATE_POSITIONS];
        private float[] m_candidateLOSMultipliers = new float[iMAX_CANDIDATE_POSITIONS]; // Building LOS multipliers
        private int m_iCandidatePositionCount = 0;
        
        // A* Heuristic support
        private bool m_bUseHeuristic = SaveGameSettings.GetSettings().PathDistanceHeuristic > 0;

        public Dictionary<ushort, int> Items
        {
            get { return m_candidateData; }
        }

        public int Count
        {
            get
            {
                return m_candidateData.Count;
            }
        }

        public void Add(ushort nodeId, int id)
        {
            m_candidateData[nodeId] = id;

            // We keep 20 candidates only for speed
            if (m_iCandidatePositionCount < iMAX_CANDIDATE_POSITIONS)
            {
                NetNode node = NetNodes[nodeId];
                m_candidatePositionX[m_iCandidatePositionCount] = node.m_position.x;
                m_candidatePositionZ[m_iCandidatePositionCount] = node.m_position.z;
                m_iCandidatePositionCount++;
            }
        }

        public bool Contains(ushort nodeId, out int candidateId)
        {
            if (m_candidateData.TryGetValue(nodeId, out candidateId))
            {
                return true;
            }
                
            return false;
        }

        public bool ContainsKey(ushort nodeId)
        {
            return m_candidateData.ContainsKey(nodeId);
        }

        public void Clear()
        {
            m_candidateData.Clear();
            m_iCandidatePositionCount = 0;
        }

        // A* Hueristic, find smallest LOS from node to candidate positions
        // Ensures we are expanding nodes in the right direction instead of
        // heading in the wrong direction wasting time.
        // PERFORMANCE CRITICAL!
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public float GetNodeEstimateToCandidates(ushort nodeId)
        {
            if (m_bUseHeuristic)
            {
                float fMinDistanceSquared = float.MaxValue;

                NetNode node = NetNodes[nodeId];
                if (node.m_flags != 0)
                {
                    // Get nodes position and multiplier
                    float xpos = node.m_position.x;
                    float zpos = node.m_position.z;

                    for (int i = 0; i < m_iCandidatePositionCount; ++i)
                    {
                        // Need to make sure we apply outside connection multipliers here as well
                        float x = xpos - m_candidatePositionX[i];
                        float z = zpos - m_candidatePositionZ[i];
                        float fDistanceSquared = (x * x + z * z);
                        fMinDistanceSquared = Mathf.Min(fMinDistanceSquared, fDistanceSquared);
                    }
                }

                if (fMinDistanceSquared != float.MaxValue)
                {
                    return (float)Math.Sqrt(fMinDistanceSquared);
                }
                else
                {
                    return float.MaxValue;
                }
            }

            return 0f;
        }
    }
}