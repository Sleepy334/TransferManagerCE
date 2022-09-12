using System;
using System.Threading;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Util;

namespace TransferManagerCE.CustomManager
{
    public class TransferManagerThread
    {
        public static volatile bool s_runThreads = true;
        public static int ThreadCount = 8;
        public static Thread[]? s_transferThreads = null;
        public static EventWaitHandle s_waitHandle = new AutoResetEvent(false); // AutoResetEvent releases 1 thread only each time Set() is called.
        
        // Max running threads
        private static int s_runningThreads = 0;
        private static int s_maxRunningThreads = 0;

        public static int RunningThreads()
        {
            return s_runningThreads;
        }

        public static int MaxRunningThreads()
        {
            return s_maxRunningThreads;
        }

        public static void StartThreads()
        {
            if (s_transferThreads == null)
            {
                // Reset run flag
                s_runThreads = true;

                // Create new threads
                ThreadCount = Math.Max(2, Environment.ProcessorCount - 1);
                s_transferThreads = new Thread[ThreadCount];

                // Create TransferManager background thread and start
                for (int i = 0; i < ThreadCount; i++)
                {
                    s_transferThreads[i] = new Thread(new TransferManagerThread().MatchOfferThread);
                    s_transferThreads[i].IsBackground = true;
                    s_transferThreads[i].Start();
                }
            }
        }

        public static void StopThreads()
        {
            s_runThreads = false;

            // Release all threads so they can finish gracefully.
            for (int i = 0; i < ThreadCount; i++)
            {
                s_waitHandle.Set();
            }

            s_transferThreads = null;
        }

        public TransferManagerThread()
        {
        }

        /// <summary>
        /// Thread loop: dequeue job from workqueue and perform offer matching
        /// </summary>
        public void MatchOfferThread()
        {
#if DEBUG
            Debug.Log($"MatchOffersThread: Thread started.");
#endif
            CustomTransferManager manager = new CustomTransferManager();
            while (s_runThreads)
            {
                // Block threads till a job arrives
                s_waitHandle.WaitOne();

                // Increment running thread count
                s_maxRunningThreads = Math.Max(Interlocked.Increment(ref s_runningThreads), s_maxRunningThreads);

                // Dequeue work job
                manager.job = TransferJobQueue.Instance.DequeueWork();
                if (manager.job != null)
                {
                    // match offers in job
                    manager.MatchOffers(manager.job.material);

                    // return to jobpool
                    TransferJobPool.Instance.Return(manager.job);
                    manager.job = null;
                }

                // Thread will now block again
                Interlocked.Decrement(ref s_runningThreads);
            }
#if DEBUG
            Debug.Log($"MatchOffersThread: Thread ended.");
#endif
        }
    }
}