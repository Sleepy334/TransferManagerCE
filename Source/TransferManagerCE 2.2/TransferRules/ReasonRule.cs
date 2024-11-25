using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.TransferRules
{
    public class ReasonRule
    {
        public int m_id = 0;
        public string m_name = "";
        public HashSet<TransferReason> m_reasons = new HashSet<TransferReason>();
        public bool m_incomingDistrict = false;
        public bool m_outgoingDistrict = false;
        public bool m_incomingBuilding = false;
        public bool m_outgoingBuilding = false;
        public bool m_distance = false;
        public bool m_import = false;
        public bool m_export = false;

        public void AddReason(TransferReason reason)
        {
            m_reasons.Add(reason);
        }
    }
}
