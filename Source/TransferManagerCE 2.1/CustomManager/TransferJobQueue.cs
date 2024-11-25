using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;

namespace TransferManagerCE
{
    public class TransferJobQueue
    {
        const int iINITIAL_QUEUE_SIZE = 32;

        // Static mambers
        private static TransferJobQueue? s_instance = null;

        // Members
        private Queue<TransferJob>? m_workQueue = null;
        private readonly object m_workQueueLock = new object();
        private int m_iMaxJobCount = 0;

        public static TransferJobQueue Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new TransferJobQueue();
                }
                return s_instance;
            }
        }

        public TransferJobQueue()
        {
            m_workQueue = new Queue<TransferJob>(iINITIAL_QUEUE_SIZE);
            m_iMaxJobCount = 0;
        }

        public void Destroy()
        {
            if (m_workQueue != null)
            {
                m_workQueue.Clear();
                m_workQueue = null;
                m_iMaxJobCount = 0;
            }

            s_instance = null;
        }

        public int Count()
        {
            if (m_workQueue != null)
            {
                return m_workQueue.Count;
            }
            return 0;
        }

        public int GetMaxUsageCount()
        {
            return m_iMaxJobCount;
        }

        /// <summary>
        /// Thread-safe Enqueue
        /// </summary>
        /// <param name="job"></param>
        public void EnqueueWork(TransferJob job)
        {
            if (m_workQueue != null)
            {
                lock (m_workQueueLock)
                {
                    m_workQueue.Enqueue(job);
                    m_iMaxJobCount = Math.Max(m_iMaxJobCount, m_workQueue.Count);
                }
                // Release one thread to handle this job.
                TransferManagerThread.ReleaseOneThread(); 
            }
        }

        /// <summary>
        /// Thread-safe Dequeue
        /// </summary>
        /// <returns></returns>
        public TransferJob? DequeueWork()
        {
            TransferJob? job = null;

            if (m_workQueue != null)
            {
                int iCount = 0;
                lock (m_workQueueLock)
                {
                    iCount = m_workQueue.Count;
                    if (iCount > 0)
                    {
                        job = m_workQueue.Dequeue();
                    }
                }

                if (iCount > 1)
                {
                    // Release another thread as we still have jobs waiting
                    TransferManagerThread.ReleaseOneThread();
                }
            }
            
            return job;
        }
    }
}