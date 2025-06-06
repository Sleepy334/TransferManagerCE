using ColossalFramework.Math;
using System.Reflection;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataHotel : StatusDataBuilding
    {
        public StatusDataHotel(BuildingType eBuildingType, ushort BuildingId) :
            base(CustomTransferReason.Reason.None, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Guests";
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Guests / Max Guests";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                HotelAI? hotelAI = building.Info?.GetAI() as HotelAI;
                if (hotelAI != null)
                {
                    return $"{HotelAI.GetCurrentGuests(ref building)} / {hotelAI.m_rooms}";
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