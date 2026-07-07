using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
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
            string sTimer = base.CalculateTimer(out tooltip);

            AddTimerText(TimerType.Death, ref sTimer, ref tooltip);

            return sTimer;
        }
    }
}