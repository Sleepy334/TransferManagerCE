using ColossalFramework.Math;
using Epic.OnlineServices.Presence;
using ICities;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataOffice : StatusData
    {
        public StatusDataOffice(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            if (m_material != TransferReason.None)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    return (building.m_customBuffer2 * 0.001).ToString("N1");
                }
            }

            return "";
        }

        protected override string CalculateTimer()
        {
            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;

            string sTimer = base.CalculateTimer();
            
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.m_outgoingProblemTimer > 0)
            {
                if (!string.IsNullOrEmpty(sTimer))
                {
                    sTimer += " ";
                }
                sTimer += "O:" + building.m_outgoingProblemTimer;
            }

            return sTimer;
        }

        protected override string CalculateTarget()
        {
            return ""; // No vehicles
        }

        protected override string CalculateResponder()
        {
            return ""; // No vehicles
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            if (building.Info.GetSubService() == ItemClass.SubService.OfficeHightech)
            {
                return TransferReason.Goods;
            }

            return TransferReason.None;
        }
    }
}