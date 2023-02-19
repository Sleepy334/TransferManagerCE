using HarmonyLib;
using TransferManagerCE.TransferOffers;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CommonBuildingAIHandleCrime
    {
        // Crime2 support
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static bool HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager && DependencyUtilities.IsNaturalDisastersDLC())
            {
                // Call our Crime2 version of the function
                CrimeHandler.HandleCrime(buildingID, ref data, crimeAccumulation, citizenCount);
                return false;
            }

            // Run normal HandleCrime function
            return true;
        }
    }
}
