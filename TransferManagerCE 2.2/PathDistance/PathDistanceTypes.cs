using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    public class PathDistanceTypes
    {
        public static bool IsPathDistanceSupported(TransferReason material)
        {
            switch (material)
            {
                // case TransferReason.DeadMove:  - Removed as we want to scale by priority
                // case TransferReason.GarbageMove: - Removed as we want to scale by priority
                // case TransferReason.GarbageTransfer: - Removed as we want to scale by priority
                // case TransferReason.SnowMove: - Removed as we want to scale by priority
                // case TransferReason.SickMove: - SickMove is for helicopters, it doesnt need path distance
                // case TransferReason.ParkMaintenance: - We get lots of no road access issues with all the parking assets from the workshop.
                case TransferReason.Dead:
                case TransferReason.Garbage:
                case TransferReason.Sick:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Cash:
                case TransferReason.CriminalMove:
                case TransferReason.RoadMaintenance:
                case TransferReason.Snow:
                    return SaveGameSettings.GetSettings().UsePathDistanceServices;

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
                    return SaveGameSettings.GetSettings().UsePathDistanceGoods;

                default: return false;
            }
        }

        public static bool IsConnectedLOSSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.DeadMove:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.SnowMove:
                    return SaveGameSettings.GetSettings().UsePathDistanceServices;

                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                    return SaveGameSettings.GetSettings().UsePathDistanceGoods;

                default: return false;
            }
        }

        public static bool IsGoodsMaterial(TransferReason material)
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

        public static bool IsPedestrianZoneService(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Dead:
                case TransferReason.Sick:
                case TransferReason.Crime:
                case TransferReason.Fire:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsOtherService(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Cash:
                case TransferReason.CriminalMove:
                case TransferReason.RoadMaintenance:
                case TransferReason.Snow:
                    return true;

                default:
                    return false;
            }
        }
        

        public static void GetService(TransferReason material, out ItemClass.Service service1, out ItemClass.Service service2, out ItemClass.Service service3)
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
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                    {
                        service1 = ItemClass.Service.Road;
                        service2 = ItemClass.Service.PublicTransport;
                        service3 = ItemClass.Service.Beautification;
                        break;
                    }
                default:
                    {
                        service1 = ItemClass.Service.Road;
                        service2 = ItemClass.Service.None;
                        service3 = ItemClass.Service.None;
                        break;
                    }
            }
        }

        public static NetInfo.LaneType GetLaneTypes(TransferReason material)
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
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                    {
                        return NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
                    }
                default:
                    {
                        return NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
                    }
            }
        }

        public static VehicleInfo.VehicleType GetVehicleTypes(TransferReason material)
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
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                    {
                        return VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane;
                    }
                default:
                    {
                        return VehicleInfo.VehicleType.Car;
                    }
            }
        }

        public static VehicleInfo.VehicleCategory GetVehicleCategory(TransferReason material)
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
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                    {
                        return VehicleInfo.VehicleCategory.CargoTruck | VehicleInfo.VehicleCategory.CargoPlane | VehicleInfo.VehicleCategory.CargoShip | VehicleInfo.VehicleCategory.CargoTrain;
                    }
                default:
                    {
                        return VehicleInfo.VehicleCategory.RoadTransport;
                    }
            }
        }
    }
}