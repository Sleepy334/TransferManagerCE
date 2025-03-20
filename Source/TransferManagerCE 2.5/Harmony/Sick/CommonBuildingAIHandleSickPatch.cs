using ColossalFramework;
using Epic.OnlineServices.Presence;
using HarmonyLib;
using ICities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Notification;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CommonBuildingAIHandleSickPatch
    {
        const int iDEATH_TIMER_VALUE = 255;

        [HarmonyPatch(typeof(CommonBuildingAI), "HandleSick")]
        [HarmonyPrefix]
        public static bool HandleSick(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Noise);
                if (behaviour.m_sickCount != 0 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
                {
                    // We always update the health timer while there are sick in the building.
                    buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, buildingData.m_healthProblemTimer + 1);

                    // Try and set problem
                    if (buildingData.Info.GetService() == ItemClass.Service.Residential)
                    {
                        Singleton<NaturalResourceManager>.instance.CheckPollution(buildingData.m_position, out var groundPollution);
                        Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.NoisePollution, buildingData.m_position, out var local);
                        int num = buildingData.m_waterPollution * 2;
                        int num2;
                        int num3;
                        if (buildingData.Info.m_class.m_subService == ItemClass.SubService.ResidentialLowEco || buildingData.Info.m_class.m_subService == ItemClass.SubService.ResidentialHighEco)
                        {
                            num2 = groundPollution * 200 / 255;
                            num3 = local * 150 / 255;
                        }
                        else
                        {
                            num2 = groundPollution * 100 / 255;
                            num3 = local * 100 / 255;
                        }
                        int num4 = 0;
                        num4 = ((num >= 35) ? (num * 2 - 35) : num);
                        if (num4 > 10 && num4 > num2 && num4 > num3)
                        {
                            if (buildingData.m_healthProblemTimer >= SickHandler.iSICK_MAJOR_PROBLEM_TIMER_VALUE)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.DirtyWater | Notification.Problem1.MajorProblem);
                            }
                            else if (buildingData.m_healthProblemTimer >= SickHandler.iSICK_MINOR_PROBLEM_TIMER_VALUE)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.DirtyWater);
                            }
                        }
                        else if (num2 > 10 && num2 > num3)
                        {
                            if (buildingData.m_healthProblemTimer >= SickHandler.iSICK_MAJOR_PROBLEM_TIMER_VALUE)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Pollution | Notification.Problem1.MajorProblem);
                            }
                            else if (buildingData.m_healthProblemTimer >= SickHandler.iSICK_MINOR_PROBLEM_TIMER_VALUE)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Pollution);
                            }
                        }
                        else if (num3 > 10)
                        {
                            if (buildingData.m_healthProblemTimer >= SickHandler.iSICK_MAJOR_PROBLEM_TIMER_VALUE)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Noise | Notification.Problem1.MajorProblem);
                            }
                            else if (buildingData.m_healthProblemTimer >= SickHandler.iSICK_MINOR_PROBLEM_TIMER_VALUE)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Noise);
                            }
                        }
                    }
                }
                else
                {
                    buildingData.m_healthProblemTimer = 0;
                }

                buildingData.m_problems = problemStruct;

                // Don't run vanilla
                return false;
            }

            // Run vanilla function
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CommonBuildingAI), "SimulationStepActive")]
        public static void SimulationStepActivePostfix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideSickHandler &&
                Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                // If timer runs out then kill remaining sick cims as punishment
                if (buildingData.m_healthProblemTimer >= iDEATH_TIMER_VALUE)
                {
                    List<uint> cimSick = BuildingUtils.GetSick(buildingID, buildingData);
                    int iSickCount = cimSick.Count;
                    if (iSickCount > 0)
                    {
                        // Kill half the sick cims as punishment
                        foreach (uint sickId in cimSick)
                        {
                            ref Citizen citizen = ref CitizenManager.instance.m_citizens.m_buffer[sickId];
                            if (citizen.m_flags != 0)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                {
                                    Die(sickId, ref citizen); // Kill cim
                                }
                                else
                                {
                                    citizen.Sick = false; // This cim survived the illness
                                }
                            } 
                        }

                        // We also sometimes abandon residential building if it has an actual sick problem or there are so many sick the system cant keep up.
                        if (buildingData.Info is not null &&
                            buildingData.Info.GetService() == ItemClass.Service.Residential &&
                            (buildingData.m_problems & (Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Noise)).IsNotNone &&
                            (Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0 || iSickCount > 5))
                        {
                            buildingData.m_majorProblemTimer = 64; // Setting this to 64 will trigger an abandonment.
                        }
                    }

                    // Reset counter now we have punished the building.
                    buildingData.m_healthProblemTimer = 0;
                }
            }
        }

        private static void Die(uint citizenID, ref Citizen data)
        {
            data.Sick = false;
            data.Dead = true;
            data.SetParkedVehicle(citizenID, 0);
            if ((data.m_flags & Citizen.Flags.MovingIn) != 0)
            {
                return;
            }
            ushort num = data.GetBuildingByLocation();
            if (num == 0)
            {
                num = data.m_homeBuilding;
            }
            if (num != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[num].m_position;
                byte district = instance.GetDistrict(position);
                instance.m_districts.m_buffer[district].m_deathData.m_tempCount++;
                if (IsSenior(citizenID))
                {
                    instance.m_districts.m_buffer[district].m_deadSeniorsData.m_tempCount++;
                    instance.m_districts.m_buffer[district].m_ageAtDeathData.m_tempCount += (uint)data.Age;
                }
            }
        }

        private static bool IsSenior(uint citizenID)
        {
            return Citizen.GetAgeGroup(Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].Age) == Citizen.AgeGroup.Senior;
        }
    }
}
