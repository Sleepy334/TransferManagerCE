using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.SaveGameSettings;

namespace TransferManagerCE.CustomManager
{
    public class PathDistanceTypes
    {
        public enum PathDistanceAlgorithm
        {
            LineOfSight = 0,
            ConnectedLineOfSight = 1,
            PathDistance = 2
        }

        public static PathDistanceAlgorithm GetDistanceAlgorithm(TransferReason material)
        {
            PathDistanceAlgorithm algorithm;

            if (PathDistanceTypes.IsGoodsMaterial(material))
            {
                algorithm = (PathDistanceAlgorithm) Math.Min((int)GetMaxPathDistanceAlgorithm(material), SaveGameSettings.GetSettings().PathDistanceGoods);
            }
            else
            {
                algorithm = (PathDistanceAlgorithm) Math.Min((int)GetMaxPathDistanceAlgorithm(material), SaveGameSettings.GetSettings().PathDistanceServices);
            }
            return algorithm;
        }

        private static PathDistanceAlgorithm GetMaxPathDistanceAlgorithm(TransferReason material)
        {
            switch (material)
            {
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
                    return PathDistanceAlgorithm.PathDistance; // Services

                case TransferReason.DeadMove: // we want to scale by priority
                case TransferReason.GarbageMove: // we want to scale by priority
                case TransferReason.GarbageTransfer: // we want to scale by priority
                case TransferReason.SnowMove: // we want to scale by priority
                    return PathDistanceAlgorithm.ConnectedLineOfSight;

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
                    return PathDistanceAlgorithm.PathDistance; // Goods

                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                    return PathDistanceAlgorithm.ConnectedLineOfSight;

                default: 
                    return PathDistanceAlgorithm.LineOfSight;
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

                // These mail options use the goods paths for the pathing algorithm
                case TransferReason.UnsortedMail:
                case TransferReason.SortedMail:
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
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
                case TransferReason.DeadMove:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.SnowMove:
                    return true;

                default:
                    return false;
            }
        }

        public static void GetService(TransferReason material, out ItemClass.Service service1, out ItemClass.Service service2, out ItemClass.Service service3)
        {
            if (IsGoodsMaterial(material))
            {
                service1 = ItemClass.Service.Road;
                service2 = ItemClass.Service.PublicTransport;
                service3 = ItemClass.Service.Beautification; // Cargo stations label their connector nodes as Beautification. Why CO?
            }
            else
            {
                service1 = ItemClass.Service.Road;
                service2 = ItemClass.Service.None;
                service3 = ItemClass.Service.None;
            }
        }

        public static NetInfo.LaneType GetLaneTypes(TransferReason material)
        {
            if (IsGoodsMaterial(material))
            {
                return NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
            }
            else
            {
                return NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
            }
        }

        public static VehicleInfo.VehicleType GetVehicleTypes(TransferReason material)
        {
            if (IsGoodsMaterial(material))
            {
                return VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane;
            }
            else
            {
                return VehicleInfo.VehicleType.Car;
            }
        }

        public static VehicleInfo.VehicleCategory GetVehicleCategory(TransferReason material)
        {
            if (IsGoodsMaterial(material))
            {
                return VehicleInfo.VehicleCategory.CargoTruck | VehicleInfo.VehicleCategory.CargoPlane | VehicleInfo.VehicleCategory.CargoShip | VehicleInfo.VehicleCategory.CargoTrain;
            } 
            else
            {
                return VehicleInfo.VehicleCategory.RoadTransport;
            }
        }
    }
}