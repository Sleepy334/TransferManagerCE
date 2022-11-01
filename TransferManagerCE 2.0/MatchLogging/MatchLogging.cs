using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchLogging
    {
        // Doesn't need to be so big now we have the file buffer
        const int iMATCH_LOGGING_LIMIT = 10000; 

        private static MatchLogging? s_instance = null;

        // Stores all matches made by the Transfer Manager
        static readonly object s_MatchLock = new object();
        private Queue<MatchData>? m_MatchQueue = null;

        private ushort m_buildingId = 0;

        public static MatchLogging Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new MatchLogging();
                    MatchLoggingThread.StartThread();
                }
                return s_instance;
            }
        }

        public MatchLogging()
        {
            m_MatchQueue = new Queue<MatchData>(iMATCH_LOGGING_LIMIT);
        }

        public void SetBuildingId(ushort buildingId)
        {
            m_buildingId = buildingId;
        }

        public void StartTransfer(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer)
        {
            if (m_MatchQueue != null)
            {
                MatchData match = new MatchData(material, outgoingOffer, incomingOffer);
                List<ushort> incomingBuildings = match.m_incoming.GetBuildings();
                List<ushort> outgoingBuildings = match.m_outgoing.GetBuildings();

                if (incomingBuildings.Count > 0 || outgoingBuildings.Count > 0)
                {
                    lock (s_MatchLock)
                    {
                        while (m_MatchQueue.Count >= iMATCH_LOGGING_LIMIT - 1)
                        {
                            MatchData data = m_MatchQueue.Dequeue();

                            // Write old matches to file
                            MatchLoggingThread.AddMatchToBuffer(data);
                        }

                        m_MatchQueue.Enqueue(match);
                    }
                }

                // Add to current building matches if open
                if (m_buildingId != 0 && 
                    (incomingBuildings.Contains(m_buildingId) || outgoingBuildings.Contains(m_buildingId))) 
                {
                    if (BuildingPanel.Instance != null && BuildingPanel.Instance.isVisible)
                    {
                        BuildingMatchData? buildingMatch = GetBuildingMatch(m_buildingId, match);
                        if (buildingMatch != null)
                        {
                            BuildingPanel.Instance.GetBuildingMatches().AddMatch(m_buildingId, buildingMatch);
                        }
                    }
                }
            }
        }

        public static BuildingMatchData? GetBuildingMatch(ushort buildingId, MatchData match)
        {
            // Check just the Building first
            if (match.m_incoming.Building == buildingId)
            {
                return new BuildingMatchData(true, match);
            }
            else if (match.m_outgoing.Building == buildingId)
            {
                return new BuildingMatchData(false, match);
            }
            else if (match.m_incoming.GetBuildings().Contains(buildingId))
            {
                return new BuildingMatchData(true, match);
            }
            else if (match.m_outgoing.GetBuildings().Contains(buildingId))
            {
                return new BuildingMatchData(false, match);
            }

            return null;
        }

        public List<BuildingMatchData> GetMatchesForBuilding()
        {
            // Load matches from buffer
            List<BuildingMatchData> matches = new List<BuildingMatchData>();

            if (m_buildingId != 0)
            {
                // Request archived matches for this building
                MatchLoggingThread.RequestMatchesForBuilding(m_buildingId);

                if (m_MatchQueue != null)
                {
                    lock (s_MatchLock)
                    {
                        foreach (MatchData matchData in m_MatchQueue)
                        {
                            BuildingMatchData? buildingMatch = GetBuildingMatch(m_buildingId, matchData);
                            if (buildingMatch != null)
                            {
                                matches.Add(buildingMatch);
                            }
                        }
                    }
                }
            }
            
            return matches;
        }
    }
}