using ColossalFramework;
using HarmonyLib;
using System;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class PrivateBuildingAISimulationStepPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PrivateBuildingAI), "SimulationStep")]
        public static void SimulationStepPostFix(PrivateBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideSickHandler &&
                Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                // If we are using our sick handler, we disable the major problem timer so
                // that we can run the sick timer all the way to 255.
                if (buildingData.m_healthProblemTimer > 0)
                { 
                    buildingData.m_majorProblemTimer = 0;
                }

                // Call our sick handler
                SickHandler.HandleSick(__instance, buildingID, ref buildingData, BuildingUtils.GetSickCount(buildingID, buildingData));
            }
        }
    }
}
