using HarmonyLib;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class PrivateBuildingAISimulationStepPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PrivateBuildingAI), "SimulationStep")]
        public static void PostFix(PrivateBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                if (buildingData.Info.GetAI() is ResidentialBuildingAI)
                {
                    if (SaveGameSettings.GetSettings().OverrideResidentialSickHandler)
                    {
                        // Residential always have sick collected so we don't need to clear timer if turned off.
                        SickHandler.HandleSick(__instance, buildingID, ref buildingData, BuildingUtils.GetSickCount(buildingID, buildingData));
                    }
                }
                else
                {
                    if (SaveGameSettings.GetSettings().CollectSickFromOtherBuildings || buildingData.m_healthProblemTimer > 0)
                    {
                        // Commercial and Industrial buildings don't put out transfer offers to remove their sick, so they just hang around forever.
                        SickHandler.HandleSick(__instance, buildingID, ref buildingData, BuildingUtils.GetSickCount(buildingID, buildingData));
                    }
                }
            }
        }
    }
}
