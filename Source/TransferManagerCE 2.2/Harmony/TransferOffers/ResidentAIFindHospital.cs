using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class ResidentAIFindHospital
    {
        // Overwrite the base games FindHospital function with our own fixed version.
        [HarmonyPatch(typeof(ResidentAI), "FindHospital")]
        [HarmonyPrefix]

        // We need to patch this function before the Nursing Home mod gets at it otherwise it will break our code.
        [HarmonyBefore(new string[] {"t1a2l.SeniorCitizenCenterMod", "t1a2l.CimCareMod" })]
        public static bool Prefix(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason, ref bool __result)
        {
            if (IsInNursingHomeAndNotTooSick(citizenID, sourceBuilding))
            {
                // We pretend we have successfully sent out an offer but we actually don't need to
                __result = true;
            }
            else if (SaveGameSettings.GetSettings().EnableNewTransferManager && SaveGameSettings.GetSettings().OverrideResidentialSickHandler)
            {
                if (sourceBuilding == 0)
                {
                    // Call our bug fixed version of the function
                    __result = FindHospital(citizenID, sourceBuilding, reason);
                }
                else
                {
                    // Bypass vanilla function as we will handle building collection ourselves
                    __result = true;
                }
            }
            else
            {
                // Call our bug fixed version of the function
                __result = FindHospital(citizenID, sourceBuilding, reason);
            }

            // Always return false as we don't want to run the buggy vanilla function
            return false; 
        }

        // Added support for the nursing home mod which tries to patch the same function
        private static bool IsInNursingHomeAndNotTooSick(uint citizenID, ushort sourceBuilding)
        {
            if (DependencyUtils.IsSeniorCitizenCenterModRunning() &&
                Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare) &&
                Singleton<CitizenManager>.exists && 
                Singleton<CitizenManager>.instance != null &&
                IsSenior(citizenID))
            {
                Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];
                if (citizen.m_flags != 0 && sourceBuilding == citizen.m_homeBuilding && citizen.m_health >= 40)
                {
                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizen.m_homeBuilding];
                    if (building.Info != null)
                    {
                        return building.Info.GetAI().name.Equals("NursingHomeAI");
                    }
                }
            }
            return false;
        }

        // There is a bug in ResidentAI.FindHospital where it adds Childcare and Eldercare offers as AddOutgoingOffer half the time when it should always be AddIncomingOffer for a citizen
        private static bool FindHospital(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            if (reason == TransferManager.TransferReason.Dead)
            {
                if (Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
                {
                    return true;
                }

                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                return false;
            }

            if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                DistrictManager instance2 = Singleton<DistrictManager>.instance;
                Vector3 position = instance.m_buildings.m_buffer[sourceBuilding].m_position;
                byte district = instance2.GetDistrict(position);
                DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;

                // Add a transfer offer
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = 6;
                offer.Citizen = citizenID;
                offer.Position = position;
                offer.Amount = 1;

                // Half the time request Eldercare/Childcare services instead of using a Hospital if the citizen isnt too sick
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0 && 
                    RequestEldercareChildcareService(citizenID, offer))
                {
                    return true; // offer sent
                }

                // Add a Sick or Sick2 outgoing offer instead
                bool bNaturalDisasters = DependencyUtils.IsNaturalDisastersDLC();
                if (bNaturalDisasters && (servicePolicies & DistrictPolicies.Services.HelicopterPriority) != 0)
                {
                    instance2.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.HelicopterPriority;
                    offer.Active = false;
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick2, offer);
                }
                else if (bNaturalDisasters && ((instance.m_buildings.m_buffer[sourceBuilding].m_flags & Building.Flags.RoadAccessFailed) != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(20u) == 0))
                {
                    offer.Active = false;
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick2, offer);
                }
                else
                {
                    offer.Active = (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0);
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick, offer);
                }

                return true;
            }

            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
            return false;
        }

        public static bool RequestEldercareChildcareService(uint citizenID, TransferManager.TransferOffer offer)
        {
            if (Singleton<CitizenManager>.exists &&
                Singleton<CitizenManager>.instance != null &&
                Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].m_health >= 40 &&
                (IsChild(citizenID) || IsSenior(citizenID)))
            {
                TransferManager.TransferReason reason = TransferManager.TransferReason.None;
                FastList<ushort> serviceBuildings = Singleton<BuildingManager>.instance.GetServiceBuildings(ItemClass.Service.HealthCare);
                for (int i = 0; i < serviceBuildings.m_size; i++)
                {
                    BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]].Info;
                    if ((object)info != null)
                    {
                        if (IsChild(citizenID) && info.m_class.m_level == ItemClass.Level.Level4)
                        {
                            reason = TransferManager.TransferReason.ChildCare;
                            break;
                        }
                        else if (IsSenior(citizenID) && info.m_class.m_level == ItemClass.Level.Level5)
                        {
                            reason = TransferManager.TransferReason.ElderCare;
                            break;
                        }
                    }
                }

                // Send request if we found a Childcare/Eldercare facility
                if (reason != TransferManager.TransferReason.None)
                {
                    // WARNING: Childcare and Eldercare need an IN offer
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer(reason, offer);
                    return true;
                }
            }

            return false;
        }

        private static bool IsChild(uint citizenID)
        {
            return Citizen.GetAgeGroup(Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].Age) == Citizen.AgeGroup.Child || Citizen.GetAgeGroup(Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].Age) == Citizen.AgeGroup.Teen;
        }

        private static bool IsSenior(uint citizenID)
        {
            return Citizen.GetAgeGroup(Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID].Age) == Citizen.AgeGroup.Senior;
        }
    }
}