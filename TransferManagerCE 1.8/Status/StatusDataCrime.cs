using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCrime : StatusData
    {
        public StatusDataCrime(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Crime, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            PoliceStationAI? station = building.Info.GetAI() as PoliceStationAI;
            if (station != null)
            {
                return CitiesUtils.GetCriminalsAtPoliceStation(building).Count + "/" + station.m_jailCapacity;
            }
            else 
            {
                return building.m_crimeBuffer + "/" + CitiesUtils.GetCriminals(m_buildingId, building).Count;
            }
        }
    }
}