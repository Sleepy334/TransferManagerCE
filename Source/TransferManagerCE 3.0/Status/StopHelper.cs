using System;
using ColossalFramework;
using UnityEngine;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCore.Data;
using static TransferManagerCore.BuildingTypeHelper;
using static TransferManagerCore.StopHelper;
using UnityEngine.Networking.Types;

namespace TransferManagerCore
{
    public class StopHelper
    {
        public enum StopType
        {
            None,
            Intercity,
            TransportLine,
            CableCar,
            Evacuation,
        };

        // ----------------------------------------------------------------------------------------
        private List<StatusData> m_IntercityStopsIn = new List<StatusData>();
        private List<StatusData> m_IntercityStopsOut = new List<StatusData>();
        private List<StatusData> m_CableCarStops = new List<StatusData>();
        private List<StatusData> m_EvacuationStops = new List<StatusData>();
        private List<StatusData> m_LineStops = new List<StatusData>();
        private List<StatusDataVehicle> m_nodeVehicles = new List<StatusDataVehicle>();
        private HashSet<ushort> m_addedVehicles = new HashSet<ushort>();

        private float m_fBuildingSize = 0f;
        private BuildingType m_eBuildingType = BuildingType.None;

        // ----------------------------------------------------------------------------------------
        public StopHelper()
        {
        }

