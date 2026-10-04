using SleepyCommon;
using System;
using System.Text;
using TransferManagerCore.CustomManager;
using static TransferManager;

namespace TransferManagerCore
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
            return m_bIncoming ? Localization.Get("inout_In") : Localization.Get("inout_Out");
        }

        public override void Show()
        {
            InstanceHelper.ShowInstance(m_object);
        }

        public string GetToolTipText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"{Localization.Get("tip_Material")}: {m_material.GetLocalizedReason()}\n");

            if (m_byLocalPark != 0)
            {
                stringBuilder.Append($"{Localization.Get("tip_Park")}: {DescribePark()}\n");
            }

            stringBuilder.Append($"{Localization.Get("tip_Object")}: {DescribeOfferObject(true)}\n");
            stringBuilder.Append($"{Localization.Get("tip_Priority")}: {Priority}\n");
            stringBuilder.Append($"{Localization.Get("tip_Amount")}: {DescribeAmount()}\n");
            stringBuilder.Append($"{Localization.Get("tip_Active")}: {(Active ? Localization.Get("tip_ActiveValue") + " (Active)" : Localization.Get("tip_PassiveValue") + " (Passive)")}\n");

            return stringBuilder.ToString();
        }
    }
}

