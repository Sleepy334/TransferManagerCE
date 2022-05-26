using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataFire : StatusData
    {
        public StatusDataFire(ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Fire, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_fireIntensity.ToString();
        }
    }
}