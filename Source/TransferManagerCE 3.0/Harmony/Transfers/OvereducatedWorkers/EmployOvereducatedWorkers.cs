using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class EmployOvereducatedWorkersPatch
    {
        // HandleWorkPlaces: We change priority to include building priority
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleWorkPlaces")]
        public static bool HandleWorkPlaces(ushort buildingID, ref Building data, int workPlaces0, int workPlaces1, int workPlaces2, int workPlaces3, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount)
        {
            if (!SaveGameSettings.GetSettings().EmployOverEducatedWorkers)
            {
                return true; // Execute vanilla function
            }

            int iTotalWorkplaces = workPlaces0 + workPlaces1 + workPlaces2 + workPlaces3;
            if (iTotalWorkplaces == 0)
            {
                return false; // Dont run vanilla function
            }

            if (totalWorkerCount >= iTotalWorkplaces || data.m_citizenUnits == 0)
            {
                return false; // Dont run vanilla function
            }

            // Determine priority
            float fPercentFilled = (float)totalWorkerCount / (float)iTotalWorkplaces;

            int iBuildingPriority;
            if (data.m_workerProblemTimer > 0)
            {
                // Timer running set max priority
                iBuildingPriority = 7;
            }
            else
            {
                // Otherwise scale priority based on filled job level, limit to 6 so we save highest priority for building with timer running
                iBuildingPriority = Mathf.Clamp((int)((1.19f - fPercentFilled) * 8.0), 1, 6);
            }
            //CDebug.Log($"buildingID: {buildingID} Workers: {iTotalWorkers} Jobs: {iTotalWorkplaces} Percent: {fPercentFilled} Priority: {iPriority}");

            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
            {
                int availablePlaces = iTotalWorkplaces - totalWorkerCount;
                int workerCount0 = behaviour.m_educated0Count;
                int workerCount1 = behaviour.m_educated1Count;
                int workerCount2 = behaviour.m_educated2Count;
                int workerCount3 = behaviour.m_educated3Count;

                // Over educated workplace requests
                int minJobLevel = 0;
                if (workerCount0 >= workPlaces0)
                {
                    minJobLevel++;
                    if (workerCount1 >= workPlaces1)
                    {
                        minJobLevel++;
                        if (workerCount2 >= workPlaces2)
                        {
                            minJobLevel++;
                        }
                    }
                }
                        
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Building = buildingID;
                offer.Position = data.m_position;
                offer.Priority = 0;
                offer.Amount = 1;

                int level = Singleton<SimulationManager>.instance.m_randomizer.Int32(minJobLevel, 3);
                switch (level)
                {
                    case 0:
                        if (workerCount0 < workPlaces0)
                        {
                            offer.Priority = iBuildingPriority;
                            offer.Amount = Math.Min(availablePlaces, workPlaces0 - workerCount0);
                        }

                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker0, offer);
                        break;
                    case 1:
                        if (workerCount1 < workPlaces1)
                        {
                            offer.Priority = iBuildingPriority;
                            offer.Amount = Math.Min(availablePlaces, workPlaces1 - workerCount1);
                        }

                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker1, offer);
                        break;
                    case 2:
                        if (workerCount2 < workPlaces2)
                        {
                            offer.Priority = iBuildingPriority;
                            offer.Amount = Math.Min(availablePlaces, workPlaces2 - workerCount2);
                        }

                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker2, offer);
                        break;
                    case 3:
                        if (workerCount3 < workPlaces3)
                        {
                            offer.Priority = iBuildingPriority;
                            offer.Amount = Math.Min(availablePlaces, workPlaces3 - workerCount3);
                        }

                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker3, offer);
                        break;
                }

                // Warn about education level if needed
                int num8 = (workPlaces3 * 300 + workPlaces2 * 200 + workPlaces1 * 100) / (iTotalWorkplaces + 1);
                int num9 = (workerCount3 * 300 + workerCount2 * 200 + workerCount1 * 100) / (aliveWorkerCount + 1);
                if (num9 < num8 - 100 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Education))
                {
                    GuideController properties = Singleton<GuideManager>.instance.m_properties;
                    if ((object)properties != null)
                    {
                        int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Education);
                        Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded, ItemClass.Service.Education);
                    }
                }
            }

            return false; // Dont run vanilla function
        }

        // UpdateWorkplace: When EmployOverEducatedWorkers we modiy this function so workers occasionally apply for lower level jobs.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ResidentAI), "UpdateWorkplace")]
        public static bool UpdateWorkplace(uint citizenID, ref Citizen data)
        {
            if (!SaveGameSettings.GetSettings().EmployOverEducatedWorkers)
            {
                return true; // Execute vanilla function
            }

            if (data.m_workBuilding != 0 || data.m_homeBuilding == 0)
            {
                return false; // Dont run vanilla function
            }

            BuildingManager instance = Singleton<BuildingManager>.instance;
            Vector3 position = instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
            int age = data.Age;
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                    if (!data.Education1)
                    {
                        transferReason = TransferManager.TransferReason.Student1;
                    }
                    break;
                case Citizen.AgeGroup.Teen:
                    if (data.Education1 && !data.Education2)
                    {
                        transferReason = TransferManager.TransferReason.Student2;
                    }
                    break;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (data.Education1 && data.Education2 && !data.Education3)
                    {
                        transferReason = TransferManager.TransferReason.Student3;
                    }
                    break;
            }

            if (data.Unemployed != 0 && ((servicePolicies & DistrictPolicies.Services.EducationBoost) == 0 || transferReason != TransferManager.TransferReason.Student3 || age % 5 > 2))
            {
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Citizen = citizenID;
                offer.Position = position;
                offer.Amount = 1;
                offer.Active = true;

                TransferManager.TransferReason reason = TransferManager.TransferReason.None;

                // 50% of the time we apply for lower level jobs.
                if ((uint) data.EducationLevel > 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                {
                    // Apply for lower level jobs
                    int iRandomReason = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint) data.EducationLevel);
                    switch (iRandomReason)
                    {
                        case 0:
                            reason = TransferManager.TransferReason.Worker0;
                            break;
                        case 1:
                            reason = TransferManager.TransferReason.Worker1;
                            break;
                        case 2:
                            reason = TransferManager.TransferReason.Worker2;
                            break;
                    }

                    // Applying for lower level job, only accept it if really needed.
                    // We occasionally set this to 1 so we can still be matched to Jobs with P:1
                    offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(2u); 
                }
                else
                {
                    // Apply for normal level jobs
                    switch (data.EducationLevel)
                    {
                        case Citizen.Education.Uneducated:
                            reason = TransferManager.TransferReason.Worker0;
                            break;
                        case Citizen.Education.OneSchool:
                            reason = TransferManager.TransferReason.Worker1;
                            break;
                        case Citizen.Education.TwoSchools:
                            reason = TransferManager.TransferReason.Worker2;
                            break;
                        case Citizen.Education.ThreeSchools:
                            reason = TransferManager.TransferReason.Worker3;
                            break;
                    }

                    // Applying for job at normal education level, randomize priority to mix up citizen selection but always at least P1 so we can match.
                    // We limit max priority to 4 so the higher priorities are saved for buioldings that need workers pretty badly.
                    offer.Priority = Math.Max(1, Singleton<SimulationManager>.instance.m_randomizer.Int32(5u));
                }

                // Apply for a job
                Singleton<TransferManager>.instance.AddOutgoingOffer(reason, offer);
            }

            // Apply to school
            switch (transferReason)
            {
                case TransferManager.TransferReason.Student3:
                    if ((servicePolicies & DistrictPolicies.Services.SchoolsOut) != 0 && age % 5 <= 1)
                    {
                        break;
                    }
                    goto default;
                default:
                    {
                        TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
                        offer2.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
                        offer2.Citizen = citizenID;
                        offer2.Position = position;
                        offer2.Amount = 1;
                        offer2.Active = true;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(transferReason, offer2);
                        break;
                    }
                case TransferManager.TransferReason.None:
                    break;
            }

            return false; // Dont run vanilla function
        }

    } // endclass
} // end namespace
