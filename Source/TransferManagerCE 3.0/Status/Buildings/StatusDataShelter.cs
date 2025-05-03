using SleepyCommon;
using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataShelter : StatusDataBuilding
    {
        public StatusDataShelter(TransferReason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ShelterAI? buildingAI = building.Info?.m_buildingAI as ShelterAI;
            if (buildingAI is not null)
            {
                int amount;
                int max;
                if (m_material == TransferReason.Food || m_material == TransferReason.Goods)
                {
                    buildingAI.GetFoodStatus(m_buildingId, ref building, out amount, out max);
                    tooltip = MakeTooltip(true, amount, max);
                    return Utils.MakePercent(amount, max);
                }
            }

            tooltip = "";
            return 0.ToString();
        }
    }
}