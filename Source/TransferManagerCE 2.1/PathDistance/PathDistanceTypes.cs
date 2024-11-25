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
                // case TransferReason.CriminalMove: - Removed as we want to scale by priority

                case TransferReason.Dead:
                case TransferReason.Garbage:
                case TransferReason.Sick:
                case TransferReason.SickMove:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Cash:
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