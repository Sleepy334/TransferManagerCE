using ColossalFramework.Math;
using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;
using UnityEngine;

namespace TransferManagerCE.TransferOffers
{
    internal class HandleServices
    {
        // Copied from CommonBuildingAI.HandleDead
        public static void HandleDead(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour)
        {
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Death);
            if (behaviour.m_deadCount != 0 && Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
            {
                buildingData.m_deathProblemTimer = (byte)Mathf.Min(255, buildingData.m_deathProblemTimer + 1);
                if (buildingData.m_deathProblemTimer >= 128)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Death | Notification.Problem1.MajorProblem);
                }
                else if (buildingData.m_deathProblemTimer >= 64)
                {
                    problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Death);
                }

                int deadCount = behaviour.m_deadCount;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CitiesUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Dead, ref count, ref cargo, ref capacity, ref outside);
                deadCount -= capacity;
                if (deadCount > 0)
                {
                    TransferOffer offer = default;
                    offer.Priority = buildingData.m_deathProblemTimer * 7 / 128;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;
                    offer.Active = false;
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Dead, offer);
                }
            }
            else
            {
                buildingData.m_deathProblemTimer = 0;
            }

            buildingData.m_problems = problemStruct;
        }

        // Copied from CommonBuildingAI.HandleSick
        public static void HandleSick(ushort buildingID, ref Building buildingData, int iSickCount)
        {
            if (buildingData.Info.GetAI() is HospitalAI)
            {
                return;
            }

            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Noise);
            if (iSickCount != 0 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                Singleton<NaturalResourceManager>.instance.CheckPollution(buildingData.m_position, out byte groundPollution);
                Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.NoisePollution, buildingData.m_position, out int local);
                int waterPollutionFactor = buildingData.m_waterPollution * 2;
                int groundPollutionFactor;
                int localFactor;
                if (buildingData.Info.m_class.m_subService == ItemClass.SubService.ResidentialLowEco || buildingData.Info.m_class.m_subService == ItemClass.SubService.ResidentialHighEco)
                {
                    groundPollutionFactor = groundPollution * 200 / 255;
                    localFactor = local * 150 / 255;
                }
                else
                {
                    groundPollutionFactor = groundPollution * 100 / 255;
                    localFactor = local * 100 / 255;
                }

                int totalFactor = waterPollutionFactor >= 35 ? waterPollutionFactor * 2 - 35 : waterPollutionFactor;
                if (totalFactor > 10 && totalFactor > groundPollutionFactor && totalFactor > localFactor)
                {
                    buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, buildingData.m_healthProblemTimer + 1);
                    if (buildingData.m_healthProblemTimer >= 96)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.DirtyWater | Notification.Problem1.MajorProblem);
                    }
                    else if (buildingData.m_healthProblemTimer >= 32)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.DirtyWater);
                    }
                }
                else if (groundPollutionFactor > 10 && groundPollutionFactor > localFactor)
                {
                    buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, buildingData.m_healthProblemTimer + 1);
                    if (buildingData.m_healthProblemTimer >= 96)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Pollution | Notification.Problem1.MajorProblem);
                    }
                    else if (buildingData.m_healthProblemTimer >= 32)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Pollution);
                    }
                }
                else if (localFactor > 10)
                {
                    buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, buildingData.m_healthProblemTimer + 1);
                    if (buildingData.m_healthProblemTimer >= 96)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Noise | Notification.Problem1.MajorProblem);
                    }
                    else if (buildingData.m_healthProblemTimer >= 32)
                    {
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Noise);
                    }
                }
                else
                {
                    // Clear sick problem
                    buildingData.m_healthProblemTimer = 0;
                }
            }
            else
            {
                buildingData.m_healthProblemTimer = 0;
            }

            buildingData.m_problems = problemStruct;

            // NEW CODE TO FIX BUGS STARTS HERE
            // Add outgoing offer if needed
            const int iDEATH_TIMER_VALUE = 120;

            // We send out an offer for sick people even if m_healthProblemTimer=0
            // This way the sick slowly get collected/cured instead of building up in the city.
            if (iSickCount > 0)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                if (buildingData.m_healthProblemTimer >= iDEATH_TIMER_VALUE)
                {
                    // The timer has run out, the citizens are now either recovered or dead.
                    List<uint> cimSick = CitiesUtils.GetSick(buildingID, buildingData);
                    if (cimSick.Count > 0)
                    {
                        foreach (uint sick in cimSick)
                        {
                            // Remove the sick flag
                            ref Citizen citizen = ref CitizenManager.instance.m_citizens.m_buffer[sick];
                            if (citizen.m_flags != 0)
                            {
                                // Remove sick flag
                                citizen.Sick = false;

                                // Add dead flag, 25% chance of dieing
                                if (random.Int32(4U) == 0)
                                {
                                    citizen.Dead = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Add a random delay so it is more realistic
                    // We change the rate of requests depending on timer
                    uint uiRandomDelay = buildingData.m_healthProblemTimer > 0 ? 4U : 8U;
                    if (random.Int32(uiRandomDelay) == 0)
                    {
                        AddSickOffers(buildingID, ref buildingData, iSickCount);
                    }
                }
            }
        }

        public static void AddSickOffers(ushort buildingID, ref Building buildingData, int iBuildingSickCount)
        {
            bool bNaturalDisasters = DependencyUtilities.IsNaturalDisastersDLC();
            int sickCount = iBuildingSickCount;
            int count = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;

            // Ambulances
            CitiesUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Sick, ref count, ref cargo, ref capacity, ref outside);
            sickCount -= capacity;

            // Medical helicopters
            if (bNaturalDisasters)
            {
                CitiesUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Sick2, ref count, ref cargo, ref capacity, ref outside);
                sickCount -= capacity;
            }

            if (sickCount > 0)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                List<uint> cimSick = CitiesUtils.GetSick(buildingID, buildingData);
                if (cimSick.Count > 0)
                {
                    // Select only 1 random sick citizen to request at a time so it is more realistic.
                    // Otherwise we end up with dozens of ambulances showing up at the same time which looks crap.
                    int iCitizen = random.Int32((uint)cimSick.Count);
                    uint citizenId = cimSick[iCitizen];

                    TransferOffer offer = default;
                    offer.Priority = buildingData.m_healthProblemTimer * 7 / 128;
                    offer.Citizen = citizenId;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;

                    // Half the time request Eldercare/Childcare services instead of using a Hospital if the citizen isnt too sick
                    // but only when we arent on the timer.
                    if (buildingData.m_healthProblemTimer == 0 &&
                        random.Int32(2u) == 0 &&
                        ResidentAIFindHospital.RequestEldercareChildcareService(citizenId, offer))
                    {
                        return; // offer sent
                    }

                    // Otherwise request Ambualnce/Helicopter/Walk
                    DistrictManager instance2 = Singleton<DistrictManager>.instance;
                    byte district = instance2.GetDistrict(buildingData.m_position);
                    DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;

                    // Request helicopter or ambulance
                    if (bNaturalDisasters && (servicePolicies & DistrictPolicies.Services.HelicopterPriority) != 0)
                    {
                        instance2.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.HelicopterPriority;
                        offer.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick2, offer);
                    }
                    else if ((buildingData.m_flags & Building.Flags.RoadAccessFailed) != 0)
                    {
                        // No Road Access - request a helicopter or offer to walk 50/50
                        if (bNaturalDisasters && random.Int32(2u) == 0)
                        {
                            // Request a helicopter
                            offer.Active = false;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick2, offer);
                        }
                        else
                        {
                            // Offer to walk
                            offer.Active = true;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick, offer);
                        }
                    }
                    else if (bNaturalDisasters && random.Int32(20u) == 0)
                    {
                        // Request a helicopter occasionally
                        offer.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick2, offer);
                    }
                    else
                    {
                        // Most of the time we ask for an ambulance as it is more fun than walking to hospital
                        // only occasionally offer walking incase their are no ambulances available
                        offer.Active = random.Int32(6u) == 0;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick, offer);
                    }
                }
            }
        }
    }
}
