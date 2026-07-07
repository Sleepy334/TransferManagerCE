using HarmonyLib;

namespace TransferManagerCore
{
    // ------------------------------------------------------------------------
    [HarmonyPatch]
    public static class SickHandlerPatch
    {
        // --------------------------------------------------------------------
        // Bypass the base games FindHospital function with our own fixed version if requested (OverrideResidentialSickHandler)
        [HarmonyPatch(typeof(ResidentAI), "FindHospital")]
        [HarmonyPrefix]
        public static bool Prefix(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason, ref bool __result)
        {
            if (sourceBuilding != 0 &&
                SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                // Bypass vanilla function as we will handle building collection ourselves
                __result = true;
                return false;
            }

            // Fall through to default ResidentAI.FindHospital
            return true;
        }
    }
}