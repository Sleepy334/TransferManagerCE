using ColossalFramework;
using static TransferManager;
using UnityEngine;

namespace TransferManagerCE
{
    public class DeadHandler
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
                BuildingUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Dead, ref count, ref cargo, ref capacity, ref outside);
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
    }
}
