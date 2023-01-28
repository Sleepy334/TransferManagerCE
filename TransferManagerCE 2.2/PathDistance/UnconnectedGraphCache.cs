using ColossalFramework;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class UnconnectedGraphCache
    {
        private static UnconnectedGraph? s_goodsGraph = null;
        static readonly object s_goodsLock = new object();

        private static UnconnectedGraph? s_pedestrianZoneServicesGraph = null;
        static readonly object s_pedestrianZoneServicesLock = new object();

        private static UnconnectedGraph? s_otherServicesGraph = null;
        static readonly object s_otherServicesLock = new object();

        public static bool IsConnected(TransferReason material, ushort node1, ushort node2)
        {
            if (PathDistanceTypes.IsGoodsMaterial(material))
            {
                lock (s_goodsLock)
                {
                    return GetGoodsGraph().IsConnected(node1, node2);
                }
            }
            else if (PathDistanceTypes.IsPedestrianZoneService(material))
            {
                lock (s_pedestrianZoneServicesLock)
                {
                    return GetPedestrianZoneServicesGraph().IsConnected(node1, node2);
                }
            }
            else
            {
                lock (s_otherServicesLock)
                {
                    return GetOtherServicesGraph().IsConnected(node1, node2);
                }
            }
        }

        private static UnconnectedGraph GetGoodsGraph()
        {
            lock (s_goodsLock)
            {
                if (s_goodsGraph == null || !s_goodsGraph.IsValid())
                {
                    s_goodsGraph = new UnconnectedGraph();
                    
                    // Set up lane requirements
                    PathDistanceTypes.GetService(TransferReason.Goods, out s_goodsGraph.m_service1, out s_goodsGraph.m_service2, out s_goodsGraph.m_service3);
                    s_goodsGraph.m_laneTypes = PathDistanceTypes.GetLaneTypes(TransferReason.Goods);
                    s_goodsGraph.m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(TransferReason.Goods);
                    s_goodsGraph.m_bPedestrianZone = false;

                    s_goodsGraph.FloodFill();
                }

                return s_goodsGraph;
            }
        }

        private static UnconnectedGraph GetPedestrianZoneServicesGraph()
        {
            lock (s_pedestrianZoneServicesLock)
            {
                if (s_pedestrianZoneServicesGraph == null || !s_pedestrianZoneServicesGraph.IsValid())
                {
                    s_pedestrianZoneServicesGraph = new UnconnectedGraph();

                    // Set up lane requirements
                    PathDistanceTypes.GetService(TransferReason.Dead, out s_pedestrianZoneServicesGraph.m_service1, out s_pedestrianZoneServicesGraph.m_service2, out s_pedestrianZoneServicesGraph.m_service3);
                    s_pedestrianZoneServicesGraph.m_laneTypes = PathDistanceTypes.GetLaneTypes(TransferReason.Dead);
                    s_pedestrianZoneServicesGraph.m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(TransferReason.Dead);
                    s_pedestrianZoneServicesGraph.m_bPedestrianZone = true;

                    s_pedestrianZoneServicesGraph.FloodFill();
                }

                return s_pedestrianZoneServicesGraph;
            }
        }

        private static UnconnectedGraph GetOtherServicesGraph()
        {
            lock (s_otherServicesLock)
            {
                if (s_otherServicesGraph == null || !s_otherServicesGraph.IsValid())
                {
                    s_otherServicesGraph = new UnconnectedGraph();

                    // Set up lane requirements
                    PathDistanceTypes.GetService(TransferReason.Garbage, out s_otherServicesGraph.m_service1, out s_otherServicesGraph.m_service2, out s_otherServicesGraph.m_service3);
                    s_otherServicesGraph.m_laneTypes = PathDistanceTypes.GetLaneTypes(TransferReason.Garbage);
                    s_otherServicesGraph.m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(TransferReason.Garbage);
                    s_otherServicesGraph.m_bPedestrianZone = false;

                    s_otherServicesGraph.FloodFill();
                }

                return s_otherServicesGraph;
            }
        }

        public static ConnectedStorage GetGoodsBufferCopy()
        {
            lock (s_goodsLock)
            {
                return new ConnectedStorage(GetGoodsGraph().GetBuffer());
            }
        }

        public static ConnectedStorage GetPedestrianZoneServicesBufferCopy()
        {
            lock (s_pedestrianZoneServicesLock)
            {
                return new ConnectedStorage(GetPedestrianZoneServicesGraph().GetBuffer());
            }
        }

        public static ConnectedStorage GetOtherServicesBufferCopy()
        {
            lock (s_otherServicesLock)
            {
                return new ConnectedStorage(GetOtherServicesGraph().GetBuffer());
            }
        }
    }
}