using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class OutsideConnectionAIPatch
    {
        private static bool s_bInAddConnectionOffers = false;

        public static bool IsInAddConnectionOffers
        {
            get { return s_bInAddConnectionOffers; }
        }

        [HarmonyPatch(typeof(OutsideConnectionAI), "AddConnectionOffers")]
        [HarmonyPrefix]
        public static bool AddConnectionOffersPrefix(ushort buildingID, ref int cargoCapacity, ref int residentCapacity, ref int touristFactor0, ref int touristFactor1, ref int touristFactor2, ref int dummyTrafficFactor)
        {
            s_bInAddConnectionOffers = true;

            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                !DependencyUtils.IsAdvancedOutsideConnectionsRunning() &&
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

        [HarmonyPatch(typeof(OutsideConnectionAI), "AddConnectionOffers")]
        [HarmonyPostfix]
        public static void AddConnectionOffersPostfix(ushort buildingID, ref int cargoCapacity, ref int residentCapacity, ref int touristFactor0, ref int touristFactor1, ref int touristFactor2, ref int dummyTrafficFactor)
        {
            s_bInAddConnectionOffers = false;
        }
    }
}
