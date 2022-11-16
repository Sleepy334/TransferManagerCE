using System.Collections;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class ConnectedStorage : IEnumerable<KeyValuePair<ushort, int>>
    {
        private Dictionary<ushort, int> m_nodes;

        public ConnectedStorage() 
        {
            m_nodes = new Dictionary<ushort, int>();
        }

        public ConnectedStorage(ConnectedStorage oSecond)
        {
            m_nodes = new Dictionary<ushort, int>(oSecond.m_nodes);
        }

        public int Count
        {
            get { return m_nodes.Count; }
        }

        public bool HasVisited(ushort nodeId)
        {
            return m_nodes.ContainsKey(nodeId);
        }

        public int GetColor(ushort nodeId)
        {
            return m_nodes[nodeId];
        }

        public void SetColor(ushort nodeId, int color)
        {
            m_nodes[nodeId] = color;
        }

        public bool IsConnected(ushort nodeId1, ushort nodeId2)
        {
            if (HasVisited(nodeId1) && HasVisited(nodeId2))
            {
                return m_nodes[nodeId1] == m_nodes[nodeId2];
            }
            else
            {
                // It's a request from a new node so the graph has probably been changed
                // return true here as we don't know any better.
                return true;
            }
        }

        public IEnumerator<KeyValuePair<ushort, int>> GetEnumerator()
        {
            return m_nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_nodes.GetEnumerator();
        }
    }
}