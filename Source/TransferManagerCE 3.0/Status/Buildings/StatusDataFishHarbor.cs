using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataFishHarbor : StatusDataExtractionFacility
    {
        public StatusDataFishHarbor(BuildingType eBuildingType, ushort BuildingId) :
            base(CustomTransferReason.Reason.Fish, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        { 
            int value = 0;
            int bufferSize = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (building.Info?.m_buildingAI)
            {
                case FishingHarborAI fishingAI:
                    {
                        value = building.m_customBuffer2;
                        bufferSize = fishingAI.m_storageBufferSize;
                        break;
                    }
                case FishFarmAI fishFarmAI:
                    {
                        value = building.m_customBuffer2;
                        bufferSize = fishFarmAI.m_storageBufferSize;
                        break;
                    }
            }

            WarnText(false, true, value, bufferSize);
            tooltip = MakeTooltip(value, bufferSize);
            return DisplayValueAsPercent(value, bufferSize);
        }
    }
}