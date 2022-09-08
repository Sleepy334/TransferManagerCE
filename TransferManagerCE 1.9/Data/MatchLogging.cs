using System;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchLogging
    {
        const int iMATCH_LOGGING_LIMIT = 80000;

        private static MatchLogging? s_instance = null;
        static readonly object s_MatchLock = new object();

        private Queue<MatchData>? m_Matches = null;

        public static MatchLogging Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new MatchLogging();
                }
                return s_instance;
            }
        }

        public MatchLogging()
        {
            m_Matches = new Queue<MatchData>(iMATCH_LOGGING_LIMIT);
        }

        public void StartTransfer(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {
            if (m_Matches != null)
            {
                MatchData match = new MatchData(material, outgoingOffer, incomingOffer, deltaamount);
                lock (s_MatchLock)
                {
                    if (match.m_incoming.GetBuilding() != 0 || match.m_outgoing.GetBuilding() != 0)
                    {
                        while (m_Matches.Count >= iMATCH_LOGGING_LIMIT - 1)
                        {
                            m_Matches.Dequeue();
                        }

                        m_Matches.Enqueue(match);
                    }
                }
            }
        }

        public List<MatchData> GetMatchesForBuilding(ushort buildingId)
        {
            List<MatchData> matches = new List<MatchData>();
            if (m_Matches != null)
            {
                lock (s_MatchLock)
                {
                    foreach (MatchData matchData in m_Matches)
                    {
                        // Check just the Building first
                        if (matchData.m_incoming.Building == buildingId)
                        {
                            matches.Add(new MatchData(buildingId, true, matchData));
                        }
                        else if (matchData.m_outgoing.Building == buildingId)
                        {
                            matches.Add(new MatchData(buildingId, false, matchData));
                        }
                        else if (matchData.m_incoming.GetBuilding() == buildingId)
                        {
                            matches.Add(new MatchData(buildingId, true, matchData));
                        }
                        else if (matchData.m_outgoing.GetBuilding() == buildingId)
                        {
                            matches.Add(new MatchData(buildingId, false, matchData));
                        }
                    }
                }
            }
            return matches;
        }
    }
}