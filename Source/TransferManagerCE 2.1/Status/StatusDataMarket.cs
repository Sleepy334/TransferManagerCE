using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataMarket : StatusData
    {
        public StatusDataMarket(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            int incomingBuffer = (int)((double)building.m_customBuffer1 * 0.001);
            return incomingBuffer.ToString();
        }
    }
}