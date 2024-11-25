using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    public class GeneralContainer : IComparable
    {
        public string m_description;
        public string m_value;

        public GeneralContainer(string sDescription, string sValue)
        {
            m_description = sDescription;
            m_value = sValue;
        }

        public int CompareTo(object second)
        {
            return 1;
        }
    }
}