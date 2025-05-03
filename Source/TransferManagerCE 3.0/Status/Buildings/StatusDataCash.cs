using ColossalFramework.Math;
using System;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCash : StatusDataBuilding
    {
        public StatusDataCash(BuildingType eBuildingType, ushort BuildingId) : 
            base(TransferReason.Cash, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                switch (m_eBuildingType)
                {
                    case BuildingType.Bank:
                        {
                            tooltip = "Percent of Cash Capacity";
                            return "0"; // TODO
                        }
                    case BuildingType.ServicePoint:
                        {
                            ServicePointUtils.GetServicePointOutValues(m_buildingId, TransferReason.Cash, out int iCount, out int iBuffer);
                            tooltip = $"Buildings with {m_material}: {iCount}\n{m_material} buffer: {DisplayBufferLong(iBuffer)}";
                            return $"{iCount} | {DisplayBuffer(iBuffer)}";
                        }
                    default:
                        {
                            int iCashCapacity = GetCashCapacity(m_buildingId, building);
                            if (iCashCapacity > 0)
                            {
                                WarnText(false, true, building.m_cashBuffer, iCashCapacity);
                                tooltip = MakeTooltip(building.m_cashBuffer, iCashCapacity);
                                return DisplayValueAsPercent(building.m_cashBuffer, iCashCapacity);
                            }
                            else
                            {
                                tooltip = MakeTooltip(building.m_cashBuffer);
                                return DisplayBuffer(building.m_cashBuffer / 10);
                            }
                        }
                }
            }

            tooltip = "";
            return "0";
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