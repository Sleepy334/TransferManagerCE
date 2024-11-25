using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(AirportBuildingAI), "HandleCrime")]
    public static class AirportBuildingAIPatch
    {
        // There is a bug in AirportBuildingAI.HandleCrime that it never calls AddOutgoingOffer.
        [HarmonyPostfix]
        public static void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            int crimeBuffer = (int)data.m_crimeBuffer;
            if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment) && Singleton<SimulationManager>.instance.m_randomizer.Int32(5U) == 0)
            {
#if DEBUG
                //Debug.Log($"Adding crime request {buildingID}");
#endif
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                CitiesUtils.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
                if (count == 0)
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, new TransferManager.TransferOffer()
                    {
                        Priority = crimeBuffer / Mathf.Max(1, citizenCount * 10),
                        Building = buildingID,
                        Position = data.m_position,
                        Amount = 1
                    });
            }
        }
    }
}
