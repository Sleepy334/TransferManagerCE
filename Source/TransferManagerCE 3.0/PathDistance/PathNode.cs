using ColossalFramework;
using SleepyCommon;
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
        private static NetSegment[] NetSegments = null;
        private static NetNode[] NetNodes = null;
        private static Building[] Buildings = null;
        private static DistrictPark[] Parks = null;

        private static void Init()
        {
            if (s_bInitNeeded)
            {
                s_bInitNeeded = false;
                NetSegments = Singleton<NetManager>.instance.m_segments.m_buffer;
                NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
                Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                Parks = Singleton<DistrictManager>.instance.m_parks.m_buffer;
            }
        }

        public static ushort FindNearestNode(CustomTransferReason.Reason material, CustomTransferOffer offer)
        {
            ushort uiNearestNodeId = 0;

            // If it is a park request (m_isLocalPark > 0) then the material magically transports so don't use pathing
            if (offer.LocalPark == 0)
            {
                Init();

                if (offer.Building != 0 && offer.IsOutside())
                {
                    Building building = Buildings[offer.Building];
                    uiNearestNodeId = FindOutsideConnectionNode(offer.Building, building.m_position);
                }

                if (uiNearestNodeId == 0)
                {
                    ushort segmentId = FindStartSegment(material, offer);
                    if (segmentId != 0)
                    {
                        NetSegment segment = NetSegments[segmentId];

                        // Get starting node based on segment direction and active status
                        if (segment.Info.m_canCrossLanes)
                        {
                            // We need the closest node
                            Building building = Buildings[offer.Building];
                            uiNearestNodeId = GetClosestNode(building.m_position, segmentId, segment);
                        }
                        else
                        {
                            // We need the right node
                            uiNearestNodeId = GetStartNode(segmentId, segment, offer.Active);
                        }
                    }
                }
            }

            return uiNearestNodeId;
        }

        public static ushort FindNearestNode(ushort buildingId, ushort segmentId, bool bActive)
        {
            Init();

            ushort uiNearestNodeId = 0;
            if (buildingId != 0)
            {
                Building building = Buildings[buildingId];
                uiNearestNodeId = FindOutsideConnectionNode(buildingId, building.m_position);
            }
            if (uiNearestNodeId == 0)
            {
                if (segmentId != 0)
                {
                    NetSegment segment = NetSegments[segmentId];
                    if (segment.m_flags != 0)
                    {
                        // Get starting node based on segment direction and active status
                        if (segment.Info.m_canCrossLanes)
                        {
                            // We need the closest node
                            Building building = Buildings[buildingId];
                            uiNearestNodeId = GetClosestNode(building.m_position, segmentId, segment);
                        }
                        else
                        {
                            // We need the right node
                            uiNearestNodeId = GetStartNode(segmentId, segment, bActive);
                        }
                    }
                }
            }

            return uiNearestNodeId;
        }

        private static ushort GetClosestNode(Vector3 position, ushort segmentId, NetSegment segment)
        {
            ushort usNode = 0;

            if (segment.m_flags != 0)
            {
                NetNode start = NetNodes[segment.m_startNode];
                NetNode end = NetNodes[segment.m_endNode];

                float fDistanceStart = Vector3.SqrMagnitude(position - start.m_position);
                float fDistanceEnd = Vector3.SqrMagnitude(position - end.m_position);
                
                if (fDistanceStart < fDistanceEnd)
                {
                    usNode = segment.m_startNode;
                }
                else
                {
                    usNode = segment.m_endNode;
                }
            }

            return usNode;
        }

        private static ushort GetStartNode(ushort segmentId, NetSegment segment, bool bActive)
        {
            ushort usNode = 0;

            if (segment.m_flags != 0)
            {
                if (bActive)
                {
                    usNode = GetHeadNode(segmentId, segment);
                }
                else
                {
                    usNode = GetTailNode(segmentId, segment);
                }
            }

            return usNode;
        }

        private static ushort GetHeadNode(ushort segmentId, NetSegment segment)
        {
            ushort usNode = 0;
            if (segment.m_flags != 0)
            {
                if ((segment.m_flags & NetSegment.Flags.Invert) != 0)
                {
                    usNode = segment.m_startNode;
                }
                else
                {
                    usNode = segment.m_endNode; 
                }
            }

            return usNode;
        }

        private static ushort GetTailNode(ushort segmentId, NetSegment segment)
        {
            ushort usNode = 0;
            if (segment.m_flags != 0)
            {
                if ((segment.m_flags & NetSegment.Flags.Invert) != 0)
                {
                    usNode = segment.m_endNode;
                }
                else
                {
                    usNode = segment.m_startNode; 
                }
            }

            return usNode;
        }

        private static ushort FindStartSegment(CustomTransferReason.Reason material, CustomTransferOffer offer)
        {
            Init();

            switch (offer.m_object.Type)
            {
                case InstanceType.Building:
                    {
                        // We have a specialized function for buildings as they have the m_accessSegment field that we can use.
                        return FindStartSegmentBuilding(offer.Building, material);
                    }
                case InstanceType.Vehicle:
                    {
                        return FindPathPosition(material, offer.m_object);
                    }
                case InstanceType.Citizen:
                    {
                        // Can we get a building for this citizen
                        if (offer.GetBuilding() != 0)
                        {
                            // We have a specialized function for buildings as they have the m_accessSegment field that we can use.
                            return FindStartSegmentBuilding(offer.GetBuilding(), material);
                        }

                        return FindPathPosition(material, offer.m_object);
                    }
                case InstanceType.Park:
                    {
                        // We need to find a ServicePoint node instead
                        DistrictPark park = Parks[offer.Park];
                        if (park.m_flags != 0 && park.IsPedestrianZone)
                        {
                            // TryGetRandomServicePoint fails a lot, function seems buggy.
                            // Try to find all service points with capacity for this material
                            DistrictPark.PedestrianZoneTransferReason reason;
                            if (DistrictPark.TryGetPedestrianReason((TransferReason) material, out reason))
                            {
                                List<Building> servicePoints = new List<Building>();
                                foreach (ushort buildingId in park.m_finalServicePointList)
                                {
                                    Building building = Buildings[buildingId];
                                    if (building.m_flags != 0 && building.m_accessSegment != 0)
                                    {
                                        // Copied from TryGetRandomServicePoint
                                        ServicePointAI? servicePointAI = building.Info.m_buildingAI as ServicePointAI;
                                        if (servicePointAI is not null &&
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
                case InstanceType.NetSegment:
                    {
                        return offer.NetSegment;
                    }
                default:
                    {
                        CDebug.Log($"Type: {offer.m_object.Type} Index: {offer.m_object.Index}");
                        return FindPathPosition(material, offer.m_object);
                    }
            }
        }

        public static ushort FindStartSegmentBuilding(ushort buildingId, CustomTransferReason.Reason material)
        {
            Init();

            Building building = Buildings[buildingId];

            // Is there a parent building with road access
            if (building.m_flags != 0 && 
               (building.m_flags & Building.Flags.RoadAccessFailed) != 0 &&
                building.m_parentBuilding != 0)
            {
                buildingId = building.m_parentBuilding;
                building = Buildings[building.m_parentBuilding];
            }

            if (building.m_flags != 0 && (building.m_flags & Building.Flags.RoadAccessFailed) == 0)
            {
                InstanceID instance = new InstanceID { Building = buildingId };

                // Handle special cases
                switch (material)
                {
                    case CustomTransferReason.Reason.Dead:
                        {
                            if (building.Info.GetAI() is not null && building.Info.GetAI() is ParkBuildingAI)
                            {
                                // ParkBuildingAI.HandleDead2 doesn't use m_accessSegment but just calls FindRoadAccess.
                                // Match that so we pick up the dead for them even if it looks bad :-(
                                if (FindRoadAccess(buildingId, building, GetSidewalkPosition(instance), out ushort segmentId))
                                {
                                    return segmentId;
                                }
                            }
                            break;
                        }
                }

                if (building.m_accessSegment != 0)
                {
                    NetSegment segment = NetSegments[building.m_accessSegment];
                    if (segment.m_flags != 0)
                    {
                        NetInfo.LaneType laneTypes = PathDistanceTypes.GetLaneTypes(PathDistanceTypes.IsGoodsMaterial(material));
                        if ((segment.Info.m_laneTypes & laneTypes) != 0)
                        {
                            // this segment will do
                            return building.m_accessSegment;
                        }
                        else
                        {
                            // The access segment does not support requested vehicle type.
                            RoadAccessStorage.AddInstance(instance);
                        }
                    }
                }

                // Else try to find a valid start segment
                return FindPathPosition(material, instance);
            }

            return 0;
        }

        public static Vector3 GetSidewalkPosition(InstanceID instance)
        {
            Init();
            Vector3 position = Vector3.zero;

            switch (instance.Type)
            {
                case InstanceType.Building:
                    {
                        Building building = Buildings[instance.Building];
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

        private static ushort FindPathPosition(CustomTransferReason.Reason material, InstanceID instance)
        {
            // Default method, get position then find nearest segment.
            Vector3 position = GetSidewalkPosition(instance);
            if (position != Vector3.zero)
            {
                bool bIsGodsMaterial = PathDistanceTypes.IsGoodsMaterial(material);

                ItemClass.Service service1;
                ItemClass.Service service2;
                ItemClass.Service service3;
                PathDistanceTypes.GetService(bIsGodsMaterial, out service1, out service2, out service3);

                NetInfo.LaneType laneTypes = PathDistanceTypes.GetLaneTypes(bIsGodsMaterial);
                VehicleInfo.VehicleType vehicleType = PathDistanceTypes.GetVehicleTypes(bIsGodsMaterial);
                VehicleInfo.VehicleCategory vehicleCategory = PathDistanceTypes.GetVehicleCategory(bIsGodsMaterial);

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
                                                excludeLaneWidth: false,
                                                checkPedestrianStreet: true,
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
                    RoadAccessStorage.AddInstance(instance);
#endif
                }
            }

            return 0;
        }

        private static bool FindRoadAccess(ushort buildingID, Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true)
        {
            Bounds bounds = new Bounds(position, new Vector3(40f, 40f, 40f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);

            segmentID = 0;
            float num5 = float.MaxValue;
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    int num6 = 0;
                    for (ushort num7 = instance.m_segmentGrid[i * 270 + j]; num7 != 0; num7 = instance.m_segments.m_buffer[num7].m_nextGridSegment)
                    {
                        if (num6++ >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }

                        NetInfo info = instance.m_segments.m_buffer[num7].Info;
                        if (info.m_class.m_service == ItemClass.Service.Road && !info.m_netAI.IsUnderground() && !info.m_netAI.IsOverground() && info.m_netAI is RoadBaseAI && (untouchable || (instance.m_segments.m_buffer[num7].m_flags & NetSegment.Flags.Untouchable) == 0) && info.m_hasPedestrianLanes && (info.m_hasForwardVehicleLanes || info.m_hasBackwardVehicleLanes))
                        {
                            ushort startNode = instance.m_segments.m_buffer[num7].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[num7].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[endNode].m_position;
                            Vector3 min3 = bounds.min;
                            float a = min3.x - 64f - position2.x;
                            Vector3 min4 = bounds.min;
                            float a2 = Mathf.Max(a, min4.z - 64f - position2.z);
                            float x = position2.x;
                            Vector3 max3 = bounds.max;
                            float a3 = x - max3.x - 64f;
                            float z = position2.z;
                            Vector3 max4 = bounds.max;
                            float num8 = Mathf.Max(a2, Mathf.Max(a3, z - max4.z - 64f));
                            Vector3 min5 = bounds.min;
                            float a4 = min5.x - 64f - position3.x;
                            Vector3 min6 = bounds.min;
                            float a5 = Mathf.Max(a4, min6.z - 64f - position3.z);
                            float x2 = position3.x;
                            Vector3 max5 = bounds.max;
                            float a6 = x2 - max5.x - 64f;
                            float z2 = position3.z;
                            Vector3 max6 = bounds.max;
                            float num9 = Mathf.Max(a5, Mathf.Max(a6, z2 - max6.z - 64f));
                            if ((!(num8 >= 0f) || !(num9 >= 0f)) && instance.m_segments.m_buffer[num7].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[num7].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.RoadTransport, VehicleInfo.VehicleType.None, requireConnect: false, out Vector3 positionA, out int _, out float _, out Vector3 _, out int _, out float _))
                            {
                                float num10 = Vector3.SqrMagnitude(position - positionA);
                                if (!(num10 >= 400f) && !(num10 >= num5))
                                {
                                    segmentID = num7;
                                    if (!mostCloser)
                                    {
                                        return true;
                                    }

                                    num5 = num10;
                                }
                            }
                        }
                    }
                }
            }

            return segmentID != 0;
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

        public static ushort FindBuildingNode(CustomTransferReason.Reason material, ushort buildingId, bool bActive)
        {
            ushort segmentId = PathNode.FindStartSegmentBuilding(buildingId, material);
            if (segmentId != 0)
            {
                return PathNode.FindNearestNode(buildingId, segmentId, bActive);
            }

            return 0;
        }
    }
}