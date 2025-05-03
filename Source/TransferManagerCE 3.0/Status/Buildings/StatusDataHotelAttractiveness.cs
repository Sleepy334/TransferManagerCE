using ColossalFramework.Math;
using System.Reflection;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataHotelAttractiveness : StatusDataBuilding
    {
        public StatusDataHotelAttractiveness(BuildingType eBuildingType, ushort BuildingId) :
            base(TransferReason.None, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Attractiveness";
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Attractiveness score of hotel";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                HotelAI? hotelAI = building.Info?.GetAI() as HotelAI;
                if (hotelAI != null)
                {
                    return $"{HotelAI.GetPopularityRatingPercent(ref building)}%";
                }
            }

            return "";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            tooltip = "";
            return "";
        }
    }
}