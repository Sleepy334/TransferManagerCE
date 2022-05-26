using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataMail : StatusData
    {
        public StatusDataMail(ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Mail, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_mailBuffer.ToString();
        }
    }
}