using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(OutsideConnectionAI), "GenerateName")]
    public static class OutsideConnectionAIGenerateNamePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort buildingID, InstanceID caller, ref string __result)
        {
            if (!DependencyUtils.IsAdvancedOutsideConnectionsRunning())
            {
                if (OutsideConnectionSettings.HasSettings(buildingID))
                {
                    OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(buildingID);

                    // DO NOT CALL GetName here as it will stack overflow!
                    if (!string.IsNullOrEmpty(settings.m_name))
                    {
                        __result = settings.m_name;
                        return;
                    }
                }

                // Add the outside connection number on the end to help differentiate
                __result = $"{__result} #{GetPosition(buildingID)}";
            }
        }

        private static int GetPosition(ushort buildingId)
        {
            int iPosition = 0;
            foreach (ushort outsideId in BuildingManager.instance.GetOutsideConnections())
            {
                iPosition++;
                if (buildingId == outsideId)
                {
                    break;
                }
            }

            return iPosition;
        }
    }
}
