using TransferManagerCE.CustomManager;
using UnityEngine;

namespace TransferManagerCE
{
    public class CycleJobData
    {
        public int m_prevIndex = 0;

        public bool m_bCreated = false;
        public int m_cycle = 0;
        public int m_jobsStarted = 0;
        public int m_jobsCompleted = 0;
        public long m_totalTicks = 0;
        public long m_cycleStartTicks = 0;
        public long m_cycleEndTicks = 0;

        // Longest match job
        public CustomTransferReason.Reason m_material = CustomTransferReason.Reason.None;
        public long m_ticks = 0;

        // --------------------------------------------------------------------------
        public CycleJobData()
        {
        }

        public CycleJobData(CycleJobData oSecond)
        {
            m_bCreated = oSecond.m_bCreated;
            m_cycle = oSecond.m_cycle;
            m_ticks = oSecond.m_ticks;
            m_jobsStarted = oSecond.m_jobsStarted;
            m_jobsCompleted = oSecond.m_jobsCompleted;
            m_cycleStartTicks = oSecond.m_cycleStartTicks;
            m_cycleEndTicks = oSecond.m_cycleEndTicks;

            m_material = oSecond.m_material;
            m_totalTicks = oSecond.m_totalTicks;
        }

        public void Clear()
        {
            m_bCreated = false;
            m_cycle = 0;
            m_ticks = 0;
            m_material = CustomTransferReason.Reason.None;
            m_jobsStarted = 0;
            m_jobsCompleted = 0;
            m_totalTicks = 0;
            m_cycleStartTicks = 0;
            m_cycleEndTicks = 0;
            m_prevIndex = 0;
        }

        public bool IsCompleted()
        {
            return m_cycleEndTicks > 0 && m_jobsCompleted >= m_jobsStarted;
        }

        public void Update(CustomTransferReason.Reason material, long jobMatchTimeTicks)
        {
            // Update cycle data
            m_jobsCompleted++;
            m_totalTicks += jobMatchTimeTicks;

            if (jobMatchTimeTicks > m_ticks)
            {
                m_ticks = jobMatchTimeTicks;
                m_material = material;
            }
        }

        public void CycleCompleted(long stopTicks)
        {
            m_cycleEndTicks = stopTicks;
        }

        public long DurationTicks()
        {
            return m_cycleEndTicks - m_cycleStartTicks;
        }
    }
}
