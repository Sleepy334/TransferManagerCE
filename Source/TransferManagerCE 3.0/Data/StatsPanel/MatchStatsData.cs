using System;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchStatsData : IComparable
    {
        public int TotalMatches = 0;
        public int TotalMatchAmount = 0;
        public int TotalIncomingCount = 0;
        public int TotalIncomingAmount = 0;
        public int TotalOutgoingCount = 0;
        public int TotalOutgoingAmount = 0;
        public double TotalDistance = 0;
        public int TotalOutside = 0;

        public long JobTimeLastTicks = 0;
        public long JobTimeMaxTicks = 0;
        public long JobTimeTotalTicks = 0;
        public long JobCount = 0;

        public CustomTransferReason m_material = TransferReason.None;

        public MatchStatsData()
        {
        }

        public MatchStatsData(TransferReason material)
        {
            m_material = material;
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }
            MatchStatsData oSecond = (MatchStatsData)second;

            // Sort by average job time
            if (GetJobAverageTicks() != oSecond.GetJobAverageTicks()) 
            {
                return oSecond.GetJobAverageTicks().CompareTo(GetJobAverageTicks());
            }

            // Sort by number of matches
            if (oSecond.TotalMatchAmount != TotalMatchAmount)
            {
                return oSecond.TotalMatchAmount.CompareTo(TotalMatchAmount);
            }

            // Sort by number of matches
            if (oSecond.TotalMatchAmount != TotalMatchAmount) 
            {
                return oSecond.TotalMatchAmount.CompareTo(TotalMatchAmount);
            }

            return m_material.ToString().CompareTo(oSecond.m_material.ToString());
        }

        public void UpdateJobStats(long jobTicks)
        {
            JobCount++;
            JobTimeLastTicks = jobTicks;
            JobTimeMaxTicks = (long)Mathf.Max(JobTimeMaxTicks, jobTicks);
            JobTimeTotalTicks += jobTicks;
        }

        public long GetJobAverageTicks()
        {
            if (JobCount > 0)
            {
                return JobTimeTotalTicks / JobCount;
            }

            return 0;
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

        public virtual bool IsSeparator()
        {
            return false;
        }

        public virtual string GetMaterialDescription()
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

        public Color GetTextColor()
        {
            if (TotalMatches == 0)
            {
                return KnownColor.orange;
            }
            else
            {
                return KnownColor.white;
            }
        }
    }
}