using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(OutsideConnectionAI), "AddConnectionOffers")]
    public static class OutsideConnectionAIPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ushort buildingID, ref int cargoCapacity, ref int residentCapacity, ref int touristFactor0, ref int touristFactor1, ref int touristFactor2, ref int dummyTrafficFactor)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager && 
                !DependencyUtilities.IsAdvancedOutsideConnectionsRunning() &&  
                OutsideConnectionSettings.HasSettings(buildingID))
            {
                OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(buildingID);

                cargoCapacity = settings.m_cargoCapacity;
                residentCapacity = settings.m_residentCapacity;
                touristFactor0 = settings.m_touristFactor0;
                touristFactor1 = settings.m_touristFactor1;
                touristFactor2 = settings.m_touristFactor2;
                dummyTrafficFactor = settings.m_dummyTrafficFactor;
            }

            return true;
        }
    }
}
