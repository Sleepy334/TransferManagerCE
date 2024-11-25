using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace TransferManagerCE
{
    internal class PathNodeCache
    {
        const float fOUTSIDE_CONNECTION_TRAVEL_TIME = 3000.0f;

        // Stores buildings and nodes for quick conversion to nodes for path finding
        private static Dictionary<ushort, ushort>? s_OutsideConnections = null;

        // Stores segmentId and effective distance for segment
        private static Dictionary<ushort, float>? s_OutsideSegments = null;

        static readonly object s_cacheLock = new object();

        public static void InvalidateOutsideConnections()
        {
            lock (s_cacheLock)
            {
                s_OutsideConnections = null;
                s_OutsideSegments = null;
            }
        }

        public static bool GetOutsideSegmentDistance(ushort segmentId, out float fDistance)
        {
            UpdateOutsideConnections();

            lock (s_cacheLock)
            {
                fDistance = 0;
                if (s_OutsideSegments != null)
                {
                    if (s_OutsideSegments.TryGetValue(segmentId, out fDistance))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static ushort FindCachedOutsideNode(ushort buildingId)
        {
            UpdateOutsideConnections();

            lock (s_cacheLock)
            {
                if (s_OutsideConnections != null && s_OutsideConnections.ContainsKey(buildingId))
                {
                    return s_OutsideConnections[buildingId];
                }
            }

            return 0;
        }

        private static void UpdateOutsideConnections()
        {
            lock (s_cacheLock)
            {
                if (s_OutsideConnections == null)
                {
                    s_OutsideConnections = new Dictionary<ushort, ushort>();

                    Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;

                    FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
                    foreach (var buildingId in connections)
                    {
                        Building building = Buildings[buildingId];
                        if (building.m_flags != 0)
                        {
                            ushort nodeId = FindNearestOutsideConnectionNode(PathNode.GetPosition(new InstanceID { Building = buildingId }), building.Info.GetService(), building.Info.GetSubService());
                            if (nodeId != 0)
                            {
                                s_OutsideConnections[buildingId] = nodeId;

                                float fDistanceMultiplier = BuildingSettings.GetEffectiveOutsideMultiplier(buildingId);
                                if (fDistanceMultiplier > 1.0f)
                                {
                                    // Find the segment for this node
                                    NetNode node = NetManager.instance.m_nodes.m_buffer[nodeId];
                                    if (node.m_flags != 0)
                                    {
                                        // Loop through segments to find neighboring roads
                                        for (int i = 0; i < 8; ++i)
                                        {
                                            ushort segmentId = node.GetSegment(i);
                                            if (segmentId != 0)
                                            {
                                                NetSegment segment = NetManager.instance.m_segments.m_buffer[segmentId];
                                                if (segment.m_flags != 0)
                                                {
                                                    if (s_OutsideSegments == null)
                                                    {
                                                        s_OutsideSegments = new Dictionary<ushort, float>();
                                                    }
                                                    s_OutsideSegments[segmentId] = fDistanceMultiplier * fOUTSIDE_CONNECTION_TRAVEL_TIME;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                RoadAccessData.AddInstance(new InstanceID { Building = buildingId });
                            }
                        }
                    }
                }
            }
        }

        private static ushort FindNearestOutsideConnectionNode(Vector3 pos, ItemClass.Service service, ItemClass.SubService subService)
        {
            NetNode[] NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            float maxDistance = 128f;
            
            Bounds bounds = new Bounds(pos, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            Vector3 min = bounds.min;
            int minx = Mathf.Max((int)((min.x - 64f) / 64f + 135f), 0);
            Vector3 min2 = bounds.min;
            int minz = Mathf.Max((int)((min2.z - 64f) / 64f + 135f), 0);
            Vector3 max = bounds.max;
            int maxx = Mathf.Min((int)((max.x + 64f) / 64f + 135f), 269);
            Vector3 max2 = bounds.max;
            int maxz = Mathf.Min((int)((max2.z + 64f) / 64f + 135f), 269);

            float nearestDistance = float.PositiveInfinity;
            float distMetric;
            ushort nearestClassNode = 0;

            for (int z = minz; z <= maxz; z++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    int iLoopCount = 0;
                    ushort nodeId = NetManager.instance.m_nodeGrid[(z * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x];
                    while (nodeId != 0)
                    {
                        NetNode node = NetNodes[nodeId];
                        if ((node.m_flags & NetNode.Flags.Outside) != 0 &&
                            node.Info.GetService() == service &&
                            node.Info.GetSubService() == subService)
                        {
                            distMetric = Vector3.SqrMagnitude(pos - node.m_position);
                            if (distMetric < nearestDistance)
                            {
                                nearestDistance = distMetric;
                                nearestClassNode = nodeId;
                            }
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

            return nearestClassNode;
        }
    }
}
