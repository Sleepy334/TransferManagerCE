using HarmonyLib;
using TransferManagerCE.TransferOffers;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class PrivateBuildingAISimulationStepPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PrivateBuildingAI), "SimulationStep")]
        public static void PostFix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                if (buildingData.Info.GetAI() is ResidentialBuildingAI)
                {
                    if (SaveGameSettings.GetSettings().OverrideResidentialSickHandler)
                    {
                        // Residential always have sick collected so we don't need to clear timer if turned off.
                        int iSickCount = CitiesUtils.GetSick(buildingID, buildingData).Count;
                        HandleServices.HandleSick(buildingID, ref buildingData, iSickCount);
                    }
                }
                else
                {
                    if (SaveGameSettings.GetSettings().CollectSickFromOtherBuildings || buildingData.m_healthProblemTimer > 0)
                    {
                        // Commercial and Industrial buildings don't put out transfer offers to remove their sick, so they just hang around forever.
                        int iSickCount = CitiesUtils.GetSick(buildingID, buildingData).Count;
                        HandleServices.HandleSick(buildingID, ref buildingData, iSickCount);
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    public static class PlayerBuildingAISimulationStepActivePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        public static void PostFix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager && !(buildingData.Info.GetAI() is HospitalAI))
            {
                if (SaveGameSettings.GetSettings().CollectSickFromOtherBuildings || buildingData.m_healthProblemTimer > 0)
                {
                    int iSickCount = CitiesUtils.GetSick(buildingID, buildingData).Count;
                    HandleServices.HandleSick(buildingID, ref buildingData, iSickCount);
                }
            }
        }
    }
}
