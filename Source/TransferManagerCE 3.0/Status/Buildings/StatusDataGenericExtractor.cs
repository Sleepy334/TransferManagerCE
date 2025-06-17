using ColossalFramework.Math;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.CustomTransferReason;

namespace TransferManagerCE.Data
{
    public class StatusDataGenericExtractor : StatusDataBuilding
    {
        public StatusDataGenericExtractor(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info.GetAI() is IndustrialExtractorAI extractor)
            {
                int iProductionCapacity = extractor.CalculateProductionCapacity((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
                int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);

                WarnText(false, true, building.m_customBuffer1, iStorageCapacity);
                tooltip = MakeTooltip(building.m_customBuffer1, iStorageCapacity);
                return DisplayValueAsPercent(building.m_customBuffer1, iStorageCapacity);
            }

            tooltip = MakeTooltip(building.m_customBuffer1);
            return Math.Round((double)building.m_customBuffer1 * 0.001, 1).ToString("N1");
        }

        protected override string CalculateTimer(out string tooltip)
        {
            string sTimer = base.CalculateTimer(out tooltip);

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_outgoingProblemTimer > 0)
            {
                if (string.IsNullOrEmpty(sTimer))
                {
                    sTimer += " ";
                }
                sTimer += "O:" + building.m_outgoingProblemTimer;
            }

            return sTimer;
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            switch (building.Info.m_class.m_subService)
            {
                case ItemClass.SubService.IndustrialForestry:
                    return TransferManager.TransferReason.Logs;
                case ItemClass.SubService.IndustrialFarming:
                    return TransferManager.TransferReason.Grain;
                case ItemClass.SubService.IndustrialOil:
                    return TransferManager.TransferReason.Oil;
                case ItemClass.SubService.IndustrialOre:
                    return TransferManager.TransferReason.Ore;
                default:
                    return TransferManager.TransferReason.None;
            }
        }
    }
}