using System;
using System.Collections.Generic;
using TransferManagerCE.Util;

namespace TransferManagerCE.CustomManager
{
    /// <summary>
    /// TransferJobPool: pool of TransferJobs
    /// </summary>
    public sealed class TransferJobPool
    {
        public const int iTRANSFER_JOB_QUEUE_INITIAL_SIZE = 32;

        // Static members
        private static TransferJobPool? s_instance = null;
        
        // Members
        private int m_usageCount = 0;
        private int m_maxUsageCount = 0;
        private Stack<TransferJob>? m_pooledJobs = null;
        private readonly object m_poolLock = new object();

        public static TransferJobPool Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new TransferJobPool();
                }
                return s_instance;
            }
        }

        public int GetMaxUsageCount() => m_maxUsageCount;

        public void Initialize()
        {
            if (m_pooledJobs == null)
            {
                // allocate object pool of work packages
                m_pooledJobs = new Stack<TransferJob>(iTRANSFER_JOB_QUEUE_INITIAL_SIZE);

                for (int i = 0; i < iTRANSFER_JOB_QUEUE_INITIAL_SIZE; i++)
                {
                    m_pooledJobs.Push(new TransferJob());
                }
            }
        }

        public void Delete()
        {
            if (m_pooledJobs != null)
            {
                // unallocate object pool of work packages
                m_pooledJobs.Clear();
                m_pooledJobs = null;
                m_usageCount = 0;
                m_maxUsageCount = 0;
            }

            s_instance = null;
        }

        public TransferJob? Lease()
        {
            TransferJob? job = null;
            
            if (m_pooledJobs != null)
            {
                lock (m_poolLock)
                {
                    if (m_pooledJobs.Count > 0)
                    {
                        job = m_pooledJobs.Pop();
                    }
                    else
                    {
                        // We have run out of stored transfer jobs, create a new one.
                        job = new TransferJob();
                    }

                    // Update usage stats
                    m_usageCount++;
                    m_maxUsageCount = Math.Max(m_usageCount, m_maxUsageCount);
                }
            }

            return job;
        }

        public void Return(TransferJob job)
        {
            if (m_pooledJobs != null)
            {
                job.Reset(); //flag as unused

                lock (m_poolLock)
                {
                    m_pooledJobs.Push(job);

                    // Update usage count
                    m_usageCount = Math.Max(0, m_usageCount - 1);
                }
            }
        }

        public int Count()
        {
            return m_usageCount;
        }
    }
}
