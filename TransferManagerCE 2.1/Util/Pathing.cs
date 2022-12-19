using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace TransferManagerCE
{
    public class Pathing
    {
        const float fMAX_SEARCH_DISTANCE = 128f;

        public static Vector3 GetPosition(InstanceID instance)
        {
            Vector3 position = Vector3.zero;

            switch (instance.Type)
            {
                case InstanceType.Building:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[instance.Building];
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

        public static ushort FindStartSegment(InstanceID instance)
        {
            Vector3 position = GetPosition(instance);

            if (position != Vector3.zero && PathManager.FindPathPosition(position,
                                                ItemClass.Service.Road,
                                                NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                                                VehicleInfo.VehicleType.Car,
                                                VehicleInfo.VehicleCategory.All,
                                                false,
                                                requireConnect: false,
                                                fMAX_SEARCH_DISTANCE,
                                                out PathUnit.Position pathStartPosA,
                                                out PathUnit.Position pathStartPosB,
                                                out float distanceSqrA,
                                                out float distanceSqrB))
            {
                return pathStartPosA.m_segment;
            }

            return 0;
        }

        public static void CalculatePathLength(ushort startBuildingId, ushort endBuildingId)
        {
            if (startBuildingId != 0 && endBuildingId != 0)
            {
                Building buildingStart = BuildingManager.instance.m_buildings.m_buffer[startBuildingId];
                Building buildingEnd = BuildingManager.instance.m_buildings.m_buffer[endBuildingId];
                if (buildingStart.m_flags != 0 && buildingEnd.m_flags != 0)
                {
                    CalculatePathLength(buildingStart.m_position, buildingEnd.m_position);
                }
            }
        }

        public static uint CreatePath(InstanceID start, InstanceID end)
        {
            return CreatePath(GetPosition(start), GetPosition(end));
        }

        public static uint CreatePath(Vector3 startPosition, Vector3 endPosition)
        {
            if (PathManager.FindPathPosition(startPosition, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.All, false, requireConnect: false, fMAX_SEARCH_DISTANCE, out PathUnit.Position pathStartPosA, out PathUnit.Position pathStartPosB, out float distanceSqrA, out float _) &&
                PathManager.FindPathPosition(endPosition, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.All, false, requireConnect: false, fMAX_SEARCH_DISTANCE, out PathUnit.Position pathEndPosA, out PathUnit.Position pathEndPosB, out float distanceSqrB, out float _))
            {
                uint firstPathUnit;
                if (PathManager.instance.CreatePath(out firstPathUnit,
                    ref Singleton<SimulationManager>.instance.m_randomizer,
                    Singleton<SimulationManager>.instance.m_currentBuildIndex,
                    pathStartPosA,
                    pathEndPosA,
                    NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                    VehicleInfo.VehicleType.Car,
                    VehicleInfo.VehicleCategory.All,
                    100000f))
                {
                    return firstPathUnit;
                }
            }

            return 0;
        }

        public static float GetPathLength(uint pathId, out float fSpeed)
        {
            float fLength = 0;
            fSpeed = 0;

            PathUnit path = PathManager.instance.m_pathUnits.m_buffer[pathId];
            if ((path.m_pathFindFlags & PathUnit.FLAG_READY) == PathUnit.FLAG_READY)
            {
                fLength += path.m_length;
                fSpeed = path.m_speed;

                uint pathUnit = path.m_nextPathUnit;
                while (pathUnit != 0)
                {
                    path = PathManager.instance.m_pathUnits.m_buffer[pathUnit];
                    fLength += path.m_length;
                    pathUnit = path.m_nextPathUnit;
                }
            }

            return fLength;
        }

        public static float CalculatePathLength(InstanceID start, InstanceID end)
        {
            return CalculatePathLength(GetPosition(start), GetPosition(end));
        }

        public static float CalculatePathLength(Vector3 startPosition, Vector3 endPosition)
        {
            float fTotalPathDistance = -1.0f;

            if (PathManager.FindPathPosition(startPosition, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.All, false, requireConnect: false, fMAX_SEARCH_DISTANCE, out PathUnit.Position pathStartPosA, out PathUnit.Position pathStartPosB, out float distanceSqrA, out float _) &&
                PathManager.FindPathPosition(endPosition, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.All, false, requireConnect: false, fMAX_SEARCH_DISTANCE, out PathUnit.Position pathEndPosA, out PathUnit.Position pathEndPosB, out float distanceSqrB, out float _))
            {
                uint firstPathUnit;
                if (PathManager.instance.CreatePath(out firstPathUnit, 
                                ref Singleton<SimulationManager>.instance.m_randomizer, 
                                Singleton<SimulationManager>.instance.m_currentBuildIndex, 
                                pathStartPosA, 
                                pathEndPosA, 
                                NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, 
                                VehicleInfo.VehicleType.Car,
                                VehicleInfo.VehicleCategory.All,
                                100000f))
                {
                    ref PathUnit path = ref PathManager.instance.m_pathUnits.m_buffer[firstPathUnit];

                    Stopwatch sw = Stopwatch.StartNew();
                    Debug.Log($"Flags {path.m_pathFindFlags} SimulationFlags {path.m_simulationFlags} m_positionCount: {path.m_positionCount} sw: {sw.ElapsedTicks}");

                    // How best to wait for path to be calculated?
                    long iStartTimer = sw.ElapsedMilliseconds;
                    while ((sw.ElapsedMilliseconds - iStartTimer) < 5 && path.m_positionCount == 20 && (path.m_pathFindFlags & PathUnit.FLAG_FAILED) != PathUnit.FLAG_FAILED)
                    {
                        // Spin till path calulated
                    }

                    Debug.Log($"Flags {path.m_pathFindFlags} SimulationFlags {path.m_simulationFlags} m_positionCount: {path.m_positionCount} sw: {sw.ElapsedTicks}");


                    string sPath = $"Path Created: {firstPathUnit}";
                    float fLength = 0;
                    float fSpeed = 0;

                    if (path.m_positionCount != 20 && (path.m_pathFindFlags & PathUnit.FLAG_FAILED) != PathUnit.FLAG_FAILED)
                    {
                        uint pathUnit = firstPathUnit;
                        while (pathUnit != 0)
                        {
                            path = PathManager.instance.m_pathUnits.m_buffer[pathUnit];
                            sPath += $"PathUnit:{pathUnit} Flags: {path.m_pathFindFlags} PositionCount: {path.m_positionCount} Length:{path.m_length} Speed:{path.m_speed} NextUnit: {path.m_nextPathUnit}\r\n";
                            fLength += path.m_length;
                            fSpeed = path.m_speed;

                            
                            for (int i = 0; i < path.m_positionCount; i++)
                            {
                                PathUnit.Position pos = path.GetPosition(i);
                                if (pos.m_segment != 0)
                                {
                                    sPath += $"Segment: {pos.m_segment} Lane: {pos.m_lane} Offset: {pos.m_offset}\r\n";
                                }
                            }
                            

                            pathUnit = path.m_nextPathUnit;
                        }
                    }

                    // Release path unit we dont need it any more
                    {
                        while (!Monitor.TryEnter(PathManager.instance.m_bufferLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                        {
                        }
                        PathManager.instance.ReleasePath(firstPathUnit);
                        Monitor.Exit(PathManager.instance.m_bufferLock);
                    }

                    fTotalPathDistance = fLength;
                    double dDistance = Math.Sqrt(Vector3.SqrMagnitude(startPosition - endPosition)) * 0.001;
                    sPath += $"Distance:{dDistance} PathLength: {fTotalPathDistance * 0.001} Speed:{fSpeed} StartSegmentA:{pathStartPosA.m_segment} StartSegmentB:{pathStartPosB.m_segment} EndSegmentA:{pathEndPosA.m_segment} EndSegmentB:{pathEndPosB.m_segment}\r\n";

                    Debug.Log(sPath);
                }
            }
            else
            {
                Debug.Log("Failed to find positions");
            }

            return fTotalPathDistance;
        }

        public static uint FindNearestNode(InstanceID instance)
        {
            ushort segmentId = FindStartSegment(instance);
            if (segmentId != 0)
            {
                NetSegment segment = NetManager.instance.m_segments.m_buffer[segmentId];
                if (segment.m_flags != 0)
                {
                    // Return closest node
                    return segment.m_startNode;
                }
            }
            return 0;
        }

        public static bool FindRoadAccess(
                                  Vector3 position,
                                  out ushort segmentID,
                                  bool mostCloser = false,
                                  bool untouchable = true)
        {
            Bounds bounds = new Bounds(position, new Vector3(40f, 40f, 40f));
            int num1 = Mathf.Max((int)(((double)bounds.min.x - 64.0) / 64.0 + 135.0), 0);
            int num2 = Mathf.Max((int)(((double)bounds.min.z - 64.0) / 64.0 + 135.0), 0);
            int num3 = Mathf.Min((int)(((double)bounds.max.x + 64.0) / 64.0 + 135.0), 269);
            int num4 = Mathf.Min((int)(((double)bounds.max.z + 64.0) / 64.0 + 135.0), 269);
            segmentID = (ushort)0;
            float num5 = float.MaxValue;
            NetManager instance = Singleton<NetManager>.instance;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    int num6 = 0;
                    for (ushort nextGridSegment = instance.m_segmentGrid[index1 * 270 + index2]; nextGridSegment != (ushort)0; nextGridSegment = instance.m_segments.m_buffer[(int)nextGridSegment].m_nextGridSegment)
                    {
                        if (num6++ >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                        NetInfo info = instance.m_segments.m_buffer[(int)nextGridSegment].Info;
                        if (info.m_class.m_service == ItemClass.Service.Road && !info.m_netAI.IsUnderground() && !info.m_netAI.IsOverground() && info.m_netAI is RoadBaseAI && (untouchable || (instance.m_segments.m_buffer[(int)nextGridSegment].m_flags & NetSegment.Flags.Untouchable) == NetSegment.Flags.None) && info.m_hasPedestrianLanes && (info.m_hasForwardVehicleLanes || info.m_hasBackwardVehicleLanes))
                        {
                            ushort startNode = instance.m_segments.m_buffer[(int)nextGridSegment].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[(int)nextGridSegment].m_endNode;
                            Vector3 position1 = instance.m_nodes.m_buffer[(int)startNode].m_position;
                            Vector3 position2 = instance.m_nodes.m_buffer[(int)endNode].m_position;
                            float num7 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position1.x, bounds.min.z - 64f - position1.z), Mathf.Max((float)((double)position1.x - (double)bounds.max.x - 64.0), (float)((double)position1.z - (double)bounds.max.z - 64.0)));
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max((float)((double)position2.x - (double)bounds.max.x - 64.0), (float)((double)position2.z - (double)bounds.max.z - 64.0)));
                            Vector3 positionA;
                            if (((double)num7 < 0.0 || (double)num8 < 0.0) && instance.m_segments.m_buffer[(int)nextGridSegment].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[(int)nextGridSegment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.RoadTransport, VehicleInfo.VehicleType.None, false, out positionA, out int _, out float _, out Vector3 _, out int _, out float _))
                            {
                                float num9 = Vector3.SqrMagnitude(position - positionA);
                                if ((double)num9 < 400.0 && (double)num9 < (double)num5)
                                {
                                    segmentID = nextGridSegment;
                                    if (!mostCloser)
                                        return true;
                                    num5 = num9;
                                }
                            }
                        }
                    }
                }
            }
            if (segmentID != (ushort)0)
                return true;

            return false;
        }
    }
}
