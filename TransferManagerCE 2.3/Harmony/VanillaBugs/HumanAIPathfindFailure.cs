using ColossalFramework;
using HarmonyLib;
using System.Diagnostics;
using TransferManagerCE.UI;
using TransferManagerCE.Util;
using static RenderManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class HumanAIPathfindFailure
    {
        public static int s_pathFailCount = 0;
        private static uint s_citizenId = 0;

        [HarmonyPatch(typeof(HumanAI), "PathfindFailure")]
        [HarmonyPrefix]
        public static bool PathfindFailurePrefix(ushort instanceID, ref CitizenInstance data)
        {
            s_citizenId = data.m_citizen;
            s_pathFailCount++;
#if DEBUG
            // We currently only add these path fails in DEBUG as there are usually so many of them.
            Citizen cim = CitizenManager.instance.m_citizens.m_buffer[s_citizenId];
            if (cim.m_flags != 0 && data.m_targetBuilding != 0)
            {
                InstanceID target;
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                {
                    target = new InstanceID {  NetNode = data.m_targetBuilding };
                }
                else
                {
                    target = new InstanceID { Building = data.m_targetBuilding };
                }
                PathFindFailure.RecordPathFindFailure(new InstanceID { Building = data.m_sourceBuilding }, target);
            }
#endif
            return true;
        }

        [HarmonyPatch(typeof(HumanAI), "PathfindFailure")]
        [HarmonyPostfix]
        public static void PathfindFailurePostfix()
        {
            if (s_citizenId != 0)
            {
                Citizen cim = Singleton<CitizenManager>.instance.m_citizens.m_buffer[s_citizenId];
                if ((cim.m_flags & Citizen.Flags.MovingIn) != 0)
                {
                    // We have had a path fail on a citizen trying to move into the city. remove it now
                    Singleton<CitizenManager>.instance.ReleaseCitizen(s_citizenId);
#if DEBUG
                    Debug.Log($"Removing citizen:{s_citizenId} - MovingIn flag still set");
#endif
                }
            }

            s_citizenId = 0;
        }
    }
}
