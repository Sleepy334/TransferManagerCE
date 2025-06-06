using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataDead : StatusDataBuilding
    {
        public StatusDataDead(BuildingType eBuildingType, ushort BuildingId) : 
            base(CustomTransferReason.Reason.Dead, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.Cemetery:
                    {
                        CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                        if (cemeteryAI is not null)
                        {
                            int iAmount;
                            int iMax;
                            cemeteryAI.GetMaterialAmount(m_buildingId, ref building, TransferReason.Dead, out iAmount, out iMax);
                            tooltip = MakeTooltip(iAmount, iMax);
                            return iAmount + "/" + iMax;
                        }
                        else
                        {
                            return BuildingUtils.GetDeadCount(m_buildingId, building).ToString();
                        }
                    }
                default:
                    {
                        // Default handling
                        WarnText(false, true, building.m_deathProblemTimer, 1);
                        return BuildingUtils.GetDeadCount(m_buildingId, building).ToString();
                    }
            }
        }
        
        protected override string CalculateTimer(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_deathProblemTimer > 0)
            {
                return base.CalculateTimer(out tooltip) + "D:" + building.m_deathProblemTimer.ToString();
            }
            else
            {
                return base.CalculateTimer(out tooltip);
            }
        }
    }
}