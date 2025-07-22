using HarmonyLib;
using System;
using ColossalFramework;
using TransferManagerCE.Settings;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class GarbageTruckAIPatch : VehicleAIPatch
    {
        internal const ushort GARBAGE_BUFFER_MIN_LEVEL = 100;

        [HarmonyPatch(typeof(GarbageTruckAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void SimulationStepPostfix(GarbageTruckAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            
            if (ModSettings.GetSettings().GarbageTruckAI &&
                (TransferManager.TransferReason) vehicleData.m_transferType == TransferManager.TransferReason.Garbage)
            {
                // We occasionally check the target buildings garbage buffer to see if it has been emptied already.
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10U) == 0 && ShouldClearTarget(vehicleID, vehicleData))
                {
                    // DEBUG
                    //Building building = BuildingManager.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                    //CDebug.Log($"Clearing Target - Vehicle: #{vehicleID} Building: #{vehicleData.m_targetBuilding} BuildingType: {building.Info.GetAI().GetType()} GarbageBuffer: {building.m_garbageBuffer}");
                    //if (Input.GetKey(KeyCode.LeftControl))
                    //{
                    //    InstanceHelper.ShowInstance(new InstanceID { Building = vehicleData.m_targetBuilding });
                    //}
                    // DEBUG

                    // clear target as it has been resolved
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0);
                }

                // If this garbage truck is heading back to base and still has some capacity, ask for a new assignment
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10U) == 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.WaitingTarget) == 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.Arriving) == 0 &&
                    vehicleData.m_sourceBuilding != 0 &&
                    vehicleData.m_transferSize < __instance.m_cargoCapacity &&
                    !ShouldReturnToSource(vehicleID, ref vehicleData))
                {
                    //CDebug.Log($"Adding offer - Vehicle: #{vehicleID} Building: #{vehicleData.m_sourceBuilding}");

                    // Add an offer to see if we get another assignment
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = 7;
                    offer.Vehicle = vehicleID;
                    if (vehicleData.m_sourceBuilding != 0)
                    {
                        offer.Position = vehicleData.GetLastFramePosition() * 0.25f + Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].m_position * 0.75f;
                    }
                    else
                    {
                        offer.Position = vehicleData.GetLastFramePosition();
                    }
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason) vehicleData.m_transferType, offer);
                    vehicleData.m_flags &= ~Vehicle.Flags.GoingBack;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingTarget;
                }
            }
        }

        public static bool ShouldClearTarget(ushort vehicleID, Vehicle vehicleData)
        {
            if (vehicleData.m_targetBuilding != 0 &&
                (vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) == 0 &&
                (vehicleData.m_flags & Vehicle.Flags.Arriving) == 0 &&
                (vehicleData.m_flags2 & Vehicle.Flags2.TransferToServicePoint) == 0)
            {
                // Check there is garbage to go and get.
                Building building = BuildingManager.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                if (building.m_flags != 0 &&
                    building.m_garbageBuffer < GARBAGE_BUFFER_MIN_LEVEL &&
                    (building.m_problems & Notification.Problem1.Garbage).IsNone)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
