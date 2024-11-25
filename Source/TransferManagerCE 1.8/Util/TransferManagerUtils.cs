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
                    // Loop through outgoing for this material
                    int material_offset = material * 8;
                    int offer_offset;

                    for (int priority = 7; priority >= 0; --priority)
                    {
                        offer_offset = material_offset + priority;
                        for (int offerIndex = 0; offerIndex < outgoingCount[offer_offset]; offerIndex++)
                        {
                            TransferOffer offer = outgoingOffers[offer_offset * 256 + offerIndex];
                            if (GetOfferBuilding(offer) == buildingId)
                            {
                                offers.Add(new OfferData((TransferReason)material, false, offer));
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
                            if (GetOfferBuilding(offer) == buildingId)
                            {
                                offers.Add(new OfferData((TransferReason)material, true, offer));
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
            string sMessage = "";
            if (offer.Building > 0 && offer.Building < BuildingManager.instance.m_buildings.m_size)
            {
                var instB = default(InstanceID);
                instB.Building = offer.Building;
                sMessage += " (" + offer.Building + ")" + BuildingManager.instance.m_buildings.m_buffer[offer.Building].Info?.name + "(" + InstanceManager.instance.GetName(instB) + ")";
            }
            if (offer.Vehicle > 0 && offer.Vehicle < VehicleManager.instance.m_vehicles.m_size)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[offer.Vehicle];
                sMessage += " (" + offer.Vehicle + ")" + vehicle.Info?.name;

                // Add building
                if (vehicle.m_sourceBuilding != 0)
                {
                    var instB = new InstanceID { Building = vehicle.m_sourceBuilding };
                    sMessage += "@(" + offer.Building + ")" + BuildingManager.instance.m_buildings.m_buffer[offer.Building].Info?.name + "(" + InstanceManager.instance.GetName(instB) + ")";
                }
            }
            if (offer.Citizen > 0)
            {
                sMessage += $" Citizen:{offer.Citizen}";
                Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                sMessage += $" Building:{oCitizen.GetBuildingByLocation()}";
            }
            if (offer.NetSegment > 0)
            {
                sMessage += $" NetSegment={offer.NetSegment}";
            }
            if (offer.TransportLine > 0)
            {
                sMessage += $" TransportLine={offer.TransportLine}";
            }
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

        public static bool IsOfferForBuilding(ushort buildingId, ref TransferOffer offer)
        {
            if (offer.m_object.Type == InstanceType.Building)
            {
                return offer.Building == buildingId;
            }
            else if (offer.m_object.Type == InstanceType.Vehicle)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[offer.Vehicle];
                return vehicle.m_sourceBuilding == buildingId;
            }
            else if (offer.m_object.Type == InstanceType.Citizen)
            {
                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                if (citizen.m_homeBuilding == buildingId)
                {
                    return true;
                }
                else if (citizen.m_workBuilding == buildingId)
                {
                    return true;
                }
                else if(citizen.m_visitBuilding == buildingId)
                {
                    return true;
                }
            }

            return false;
        }

        public static ushort GetOfferBuilding(TransferOffer offer)
        {
            switch (offer.m_object.Type)
            {
                case InstanceType.Building:
                    {
                        return offer.Building;
                    }
                case InstanceType.Vehicle:
                    {
                        Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[offer.Vehicle];
                        return vehicle.m_sourceBuilding;
                    }
                case InstanceType.Citizen:
                    {
                        Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                        return citizen.GetBuildingByLocation();
                    }
            }   

            return 0;
        }
    }
}

