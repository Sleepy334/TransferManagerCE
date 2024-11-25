using HarmonyLib;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class HospitalAIProduceGoods
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HospitalAI), "ProduceGoods")]
        public static void PostFix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Hospitals don't put out transfer offers to remove their dead, so they just hang around forever.
                DeadHandler.HandleDead(buildingID, ref buildingData, ref behaviour);
            }
        }
    }
}
