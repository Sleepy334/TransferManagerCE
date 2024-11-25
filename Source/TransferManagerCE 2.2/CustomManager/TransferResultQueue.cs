using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferResultQueue
    {
        /// <summary>
        /// TransferResult: individual work package for StartTransfers
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TransferResult
        {
            public TransferReason material;
            public TransferOffer outgoingOffer;
            public TransferOffer incomingOffer;
            public int deltaamount;
        }

        const int iINITIAL_QUEUE_SIZE = 2000;

        // members
        private readonly object m_MatchLock = new object(); // match lock so it is thread safe
        private Queue<TransferResult>? m_Matches = null;
        public int m_iMaxQueueDepth = 0;

        public int GetMaxUsageCount()
        {
            return m_iMaxQueueDepth;
        }

        public int GetCount()
        {
            if (m_Matches != null)
            {
                return m_Matches.Count;
            }
            return 0;
        }

        public TransferResultQueue()
        {
            m_Matches = new Queue<TransferResult>(iINITIAL_QUEUE_SIZE);
        }

        public void EnqueueTransferResult(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {            
            if (m_Matches != null)
            {
                TransferResult result = new TransferResult();
                result.material = material;
                result.outgoingOffer = outgoingOffer;
                result.incomingOffer = incomingOffer;
                result.deltaamount = deltaamount;

                lock (m_MatchLock)
                {
                    m_Matches.Enqueue(result);

                    // Update max queue depth stat.
                    m_iMaxQueueDepth = Math.Max(m_Matches.Count, m_iMaxQueueDepth);
                }
            }
        }

        public void StartTransfers()
        {
            if (m_Matches != null)
            {
                Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Citizen[] Citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;
                DistrictPark[] Parks = Singleton<DistrictManager>.instance.m_parks.m_buffer;

                while (m_Matches.Count > 0)
                {
                    TransferResult oResult;
                    lock (m_MatchLock)
                    {
                        oResult = m_Matches.Dequeue();
                    }

                    if (oResult.material != TransferReason.None)
                    {
                        // Handle this match
                        MatchHandler.Match(oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);

                        // Start transfer
                        TransferHandler.StartTransfer(Buildings, Vehicles, Citizens, Parks, oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);
                    }
                }
            }
        }
    }
}