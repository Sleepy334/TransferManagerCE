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
            sMessage += " Building: " + offer.GetBuilding();
            sMessage += " IsOutside: " + offer.IsOutside();
            sMessage += " IsWarehouse: " + offer.IsWarehouse();
            if (offer.IsWarehouse())
            {
                sMessage += " WarehouseMode: " + offer.GetWarehouseMode();
                sMessage += " IncomingStorage: " + Math.Round(offer.GetWarehouseIncomingStoragePercent(), 2);
                sMessage += " OutgoingStorage: " + Math.Round(offer.GetWarehouseOutgoingStoragePercent(), 2);
            }
            sMessage += $" District:{offer.m_preferLocal}";
            sMessage += $" Node:{offer.m_nearestNode}";
            return sMessage;
        }

        public static string DebugOffer(TransferOffer offer)
        {
            string sMessage = InstanceHelper.DescribeInstance(offer.m_object) + "[" + offer.m_object.Type + ":" + offer.m_object.Index + "]";
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
            sMessage += $" Park: {offer.m_isLocalPark}";
            return sMessage;
        }
    }
}

