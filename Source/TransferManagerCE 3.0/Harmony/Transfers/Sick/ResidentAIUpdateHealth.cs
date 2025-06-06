using HarmonyLib;
using ColossalFramework;
using UnityEngine;

namespace TransferManagerCE
{
    // We can randomly make people sick so the hospitals have something to do.
    [HarmonyPatch]
    public static class ResidentAIUpdateHealth
    {
        [HarmonyPatch(typeof(ResidentAI), "UpdateHealth")]
        [HarmonyPostfix]
        public static void UpdateHealth(uint citizenID, ref Citizen data, ref bool __result)
        {
            // Only apply if we have unlocked hospitals
            if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                // __result is true when cim has been killed.
                if (!__result && data.m_homeBuilding != 0 && !data.Dead && !data.Sick)
                {
                    uint randomSickRate = SaveGameSettings.GetSettings().RandomSickRate;
                    if (randomSickRate > 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(randomSickRate) == 0)
                    {
                        // 10 = Really sick, 50 = Well enough that they can go to ElderCare or ChildCare to get well.
                        data.m_health = (byte) Mathf.Min(data.m_health, Singleton<SimulationManager>.instance.m_randomizer.Int32(10, 50));
                        data.Sick = true;
                    }
                }
            }
        }
    }
}