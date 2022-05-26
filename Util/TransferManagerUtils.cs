using ColossalFramework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferManagerUtils
    {   
        public static List<TransferOffer> RemoveExisitingOutgoingOffers(TransferReason material, List<TransferOffer> newOutgoingOffers)
        {
            TransferManager manager = Singleton<TransferManager>.instance;

            // Reflect transfer offer fields.
            FieldInfo outgoingOfferField = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo outgoingCountField = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);

            TransferOffer[] outgoingOffers = (TransferOffer[])outgoingOfferField.GetValue(manager);
            ushort[] outgoingCount = (ushort[])outgoingCountField.GetValue(manager);
            
            List<TransferOffer> existing = new List<TransferOffer>();

            int material_offset = (int)material * 8;
            int offer_offset;
            for (int priority = 7; priority >= 0; --priority)
            {
                offer_offset = material_offset + priority;
                for (int offerIndex = 0; offerIndex < outgoingCount[offer_offset]; offerIndex++)
                {
                    TransferOffer offer = outgoingOffers[offer_offset * 256 + offerIndex];
                    if (offer.Citizen != 0)
                    {
                        // Check against list of new offers
                        foreach (TransferOffer offerSearch in newOutgoingOffers)
                        {
                            // Currently just checking Citizen
                            if (offerSearch.Citizen == offer.Citizen && !existing.Contains(offerSearch))
                            {
#if DEBUG
                                Debug.Log($"CALL AGAIN: Existing transfer offer {CustomTransferManager.DebugInspectOffer2(offer)} DETECTED");
#endif
                                existing.Add(offerSearch);
                            }
                        }
                    }
                }
            }

            return newOutgoingOffers.Except(existing).ToList();
        }

        public static List<OfferData> GetOffersForBuilding(ushort buildingId)
        {
            List<OfferData> offers = new List<OfferData>();

            TransferManager manager = Singleton<TransferManager>.instance;

            // Reflect transfer offer fields.
            FieldInfo incomingField = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo outgoingField = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            TransferOffer[] incomingOffers = incomingField.GetValue(manager) as TransferOffer[];
            TransferOffer[] outgoingOffers = outgoingField.GetValue(manager) as TransferOffer[];

            // Find offers to this building.
            if (incomingOffers != null && outgoingOffers != null)
            {
                for (int i = 0; i < incomingOffers.Length; ++i)
                {
                    // Calculate reason and priority
                    TransferManager.TransferReason material = (TransferManager.TransferReason)((i & 0xFFFFF800) >> 11);
                    int priority = (i & 0x0700) >> 8;

                    // Incoming offers.
                    if (incomingOffers[i].Amount > 0)
                    {
                        if (incomingOffers[i].Building == buildingId)
                        {
                            offers.Add(new OfferData(material, true, incomingOffers[i], priority));
                        }
                        else if (incomingOffers[i].Citizen != 0)
                        {
                            Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[incomingOffers[i].Citizen];
                            if (oCitizen.GetBuildingByLocation() == buildingId)
                            {
                                offers.Add(new OfferData(material, true, incomingOffers[i], priority));
                            }
                        }
                    }


                    // Outgoing offers.
                    if (outgoingOffers[i].Amount > 0)
                    {
                        if (outgoingOffers[i].Building == buildingId)
                        {
                            offers.Add(new OfferData(material, false, outgoingOffers[i], priority));
                        }
                        else if (outgoingOffers[i].Citizen != 0)
                        {
                            Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[outgoingOffers[i].Citizen];
                            if (oCitizen.GetBuildingByLocation() == buildingId)
                            {
                                offers.Add(new OfferData(material, false, outgoingOffers[i], priority));
                            }
                        }
                    }
                }
            }

            return offers;
        }
    }
}

