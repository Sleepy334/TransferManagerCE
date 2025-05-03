using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class OfferData : IComparable
    {
        public CustomTransferReason m_material = TransferReason.None;
        public TransferOffer m_offer;
        public bool m_bIncoming;
        public int m_iPrioirty;
        public bool m_bActive;
        public byte m_byLocalPark;

        public OfferData(TransferReason material, bool bIncoming, TransferOffer offer)
        {
            m_material = material;
            m_bIncoming = bIncoming;
            m_offer = offer;
            m_iPrioirty = offer.Priority;
            m_bActive = offer.Active;
            m_byLocalPark = offer.m_isLocalPark;
        }

        public static int CompareTo(OfferData first, OfferData second)
        {
            // Descending priority
            if (second.m_iPrioirty != first.m_iPrioirty)
            {
                return second.m_iPrioirty - first.m_iPrioirty;
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

        public string DisplayOffer(bool bShowId = false)
        {
            string sMessage = "";

            if (m_offer.m_object.Type != InstanceType.Building)
            {
                sMessage = InstanceHelper.DescribeInstance(m_offer.m_object, bShowId);
            }

            // Don't add the building if it's a park as we just show all offers for the park at each ServicePoint
            if (m_offer.m_object.Type != InstanceType.Park)
            {
                List<ushort> buildings = InstanceHelper.GetBuildings(m_offer.m_object);
                if (buildings.Count > 0)
                {
                    if (sMessage.Length > 0)
                    {
                        sMessage += " @ ";
                    }
                    if (bShowId)
                    {
                        sMessage += $"#{buildings[0]}:";
                    }
                    sMessage += CitiesUtils.GetBuildingName(buildings[0]);
                }
            }

            return sMessage;
        }

        public void Show()
        {
            InstanceHelper.ShowInstance(m_offer.m_object);
        }

        public string ToolTip()
        {
            return $"Object: {DisplayOffer(true)}\nMaterial: {m_material}\nDirection: {(m_bIncoming ? "In" : "Out")}\nPriority: {m_iPrioirty}\nAmount: {m_offer.Amount}\nActive: {(m_bActive ? "Yes" : "No")}\nPark: {m_byLocalPark}";
        }
    }
}

