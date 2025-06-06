using HarmonyLib;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class PlayerBuildingAISimulationStepActivePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        public static void PostFix(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                SickHandler.HandleSick(__instance, buildingID, ref buildingData, BuildingUtils.GetSickCount(buildingID, buildingData));
            }
            else
            {
                buildingData.m_healthProblemTimer = 0;
            }
        }
    }
}
