using HarmonyLib;

namespace TransferManagerCE
{
    [HarmonyPatch]
    internal class DistrictEventPatches
    {
        // WARNING: Performance critical, called thousands of times when rendering districts.
        [HarmonyPatch(typeof(DistrictManager), "ReleaseDistrictImplementation")]
        [HarmonyPostfix]
        public static void ReleaseDistrictImplementationPostfix(byte district, ref District data)
        {
            // We just set the invalid flag so we dont slow down the system
            BuildingSettingsStorage.Invalidate();
        }

        // WARNING: Performance critical, called thousands of times when rendering districts.
        [HarmonyPatch(typeof(DistrictManager), "ReleaseParkImplementation")]
        [HarmonyPostfix]
        public static void ReleaseParkImplementationPostfix(byte park, ref DistrictPark data)
        {
            // We just set the invalid flag so we dont slow down the system
            BuildingSettingsStorage.Invalidate();
        }
    }
}
