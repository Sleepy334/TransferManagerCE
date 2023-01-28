using ColossalFramework;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;

namespace TransferManagerCE
{
    public class BuildingOffers
    {
        TransferOffer[]? m_outgoingOffers = null;
        TransferOffer[]? m_incomingOffers = null;
        ushort[]? m_outgoingCount = null;
        ushort[]? m_incomingCount = null;

        public BuildingOffers()
        {
            TransferManager manager = Singleton<TransferManager>.instance;

            // Reflect transfer offer fields.
            FieldInfo outgoingOfferField = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo outgoingCountField = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo incomingOfferField = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo incomingCountField = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);

            m_outgoingOffers = (TransferOffer[])outgoingOfferField.GetValue(manager);
            m_incomingOffers = (TransferOffer[])incomingOfferField.GetValue(manager);
            m_outgoingCount = (ushort[])outgoingCountField.GetValue(manager);
            m_incomingCount = (ushort[])incomingCountField.GetValue(manager);
        }

        public List<OfferData> GetOffersForBuilding(ushort buildingId)
        {
            List<OfferData> offers = new List<OfferData>();

            // Find offers to this building.
            if (m_incomingOffers != null && m_outgoingOffers != null)
            {
                for (int material = 0; material < TRANSFER_REASON_COUNT; material++)
                {
                    TransferReason reason = (TransferReason)material;

                    // Loop through outgoing for this material
                    int material_offset = material * 8;
                    int offer_offset;

                    for (int priority = 7; priority >= 0; --priority)
                    {
                        offer_offset = material_offset + priority;
                        for (int offerIndex = 0; offerIndex < m_outgoingCount[offer_offset]; offerIndex++)
                        {
                            TransferOffer offer = m_outgoingOffers[offer_offset * 256 + offerIndex];
                            if (InstanceHelper.GetBuildings(offer.m_object).Contains(buildingId))
                            {
                                offers.Add(new OfferData(reason, false, offer));
                            }
                        }
                    }

                    // Loop through incoming for this material
                    for (int priority = 7; priority >= 0; --priority)
                    {
                        offer_offset = material_offset + priority;
                        for (int offerIndex = 0; offerIndex < m_incomingCount[offer_offset]; offerIndex++)
                        {
                            TransferOffer offer = m_incomingOffers[offer_offset * 256 + offerIndex];
                            if (InstanceHelper.GetBuildings(offer.m_object).Contains(buildingId))
                            {
                                offers.Add(new OfferData(reason, true, offer));
                            }
                        }
                    }
                }
            }

            return offers;
        }

        public bool IsIncomingOfferInManager(TransferReason material, TransferOffer offer)
        {
            for (int i = 0; i < 8; i++)
            {
                int num = (int)material * 8 + i;
                int num2 = m_incomingCount[num];
                for (int num3 = num2 - 1; num3 >= 0; num3--)
                {
                    int num4 = num * 256 + num3;
                    if (m_incomingOffers[num4].m_object == offer.m_object && m_incomingOffers[num4].m_isLocalPark == offer.m_isLocalPark)
                    {
                        return true;
                    }
                }

                m_incomingCount[num] = (ushort)num2;
            }

            return false;
        }

        public bool IsOutgoingOfferInManager(TransferReason material, TransferOffer offer)
        {
            for (int i = 0; i < 8; i++)
            {
                int num = (int)material * 8 + i;
                int num2 = m_outgoingCount[num];
                for (int num3 = num2 - 1; num3 >= 0; num3--)
                {
                    int num4 = num * 256 + num3;
                    if (m_outgoingOffers[num4].m_object == offer.m_object && m_outgoingOffers[num4].m_isLocalPark == offer.m_isLocalPark)
                    {
                        return true;
                    }
                }

                m_outgoingCount[num] = (ushort)num2;
            }

            return false;
        }
    }
}