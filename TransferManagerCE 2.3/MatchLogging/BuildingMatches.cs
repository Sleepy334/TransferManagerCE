﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransferManagerCE.UI;

namespace TransferManagerCE
{
    public class BuildingMatches
    {
        List<BuildingMatchData>? m_Matches = null;
        static readonly object s_MatchLock = new object();
        private ushort m_buildingId = 0;
        private ushort m_subBuildingId = 0;

        public BuildingMatches()
        {

        }

        public void SetBuildingId(ushort buildingId, ushort subBuildingId)
        {
            if (m_buildingId != buildingId)
            {
                m_buildingId = buildingId;
                InvalidateMatches();
            }
            if (m_subBuildingId != subBuildingId)
            {
                m_subBuildingId = subBuildingId;
                InvalidateMatches();
            }
        }

        public List<BuildingMatchData>? GetSortedBuildingMatches()
        {
            List<BuildingMatchData>? list = null;

            if (m_buildingId != 0)
            {
                lock (s_MatchLock)
                {
                    if (m_Matches is null)
                    {
                        m_Matches = MatchLogging.Instance.GetMatchesForBuilding();
                        if (m_Matches is not null)
                        {
                            m_Matches.Sort();
                        }
                    }

                    // Return a copy so we don't get threading issues
                    list = new List<BuildingMatchData>(m_Matches);
                }
            }          

            return list;
        }

        public void InvalidateMatches()
        {
            lock (s_MatchLock)
            {
                m_Matches = null;
            }
        }

        public void AddMatch(ushort buildingId, BuildingMatchData match)
        {
            lock (s_MatchLock)
            {
                if (m_Matches is not null && buildingId != 0 && (buildingId == m_buildingId || buildingId == m_subBuildingId))
                {
                    m_Matches.Insert(0, match);

                    // Request an update
                    if (BuildingPanel.Instance is not null)
                    {
                        BuildingPanel.Instance.InvalidatePanel();
                    }
                }
            }
        }

        // Called from MatchLoggingThread so ensure it is thread safe.
        public void AddMatches(ushort buildingId, List<BuildingMatchData> matches)
        {
            lock (s_MatchLock)
            {
                if (m_Matches is not null && buildingId != 0 && (buildingId == m_buildingId || buildingId == m_subBuildingId))
                {
                    m_Matches.AddRange(matches);
                    m_Matches.Sort();

                    // Request an update
                    if (BuildingPanel.Instance is not null)
                    {
                        BuildingPanel.Instance.InvalidatePanel();
                    }
                }
            }
        }
    }
}
