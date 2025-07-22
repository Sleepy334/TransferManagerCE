using System.Collections.Generic;

namespace TransferManagerCE
{
    // Stores a list of nodes linked to the parent node and the travel time to get there.
    public class NodeLinkData
    {
        private List<NodeLink> m_nodeLinks;

        public NodeLinkData()
        {
            m_nodeLinks = new List<NodeLink>();
        }

        public NodeLinkData(NodeLinkData oSecond)
        {
            m_nodeLinks = new List<NodeLink>(oSecond.m_nodeLinks);
        } 

        public List<NodeLink> items
        {
            get 
            { 
                return m_nodeLinks; 
            }
        }

        public int Count
        {
            get 
            { 
                return m_nodeLinks.Count;
            }
        }

        public int ExcludeBypassCount
        {
            get
            {
                // Count nodes but ignore bypass nodes for classification
                int iCount = 0;
                foreach (NodeLink link in m_nodeLinks)
                {
                    if (!link.IsBypassNode())
                    {
                        iCount++;
                    }
                }

                return iCount;
            }
        }

        public void Add(NodeLink link)
        {
            m_nodeLinks.Add(link);
        }

        public void Add(ushort nodeId, float fTravelTime, NetInfo.Direction direction, ushort bypassNode)
        {
            m_nodeLinks.Add(new NodeLink(nodeId, fTravelTime, direction, bypassNode));
        }

        public void Clear()
        {
            m_nodeLinks.Clear();
        }

        public string GetNodeDescription()
        {
            string sText = "";

            // Ignore bypass nodes for classification
            switch (ExcludeBypassCount)
            {
                case 1:
                    {
                        sText += $"\nDead End";
                        break;
                    }
                case 2:
                    {
                        sText += $"\nMiddle Node";
                        break;
                    }
                default:
                    {
                        sText += $"\nJunction Node";
                        break;
                    }
            }

            return sText;
        }

        public override string ToString()
        {
            string sText = GetNodeDescription();
            sText += "\n\n<color #FFFFFF>Linked Nodes:</color>";
            foreach (NodeLink link in m_nodeLinks)
            {
                sText += "\n";

                if (link.IsBypassNode())
                {
                    sText += "<color #00AA00>";
                }

                sText += $"Node: {link.m_nodeId} TravelTime: {link.m_fTravelTime.ToString("N2")} Direction: {link.m_direction}";

                if (link.IsBypassNode())
                {
                    sText += $" BypassNode: {link.m_bypassNode}</color>";
                }

            }
            return sText;
        }

        public ushort GetActualNode(ushort nodeId)
        {
            foreach (NodeLink link in m_nodeLinks)
            {
                if (link.m_nodeId == nodeId)
                {
                    if (link.IsBypassNode())
                    {
                        return link.m_bypassNode;
                    }
                    else
                    {
                        return nodeId;
                    }
                }
            }

            return nodeId;
        }
    }            
}
