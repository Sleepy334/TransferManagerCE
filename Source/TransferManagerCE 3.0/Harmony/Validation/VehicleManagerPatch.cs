using HarmonyLib;
using SleepyCommon;

namespace TransferManagerCore
{
    // ------------------------------------------------------------------------
    [HarmonyPatch]
    public class VehicleManagerPatch
    {
        // --------------------------------------------------------------------
        // DEBUGGING, check cargo vehicle arrays are still valid
        [HarmonyPatch(typeof(VehicleManager), "ReleaseVehicle")]
        [HarmonyPrefix]
        public static void ReleaseVehicle(ushort vehicle)
        {
            Vehicle vehicleData = VehicleManager.instance.m_vehicles.m_buffer[vehicle];
            Log.Error($"Releasing vehicle: {vehicle} Flags: {vehicleData.m_flags} BlockCounter: {vehicleData.m_blockCounter}");
        }
    }
}