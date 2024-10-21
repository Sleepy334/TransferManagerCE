using HarmonyLib;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CheckPassengersPatches
    {
        [HarmonyPatch(typeof(PassengerShipAI), "CheckPassengers")]
        [HarmonyPostfix]
        public static void PassengerShipAICheckPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop, ref int __result)
        {
            if (currentStop != 0 && nextStop != 0 && __result == 0 && ModSettings.GetSettings().ResetStopMaxWaitTimeWhenNoPasengers)
            {
                ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[currentStop];
                // Set wait counter back to 0 for this stop so we dont get into a spawn loop
                node.m_maxWaitTime = 0;
            }
        }

        [HarmonyPatch(typeof(PassengerPlaneAI), "CheckPassengers")]
        [HarmonyPostfix]
        public static void PassengerPlaneAICheckPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop, ref int __result)
        {
            if (currentStop != 0 && nextStop != 0 && __result == 0 && ModSettings.GetSettings().ResetStopMaxWaitTimeWhenNoPasengers)
            {
                ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[currentStop];
                // Set wait counter back to 0 for this stop so we dont get into a spawn loop
                node.m_maxWaitTime = 0;
            }
        }

        [HarmonyPatch(typeof(PassengerTrainAI), "CheckPassengers")]
        [HarmonyPostfix]
        public static void PassengerTrainAICheckPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop, ref int __result)
        {
            if (currentStop != 0 && nextStop != 0 && __result == 0 && ModSettings.GetSettings().ResetStopMaxWaitTimeWhenNoPasengers)
            {
                ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[currentStop];
                // Set wait counter back to 0 for this stop so we dont get into a spawn loop
                node.m_maxWaitTime = 0;
            }
        }
    }
}
