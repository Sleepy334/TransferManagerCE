using TransferManagerCE.CustomManager;
using static TransferManager;

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

        public static bool IsConnected(CustomTransferReason.Reason material, ushort node1, ushort node2)
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

        private static PathConnected GetGoodsGraph()
        {
            lock (s_goodsLock)
            {
                if (s_goodsGraph is null || !s_goodsGraph.IsValid())
                {
                    s_goodsGraph = new PathConnected();

                    // Set up lane requirements
                    s_goodsGraph.SetMaterial(CustomTransferReason.Reason.Goods);
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
                    s_pedestrianZoneServicesGraph = new PathConnected();

                    // Set up lane requirements
                    s_pedestrianZoneServicesGraph.SetMaterial(CustomTransferReason.Reason.Dead);
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
                    s_otherServicesGraph = new PathConnected();

                    // Set up lane requirements
                    s_otherServicesGraph.SetMaterial(CustomTransferReason.Reason.Garbage);
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