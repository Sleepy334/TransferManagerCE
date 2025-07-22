using System;
using ColossalFramework;
using UnityEngine;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE
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

        private List<StatusData> m_listIntercityStops = new List<StatusData>();
        private List<StatusData> m_listLineStops = new List<StatusData>();
        private HashSet<ushort> m_setAddedVehicles = new HashSet<ushort>();
        private float m_fBuildingSize = 0f;
        private BuildingType m_eBuildingType = BuildingType.None;

        // ----------------------------------------------------------------------------------------
        public StopHelper()
        {
        } 

        public List<StatusData> GetStatusList(ushort buildingId, out int iVehicleCount)
        {
            List<StatusData> list = new List<StatusData>();

            m_listIntercityStops.Clear();
            m_listLineStops.Clear();
            m_setAddedVehicles.Clear();
            m_eBuildingType = BuildingType.None;
            m_fBuildingSize = 0.0f;

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
                }

                SortAndMergeList("Intercity Stops", list, m_listIntercityStops);
                SortAndMergeList("Line Stops", list, m_listLineStops);
            }

            iVehicleCount = m_setAddedVehicles.Count;
            return list;
        }

        private void AddToList(List<StatusData> list, StatusData data)
        {
            if (data.HasVehicle())
            {
                if (!m_setAddedVehicles.Contains(data.GetVehicleId()))
                {
                    // Only add vehicle if not already in list
                    m_setAddedVehicles.Add(data.GetVehicleId());
                    list.Add(data);
                }
            }
            else
            {
                list.Add(data);
            }
        }

        private void SortAndMergeList(string sHeader, List<StatusData> list, List<StatusData> listToAdd, bool bSort = true)
        {
            if (listToAdd.Count > 0)
            {
                if (list.Count > 0)
                {
                    list.Add(new StatusDataSeparator());
                }
                if (bSort)
                {
                    listToAdd.Sort();
                }

                if (!string.IsNullOrEmpty(sHeader))
                {
                    list.Add(new StatusDataHeader(sHeader));
                }
                
                list.AddRange(listToAdd);
            }
        }

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
                                int iAdded = 0;
                                ushort vehicleId = line.m_vehicles;
                                int iVehicleLoopCount = 0;
                                while (vehicleId != 0)
                                {
                                    Vehicle vehicle = Vehicles[vehicleId];
                                    if (vehicle.m_flags != 0 && vehicle.m_targetBuilding == stop)
                                    {
                                        AddToList(m_listLineStops, new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop, vehicleId));
                                        iAdded++;
                                    }

                                    vehicleId = vehicle.m_nextLineVehicle;

                                    if (++iVehicleLoopCount >= 32768)
                                    {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }

                                // If there arent any vehicles for this stop then add a "None" one instead.
                                if (iAdded == 0)
                                {
                                    AddToList(m_listLineStops, new StatusTransportLineStop(eBuildingType, buildingId, node.m_transportLine, stop, 0));
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

        private void AddNetStops(BuildingType eBuildingType, Building building, ushort buildingId)
        {
            NetNode[] Nodes = NetManager.instance.m_nodes.m_buffer;
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            HashSet<ushort> addedNodes = new HashSet<ushort>();
            HashSet<ushort> addedSegmentIds = new HashSet<ushort>();

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
                        NetInfo info = node.Info;

                        if ((object)info != null)
                        {
                            StopType eStopType = GetStopType(eBuildingType, info.m_class.m_layer, node.m_transportLine);
                            if (eStopType != StopType.None)
                            {
                                switch (eStopType)
                                {
                                    case StopType.Intercity:
                                        {
                                            CreateIntercityLines(eBuildingType, buildingId, target.NetNode, vehicleID, addedSegmentIds);
                                            break;
                                        }
                                    default:
                                        {
                                            StatusData? data = CreateStatusData(eStopType, eBuildingType, buildingId, node.m_transportLine, target.NetNode, vehicleID);
                                            if (data != null)
                                            {
                                                if (eStopType == StopType.CableCar || node.m_transportLine != 0)
                                                {
                                                    AddToList(m_listLineStops, data);
                                                }
                                                else
                                                {
                                                    AddToList(m_listIntercityStops, data);
                                                }
                                                addedNodes.Add(target.NetNode);
                                            }
                                            break;
                                        }
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

            // Add net/intercity stops that dont have vehicles on the way
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
                        StopType eStopType = GetStopType(eBuildingType, info.m_class.m_layer, node.m_transportLine);
                        switch (eStopType)
                        {
                            case StopType.Intercity:
                                {
                                    CreateIntercityLines(eBuildingType, buildingId, nodeId, 0, addedSegmentIds);
                                    break;
                                }
                            default:
                                {
                                    StatusData? data = CreateStatusData(eStopType, eBuildingType, buildingId, node.m_transportLine, nodeId, 0);
                                    if (data != null)
                                    {
                                        if (eStopType == StopType.CableCar || node.m_transportLine != 0)
                                        {
                                            AddToList(m_listLineStops, data);
                                        }
                                        else
                                        {
                                            AddToList(m_listIntercityStops, data);
                                        }
                                    }
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
        }

        private StopType GetStopType(BuildingType eBuildingType, ItemClass.Layer layer, ushort transportLineId)
        {
            if (layer == ItemClass.Layer.PublicTransport)
            {
                switch (eBuildingType)
                {
                    case BuildingType.TransportStation:
                        {
                            if (transportLineId == 0)
                            {
                                return StopType.Intercity;
                            }
                            else
                            {
                                return StopType.TransportLine;
                            }
                        }
                    case BuildingType.CableCarStation:
                        {
                            return StopType.CableCar;
                        }
                    case BuildingType.DisasterShelter:
                        {
                            return StopType.Evacuation;
                        }
                }
            }

            return StopType.None;
        }

        private void CreateIntercityLines(BuildingType eBuildingType, ushort buildingId, ushort nodeId, ushort vehicleId, HashSet<ushort> addedSegmentIds)
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

                        // Add line
                        StatusData data;
                        if (vehicleId != 0 && nodeId == segment.m_endNode)
                        {
                            data = new StatusIntercityStop(eBuildingType, buildingId, segmentId, segment.m_startNode, segment.m_endNode, vehicleId);
                        }
                        else
                        {
                            data = new StatusIntercityStop(eBuildingType, buildingId, segmentId, segment.m_startNode, segment.m_endNode, 0);
                        }

                        AddToList(m_listIntercityStops, data);
                        addedSegmentIds.Add(segmentId);
                    }
                }
            }
        }

        private StatusData? CreateStatusData(StopType stopType, BuildingType eBuildingType, ushort buildingId, ushort LineId, ushort nodeId, ushort vehicleId)
        {
            switch (stopType)
            {
                case StopType.TransportLine: return new StatusTransportLineStop(eBuildingType, buildingId, LineId, nodeId, vehicleId);
                case StopType.CableCar: return new StatusCableCarStop(eBuildingType, buildingId, nodeId, vehicleId);
                case StopType.Evacuation: return new StatusEvacuationStop(eBuildingType, buildingId, nodeId, vehicleId);
            }

            return null;
        }
    }
}