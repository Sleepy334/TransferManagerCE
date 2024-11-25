using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusGenericProcessing : StatusData
    {
        public StatusGenericProcessing(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info.GetAI() is IndustrialBuildingAI processingAI)
            {
                TransferReason outgoingMaterial = GetOutgoingTransferReason(building);
                if (m_material == outgoingMaterial)
                {
                    int iProductionCapacity = processingAI.CalculateProductionCapacity((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
                    int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);
                    return Math.Round((double)building.m_customBuffer2 * 0.001, 1) + "/" + ((double)iStorageCapacity * 0.001);
                }
                else
                {
                    int iProductionCapacity = processingAI.CalculateProductionCapacity((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
                    int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);
                    return Math.Round((double)building.m_customBuffer1 * 0.001, 1) + "/" + ((double)iStorageCapacity * 0.001);
                }
            }

            return "0";
        }

        protected override string CalculateTimer()
        {
            string sTimer = base.CalculateTimer();

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                TransferReason outgoingMaterial = GetOutgoingTransferReason(building);
                if (m_material == outgoingMaterial)
                {
                    if (building.m_outgoingProblemTimer > 0)
                    {
                        if (string.IsNullOrEmpty(sTimer))
                        {
                            sTimer += " ";
                        }
                        sTimer += "O:" + building.m_outgoingProblemTimer;
                    }
                }
                else
                {
                    if (building.m_incomingProblemTimer > 0)
                    {
                        if (string.IsNullOrEmpty(sTimer))
                        {
                            sTimer += " ";
                        }
                        sTimer += "I:" + building.m_incomingProblemTimer;
                    }
                }
            }

            return sTimer;
        }

        protected override string CalculateTarget()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (m_material == GetOutgoingTransferReason(building))
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateTarget();
            }
        }

        protected override string CalculateResponder()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (m_material == GetOutgoingTransferReason(building))
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateResponder();
            }
        }

        public static TransferReason GetIncomingTransferReason(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
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
                    switch (new Randomizer(buildingId).Int32(4u))
                    {
                        case 0:
                            return TransferManager.TransferReason.Lumber;
                        case 1:
                            return TransferManager.TransferReason.Food;
                        case 2:
                            return TransferManager.TransferReason.Petrol;
                        case 3:
                            return TransferManager.TransferReason.Coal;
                        default:
                            return TransferManager.TransferReason.None;
                    }
            }
        }

        public static TransferReason GetSecondaryIncomingTransferReason(ushort buildingID)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingID];
            if (building.Info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric)
            {
                switch (new Randomizer(buildingID).Int32(8u))
                {
                    case 0:
                        return TransferManager.TransferReason.PlanedTimber;
                    case 1:
                        return TransferManager.TransferReason.Paper;
                    case 2:
                        return TransferManager.TransferReason.Flours;
                    case 3:
                        return TransferManager.TransferReason.AnimalProducts;
                    case 4:
                        return TransferManager.TransferReason.Petroleum;
                    case 5:
                        return TransferManager.TransferReason.Plastics;
                    case 6:
                        return TransferManager.TransferReason.Metals;
                    case 7:
                        return TransferManager.TransferReason.Glass;
                }
            }

            return TransferManager.TransferReason.None;
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            switch (building.Info.m_class.m_subService)
            {
                case ItemClass.SubService.IndustrialForestry:
                    return TransferManager.TransferReason.Lumber;
                case ItemClass.SubService.IndustrialFarming:
                    return TransferManager.TransferReason.Food;
                case ItemClass.SubService.IndustrialOil:
                    return TransferManager.TransferReason.Petrol;
                case ItemClass.SubService.IndustrialOre:
                    return TransferManager.TransferReason.Coal;
                default:
                    return TransferManager.TransferReason.Goods;
            }
        }
    }
}