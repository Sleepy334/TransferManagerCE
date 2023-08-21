using ColossalFramework.Math;
using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    internal class SickHandler
    {
        // Copied from CommonBuildingAI.HandleSick
        public static void HandleSick(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, int iSickCount)
        {
            // Call our reverse patch of CommonBuildingAI.HandleSick
            CommonBuildingAIHandleSickPatch.HandleSick(__instance, buildingID, ref buildingData, iSickCount);

            // Add outgoing offer if needed
            const int iDEATH_TIMER_VALUE = 120;

            // We send out an offer for sick people even if m_healthProblemTimer = 0
            // This way the sick slowly get collected/cured instead of building up in the city.
            if (iSickCount > 0)
            {
                if (buildingData.m_healthProblemTimer >= iDEATH_TIMER_VALUE)
                {
                    // The timer has run out, the citizens are now either recovered or dead.
                    List<uint> cimSick = BuildingUtils.GetSick(buildingID, buildingData);
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
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(4U) == 0)
                                {
                                    citizen.Dead = true;
                                }
                            }
                        }
                    }
                }

                AddSickOffers(buildingID, ref buildingData, iSickCount);    
            }
        }

        private static void AddSickOffers(ushort buildingID, ref Building buildingData, int iBuildingSickCount)
        {
            // We change the rate of requests depending on timer
            uint uiRandomDelay = buildingData.m_healthProblemTimer > 0 ? 3U : 6U;

            // Add a random delay so it is more realistic
            if (iBuildingSickCount > 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(uiRandomDelay) == 0)
            {
                bool bNaturalDisasters = DependencyUtils.IsNaturalDisastersDLC();
                int sickCount = iBuildingSickCount;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;

                // Ambulances
                BuildingUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Sick, ref count, ref cargo, ref capacity, ref outside);
                sickCount -= capacity;

                // Medical helicopters
                if (bNaturalDisasters)
                {
                    BuildingUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Sick2, ref count, ref cargo, ref capacity, ref outside);
                    sickCount -= capacity;
                }

                if (sickCount > 0)
                {
                    Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                    List<uint> cimSick = BuildingUtils.GetSick(buildingID, buildingData);
                    if (cimSick.Count > 0)
                    {
                        // Select only 1 random sick citizen to request at a time so it is more realistic.
                        // Otherwise we end up with dozens of ambulances showing up at the same time which looks crap.
                        int iCitizenIndex = random.Int32((uint)cimSick.Count);
                        uint citizenId = cimSick[iCitizenIndex];

                        TransferOffer offer = default;
                        offer.Priority = buildingData.m_healthProblemTimer * 7 / 96; // 96 is major problem point
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
                            offer.Active = random.Int32(10u) == 0;
                            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick, offer);
                        }
                    }
                }
            } 
        }
    }
}
