using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    internal class TransferManagerModes
    {
        public enum TransferMode
        {
            Priority,
            IncomingFirst,
            OutgoingFirst,
            Balanced,
        }

        public static TransferMode GetTransferMode(TransferReason material)
        {
            switch (material)
            {
                // These are all 1-E08 in the vanilla GetDistanceMultiplier so would effectively match first offer
                case TransferReason.PartnerYoung:
                case TransferReason.PartnerAdult:
                case TransferReason.Family0:
                case TransferReason.Family1:
                case TransferReason.Family2:
                case TransferReason.Family3:
                case TransferReason.Single0:
                case TransferReason.Single1:
                case TransferReason.Single2:
                case TransferReason.Single3:
                case TransferReason.Single0B:
                case TransferReason.Single1B:
                case TransferReason.Single2B:
                case TransferReason.Single3B:
                case TransferReason.LeaveCity0:
                case TransferReason.LeaveCity1:
                case TransferReason.LeaveCity2:
                case TransferReason.DummyCar:        // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.DummyTrain:      // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.DummyShip:       // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.DummyPlane:      // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.TouristA:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case TransferReason.TouristB:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case TransferReason.TouristC:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case TransferReason.TouristD:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                    {
                        return TransferMode.Priority;
                    }

                case TransferReason.Garbage:        // Match building with closest LandFill
                case TransferReason.Dead:           // Matches bodies to cemeteries
                case TransferReason.Crime:          // Matches crime to police stations
                case TransferReason.Cash:           // New Financial District service
                case TransferReason.Mail:           // Matches building to PostOffice
                case TransferReason.Collapsed:      // Matches building to nearby service depot.
                case TransferReason.Fire:           // We always want the closest fire station to respond
                case TransferReason.Fire2:          // We always want the closest fire station to respond
                case TransferReason.ForestFire:     // We always want the closest fire station to respond
                case TransferReason.Sick:           // Always match patient with closest hospital
                case TransferReason.Sick2:          // Always match patient with closest hospital
                case TransferReason.SickMove:       // outgoing(active) from medcopter, incoming(passive) from hospitals -> moved to outgoing first so closest clinic is used
                case TransferReason.Student1:       // Match elementary student to closest school
                case TransferReason.Student2:       // Match high school student to closest school
                    {
                        return TransferMode.OutgoingFirst;
                    }

                case TransferReason.ElderCare:       // Citizen is IN, Eldercare Center is OUT.
                case TransferReason.ChildCare:       // Citizen is IN, Childcare Center is OUT.
                case TransferReason.Taxi:            // Taxi is OUT from Taxi Depot.
                case TransferReason.RoadMaintenance: // RoadMaintenance is OUT from service depot
                    {
                        return TransferMode.IncomingFirst;
                    }

                case TransferReason.DeadMove:        // Match Priority and Distance so Cemeteries match to Crematoriums first
                case TransferReason.CriminalMove:    // Match Priority and Distance
                case TransferReason.GarbageTransfer: // Match Priority and Distance so Landfill transfers to Recycling Center etc...
                case TransferReason.GarbageMove:     // Match Priority and Distance so Landfill transfers to Recycling Center etc...
                case TransferReason.Collapsed2:      // Collapsed2 only ever gets Priority 1 unless there is no road access so we use balanced match mode to ensure a match.
                default:
                    {
                        return TransferMode.Balanced;
                    }
            }
        }

        public static bool IsWarehouseMaterial(TransferReason material)
        {
            switch (material)
            {
                // Raw warehouses
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:

                // General warehouses
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
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        public static bool IsFactoryMaterial(TransferReason material)
        {
            switch (material)
            {
                // Raw materials used by ProcessingPlants
                case TransferReason.Oil:
                case TransferReason.Grain:
                case TransferReason.Logs:
                case TransferReason.Ore:

                // Material used by Generic Factories
                case TransferReason.Lumber:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:

                // DLC materials used by unique factories
                case TransferReason.Flours:
                case TransferReason.Paper:
                case TransferReason.PlanedTimber:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Glass:
                case TransferReason.Metals:
                case TransferReason.AnimalProducts:
                case TransferReason.Fish:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsFastChecksOnly(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.PartnerYoung:
                case TransferReason.PartnerAdult:
                case TransferReason.Family0:
                case TransferReason.Family1:
                case TransferReason.Family2:
                case TransferReason.Family3:
                case TransferReason.Single0:
                case TransferReason.Single1:
                case TransferReason.Single2:
                case TransferReason.Single3:
                case TransferReason.Single0B:
                case TransferReason.Single1B:
                case TransferReason.Single2B:
                case TransferReason.Single3B:
                case TransferReason.LeaveCity0:
                case TransferReason.LeaveCity1:
                case TransferReason.LeaveCity2:
                case TransferReason.DummyCar: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.DummyTrain: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.DummyShip: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.DummyPlane: // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case TransferReason.TouristA: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case TransferReason.TouristB: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case TransferReason.TouristC: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case TransferReason.TouristD: // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                    return true;

                default:
                    return false;
            }
        }

        public static Color GetTransferReasonColor(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.Crime:
                case TransferReason.CriminalMove:
                case TransferReason.Cash:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.SickMove:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Taxi:
                case TransferReason.Dead:
                case TransferReason.DeadMove:
                case TransferReason.Snow:
                case TransferReason.SnowMove:
                case TransferReason.EvacuateA:
                case TransferReason.EvacuateB:
                case TransferReason.EvacuateC:
                case TransferReason.EvacuateD:
                case TransferReason.EvacuateVipA:
                case TransferReason.EvacuateVipB:
                case TransferReason.EvacuateVipC:
                case TransferReason.EvacuateVipD:
                    return Color.magenta;

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
                    return Color.cyan;

                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.Fish:
                    return Color.blue;

                case TransferReason.Mail:
                case TransferReason.SortedMail:
                case TransferReason.UnsortedMail:
                case TransferReason.IncomingMail:
                case TransferReason.OutgoingMail:
                    return Color.green;

                default: return Color.yellow;
            }
        }
    }
}
