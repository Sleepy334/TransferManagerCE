using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatsContainer : IComparable
    {
        public int TotalMatches = 0;
        public int TotalMatchAmount = 0;
        public int TotalIncomingCount = 0;
        public int TotalIncomingAmount = 0;
        public int TotalOutgoingCount = 0;
        public int TotalOutgoingAmount = 0;
        public double TotalDistance = 0;
        public int TotalOutside = 0;
        public CustomTransferReason m_material;

        public StatsContainer()
        {
            TotalMatches = 0;
            TotalMatchAmount = 0;
            TotalIncomingCount = 0;
            TotalIncomingAmount = 0;
            TotalOutgoingCount = 0;
            TotalOutgoingAmount = 0;
            TotalDistance = 0;
            TotalOutside = 0;
            m_material = TransferReason.None;
        }

        public StatsContainer(TransferReason material)
        {
            TotalMatches = 0;
            TotalMatchAmount = 0;
            TotalIncomingCount = 0;
            TotalIncomingAmount = 0;
            TotalOutgoingCount = 0;
            TotalOutgoingAmount = 0;
            TotalDistance = 0;
            TotalOutside = 0;
            m_material = material;
        }

        public int CompareTo(object second)
        {
            return 1;
        }

        public float SafeDiv(float fNumerator, float fDenominator)
        {
            if (fNumerator == 0 || fDenominator == 0)
            {
                return 0f;
            }
            else
            {
                return fNumerator / fDenominator;
            }
        }

        public string GetMaterialDescription()
        {
            if (m_material == TransferReason.None)
            {
                return "Total";
            }
            else
            {
                return m_material.ToString();
            }
        }

        public string GetAverageDistance()
        {
            if (TotalMatches == 0)
            {
                return "0";
            }
            else
            {
                return ((TotalDistance / (double)TotalMatches) * 0.001).ToString("0.00");
            }
        }
    }
}