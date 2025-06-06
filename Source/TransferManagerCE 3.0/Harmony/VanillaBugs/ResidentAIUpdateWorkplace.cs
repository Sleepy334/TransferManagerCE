using ColossalFramework;
using HarmonyLib;
using System;
using System.ComponentModel.Design;
using static RenderManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class ResidentAIUpdateWorkplace
    {
        // We override UpdateWorkplace and check for these issues as they may add transfer offers to the system then get released in UpdateLocation afterwards
        [HarmonyPatch(typeof(ResidentAI), "UpdateWorkplace")]
        [HarmonyPrefix]
        public static bool UpdateWorkplacePrefix(uint citizenID, ref Citizen data)
        {
            // Check if this citizen is about to be released and don't add an offer for it
            if (data.m_homeBuilding == 0 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
            {
#if DEBUG
                //CDebug.Log($"Invalid citizen: {citizenID} All 0");
#endif
                return false;
            }

            
            if (data.CurrentLocation == Citizen.Location.Home && (data.m_flags & Citizen.Flags.MovingIn) != 0)
            {
                // Invalid citizen, @Home with MovingIn flag is set but no valid home
#if DEBUG
                //CDebug.Log($"Invalid citizen: {citizenID} @Home, HomeBuilding:{data.m_homeBuilding} MovingIn:{(data.m_flags & Citizen.Flags.MovingIn) != 0}");
#endif
                return false;
            }

            return true;
        }
    }
}
