using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class RoadAccessData : IComparable
    {
        public InstanceID m_source;
        public int m_iCount;
        public string? m_description = null;

        public RoadAccessData(InstanceID source, int iCount)
        {
            m_source = source;
            m_iCount = iCount;
        }

        public RoadAccessData(RoadAccessData oSecond)
        {
            m_source = oSecond.m_source;
            m_iCount = oSecond.m_iCount;
        }

        public string GetDescription()
        {
            if (m_description is null)
            {
                m_description = InstanceHelper.DescribeInstance(m_source);
            }
            return m_description;
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            RoadAccessData oSecond = (RoadAccessData)second;
            if (oSecond.m_iCount == m_iCount)
            {
                return GetDescription().CompareTo(oSecond.GetDescription());
            }
            else
            {
                return oSecond.m_iCount.CompareTo(m_iCount);
            }
        }
    }
}