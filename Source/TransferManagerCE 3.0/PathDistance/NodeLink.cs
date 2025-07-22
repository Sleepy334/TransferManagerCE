namespace TransferManagerCE
{
    public struct NodeLink
    {
        public ushort m_nodeId;
        public float m_fTravelTime;
        public NetInfo.Direction m_direction;
        public ushort m_bypassNode = 0;

        // --------------------------------------------------------------------
        public NodeLink()
        {
            m_nodeId = 0;
            m_fTravelTime = 0.0f;
            m_direction = NetInfo.Direction.Both;
            m_bypassNode = 0;
        }

        public NodeLink(ushort nodeId, float fTravelTime, NetInfo.Direction direction, ushort bypassNode)
        {
            m_nodeId = nodeId;
            m_fTravelTime = fTravelTime;
            m_direction = direction;
            m_bypassNode = bypassNode;
        }

        public bool IsBypassNode()
        {
            return m_bypassNode != 0;
        }
    }
}