using ColossalFramework.Math;
using ICities;
using SleepyCommon;
using System.Reflection;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    // --------------------------------------------------------------------------------------------
    public class StatusDataBuildingCommercial : StatusDataBuilding
    {
        private static int? s_maxLoadSize = null;

        public StatusDataBuildingCommercial(TransferReason material, BuildingType eBuildingType, ushort BuildingId) :
            base(material, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                CommercialBuildingAI? buildingAI = building.Info.GetAI() as CommercialBuildingAI;
                if (buildingAI is not null)
                {
                    switch (m_material)
                    {
                        case TransferReason.Goods:
                        case TransferReason.Food:
                        case TransferReason.LuxuryProducts:
                            {
                                int maxIncomingLoadSize = MaxIncomingLoadSize(buildingAI);
                                int visitPlaceCount = GetCustomerPlaces(buildingAI, building);
                                int iBufferSize = Mathf.Max(visitPlaceCount * 500, maxIncomingLoadSize * 4);

                                WarnText(true, false, building.m_customBuffer1, iBufferSize);
                                tooltip = MakeTooltip(true, building.m_customBuffer1, iBufferSize);
                                return DisplayValueAsPercent(building.m_customBuffer1, iBufferSize);
                            }
                        default:
                            {
                                int iCustomerCount = GetCustomerCount(buildingAI, m_buildingId, building);
                                int iVisitPlaces = GetCustomerPlaces(buildingAI, building);

                                WarnText(true, false, iCustomerCount, iVisitPlaces);
                                tooltip = "Customers / Total Places"; ;
                                return $"{iCustomerCount} / {iVisitPlaces}";
                            }
                    }
                }
            }

            tooltip = "";
            return "";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;

            string sTimer = base.CalculateTimer(out tooltip);
            
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                if (bIncoming && building.m_incomingProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sTimer))
                    {
                        sTimer += " ";
                    }
                    sTimer += "I:" + building.m_incomingProblemTimer;
                }

                if (!bIncoming && building.m_outgoingProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sTimer))
                    {
                        sTimer += " ";
                    }
                    sTimer += "O:" + building.m_outgoingProblemTimer;
                }
            }

            return sTimer;
        }

        public static TransferReason GetOutgoingTransferReason(CommercialBuildingAI buildingAI, ushort buildingId)
        {
            if (buildingAI is not null)
            {
                MethodInfo? methodGetOutgoingTransferReason = buildingAI.GetType().GetMethod("GetOutgoingTransferReason", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodGetOutgoingTransferReason != null)
                {
                    object[] args = new object[] { buildingId };
                    return (TransferReason) methodGetOutgoingTransferReason.Invoke(buildingAI, args);
                }
            }

            return TransferReason.None;
        }

        public static int GetCustomerCount(CommonBuildingAI buildingAI, ushort buildingId, Building building)
        {
            return BuildingUtils.GetVisitorCount(buildingAI, buildingId, building);
        }

        public int GetCustomerPlaces(CommercialBuildingAI buildingAI, Building building)
        {
            return buildingAI.CalculateVisitplaceCount((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
        }

        // We call this function incase it is altered by something like Rebalanced Industries
        // Cache the result as reflection is slow.
        private static int MaxIncomingLoadSize(CommercialBuildingAI instance)
        {
            if (s_maxLoadSize == null)
            {
                // We try to call WarehouseAI.GetMaxLoadSize as some mods such as Industry Rebalanced modify this value
                // Unfortunately it is private so we need to use reflection, so we cache the results.
                MethodInfo getMaxIncomingLoadSize = typeof(CommercialBuildingAI).GetMethod("MaxIncomingLoadSize", BindingFlags.Instance | BindingFlags.NonPublic);
                if (getMaxIncomingLoadSize != null)
                {
                    s_maxLoadSize = (int)getMaxIncomingLoadSize.Invoke(instance, null);
                }
                else
                {
                    // Fall back on default if we fail to get the function
                    s_maxLoadSize = 4000;
                }
            }

            return s_maxLoadSize.Value;
        }
    }
}