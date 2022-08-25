using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public class PathingContainer : ListData
    {
        private static int s_iPathingId = 1;

        private int m_iPathingId;
        public long m_time;
        public ushort m_sourceBuilding;
        public ushort m_targetBuilding;

        public PathingContainer(long time, ushort sourceBuilding, ushort targetBuilding)
        {
            m_iPathingId = s_iPathingId++;
            m_time = time;
            m_sourceBuilding = sourceBuilding;
            m_targetBuilding = targetBuilding;
        }

        public PathingContainer(PathingContainer oSecond) 
        {
            m_iPathingId = s_iPathingId++;
            m_time = oSecond.m_time;
            m_sourceBuilding = oSecond.m_sourceBuilding;
            m_targetBuilding = oSecond.m_targetBuilding;
        }

        public override int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }

            PathingContainer oSecond = (PathingContainer)second;
            if (GetMaxPathFailCount() == oSecond.GetMaxPathFailCount())
            {
                return m_iPathingId.CompareTo(oSecond.m_iPathingId);
            }
            else
            {
                return oSecond.GetMaxPathFailCount().CompareTo(GetMaxPathFailCount());
            }
        }

        public static int CompareToTime(PathingContainer first, PathingContainer second)
        {
            return second.m_time.CompareTo(first.m_time);
        }

        public int GetMaxPathFailCount()
        {
            return Math.Max(PathFindFailure.GetTotalPathFailures(m_sourceBuilding), PathFindFailure.GetTotalPathFailures(m_targetBuilding));
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_TIME: return GetSeconds().ToString();
                case ListViewRowComparer.Columns.COLUMN_OWNER: return CitiesUtils.GetBuildingName(m_sourceBuilding) + "(" + PathFindFailure.GetTotalPathFailures(m_sourceBuilding) + ")";
                case ListViewRowComparer.Columns.COLUMN_TARGET: return CitiesUtils.GetBuildingName(m_targetBuilding) + "(" + PathFindFailure.GetTotalPathFailures(m_targetBuilding) + ")";
            }
            return "TBD";
        }

        public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, GetText(ListViewRowComparer.Columns.COLUMN_TIME), "", TransferIssuePanel.iCOLUMN_WIDTH_TIME, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, GetText(ListViewRowComparer.Columns.COLUMN_OWNER), "", TransferIssuePanel.iCOLUMN_WIDTH_PATHING_BUILDING, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, GetText(ListViewRowComparer.Columns.COLUMN_TARGET), "", TransferIssuePanel.iCOLUMN_WIDTH_PATHING_BUILDING, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }

        public override void OnClick(ListViewRowColumn column)
        {
            switch (column.GetColumn())
            {
                case ListViewRowComparer.Columns.COLUMN_OWNER: CitiesUtils.ShowBuilding(m_sourceBuilding); break;
                case ListViewRowComparer.Columns.COLUMN_TARGET: CitiesUtils.ShowBuilding(m_targetBuilding); break;
            }
        }

        public long GetSeconds()
        {
            return (DateTime.Now.Ticks - m_time) / (TimeSpan.TicksPerMillisecond * 1000);
        }
        
    }
}