using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public class PathingContainer : IComparable
    {
        private static int s_iPathingId = 1;

        private int m_iPathingId;
        public long m_time;
        public InstanceID m_source;
        public InstanceID m_target;

        public PathingContainer(long time, InstanceID source, InstanceID target)
        {
            m_iPathingId = s_iPathingId++;
            m_time = time;
            m_source = source;
            m_target = target;
        }

        public PathingContainer(PathingContainer oSecond) 
        {
            m_iPathingId = s_iPathingId++;
            m_time = oSecond.m_time;
            m_source = oSecond.m_source;
            m_target = oSecond.m_target;
        }

        public int CompareTo(object second)
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
            return Math.Max(PathFindFailure.GetTotalPathFailures(m_source), PathFindFailure.GetTotalPathFailures(m_target));
        }

        public long GetSeconds()
        {
            return (DateTime.Now.Ticks - m_time) / (TimeSpan.TicksPerMillisecond * 1000);
        }
        
    }
}