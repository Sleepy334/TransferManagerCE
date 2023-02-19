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

        protected override string CalculateValue()
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
                            return BuildingUtils.GetDeadCount(m_buildingId, building).ToString();
                        }
                    }
                default:
                    {
                        return BuildingUtils.GetDeadCount(m_buildingId, building).ToString();
                    }
            }
        }
        
        protected override string CalculateTimer()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_deathProblemTimer > 0)
            {
                return base.CalculateTimer() + "D:" + building.m_deathProblemTimer.ToString();
            }
            else
            {
                return base.CalculateTimer();
            }
        }
        
    }
}