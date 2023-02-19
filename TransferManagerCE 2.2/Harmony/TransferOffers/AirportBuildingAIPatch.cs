using ColossalFramework;
using HarmonyLib;
using TransferManagerCE.TransferOffers;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(AirportBuildingAI), "HandleCrime")]
    public static class AirportBuildingAIPatch
    {
        // There is a bug in AirportBuildingAI.HandleCrime that it never calls AddOutgoingOffer.
        [HarmonyPostfix]
        public static void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                CrimeHandler.AddCrimeOffer(buildingID, ref data, citizenCount);
            }
        }
    }
}
