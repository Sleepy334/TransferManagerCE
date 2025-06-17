using HarmonyLib;
using System;
using ColossalFramework;
using TransferManagerCE.Settings;
using TransferManagerCE.Data;
using UnityEngine;
using System.Collections.Generic;
using ColossalFramework.Math;
using static RenderManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class PoliceVehicleAIPatch : VehicleAIPatch
    {
        private static HashSet<CustomTransferReason.Reason> s_transferReasons = new HashSet<CustomTransferReason.Reason>() { CustomTransferReason.Reason.Crime, CustomTransferReason.Reason.Crime2 };
        private const float MAX_DISTANCE_SEARCH = 160f;

        [HarmonyPatch(typeof(PoliceCarAI), "SimulationStep", 
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, 
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void PoliceCarAIPostfix(PoliceCarAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (ModSettings.GetSettings().PoliceCarAI && (CustomTransferReason.Reason)vehicleData.m_transferType == CustomTransferReason.Reason.Crime)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                // Check building still has crime
                ClearTargetIfResolved(vehicleID, ref vehicleData);

                // Periodically search for nearby crime while travelling back to station.
                if (random.Int32(20U) == 0 && 
                    (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.WaitingTarget) == 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.Arriving) == 0 &&
                    vehicleData.m_sourceBuilding != 0 &&
                    vehicleData.m_transferSize < __instance.m_criminalCapacity &&
                    !ShouldReturnToSource(vehicleID, ref vehicleData))
                {
                    ushort newTarget = FindBuildingWithCrime(vehicleID, vehicleData.GetLastFramePosition(), MAX_DISTANCE_SEARCH);
                    if (newTarget != 0)
                    {
                        // clear flag goingback and waiting target
                        vehicleData.m_flags = vehicleData.m_flags & ~Vehicle.Flags.GoingBack & ~Vehicle.Flags.WaitingTarget;

                        // set new target
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void PoliceCopterAIPostfix(PoliceCopterAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // With police copter we just clear the current assignment.
            if (ModSettings.GetSettings().PoliceCopterAI && (CustomTransferReason.Reason)vehicleData.m_transferType == CustomTransferReason.Reason.Crime2)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                ClearTargetIfResolved(vehicleID, ref vehicleData);
                
                // Periodically search for nearby crime while travelling back to station.
                if (random.Int32(20U) == 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0 &&
                    (vehicleData.m_flags & Vehicle.Flags.WaitingTarget) == 0 &&
                    vehicleData.m_sourceBuilding != 0 &&
                    vehicleData.m_transferSize < __instance.m_crimeCapacity &&
                    !ShouldReturnToSource(vehicleID, ref vehicleData))
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = 7;
                    offer.Vehicle = vehicleID;
                    offer.Position = vehicleData.GetLastFramePosition();
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason) CustomTransferReason.Reason.Crime2, offer);
                    vehicleData.m_flags &= ~Vehicle.Flags.GoingBack; 
                    vehicleData.m_flags |= Vehicle.Flags.WaitingTarget;
                }
            }
        }

        public static void ClearTargetIfResolved(ushort vehicleID, ref Vehicle vehicleData)
        {
            // Check building still has crime
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10U) == 0 && ShouldClearTarget(vehicleID, vehicleData))
            {
                //clear target
                vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0);
            }
        }

        public static bool ShouldClearTarget(ushort vehicleID, Vehicle vehicleData)
        {
            bool bClearTarget = false;

            if (vehicleData.m_targetBuilding != 0 &&
                (vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) == 0) 
            {
                // Check there is crime to go and get.
                Building building = BuildingManager.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                if (building.m_flags != 0)
                {
                    int iCitizenCount = CrimeCitizenCountStorage.GetCitizenCount(vehicleData.m_targetBuilding, building);
                    if (iCitizenCount == 0)
                    {
                        // If the building has a large crime buffer then clear it anyway
                        if (building.m_crimeBuffer < StatusDataCrime.iMAJOR_CRIME_RATE)
                        {
                            bClearTarget = true;
                        }
                    }
                    else if (building.m_crimeBuffer < iCitizenCount * 10 && BuildingUtils.GetCriminalCount(vehicleData.m_targetBuilding, building) == 0)
                    {
                        bClearTarget = true;
                    }
                }
            }

            return bClearTarget;
        }

        /// <summary>
        /// Find close by building with crime
        /// </summary>
        public static ushort FindBuildingWithCrime(ushort vehicleId, Vector3 pos, float maxDistance)
        {
            ushort result = 0;
            BuildingUtils.EnumerateNearbyBuildings(pos, maxDistance, (buildingID, building) =>
            {
                if ((building.m_flags & Building.Flags.Created) != 0 &&
                    (building.m_flags & Building.Flags.Active) != 0 &&
                    (building.m_flags & Building.Flags.Abandoned) == 0 &&
                    building.m_crimeBuffer > 0 &&
                    building.Info is not null && 
                    building.Info.GetService() != ItemClass.Service.PoliceDepartment &&
                    building.Info.GetAI() is CommonBuildingAI)
                {
                    int iCitizenCount = CrimeCitizenCountStorage.GetCitizenCount(buildingID, building);
                    if (iCitizenCount > 0)
                    {
                        if (building.m_crimeBuffer > iCitizenCount * 25)
                        {
                            if (!BuildingUtils.HasAnyGuestVehicles(buildingID, building, s_transferReasons))
                            {
                                result = buildingID;
                                return false; // stop looking
                            }
                        }
                        else if (SaveGameSettings.GetSettings().PoliceToughOnCrime && BuildingUtils.GetCriminalCount(buildingID, building) > 1)
                        {
                            if (!BuildingUtils.HasAnyGuestVehicles(buildingID, building, s_transferReasons))
                            {
                                result = buildingID;
                                return false; // stop looking
                            }
                        }
                    }
                }

                return true; // Keep looking
            });

            return result;
        }
    }
}
