using ColossalFramework;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using TransferManagerCE.Common;

namespace TransferManagerCE
{
    public class PathQueue
    {
        // --------------------------------------------------------------------
        private PriorityQueue<PathData> m_sortedNodes = new PriorityQueue<PathData>(Singleton<NetManager>.instance.m_nodeCount);
        private Dictionary<ushort, PathData>? m_foundNodes = null;

        // --------------------------------------------------------------------
        public PathQueue(bool bStoreFoundNodes)
        {
            if (bStoreFoundNodes)
            {
                m_foundNodes = new Dictionary<ushort, PathData>();
            }
        }

        // --------------------------------------------------------------------
        public void Push(PathData data)
        {
            m_sortedNodes.Push(data);

            if (m_foundNodes != null)
            {
                m_foundNodes[data.Key] = data;
            }
        }

        // --------------------------------------------------------------------
        public PathData Pop()
        {
            PathData data = m_sortedNodes.Top;
            m_sortedNodes.Pop();
            return data;
        }

        // --------------------------------------------------------------------
        public void Update(PathData data)
        {
            m_sortedNodes.Update(data);

            if (m_foundNodes != null)
            {
                m_foundNodes[data.Key] = data;
            }
        }

        // --------------------------------------------------------------------
        public bool TryGetValue(ushort Key, out PathData Value)
        {
            return m_sortedNodes.TryGetValue(Key, out Value);
        }

        // --------------------------------------------------------------------
        public void Clear()
        {
            m_sortedNodes.Clear();

            if (m_foundNodes != null)
            {
                m_foundNodes.Clear();
            }
        }

        // --------------------------------------------------------------------
        public int Count
        {
            get
            {
                return m_sortedNodes.Count;
            }
        }

        // --------------------------------------------------------------------
        public Dictionary<ushort, PathData>? GetExaminedNodes()
        {
            return m_foundNodes;
        }
    }
}
