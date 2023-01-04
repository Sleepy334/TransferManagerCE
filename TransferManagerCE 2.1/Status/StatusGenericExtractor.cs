using ColossalFramework.Math;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusGenericExtractor : StatusData
    {
        public StatusGenericExtractor(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info.GetAI() is IndustrialExtractorAI extractor)
            {
                int iProductionCapacity = extractor.CalculateProductionCapacity((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
                int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);
                return Math.Round((double)building.m_customBuffer1 * 0.001, 1) + "/" + ((double)iStorageCapacity * 0.001);
            }

            return Math.Round((double)building.m_customBuffer1 * 0.001, 1).ToString();
        }

        public override string GetTimer()
        {
            string sTimer = base.GetTimer();

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

        public override string GetTarget()
        {
            return ""; // An extractor will never have a responder
        }

        public override string GetResponder()
        {
            return ""; // An extractor will never have a responder
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