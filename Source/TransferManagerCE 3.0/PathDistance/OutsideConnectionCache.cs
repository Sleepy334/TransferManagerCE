using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    internal class OutsideConnectionCache
    {
        // Stores buildings -> node
        private static Dictionary<ushort, ushort>? s_OutsideConnectionNodes = null;

        // Stores nodes -> multipliers 
        private static Dictionary<ushort, float>? s_OutsideNodeMultipliers = null;

        // Stores segmentId -> effective travel time
        private static Dictionary<ushort, float>? s_OutsideSegmentTravelTime = null;

        private static readonly object s_cacheLock = new object();

        private static Stopwatch s_stopwatch = Stopwatch.StartNew();
        public static long s_totalGenerationTicks = 0;
        public static int s_totalGenerations = 0;

        // ----------------------------------------------------------------------------------------
        public static void Invalidate()
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
                if (s_OutsideConnectionNodes is null)
                {
                    UpdateOutsideConnections();
                }

                if (s_OutsideConnectionNodes is not null && s_OutsideConnectionNodes.TryGetValue(buildingId, out ushort nodeId))
                {
                    return nodeId;
                }
                else
                {
                    // Load data for this connection
                    UpdateOutsideConnection(BuildingManager.instance.m_buildings.m_buffer, NetManager.instance.m_nodes.m_buffer, buildingId);

                    // Return node if found
                    if (s_OutsideConnectionNodes.TryGetValue(buildingId, out ushort nodeId2))
                    {
                        return nodeId2;
                    }
                }
            }

            return 0;
        }

        public static bool IsOutsideConnectionNode(ushort nodeId)
        {
            lock (s_cacheLock)
            {
                if (s_OutsideNodeMultipliers is null)
                {
                    UpdateOutsideConnections();
                }

                return s_OutsideNodeMultipliers.ContainsKey(nodeId);
            }
        }

        public static float GetOutsideNodeMultiplier(ushort nodeId)
        {
            lock (s_cacheLock)
            {
                if (s_OutsideNodeMultipliers is null)
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
                if (s_OutsideSegmentTravelTime is null)
                {
                    UpdateOutsideConnections();
                }

                fTravelTime = 0;
                if (s_OutsideSegmentTravelTime is not null)
                {
                    if (s_OutsideSegmentTravelTime.TryGetValue(segmentId, out fTravelTime))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Note: We override the actual travel time for this last segment to make this connection further away than local offers.
        private static void UpdateOutsideConnection(Building[] Buildings, NetNode[] Nodes, ushort buildingId)
        {
            lock (s_cacheLock)
            {
                Building building = Buildings[buildingId];
                if (building.m_flags != 0)
                {
                    ushort nodeId = FindOutsideConnectionNode(buildingId, building.m_position);
                    if (nodeId != 0)
                    {
                        NetNode node = Nodes[nodeId];
                        if (node.m_flags != 0 && node.m_building == buildingId)
                        {
                            if (s_OutsideConnectionNodes is null || s_OutsideNodeMultipliers is null || s_OutsideSegmentTravelTime is null)
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
                        RoadAccessStorage.AddInstance(new InstanceID { Building = buildingId });
                    }
                }
            }
        }

        private static void UpdateOutsideConnections()
        {
            long startTicks = s_stopwatch.ElapsedTicks;

            lock (s_cacheLock)
            {
                if (s_OutsideConnectionNodes is null || s_OutsideNodeMultipliers is null || s_OutsideSegmentTravelTime is null)
                {
                    Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;
                    NetNode[] Nodes = NetManager.instance.m_nodes.m_buffer;

                    // Get the list of outside connecions and update cache for each one
                    FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
                    foreach (var buildingId in connections)
                    {
                        UpdateOutsideConnection(Buildings, Nodes, buildingId);
                    }
                }
            }

            long stopTicks = s_stopwatch.ElapsedTicks;
            s_totalGenerationTicks += stopTicks - startTicks;
            s_totalGenerations++;
        }

        private static ushort FindOutsideConnectionNode(ushort buildingId, Vector3 pos)
        {
            ushort nodeId = 0;

            NodeUtils.EnumerateNearbyNodes(pos, 128f, (nodeID, node) =>
            {
                if ((node.m_flags & NetNode.Flags.Outside) != 0 && node.m_building == buildingId)
                {
                    nodeId = nodeID;
                    return false;
                }
                return true;
            });

            return nodeId;
        }
    }
}
