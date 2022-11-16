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
                        lock (s_goodsLock)
                        {
                            if (s_goodsGraph == null || !s_goodsGraph.IsValid())
                            {
                                s_goodsGraph = new UnconnectedGraph();
                                NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(TransferReason.Goods);
                                s_goodsGraph.FloodFill(laneType);
                            }

                            // Return a copy for use in the transfer manager
                            return s_goodsGraph.IsConnected(node1, node2);
                        }
                    }
                default:
                    {
                        lock (s_servicesLock)
                        {
                            if (s_servicesGraph == null || !s_servicesGraph.IsValid())
                            {
                                s_servicesGraph = new UnconnectedGraph();
                                NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(TransferReason.Dead);
                                s_servicesGraph.FloodFill(laneType);
                            }

                            // Return a copy for use in the transfer manager
                            return s_servicesGraph.IsConnected(node1, node2);
                        }
                    }
            }
        }
    }
}