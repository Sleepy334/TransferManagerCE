using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TransferManagerCE.CustomManager;
using UnityEngine;
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
                                Debug.Log($"CALL AGAIN: Existing transfer offer {TransferManagerUtils.DebugOffer(offer)} DETECTED");
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
            FieldInfo incomingCountField = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo outgoingCountField = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            
            TransferOffer[] incomingOffers = incomingField.GetValue(manager) as TransferOffer[];
            TransferOffer[] outgoingOffers = outgoingField.GetValue(manager) as TransferOffer[];
            ushort[] incomingCount = (ushort[])incomingCountField.GetValue(manager);
            ushort[] outgoingCount = (ushort[])outgoingCountField.GetValue(manager);

            // Find offers to this building.
            if (incomingOffers != null && outgoingOffers != null)
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
                        for (int offerIndex = 0; offerIndex < outgoingCount[offer_offset]; offerIndex++)
                        {
                            TransferOffer offer = outgoingOffers[offer_offset * 256 + offerIndex];
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
                        for (int offerIndex = 0; offerIndex < incomingCount[offer_offset]; offerIndex++)
                        {
                            TransferOffer offer = incomingOffers[offer_offset * 256 + offerIndex];
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

        public static string DebugMatch(TransferReason material, CustomTransferOffer outgoing, CustomTransferOffer incoming)
        {
            string sMessage = "\r\n";
            sMessage += "Material: " + material;
            sMessage += " Distance:" + Math.Sqrt(Vector3.SqrMagnitude(incoming.Position - outgoing.Position)) * 0.001;
            sMessage += " Outside:" + outgoing.IsOutside() + " | " + incoming.IsOutside();
            sMessage += "\r\nOutgoing: " + DebugOffer(outgoing);
            sMessage += "\r\nIncoming: " + DebugOffer(incoming);
            return sMessage;
        }

        public static string DebugOffer(CustomTransferOffer offer)
        {
            string sMessage = DebugOffer(offer.m_offer);
            sMessage += " IsOutside: " + offer.IsOutside();
            sMessage += " IsWarehouse: " + offer.IsWarehouse();
            if (offer.IsWarehouse())
            {
                sMessage += " WarehouseMode: " + offer.GetWarehouseMode();
                sMessage += " Storage: " + Math.Round(offer.GetWarehouseStoragePercent(), 2);
            }
            return sMessage;
        }

        public static string DebugOffer(TransferOffer offer)
        {
            string sMessage = InstanceHelper.DescribeInstance(offer.m_object);
            sMessage += " Priority:" + offer.Priority;
            if (offer.Active)
            {
                sMessage += " Active";
            }
            else
            {
                sMessage += " Passive";
            }
            sMessage += " Exclude:" + offer.Exclude;
            sMessage += " Amount: " + offer.Amount;
            return sMessage;
        }
    }
}

