using System;
using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class MaxMailPatch
    {
        // protected int HandleCommonConsumption(ushort buildingID, ref Building data, ref Building.Frame frameData, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation,
        //                                          ref int mailAccumulation, int maxMail, DistrictPolicies.Services policies, ushort mainBuildingID)
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCommonConsumption",
            new Type[] { typeof(ushort), typeof(Building), typeof(Building.Frame), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(DistrictPolicies.Services), typeof(ushort) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static void HandleCommonConsumption(CommonBuildingAI __instance, ushort buildingID, ref Building data, ref Building.Frame frameData, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, 
                                                                                ref int sewageAccumulation, ref int garbageAccumulation, ref int mailAccumulation, int maxMail, DistrictPolicies.Services policies, ushort mainBuildingID, ref int __result)
        {
            switch (__instance)
            {
                case ParkGateAI parkgateAI:
                    {
                        // Vanilla bug fix: Park gates dont have proper worker and visitor counts so the normal mailAccumulation calculation doesnt work
                        // We create an approximation here using the production rate so that we still have post vehicles come and pick up mail.
                        if (mailAccumulation == 0 &&
                            data.m_fireIntensity == 0 &&
                            (data.m_flags & Building.Flags.Evacuating) == 0)
                        {
                            int budget = parkgateAI.GetBudget(buildingID, ref data);
                            int productionRate = data.m_productionRate;
                            productionRate = ParkGateAI.GetProductionRate(productionRate, budget);
                            mailAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint) productionRate);
                        }
                        
                        maxMail = Mathf.Max(maxMail, SaveGameSettings.GetSettings().MainBuildingMaxMail);
                        break;
                    }
                case AirportEntranceAI:
                case MainIndustryBuildingAI:
                case MainCampusBuildingAI:
                    {
                        // Override maxMail with user requested buffer size (if larger)
                        maxMail = Mathf.Max(maxMail, SaveGameSettings.GetSettings().MainBuildingMaxMail);
                        break;
                    }
            }
        }
    }
}
