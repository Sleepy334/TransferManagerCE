using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    internal class TransferManagerModes
    {
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

        public static Color GetTransferReasonColor(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.GarbageMove:
                case TransferReason.GarbageTransfer:
                case TransferReason.Crime:
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
