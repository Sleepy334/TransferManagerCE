using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataMail : StatusData
    {
        public StatusDataMail(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info.GetAI() is PostOfficeAI postOfficeAI)
            {
                int amount;
                int max;
                postOfficeAI.GetMaterialAmount(m_buildingId, ref building, m_material, out amount, out max);
                return (amount * 0.1).ToString();
            }
            else
            {
                return building.m_mailBuffer.ToString();
            }
        }
    }
}