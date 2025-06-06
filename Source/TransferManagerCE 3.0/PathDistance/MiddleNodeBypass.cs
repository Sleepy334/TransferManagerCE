using KianCommons;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static TransferManagerCE.NodeLinkData;

namespace TransferManagerCE
{
    internal class MiddleNodeBypass
    {
        const int iMIN_LINK_COUNT = 5;

        private Dictionary<ushort, NodeLinkData> m_data;
        private Dictionary<ushort, NodeLinkData> m_newLinks = new Dictionary<ushort, NodeLinkData>();

        public MiddleNodeBypass(Dictionary<ushort, NodeLinkData> data)
        {
            m_data = data;

        }

        public void Bypass()
        {
            ushort[] keys = m_data.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                ushort startNodeId = keys[i];
                NodeLinkData data = m_data[startNodeId];
                if (data.Count != 2)
                {
                    // this is a junction or a dead end, follow all middle nodes
                    NodeLink[] links = data.items.ToArray();
                    for (int j = 0; j < links.Length; j++)
                    {
                        NodeLink link = links[j];
                        float fTravelTime = link.m_fTravelTime;
                        int iNodeCount = 1;
                        FollowMiddleNodes(startNodeId, startNodeId, link.m_nodeId, link.m_direction, ref fTravelTime, ref iNodeCount);
                    }
                }
            }

            // Replace node data in graph
            foreach (KeyValuePair<ushort, NodeLinkData> kvp in m_newLinks)
            {
                m_data[kvp.Key] = kvp.Value;
            }
        }

        public void FollowMiddleNodes(ushort startNodeId, ushort prevNode, ushort nodeId, NetInfo.Direction direction, ref float fTravelTime, ref int iNodeCount)
        {
            if (m_data.TryGetValue(nodeId, out NodeLinkData linkData))
            {
                if (linkData.Count == 2)
                {
                    // Keep following node
                    NodeLink[] links = linkData.items.ToArray();
                    for (int i = 0; i < links.Length; i++)
                    {
                        NodeLink link = links[i];
                        if (link.m_nodeId != prevNode && (link.m_direction == NetInfo.Direction.Both || link.m_direction == direction))
                        {
                            fTravelTime += link.m_fTravelTime;
                            iNodeCount++;
                            FollowMiddleNodes(startNodeId, nodeId, link.m_nodeId, Min(direction, link.m_direction), ref fTravelTime, ref iNodeCount);
                        }
                    }
                }
                else
                {
                    // Last node
                    iNodeCount++;

                    if (iNodeCount >= iMIN_LINK_COUNT)
                    {
                        // Add link to new graph
                        //CDebug.Log($"Adding node link: {startNodeId} LinkNode: {nodeId} TravelTime: {fTravelTime} Direction: {direction}");
                        if (!m_newLinks.TryGetValue(startNodeId, out NodeLinkData data))
                        {
                            data = new NodeLinkData(m_data[startNodeId]); // Take a copy as we cant change in place while looping
                        }
                        data.Add(new NodeLink(nodeId, fTravelTime, direction, true));
                        m_newLinks[startNodeId] = data;
                    }
                }
            }
            else
            {
                CDebug.Log($"ERROR: Node: {nodeId} not found in graph.");
            }
        }

        private static NetInfo.Direction Min(NetInfo.Direction direction1, NetInfo.Direction direction2)
        {
            return (NetInfo.Direction) Math.Min((int) direction1, (int) direction2);
        }
    }
}
