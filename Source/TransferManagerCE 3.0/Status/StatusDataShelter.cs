using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataShelter : StatusData
    {
        public StatusDataShelter(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
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
                    return Math.Round(amount * 0.001)  + "/" + Math.Round(max * 0.001);
                }
            }
            return 0.ToString();
        }
    }
}