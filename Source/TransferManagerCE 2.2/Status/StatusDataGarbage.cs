using System;
using System.Collections.Generic;
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
                            if (buildingAI != null)
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
                        Dictionary<TransferReason, int> serviceValues = StatusHelper.GetServicePointValues(m_buildingId);
                        if (serviceValues.ContainsKey(TransferReason.Garbage))
                        {
                            return $"{building.m_garbageBuffer}/{serviceValues[TransferReason.Garbage]}";
                        }
                        else
                        {
                            return $"{building.m_garbageBuffer}/0";
                        }
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
                        return "<garbage buffer> / <# of buildings wanting garbage collection>";
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