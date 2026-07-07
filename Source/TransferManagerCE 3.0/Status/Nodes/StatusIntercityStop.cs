using System;
using System.Collections.Generic;
using ColossalFramework;
using SleepyCommon;
using UnityEngine;
using static NetInfo;
using static ParadeGroupInfo;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

// ----------------------------------------------------------------------------------------
namespace TransferManagerCore.Data
{
    // ----------------------------------------------------------------------------------------
    public class StatusIntercityStop : StatusSegmentStop
    {
        // ----------------------------------------------------------------------------------------
        public StatusIntercityStop(BuildingType eBuildingType, ushort buildingId, ushort segmentId, ushort startNodeId, ushort endNodeId) :
            base(eBuildingType, buildingId, segmentId, startNodeId, endNodeId)
        {
        }

        // --------------------------------------------------------------------
        public override string GetMaterialDisplay()
        {
            return "Intercity Stop";
        }

        // --------------------------------------------------------------------
        public override string GetMaterialDescription()
        {
            return "Intercity Stop";
        }

        // --------------------------------------------------------------------
        public override bool CanDelete()
        {
            return true;
        }

        // --------------------------------------------------------------------
        public override string GetDeleteTooltip()
        {
            return $"{Localization.Get("btnDeleteIntercityLine")} [#{m_startNodeId} - #{m_endNodeId}]";
        }

        // --------------------------------------------------------------------
        public override void OnClickDelete()
        {
            // Remove line
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                RemoveConnectionLine(m_buildingId, m_startNodeId, m_endNodeId);
            });
        }

        // --------------------------------------------------------------------
        protected override TransportInfo.TransportType GetTransportType()
        {
            TransportInfo.TransportType eTransportType;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (building.Info.GetSubService())
            {
                case ItemClass.SubService.PublicTransportBus:
                    {
                        eTransportType = TransportInfo.TransportType.Bus;
                        break;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    {
                        eTransportType = TransportInfo.TransportType.Ship;
                        break;
                    }
                case ItemClass.SubService.PublicTransportPlane:
                    {
                        eTransportType = TransportInfo.TransportType.Airplane;
                        break;
                    }
                case ItemClass.SubService.PublicTransportTrain:
                    {
                        eTransportType = TransportInfo.TransportType.Train;
                        break;
                    }
                default:
                    {
                        eTransportType = TransportInfo.TransportType.Train;
                        break;
                    }
            }

            return eTransportType;
        }

        // --------------------------------------------------------------------
        // This will remove the IN and OUT intercity line at the same time
        // Based on TransportStationAI.RemoveConnectionLines
        private static void RemoveConnectionLine(ushort buildingId, ushort startNodeId, ushort endNodeId)
        {
            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<ushort> vehiclesToRemove = new HashSet<ushort>();

            // Remove any vehicles on these lines
            BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicleData) =>
            {
                InstanceID target = vehicleData.Info.m_vehicleAI.GetTargetID(vehicleId, ref vehicleData);
                if (target.NetNode != 0 &&
                    (target.NetNode == startNodeId || target.NetNode == endNodeId))
                {
                    vehiclesToRemove.Add(vehicleId);

                }

                return true;
            });
            foreach (ushort vehicleId in vehiclesToRemove)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
            }
            

            // Remove line from building list
            NetManager instance = Singleton<NetManager>.instance;
            ushort num = 0;
            ushort num2 = building.m_netNode;
            int iLoopCount = 0;

            while (num2 != 0)
            {
                NetInfo info = instance.m_nodes.m_buffer[num2].Info;
                ushort nextBuildingNode = instance.m_nodes.m_buffer[num2].m_nextBuildingNode;

                if (num2 == startNodeId || num2 == endNodeId)
                {
                    // Remove node from building
                    if (num != 0)
                    {
                        instance.m_nodes.m_buffer[num].m_nextBuildingNode = nextBuildingNode;
                    }
                    else
                    {
                        building.m_netNode = nextBuildingNode;
                    }

                    ReleaseLines(num2);
                    instance.ReleaseNode(num2);
                    num2 = num;
                }

                // Update for next node
                num = num2;
                num2 = nextBuildingNode;

                if (++iLoopCount > 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        // --------------------------------------------------------------------
        private static void ReleaseLines(ushort node)
        {
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = instance.m_nodes.m_buffer[node].GetSegment(i);
                if (segment != 0)
                {
                    instance.ReleaseSegment(segment, keepNodes: true);
                }
            }
        }
    }
}