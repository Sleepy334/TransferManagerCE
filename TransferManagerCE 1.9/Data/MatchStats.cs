using ColossalFramework;
using System;
using System.Reflection;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchStats
    {
        public const int iSTATS_ARRAY_SIZE = TRANSFER_REASON_COUNT + 1;
        public const int iMATERIAL_TOTAL_LOCATION = TRANSFER_REASON_COUNT;

        public static StatsContainer[]? s_Stats = null;

        public static int s_lastMatchCount = 0;
        public static long s_lastUpdateTicks = DateTime.Now.Ticks;

        public static void Init()
        {
            if (s_Stats == null)
            {
                s_Stats = new StatsContainer[iSTATS_ARRAY_SIZE];
                s_lastMatchCount = 0;
                s_lastUpdateTicks = DateTime.Now.Ticks;
            }

            if (s_Stats != null)
            {
                for (int i = 0; i < s_Stats.Length; i++)
                {
                    s_Stats[i] = new StatsContainer((TransferReason)i);
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
            if (incomingAmount != null && outgoingAmount != null && incomingCount != null && outgoingCount != null)
            {
                for (int material = 0; material < TRANSFER_REASON_COUNT; material++)
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
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats != null)
            {
                double distance = Math.Sqrt(Vector3.SqrMagnitude(incomingOffer.Position - outgoingOffer.Position));
                bool bOutside = BuildingTypeHelper.IsOutsideConnection(TransferManagerUtils.GetOfferBuilding(outgoingOffer)) || 
                                BuildingTypeHelper.IsOutsideConnection(TransferManagerUtils.GetOfferBuilding(incomingOffer));
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

        public static float GetMatchesPerSecond()
        {
            float fMatchesPerSecond = 0.0f;
            
            if (s_Stats != null)
            {
                float fSeconds = (DateTime.Now.Ticks - s_lastUpdateTicks) / TimeSpan.TicksPerSecond;
                if (fSeconds != 0.0)
                {
                    int iMatchesSincelastUpdate = s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches - s_lastMatchCount;
                    fMatchesPerSecond = iMatchesSincelastUpdate / fSeconds;
                }

                // Update counters
                s_lastMatchCount = s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches;
                s_lastUpdateTicks = DateTime.Now.Ticks;
            }

            return fMatchesPerSecond;
        }

        public static int GetTotalMatches()
        {
            if (s_Stats != null)
            {
                return s_Stats[iMATERIAL_TOTAL_LOCATION].TotalMatches;
            }
            return 0;
        }

        public static string GetAverageDistance()
        {
            if (s_Stats == null || GetTotalMatches() == 0)
            {
                return "0";
            }
            else
            {
                return ((s_Stats[iMATERIAL_TOTAL_LOCATION].TotalDistance / (double)GetTotalMatches()) * 0.001).ToString("0.00");
            }
        }

        public static void RecordAddIncoming(TransferReason material, TransferOffer offer)
        {
            // Update the stats for the specific material
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats != null)
            {
                if ((int)material < s_Stats.Length)
                {
                    s_Stats[(int)material].TotalIncomingCount++;
                    s_Stats[(int)material].TotalIncomingAmount += offer.Amount;
                }

                // Update the stats
                if (iMATERIAL_TOTAL_LOCATION < s_Stats.Length)
                {
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingCount++;
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingAmount += offer.Amount;
                }
            }
        }

        public static void RecordAddOutgoing(TransferReason material, TransferOffer offer)
        {
            // Update the stats for the specific material
            if (ModSettings.GetSettings().StatisticsEnabled && s_Stats != null)
            {
                if ((int)material < s_Stats.Length)
                {
                    s_Stats[(int)material].TotalOutgoingCount++;
                    s_Stats[(int)material].TotalOutgoingAmount += offer.Amount;
                }

                // Update the stats
                if (iMATERIAL_TOTAL_LOCATION < s_Stats.Length)
                {
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingCount++;
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingAmount += offer.Amount;
                }
            }
        }
    }
}
