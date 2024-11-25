using HarmonyLib;
using System.Runtime.CompilerServices;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CommonBuildingAIHandleSickPatch
    {
        public static void HandleSick(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, int iSickCount)
        {
            // Call our reverse patch,
            // but only if not a ResidentialBuildingAI as it is already called in ResidentialBuildingAI.SimulationStepActive
            if (__instance is not ResidentialBuildingAI && __instance is not HospitalAI)
            {
                Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                behaviour.m_sickCount = iSickCount; // The only value actually used
                HandleSick(__instance, buildingID, ref buildingData, ref behaviour, buildingData.m_citizenCount);
            }
        }

        [HarmonyPatch(typeof(CommonBuildingAI), "HandleSick")]
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void HandleSick(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            Debug.LogError("CommonBuildingAI.HandleSick was not applied.");
            throw new System.NotImplementedException();
        }
    }
}
