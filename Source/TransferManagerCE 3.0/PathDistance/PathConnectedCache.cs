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
                            return GetGoodsGraph(false).IsConnected(node1, node2);
                        }
                    }
                case NetworkMode.PedestrianZone:
                    {
                        lock (s_pedestrianZoneServicesLock)
                        {
                            return GetPedestrianZoneServicesGraph(false).IsConnected(node1, node2);
                        }
                    }
                case NetworkMode.OtherServices:
                    {
                        lock (s_otherServicesLock)
                        {
                            return GetOtherServicesGraph(false).IsConnected(node1, node2);
                        }
                    }
            }

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public static int GetNodeColor(NetworkMode mode, ushort node)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        lock (s_goodsLock)
                        {
                            return GetGoodsGraph(false).GetColor(node);
                        }
                    }
                case NetworkMode.PedestrianZone:
                    {
                        lock (s_pedestrianZoneServicesLock)
                        {
                            return GetPedestrianZoneServicesGraph(false).GetColor(node);
                        }
                    }
                case NetworkMode.OtherServices:
                    {
                        lock (s_otherServicesLock)
                        {
                            return GetOtherServicesGraph(false).GetColor(node);
                        }
                    }
            }

            return 0;
        }

        // ----------------------------------------------------------------------------------------
        public static void UpdateCache(NetworkMode mode)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        GetGoodsGraph(true);
                        break;
                    }
                case NetworkMode.PedestrianZone:
                    {
                        GetPedestrianZoneServicesGraph(true);
                        break;
                    }
                case NetworkMode.OtherServices:
                    {
                        GetOtherServicesGraph(true);
                        break;
                    }
            }
        }

        // ----------------------------------------------------------------------------------------
        public static ConnectedStorage GetGoodsBufferCopy()
        {
            lock (s_goodsLock)
            {
                return new ConnectedStorage(GetGoodsGraph(true).GetBuffer());
            }
        }

        // ----------------------------------------------------------------------------------------
        public static ConnectedStorage GetPedestrianZoneServicesBufferCopy()
        {
            lock (s_pedestrianZoneServicesLock)
            {
                return new ConnectedStorage(GetPedestrianZoneServicesGraph(true).GetBuffer());
            }
        }

        // ----------------------------------------------------------------------------------------
        public static ConnectedStorage GetOtherServicesBufferCopy()
        {
            lock (s_otherServicesLock)
            {
                return new ConnectedStorage(GetOtherServicesGraph(true).GetBuffer());
            }
        }

        // ----------------------------------------------------------------------------------------
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

        // ----------------------------------------------------------------------------------------
        private static PathConnected GetGoodsGraph(bool bCheckIsValid)
        {
            lock (s_goodsLock)
            {
                if (s_goodsGraph is null)
                {
                    s_goodsGraph = new PathConnected(NetworkMode.Goods);
                    s_goodsGraph.FloodFill();
                }
                else if (bCheckIsValid && !s_goodsGraph.IsValid())
                {
                    s_goodsGraph.FloodFill();
                }

                return s_goodsGraph;
            }
        }

        // ----------------------------------------------------------------------------------------
        private static PathConnected GetPedestrianZoneServicesGraph(bool bCheckIsValid)
        {
            lock (s_pedestrianZoneServicesLock)
            {
                if (s_pedestrianZoneServicesGraph is null)
                {
                    s_pedestrianZoneServicesGraph = new PathConnected(NetworkMode.PedestrianZone);
                    s_pedestrianZoneServicesGraph.FloodFill();
                }
                else if (bCheckIsValid && !s_pedestrianZoneServicesGraph.IsValid())
                {
                    s_pedestrianZoneServicesGraph.FloodFill();
                }

                return s_pedestrianZoneServicesGraph;
            }
        }

        // ----------------------------------------------------------------------------------------
        private static PathConnected GetOtherServicesGraph(bool bCheckIsValid)
        {
            lock (s_otherServicesLock)
            {
                if (s_otherServicesGraph is null)
                {
                    s_otherServicesGraph = new PathConnected(NetworkMode.OtherServices);
                    s_otherServicesGraph.FloodFill();
                }
                else if (bCheckIsValid && !s_otherServicesGraph.IsValid())
                {
                    s_otherServicesGraph.FloodFill();
                }

                return s_otherServicesGraph;
            }
        }
    }
}