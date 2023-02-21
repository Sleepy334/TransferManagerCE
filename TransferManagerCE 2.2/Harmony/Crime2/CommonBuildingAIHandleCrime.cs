using HarmonyLib;
using TransferManagerCE.TransferOffers;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CommonBuildingAIHandleCrime
    {
        // This gets set to reject offers during HandleCrime
        public static bool s_bRejectOffers = false;

        // Crime2 support
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static bool HandleCrimePrefix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager && DependencyUtilities.IsNaturalDisastersDLC())
            {
                s_bRejectOffers = true;
            }

            // Run normal HandleCrime function
            return true;
        }

        // Crime2 support
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyPostfix]
        public static void HandleCrimePostfix(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            // Turn off rejecting Crime offers
            s_bRejectOffers = false;

            if (SaveGameSettings.GetSettings().EnableNewTransferManager && DependencyUtilities.IsNaturalDisastersDLC())
            {
                CrimeHandler.AddCrimeOffer(buildingID, ref data, citizenCount);
            }
        }
    }
}
