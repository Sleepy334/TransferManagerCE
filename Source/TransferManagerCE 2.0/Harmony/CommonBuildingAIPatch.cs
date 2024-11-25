using ColossalFramework;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(CommonBuildingAI), "HandleSick")]
    public static class CommonBuildingAIPatch
    {
        // There is a bug in AirportBuildingAI.HandleCrime that it never calls AddOutgoingOffer.
        [HarmonyPostfix]
        public static void HandleSick(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            // CommonBuildingAI.HandleSick does not put out an offer for Sick residents. This patch fixes that (replacement for Call Again).
            if (buildingData.m_healthProblemTimer > 0 && behaviour.m_sickCount != 0 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare) && Singleton<SimulationManager>.instance.m_randomizer.Int32(5U) == 0)
            {
                int sickCount = behaviour.m_sickCount;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CitiesUtils.CalculateGuestVehicles(buildingID, ref buildingData, TransferReason.Sick, ref count, ref cargo, ref capacity, ref outside);
                sickCount -= capacity;
                if (sickCount > 0)
                {
                    // You actually need to add a transfer for a citizen. We only add 1 at a time so it is more realistic
                    List<uint> cimSick = CitiesUtils.GetCitizens(buildingID, buildingData, Citizen.Flags.Sick);
                    if (cimSick.Count > 0)
                    {
#if DEBUG
                        //Debug.Log($"Adding sick request building:{buildingID} citizen:{cimSick[0]}");
#endif
                        TransferOffer offer = default(TransferOffer);
                        offer.Priority = buildingData.m_healthProblemTimer * 7 / 128;
                        offer.Citizen = cimSick[0];
                        offer.Position = buildingData.m_position;
                        offer.Amount = 1;
                        offer.Active = false;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick, offer);
                    }
                }
            }
        }
    }
}
