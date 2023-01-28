using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CommonBuildingAIHandleCrime
    {
        // Crime2 support
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyPostfix]
        public static void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager && DependencyUtilities.IsNaturalDisastersDLC())
            {
                int crimeBuffer = (int)data.m_crimeBuffer;
                if (citizenCount > 0 && crimeBuffer > citizenCount * 15 && 
                    Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
                    offer.Priority = crimeBuffer / Mathf.Max(1, citizenCount * 10);
                    offer.Building = buildingID;
                    offer.Position = data.m_position;
                    offer.Amount = 1;

                    // Have we got any police helicopters responding
                    int count2 = 0;
                    int cargo2 = 0;
                    int capacity2 = 0;
                    int outside2 = 0;
                    CitiesUtils.CalculateGuestVehicles(buildingID, ref data, (TransferManager.TransferReason)CustomTransferReason.Reason.Crime2, ref count2, ref cargo2, ref capacity2, ref outside2);

                    if (count2 > 0)
                    {
                        // Make sure we don't add a Crime offer if a police helicopter is already responding
                        Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                    }
                    else
                    {
                        // Add support for the helicopter policy
                        DistrictManager instance2 = Singleton<DistrictManager>.instance;
                        byte district = instance2.GetDistrict(data.m_position);
                        DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;

                        // Occasionally add a Crime2 offer instead of a Crime offer
                        if ((data.m_flags & Building.Flags.RoadAccessFailed) != 0 ||
                            (servicePolicies & DistrictPolicies.Services.HelicopterPriority) != 0 ||
                            Singleton<SimulationManager>.instance.m_randomizer.Int32(20U) == 0)
                        {
                            // Check for Police vehicles
                            int count = 0;
                            int cargo = 0;
                            int capacity = 0;
                            int outside = 0;
                            CitiesUtils.CalculateGuestVehicles(buildingID, ref data, (TransferManager.TransferReason)CustomTransferReason.Reason.Crime, ref count, ref cargo, ref capacity, ref outside);

                            if (count == 0)
                            {
                                // Remove Crime offer
                                Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Crime, offer);

                                // Add Crime2 offer instead
                                Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)CustomTransferReason.Reason.Crime2, offer);
                            }
                        }    
                    }
                }
            }
        }
    }
}
