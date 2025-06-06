using SleepyCommon;
using System;
using System.Text;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class OfferData : MatchOffer, IComparable
    {
        public CustomTransferReason m_material = TransferReason.None;
        public bool m_bIncoming;

        public OfferData(TransferReason material, bool bIncoming, TransferOffer offer) : 
            base(offer)
        {
            m_material = material;
            m_bIncoming = bIncoming;
        }

        public static int CompareTo(OfferData first, OfferData second)
        {
            // Descending priority
            if (second.Priority != first.Priority)
            {
                return second.Priority - first.Priority;
            }

            return (CustomTransferReason.Reason)second.m_material - (CustomTransferReason.Reason)first.m_material;
        }

        public int CompareTo(object second)
        {
            if (second is null) {
                return 1;
            }
            OfferData oSecond = (OfferData)second;
            return CompareTo(this, oSecond);
        }

        public string DescribeInOut()
        {
            if (m_bIncoming)
            {
                return "IN";
            }
            else
            {
                return "OUT";
            }
        }

        public void Show()
        {
            InstanceHelper.ShowInstance(m_object);
        }

        public string GetToolTipText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"Material: {m_material}\n");

            if (m_byLocalPark != 0)
            {
                stringBuilder.Append($"Park: {DescribePark()}\n");
            }

            stringBuilder.Append($"Object: {DescribeOfferObject(true)}\n");
            stringBuilder.Append($"Priorty: {Priority}\n");
            stringBuilder.Append($"Amount: {DescribeAmount()}\n");
            stringBuilder.Append($"Active: {DescribeActive()}\n");

            return stringBuilder.ToString();
        }
    }
}

