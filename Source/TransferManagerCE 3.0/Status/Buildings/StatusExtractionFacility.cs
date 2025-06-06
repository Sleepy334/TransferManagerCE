using System;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataExtractionFacility : StatusDataBuilding
    {
        public StatusDataExtractionFacility(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            int value = 0;
            int bufferSize = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info?.m_buildingAI is ExtractingFacilityAI buildingAI)
            {
                if (buildingAI.m_outputResource != TransferReason.None)
                {
                    bufferSize = buildingAI.GetOutputBufferSize(m_buildingId, ref building);
                    value = building.m_customBuffer1;
                }
            } 
            else if (building.Info?.m_buildingAI is FishingHarborAI fishingAI)
            {
                value = building.m_customBuffer2;
                bufferSize = fishingAI.m_storageBufferSize;
            }
            else if (building.Info?.m_buildingAI is FishFarmAI fishFarmAI)
            {
                value = building.m_customBuffer2;
                bufferSize = fishFarmAI.m_storageBufferSize;
            }

            WarnText(false, true, value, bufferSize);
            tooltip = MakeTooltip(false, value, bufferSize);
            return DisplayValueAsPercent(value, bufferSize);
        }

        public static TransferReason GetOutputResource(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.Info?.m_buildingAI is ExtractingFacilityAI buildingAI)
            {
                return buildingAI.m_outputResource;
            }
            else if (building.Info?.m_buildingAI is FishingHarborAI)
            {
                return TransferReason.Fish;
            }
            else if (building.Info?.m_buildingAI is FishFarmAI)
            {
                return TransferReason.Fish;
            }
            return TransferReason.None;
        }
    }
}