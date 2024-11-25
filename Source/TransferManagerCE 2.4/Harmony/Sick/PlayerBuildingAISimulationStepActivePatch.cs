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
                (SaveGameSettings.GetSettings().CollectSickFromOtherBuildings || buildingData.m_healthProblemTimer > 0) && 
                buildingData.Info.GetAI() is not HospitalAI)
            {
                SickHandler.HandleSick(__instance, buildingID, ref buildingData, BuildingUtils.GetSickCount(buildingID, buildingData));
            }
        }
    }
}
