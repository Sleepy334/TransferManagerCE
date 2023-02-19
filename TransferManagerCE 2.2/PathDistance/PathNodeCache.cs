using ICities;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    internal class PathNodeCache
    {
        // Stores buildings -> node
        private static Dictionary<ushort, ushort>? s_OutsideConnectionNodes = null;

        // Stores nodes -> multipliers 
        private static Dictionary<ushort, float>? s_OutsideNodeMultipliers = null;

        // Stores segmentId -> effective travel time
        private static Dictionary<ushort, float>? s_OutsideSegmentTravelTime = null;

        private static readonly object s_cacheLock = new object();

        public static void InvalidateOutsideConnections()
        {
            lock (s_cacheLock)
            {
                s_OutsideConnectionNodes = null;
                s_OutsideNodeMultipliers = null;
                s_OutsideSegmentTravelTime = null;
            }
        }

        public static ushort FindCachedOutsideNode(ushort buildingId)
        {
            lock (s_cacheLock)
            {
                if (s_OutsideConnectionNodes == null)
                {
                    UpdateOutsideConnections();
                }

                if (s_OutsideConnectionNodes != null && s_OutsideConnectionNodes.TryGetValue(buildingId, out ushort nodeId))
                {
                    return nodeId;
                }
                else
                {
                    // Load data for this connection
                    UpdateOutsideConnection(BuildingManager.instance.m_buildings.m_buffer, buildingId);

                    // Return node if found
                    if (s_OutsideConnectionNodes.TryGetValue(buildingId, out ushort nodeId2))
                    {
                        return nodeId2;
                    }
                }
            }

            return 0;
        }

        public static float GetOutsideNodeMultiplier(ushort nodeId)
        {
            lock (s_cacheLock)
            {
                if (s_OutsideNodeMultipliers == null)
                {
                    UpdateOutsideConnections();
                }

                if (s_OutsideNodeMultipliers.TryGetValue(nodeId, out float fMultiplier))
                {
                    return fMultiplier;
                }
            }

            return 1.0f;
        }

        public static bool GetOutsideSegmentTravelTime(ushort segmentId, out float fTravelTime)
        {
            lock (s_cacheLock)
            {
                if (s_OutsideSegmentTravelTime == null)
                {
                    UpdateOutsideConnections();
                }

                fTravelTime = 0;
                if (s_OutsideSegmentTravelTime != null)
                {
                    if (s_OutsideSegmentTravelTime.TryGetValue(segmentId, out fTravelTime))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void UpdateOutsideConnection(Building[] Buildings, ushort buildingId)
        {
            lock (s_cacheLock)
            {
                Building building = Buildings[buildingId];
                if (building.m_flags != 0)
                {
                    ushort nodeId = FindNearestOutsideConnectionNode(PathNode.GetPosition(new InstanceID { Building = buildingId }), building.Info.GetService(), building.Info.GetSubService());
                    if (nodeId != 0)
                    {
                        if (s_OutsideConnectionNodes == null || s_OutsideNodeMultipliers == null || s_OutsideSegmentTravelTime == null)
                        {
                            s_OutsideConnectionNodes = new Dictionary<ushort, ushort>();
                            s_OutsideNodeMultipliers = new Dictionary<ushort, float>();
                            s_OutsideSegmentTravelTime = new Dictionary<ushort, float>();
                        }

                        // Add the outside connection data
                        s_OutsideConnectionNodes[buildingId] = nodeId;
                        float fEffectiveMultiplier = BuildingSettingsFast.GetEffectiveOutsideMultiplier(buildingId);
                        s_OutsideNodeMultipliers[nodeId] = fEffectiveMultiplier;

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
                                        s_OutsideSegmentTravelTime[segmentId] = fEffectiveMultiplier * SaveGameSettings.GetSettings().PathDistanceTravelTimeBaseValue;
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

        private static void UpdateOutsideConnections()
        {
            lock (s_cacheLock)
            {
                if (s_OutsideConnectionNodes == null || s_OutsideNodeMultipliers == null || s_OutsideSegmentTravelTime == null)
                {
                    Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;

                    // Get the list of outside connecions and update cache for each one
                    FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
                    foreach (var buildingId in connections)
                    {
                        UpdateOutsideConnection(Buildings, buildingId);
                    }
                }
            }
        }

        private static ushort FindNearestOutsideConnectionNode(Vector3 pos, ItemClass.Service service, ItemClass.SubService subService)
        {
            float nearestDistance = float.PositiveInfinity;
            float distMetric;
            ushort nearestClassNode = 0;

            NodeUtils.EnumerateNearbyNodes(pos, 128f, (nodeID, node) =>
            {
                if ((node.m_flags & NetNode.Flags.Outside) != 0 &&
                            node.Info.GetService() == service &&
                            node.Info.GetSubService() == subService)
                {
                    distMetric = Vector3.SqrMagnitude(pos - node.m_position);
                    if (distMetric < nearestDistance)
                    {
                        nearestDistance = distMetric;
                        nearestClassNode = nodeID;
                    }
                }
            });

            return nearestClassNode;
        }
    }
}
