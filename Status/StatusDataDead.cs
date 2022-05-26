using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataDead : StatusData
    {
        public StatusDataDead(ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Dead, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return CitiesUtils.GetDeadCitizens(m_buildingId, building).Count.ToString();
        }
        
        public override string GetDescription()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return "Timer: " + building.m_deathProblemTimer.ToString();
        }
        
    }
}