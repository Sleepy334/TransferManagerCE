using HarmonyLib;
using ColossalFramework;
using UnityEngine;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class AutofillCargoAIPatch : VehicleAIPatch
    {
        // --------------------------------------------------------------------
        // Autofill: Try and find a customer for unnassigned cargo on ships, planes and trains.
        [HarmonyPatch(typeof(CargoTruckAI), "SimulationStep")]
        [HarmonyPostfix]
        public static void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            // Check its enabled
            if (!SaveGameSettings.GetSettings().Autofill)
            {
                return;
            }

            if (UnityEngine.Random.Range(0, 4) == 0)
            {
                if (data.m_cargoParent != 0 &&
                    data.m_targetBuilding == 0 &&
                    data.m_transferSize > 0 &&
                    (data.m_flags & Vehicle.Flags.TransferToTarget) != 0 &&
                    (data.m_flags & Vehicle.Flags.WaitingTarget) == 0 &&
                    (data.m_flags & Vehicle.Flags.Arriving) == 0 &&
                    (data.m_flags & Vehicle.Flags.WaitingPath) == 0 &&
                    !ShouldReturnToSource(vehicleID, ref data))
                {
                    // Find destinations for Autofill cargo
                    //CDebug.Log($"Adding Offer - Vehicle: {vehicleID} Flags: {data.m_flags} Material: {(CustomTransferReason.Reason)data.m_transferType} TransferSize: {data.m_transferSize} Parent: {data.m_cargoParent} Source: {data.m_sourceBuilding} Target: {data.m_targetBuilding}");

                    TransferManager.TransferOffer offer = default;
                    offer.Vehicle = vehicleID;
                    offer.Priority = 1; // Higher than outside connection so hopefully we match more
                    offer.Position = GetCargoVehicleOfferPosition(vehicleID, ref data);
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer);

                    data.m_flags &= ~Vehicle.Flags.GoingBack;
                    data.m_flags |= Vehicle.Flags.WaitingTarget;
                }
            }
        }

        // --------------------------------------------------------------------
        // We need to override this to handle the case where a cargo child vehicle
        // doesnt have a target yet so we skip all the ChangeVehicle behaviour.
        [HarmonyPatch(typeof(CargoTruckAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransferPrefix(ushort vehicleID, ref Vehicle data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            // Check its enabled
            if (SaveGameSettings.GetSettings().Autofill)
            {
                if (material == (TransferManager.TransferReason)data.m_transferType &&
                    offer.Building != 0 &&
                    data.m_cargoParent != 0 &&
                    data.m_targetBuilding == 0)
                {
                    data.m_flags |= Vehicle.Flags.TransferToTarget;
                    data.m_targetBuilding = offer.Building;
                    data.m_flags &= ~Vehicle.Flags.WaitingTarget;
                    data.m_flags &= ~Vehicle.Flags.GoingBack;
                    BuildingManager.instance.m_buildings.m_buffer[data.m_targetBuilding].AddGuestVehicle(vehicleID, ref data);

                    // Charge for resource
                    if ((data.m_flags & Vehicle.Flags.Importing) != 0)
                    {
                        Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                        OutsideConnectionAI.ImportResource(offer.Building, ref Buildings[offer.Building], material, data.m_transferSize);
                    }

                    //CDebug.Log($"StartTransferPrefix - Vehicle: {vehicleID} Flags: {data.m_flags} Material: {(CustomTransferReason.Reason)data.m_transferType} TransferSize: {data.m_transferSize} Parent: {data.m_cargoParent} Source: {data.m_sourceBuilding} DataTarget: {data.m_targetBuilding} Target: {offer.Building}");
                    return false; // Dont call vanilla
                }
            }

            return true; // Handle normally
        }
    }
}