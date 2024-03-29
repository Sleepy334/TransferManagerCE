﻿using System;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class TransferManagerStats
    {
        // Stats
        public static int s_TotalMatchJobs = 0;
        public static long s_TotalMatchTimeMS = 0;

        // Path distance specific stats
        public static int s_TotalPathDistanceMatchJobs = 0;
        public static long s_TotalPathDistanceMatchTimeMS = 0;

        // Longest match time
        public static long s_longestMatch = 0;
        public static TransferReason s_longestMaterial = TransferReason.None;

        // Largest match
        public static int s_largestIncoming = 0;
        public static int s_largestOutgoing = 0;
        public static TransferReason s_largestMaterial = TransferReason.None;

        public static void Init()
        {
            s_TotalMatchJobs = 0;
            s_TotalMatchTimeMS = 0;
            s_TotalPathDistanceMatchJobs = 0;
            s_TotalPathDistanceMatchTimeMS = 0;
            s_longestMatch = 0;
            s_longestMaterial = TransferReason.None;
            s_largestIncoming = 0;
            s_largestOutgoing = 0;
            s_largestMaterial = TransferReason.None;
        }

        public static float GetAverageMatchTime()
        {
            if (s_TotalMatchJobs > 0)
            {
                return (float)s_TotalMatchTimeMS / (float)s_TotalMatchJobs;
            }
            return 0.0f;
        }

        public static float GetAveragePathDistanceMatchTime()
        {
            if (s_TotalPathDistanceMatchJobs > 0)
            {
                return (float)s_TotalPathDistanceMatchTimeMS / (float)s_TotalPathDistanceMatchJobs;
            }
            return 0.0f;
        }

        public static void UpdateLargestMatch(TransferJob job)
        {
            if (Math.Min(job.m_incomingCount, job.m_outgoingCount) > Math.Min(s_largestIncoming, s_largestOutgoing))
            {
                s_largestIncoming = job.m_incomingCount;
                s_largestOutgoing = job.m_outgoingCount;
                s_largestMaterial = job.material;
            }
        }
    }
}
