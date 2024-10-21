using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TransferManagerCE
{
    public static class NodeUtils
    {
        public delegate bool NodeDelegate(ushort nodeID, NetNode node); // Return true to continue loop

        public static void EnumerateNearbyNodes(Vector3 pos, float maxDistance, NodeDelegate func)
        {
            NetNode[] NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            ushort[] NodeGrid = Singleton<NetManager>.instance.m_nodeGrid;

            Bounds bounds = new Bounds(pos, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int minx = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int minz = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int maxx = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int maxz = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);

            for (int z = minz; z <= maxz; z++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    int iLoopCount = 0;
                    ushort nodeId = NodeGrid[(z * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x];
                    while (nodeId != 0)
                    {
                        NetNode node = NetNodes[nodeId];

                        if (!func(nodeId, node))
                        {
                            return;
                        }

                        nodeId = node.m_nextGridNode;

                        if (++iLoopCount >= NetManager.MAX_NODE_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }
    }
}
