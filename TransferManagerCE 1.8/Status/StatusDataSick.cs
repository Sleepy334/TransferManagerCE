using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSick : StatusData
    {
        public StatusDataSick(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Sick, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.Hospital:
                    {
                        HospitalAI? hospital = building.Info.m_buildingAI as HospitalAI;
                        if (hospital != null)
                        {
                            return CitiesUtils.GetSick(m_buildingId, building).Count + "/" + hospital.m_patientCapacity;
                        }
                        else
                        {
                            return CitiesUtils.GetSick(m_buildingId, building).Count.ToString();
                        }
                    }
                default:
                    {
                        return CitiesUtils.GetSick(m_buildingId, building).Count.ToString();
                    }
            }
        }
        
        public override string GetTimer()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_healthProblemTimer > 0)
            {
                return base.GetTimer() + "S:" + building.m_healthProblemTimer.ToString();
            }
            else
            {
                return base.GetTimer();
            }
        }
        
    }
}