using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataCrime : StatusData
    {
        public StatusDataCrime(ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Crime, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_crimeBuffer.ToString();
        }
    }
}