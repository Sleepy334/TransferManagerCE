using ColossalFramework;
using System.Collections.Generic;
using static RenderManager;
using System.Reflection;
using SleepyCommon;
using TransferManagerCore;

namespace TransferManagerCore
{
    public class ClearExtendedReasons
    {
        // --------------------------------------------------------------------
        public static void Clear()
        {
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            HashSet<ushort> extendedVehicles = new HashSet<ushort>();
            for (int i = 0; i < Vehicles.Length; ++i)
            {
                Vehicle vehicle = Vehicles[i];

                if (vehicle.m_transferType > TransferManager.TRANSFER_REASON_COUNT &&
                    vehicle.m_transferType != (byte) TransferManager.TransferReason.None)
                {
                    extendedVehicles.Add((ushort) i);
                }
            }

            // Perform actual de-spawning
            int[] m_outgoingAmount = (int[])typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(TransferManager.instance);
            bool bTransferArrayExtended = m_outgoingAmount.Length == 256;

            foreach (ushort vehicleId in extendedVehicles)
            {
                ref Vehicle vehicle = ref Vehicles[vehicleId];

                if (vehicle.m_flags != 0 && vehicle.m_transferType > TransferManager.TRANSFER_REASON_COUNT)
                {
                    Log.Info($"Clear Extended Reasons | Vehicle: #{vehicleId} AI: {vehicle.Info.m_vehicleAI} Reason: {(CustomTransferReason.Reason) vehicle.m_transferType}.");

                    if (!bTransferArrayExtended)
                    {
                        // We have to remove extended transfer type before releasing vehicle otherwise we will crash
                        vehicle.m_transferType = 0;
                    }

                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
                }
            }

            Log.Info($"Clear Extended Reasons | {extendedVehicles.Count} vehicles had extended reasons.");
        }
    }
}