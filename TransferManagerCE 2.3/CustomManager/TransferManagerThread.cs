using System;
using System.Threading;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    public class TransferManagerThread
    {
        // Static members so we only have one value for all threads
        public static int ThreadCount = 8; 
        private static volatile bool s_runThread = true;
        private static Thread[]? s_transferThreads = null;
        private static EventWaitHandle? s_waitHandle = null;
        
        // Max running threads
        private static int s_runningThreads = 0;
        private static int s_maxRunningThreads = 0;

        public static void StartThreads()
        {
            if (s_transferThreads is null)
            {
                s_runThread = true;

                // AutoResetEvent releases 1 thread only each time Set() is called.
                s_waitHandle = new AutoResetEvent(false);

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
            // Tell threads to quit
            s_runThread = false;

            // Release all threads
            if (s_waitHandle is not null)
            {
                for (int i = 0; i < ThreadCount; ++i)
                {
                    s_waitHandle.Set();
                }
            }
            
            s_transferThreads = null;
            s_waitHandle = null;
            s_runningThreads = 0;
            s_maxRunningThreads = 0;
        }

        public static int RunningThreads()
        {
            return s_runningThreads;
        }

        public static int MaxRunningThreads()
        {
            return s_maxRunningThreads;
        }

        public static void ReleaseOneThread()
        {
            if (s_waitHandle is not null)
            {
                s_waitHandle.Set();
            }
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
            while (s_runThread)
            {
                // Block threads till a job arrives
                if (s_waitHandle is not null)
                {
                    s_waitHandle.WaitOne();
                }
                else
                {
                    throw new Exception("Thread event is null");
                }

                // Increment running thread count
                s_maxRunningThreads = Math.Max(Interlocked.Increment(ref s_runningThreads), s_maxRunningThreads);
                
                // Dequeue work job
                manager.job = TransferJobQueue.Instance.DequeueWork();
                if (manager.job is not null)
                {
                    TransferReason material = manager.job.material;

                    // match offers in job
                    manager.MatchOffers(material);

                    // Update running reason set
                    CustomTransferDispatcher.Instance.RemoveDispatchedReason(material);

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