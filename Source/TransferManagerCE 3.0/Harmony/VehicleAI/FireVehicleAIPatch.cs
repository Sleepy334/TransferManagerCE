using HarmonyLib;
using System;
using ColossalFramework;
using UnityEngine;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using ColossalFramework.Math;
using static TreeManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class FireVehicleAIPatch : VehicleAIPatch
    {
        // Construct a static so we don't constantly create and throw away a hashset.
        private static HashSet<CustomTransferReason.Reason> s_transferFire = new HashSet<CustomTransferReason.Reason>() { CustomTransferReason.Reason.Fire };
        private static HashSet<CustomTransferReason.Reason> s_transferFire2 = new HashSet<CustomTransferReason.Reason>() { CustomTransferReason.Reason.Fire2 };
        private static HashSet<CustomTransferReason.Reason> s_transferFireAndFire2 = new HashSet<CustomTransferReason.Reason>() { CustomTransferReason.Reason.Fire, CustomTransferReason.Reason.Fire2 };
        private const float FIRE_DISTANCE_SEARCH = 160f;

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(FireTruckAI), "SetTarget")]
        [HarmonyPrefix]
        public static void FireTruckAISetTarget(ushort vehicleID, ref Vehicle data, ref ushort targetBuilding)
        {
            if (targetBuilding == 0)
            {
                // See if we can find a new target nearby
                if (data.m_sourceBuilding != 0 && !ShouldReturnToSource(vehicleID, ref data))
                {
                    ushort newTarget = FindBuildingWithFire(vehicleID, data.GetLastFramePosition(), FIRE_DISTANCE_SEARCH, s_transferFire);
                    if (newTarget != 0)
                    {
                        //Debug.Log($"Fire - Vehicle: {vehicleID} Found new target: {newTarget}");
                        targetBuilding = newTarget;
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(FireTruckAI), "SimulationStep", 
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, 
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void FireTruckAISimulationStepPostfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (ModSettings.GetSettings().FireTruckAI && (CustomTransferReason)vehicleData.m_transferType == CustomTransferReason.Reason.Fire)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                // Check assigned fire is still raging
                if (random.Int32(10U) == 0 && vehicleData.m_targetBuilding != 0)
                {
                    // Check if the fire is out or a responding vehicle is closer
                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                    if (building.m_fireIntensity == 0 || BuildingUtils.HasAnyCloserGuestVehicles(vehicleID, vehicleData.GetLastFramePosition(), vehicleData.m_targetBuilding, building, s_transferFire))
                    {
                        //Debug.Log($"Fire - Vehicle: {vehicleID} Clearing target: {vehicleData.m_targetBuilding}");
                        // Clear target
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0);
                    }
                }

                // Periodically search for nearby fires while travelling back to station.
                if (random.Int32(10U) == 0 &&
                    (vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0 &&
                    vehicleData.m_sourceBuilding != 0 &&
                    !ShouldReturnToSource(vehicleID, ref vehicleData))
                {
                    ushort newTarget = FindBuildingWithFire(vehicleID, vehicleData.GetLastFramePosition(), FIRE_DISTANCE_SEARCH, s_transferFire);
                    if (newTarget != 0)
                    {
                        //Debug.Log($"Fire - Vehicle: {vehicleID} Found new target: {newTarget}");

                        // clear flag goingback and waiting target
                        vehicleData.m_flags = vehicleData.m_flags & ~Vehicle.Flags.GoingBack & ~Vehicle.Flags.WaitingTarget;

                        // set new target
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);

                        // If the fire truck is stopped, the new target building is close enough that it will not move again so retarget deployed firefighting cims
                        if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0)
                        {
                            //Debug.Log($"FireTruck - Vehicle: {vehicleID} TargetCimsParentVehicleTarget: {newTarget}");
                            TargetCimsParentVehicleTarget(vehicleID, vehicleData);
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        // We will put out any trees that are near enough (40m) to the fire truck as well.
        [HarmonyPatch(typeof(FireTruckAI), "ExtinguishFire")]
        [HarmonyPostfix]
        public static void FireTruckAIExtinguishFire(FireTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort buildingID, ref Building buildingData)
        {
            if (ModSettings.GetSettings().FireTruckExtinguishTrees)
            {
                BurningTree[] BurningTrees = Singleton<TreeManager>.instance.m_burningTrees.m_buffer;
                TreeInstance[] Trees = Singleton<TreeManager>.instance.m_trees.m_buffer;

                TreeManager instance = Singleton<TreeManager>.instance;
                Vector3 position = data.GetLastFramePosition();

                int num = __instance.m_fireFightingRate;

                // Loop through burning trees and reduce fire intensity
                int size = instance.m_burningTrees.m_size;
                for (int i = 0; i < size; i++)
                {
                    ref BurningTree burningTree = ref BurningTrees[i];

                    int fireIntensity = burningTree.m_fireIntensity;
                    if (fireIntensity > 0)
                    {
                        uint treeIndex = burningTree.m_treeIndex;

                        ref TreeInstance tree = ref Trees[treeIndex];
                        if (tree.GrowState == 0)
                        {
                            continue;
                        }

                        // If the tree is close enough then 
                        float num2 = VectorUtils.LengthSqrXZ(tree.Position - position);
                        if (num2 < 1600f) // 40m
                        {
                            // We match it to the buildings intensity so it gets put out at the same time
                            fireIntensity = Math.Min(fireIntensity, buildingData.m_fireIntensity);
                            if (fireIntensity <= 0)
                            {
                                burningTree.m_fireIntensity = 0;
                                TreeInstance.Flags flags = (TreeInstance.Flags)tree.m_flags;
                                flags &= ~TreeInstance.Flags.Burning;
                                tree.m_flags = (ushort)flags;
                                InstanceID id = default(InstanceID);
                                id.Tree = treeIndex;
                                Singleton<InstanceManager>.instance.SetGroup(id, null);
                            }
                            else
                            {
                                burningTree.m_fireIntensity = (byte)fireIntensity;
                            }
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(FireCopterAI), "SimulationStep", 
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, 
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void FireCopterAISimulationStepPostfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (ModSettings.GetSettings().FireCopterAI)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                // WARNING: Do not clear target building as it uses this as rally point for putting out tree fires.

                // Periodically search for nearby fires while travelling back to station.
                if (random.Int32(10U) == 0 && 
                    (vehicleData.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack &&
                    (vehicleData.m_flags & Vehicle.Flags.WaitingTarget) == 0 &&
                    !ShouldReturnToSource(vehicleID, ref vehicleData))
                {
                    ushort newTarget = FindBuildingWithFire(vehicleID, vehicleData.GetLastFramePosition(), FIRE_DISTANCE_SEARCH, s_transferFireAndFire2);
                    if (newTarget != 0)
                    {
                        // set correct transfertype
                        if (vehicleData.m_transferType == (byte)TransferManager.TransferReason.ForestFire)
                        {
                            vehicleData.m_transferType = (byte)TransferManager.TransferReason.Fire2;
                        }

                        // set new target
                        //Debug.Log($"FireCopter - Vehicle: {vehicleID} SetTarget: {newTarget}");
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Find close by building with fire
        /// </summary>
        public static ushort FindBuildingWithFire(ushort vehicleId, Vector3 pos, float maxDistance, HashSet<CustomTransferReason.Reason> reasons)
        {
            ushort result = 0;
            BuildingUtils.EnumerateNearbyBuildings(pos, maxDistance, (buildingID, building) =>
            {
                if (building.m_fireIntensity > 0)
                {
                    // check if not already dispatched to
                    if (!BuildingUtils.HasAnyCloserGuestVehicles(vehicleId, pos, buildingID, building, reasons))
                    {
                        result = buildingID;
                        return false; // stop looking
                    }
                }
                return true; // Keep looking
            });

            return result;
        }

        // ----------------------------------------------------------------------------------------
        public static void TargetCimsParentVehicleTarget(ushort vehicleID, Vehicle vehicleData)
        {
            //not close enough? return
            if (Vector3.SqrMagnitude(vehicleData.GetLastFramePosition() - Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_position) > 50 * 50)
            {
                return;
            }

            CitizenInstance[] CitizenInstances = Singleton<CitizenManager>.instance.m_instances.m_buffer;

            // Loop through fire fighters and set their target
            ushort targetBuilding = vehicleData.m_targetBuilding;
            CitizenUtils.EnumerateCitizens(new InstanceID { Vehicle = vehicleID }, vehicleData.m_citizenUnits, (citizenId, citizen) =>
            {
                ushort instance2 = citizen.m_instance;
                if (instance2 != 0)
                {
                    ref CitizenInstance cimInstance = ref CitizenInstances[instance2];
                    if (cimInstance.m_flags != 0)
                    {
                        CitizenInfo info = cimInstance.Info;
                        info.m_citizenAI.SetTarget(instance2, ref cimInstance, targetBuilding);
                    }
                }
                return true; // Continue loop
            });
        }
    }
}
