using System;
using System.Runtime.CompilerServices;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    internal class TransferManagerModes
    {
        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static CustomTransferManager.OFFER_MATCHMODE GetMatchOffersMode(TransferReason material)
        {
            //incoming first: pick highest priority outgoing offers by distance
            //outgoing first: try to fulfill all outgoing offers by descending priority. incoming offer mapped by distance only (priority not relevant).
            //balanced: outgoing/incoming together by priorty descending
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Grain:
                case TransferReason.Lumber:
                case TransferReason.Logs:
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Metals:
                case TransferReason.Glass:
                case TransferReason.PlanedTimber:
                case TransferReason.Paper:
                case TransferReason.Fish:
                    //warehouse incoming behaviour: empty=prio 0; balanced=prio 0-2; fill=prio 2;
                    //warehouse outgoing behaviour: empty=prio 2 ; balanced=prio 0-2; fill=prio 0;
                    return CustomTransferManager.OFFER_MATCHMODE.BALANCED;

                // all mail services like goods transfers:
                case TransferReason.Mail:               //outgoing (passive) from buidings, incoming(active) from postoffice
                case TransferReason.SortedMail:         //outside connections outgoing(active), incoming(passive) from postoffice
                case TransferReason.UnsortedMail:       //outgoing(active) from ???, incoming(passive) from postsortingfacilities
                case TransferReason.IncomingMail:       //outside connections outgoing(active), incoming(passive) from postsortingfacilities
                case TransferReason.OutgoingMail:       //outside connections incoming(passive)
                    return CustomTransferManager.OFFER_MATCHMODE.BALANCED;

                // Services which should be outgoing first, but also benefit from incoming match-making (vehicles in the field with capacity to spare)
                case TransferReason.Dead:               //Dead: outgoing offer (passive)
                case TransferReason.Garbage:            //Garbage: outgoing offer (passive) from buldings with garbage to be collected, incoming (active) from landfills    
                case TransferReason.Collapsed:          //Collapsed: outgoing (passive) from buildings
                case TransferReason.Collapsed2:         //Collapsed2: helicopter
                case TransferReason.Snow:               //outgoing (passive) from netsegements, incoming (active) from snowdumps
                case TransferReason.RoadMaintenance:    //incoming (passive) from netsegments, outgoing (active) from maintenance depot
                case TransferReason.ParkMaintenance:    //incoming (passive) from park main gate building, 
                case TransferReason.CriminalMove:       //outging (passive) from policestations, incoming(active) from prisons (REVERSED ACTIVE/PASSIVE COMPARED TO OTHER MOVE TRANSFERS!)
                case TransferReason.GarbageTransfer:    //GarbageTransfer: outgoing (passive) from landfills/wtf, incoming (active) from wasteprocessingcomplex
                case TransferReason.GarbageMove:        //GarbageMove: outgoing (active) from emptying landfills, incoming (passive) from receiving landfills/wastetransferfacilities/wasteprocessingcomplex
                case TransferReason.DeadMove:           //outgoing (active) from emptying, incoming (passive) from receiving
                case TransferReason.SnowMove:           //outgoing (active) from emptying snowdumps, incoming (passive) from receiving
                    return CustomTransferManager.OFFER_MATCHMODE.BALANCED;

                case TransferReason.Crime:              //Crime: outgoing offer (passive) 
                case TransferReason.ForestFire:         //like Fire2
                case TransferReason.Fire2:              //Fire2: helicopter
                case TransferReason.Fire:               //Fire: outgoing offer (passive) - always prio7
                case TransferReason.Sick:               //Sick: outgoing offer (passive) [special case: citizen with outgoing and active]
                case TransferReason.Sick2:              //Sick2: helicopter
                case TransferReason.SickMove:           //outgoing(active) from medcopter, incoming(passive) from hospitals -> moved to outgoing first so closest clinic is used
                    return CustomTransferManager.OFFER_MATCHMODE.OUTGOING_FIRST;

                case TransferReason.Taxi:               //outgoing(active) from depots/taxis, incoming(passive) from citizens and taxistands
                    return CustomTransferManager.OFFER_MATCHMODE.INCOMING_FIRST;

                default:
                    return CustomTransferManager.OFFER_MATCHMODE.BALANCED;
            }
        }

        public static CustomTransferManager.DistanceMode GetMatchDistanceMode(TransferReason material)
        {
            //incoming first: pick highest priority outgoing offers by distance
            //outgoing first: try to fulfill all outgoing offers by descending priority. incoming offer mapped by distance only (priority not relevant).
            //balanced: outgoing/incoming together by priorty descending
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Food:
                case TransferReason.Grain:
                case TransferReason.Lumber:
                case TransferReason.Logs:
                case TransferReason.Goods:
                case TransferReason.LuxuryProducts:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Metals:
                case TransferReason.Glass:
                case TransferReason.PlanedTimber:
                case TransferReason.Paper:
                case TransferReason.Fish:
                case TransferReason.Mail:               //outgoing (passive) from buidings, incoming(active) from postoffice
                case TransferReason.SortedMail:         //outside connections outgoing(active), incoming(passive) from postoffice
                case TransferReason.UnsortedMail:       //outgoing(active) from ???, incoming(passive) from postsortingfacilities
                case TransferReason.IncomingMail:       //outside connections outgoing(active), incoming(passive) from postsortingfacilities
                case TransferReason.OutgoingMail:       //outside connections incoming(passive)
                case TransferReason.Dead:               //Dead: outgoing offer (passive)
                case TransferReason.Garbage:            //Garbage: outgoing offer (passive) from buldings with garbage to be collected, incoming (active) from landfills    
                case TransferReason.Collapsed:          //Collapsed: outgoing (passive) from buildings
                case TransferReason.Collapsed2:         //Collapsed2: helicopter
                case TransferReason.Snow:               //outgoing (passive) from netsegements, incoming (active) from snowdumps
                case TransferReason.RoadMaintenance:    //incoming (passive) from netsegments, outgoing (active) from maintenance depot
                case TransferReason.ParkMaintenance:    //incoming (passive) from park main gate building, 
                case TransferReason.CriminalMove:       //outging (passive) from policestations, incoming(active) from prisons (REVERSED ACTIVE/PASSIVE COMPARED TO OTHER MOVE TRANSFERS!)
                case TransferReason.GarbageTransfer:    //GarbageTransfer: outgoing (passive) from landfills/wtf, incoming (active) from wasteprocessingcomplex
                case TransferReason.GarbageMove:        //GarbageMove: outgoing (active) from emptying landfills, incoming (passive) from receiving landfills/wastetransferfacilities/wasteprocessingcomplex
                case TransferReason.DeadMove:           //outgoing (active) from emptying, incoming (passive) from receiving
                case TransferReason.SnowMove:           //outgoing (active) from emptying snowdumps, incoming (passive) from receiving
                case TransferReason.Crime:              //Crime: outgoing offer (passive) 
                case TransferReason.ForestFire:         //like Fire2
                case TransferReason.Fire2:              //Fire2: helicopter
                case TransferReason.Fire:               //Fire: outgoing offer (passive) - always prio7
                case TransferReason.Sick:               //Sick: outgoing offer (passive) [special case: citizen with outgoing and active]
                case TransferReason.Sick2:              //Sick2: helicopter
                case TransferReason.SickMove:           //outgoing(active) from medcopter, incoming(passive) from hospitals -> moved to outgoing first so closest clinic is used
                    return CustomTransferManager.DistanceMode.Distance;

                default:
                    return CustomTransferManager.DistanceMode.PriorityDistance;
            }
        }

        // Services subject to global prefer local services:
        public static bool IsGlobalPreferLocalSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Dead:
                    return true;

                // Goods subject to prefer local:
                // -none- it is too powerful, city will fall apart

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        // Services subject to building prefer local services:
        public static bool IsBuildingPreferLocalSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Dead:
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:
                case TransferReason.Goods:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Lumber:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Paper:
                case TransferReason.PlanedTimber:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Glass:
                case TransferReason.Metals:
                case TransferReason.LuxuryProducts:
                case TransferReason.Fish:
                    return true;

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        public static float GetDistanceMultiplier(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                    return 5E-07f;
                case TransferReason.Crime:
                    return 1E-05f;
                case TransferReason.Sick:
                    return 1E-06f;
                case TransferReason.Dead:
                    return 1E-05f;
                case TransferReason.Worker0:
                    return 1E-07f;
                case TransferReason.Worker1:
                    return 1E-07f;
                case TransferReason.Worker2:
                    return 1E-07f;
                case TransferReason.Worker3:
                    return 1E-07f;
                case TransferReason.Student1:
                    return 2E-07f;
                case TransferReason.Student2:
                    return 2E-07f;
                case TransferReason.Student3:
                    return 2E-07f;
                case TransferReason.Fire:
                    return 1E-05f;
                case TransferReason.Bus:
                    return 1E-05f;
                case TransferReason.BiofuelBus:
                    return 1E-05f;
                case TransferReason.Oil:
                    return 1E-07f;
                case TransferReason.Ore:
                    return 1E-07f;
                case TransferReason.Logs:
                    return 1E-07f;
                case TransferReason.Grain:
                    return 1E-07f;
                case TransferReason.Goods:
                    return 1E-07f;
                case TransferReason.PassengerTrain:
                    return 1E-05f;
                case TransferReason.Coal:
                    return 1E-07f;
                case TransferReason.Family0:
                    return 1E-08f;
                case TransferReason.Family1:
                    return 1E-08f;
                case TransferReason.Family2:
                    return 1E-08f;
                case TransferReason.Family3:
                    return 1E-08f;
                case TransferReason.Single0:
                    return 1E-08f;
                case TransferReason.Single1:
                    return 1E-08f;
                case TransferReason.Single2:
                    return 1E-08f;
                case TransferReason.Single3:
                    return 1E-08f;
                case TransferReason.PartnerYoung:
                    return 1E-08f;
                case TransferReason.PartnerAdult:
                    return 1E-08f;
                case TransferReason.Shopping:
                    return 2E-07f;
                case TransferReason.Petrol:
                    return 1E-07f;
                case TransferReason.Food:
                    return 1E-07f;
                case TransferReason.LeaveCity0:
                    return 1E-08f;
                case TransferReason.LeaveCity1:
                    return 1E-08f;
                case TransferReason.LeaveCity2:
                    return 1E-08f;
                case TransferReason.Entertainment:
                    return 2E-07f;
                case TransferReason.Lumber:
                    return 1E-07f;
                case TransferReason.GarbageMove:
                    return 5E-07f;
                case TransferReason.MetroTrain:
                    return 1E-05f;
                case TransferReason.PassengerPlane:
                    return 1E-05f;
                case TransferReason.PassengerShip:
                    return 1E-05f;
                case TransferReason.DeadMove:
                    return 5E-07f;
                case TransferReason.DummyCar:
                    return -1E-08f;
                case TransferReason.DummyTrain:
                    return -1E-08f;
                case TransferReason.DummyShip:
                    return -1E-08f;
                case TransferReason.DummyPlane:
                    return -1E-08f;
                case TransferReason.Single0B:
                    return 1E-08f;
                case TransferReason.Single1B:
                    return 1E-08f;
                case TransferReason.Single2B:
                    return 1E-08f;
                case TransferReason.Single3B:
                    return 1E-08f;
                case TransferReason.ShoppingB:
                    return 2E-07f;
                case TransferReason.ShoppingC:
                    return 2E-07f;
                case TransferReason.ShoppingD:
                    return 2E-07f;
                case TransferReason.ShoppingE:
                    return 2E-07f;
                case TransferReason.ShoppingF:
                    return 2E-07f;
                case TransferReason.ShoppingG:
                    return 2E-07f;
                case TransferReason.ShoppingH:
                    return 2E-07f;
                case TransferReason.EntertainmentB:
                    return 2E-07f;
                case TransferReason.EntertainmentC:
                    return 2E-07f;
                case TransferReason.EntertainmentD:
                    return 2E-07f;
                case TransferReason.Taxi:
                    return 1E-05f;
                case TransferReason.CriminalMove:
                    return 5E-07f;
                case TransferReason.Tram:
                    return 1E-05f;
                case TransferReason.Snow:
                    return 5E-07f;
                case TransferReason.SnowMove:
                    return 5E-07f;
                case TransferReason.RoadMaintenance:
                    return 5E-07f;
                case TransferReason.SickMove:
                    return 1E-07f;
                case TransferReason.ForestFire:
                    return 1E-05f;
                case TransferReason.Collapsed:
                    return 1E-05f;
                case TransferReason.Collapsed2:
                    return 1E-05f;
                case TransferReason.Fire2:
                    return 1E-05f;
                case TransferReason.Sick2:
                    return 1E-06f;
                case TransferReason.FloodWater:
                    return 5E-07f;
                case TransferReason.EvacuateA:
                    return 1E-05f;
                case TransferReason.EvacuateB:
                    return 1E-05f;
                case TransferReason.EvacuateC:
                    return 1E-05f;
                case TransferReason.EvacuateD:
                    return 1E-05f;
                case TransferReason.EvacuateVipA:
                    return 1E-05f;
                case TransferReason.EvacuateVipB:
                    return 1E-05f;
                case TransferReason.EvacuateVipC:
                    return 1E-05f;
                case TransferReason.EvacuateVipD:
                    return 1E-05f;
                case TransferReason.Ferry:
                    return 1E-05f;
                case TransferReason.CableCar:
                    return 1E-05f;
                case TransferReason.Blimp:
                    return 1E-05f;
                case TransferReason.Monorail:
                    return 1E-05f;
                case TransferReason.TouristBus:
                    return 1E-05f;
                case TransferReason.ParkMaintenance:
                    return 5E-07f;
                case TransferReason.TouristA:
                    return 2E-07f;
                case TransferReason.TouristB:
                    return 2E-07f;
                case TransferReason.TouristC:
                    return 2E-07f;
                case TransferReason.TouristD:
                    return 2E-07f;
                case TransferReason.Mail:
                    return 1E-05f;
                case TransferReason.UnsortedMail:
                    return 5E-07f;
                case TransferReason.SortedMail:
                    return 5E-07f;
                case TransferReason.OutgoingMail:
                    return 5E-07f;
                case TransferReason.IncomingMail:
                    return 5E-07f;
                case TransferReason.AnimalProducts:
                    return 1E-07f;
                case TransferReason.Flours:
                    return 1E-07f;
                case TransferReason.Paper:
                    return 1E-07f;
                case TransferReason.PlanedTimber:
                    return 1E-07f;
                case TransferReason.Petroleum:
                    return 1E-07f;
                case TransferReason.Plastics:
                    return 1E-07f;
                case TransferReason.Glass:
                    return 1E-07f;
                case TransferReason.Metals:
                    return 1E-07f;
                case TransferReason.LuxuryProducts:
                    return 1E-07f;
                case TransferReason.GarbageTransfer:
                    return 5E-07f;
                case TransferReason.PassengerHelicopter:
                    return 1E-05f;
                case TransferReason.Trolleybus:
                    return 1E-05f;
                case TransferReason.Fish:
                    return 1E-05f;
                case TransferReason.ElderCare:
                    return 1E-06f;
                case TransferReason.ChildCare:
                    return 1E-06f;
                default:
                    return 1E-07f;
            }
        }

    }
}
