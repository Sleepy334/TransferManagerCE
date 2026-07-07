using HarmonyLib;
using ColossalFramework;
using UnityEngine;
using TransferManagerCore.Settings;
using SleepyCommon;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class CargoTruckAIPatch : VehicleAIPatch
    {
        // --------------------------------------------------------------------
        // Try and find a customer for this resource
        [HarmonyPatch(typeof(CargoTruckAI), "SimulationStep")]
        [HarmonyPostfix]
        public static void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if (ModSettings.GetSettings().CargoTruckAI)
            {
                // Request new locations for trucks already out in the city
                if (UnityEngine.Random.Range(0, 10) == 0 &&
                    (data.m_flags & Vehicle.Flags.TransferToTarget) != 0 &&
                    (data.m_flags & Vehicle.Flags.WaitingTarget) == 0 &&
                    (data.m_flags & Vehicle.Flags.Arriving) == 0 &&
                    (data.m_flags & Vehicle.Flags.WaitingPath) == 0 &&
                    data.m_targetBuilding == 0 &&
                    !ShouldReturnToSource(vehicleID, ref data) &&
                    data.m_cargoParent == 0 &&
                    (data.m_flags & Vehicle.Flags.Spawned) != 0 &&
                    (data.m_flags & Vehicle.Flags.GoingBack) != 0 &&
                    data.m_transferSize > 2000)
                {
                    //CDebug.Log($"Adding Offer - Vehicle: {vehicleID} Flags: {data.m_flags} Material: {(CustomTransferReason.Reason)data.m_transferType} TransferSize: {data.m_transferSize} Parent: {data.m_cargoParent} Source: {data.m_sourceBuilding} Target: {data.m_targetBuilding}");

                    TransferManager.TransferOffer offer = default;
                    offer.Vehicle = vehicleID;
                    offer.Priority = 7;
                    offer.Position = GetCargoVehicleOfferPosition(vehicleID, ref data);
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer);

                    data.m_flags &= ~Vehicle.Flags.GoingBack;
                    data.m_flags |= Vehicle.Flags.WaitingTarget;
                }
            }
        }
    }
}