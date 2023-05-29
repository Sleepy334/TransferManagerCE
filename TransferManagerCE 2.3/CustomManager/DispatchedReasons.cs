using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    internal class DispatchedReasons
    {
        // A set of all running job reasons
        private HashSet<CustomTransferReason.Reason> m_dispatchedReasons = new HashSet<CustomTransferReason.Reason>();
        private readonly object m_reasonsLock = new object();

        public bool IsDispatchedReason(CustomTransferReason.Reason material)
        {
            lock (m_reasonsLock)
            {
                return m_dispatchedReasons.Contains(material);
            }
        }

        public void AddDispatchedReason(CustomTransferReason.Reason material)
        {
            lock (m_reasonsLock)
            {
                m_dispatchedReasons.Add(material);
            }
        }

        public void RemoveDispatchedReason(CustomTransferReason.Reason material)
        {
            lock (m_reasonsLock)
            {
                if (m_dispatchedReasons.Contains(material))
                {
                    m_dispatchedReasons.Remove(material);
                }
            }
        }
    }
}
