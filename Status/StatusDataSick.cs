using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataSick : StatusData
    {
        public StatusDataSick(ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Sick, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return CitiesUtils.GetSickCitizens(m_buildingId, building).Count.ToString();
        }
        
        public override string GetTimer()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_healthProblemTimer.ToString();
        }
        
    }
}