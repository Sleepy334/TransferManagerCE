using SleepyCommon;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    public class StatsBase : IComparable
    {
        public string m_description;
        public string m_value;
        protected KnownColor m_color = KnownColor.white;

        public StatsBase(string sDescription, string sValue)
        {
            m_description = sDescription;
            m_value = sValue;
        }

        public virtual bool IsHeader()
        {
            return false;
        }

        public int CompareTo(object second)
        {
            return 1;
        }

        public Color GetColor()
        {
            return m_color.color;
        }
    }
}