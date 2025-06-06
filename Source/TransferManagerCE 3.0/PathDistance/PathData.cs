using TransferManagerCE.Common;

namespace TransferManagerCE
{
    // --------------------------------------------------------------------
    public class PathData : PriorityQueueNode
    {
        const float fHEURISTIC_SCALE_FACTOR = 2.0f; // 50% is 1:1, 100% is twice as strong

        private ushort m_nodeId;
        private float m_fTravelTime = float.MaxValue; // Current travel time to this node
        private float m_fHeuristic = float.MaxValue;
        private float m_fPriority = float.MaxValue;
        private bool m_bVisited = false;
        private ushort m_prevNode = 0;

        // We can scale up the hueristic so it can dominate the calculation for speed
        // Based on the PathDistanceAccuracy slider
        private static float s_fHeuristicScale = 0.5f * fHEURISTIC_SCALE_FACTOR; // 50%

        // ----------------------------------------------------------------------------------------
        public PathData(ushort nodeId, ushort prevNode, float fTravelTime, float fHeuristic)
        {
            m_nodeId = nodeId;
            m_prevNode = prevNode;
            m_fTravelTime = fTravelTime;
            m_fHeuristic = fHeuristic; // Scale heuristic
            m_fPriority = fHeuristic * s_fHeuristicScale + fTravelTime;
        }

        public override ushort Key
        {
            get { return m_nodeId; }
        }

        public override float Priority
        {
            get
            {
                return m_fPriority;
            }
        }

        public bool visited
        {
            get
            {
                return m_bVisited;
            }
            set
            {
                m_bVisited = value;
            }
        }

        public ushort nodeId
        {
            get
            {
                return m_nodeId;
            }
            set
            {
                m_nodeId = value;
            }
        }

        public ushort prevId
        {
            get 
            { 
                return m_prevNode; 
            }
            set
            {
                m_prevNode = value;
            }
        }

        public float TravelTime()
        {
            return m_fTravelTime;
        }

        public float Heuristic()
        {
            return m_fHeuristic;
        }

        public void UpdateTravelTime(float fTravelTime)
        {
            // Update travel time
            m_fTravelTime = fTravelTime;

            // Update priority
            m_fPriority = Heuristic() * s_fHeuristicScale + TravelTime();
        }

        public static void UpdateHeuristicScale()
        {
            s_fHeuristicScale = (SaveGameSettings.GetSettings().PathDistanceHeuristic * 0.01f) * fHEURISTIC_SCALE_FACTOR;
        }
    }
}