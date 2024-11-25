using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TransferManagerCE.UI;
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
        private List<ushort> m_buildingIds = new List<ushort>();

        public static MatchLogging Instance
        {
            get
            {
                if (s_instance is null)
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

        public void SetBuildingId(ushort buildingId, List<ushort> subBuildingIds)
        {
            m_buildingIds.Clear();
            m_buildingIds.Add(buildingId); // Parent building
            m_buildingIds.AddRange(subBuildingIds); // Any sub buildings
        }

        public void StartTransfer(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer)
        {
            if (m_MatchQueue is not null)
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

                    // Add to current building matches if open
                    if (BuildingPanel.Instance is not null && BuildingPanel.Instance.isVisible)
                    {
                        if (m_buildingIds.Count > 0)
                        {
                            foreach (ushort buildingId in m_buildingIds)
                            {
                                if (incomingBuildings.Contains(buildingId) || outgoingBuildings.Contains(buildingId))
                                {
                                    BuildingMatchData? buildingMatch = GetBuildingMatch(buildingId, match);
                                    if (buildingMatch is not null)
                                    {
                                        BuildingPanel.Instance.GetBuildingMatches().AddMatch(buildingId, buildingMatch);
                                    }
                                }
                            }
                        } 
                    }  
                }
            }
        }

        public static BuildingMatchData? GetBuildingMatch(ushort buildingId, MatchData match)
        {
            if (match.m_incoming.m_buildings.Contains(buildingId))
            {
                return new BuildingMatchData(true, match);
            }
            else if (match.m_outgoing.m_buildings.Contains(buildingId))
            {
                return new BuildingMatchData(false, match);
            }

            return null;
        }

        public List<BuildingMatchData> GetMatchesForBuilding()
        {
            // Load matches from buffer
            List<BuildingMatchData> matches = new List<BuildingMatchData>();

            if (m_buildingIds.Count > 0)
            {
                // Request archived matches for this building
                MatchLoggingThread.RequestMatchesForBuildings(m_buildingIds);

                if (m_MatchQueue is not null)
                {
                    lock (s_MatchLock)
                    {
                        foreach (MatchData matchData in m_MatchQueue)
                        {
                            if (m_buildingIds.Count > 0)
                            {
                                foreach (ushort buildingId in m_buildingIds)
                                {
                                    BuildingMatchData? buildingMatch = GetBuildingMatch(buildingId, matchData);
                                    if (buildingMatch is not null)
                                    {
                                        matches.Add(buildingMatch);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return matches;
        }
    }
}