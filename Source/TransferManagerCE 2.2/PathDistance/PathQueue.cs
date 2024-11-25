using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Common;

namespace TransferManagerCE
{
    public class PathQueue
    {
        // --------------------------------------------------------------------
        public class QueueData
        {
            private ushort m_nodeId;
            private float m_fTravelTime = float.MaxValue; // Current travel time to this node
            private float m_fPriority = float.MaxValue; // priority for this node (TravelTime + Heuristic)

            // Based on the PathDistanceAccuracy slider
            private static float s_fNodeFactorScale = 2.0f; // 80%

            public QueueData(ushort nodeId, float fTravelTime, float fHeuristic)
            {
                m_nodeId = nodeId;
                m_fTravelTime = fTravelTime;
                m_fPriority = s_fNodeFactorScale * fHeuristic + m_fTravelTime; // Scale heuristic by user accuracy preference
            }

            public ushort Node()
            {
                return m_nodeId;
            }

            public float TravelTime()
            {
                return m_fTravelTime;
            }
            public float Heuristic()
            {
                return (m_fPriority - m_fTravelTime) / s_fNodeFactorScale;
            }
            
            public float Priority()
            {
                return m_fPriority;
            }

            public void UpdateTravelTime(float fTravelTime)
            {
                // Extract heuristic
                float fHeuristic = m_fPriority - m_fTravelTime;
                // Update travel time
                m_fTravelTime = fTravelTime;
                // Re-calc priority
                m_fPriority = fHeuristic + m_fTravelTime;
            }

            public static void UpdateHeuristicScale()
            {
                s_fNodeFactorScale = (SaveGameSettings.GetSettings().PathDistanceHeuristic * 0.01f) * 2.5f;
            }
        }

        public class QueueDataComparer : IComparer<QueueData>
        {
            int IComparer<QueueData>.Compare(QueueData x, QueueData y)
            {
                return x.Priority() < y.Priority() ? -1 : 1;
            }
        }

        // --------------------------------------------------------------------
        private PriorityQueue<QueueData> m_sortedNodes = new PriorityQueue<QueueData>(Singleton<NetManager>.instance.m_nodeCount, new QueueDataComparer());

        // --------------------------------------------------------------------
        public void Push(QueueData data)
        {
            m_sortedNodes.Push(data);
        }

        // --------------------------------------------------------------------
        public QueueData Pop()
        {
            QueueData data = m_sortedNodes.Top;
            m_sortedNodes.Pop();
            return data;
        }

        // --------------------------------------------------------------------
        public void Clear()
        {
            m_sortedNodes.Clear();
        }

        // --------------------------------------------------------------------
        public int Count
        {
            get
            {
                return m_sortedNodes.Count;
            }
        }
    }
}
