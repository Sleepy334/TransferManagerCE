using ColossalFramework.HTTP.Paradox;
using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using static RenderManager;
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

        public static PathDistanceAlgorithm GetDistanceAlgorithm(CustomTransferReason.Reason material)
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

        private static PathDistanceAlgorithm GetMaxPathDistanceAlgorithm(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                // Services
                case CustomTransferReason.Reason.Dead:
                case CustomTransferReason.Reason.Garbage:
                case CustomTransferReason.Reason.Sick:
                case CustomTransferReason.Reason.Crime:
                case CustomTransferReason.Reason.Fire:
                case CustomTransferReason.Reason.Mail:
                case CustomTransferReason.Reason.Mail2:
                case CustomTransferReason.Reason.Taxi:
                case CustomTransferReason.Reason.TaxiMove:
                case CustomTransferReason.Reason.Cash:
                case CustomTransferReason.Reason.CriminalMove:
                case CustomTransferReason.Reason.RoadMaintenance:
                case CustomTransferReason.Reason.Snow:
                    return PathDistanceAlgorithm.PathDistance;

                // Goods
                case CustomTransferReason.Reason.Oil:
                case CustomTransferReason.Reason.Ore:
                case CustomTransferReason.Reason.ForestProducts:
                case CustomTransferReason.Reason.Crops:
                case CustomTransferReason.Reason.Coal:
                case CustomTransferReason.Reason.Petrol:
                case CustomTransferReason.Reason.Food:
                case CustomTransferReason.Reason.Lumber:
                case CustomTransferReason.Reason.Flours:
                case CustomTransferReason.Reason.Paper:
                case CustomTransferReason.Reason.PlanedTimber:
                case CustomTransferReason.Reason.Petroleum:
                case CustomTransferReason.Reason.Plastics:
                case CustomTransferReason.Reason.Glass:
                case CustomTransferReason.Reason.Metals:
                case CustomTransferReason.Reason.AnimalProducts:
                case CustomTransferReason.Reason.Goods:
                case CustomTransferReason.Reason.LuxuryProducts:
                case CustomTransferReason.Reason.Fish:
                    return PathDistanceAlgorithm.PathDistance;

                // we want to scale by priority so need to use ConnectedLineOfSight
                case CustomTransferReason.Reason.DeadMove:
                case CustomTransferReason.Reason.GarbageMove:
                case CustomTransferReason.Reason.GarbageTransfer:
                case CustomTransferReason.Reason.SnowMove: 
                    return PathDistanceAlgorithm.ConnectedLineOfSight;

                // All transfers are to outside connections so just do connected line of sight
                case CustomTransferReason.Reason.UnsortedMail:
                case CustomTransferReason.Reason.SortedMail:
                case CustomTransferReason.Reason.IncomingMail:
                case CustomTransferReason.Reason.OutgoingMail:
                    return PathDistanceAlgorithm.ConnectedLineOfSight;

                // We get lots of no road access issues with all the parking assets from the workshop.
                case CustomTransferReason.Reason.ParkMaintenance: 
                    return PathDistanceAlgorithm.ConnectedLineOfSight;

                // Helicopter reasons
                case CustomTransferReason.Reason.Crime2:
                case CustomTransferReason.Reason.Sick2:
                case CustomTransferReason.Reason.SickMove:
                case CustomTransferReason.Reason.Collapsed2:
                    return PathDistanceAlgorithm.LineOfSight;

                default: 
                    return PathDistanceAlgorithm.LineOfSight;
            }
        }

        public static bool IsGoodsMaterial(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Oil:
                case CustomTransferReason.Reason.Ore:
                case CustomTransferReason.Reason.ForestProducts:
                case CustomTransferReason.Reason.Crops:
                case CustomTransferReason.Reason.Coal:
                case CustomTransferReason.Reason.Petrol:
                case CustomTransferReason.Reason.Food:
                case CustomTransferReason.Reason.Lumber:
                case CustomTransferReason.Reason.Flours:
                case CustomTransferReason.Reason.Paper:
                case CustomTransferReason.Reason.PlanedTimber:
                case CustomTransferReason.Reason.Petroleum:
                case CustomTransferReason.Reason.Plastics:
                case CustomTransferReason.Reason.Glass:
                case CustomTransferReason.Reason.Metals:
                case CustomTransferReason.Reason.AnimalProducts:
                case CustomTransferReason.Reason.Goods:
                case CustomTransferReason.Reason.LuxuryProducts:
                case CustomTransferReason.Reason.Fish:
                    return true;

                // These mail options use the goods paths for the pathing algorithm
                case CustomTransferReason.Reason.UnsortedMail:
                case CustomTransferReason.Reason.SortedMail:
                case CustomTransferReason.Reason.IncomingMail:
                case CustomTransferReason.Reason.OutgoingMail:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPedestrianZoneService(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Dead:
                case CustomTransferReason.Reason.Sick:
                case CustomTransferReason.Reason.Crime:
                case CustomTransferReason.Reason.Fire:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsOtherService(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Garbage:
                case CustomTransferReason.Reason.Mail:
                case CustomTransferReason.Reason.Mail2:
                case CustomTransferReason.Reason.Taxi:
                case CustomTransferReason.Reason.Cash:
                case CustomTransferReason.Reason.CriminalMove:
                case CustomTransferReason.Reason.RoadMaintenance:
                case CustomTransferReason.Reason.Snow:
                case CustomTransferReason.Reason.DeadMove:
                case CustomTransferReason.Reason.GarbageMove:
                case CustomTransferReason.Reason.GarbageTransfer:
                case CustomTransferReason.Reason.SnowMove:
                    return true;

                default:
                    return false;
            }
        }

        public static void GetService(bool bGoodsMaterial, out ItemClass.Service service1, out ItemClass.Service service2, out ItemClass.Service service3)
        {
            if (bGoodsMaterial)
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

        public static NetInfo.LaneType GetLaneTypes(bool bGoodsMaterial)
        {
            if (bGoodsMaterial)
            {
                return NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
            }
            else
            {
                return NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
            }
        }

        public static VehicleInfo.VehicleType GetVehicleTypes(bool bGoodsMaterial)
        {
            if (bGoodsMaterial)
            {
                return VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane | VehicleInfo.VehicleType.Ferry;
            }
            else
            {
                return VehicleInfo.VehicleType.Car;
            }
        }

        public static VehicleInfo.VehicleCategory GetVehicleCategory(bool bGoodsMaterial)
        {
            if (bGoodsMaterial)
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