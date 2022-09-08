using System;
using System.Runtime.CompilerServices;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    internal class TransferManagerModes
    {
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
                case TransferReason.RoadMaintenance:
                case TransferReason.Snow:
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

        // Services subject to building prefer local services:
        public static bool IsServiceReason(TransferReason material)
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

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }
    }
}
