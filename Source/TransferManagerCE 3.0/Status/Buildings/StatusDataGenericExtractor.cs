using ColossalFramework.Math;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataGenericExtractor : StatusDataGenericIndustry
    {
        // --------------------------------------------------------------------
        public StatusDataGenericExtractor(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        // --------------------------------------------------------------------
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

        // --------------------------------------------------------------------
        protected override string CalculateTimer(out string tooltip)
        {
            string sTimer = base.CalculateTimer(out tooltip);

            AddTimerText(TimerType.Outgoing, ref sTimer, ref tooltip);

            return sTimer;
        }
    }
}