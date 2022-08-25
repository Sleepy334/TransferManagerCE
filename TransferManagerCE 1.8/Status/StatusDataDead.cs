using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataDead : StatusData
    {
        public StatusDataDead(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Dead, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.Cemetery:
                    {
                        CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                        if (cemeteryAI != null)
                        {
                            int iAmount;
                            int iMax;
                            cemeteryAI.GetMaterialAmount(m_buildingId, ref building, TransferReason.Dead, out iAmount, out iMax);
                            return iAmount + "/" + iMax;
                        }
                        else
                        {
                            return CitiesUtils.GetDead(m_buildingId, building).Count.ToString();
                        }
                    }
                default:
                    {
                        return CitiesUtils.GetDead(m_buildingId, building).Count.ToString();
                    }
            }
        }
        
        public override string GetTimer()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_deathProblemTimer > 0)
            {
                return base.GetTimer() + "D:" + building.m_deathProblemTimer.ToString();
            }
            else
            {
                return base.GetTimer();
            }
        }
        
    }
}