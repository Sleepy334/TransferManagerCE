using static TransferManager;

namespace TransferManagerCE
{
    public class CustomTransferReason
    {
        public const int iLAST_REASON = 122;

        private TransferReason m_material;

        public CustomTransferReason(TransferReason reason)
        {
            m_material = reason;
        }

        public static implicit operator TransferReason(CustomTransferReason reason)
        {
            return reason.m_material;
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
            if (second == null)
            {
                return 1;
            }
            CustomTransferReason oSecond = (CustomTransferReason)second;
            return oSecond.m_material.CompareTo(m_material);
        }

        // We return more descriptive names for some of the material types.
        public override string ToString()
        {
            switch (m_material)
            {
                case TransferReason.Student1: return "StudentES";
                case TransferReason.Student2: return "StudentHS";
                case TransferReason.Student3: return "StudentUni";
                case TransferReason.Grain: return "Crops";
                case TransferReason.Logs: return "ForestProducts";

                // Prison Helicopter mod
                case (TransferReason) 120: return "PrisonMove1"; // 120 - sends prison vans from big police stations to small police stations to pick up prisoners
                case (TransferReason) 121: return "PrisonMove2"; // 121 - send prison helicopters from the police helicopter depot to pickup prisoners from big police stations
                case (TransferReason) 122: return "PrisonMove3"; // 122 - take prisoners from big police stations to prisons

                default: return m_material.ToString();
            }
        }
    }
}
