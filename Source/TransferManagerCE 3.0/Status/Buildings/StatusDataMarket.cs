using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataMarket : StatusDataBuilding
    {
        public StatusDataMarket(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            int incomingBuffer = (int)((double)building.m_customBuffer1 * 0.001);
            tooltip = "";
            return incomingBuffer.ToString();
        }
    }
}