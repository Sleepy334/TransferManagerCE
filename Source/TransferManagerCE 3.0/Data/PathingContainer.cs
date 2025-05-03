using System;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public class PathingContainer : IComparable
    {
        public enum LocationType
        {
            Local,
            Outside,
        }
        public enum SourceOrTarget
        {
            None,
            Source,
            Target,
        }
        private static int s_iPathingId = 1;
        private int m_iPathingId;
        public LocationType m_locationType;
        public long m_time;
        public InstanceID m_source;
        public InstanceID m_target;
        public SourceOrTarget m_eSourceOrTarget;
        public bool m_bSubBuilding;

        public PathingContainer(LocationType locationType, long time, InstanceID source, InstanceID target, SourceOrTarget eSourceOrTarget, bool bSubBuilding)
        {
            // Increase internal counter
            m_iPathingId = s_iPathingId++;

            m_locationType = locationType;
            m_time = time;
            m_source = source;
            m_target = target;
            m_eSourceOrTarget = eSourceOrTarget;
            m_bSubBuilding = bSubBuilding;
        }

        public PathingContainer(PathingContainer oSecond) 
        {
            m_iPathingId = s_iPathingId++;

            m_locationType = oSecond.m_locationType;
            m_time = oSecond.m_time;
            m_source = oSecond.m_source;
            m_target = oSecond.m_target;
            m_eSourceOrTarget = oSecond.m_eSourceOrTarget;
            m_bSubBuilding = oSecond.m_bSubBuilding;
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            PathingContainer oSecond = (PathingContainer)second;

            if (GetMaxPathFailCount() == oSecond.GetMaxPathFailCount())
            {
                return oSecond.m_time.CompareTo(m_time);
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

        public string GetTimeDescription()
        {
            if (m_locationType == LocationType.Local)
            {
                return $"{GetSeconds()} / {PathFindFailure.PATH_TIMEOUT_INTERNAL / (TimeSpan.TicksPerMillisecond * 1000)}";
            }
            else
            {
                return $"{GetSeconds()} / {PathFindFailure.PATH_TIMEOUT_OUTSIDE / (TimeSpan.TicksPerMillisecond * 1000)}";
            }
        }

        public long GetSeconds()
        {
            return (DateTime.Now.Ticks - m_time) / (TimeSpan.TicksPerMillisecond * 1000);
        }
        
    }
}