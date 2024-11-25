using System.Collections.Generic;
using TransferManagerCE.Util;

namespace TransferManagerCE.CustomManager
{
    /// <summary>
    /// TransferJobPool: pool of TransferJobs
    /// </summary>
    public sealed class TransferJobPool
    {
        private static TransferJobPool _instance = null;
        private Stack<TransferJob> pooledJobs = null;
        private static readonly object _poolLock = new object();

        #region STATISTICS
        private int _usageCount = 0;
        private int _maxUsageCount = 0;
        public int GetMaxUsage() => _maxUsageCount;
        #endregion

        public static TransferJobPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TransferJobPool();
                }
                return _instance;
            }
        }

        public void Initialize()
        {
            // allocate object pool of work packages
            pooledJobs = new Stack<TransferJob>(TransferManager.TRANSFER_REASON_COUNT);

            for (int i = 0; i < TransferManager.TRANSFER_REASON_COUNT; i++)
            {
                pooledJobs.Push(new TransferJob());
            }
            
            unsafe
            {
#if DEBUG
                DebugLog.LogOnly(DebugLog.REASON_ALL, $"TransferJobPool initialized, pool stack size is {pooledJobs.Count}");
#endif
            }
        }

        public void Delete()
        {
            Debug.Log($"Deleting instance: {_instance}");
            // unallocate object pool of work packages
            pooledJobs.Clear();
            pooledJobs = null;
            TransferJobPool._instance = null;
        }

        public TransferJob Lease()
        {
            lock (_poolLock)
            {
                if (pooledJobs.Count > 0)
                {
                    _usageCount++;
                    _maxUsageCount = (_usageCount > _maxUsageCount) ? _usageCount : _maxUsageCount;
                    return pooledJobs.Pop();
                }
                else
                {
                    Debug.LogError("TransferJobPool: pooled jobs exhausted!");
                    return null;
                }
            }
        }

        public void Return(TransferJob job)
        {
            lock (_poolLock)
            {
                _usageCount--;
                job.material = TransferManager.TransferReason.None; //flag as unused
                pooledJobs.Push(job);
            }
        }

    }
}
