using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TransferManagerCE.CustomManager.Stats
{
    public class CycleJobDataStorage
    {
        private CycleJobData m_emptyData = new CycleJobData();
        private readonly object m_lock = new object();
        private Stopwatch m_stopwatch = Stopwatch.StartNew();

        private int m_completedIndex = 0;
        private int m_currentIndex = 0;

        private List<CycleJobData> m_container = new List<CycleJobData>();

        // ----------------------------------------------------------------------------------------
        public CycleJobDataStorage()
        {
            m_container.Add(m_emptyData);
        }

        public CycleJobData GetLatestCompletedCopy()
        {
            lock (m_lock)
            {
                if (m_completedIndex != 0)
                {
                    return new CycleJobData(m_container[m_completedIndex]);
                }
                return m_emptyData;
            }
        }

        public void CycleStarted(int cycle)
        {
            lock (m_lock)
            {
                int iTempIndex = 0;
                if (m_currentIndex != 0) 
                {
                    iTempIndex = m_currentIndex;

                    // Let last data know its cycle is finished
                    CycleJobData data = m_container[m_currentIndex];
                    data.CycleCompleted(m_stopwatch.ElapsedTicks);
                }

                // Add new cycle data to front of list
                int iNewIndex = GetNewCycleData();
                if (iNewIndex != 0) 
                {
                    CycleJobData data = m_container[iNewIndex];
                    data.m_bCreated = true;
                    data.m_cycle = cycle;
                    data.m_cycleStartTicks = m_stopwatch.ElapsedTicks;

                    // Update pointers
                    data.m_prevIndex = iTempIndex;
                    m_currentIndex = iNewIndex;
                }

                UpdateCompleted();
            }
        }

        public void UpdateCompleted()
        {
            lock (m_lock)
            {
                int index = m_currentIndex;
                while (index != 0)
                {
                    CycleJobData data = m_container[index];
                    if (data.IsCompleted())
                    {
                        // We have a new completed cycle to diplay
                        m_completedIndex = index;

                        // Unlink any earlier data as we dont care about it any more
                        int iPrevIndex = data.m_prevIndex;
                        data.m_prevIndex = 0;
                        Unlink(iPrevIndex);

                        return;
                    }

                    index = data.m_prevIndex;
                }
            }
        }

        public CycleJobData? GetCycleData(int cycle)
        {
            int index = m_currentIndex;
            while (index != 0)
            {
                CycleJobData data = m_container[index];
                if (data.m_bCreated && data.m_cycle == cycle)
                {
                    return data;
                }

                index = data.m_prevIndex;
            }
            return null;
        }

        public void JobStarted(int cycle)
        {
            lock (m_lock)
            {
                CycleJobData data = GetCycleData(cycle);
                if (data != null)
                {
                    // Increment job started count
                    data.m_jobsStarted++;
                }
            }
        }

        public void UpdateStats(int cycle, CustomTransferReason.Reason material, long jobMatchTimeTicks)
        {
            lock (m_lock)
            {
                CycleJobData data = GetCycleData(cycle);
                if (data != null)
                {
                    // Increment job started count
                    data.Update(material, jobMatchTimeTicks);
                }
            }
        }

        public int GetNewCycleData()
        {
            lock (m_lock)
            {
                // Find exisitng empty object
                for (int i = 1; i < m_container.Count; ++i)
                {
                    CycleJobData? data = m_container[i];
                    if (!data.m_bCreated)
                    {
                        return i;
                    }
                }

                // No empty object found make new one
                m_container.Add(new CycleJobData());
                return m_container.Count - 1;
            }
        }

        public void Unlink(int unlinkIndex)
        {
            int index = unlinkIndex;
            while (index != 0)
            {
                CycleJobData data = m_container[index];
                
                // Get next index
                index = data.m_prevIndex;

                // Now clear usage of object so it can be reused
                data.Clear();
            }
        }
    }
}
