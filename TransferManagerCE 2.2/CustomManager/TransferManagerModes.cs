﻿using System;
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

        public static TransferMode GetTransferMode(CustomTransferReason material)
        {
            switch (material.ToReason())
            {
                // These are all 1-E08 in the vanilla GetDistanceMultiplier so would effectively match first offer
                case CustomTransferReason.Reason.PartnerYoung:
                case CustomTransferReason.Reason.PartnerAdult:
                case CustomTransferReason.Reason.Family0:
                case CustomTransferReason.Reason.Family1:
                case CustomTransferReason.Reason.Family2:
                case CustomTransferReason.Reason.Family3:
                case CustomTransferReason.Reason.Single0:
                case CustomTransferReason.Reason.Single1:
                case CustomTransferReason.Reason.Single2:
                case CustomTransferReason.Reason.Single3:
                case CustomTransferReason.Reason.Single0B:
                case CustomTransferReason.Reason.Single1B:
                case CustomTransferReason.Reason.Single2B:
                case CustomTransferReason.Reason.Single3B:
                case CustomTransferReason.Reason.LeaveCity0:
                case CustomTransferReason.Reason.LeaveCity1:
                case CustomTransferReason.Reason.LeaveCity2:
                case CustomTransferReason.Reason.DummyCar:        // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case CustomTransferReason.Reason.DummyTrain:      // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case CustomTransferReason.Reason.DummyShip:       // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case CustomTransferReason.Reason.DummyPlane:      // In vanilla these actually match to furthest away, for now we just do priority based matching(random)
                case CustomTransferReason.Reason.TouristA:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case CustomTransferReason.Reason.TouristB:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case CustomTransferReason.Reason.TouristC:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case CustomTransferReason.Reason.TouristD:        // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                    {
                        return TransferMode.Priority;
                    }

                case CustomTransferReason.Reason.Mail:            // We want vans (P:7) to be matched with closest mail first (IN) CloseByOnly: ON, Then buildings (OUT) to Post Offices, CloseByOnly: OFF
                case CustomTransferReason.Reason.Snow:            // Snow - We want trucks (P:7) to be matched with closest segment, and Snow Dump (P:1) to be matched with highest priority segment
                case CustomTransferReason.Reason.Garbage:        // Match building with closest LandFill
                case CustomTransferReason.Reason.Dead:           // Matches bodies to cemeteries
                case CustomTransferReason.Reason.Crime:          // Matches crime to police stations
                case CustomTransferReason.Reason.Crime2:         // Matches crime to police helicopter depots
                case CustomTransferReason.Reason.CriminalMove:   // Match Police station with closest prison
                case CustomTransferReason.Reason.Cash:           // New Financial District service
                case CustomTransferReason.Reason.Collapsed:      // Matches building to nearby service depot.
                case CustomTransferReason.Reason.Fire:           // We always want the closest fire station to respond
                case CustomTransferReason.Reason.Fire2:          // We always want the closest fire station to respond
                case CustomTransferReason.Reason.ForestFire:     // We always want the closest fire station to respond
                case CustomTransferReason.Reason.Sick:           // Always match patient with closest hospital
                case CustomTransferReason.Reason.Sick2:          // Always match patient with closest hospital
                case CustomTransferReason.Reason.SickMove:       // outgoing(active) from medcopter, incoming(passive) from hospitals -> moved to outgoing first so closest clinic is used
                case CustomTransferReason.Reason.StudentES:       // Match elementary student to closest school
                case CustomTransferReason.Reason.StudentHS:       // Match high school student to closest school
                    {
                        return TransferMode.OutgoingFirst;
                    }

                case CustomTransferReason.Reason.Goods:           // We match commercial first then factories OUT offers after
                case CustomTransferReason.Reason.LuxuryProducts:  // We match commercial first then factories OUT offers after
                case CustomTransferReason.Reason.ElderCare:       // Citizen is IN, Eldercare Center is OUT, IncomingFirst will match Cim with closest ElderCare facility
                case CustomTransferReason.Reason.ChildCare:       // Citizen is IN, Childcare Center is OUT, IncomingFirst will match Cim with closest ChildCare facility
                case CustomTransferReason.Reason.Taxi:            // Taxi is OUT from Taxi Depot, IncomingFirst will match Cim with closest Taxi/Taxi Depot
                    {
                        return TransferMode.IncomingFirst;
                    }

                case CustomTransferReason.Reason.SnowMove:        // SnowMove - Balanced and Priority scaled.
                case CustomTransferReason.Reason.RoadMaintenance: // RoadMaintenance - We want trucks (P:7) to be matched with closest segment, and Depot (P:1) to be matched with highest priority segment
                case CustomTransferReason.Reason.DeadMove:        // Match Priority and Distance so Cemeteries match to Crematoriums first
                case CustomTransferReason.Reason.GarbageTransfer: // Match Priority and Distance so Landfill transfers to Recycling Center etc...
                case CustomTransferReason.Reason.GarbageMove:     // Match Priority and Distance so Landfill transfers to Recycling Center etc...
                case CustomTransferReason.Reason.Collapsed2:      // Collapsed2 only ever gets Priority 1 unless there is no road access so we use balanced match mode to ensure a match.
                default:
                    {
                        return TransferMode.Balanced;
                    }
            }
        }

        public static bool IsScaleByPriority(CustomTransferReason material)
        {
            switch (material.ToReason())
            {
                // Note: A lot of these are purely priority matched but we leave the material here in case it ever
                // gets moved to a different mode.
                case CustomTransferReason.Reason.PartnerYoung:
                case CustomTransferReason.Reason.PartnerAdult:
                case CustomTransferReason.Reason.Family0:
                case CustomTransferReason.Reason.Family1:
                case CustomTransferReason.Reason.Family2:
                case CustomTransferReason.Reason.Family3:
                case CustomTransferReason.Reason.Single0:
                case CustomTransferReason.Reason.Single1:
                case CustomTransferReason.Reason.Single2:
                case CustomTransferReason.Reason.Single3:
                case CustomTransferReason.Reason.Single0B:
                case CustomTransferReason.Reason.Single1B:
                case CustomTransferReason.Reason.Single2B:
                case CustomTransferReason.Reason.Single3B:
                case CustomTransferReason.Reason.LeaveCity0:
                case CustomTransferReason.Reason.LeaveCity1:
                case CustomTransferReason.Reason.LeaveCity2:
                case CustomTransferReason.Reason.Worker0:
                case CustomTransferReason.Reason.Worker1:
                case CustomTransferReason.Reason.Worker2:
                case CustomTransferReason.Reason.Worker3:
                case CustomTransferReason.Reason.StudentES:
                case CustomTransferReason.Reason.StudentHS:
                case CustomTransferReason.Reason.StudentUni:
                case CustomTransferReason.Reason.Entertainment:
                case CustomTransferReason.Reason.Shopping:
                case CustomTransferReason.Reason.ShoppingB:
                case CustomTransferReason.Reason.ShoppingC:
                case CustomTransferReason.Reason.ShoppingD:
                case CustomTransferReason.Reason.ShoppingE:
                case CustomTransferReason.Reason.ShoppingF:
                case CustomTransferReason.Reason.ShoppingG:
                case CustomTransferReason.Reason.ShoppingH:
                case CustomTransferReason.Reason.EntertainmentB:
                case CustomTransferReason.Reason.EntertainmentC:
                case CustomTransferReason.Reason.EntertainmentD:
                case CustomTransferReason.Reason.TouristA:
                case CustomTransferReason.Reason.TouristB:
                case CustomTransferReason.Reason.TouristC:
                case CustomTransferReason.Reason.TouristD:

                // Scale mail sorting reasons so post sorting facilites and post offices get filled/emptied based on need
                case CustomTransferReason.Reason.OutgoingMail:
                case CustomTransferReason.Reason.IncomingMail:
                case CustomTransferReason.Reason.UnsortedMail:
                case CustomTransferReason.Reason.SortedMail:

                // Need to scale the *Move functions by priority as well
                // so that we match Cemeteries with Crematoriums and Landfill with Recycling plants
                case CustomTransferReason.Reason.CriminalMove:
                case CustomTransferReason.Reason.DeadMove:
                case CustomTransferReason.Reason.GarbageMove:
                case CustomTransferReason.Reason.GarbageTransfer:
                case CustomTransferReason.Reason.SnowMove:
                    {
                        // Scale by priority. Higher priorities will appear closer
                        return true;
                    }

                default:
                    {
                        // No priority scaling
                        return false;
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
