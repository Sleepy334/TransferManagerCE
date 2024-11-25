using System;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusPowerPlant : StatusData
    {
        public StatusPowerPlant(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            double dValue = 0;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info?.m_buildingAI is PowerPlantAI buildingAI)
            {
                if (buildingAI.m_resourceType != TransferReason.None)
                {
                    dValue = (building.m_customBuffer1 * 0.001);
                }
            }

            return Math.Round(dValue).ToString("N0");
        }

        public static TransferReason GetInputResource(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.Info?.m_buildingAI is PowerPlantAI buildingAI)
            {
                return buildingAI.m_resourceType;
            }
            return TransferReason.None;
        }
    }
}