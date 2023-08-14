using System;
using System.Collections.Generic;
using TransferManagerCE.Util;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataGarbage : StatusData
    {
        public StatusDataGarbage(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.IncinerationPlant:
                case BuildingType.Landfill:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                case BuildingType.WasteTransfer:
                    {
                        if (m_material == TransferReason.Goods)
                        {
                            // Outgoing buffer
                            return building.m_customBuffer2.ToString();
                        }
                        else
                        {
                            LandfillSiteAI? buildingAI = building.Info.GetAI() as LandfillSiteAI;
                            if (buildingAI is not null)
                            {
                                float fCurrent = buildingAI.GetGarbageAmount(m_buildingId, ref building);
                                float fCapacity = buildingAI.m_garbageCapacity;
                                return $"{Math.Round((fCurrent / fCapacity * 100.0), 0)}%";
                            }
                            else
                            {
                                int incomingBuffer = building.m_customBuffer1 * 1000 + building.m_garbageBuffer;
                                return incomingBuffer.ToString();
                            }
                        }
                    }
                case BuildingType.ServicePoint:
                    {
                        ServicePointUtils.GetServicePointOutValues(m_buildingId, TransferReason.Garbage, out int iCount, out int iBuffer);
                        return $"{iCount} | {ServicePointUtils.DisplayBuffer(iBuffer)}";
                    }
                default:
                    {
                        return building.m_garbageBuffer.ToString();
                    }
            }
        }

        public override string GetValueTooltip()
        {
            switch (m_eBuildingType)
            {
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                    {
                        if (m_material == TransferReason.Goods)
                        {
                            return "Amount of Coal/Petrol/Lumber in storage.";
                        }
                        else
                        {
                            return "Amount of garbage in garbage facility.";
                        }
                    }
                case BuildingType.IncinerationPlant:
                case BuildingType.Landfill:
                case BuildingType.WasteTransfer:
                    {
                        return "Amount of garbage in garbage facility.";
                    }
                case BuildingType.ServicePoint:
                    {
                        return "<Building Count> | <Garbage buffer>";
                    }
                default:
                    {
                        return "Value of buildings garbage buffer";
                    }
            }
        }

        protected override string CalculateTarget()
        {
            if (m_material == TransferReason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateTarget();
            }
        }

        protected override string CalculateResponder()
        {
            if (m_material == TransferReason.Goods)
            {
                return "";
            }
            else
            {
                return base.CalculateResponder();
            }
        }
    }
}