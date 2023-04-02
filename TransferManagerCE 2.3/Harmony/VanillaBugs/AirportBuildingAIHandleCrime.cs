using HarmonyLib;
using System;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class AirportBuildingAIHandleCrime
    {
        // There is a bug in TransportStationAI.CreateIncomingVehicle since 1.16.1 that it doesnt check if m_transportLineInfo is null before calling it
        // Bus stations seem to fail this check.
        [HarmonyPatch(typeof(AirportBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, ref int citizenCount)
        {
            // We increase citizen count by 100 in the same way as MainBuildingAI.HandleCrime
            // so that we don't have the crime notification going off all the time.
            citizenCount += 100;
        }
    }
}

