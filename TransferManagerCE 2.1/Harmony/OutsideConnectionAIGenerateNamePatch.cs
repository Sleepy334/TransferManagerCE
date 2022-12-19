using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(OutsideConnectionAI), "GenerateName")]
    public static class OutsideConnectionAIGenerateNamePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort buildingID, InstanceID caller, ref string __result)
        {
            if (!DependencyUtilities.IsAdvancedOutsideConnectionsRunning() && 
                OutsideConnectionSettings.HasSettings(buildingID))
            {
                OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(buildingID);
                if (!string.IsNullOrEmpty(settings.m_name))
                {
                    __result = settings.m_name;
                }
            }
        }
    }
}
