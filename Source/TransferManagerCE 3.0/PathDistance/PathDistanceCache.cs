using static TransferManagerCE.NetworkModeHelper;

namespace TransferManagerCE
{
    public class PathDistanceCache
    {
        private static NodeLinkGraph s_goodsNodeLoader = null;
        private static NodeLinkGraph s_pedestrianZoneNodeLoader = null;
        private static NodeLinkGraph s_otherServicesNodeLoader = null;

        // ----------------------------------------------------------------------------------------
        public static NodeLinkGraph GetLoader(NetworkMode mode, bool bCheckValid)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        if (s_goodsNodeLoader is null)
                        {
                            s_goodsNodeLoader = new NodeLinkGraph(NetworkMode.Goods);
                        }
                        else if (bCheckValid)
                        {
                            s_goodsNodeLoader.Update();
                        }

                        return s_goodsNodeLoader;
                    }
                case NetworkMode.PedestrianZone:
                    {
                        if (s_pedestrianZoneNodeLoader is null)
                        {
                            s_pedestrianZoneNodeLoader = new NodeLinkGraph(NetworkMode.PedestrianZone);
                        }
                        else if (bCheckValid)
                        {
                            s_pedestrianZoneNodeLoader.Update();
                        }

                        return s_pedestrianZoneNodeLoader;
                    }
                case NetworkMode.OtherServices:
                    {
                        if (s_otherServicesNodeLoader is null)
                        {
                            s_otherServicesNodeLoader = new NodeLinkGraph(NetworkMode.OtherServices);
                        }
                        else if (bCheckValid)
                        {
                            s_otherServicesNodeLoader.Update();
                        }

                        return s_otherServicesNodeLoader;
                    }
            }

            return null;
        }

        // ----------------------------------------------------------------------------------------
        public static void Invalidate()
        {
            if (s_goodsNodeLoader is not null)
            {
                s_goodsNodeLoader.Invalidate();
            }
            if (s_pedestrianZoneNodeLoader is not null)
            {
                s_pedestrianZoneNodeLoader.Invalidate();
            }
            if (s_otherServicesNodeLoader is not null)
            {
                s_otherServicesNodeLoader.Invalidate();
            }

            // We also need to invalidate the connected data
            // as it is built from these arrays
            PathConnectedCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        public static void UpdateCache(NetworkMode mode)
        {
            GetLoader(mode, true);
        }

        // ----------------------------------------------------------------------------------------
        public static void UpdateCache()
        {
            UpdateCache(NetworkMode.Goods);
            UpdateCache(NetworkMode.PedestrianZone);
            UpdateCache(NetworkMode.OtherServices);
        }
    }
}