using System;
using static TransferManager;
using static TransferManagerCE.CustomManager.PathDistanceTypes;

namespace TransferManagerCE.CustomManager.Stats
{
    public class TransferManagerStats
    {
        private static CycleJobDataStorage s_cycleData = new CycleJobDataStorage();

        // Stats
        public static int s_TotalMatchJobs = 0;
        public static long s_TotalMatchTimeTicks = 0;

        // Path distance specific stats
        public static int s_TotalPathDistanceMatchJobs = 0;
        public static long s_TotalPathDistanceMatchTimeTicks = 0;

        // Longest match time
        public static long s_longestMatchTicks = 0;
        public static CustomTransferReason.Reason s_longestMaterial = CustomTransferReason.Reason.None;

        // Largest match
        public static int s_largestIncoming = 0;
        public static int s_largestOutgoing = 0;
        public static CustomTransferReason.Reason s_largestMaterial = CustomTransferReason.Reason.None;

        // Invalid objects
        public static int s_iInvalidCitizenObjects = 0;
        public static int s_iInvalidVehicleObjects = 0;
        public static int s_iInvalidBuildingObjects = 0;

        // Stats lock so we can update values safely.
        private static readonly object s_lock = new object();

        public static void Init()
        {
            lock (s_lock)
            {
                s_TotalMatchJobs = 0;
                s_TotalMatchTimeTicks = 0;

                s_TotalPathDistanceMatchJobs = 0;
                s_TotalPathDistanceMatchTimeTicks = 0;

                s_longestMatchTicks = 0;
                s_longestMaterial = CustomTransferReason.Reason.None;

                s_largestIncoming = 0;
                s_largestOutgoing = 0;
                s_largestMaterial = CustomTransferReason.Reason.None;

                // Invalid objects
                s_iInvalidCitizenObjects = 0;
                s_iInvalidVehicleObjects = 0;
                s_iInvalidBuildingObjects = 0;
            }
        }

        public static CycleJobDataStorage CycleData
        {
            get
            {
                return s_cycleData;
            }
        }

        public static void JobStarted(int cycle) 
        {
            CycleData.JobStarted(cycle);
        }

        public static void UpdateStats(int cycle, CustomTransferReason.Reason material, PathDistanceAlgorithm algorithm, long jobMatchTimeTicks)
        {
            CycleData.UpdateStats(cycle, material, jobMatchTimeTicks);

            lock (s_lock)
            {
                // Record longest match time for stats.
                if (jobMatchTimeTicks > s_longestMatchTicks)
                {
                    s_longestMatchTicks = jobMatchTimeTicks;
                    s_longestMaterial = material;
                }

                // Record longest path distance match time for stats.
                if (algorithm == PathDistanceAlgorithm.PathDistance)
                {
                    s_TotalPathDistanceMatchJobs++;
                    s_TotalPathDistanceMatchTimeTicks += jobMatchTimeTicks;
                }

                // Update totals
                s_TotalMatchJobs++;
                s_TotalMatchTimeTicks += jobMatchTimeTicks;

                // Update material stats
                MatchStats.RecordJob((TransferReason) material, jobMatchTimeTicks);
            }
        }

        public static void UpdateLargestMatch(TransferJob job)
        {
            lock (s_lock)
            {
                if (Math.Min(job.m_incomingCount, job.m_outgoingCount) > Math.Min(s_largestIncoming, s_largestOutgoing))
                {
                    s_largestIncoming = job.m_incomingCount;
                    s_largestOutgoing = job.m_outgoingCount;
                    s_largestMaterial = job.material;
                }
            }
        }

        public static double GetAverageMatchTime()
        {
            if (s_TotalMatchJobs > 0)
            {
                return s_TotalMatchTimeTicks * 0.0001 / s_TotalMatchJobs;
            }
            return 0.0f;
        }

        public static double GetAveragePathDistanceMatchTime()
        {
            if (s_TotalPathDistanceMatchJobs > 0)
            {
                return s_TotalPathDistanceMatchTimeTicks * 0.0001 / s_TotalPathDistanceMatchJobs;
            }
            return 0.0f;
        }

        public static int GetTotalInvalidObjectCount()
        {
            return  s_iInvalidBuildingObjects + 
                    s_iInvalidVehicleObjects + 
                    s_iInvalidCitizenObjects;
        }
    }
}
