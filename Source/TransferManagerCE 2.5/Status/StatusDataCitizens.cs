using ColossalFramework.Math;
using Epic.OnlineServices.Presence;
using ICities;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCitizens : StatusData
    {
        public StatusDataCitizens(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Citizens";
        }

        protected override string CalculateValue()
        {
            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    return $"{BuildingUtils.GetCitizenCountInBuilding(m_buildingId, building, 0)}";
                }
            }

            return "0";
        }

        protected override string CalculateTimer()
        {
            return "";
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
            return TransferReason.None;
        }
    }
}