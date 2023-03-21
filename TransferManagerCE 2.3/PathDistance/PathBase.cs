using ColossalFramework;
using System;
using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    public abstract class PathBase
    {
        // Lane requirements for this path calculation
        private ItemClass.Service m_service1;
        private ItemClass.Service m_service2;
        private ItemClass.Service m_service3;
        protected NetInfo.LaneType m_laneTypes;
        private VehicleInfo.VehicleType m_vehicleTypes;
        private bool m_bPedestrianZone;
        private bool m_bCargoPathAllowed;

        // Game instances
        protected readonly NetSegment[] NetSegments;
        protected readonly NetNode[] NetNodes;
        protected readonly NetLane[] NetLanes;

        public PathBase()
        {
            // Store local references to buffers for faster access
            NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            NetSegments = Singleton<NetManager>.instance.m_segments.m_buffer;
            NetLanes = Singleton<NetManager>.instance.m_lanes.m_buffer;
        }

        public void SetMaterial(CustomTransferReason material)
        {
            bool bGoodsMaterial = PathDistanceTypes.IsGoodsMaterial(material);
            PathDistanceTypes.GetService(bGoodsMaterial, out m_service1, out m_service2, out m_service3);
            m_laneTypes = PathDistanceTypes.GetLaneTypes(bGoodsMaterial);
            m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(bGoodsMaterial);
            m_bPedestrianZone = PathDistanceTypes.IsPedestrianZoneService(material);
            m_bCargoPathAllowed = PathDistanceTypes.IsGoodsMaterial(material);
        }

        protected abstract void ProcessSegment(ushort segmentId, ushort uiCurrentNodeId, float fCurrentTravelTime);

        protected abstract void UpdateNode(ushort uiCurrentNodeId, float fCurrentTravelTime);

        protected void ProcessNode(ushort nodeId, NetNode node, float fCurrentTravelTime)
        {
            if (node.m_flags != 0)
            {
                // Loop through segments to find neighboring roads
                for (int i = 0; i < 8; ++i)
                {
                    ushort segmentId = node.GetSegment(i);
                    if (segmentId != 0)
                    {
                        ProcessSegment(segmentId, nodeId, fCurrentTravelTime);
                    }
                }

                // Loop through lanes to if see there are any extra connections segments
                ProcessLaneSegments(node.m_lane, nodeId, fCurrentTravelTime);
            }
        }

        protected void ProcessLaneNodes(uint laneId, float fCurrentTravelTime)
        {
            // Loop through all sub nodes for this lane
            int iLaneLoopCount = 0;
            while (laneId != 0)
            {
                NetLane lane = NetLanes[laneId];
                if (lane.m_flags != 0)
                {
                    int iNodeLoopCount = 0;
                    ushort nodeId = lane.m_nodes;
                    while (nodeId != 0)
                    {
                        NetNode node = NetNodes[nodeId];
                        if (node.m_flags != 0)
                        {
                            UpdateNode(nodeId, fCurrentTravelTime);
                        }

                        nodeId = node.m_nextLaneNode;

                        // Safety check in case we get caught in an infinite loop somehow
                        if (iNodeLoopCount++ > NetManager.MAX_NODE_COUNT)
                        {
                            Debug.Log("Invalid node loop detected");
                            break;
                        }
                    }
                }

                // Update laneId
                laneId = lane.m_nextLane;

                // Safety check in case we get caught in an infinite loop somehow
                if (iLaneLoopCount++ > NetManager.MAX_LANE_COUNT)
                {
                    Debug.Log("Invalid lane loop detected");
                    break;
                }
            }
        }

        private void ProcessLaneSegments(uint laneId, ushort nodeId, float fCurrentTravelTime)
        {
            // Loop through lanes to if see there are any extra connections segments
            int iLaneCount = 0;
            while (laneId != 0)
            {
                NetLane lane = NetLanes[laneId];
                if (lane.m_flags != 0 && lane.m_segment != 0)
                {
                    ProcessSegment(lane.m_segment, nodeId, fCurrentTravelTime);
                }

                laneId = lane.m_nextLane;

                if (++iLaneCount >= 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        protected bool IsNetInfoValid(NetInfo info)
        {
            return
                (info.m_laneTypes & m_laneTypes) != 0 &&
                IsServiceValid(info) &&
                IsNetInfoVehicleTypesValid(info) &&
                (m_bPedestrianZone || !info.IsPedestrianZoneRoad());
        }

        protected bool IsServiceValid(NetInfo info)
        {
            ItemClass.Service service = info.GetService();
            if (service == m_service1 || service == m_service2 || service == m_service3)
            {
                // Cargo stations seem to label their connector nodes as Beautification for some reason so we need to check AI as well
                // as we dont want to allow Quays and 
                if (m_bCargoPathAllowed && service == ItemClass.Service.Beautification)
                {
                    return info.GetAI() is CargoPathAI;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNetInfoVehicleTypesValid(NetInfo info)
        {
            if (m_bCargoPathAllowed && info.GetAI() is CargoPathAI)
            {
                // Cargo stations seem to label their connector nodes as Beautification for some reason
                // and annoyingly their vehicle types are set to None so we need to handle this separately.
                return true;
            }
            else
            {
                return (info.m_vehicleTypes & m_vehicleTypes) != 0;
            }
        }
    }
}
