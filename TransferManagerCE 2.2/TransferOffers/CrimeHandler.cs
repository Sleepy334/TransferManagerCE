using ColossalFramework;
using static TransferManager;
using static TransferManagerCE.CustomTransferReason;
using UnityEngine;

namespace TransferManagerCE.TransferOffers
{
    public class CrimeHandler
    {
        // CommonBuildingAI.HandleCrime but modified to support Crime2
        public static void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            if (crimeAccumulation != 0)
            {
                byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                if (park != 0 && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_parkPolicies & DistrictPolicies.Park.SugarBan) != 0)
                {
                    crimeAccumulation = (int)((float)crimeAccumulation * 1.2f);
                }

                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    crimeAccumulation = crimeAccumulation * 5 >> 2;
                }

                if (data.m_eventIndex != 0)
                {
                    EventManager instance = Singleton<EventManager>.instance;
                    EventInfo info = instance.m_events.m_buffer[data.m_eventIndex].Info;
                    crimeAccumulation = info.m_eventAI.GetCrimeAccumulation(data.m_eventIndex, ref instance.m_events.m_buffer[data.m_eventIndex], crimeAccumulation);
                }

                crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
                crimeAccumulation = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Law, crimeAccumulation);
                if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    crimeAccumulation = 0;
                }
            }

            data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, data.m_crimeBuffer + crimeAccumulation);

            // Update the problem notification
            SetCrimeNotification(ref data, citizenCount);

            // Add an offer if needed
            AddCrimeOffer(buildingID, ref data, citizenCount);
        }

        public static void AddCrimeOffer(ushort buildingID, ref Building buildingData, int iCitizenCount)
        {
            int crimeBuffer = buildingData.m_crimeBuffer;
            if (iCitizenCount != 0 && crimeBuffer > iCitizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
            {
                // Check if we have police vehicles responding
                int count = BuildingUtils.GetGuestVehicleCount(buildingData, TransferReason.Crime, (TransferReason)Reason.Crime2);
                if (count == 0)
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = crimeBuffer / Mathf.Max(1, iCitizenCount * 10);
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;

                    // Add support for the helicopter policy
                    DistrictManager instance2 = Singleton<DistrictManager>.instance;
                    byte district = instance2.GetDistrict(buildingData.m_position);
                    DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;

                    // Occasionally add a Crime2 offer instead of a Crime offer
                    TransferReason reason;
                    if (DependencyUtilities.IsNaturalDisastersDLC() &&
                        ((buildingData.m_flags & Building.Flags.RoadAccessFailed) != 0 ||
                        (servicePolicies & DistrictPolicies.Services.HelicopterPriority) != 0 ||
                        Singleton<SimulationManager>.instance.m_randomizer.Int32(20U) == 0))
                    {
                        // Add Crime2 offer instead
                        reason = (TransferReason)Reason.Crime2;
                    }
                    else
                    {
                        reason = TransferReason.Crime;
                    }

                    Singleton<TransferManager>.instance.AddOutgoingOffer(reason, offer);
                }
            }
        }

        // CommonBuildingAI.SetCrimeNotification
        private static void SetCrimeNotification(ref Building data, int citizenCount)
        {
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(data.m_problems, Notification.Problem1.Crime);
            if (data.m_crimeBuffer > citizenCount * 90)
            {
                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Crime | Notification.Problem1.MajorProblem);
            }
            else if (data.m_crimeBuffer > citizenCount * 60)
            {
                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Crime);
            }

            data.m_problems = problemStruct;
        }
    }
}
