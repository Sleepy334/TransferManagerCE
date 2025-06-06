using static TransferManagerCE.NetworkModeHelper;

namespace TransferManagerCE
{
    public class PathConnectedCache
    {
        private static PathConnected? s_goodsGraph = null;
        static readonly object s_goodsLock = new object();

        private static PathConnected? s_pedestrianZoneServicesGraph = null;
        static readonly object s_pedestrianZoneServicesLock = new object();

        private static PathConnected? s_otherServicesGraph = null;
        static readonly object s_otherServicesLock = new object();

        // ----------------------------------------------------------------------------------------
        public static bool IsConnected(NetworkMode mode, ushort node1, ushort node2)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        lock (s_goodsLock)
                        {
                            return GetGoodsGraph().IsConnected(node1, node2);
                        }
                    }
                case NetworkMode.PedestrianZone:
                    {
                        lock (s_pedestrianZoneServicesLock)
                        {
                            return GetPedestrianZoneServicesGraph().IsConnected(node1, node2);
                        }
                    }
                case NetworkMode.OtherServices:
                    {
                        lock (s_otherServicesLock)
                        {
                            return GetOtherServicesGraph().IsConnected(node1, node2);
                        }
                    }
            }

            return false;
        }

        public static PathConnected GetGraph(NetworkMode mode)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        return GetGoodsGraph();
                    }
                case NetworkMode.PedestrianZone:
                    {
                        return GetPedestrianZoneServicesGraph();
                    }
                case NetworkMode.OtherServices:
                    {
                        return GetOtherServicesGraph();
                    }
                default:
                    {
                        return GetGoodsGraph();
                    }
            }
        }

        private static PathConnected GetGoodsGraph()
        {
            lock (s_goodsLock)
            {
                if (s_goodsGraph is null || !s_goodsGraph.IsValid())
                {
                    s_goodsGraph = new PathConnected(NetworkMode.Goods);
                    s_goodsGraph.FloodFill();
                }

                return s_goodsGraph;
            }
        }

        private static PathConnected GetPedestrianZoneServicesGraph()
        {
            lock (s_pedestrianZoneServicesLock)
            {
                if (s_pedestrianZoneServicesGraph is null || !s_pedestrianZoneServicesGraph.IsValid())
                {
                    s_pedestrianZoneServicesGraph = new PathConnected(NetworkMode.PedestrianZone);
                    s_pedestrianZoneServicesGraph.FloodFill();
                }

                return s_pedestrianZoneServicesGraph;
            }
        }

        private static PathConnected GetOtherServicesGraph()
        {
            lock (s_otherServicesLock)
            {
                if (s_otherServicesGraph is null || !s_otherServicesGraph.IsValid())
                {
                    s_otherServicesGraph = new PathConnected(NetworkMode.OtherServices);
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

        public static void Invalidate()
        {
            if (s_goodsGraph is not null)
            {
                s_goodsGraph.Invalidate();
            }
            if (s_pedestrianZoneServicesGraph is not null)
            {
                s_pedestrianZoneServicesGraph.Invalidate();
            }
            if (s_otherServicesGraph is not null)
            {
                s_otherServicesGraph.Invalidate();
            }
        }
    }
}