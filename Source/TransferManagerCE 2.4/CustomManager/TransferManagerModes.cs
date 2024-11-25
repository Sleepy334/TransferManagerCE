using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    public class TransferManagerModes
    {
        public enum TransferMode
        {
            Priority,
            IncomingFirst,
            OutgoingFirst,
            Balanced,
        }

        public static TransferMode GetTransferMode(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                // ---------------------------------------------------------------------------------
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

                // In vanilla these actually match to furthest away, for now we just do priority based matching (random)
                case CustomTransferReason.Reason.DummyCar:
                case CustomTransferReason.Reason.DummyTrain:
                case CustomTransferReason.Reason.DummyShip:
                case CustomTransferReason.Reason.DummyPlane:

                // Version 1.9.5 Moved these to priority based as we want tourists to spread around the city rather than match closely to the edge of the map
                case CustomTransferReason.Reason.TouristA:
                case CustomTransferReason.Reason.TouristB:
                case CustomTransferReason.Reason.TouristC:
                case CustomTransferReason.Reason.TouristD:
                case CustomTransferReason.Reason.BusinessA:
                case CustomTransferReason.Reason.BusinessB:
                case CustomTransferReason.Reason.BusinessC:
                case CustomTransferReason.Reason.BusinessD:
                case CustomTransferReason.Reason.NatureA:
                case CustomTransferReason.Reason.NatureB:
                case CustomTransferReason.Reason.NatureC:
                case CustomTransferReason.Reason.NatureD:
                    {
                        return TransferMode.Priority;
                    }

                // ---------------------------------------------------------------------------------
                case CustomTransferReason.Reason.Mail:           // We want vans (P:7) to be matched with closest mail first (IN) CloseByOnly: ON, Then buildings (OUT) to Post Offices, CloseByOnly: OFF
                case CustomTransferReason.Reason.Mail2:          // Buildings (OUT) to Post Offices, CloseByOnly: OFF
                case CustomTransferReason.Reason.Snow:           // Snow - We want trucks (P:7) to be matched with closest segment, and Snow Dump (P:1) to be matched with highest priority segment
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

                // We match shops first as the people dont actually care (priority is random) whereas shops will become abandoned.
                // We scale by priority so there is a bit of travelling to make the city feel alive, but shoppers will still shop locally most of the time
                case CustomTransferReason.Reason.Shopping:
                case CustomTransferReason.Reason.ShoppingB:
                case CustomTransferReason.Reason.ShoppingC:
                case CustomTransferReason.Reason.ShoppingD:
                case CustomTransferReason.Reason.ShoppingE:
                case CustomTransferReason.Reason.ShoppingF:
                case CustomTransferReason.Reason.ShoppingG:
                case CustomTransferReason.Reason.ShoppingH:
                case CustomTransferReason.Reason.Entertainment:
                case CustomTransferReason.Reason.EntertainmentB:
                case CustomTransferReason.Reason.EntertainmentC:
                case CustomTransferReason.Reason.EntertainmentD:
                    {
                        return TransferMode.OutgoingFirst;
                    }

                // ---------------------------------------------------------------------------------
                case CustomTransferReason.Reason.Taxi:            // Taxi is OUT from Taxi Depot, IncomingFirst will match Cim with closest Taxi/Taxi Depot
                case CustomTransferReason.Reason.TaxiMove:        // TaxiMove is IN to Taxi stands
                case CustomTransferReason.Reason.Goods:           // We match commercial first then factories OUT offers after
                case CustomTransferReason.Reason.LuxuryProducts:  // We match commercial first then factories OUT offers after
                case CustomTransferReason.Reason.ElderCare:       // Citizen is IN, Eldercare Center is OUT, IncomingFirst will match Cim with closest ElderCare facility
                case CustomTransferReason.Reason.ChildCare:       // Citizen is IN, Childcare Center is OUT, IncomingFirst will match Cim with closest ChildCare facility

                // Workers we match job to Cim so we can find close by workers
                case CustomTransferReason.Reason.Worker0:
                case CustomTransferReason.Reason.Worker1:
                case CustomTransferReason.Reason.Worker2:
                case CustomTransferReason.Reason.Worker3:
                    {
                        return TransferMode.IncomingFirst;
                    }

                // ---------------------------------------------------------------------------------
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

        public static bool IsScaleByPriority(CustomTransferReason.Reason material)
        {
            switch (material)
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
                case CustomTransferReason.Reason.Shopping:
                case CustomTransferReason.Reason.ShoppingB:
                case CustomTransferReason.Reason.ShoppingC:
                case CustomTransferReason.Reason.ShoppingD:
                case CustomTransferReason.Reason.ShoppingE:
                case CustomTransferReason.Reason.ShoppingF:
                case CustomTransferReason.Reason.ShoppingG:
                case CustomTransferReason.Reason.ShoppingH:
                case CustomTransferReason.Reason.Entertainment:
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

        public static bool IsWarehouseMaterial(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                // Raw warehouses
                case CustomTransferReason.Reason.Oil:
                case CustomTransferReason.Reason.Ore:
                case CustomTransferReason.Reason.ForestProducts:
                case CustomTransferReason.Reason.Crops:

                // General warehouses
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

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        public static bool IsFactoryMaterial(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                // Raw materials used by ProcessingPlants
                case CustomTransferReason.Reason.Oil:
                case CustomTransferReason.Reason.ForestProducts:
                case CustomTransferReason.Reason.Crops:
                case CustomTransferReason.Reason.Ore:

                // Material used by Generic Factories
                case CustomTransferReason.Reason.Lumber:
                case CustomTransferReason.Reason.Coal:
                case CustomTransferReason.Reason.Petrol:
                case CustomTransferReason.Reason.Food:

                // DLC materials used by unique factories
                case CustomTransferReason.Reason.Flours:
                case CustomTransferReason.Reason.Paper:
                case CustomTransferReason.Reason.PlanedTimber:
                case CustomTransferReason.Reason.Petroleum:
                case CustomTransferReason.Reason.Plastics:
                case CustomTransferReason.Reason.Glass:
                case CustomTransferReason.Reason.Metals:
                case CustomTransferReason.Reason.AnimalProducts:
                case CustomTransferReason.Reason.Fish:
                    return true;

                default:
                    return false;
            }
        }

        // These material types dont have any rules that can be applied so we can skip the more complicated restriction checks
        public static bool IsFastChecksOnly(CustomTransferReason.Reason material)
        {
            switch (material)
            {
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
                case CustomTransferReason.Reason.DummyCar:
                case CustomTransferReason.Reason.DummyTrain:
                case CustomTransferReason.Reason.DummyShip:
                case CustomTransferReason.Reason.DummyPlane:
                case CustomTransferReason.Reason.TouristA:
                case CustomTransferReason.Reason.TouristB:
                case CustomTransferReason.Reason.TouristC:
                case CustomTransferReason.Reason.TouristD:
                case CustomTransferReason.Reason.Entertainment:
                case CustomTransferReason.Reason.EntertainmentB:
                case CustomTransferReason.Reason.EntertainmentC:
                case CustomTransferReason.Reason.EntertainmentD:
                case CustomTransferReason.Reason.Shopping:
                case CustomTransferReason.Reason.ShoppingB:
                case CustomTransferReason.Reason.ShoppingC:
                case CustomTransferReason.Reason.ShoppingD:
                case CustomTransferReason.Reason.ShoppingE:
                case CustomTransferReason.Reason.ShoppingF:
                case CustomTransferReason.Reason.ShoppingG:
                case CustomTransferReason.Reason.ShoppingH:
                case CustomTransferReason.Reason.BusinessA:
                case CustomTransferReason.Reason.BusinessB:
                case CustomTransferReason.Reason.BusinessC:
                case CustomTransferReason.Reason.BusinessD:
                case CustomTransferReason.Reason.NatureA:
                case CustomTransferReason.Reason.NatureB:
                case CustomTransferReason.Reason.NatureC:
                case CustomTransferReason.Reason.NatureD:

                    return true;

                default:
                    return false;
            }
        }

        public static Color GetTransferReasonColor(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Garbage:
                case CustomTransferReason.Reason.GarbageMove:
                case CustomTransferReason.Reason.GarbageTransfer:
                case CustomTransferReason.Reason.Crime:
                case CustomTransferReason.Reason.CriminalMove:
                case CustomTransferReason.Reason.Cash:
                case CustomTransferReason.Reason.Fire:
                case CustomTransferReason.Reason.Fire2:
                case CustomTransferReason.Reason.ForestFire:
                case CustomTransferReason.Reason.Sick:
                case CustomTransferReason.Reason.Sick2:
                case CustomTransferReason.Reason.SickMove:
                case CustomTransferReason.Reason.Collapsed:
                case CustomTransferReason.Reason.Collapsed2:
                case CustomTransferReason.Reason.ParkMaintenance:
                case CustomTransferReason.Reason.Taxi:
                case CustomTransferReason.Reason.TaxiMove:
                case CustomTransferReason.Reason.Dead:
                case CustomTransferReason.Reason.DeadMove:
                case CustomTransferReason.Reason.Snow:
                case CustomTransferReason.Reason.SnowMove:
                case CustomTransferReason.Reason.EvacuateA:
                case CustomTransferReason.Reason.EvacuateB:
                case CustomTransferReason.Reason.EvacuateC:
                case CustomTransferReason.Reason.EvacuateD:
                case CustomTransferReason.Reason.EvacuateVipA:
                case CustomTransferReason.Reason.EvacuateVipB:
                case CustomTransferReason.Reason.EvacuateVipC:
                case CustomTransferReason.Reason.EvacuateVipD:
                    return Color.magenta;

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
                    return Color.cyan;

                case CustomTransferReason.Reason.Goods:
                case CustomTransferReason.Reason.LuxuryProducts:
                case CustomTransferReason.Reason.Fish:
                    return Color.blue;

                case CustomTransferReason.Reason.Mail:
                case CustomTransferReason.Reason.Mail2:
                case CustomTransferReason.Reason.SortedMail:
                case CustomTransferReason.Reason.UnsortedMail:
                case CustomTransferReason.Reason.IncomingMail:
                case CustomTransferReason.Reason.OutgoingMail:
                    return Color.green;

                default: return Color.yellow;
            }
        }

        // To reproduce vanilla behaviour we choose an offer if within "acceptable" distance, which allows us to exit early
        // instead of having to find the closest possible match that may be lower priority
        public static float GetAcceptableDistance(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Entertainment:
                case CustomTransferReason.Reason.EntertainmentB:
                case CustomTransferReason.Reason.EntertainmentC:
                case CustomTransferReason.Reason.EntertainmentD:
                    return 1000000f; // Accept offer if within 1km

                case CustomTransferReason.Reason.Shopping:
                case CustomTransferReason.Reason.ShoppingB:
                case CustomTransferReason.Reason.ShoppingC:
                case CustomTransferReason.Reason.ShoppingD:
                case CustomTransferReason.Reason.ShoppingE:
                case CustomTransferReason.Reason.ShoppingF:
                case CustomTransferReason.Reason.ShoppingG:
                case CustomTransferReason.Reason.ShoppingH:
                case CustomTransferReason.Reason.Worker0:
                case CustomTransferReason.Reason.Worker1:
                case CustomTransferReason.Reason.Worker2:
                case CustomTransferReason.Reason.Worker3:
                    return 250000f; // Accept offer if within 500m

                default:
                    return 0.0f; // Match closest
            }
        }

        // We restrict the match distance for the secondary match loops for these materials so they dont end up with bad matches
        public static bool ApplyCloseByOny(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Mail:
                case CustomTransferReason.Reason.Mail2:
                case CustomTransferReason.Reason.Snow:
                case CustomTransferReason.Reason.Garbage:
                case CustomTransferReason.Reason.Dead:
                case CustomTransferReason.Reason.Crime:
                case CustomTransferReason.Reason.Crime2:
                case CustomTransferReason.Reason.CriminalMove:
                case CustomTransferReason.Reason.Cash:
                case CustomTransferReason.Reason.Collapsed:
                case CustomTransferReason.Reason.Fire:
                case CustomTransferReason.Reason.Fire2:
                case CustomTransferReason.Reason.ForestFire:
                case CustomTransferReason.Reason.Sick:
                case CustomTransferReason.Reason.Sick2:
                case CustomTransferReason.Reason.ElderCare:
                case CustomTransferReason.Reason.ChildCare:
                case CustomTransferReason.Reason.Taxi:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsHelicopterReason(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Sick2:
                case CustomTransferReason.Reason.SickMove:
                case CustomTransferReason.Reason.Collapsed2:
                case CustomTransferReason.Reason.Fire2:
                case CustomTransferReason.Reason.ForestFire:
                case CustomTransferReason.Reason.Crime2:
                    return true;
            }

            return false;
        }

        public static bool IsExportRestrictionsSupported(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Oil:
                case CustomTransferReason.Reason.Ore:
                case CustomTransferReason.Reason.ForestProducts:
                case CustomTransferReason.Reason.Crops:
                case CustomTransferReason.Reason.Goods:
                case CustomTransferReason.Reason.Food:
                case CustomTransferReason.Reason.Coal:
                case CustomTransferReason.Reason.Petrol:
                case CustomTransferReason.Reason.Lumber:
                case CustomTransferReason.Reason.AnimalProducts:
                case CustomTransferReason.Reason.Flours:
                case CustomTransferReason.Reason.Paper:
                case CustomTransferReason.Reason.PlanedTimber:
                case CustomTransferReason.Reason.Petroleum:
                case CustomTransferReason.Reason.Plastics:
                case CustomTransferReason.Reason.Glass:
                case CustomTransferReason.Reason.Metals:
                case CustomTransferReason.Reason.LuxuryProducts:
                case CustomTransferReason.Reason.Fish:
                case CustomTransferReason.Reason.OutgoingMail:
                case CustomTransferReason.Reason.UnsortedMail:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsImportRestrictionsSupported(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Oil:
                case CustomTransferReason.Reason.Ore:
                case CustomTransferReason.Reason.ForestProducts:
                case CustomTransferReason.Reason.Crops:
                case CustomTransferReason.Reason.Goods:
                case CustomTransferReason.Reason.Food:
                case CustomTransferReason.Reason.Coal:
                case CustomTransferReason.Reason.Petrol:
                case CustomTransferReason.Reason.Lumber:
                case CustomTransferReason.Reason.IncomingMail:
                case CustomTransferReason.Reason.SortedMail:
                    return true;

                default:
                    return false;
            }
        }
    }
}
