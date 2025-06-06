using HarmonyLib;

namespace TransferManagerCE
{
    // The Building.m_citizenCount field is very buggy and it is very slow to calculate the citizens for all buildings
    // so we cache the value passed into the HandleCrime functions for each building type.
    [HarmonyPatch]
    public class CrimeCitizenCountPatches
    {
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void CommonBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(MainIndustryBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void MainIndustryBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(MainCampusBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void MainCampusBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(AirportBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void AirportBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(AirportCargoGateAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void AirportCargoGateAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(AirportEntranceAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void AirportEntranceAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(AirportGateAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void AirportGateAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(CampusBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void CampusBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(IndustryBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void IndustryBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(MuseumAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void MuseumAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(ParkBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void ParkBuildingAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }

        [HarmonyPatch(typeof(ParkGateAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void ParkGateAIHandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            CrimeCitizenCountStorage.SetCitizenCount(buildingID, data, citizenCount);
        }
    }
}