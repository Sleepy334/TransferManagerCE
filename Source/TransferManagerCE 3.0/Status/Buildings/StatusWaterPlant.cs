using System;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusWaterPlant : StatusDataBuilding
    {
        public StatusWaterPlant(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort BuildingId) :
            base(material, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info?.m_buildingAI is HeatingPlantAI buildingAI)
            {
                if (buildingAI.m_resourceType != TransferReason.None)
                {
                    double dValue = (building.m_customBuffer1 * 0.001);
                    double dCapacity = (buildingAI.m_resourceCapacity * 0.001);

                    tooltip = MakeTooltip((int) dValue, (int) dCapacity);
                    return $"{Math.Round(dValue)}/{Math.Round(dCapacity)}";
                }
            }

            tooltip = "";
            return "";
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