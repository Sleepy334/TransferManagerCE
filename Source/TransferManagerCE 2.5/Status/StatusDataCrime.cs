using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCrime : StatusData
    {
        public StatusDataCrime(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                if (building.Info.GetAI().name.Contains("PrisonCopterPoliceStationAI")) 
                {
                    // Prison Helicopter Mod
                    return BuildingUtils.GetCriminalsAtPoliceStation(m_buildingId, building).ToString();
                }
                switch (building.Info.GetAI())
                {
                    case PoliceStationAI station:
                        {
                            return BuildingUtils.GetCriminalsAtPoliceStation(m_buildingId, building) + "/" + station.m_jailCapacity;
                        }
                    default:
                        {
                            string sValue = "";

                            int iCitizenCount = building.m_citizenCount;
                            if (iCitizenCount > 0)
                            {
                                sValue += building.m_crimeBuffer / iCitizenCount;
                            }
                            else
                            {
                                sValue += building.m_crimeBuffer;
                            }

                            return $"{sValue} | {BuildingUtils.GetCriminalCount(m_buildingId, building)}";
                        }
                }
            }

            return "0";
        }

        public override string GetValueTooltip()
        {
            switch (m_eBuildingType)
            {
                case BuildingType.Prison:
                case BuildingType.PoliceStation:
                case BuildingType.HelicopterPrison:
                    {
                        return "# of criminals";
                    }
                default:
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                        if (building.m_flags != 0)
                        {
                            return $"Crime buffer per citizen | Criminals\n\nCrime Buffer: {building.m_crimeBuffer}\nCitizen Count: {building.m_citizenCount}\nCriminals: {BuildingUtils.GetCriminalCount(m_buildingId, building)}";
                        }

                        return "Crime buffer per citizen | Criminals";
                    }
            }
        }
    }
}