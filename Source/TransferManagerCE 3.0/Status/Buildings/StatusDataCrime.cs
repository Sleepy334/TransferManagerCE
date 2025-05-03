using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCrime : StatusDataBuilding
    {
        public const int iMAIN_BUILDING_MAJOR_CRIME_RATE = 120;
        public const int iMAIN_BUILDING_MINOR_CRIME_RATE = 80;
        public const int iMAJOR_CRIME_RATE = 90;
        public const int iMINOR_CRIME_RATE = 60;

        public StatusDataCrime(TransferReason reason, BuildingType eBuildingType, ushort BuildingId) : 
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                switch (m_eBuildingType)
                {
                    case BuildingType.Prison:
                    case BuildingType.PoliceStation:
                    case BuildingType.HelicopterPrison:
                        {
                            // Prison Helicopter Mod
                            tooltip = "# of criminals";
                            return BuildingUtils.GetCriminalsAtPoliceStation(m_buildingId, building).ToString();
                        }
                    case BuildingType.MainCampusBuilding:
                    case BuildingType.MainIndustryBuilding:
                        {
                            // MainIndustryBuildingAI add 100 to citizen count, no idea why
                            string sValue = "";

                            int iCitizenCount = CrimeCitizenCountStorage.GetCitizenCount(m_buildingId, building);
                            if (iCitizenCount > 0)
                            {
                                iCitizenCount += 100;

                                int iCrimeRatePerCitizen = building.m_crimeBuffer / iCitizenCount;
                                sValue += SleepyCommon.Utils.MakePercent(iCrimeRatePerCitizen, iMAIN_BUILDING_MAJOR_CRIME_RATE, 1);

                                // Highlight status if crime is urgent.
                                WarnText(false, true, iCrimeRatePerCitizen, iMAIN_BUILDING_MAJOR_CRIME_RATE);

                                tooltip = $"Crime Rate per Citizen: {building.m_crimeBuffer / iCitizenCount} / {iMAIN_BUILDING_MAJOR_CRIME_RATE}\n" +
                                          $"Priority: {Mathf.Clamp(building.m_crimeBuffer / Mathf.Max(1, iCitizenCount * 10), 0, 7)}\n" +
                                          $"{MakeTooltip(building.m_crimeBuffer)}\n" +
                                          $"Crime Citizen Count: {iCitizenCount}\n" +
                                          $"Criminals: {BuildingUtils.GetCriminalCount(m_buildingId, building)}";
                            }
                            else
                            {
                                tooltip = MakeTooltip(building.m_crimeBuffer);
                                sValue += building.m_crimeBuffer;
                            }

                            return $"{sValue} | {BuildingUtils.GetCriminalCount(m_buildingId, building)}";
                        }
                    default:
                        {
                            string sValue = "";

                            int iCitizenCount = CrimeCitizenCountStorage.GetCitizenCount(m_buildingId, building);
                            if (iCitizenCount > 0)
                            {
                                int iCrimeRatePerCitizen = building.m_crimeBuffer / iCitizenCount;
                                sValue += SleepyCommon.Utils.MakePercent(iCrimeRatePerCitizen, iMAJOR_CRIME_RATE, 1);

                                // Highlight status if crime is urgent.
                                WarnText(false, true, iCrimeRatePerCitizen, iMAJOR_CRIME_RATE);

                                tooltip = $"Crime Rate per Citizen: {building.m_crimeBuffer / iCitizenCount} / {iMAJOR_CRIME_RATE}\n" +
                                          $"Priority: {Mathf.Clamp(building.m_crimeBuffer / Mathf.Max(1, iCitizenCount * 10), 0, 7)}\n" +
                                          $"{MakeTooltip(building.m_crimeBuffer)}\n" +
                                          $"Crime Citizen Count: {iCitizenCount}\n" +
                                          $"Criminals: {BuildingUtils.GetCriminalCount(m_buildingId, building)}";
                            }
                            else
                            {
                                tooltip = MakeTooltip(building.m_crimeBuffer);
                                sValue += building.m_crimeBuffer;
                            }

                            return $"{sValue} | {BuildingUtils.GetCriminalCount(m_buildingId, building)}";
                        }
                }
            }

            tooltip = "";
            return "0";
        }
    }
}