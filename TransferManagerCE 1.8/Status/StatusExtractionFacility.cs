using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataExtractionFacility : StatusData
    {
        public StatusDataExtractionFacility(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            double dValue = 0;
            double dBufferSize = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info?.m_buildingAI is ExtractingFacilityAI buildingAI)
            {
                if (buildingAI.m_outputResource != TransferReason.None)
                {
                    dBufferSize = buildingAI.GetOutputBufferSize(m_buildingId, ref building) * 0.001;
                    dValue = (building.m_customBuffer1 * 0.001);
                }
            } 
            else if (building.Info?.m_buildingAI is FishingHarborAI fishingAI)
            {
                dValue = building.m_customBuffer2 * 0.1;
                dBufferSize = fishingAI.m_storageBufferSize * 0.001;
            }
            else if (building.Info?.m_buildingAI is FishFarmAI fishFarmAI)
            {
                dValue = building.m_customBuffer2 * 0.1;
                dBufferSize = fishFarmAI.m_storageBufferSize * 0.001;
            }
            
            return Math.Round(dValue).ToString("N0") + "/" + Math.Round(dBufferSize).ToString("N0");
        }

        public override string GetTarget()
        {
            return ""; // An extractor will never have a responder
        }

        public override string GetResponder()
        {
            return ""; // An extractor will never have a responder
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