using System;
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
        private List<ushort> m_buildingIds = new List<ushort>();

        public BuildingMatches()
        {

        }

        public void SetBuildingIds(List<ushort> buildingIds)
        {
            if (m_buildingIds.Count != buildingIds.Count || !m_buildingIds.SequenceEqual(buildingIds))
            {
                m_buildingIds.Clear();
                m_buildingIds.AddRange(buildingIds);
                InvalidateMatches();
            }
        }

        public void SetBuildingIds(ushort buildingId, List<ushort> subBuildingIds)
        {
            bool bUpdate = false;

            int iCount = subBuildingIds.Count + 1;
            if (m_buildingIds.Count != iCount)
            {
                bUpdate = true;
            }
            else if (!m_buildingIds.Contains(buildingId)) 
            {
                bUpdate = true; 
            }
            else 
            {
                foreach (ushort id in subBuildingIds)
                {
                    if (!m_buildingIds.Contains(id))
                    {
                        bUpdate = true;
                        break;
                    }
                }
            }

            if (bUpdate)
            {
                m_buildingIds.Clear();
                m_buildingIds.Add(buildingId);
                m_buildingIds.AddRange(subBuildingIds);
                InvalidateMatches();
            }
        }

        public List<BuildingMatchData>? GetSortedBuildingMatches()
        {
            List<BuildingMatchData>? list = null;

            if (m_buildingIds.Count > 0)
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
                if (m_Matches is not null && buildingId != 0 && m_buildingIds.Contains(buildingId))
                {
                    m_Matches.Insert(0, match);

                    // Request an update
                    if (BuildingPanel.IsVisible())
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
                if (m_Matches is not null && buildingId != 0 && m_buildingIds.Contains(buildingId))
                {
                    m_Matches.AddRange(matches);
                    m_Matches.Sort();

                    // Request an update
                    if (BuildingPanel.IsVisible() && BuildingPanel.Instance.IsTransferTabActive())
                    {
                        BuildingPanel.Instance.InvalidatePanel();
                    }
                }
            }
        }
    }
}
