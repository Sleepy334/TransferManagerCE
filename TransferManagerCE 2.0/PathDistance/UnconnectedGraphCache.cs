using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class UnconnectedGraphCache
    {
        private static UnconnectedGraph? s_goodsGraph;
        static readonly object s_goodsLock = new object();

        private static UnconnectedGraph? s_servicesGraph;
        static readonly object s_servicesLock = new object();

        public static void Invalidate()
        {
            lock (s_servicesLock)
            {
                s_servicesGraph = null;
            }

            lock (s_goodsLock)
            {
                s_goodsGraph = null;
            }
        }

        public static bool IsConnected(TransferReason material, ushort nodeId1, ushort nodeId2)
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
                    {
                        return IsGoodsConnected(nodeId1, nodeId2);
                    }
                default:
                    {
                        return IsServicesConnected(nodeId1, nodeId2);
                    }
            }
        }

        private static void InitServices()
        {
            lock (s_servicesLock)
            {
                if (s_servicesGraph == null)
                {
                    s_servicesGraph = new UnconnectedGraph();
                    NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(TransferReason.Dead);
                    s_servicesGraph.FloodFill(laneType);
                }
            }
        }

        private static void InitGoods()
        {
            lock (s_goodsLock)
            {
                if (s_goodsGraph == null)
                {
                    s_goodsGraph = new UnconnectedGraph();
                    NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(TransferReason.Goods);
                    s_goodsGraph.FloodFill(laneType);
                }
            }
        }

        private static bool IsServicesConnected(ushort nodeId1, ushort nodeId2)
        {
            lock (s_servicesLock)
            {
                if (s_servicesGraph == null)
                {
                    InitServices();
                }
                
                return s_servicesGraph.IsConnected(nodeId1, nodeId2);
            }
        }

        private static bool IsGoodsConnected(ushort nodeId1, ushort nodeId2)
        {
            lock (s_goodsLock)
            {
                if (s_goodsGraph == null)
                {
                    InitGoods();
                }

                return s_goodsGraph.IsConnected(nodeId1, nodeId2);
            }
        }
    }
}