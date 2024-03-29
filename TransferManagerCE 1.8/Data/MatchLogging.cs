using System;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchLogging
    {
        const int iMATCH_LOGGING_LIMIT = 80000;

        public static MatchLogging? instance = null;

        public Queue<MatchData> m_Matches = null;
        static readonly object s_MatchLock = new object();

        public MatchLogging()
        {
            m_Matches = new Queue<MatchData>(iMATCH_LOGGING_LIMIT);
        }

        public static void Init()
        {
            instance = new MatchLogging();
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
                        if (matchData.m_incoming.GetBuilding() == buildingId ||
                            matchData.m_outgoing.GetBuilding() == buildingId)
                        {
                            matches.Add(new MatchData(buildingId, matchData));
                        }
                    }
                }
            }
            return matches;
        }
    }
}