using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class OutsideConnectionAIGenerateNamePatch
    {
        [HarmonyPatch(typeof(OutsideConnectionAI), "GenerateName")]
        [HarmonyPrefix]
        public static bool GenerateNamePrefix(ushort buildingID, ref InstanceID caller, ref string __result)
        {
            // Has the user overriden the default name
            if (OutsideConnectionSettings.HasSettings(buildingID))
            {
                OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(buildingID);
                if (!string.IsNullOrEmpty(settings.m_name))
                {
                    __result = settings.m_name;
                    return false; // Do not call vanilla
                }
            }

            // We change the caller(seed) to always be the outside connections index. This way the connection
            // will always have the same name which makes it easier to see what is happening with the matches.
            // Note: When we use the buildingId we seem to gtet more name repeats.
            caller = new InstanceID { Building = (ushort) GetIndex(buildingID) };

            // Call base function
            return true;
        }

        public static int GetIndex(ushort buildingId)
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
