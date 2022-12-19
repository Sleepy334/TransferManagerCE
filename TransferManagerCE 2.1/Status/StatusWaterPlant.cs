using System;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusWaterPlant : StatusData
    {
        public StatusWaterPlant(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info?.m_buildingAI is HeatingPlantAI buildingAI)
            {
                if (buildingAI.m_resourceType != TransferReason.None)
                {
                    double dValue = (building.m_customBuffer1 * 0.001);
                    double dCapacity = (buildingAI.m_resourceCapacity * 0.001);
                    return $"{Math.Round(dValue)}/{Math.Round(dCapacity)}";
                }
            }

            return "0";
        }

        public static TransferReason GetInputResource(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.Info?.m_buildingAI is HeatingPlantAI buildingAI)
            {
                return buildingAI.m_resourceType;
            }
            return TransferReason.None;
        }
    }
}