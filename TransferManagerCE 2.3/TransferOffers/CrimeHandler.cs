using ColossalFramework;
using static TransferManager;
using static TransferManagerCE.CustomTransferReason;
using UnityEngine;

namespace TransferManagerCE.TransferOffers
{
    public class CrimeHandler
    {
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
                    if (DependencyUtils.IsNaturalDisastersDLC() &&
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
    }
}
