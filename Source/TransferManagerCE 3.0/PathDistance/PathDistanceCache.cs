using static TransferManagerCE.NetworkModeHelper;

namespace TransferManagerCE
{
    public class PathDistanceCache
    {
        private static NodeLinkGraph m_goodsNodeLoader = null;
        private static NodeLinkGraph m_pedestrianZoneNodeLoader = null;
        private static NodeLinkGraph m_otherServicesNodeLoader = null;

        // ----------------------------------------------------------------------------------------
        public static NodeLinkGraph GetLoader(NetworkMode mode, bool bCheckValid)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        if (m_goodsNodeLoader is null)
                        {
                            m_goodsNodeLoader = new NodeLinkGraph(NetworkMode.Goods);
                        }

                        if (bCheckValid)
                        {
                            m_goodsNodeLoader.Update();
                        }

                        return m_goodsNodeLoader;
                    }
                case NetworkMode.PedestrianZone:
                    {
                        if (m_pedestrianZoneNodeLoader is null)
                        {
                            m_pedestrianZoneNodeLoader = new NodeLinkGraph(NetworkMode.PedestrianZone);
                        }

                        if (bCheckValid)
                        {
                            m_pedestrianZoneNodeLoader.Update();
                        }

                        return m_pedestrianZoneNodeLoader;
                    }
                case NetworkMode.OtherServices:
                    {
                        if (m_otherServicesNodeLoader is null)
                        {
                            m_otherServicesNodeLoader = new NodeLinkGraph(NetworkMode.OtherServices);
                        }

                        if (bCheckValid)
                        {
                            m_otherServicesNodeLoader.Update();
                        }

                        return m_otherServicesNodeLoader;
                    }
            }

            return null;
        }

        // ----------------------------------------------------------------------------------------
        public static void Invalidate()
        {
            if (m_goodsNodeLoader is not null)
            {
                m_goodsNodeLoader.Invalidate();
            }
            if (m_pedestrianZoneNodeLoader is not null)
            {
                m_pedestrianZoneNodeLoader.Invalidate();
            }
            if (m_otherServicesNodeLoader is not null)
            {
                m_otherServicesNodeLoader.Invalidate();
            }

            // We also need to invalidate the connected data
            // as it is built from these arrays
            PathConnectedCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        public static void UpdateCache(NetworkMode mode)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        m_goodsNodeLoader = GetLoader(NetworkMode.Goods, true);
                        break;
                    }
                case NetworkMode.PedestrianZone:
                    {
                        m_pedestrianZoneNodeLoader = GetLoader(NetworkMode.PedestrianZone, true);
                        break;
                    }
                case NetworkMode.OtherServices:
                    {
                        m_otherServicesNodeLoader = GetLoader(NetworkMode.OtherServices, true);
                        break;
                    }
            }
        }

        // ----------------------------------------------------------------------------------------
        public static void UpdateCache()
        {
            if (SaveGameSettings.GetSettings().PathDistanceGoods >= 2 ||
                SaveGameSettings.GetSettings().PathDistanceServices >= 2)
            {
                UpdateCache(NetworkMode.Goods);
                UpdateCache(NetworkMode.PedestrianZone);
                UpdateCache(NetworkMode.OtherServices);
            }
        }
    }
}