        // ----------------------------------------------------------------------------------------
        public List<StatusData> GetStatusList(ushort buildingId, out int iVehicleCount)
        {
            List<StatusData> list = new List<StatusData>();

            m_IntercityStopsIn.Clear();
            m_IntercityStopsOut.Clear();
            m_CableCarStops.Clear();
            m_EvacuationStops.Clear();
            m_LineStops.Clear();
            m_nodeVehicles.Clear();
            m_addedVehicles.Clear();

            m_eBuildingType = BuildingType.None;
            m_fBuildingSize = 0.0f;
            iVehicleCount = 0;

            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    m_eBuildingType = GetBuildingType(building);

                    // Store the parents building size
                    m_fBuildingSize = Mathf.Max(building.Length, building.Width);

                    // Add building specific values
                    AddBuildingSpecific(false, m_eBuildingType, buildingId, building);

                    // Add sub building values as well
                    int iLoopCount = 0;
                    ushort subBuildingId = building.m_subBuilding;
                    while (subBuildingId != 0)
                    {
                        Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                        if (subBuilding.m_flags != 0)
                        {
                            BuildingType eSubBuildingType = GetBuildingType(subBuilding);
                            AddBuildingSpecific(true, eSubBuildingType, subBuildingId, subBuilding);
                        }

                        // setup for next sub building
                        subBuildingId = subBuilding.m_subBuilding;

                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }

                    m_nodeVehicles.Sort();

                    // Segment based stops
                    ProcessNetStops("Intercity Stops (In)", m_IntercityStopsIn, ref list, ref iVehicleCount);
                    ProcessNetStops("Intercity Stops (Out)", m_IntercityStopsOut, ref list, ref iVehicleCount);
                    ProcessNetStops("Cable Car Stops", m_CableCarStops, ref list, ref iVehicleCount);
                    ProcessNetStops("Evacuation Stops", m_EvacuationStops, ref list, ref iVehicleCount);

                    // Normal stops
                    if (m_LineStops.Count > 0)
                    {
                        AddHeading("Line Stops", list);
                        m_LineStops.Sort();

                        foreach (StatusData data in m_LineStops)
                        {
                            list.Add(data);

                            if (data is StatusNodeStop stop)
                            {
                                foreach (StatusDataVehicleNode vehicle in m_nodeVehicles)
                                {
                                    if (!m_addedVehicles.Contains(vehicle.GetVehicleId()))
                                    {
                                        InstanceID target = vehicle.GetTarget();
                                        if (target.NetNode != 0 && target.NetNode == stop.m_nodeId)
                                        {
                                            list.Add(vehicle);
                                            m_addedVehicles.Add(vehicle.GetVehicleId());
                                            iVehicleCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        // ----------------------------------------------------------------------------------------
        private void ProcessNetStops(string heading, List<StatusData> stops, ref List<StatusData> resultList, ref int iVehicleCount)
        {
            // Cable Car stops
            if (stops.Count > 0)
            {
                AddHeading(heading, resultList);
                stops.Sort();

                foreach (StatusData data in stops)
                {
                    resultList.Add(data);

                    // Add vehicles heading to this stop
                    if (data is StatusSegmentStop stop)
                    {
                        Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                        foreach (StatusDataVehicleNode vehicle in m_nodeVehicles)
                        {
                            if (!m_addedVehicles.Contains(vehicle.GetVehicleId()))
                            {
                                InstanceID target = vehicle.GetTarget();
                                if (target.NetNode != 0 && target.NetNode == stop.m_endNodeId)
                                {
                                    resultList.Add(vehicle);
                                    m_addedVehicles.Add(vehicle.GetVehicleId());
                                    iVehicleCount++;
                                }
                            }
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void AddToList(List<StatusData> list, StatusData data)
        {
            list.Add(data);
        }

        // ----------------------------------------------------------------------------------------
        private void AddHeading(string sHeader, List<StatusData> list)
        {
            if (list.Count > 0)
            {
                list.Add(new StatusDataSeparator());
            }

            if (!string.IsNullOrEmpty(sHeader))
            {
                list.Add(new StatusDataHeader(sHeader));
            }
        }

        // ----------------------------------------------------------------------------------------
        private void AddBuildingSpecific(bool bSubBuilding, BuildingTypeHelper.BuildingType eBuildingType, ushort buildingId, Building building)
        {
            // Building specific
            switch (eBuildingType)
            {
                case BuildingType.DisasterShelter:
                    {
                        AddNetStops(eBuildingType, building, buildingId);
                        break;
                    }
                case BuildingType.CableCarStation:
                    {
                        AddNetStops(eBuildingType, building, buildingId);
                        break;
                    }
                case BuildingType.TransportStation:
                    {
                        // Add stops
                        AddLineStops(eBuildingType, building, buildingId);

                        // Add intercity stops
                        AddNetStops(eBuildingType, building, buildingId);

                        break;
                    }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void AddLineStops(BuildingType eBuildingType, Building building, ushort buildingId)
        {
            NetNode[] Nodes = NetManager.instance.m_nodes.m_buffer;
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            // We use the parents building size always, squared for distance measure
            float fMaxDistanceSquared = Mathf.Max(64f, m_fBuildingSize * m_fBuildingSize);

            // Add line stops
            uint iSize = TransportManager.instance.m_lines.m_size;
            for (int i = 0; i < iSize; i++)
            {
                TransportLine line = TransportManager.instance.m_lines.m_buffer[i];
                if (line.m_flags != 0 && line.Complete)
                {
                    // Enumerate stops
                    int iLoopCount = 0;
                    ushort firstStop = line.m_stops;
                    ushort stop = firstStop;
                    while (stop != 0)
                    {
                        NetNode node = Nodes[stop];
                        if (node.m_flags != 0)
                        {
                            // Scale allowed distance by size of building, we use FindTransportBuilding so that if there is a nearby transport station then we
                            // are less likely to think they are our stops.
                            ushort transportBuildingId = BuildingManager.instance.FindTransportBuilding(node.m_position, fMaxDistanceSquared, line.Info.m_transportType);
                            if (transportBuildingId == buildingId)
                            {
                                // Add stop to list
                                AddToList(m_LineStops, new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop));

                                int iAdded = 0;
                                ushort vehicleId = line.m_vehicles;
                                int iVehicleLoopCount = 0;
                                while (vehicleId != 0)
                                {
                                    Vehicle vehicle = Vehicles[vehicleId];
                                    if (vehicle.m_flags != 0 && vehicle.m_targetBuilding == stop)
                                    {
                                        m_nodeVehicles.Add(new StatusDataVehicleLineStop(StopType.TransportLine, eBuildingType, buildingId, vehicleId, node.m_transportLine, vehicle.m_sourceBuilding, new InstanceID { NetNode = stop }));
                                        iAdded++;
                                    }

                                    vehicleId = vehicle.m_nextLineVehicle;

                                    if (++iVehicleLoopCount >= 32768)
                                    {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }

                        stop = TransportLine.GetNextStop(stop);
                        if (stop == firstStop)
                        {
                            break;
                        }

                        if (++iLoopCount >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void AddNetStops(BuildingType eBuildingType, Building building, ushort buildingId)
        {
            NetNode[] Nodes = NetManager.instance.m_nodes.m_buffer;
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            HashSet<ushort> addedNodes = new HashSet<ushort>();
            HashSet<ushort> addedSegmentIds = new HashSet<ushort>();

            // Add net/intercity stops
            int iLoopCount2 = 0;
            ushort nodeId = building.m_netNode;
            while (nodeId != 0)
            {
                NetNode node = Nodes[nodeId];

                if (!addedNodes.Contains(nodeId))
                {
                    NetInfo info = node.Info;
                    if ((object)info != null)
                    {
                        StopType eStopType = GetStopType(eBuildingType, nodeId, node);
                        switch (eStopType)
                        {
                            case StopType.Intercity:
                            case StopType.CableCar:
                                {
                                    CreateSegmentLines(eStopType, eBuildingType, buildingId, nodeId, addedSegmentIds);
                                    break;
                                }
                            case StopType.TransportLine:
                            case StopType.Evacuation:
                                {
                                    AddToList(m_LineStops, CreateStatusDataLine(eStopType, eBuildingType, buildingId, node.m_transportLine, nodeId));
                                    break;
                                }
                            case StopType.None:
                                {
                                    break;
                                }
                            default:
                                {
                                    Log.Error($"ERROR: StopType: {eStopType} NetNode {nodeId} Node SubService: {node.Info.GetSubService()} Line: {node.m_transportLine} not handled.");
                                    break;
                                }
                        }
                    }
                }

                nodeId = node.m_nextBuildingNode;

                if (++iLoopCount2 > 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            // Find any vehicles heading to the stops and add them
            uint uiSize = VehicleManager.instance.m_vehicles.m_size;
            ushort vehicleID = building.m_ownVehicles;
            int iLoopCount1 = 0;
            while (vehicleID != 0 && vehicleID < uiSize)
            {
                Vehicle vehicle = Vehicles[vehicleID];
                if (vehicle.m_flags != 0)
                {
                    InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleID, vehicle);
                    if (target.NetNode != 0)
                    {
                        NetNode node = Nodes[target.NetNode];
                        if (node.m_flags != 0 && node.Info is not null)
                        {
                            StopType eStopType = GetStopType(eBuildingType, nodeId, node);
                            if (eStopType != StopType.None)
                            {
                                if (node.m_transportLine != 0)
                                {
                                    m_nodeVehicles.Add(new StatusDataVehicleLineStop(eStopType, eBuildingType, buildingId, vehicleID, node.m_transportLine, vehicle.m_sourceBuilding, target));
                                }
                                else
                                {
                                    m_nodeVehicles.Add(new StatusDataVehicleNode(eStopType, eBuildingType, buildingId, vehicleID, vehicle.m_sourceBuilding, target));
                                }
                            }
                        }
                    }
                }

                vehicleID = vehicle.m_nextOwnVehicle;

                if (++iLoopCount1 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private StopType GetStopType(BuildingType eBuildingType, ushort nodeId, NetNode node)
        {
            if (node.Info.m_class.m_layer == ItemClass.Layer.PublicTransport)
            {
                if (eBuildingType == BuildingType.DisasterShelter)
                {
                    return StopType.Evacuation;
                }
                else
                {
                    switch (node.Info.GetSubService())
                    {
                        case ItemClass.SubService.PublicTransportBus:
                        case ItemClass.SubService.PublicTransportTrain:
                        case ItemClass.SubService.PublicTransportPlane:
                        case ItemClass.SubService.PublicTransportShip:
                            {
                                if (node.m_transportLine == 0)
                                {
                                    return StopType.Intercity;
                                }
                                else
                                {
                                    return StopType.TransportLine;
                                }
                            }
                        case ItemClass.SubService.PublicTransportMetro:
                        case ItemClass.SubService.PublicTransportMonorail:
                        case ItemClass.SubService.PublicTransportTram:
                        case ItemClass.SubService.PublicTransportTrolleybus:
                            {
                                return StopType.TransportLine;
                            }
                        case ItemClass.SubService.PublicTransportCableCar:
                            {
                                return StopType.CableCar;
                            }
                        default:
                            {
                                Log.Error($"ERROR: NetNode {nodeId} Service: {node.Info.GetService()} SubService: {node.Info.GetSubService()} Line: {node.m_transportLine} not handled.");
                                break;
                            }
                    }
                }
            }

            return StopType.None;
        }

        // ----------------------------------------------------------------------------------------
        private void CreateSegmentLines(StopType stopType, BuildingType eBuildingType, ushort buildingId, ushort nodeId, HashSet<ushort> addedSegmentIds)
        {
            NetManager instance = Singleton<NetManager>.instance;

            NetNode node = NetManager.instance.m_nodes.m_buffer[nodeId];
            if (node.m_flags != 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    ushort segmentId = node.GetSegment(i);
                    if (segmentId != 0 && !addedSegmentIds.Contains(segmentId))
                    {
                        NetSegment segment = instance.m_segments.m_buffer[segmentId];

                        // Add segment data
                        switch (stopType)
                        {
                            case StopType.Intercity:
                                {
                                    StatusIntercityStop stop = new StatusIntercityStop(eBuildingType, buildingId, segmentId, segment.m_startNode, segment.m_endNode);
                                    if (BuildingTypeHelper.IsOutsideConnection(stop.m_startBuildingId))
                                    {
                                        AddToList(m_IntercityStopsIn, stop);
                                    }
                                    else
                                    {
                                        AddToList(m_IntercityStopsOut, stop);
                                    }
                                    addedSegmentIds.Add(segmentId);
                                    break;
                                }
                            case StopType.CableCar:                                ;
                                {
                                    AddToList(m_CableCarStops, new StatusCableCarStop(eBuildingType, buildingId, segmentId, segment.m_startNode, segment.m_endNode));
                                    addedSegmentIds.Add(segmentId);
                                    break;
                                }
                            default:
                                {
                                    Log.Error($"StopType: {stopType} not handled");
                                    break;
                                }
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private StatusData? CreateStatusDataLine(StopType stopType, BuildingType eBuildingType, ushort buildingId, ushort LineId, ushort nodeId)
        {
            switch (stopType)
            {
                case StopType.TransportLine: return new StatusTransportLineStop(eBuildingType, buildingId, LineId, nodeId);
                case StopType.Evacuation: return new StatusEvacuationStop(eBuildingType, buildingId, LineId, nodeId);
            }

            return null;
        }
    }
}