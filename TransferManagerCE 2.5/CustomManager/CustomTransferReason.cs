using static TransferManager;

namespace TransferManagerCE
{
    public class CustomTransferReason
    {
        public const int iLAST_REASON = 127;

        public enum Reason
        {
            Garbage = 0,
            Crime = 1,
            Sick = 2,
            Dead = 3,
            Worker0 = 4,
            Worker1 = 5,
            Worker2 = 6,
            Worker3 = 7,
            StudentES = 8, // Student1
            StudentHS = 9, // Student2
            StudentUni = 10, // Student3
            Fire = 11,
            Bus = 12,
            Oil = 13,
            Ore = 14,
            ForestProducts = 0xF, // Logs
            Crops = 0x10,
            Goods = 17,
            PassengerTrain = 18,
            Coal = 19,
            Family0 = 20,
            Family1 = 21,
            Family2 = 22,
            Family3 = 23,
            Single0 = 24,
            Single1 = 25,
            Single2 = 26,
            Single3 = 27,
            PartnerYoung = 28,
            PartnerAdult = 29,
            Shopping = 30,
            Petrol = 0x1F,
            Food = 0x20,
            LeaveCity0 = 33,
            LeaveCity1 = 34,
            LeaveCity2 = 35,
            Entertainment = 36,
            Lumber = 37,
            GarbageMove = 38,
            MetroTrain = 39,
            PassengerPlane = 40,
            PassengerShip = 41,
            DeadMove = 42,
            DummyCar = 43,
            DummyTrain = 44,
            DummyShip = 45,
            DummyPlane = 46,
            Single0B = 47,
            Single1B = 48,
            Single2B = 49,
            Single3B = 50,
            ShoppingB = 51,
            ShoppingC = 52,
            ShoppingD = 53,
            ShoppingE = 54,
            ShoppingF = 55,
            ShoppingG = 56,
            ShoppingH = 57,
            EntertainmentB = 58,
            EntertainmentC = 59,
            EntertainmentD = 60,
            Taxi = 61,
            CriminalMove = 62,
            Tram = 0x3F,
            Snow = 0x40,
            SnowMove = 65,
            RoadMaintenance = 66,
            SickMove = 67,
            ForestFire = 68,
            Collapsed = 69,
            Collapsed2 = 70,
            Fire2 = 71,
            Sick2 = 72,
            FloodWater = 73,
            EvacuateA = 74,
            EvacuateB = 75,
            EvacuateC = 76,
            EvacuateD = 77,
            EvacuateVipA = 78,
            EvacuateVipB = 79,
            EvacuateVipC = 80,
            EvacuateVipD = 81,
            Ferry = 82,
            CableCar = 83,
            Blimp = 84,
            Monorail = 85,
            TouristBus = 86,
            ParkMaintenance = 87,
            TouristA = 88,
            TouristB = 89,
            TouristC = 90,
            TouristD = 91,
            Mail = 92,
            UnsortedMail = 93,
            SortedMail = 94,
            OutgoingMail = 95,
            IncomingMail = 96,
            AnimalProducts = 97,
            Flours = 98,
            Paper = 99,
            PlanedTimber = 100,
            Petroleum = 101,
            Plastics = 102,
            Glass = 103,
            Metals = 104,
            LuxuryProducts = 105,
            GarbageTransfer = 106,
            PassengerHelicopter = 107,
            Fish = 108,
            Trolleybus = 109,
            ElderCare = 110,
            ChildCare = 111,
            IntercityBus = 112,
            BiofuelBus = 113,
            Cash = 114,
            TaxiMove = 115,
            Mail2 = 116,
            BusinessA = 119,
            BusinessB = 120,
            BusinessC = 121,
            BusinessD = 122,
            NatureA = 123,
            NatureB = 124,
            NatureC = 125,
            NatureD = 126,
            Crime2 = 127,
            None = 0xFF
        }

        private Reason m_material;

        public CustomTransferReason(TransferReason reason)
        {
            m_material = (Reason) reason;
        }

        public CustomTransferReason(Reason reason)
        {
            m_material = reason;
        }

        public CustomTransferReason(int reason)
        {
            m_material = (Reason) reason;
        }

        public Reason ToReason()
        {
            return m_material;
        }

        public static implicit operator TransferReason(CustomTransferReason reason)
        {
            return (TransferReason) reason.m_material;
        }

        public static implicit operator CustomTransferReason(TransferReason reason)
        {
            return new CustomTransferReason(reason);
        }

        public static implicit operator CustomTransferReason(byte reason)
        {
            return new CustomTransferReason((TransferReason)reason);
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }
            CustomTransferReason oSecond = (CustomTransferReason)second;
            return oSecond.m_material.CompareTo(m_material);
        }

        // We return more descriptive names for some of the material types.
        public override string ToString()
        {
            return m_material.ToString();
        }
    }
}
