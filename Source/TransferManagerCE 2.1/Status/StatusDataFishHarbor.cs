using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataFishHarbor : StatusDataExtractionFacility
    {
        public StatusDataFishHarbor(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(TransferReason.Fish, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            double dValue = 0;
            double dBufferSize = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (building.Info?.m_buildingAI)
            {
                case FishingHarborAI fishingAI:
                    {
                        dValue = building.m_customBuffer2 * 0.1;
                        dBufferSize = fishingAI.m_storageBufferSize * 0.001;
                        break;
                    }
                case FishFarmAI fishFarmAI:
                    {
                        dValue = building.m_customBuffer2 * 0.1;
                        dBufferSize = fishFarmAI.m_storageBufferSize * 0.001;
                        break;
                    }
            }

            return Math.Round(dValue).ToString("N0") + "/" + Math.Round(dBufferSize).ToString("N0");
        }
    }
}