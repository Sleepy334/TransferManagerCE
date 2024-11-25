using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TransferManagerCE.CustomManager;
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
            public TransferOffer outgoingOffer; // structure so we get a copy
            public TransferOffer incomingOffer; // structure so we get a copy
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
            if (m_Matches is not null)
            {
                return m_Matches.Count;
            }
            return 0;
        }

        public TransferResultQueue()
        {
            m_Matches = new Queue<TransferResult>(iINITIAL_QUEUE_SIZE);
        }

        public void EnqueueTransferResult(CustomTransferReason.Reason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {            
            if (m_Matches is not null)
            {
                TransferResult result = new TransferResult();
                result.material = (TransferReason) material;
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
            if (m_Matches is not null)
            {
                Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Citizen[] Citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;
                DistrictPark[] Parks = Singleton<DistrictManager>.instance.m_parks.m_buffer;

                // Set this before loop
                const TransferReason TaxiMove = (TransferReason)CustomTransferReason.Reason.TaxiMove;
                const TransferReason Mail2 = (TransferReason)CustomTransferReason.Reason.Mail2;

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

                        // Convert TaxiMove back to Taxi and Mail2 -> Mail so they handle the transfer correctly.
                        switch (oResult.material)
                        {
                            case TaxiMove:
                                {
                                    oResult.material = TransferReason.Taxi;
                                    break;
                                }
                            case Mail2:
                                {
                                    oResult.material = TransferReason.Mail;
                                    break;
                                }
                        }

                        // Start transfer
                        TransferHandler.StartTransfer(Buildings, Vehicles, Citizens, Parks, oResult.material, oResult.outgoingOffer, oResult.incomingOffer, oResult.deltaamount);
                    }
                }
            }
        }
    }
}