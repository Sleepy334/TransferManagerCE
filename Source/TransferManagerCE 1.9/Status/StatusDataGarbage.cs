using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataGarbage : StatusData
    {
        public StatusDataGarbage(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Garbage, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.Landfill:
                case BuildingType.Recycling:
                case BuildingType.WasteProcessing:
                case BuildingType.WasteTransfer:
                    {
                        int incomingBuffer = building.m_customBuffer1 * 1000 + building.m_garbageBuffer;
                        return incomingBuffer.ToString();
                    }
                default:
                    {
                        return building.m_garbageBuffer.ToString();
                    }
            }
        }
    }
}