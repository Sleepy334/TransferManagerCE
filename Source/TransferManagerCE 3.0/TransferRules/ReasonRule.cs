using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.TransferRules
{
    public class ReasonRule
    {
        public int m_id = 0;
        public string m_name = "";
        public HashSet<CustomTransferReason.Reason> m_reasons = new HashSet<CustomTransferReason.Reason>();
        public bool m_incomingDistrict = false;
        public bool m_outgoingDistrict = false;
        public bool m_incomingBuilding = false;
        public bool m_outgoingBuilding = false;
        public bool m_distance = false;
        public bool m_import = false;
        public bool m_export = false;

        public void AddReason(CustomTransferReason.Reason reason)
        {
            m_reasons.Add(reason);
        }
    }
}
