using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    public class NetworkModeHelper 
    {
        public enum NetworkMode
        {
            None,
            Goods,
            PedestrianZone,
            OtherServices
        }

        public static NetworkMode GetNetwokMode(CustomTransferReason.Reason material)
        {
            if (PathDistanceTypes.IsGoodsMaterial(material))
            {
                return NetworkMode.Goods;
            }
            else if (PathDistanceTypes.IsPedestrianZoneService(material))
            {
                return NetworkMode.PedestrianZone;
            }
            else
            {
                return NetworkMode.OtherServices;
            }
        }

        public static CustomTransferReason.Reason GetTransferReason(NetworkMode mode)
        {
            switch (mode)
            {
                case NetworkMode.Goods:
                    {
                        return CustomTransferReason.Reason.Goods;
                    }
                case NetworkMode.PedestrianZone:
                    {
                        return CustomTransferReason.Reason.Fire; 
                    }
                case NetworkMode.OtherServices:
                    {
                        return CustomTransferReason.Reason.Garbage;
                    }
            }

            return CustomTransferReason.Reason.None;
        }
    }
}