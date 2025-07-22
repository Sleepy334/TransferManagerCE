using ColossalFramework.Math;
using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;
using UnityEngine;
using SleepyCommon;

namespace TransferManagerCE
{
    internal class SickHandler
    {
        public const int iSICK_MINOR_PROBLEM_TIMER_VALUE = 64;
        public const int iSICK_MAJOR_PROBLEM_TIMER_VALUE = 128;

        // Copied from CommonBuildingAI.HandleSick
        public static void HandleSick(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, int iSickCount)
        {
            // Check HealthCare is unlocked
            if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                return;
            }

            if (buildingData.Info is not null)
            {
                switch (buildingData.Info.GetService())
                {
                    case ItemClass.Service.HealthCare:
                        {
                            // Do not put out a call for an ambulance from a health care facility
                            return;
                        }
                    case ItemClass.Service.Residential:
                        {
                            // Residential already call handle sick, dont call twice
                            break;
                        }
                    default:
                        {
                            Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                            behaviour.m_sickCount = iSickCount; // The only value actually used
                            CommonBuildingAIHandleSickPatch.HandleSick(buildingID, ref buildingData, ref behaviour, buildingData.m_citizenCount);
                            break;
                        }
                }
            }

            if (iSickCount > 0)
            {
                // We send out an offer for sick people even if m_healthProblemTimer = 0
                // This way the sick slowly get collected/cured instead of building up in the city.
                AddSickOffers(buildingID, ref buildingData, iSickCount);    
            }
        }

        private static void AddSickOffers(ushort buildingID, ref Building buildingData, int iBuildingSickCount)
        {
            Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

            // Increase request rate after timer gets to 96
            uint randomRate = (buildingData.m_healthProblemTimer > iSICK_MAJOR_PROBLEM_TIMER_VALUE) ? 2U : 3U;

            // Add a random delay so it is more realistic
            if (iBuildingSickCount > 0 &&
                random.Int32(randomRate) == 0)
            {
                bool bNaturalDisasters = DependencyUtils.IsNaturalDisastersDLC();
                
                List<uint> cimSick = BuildingUtils.GetSickWithoutVehicles(buildingID, buildingData);
                if (cimSick.Count > 0)
                {
                    // Select only 1 random sick citizen to request at a time so it is more realistic.
                    // Otherwise we end up with dozens of ambulances showing up at the same time which looks crap.
                    int iCitizenIndex = random.Int32((uint)cimSick.Count);
                    uint citizenId = cimSick[iCitizenIndex];

                    // Added support for the nursing home mod which also patches FindHospital
                    if (IsInNursingHomeAndNotTooSick(citizenId, buildingID))
                    {
                        return;
                    }

                    TransferOffer offer = default;
                    offer.Priority = GetPriority(buildingData.m_healthProblemTimer);
                    offer.Citizen = citizenId;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;

                    // Half the time request Eldercare/Childcare services instead of using a Hospital if the citizen isnt too sick
                    // but only when we arent having a major problem
                    if (buildingData.m_healthProblemTimer < iSICK_MAJOR_PROBLEM_TIMER_VALUE &&
                        random.Int32(2u) == 0 &&
                        RequestEldercareChildcareService(citizenId, ref offer))
                    {
                        return; // offer sent
                    }

                    // Otherwise request Ambulance/Helicopter/Walk
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
                    else if (bNaturalDisasters && random.Int32(100u) <= SaveGameSettings.GetSettings().SickHelicopterRate)
                    {
                        // Request a helicopter occasionally
                        offer.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick2, offer);
                    }
                    else
                    {
                        // Most of the time we ask for an ambulance as it is more fun than walking to hospital
                        // only occasionally offer walking incase there are no ambulances available
                        offer.Active = random.Int32(100u) <= SaveGameSettings.GetSettings().SickWalkRate;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferReason.Sick, offer);
                    }
                }
            } 
        }

        // Added support for the nursing home mod which tries to patch the same function
        private static bool IsInNursingHomeAndNotTooSick(uint citizenID, ushort sourceBuilding)
        {
            if (DependencyUtils.IsSeniorCitizenCenterModRunning() &&
                Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare) &&
                Singleton<CitizenManager>.exists &&
                Singleton<CitizenManager>.instance is not null)
            {
                Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];
                if (IsSenior(citizen.Age) && citizen.m_flags != 0 && sourceBuilding == citizen.m_homeBuilding && citizen.m_health >= 40)
                {
                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizen.m_homeBuilding];
                    if (building.Info is not null)
                    {
                        return building.Info.GetAI().GetType().ToString().Contains("NursingHomeAI");
                    }
                }
            }
            return false;
        }

        public static bool RequestEldercareChildcareService(uint citizenID, ref TransferOffer offer)
        {
            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];

            if (citizen.m_health >= 40 && (IsChild(citizen.Age) || IsSenior(citizen.Age)))
            {
                TransferReason reason = TransferManager.TransferReason.None;

                FastList<ushort> serviceBuildings = Singleton<BuildingManager>.instance.GetServiceBuildings(ItemClass.Service.HealthCare);
                for (int i = 0; i < serviceBuildings.m_size; i++)
                {
                    BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]].Info;
                    if ((object)info is not null)
                    {
                        if (IsChild(citizen.Age) && info.m_class.m_level == ItemClass.Level.Level4)
                        {
                            reason = TransferReason.ChildCare;
                            break;
                        }
                        else if (IsSenior(citizen.Age) && info.m_class.m_level == ItemClass.Level.Level5)
                        {
                            reason = TransferReason.ElderCare;
                            break;
                        }
                    }
                }

                // Send request if we found a Childcare/Eldercare facility
                if (reason != TransferReason.None)
                {
                    // WARNING: Childcare and Eldercare need an IN offer
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer(reason, offer);
                    return true;
                }
            }

            return false;
        }

        private static bool IsChild(int Age)
        {
            switch (Citizen.GetAgeGroup(Age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    {
                        return true;
                    }
            }   

            return false;
        }

        private static bool IsSenior(int Age)
        {
            return Citizen.GetAgeGroup(Age) == Citizen.AgeGroup.Senior;
        }

        public static void ClearSickTimerForNonResidential()
        {
            if (!SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                // Clear non-residential building sick timer as the vanilla sick handler doesnt deal with these buildings.
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

                for (int i = 0; i < BuildingBuffer.Length; i++)
                {
                    ref Building building = ref BuildingBuffer[i];

                    if (building.m_healthProblemTimer > 0 &&
                        building.Info is not null &&
                        building.Info.GetService() != ItemClass.Service.Residential)
                    {
                        // Clear sick timer
                        building.m_healthProblemTimer = 0;

                        // Clear building problems
                        building.m_problems = Notification.RemoveProblems(building.m_problems, Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Noise);
                        
                        // Make sure to update building notifications as well
                        Singleton<BuildingManager>.instance.UpdateBuildingRenderer((ushort)i, updateGroup: true);
                    }
                }
            }
        }

        public static int GetPriority(int sickTimer)
        {
            if (SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                return Mathf.Clamp(sickTimer * 7 / iSICK_MAJOR_PROBLEM_TIMER_VALUE, 0, 7);
            }
            else
            {
                return Mathf.Clamp(sickTimer * 7 / 96, 0, 7); // 96 is major problem point
            }
        }
    }
}
