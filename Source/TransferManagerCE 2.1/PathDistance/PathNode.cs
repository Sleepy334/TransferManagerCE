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
                        // We have a specialized function for buildings as they have the m_accessSegemtn field that we can use.
                        return FindStartSegmentBuilding(offer.Building, material);
                    }
                case InstanceType.Citizen:
                    {
                        // Can we get a building for this citizen
                        if (offer.GetBuilding() != 0)
                        {
                            // We have a specialized function for buildings as they have the m_accessSegment field that we can use.
                            return FindStartSegmentBuilding(offer.GetBuilding(), material);
                        }
                        break;
                    }
            }

            return FindPathPosition(material, offer.m_object);
        }

        private static ushort FindStartSegmentBuilding(ushort buildingId, TransferReason material)
        {
            Init();
            Building building = Buildings.m_buffer[buildingId];
            if (building.m_flags != 0)
            {
                InstanceID instance = new InstanceID { Building = buildingId };

                // Handle special cases
                switch (material)
                {
                    case TransferReason.Dead:
                        {
                            if (building.Info.GetAI() != null && building.Info.GetAI() is ParkBuildingAI)
                            {
                                // ParkBuildingAI.HandleDead2 doesn't use m_accessSegment but just calls FindRoadAccess.
                                // Match that so we pick up the dead for them even if it looks bad :-(
                                if (FindRoadAccess(buildingId, building, GetPosition(instance), out ushort segmentId))
                                {
                                    return segmentId;
                                }
                            }
                            break;
                        }
                }

                // Otherwise can we use the access segment?
                if (building.m_accessSegment != 0)
                {
                    NetSegment segment = NetSegments.m_buffer[building.m_accessSegment];
                    if (segment.m_flags != 0)
                    {
                        NetInfo.LaneType laneTypes = PathDistanceTypes.GetLaneTypes(material);
                        if ((segment.Info.m_laneTypes & laneTypes) != 0)
                        {
                            // this segment will do
                            return building.m_accessSegment;
                        }
                    } 
                }

                // Else try to find a valid start segment
                return FindPathPosition(material, instance);
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

        private static ushort FindPathPosition(TransferReason material, InstanceID instance)
        {
            // Default method, get position then find nearest segment.
            Vector3 position = GetPosition(instance);
            if (position != Vector3.zero)
            {
                ItemClass.Service service1;
                ItemClass.Service service2;
                ItemClass.Service service3;
                PathDistanceTypes.GetService(material, out service1, out service2, out service3);

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
                    RoadAccessData.AddInstance(instance);
#endif
                }
            }

            return 0;
        }

        private static bool FindRoadAccess(ushort buildingID, Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true)
        {
            Bounds bounds = new Bounds(position, new Vector3(40f, 40f, 40f));
            Vector3 min = bounds.min;
            int num = Mathf.Max((int)((min.x - 64f) / 64f + 135f), 0);
            Vector3 min2 = bounds.min;
            int num2 = Mathf.Max((int)((min2.z - 64f) / 64f + 135f), 0);
            Vector3 max = bounds.max;
            int num3 = Mathf.Min((int)((max.x + 64f) / 64f + 135f), 269);
            Vector3 max2 = bounds.max;
            int num4 = Mathf.Min((int)((max2.z + 64f) / 64f + 135f), 269);
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
    }
}