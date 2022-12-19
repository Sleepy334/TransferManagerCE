using ColossalFramework;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class UnconnectedGraphCache
    {
        private static UnconnectedGraph? s_goodsGraph = null;
        static readonly object s_goodsLock = new object();

        private static UnconnectedGraph? s_servicesGraph = null;
        static readonly object s_servicesLock = new object();

        public static bool IsConnected(TransferReason material, ushort node1, ushort node2)
        {
            if (IsGoodsMaterial(material))
            {
                lock (s_goodsLock)
                {
                    return GetGoodsGraph().IsConnected(node1, node2);
                }
            }
            else
            {
                lock (s_servicesLock)
                {
                    return GetServicesGraph().IsConnected(node1, node2);
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

                    s_goodsGraph.FloodFill();
                }

                return s_goodsGraph;
            }
        }

        private static UnconnectedGraph GetServicesGraph()
        {
            lock (s_servicesLock)
            {
                if (s_servicesGraph == null || !s_servicesGraph.IsValid())
                {
                    s_servicesGraph = new UnconnectedGraph();

                    // Set up lane requirements
                    PathDistanceTypes.GetService(TransferReason.Goods, out s_servicesGraph.m_service1, out s_servicesGraph.m_service2, out s_servicesGraph.m_service3);
                    s_servicesGraph.m_laneTypes = PathDistanceTypes.GetLaneTypes(TransferReason.Dead);
                    s_servicesGraph.m_vehicleTypes = PathDistanceTypes.GetVehicleTypes(TransferReason.Dead);

                    s_servicesGraph.FloodFill();
                }

                return s_servicesGraph;
            }
        }

        public static ConnectedStorage GetGoodsBufferCopy()
        {
            lock (s_goodsLock)
            {
                return new ConnectedStorage(GetGoodsGraph().GetBuffer());
            }
        }

        public static ConnectedStorage GetServicesBufferCopy()
        {
            lock (s_servicesLock)
            {
                return new ConnectedStorage(GetServicesGraph().GetBuffer());
            }
        }

        private static bool IsGoodsMaterial(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Lumber:
                case TransferReason.Flours:
                case TransferReason.Paper:
                case TransferReason.PlanedTimber:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Glass:
                case TransferReason.Metals:
                case TransferReason.AnimalProducts:
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.Fish:
                    return true;

                default:
                    return false;
            }
        }
    }
}