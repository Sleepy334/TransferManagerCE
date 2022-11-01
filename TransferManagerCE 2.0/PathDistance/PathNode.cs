using ColossalFramework;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class PathNode
    {
        const float fMAX_SEARCH_DISTANCE = 128f;

        private static bool s_bInitNeeded = true;
        private static Array16<NetNode>? NetNodes = null;
        private static Array16<NetSegment>? NetSegments = null;
        private static Array32<NetLane>? NetLanes = null;
        private static Array16<Building>? Buildings = null;
        private static Array32<Citizen>? Citizens = null;
        private static Array16<Vehicle>? Vehicles = null;
        private static Array8<DistrictPark>? Parks = null;

        private static void Init()
        {
            if (s_bInitNeeded)
            {
                s_bInitNeeded = false;
                NetNodes = Singleton<NetManager>.instance.m_nodes;
                NetSegments = Singleton<NetManager>.instance.m_segments;
                NetLanes = Singleton<NetManager>.instance.m_lanes;
                Buildings = Singleton<BuildingManager>.instance.m_buildings;
                Citizens = Singleton<CitizenManager>.instance.m_citizens;
                Vehicles = Singleton< VehicleManager>.instance.m_vehicles;
                Parks = Singleton<DistrictManager>.instance.m_parks;
            }
        }

        public static ushort FindNearestNode(TransferReason material, CustomTransferOffer offer)
        {
            ushort uiNearestNodeId = 0;

            // If it is a park request (m_isLocalPark > 0) then the material magically transports so don't use pathing
            if (offer.m_offer.m_isLocalPark == 0)
            {
                Init();
                if (offer.Building != 0 && offer.IsOutside())
                {
                    uiNearestNodeId = PathNodeCache.FindCachedOutsideNode(offer.Building);
                }
                if (uiNearestNodeId == 0)
                {
                    ushort segmentId = FindStartSegment(material, offer);
                    if (segmentId != 0)
                    {
                        NetSegment segment = NetSegments.m_buffer[segmentId];
                        if (segment.m_flags != 0)
                        {
                            // TODO: Return closest node of the 2.
                            uiNearestNodeId = segment.m_startNode;
                        }
                    }
                }
            }
#if DEBUG
            if (uiNearestNodeId == 0)
            {
                Debug.Log($"Material: {material} Offer:{TransferManagerUtils.DebugOffer(offer)} NearestNodeId:{uiNearestNodeId}");
            }
#endif

            return uiNearestNodeId;
        }

        private static ushort FindStartSegment(TransferReason material, CustomTransferOffer offer)
        {
            Init();
            switch (offer.m_object.Type)
            {
                case InstanceType.Park:
                    {
                        // We need to find a ServicePoint node instead
                        DistrictPark park = Parks.m_buffer[offer.Park];
                        if (park.m_flags != 0 && park.IsPedestrianZone)
                        {
                            // TryGetRandomServicePoint fails a lot, function seems buggy.
                            // Try to find all service points with capacity for this material
                            DistrictPark.PedestrianZoneTransferReason reason;
                            if (DistrictPark.TryGetPedestrianReason(material, out reason))
                            {
                                List<Building> servicePoints = new List<Building>();
                                foreach (ushort buildingId in park.m_finalServicePointList)
                                {
                                    Building building = Buildings.m_buffer[buildingId];
                                    if (building.m_flags != 0 && building.m_accessSegment != 0)
                                    {
                                        // Copied from TryGetRandomServicePoint
                                        ServicePointAI? servicePointAI = building.Info.m_buildingAI as ServicePointAI;
                                        if (servicePointAI != null &&
                                            !servicePointAI.IsReachedCriticalTrafficLimit(buildingId, ref building, reason.m_deliveryCategory))
                                        {
                                            servicePoints.Add(building);
                                        }
                                    }
                                }

                                // We return a random service point in case one of the points has a broken path.
                                int iCount = servicePoints.Count;
                                if (iCount == 1)
                                {
                                    return servicePoints[0].m_accessSegment;
                                }
                                else if (iCount > 1)
                                {
                                    int iIndex = SimulationManager.instance.m_randomizer.Int32((uint)iCount);
                                    return servicePoints[iIndex].m_accessSegment;
                                }
                            }
                        }

                        // Don't fall through as we can't get a path to a park if we cant find a service point as there is no node.
                        return 0;
                    }
                case InstanceType.Building:
                    {
                        ushort segmentId = FindStartSegmentBuilding(offer.Building);
                        if (segmentId != 0)
                        {
                            return segmentId;
                        }
                        // Allow fall through
                        break;
                    }
                case InstanceType.Citizen:
                    {
                        Citizen citizen = Citizens.m_buffer[offer.Citizen];
                        if (citizen.m_flags != 0 && citizen.GetBuildingByLocation() != 0)
                        {
                            ushort buildingId = citizen.GetBuildingByLocation();
                            if (buildingId != 0)
                            {
                                ushort segmentId = FindStartSegmentBuilding(buildingId);
                                if (segmentId != 0)
                                {
                                    return segmentId;
                                }
                            }
                        }
                        // Allow fall through
                        break;
                    }
            }

            // Default method, get position then find nearest node.
            Vector3 position = GetPosition(offer.m_object);
            if (position != Vector3.zero)
            {
                ItemClass.Service service1;
                ItemClass.Service service2;
                ItemClass.Service service3;
                PathDistanceTypes.GetService(material, offer, out service1, out service2, out service3);

                NetInfo.LaneType laneTypes = PathDistanceTypes.GetLaneTypes(material);
                VehicleInfo.VehicleType vehicleType = PathDistanceTypes.GetVehicleTypes(material);
                VehicleInfo.VehicleCategory vehicleCategory = PathDistanceTypes.GetVehicleCategory(material);

                PathUnit.Position pathPosB;
                float distanceSqrA;
                float distanceSqrB;
                if (PathManager.FindPathPosition(position,
                                                service1,
                                                service2,
                                                service3,
                                                ItemClass.SubService.None,
                                                ItemClass.Level.None,
                                                laneTypes,
                                                vehicleType,
                                                vehicleCategory,
                                                VehicleInfo.VehicleType.None,
                                                allowUnderground: false,
                                                requireConnect: false,
                                                fMAX_SEARCH_DISTANCE,
                                                out PathUnit.Position pathStartPosA,
                                                out pathPosB,
                                                out distanceSqrA,
                                                out distanceSqrB))
                {
                    return pathStartPosA.m_segment;
                }
                else
                {
#if DEBUG
                    // Add these to "No Road Access" panel in debug so we can see when it isn't working
                    RoadAccessData.AddInstance(offer.m_object);
#endif
                }
            }
            
            return 0;
        }

        public static ushort FindStartSegmentBuilding(ushort buildingId)
        {
            Init();
            Building building = Buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                // Copied from TransferManager.AddIncoming.
                if (building.m_accessSegment == 0 && (building.m_problems & new Notification.ProblemStruct(Notification.Problem1.RoadNotConnected, Notification.Problem2.NotInPedestrianZone)).IsNone)
                {
                    // See if we can update m_accessSegment.
                    building.Info.m_buildingAI.CheckRoadAccess(buildingId, ref building);
                    if (building.m_accessSegment == 0)
                    {
                        RoadAccessData.AddInstance(new InstanceID { Building = buildingId });
                    }
                }

                if (building.m_accessSegment != 0)
                {
                    return building.m_accessSegment;
                }
            }

            return 0;
        }

        public static Vector3 GetPosition(InstanceID instance)
        {
            Init();
            Vector3 position = Vector3.zero;

            switch (instance.Type)
            {
                case InstanceType.Building:
                    {
                        Building building = Buildings.m_buffer[instance.Building];
                        if ((building.m_flags & Building.Flags.Collapsed) == Building.Flags.None)
                        {
                            if (building.Info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
                            {
                                position = building.CalculateSidewalkPosition((float)building.Width * 4f, 4f);
                            }
                            else if (building.Info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
                            {
                                position = building.CalculateSidewalkPosition((float)building.Width * -4f, 4f);
                            }
                            else
                            {
                                position = building.CalculateSidewalkPosition(0f, 4f);
                            }
                        }
                        break;
                    }
                default:
                    {
                        position = InstanceHelper.GetPosition(instance);
                        break;
                    }
            }

            return position;
        }

        
            /*
            private static ushort FindNearestServiceNode(TransferReason material, bool bAllowUnderground, TransferOffer offer)
            {
                return FindNearestServiceNode(GetPosition(offer.m_object), false, TransferManagerModes.GetLaneTypes(material));
            }

            public static ushort FindNearestServiceNode(Vector3 pos, bool bAllowUnderground, NetInfo.LaneType laneTypes = NetInfo.LaneType.All)
            {
                Init();

                int x = Mathf.Clamp((int)((pos.x / 64) + (TransferManager.REASON_CELL_SIZE_LARGE / 2)), 0, (int)TransferManager.REASON_CELL_SIZE_LARGE - 1);
                int z = Mathf.Clamp((int)((pos.z / 64f) + (TransferManager.REASON_CELL_SIZE_LARGE / 2)), 0, (int)TransferManager.REASON_CELL_SIZE_LARGE - 1);

                float nearestDistance = float.PositiveInfinity;
                float distMetric;
                ushort nearestClassNode = 0;

                int iLoopCount = 0;
                ushort nodeId = NetManager.instance.m_nodeGrid[(z * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x];
                while (nodeId != 0)
                {
                    //                if (buildingID == 11691)
                    //                {
                    //                    Log._DebugFormat("Considering node #{0} of type {1} at dist=0", node, nets.m_nodes.m_buffer[node].Info.m_class.m_service);
                    //                }
                    NetNode node = NetNodes.m_buffer[nodeId];
                    if ((node.Info.m_laneTypes & laneTypes) != 0 && (bAllowUnderground || !node.Info.m_netAI.IsUnderground()))
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

                if (nearestClassNode == 0)
                {
                    // Didn't find a node, perform a grid search
                    uint maxdist = 4;
                    x = Math.Max(x, (int)maxdist);
                    x = Math.Min(x, (int)TransferManager.REASON_CELL_SIZE_LARGE - 1 - (int)maxdist);
                    z = Math.Max(z, (int)maxdist);
                    z = Math.Min(z, (int)TransferManager.REASON_CELL_SIZE_LARGE - 1 - (int)maxdist);

                    for (int dist = 1; dist <= maxdist; ++dist)
                    {
                        if (nearestClassNode != 0)
                        {
                            // Found a node stop looking
                            break;
                        }

                        for (int n = -dist; n <= dist; ++n)
                        {
                            iLoopCount = 0;
                            nodeId = NetManager.instance.m_nodeGrid[((z + dist) * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x + n];
                            while (nodeId != 0)
                            {
                                NetNode node = NetNodes.m_buffer[nodeId];
                                if ((node.Info.m_laneTypes & laneTypes) != 0 && (bAllowUnderground || !node.Info.m_netAI.IsUnderground()))
                                {
                                    distMetric = Vector3.SqrMagnitude(pos - NetNodes.m_buffer[nodeId].m_position);
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

                            iLoopCount = 0;
                            nodeId = NetManager.instance.m_nodeGrid[((z - dist) * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x + n];
                            while (nodeId != 0)
                            {
                                NetNode node = NetNodes.m_buffer[nodeId];
                                if ((node.Info.m_laneTypes & laneTypes) != 0 && (bAllowUnderground || !node.Info.m_netAI.IsUnderground()))
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

                            iLoopCount = 0;
                            nodeId = NetManager.instance.m_nodeGrid[((z + n) * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x - dist];
                            while (nodeId != 0)
                            {
                                NetNode node = NetNodes.m_buffer[nodeId];
                                if ((node.Info.m_laneTypes & laneTypes) != 0 && (bAllowUnderground || !node.Info.m_netAI.IsUnderground()))
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

                            iLoopCount = 0;
                            nodeId = NetManager.instance.m_nodeGrid[((z + n) * (int)TransferManager.REASON_CELL_SIZE_LARGE) + x + dist];
                            while (nodeId != 0)
                            {
                                NetNode node = NetNodes.m_buffer[nodeId];
                                if ((node.Info.m_laneTypes & laneTypes) != 0 && (bAllowUnderground || !node.Info.m_netAI.IsUnderground()))
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
                }

                return nearestClassNode;
            }
            */
        }
}