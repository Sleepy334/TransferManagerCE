using System;
using System.Diagnostics;
using System.Reflection;
using ColossalFramework;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchStats
    {
        public static int iSTATS_ARRAY_SIZE;
        public static int iMATERIAL_TOTAL_LOCATION;

        public static MatchStatsData[]? s_Stats = null;

        private static int s_lastMatchCount = 0;
        private static long s_lastUpdateMS = 0;
        private static float s_fMatchesPerSecond = 0;
        private static Stopwatch? s_stopWatch = null;

        public static void Init()
        {
            if (s_Stats is null)
            {
                TransferManager manager = Singleton<TransferManager>.instance;
                ushort[] m_incomingCount = (ushort[])typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);

                if(m_incomingCount != null && m_incomingCount.Length > 0)
                {
                    iSTATS_ARRAY_SIZE = m_incomingCount.Length + 1;
                    iMATERIAL_TOTAL_LOCATION = m_incomingCount.Length;
                }
                else
                {
                    iSTATS_ARRAY_SIZE = TRANSFER_REASON_COUNT + 1;
                    iMATERIAL_TOTAL_LOCATION = TRANSFER_REASON_COUNT;
                }

                s_Stats = new MatchStatsData[iSTATS_ARRAY_SIZE];
                s_lastMatchCount = 0;
                s_stopWatch = Stopwatch.StartNew();
                s_lastUpdateMS = s_stopWatch.ElapsedMilliseconds;
            }

            if (s_Stats is not null)
            {
                if(s_Stats.Length != iSTATS_ARRAY_SIZE)
                {
                    Array.Resize(ref s_Stats, iSTATS_ARRAY_SIZE);
                }

                for (int i = 0; i < s_Stats.Length; i++)
                {
                    s_Stats[i] = new MatchStatsData((TransferReason)i);
                }

                CountExistingTransfers();
            }
        }

        public static void Destroy()
        {
            s_Stats = null;
        }

        private static void CountExistingTransfers()
        {
            TransferManager manager = Singleton<TransferManager>.instance;

            // Reflect transfer offer fields.
            FieldInfo incomingAmountField = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo outgoingAmountField = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo incomingCountField = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo outgoingCountField = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);

            int[] incomingAmount = (int[])incomingAmountField.GetValue(manager);
            int[] outgoingAmount = (int[])outgoingAmountField.GetValue(manager);
            ushort[] incomingCount = (ushort[])incomingCountField.GetValue(manager);
            ushort[] outgoingCount = (ushort[])outgoingCountField.GetValue(manager);

            // Add counts from already existing transfers
            if (incomingAmount is not null && outgoingAmount is not null && incomingCount is not null && outgoingCount is not null)
            {
                iMATERIAL_TOTAL_LOCATION = incomingAmount.Length;

                for (int material = 0; material < iMATERIAL_TOTAL_LOCATION; material++)
                {
                    // Incoming
                    s_Stats[material].TotalIncomingCount += incomingCount[material];
                    s_Stats[material].TotalIncomingAmount += incomingAmount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingCount += incomingCount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingAmount += incomingAmount[material];

                    // Outgoing
                    s_Stats[material].TotalOutgoingCount += outgoingCount[material];
                    s_Stats[material].TotalOutgoingAmount += outgoingAmount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingCount += outgoingCount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingAmount += outgoingAmount[material];
                }
            }
        }

        public static void RecordMatch(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats is not null)
            {
                double distance = Math.Sqrt(Vector3.SqrMagnitude(incomingOffer.Position - outgoingOffer.Position));
                bool bOutside = BuildingTypeHelper.IsOutsideConnection(outgoingOffer.Building) || 
                                BuildingTypeHelper.IsOutsideConnection(incomingOffer.Building);
                s_Stats[(int)material].TotalMatches++;
                s_Stats[(int)material].TotalMatchAmount += deltaamount;
                s_Stats[(int)material].TotalDistance += distance;

                if (bOutside)
                {
                    s_Stats[(int)material].TotalOutside++;
                }

                s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches++;
                s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatchAmount += deltaamount;
                s_Stats[iMATERIAL_TOTAL_LOCATION].TotalDistance += distance;
                if (bOutside)
                {
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutside++;
                }
            }
        }

        public static void RecordJob(TransferReason material, long jobTicks)
        {
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats is not null)
            {
                s_Stats[(int)material].UpdateJobStats(jobTicks);
                s_Stats[iMATERIAL_TOTAL_LOCATION].UpdateJobStats(jobTicks);
            }
        }

        public static float GetMatchesPerSecond()
        {
            if (s_Stats is not null && s_stopWatch is not null)
            {
                long elapsedMilliseconds = s_stopWatch.ElapsedMilliseconds - s_lastUpdateMS;
                if (elapsedMilliseconds >= 5000)
                {
                    int iMatchesSincelastUpdate = s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches - s_lastMatchCount;
                    s_fMatchesPerSecond = (float) iMatchesSincelastUpdate / ((float)elapsedMilliseconds * 0.001f);

                    // Update counters
                    s_lastMatchCount = s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches;
                    s_lastUpdateMS = s_stopWatch.ElapsedMilliseconds;
                } 
            }

            return s_fMatchesPerSecond;
        }

        public static int GetTotalMatches()
        {
            if (s_Stats is not null)
            {
                return s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches;
            }
            return 0;
        }

        public static string GetAverageDistance()
        {
            if (s_Stats is null || GetTotalMatches() == 0)
            {
                return "0";
            }
            else
            {
                return ((s_Stats[iMATERIAL_TOTAL_LOCATION].TotalDistance / (double)GetTotalMatches()) * 0.001).ToString("0.00");
            }
        }

        public static void RecordAddIncoming(TransferReason material, int amount)
        {
            // Update the stats for the specific material
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats is not null)
            {
                if ((int)material < s_Stats.Length)
                {
                    s_Stats[(int)material].TotalIncomingCount++;
                    s_Stats[(int)material].TotalIncomingAmount += amount;
                }

                // Update the stats
                if (iMATERIAL_TOTAL_LOCATION < s_Stats.Length)
                {
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingCount++;
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingAmount += amount;
                }
            }
        }

        public static void RecordAddOutgoing(TransferReason material, int amount)
        {
            // Update the stats for the specific material
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats is not null)
            {
                if ((int)material < s_Stats.Length)
                {
                    s_Stats[(int)material].TotalOutgoingCount++;
                    s_Stats[(int)material].TotalOutgoingAmount += amount;
                }

                // Update the stats
                if (iMATERIAL_TOTAL_LOCATION < s_Stats.Length)
                {
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingCount++;
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingAmount += amount;
                }
            }
        }
    }
}
