using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataFire : StatusData
    {
        public StatusDataFire(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Fire, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_fireIntensity.ToString();
        }
    }
}