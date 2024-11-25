using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransportStationAICreateIncomingVehicle
    {
        // There is a bug in TransportStationAI.CreateIncomingVehicle since 1.16.1 that it doesnt check if m_transportLineInfo is null before calling it
        // Bus stations seem to fail this check.
        [HarmonyPatch(typeof(TransportStationAI), "CreateIncomingVehicle")]
        [HarmonyPrefix]
        public static bool CreateIncomingVehiclePrefix(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort startStop, int gateIndex, ref bool __result)
        {
            if (__instance.m_transportLineInfo == null && ModSettings.GetSettings().FixTransportStationNullReferenceException)
            {
#if DEBUG
                Debug.Log($"BuildingId: {buildingID} - Error: m_transportLineInfo is null ");
#endif
                __result = false;

                // Don't call CreateIncomingVehicle as it would crash
                return false;
            }

            // Run normal CreateIncomingVehicle function
            return true;
        }
    }
}
