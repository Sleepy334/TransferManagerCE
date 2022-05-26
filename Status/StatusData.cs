using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public abstract class StatusData
    {
        public TransferReason m_material;
        public ushort m_buildingId;
        public ushort m_responder;
        public ushort m_target;

        public StatusData(TransferReason reason, ushort buildingId, ushort responder, ushort target)
        {
            m_material = reason;
            m_buildingId = buildingId;
            m_responder = responder;
            m_target = target;
        }
        public abstract string GetValue();
        public virtual string GetDescription()
        {
            return "";
        }
        public virtual void Update() { }

        public virtual string GetResponder()
        {
            if (m_responder != 0)
            {
                return CitiesUtils.GetBuildingName(m_responder);
            }

            return "None";
        }

        public virtual string GetTarget()
        {
            if (m_target != 0)
            {
                return CitiesUtils.GetVehicleName(m_target);
            }

            return "None";
        }
    }
}
