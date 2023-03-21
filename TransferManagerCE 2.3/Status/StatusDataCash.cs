using ColossalFramework.Math;
using ICities;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCash : StatusData
    {
        public StatusDataCash(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Cash, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                switch (m_eBuildingType)
                {
                    case BuildingType.Bank:
                        {
                            return "0"; // TODO
                        }
                    default:
                        {
                            int iCashCapacity = GetCashCapacity(m_buildingId, building);
                            if (iCashCapacity > 0)
                            {
                                return $"{Math.Round(((float)building.m_cashBuffer / (float)iCashCapacity * 100.0), 0)}%";
                            }
                            else
                            {
                                return $"{building.m_cashBuffer / 10}";
                            }
                        }
                }
            }

            return "0";
        }

        public override string GetValueTooltip()
        {
            return "";
        }

        public static int GetCashCapacity(ushort buildingID, Building data)
        {
            return GetGoodsCapacity(buildingID, data) * 4;
        }

        private static int GetGoodsCapacity(ushort buildingID, Building building)
        {
            if (building.Info is not null && building.Info.GetAI() is CommercialBuildingAI buildingAI)
            {
                int width = building.Width;
                int length = building.Length;
                int num = MaxIncomingLoadSize();
                int num2 = buildingAI.CalculateVisitplaceCount((ItemClass.Level)building.m_level, new Randomizer(buildingID), width, length);
                return Mathf.Max(num2 * 500, num * 4);
            }
            return 0;
        }

        private static int MaxIncomingLoadSize()
        {
            return 4000;
        }
    }
}