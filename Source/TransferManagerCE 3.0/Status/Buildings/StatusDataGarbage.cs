using ICities;
using SleepyCommon;
using System.Reflection;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataBuildingGarbage : StatusDataBuilding
    {
        private static int? s_maxLoadSize = null;

        public StatusDataBuildingGarbage(TransferReason material, BuildingType eBuildingType, ushort BuildingId) : 
            base(material, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.Landfill:
                case BuildingType.WasteTransfer:
                    {
                        LandfillSiteAI? buildingAI = building.Info.GetAI() as LandfillSiteAI;
                        if (buildingAI is not null)
                        {
                            float fCurrent = buildingAI.GetGarbageAmount(m_buildingId, ref building);
                            float fCapacity = buildingAI.m_garbageCapacity;

                            WarnText(false, true, (int)fCurrent, (int)fCapacity);
                            tooltip = MakeTooltip((int)fCurrent, (int)fCapacity);
                            return DisplayValueAsPercent((int)fCurrent, (int)fCapacity);
                        }
                        else
                        {
                            tooltip = "";
                            return "";
                        }
                    }
                case BuildingType.IncinerationPlant:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                    {
                        if (m_material == TransferReason.Goods)
                        {
                            LandfillSiteAI? buildingAI = building.Info.GetAI() as LandfillSiteAI;
                            if (buildingAI is not null)
                            {
                                int iCurrentCapacity = building.m_customBuffer2;
                                int iStorageCapacity = MaxOutgoingLoadSize(buildingAI) * 4;

                                WarnText(false, true, iCurrentCapacity, iStorageCapacity);
                                tooltip = MakeTooltip(false, iCurrentCapacity, iStorageCapacity);
                                return DisplayValueAsPercent(iCurrentCapacity, iStorageCapacity); // Outgoing
                            }
                            else
                            {
                                // Outgoing buffer
                                tooltip = MakeTooltip(building.m_customBuffer2);
                                return DisplayBuffer(building.m_customBuffer2);
                            }
                        }
                        else
                        {
                            LandfillSiteAI? buildingAI = building.Info.GetAI() as LandfillSiteAI;
                            if (buildingAI is not null)
                            {
                                float fCurrent = buildingAI.GetGarbageAmount(m_buildingId, ref building);
                                float fCapacity = buildingAI.m_garbageCapacity;

                                WarnText(true, true, (int)fCurrent, (int)fCapacity);
                                tooltip = MakeTooltip((int)fCurrent, (int)fCapacity);
                                return DisplayValueAsPercent((int) fCurrent, (int) fCapacity);
                            }
                            else
                            {
                                int incomingBuffer = building.m_customBuffer1 * 1000 + building.m_garbageBuffer;

                                tooltip = MakeTooltip(incomingBuffer);
                                return DisplayBuffer(incomingBuffer);
                            }
                        }
                    }
                case BuildingType.ServicePoint:
                    {
                        ServicePointUtils.GetServicePointOutValues(m_buildingId, TransferReason.Garbage, out int iCount, out int iBuffer);
                        
                        tooltip = $"Buildings with {m_material}: {iCount}\n{m_material}: {DisplayBufferLong(iBuffer)}";
                        return $"{iCount} | {DisplayBuffer(iBuffer)}";
                    }
                default:
                    {
                        WarnText(false, true, building.m_garbageBuffer, 8000);
                        tooltip = MakeTooltip(false, building.m_garbageBuffer, 8000);
                        return DisplayValueAsPercent(building.m_garbageBuffer, 8000);
                    }
            }
        }

        // We call this function incase it is altered by something like Rebalanced Industries
        // Cache the result as reflection is slow.
        private static int MaxOutgoingLoadSize(LandfillSiteAI instance)
        {
            if (s_maxLoadSize == null)
            {
                // We try to call WarehouseAI.GetMaxLoadSize as some mods such as Industry Rebalanced modify this value
                // Unfortunately it is private so we need to use reflection, so we cache the results.
                MethodInfo getMaxOutgoingLoadSize = typeof(LandfillSiteAI).GetMethod("MaxOutgoingLoadSize", BindingFlags.Instance | BindingFlags.NonPublic);
                if (getMaxOutgoingLoadSize != null)
                {
                    s_maxLoadSize = (int)getMaxOutgoingLoadSize.Invoke(instance, null);
                }
                else
                {
                    // Fall back on default if we fail to get the function
                    s_maxLoadSize = 8000;
                }
            }

            return s_maxLoadSize.Value;
        }
    }
}