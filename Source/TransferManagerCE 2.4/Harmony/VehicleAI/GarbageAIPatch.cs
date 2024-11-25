using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public static class GarbageAIPatch
    {
        internal const ushort GARBAGE_BUFFER_MIN_LEVEL = 800;
        internal const float GARBAGE_DISTANCE_SEARCH = 160f;

        //prevent double-dispatching of multiple vehicles to same target
        private const int LRU_MAX_SIZE = 8;
        private static Dictionary<ushort, long> LRU_DISPATCH_LIST = new Dictionary<ushort, long>(LRU_MAX_SIZE);

        internal static int lru_hit_counter;
        internal static int setnewtarget_counter;

        private static void AddBuildingLRU(ushort buildingID)
        {
            if (LRU_DISPATCH_LIST.Count >= LRU_MAX_SIZE)
            {
                // remove oldest:
                LRU_DISPATCH_LIST.Remove(LRU_DISPATCH_LIST.OrderBy(x => x.Value).First().Key);
            }

            LRU_DISPATCH_LIST.Add(buildingID, DateTime.Now.Ticks);
        }

        /// <summary>
        /// Added for new P & P DLC. Don't pick up garbage from building in pedestrian zone.
        /// </summary>
        public static bool IsPedestrianZone(Building building)
        {
            if (building.m_accessSegment != 0)
            {
                NetSegment segment = NetManager.instance.m_segments.m_buffer[building.m_accessSegment];
                if (segment.m_flags != 0)
                {
                    if (segment.Info is not null && segment.Info.IsPedestrianZoneRoad())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find close by building with garbage
        /// </summary>
        public static ushort FindBuildingWithGarbage(Vector3 pos, float maxDistance)
        {
            // Initialize default result if no building is found and specify maximum distance
            ushort result = 0;
            float shortestSquaredDistance = maxDistance * maxDistance;

            BuildingUtils.EnumerateNearbyBuildings(pos, maxDistance, (buildingID, building) =>
            {
                // Check Building garbage buffer
                if (building.m_flags != 0 &&
                    building.m_garbageBuffer >= GARBAGE_BUFFER_MIN_LEVEL &&
                    building.Info is not null &&
                    building.Info.GetService() != ItemClass.Service.Garbage &&
                    !IsPedestrianZone(building))
                {
                    // check if not already dispatched to
                    long value;
                    if (LRU_DISPATCH_LIST.TryGetValue(buildingID, out value))
                    {
                        // dont consider building
                        lru_hit_counter++;
                    }
                    else
                    {
                        // not found in LRU, may consider this building
                        float distanceSqr = VectorUtils.LengthSqrXZ(pos - building.m_position);
                        if (distanceSqr < shortestSquaredDistance)
                        {
                            result = buildingID;
                            shortestSquaredDistance = distanceSqr;
                        }
                    }
                }
            });

            if (result != 0)
            {
                AddBuildingLRU(result);
            }
                
            return result;
        }

    }


    [HarmonyPatch(typeof(GarbageTruckAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class GarbageTruckAIPatchSimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (SaveGameSettings.GetSettings().GarbageTruckAI)
            {
                // garbage capacity left?
                Vehicle oVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID];
                GarbageTruckAI oTruckAI = (GarbageTruckAI)oVehicle.Info.m_vehicleAI;
                if (oTruckAI is null || vehicleData.m_transferSize >= oTruckAI.m_cargoCapacity)
                {
                    return;
                }

                // Check transfer type is actually garbage
                if (vehicleData.m_transferType == (byte)TransferManager.TransferReason.Garbage)
                {
                    if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
                    {
                        ushort newTarget = GarbageAIPatch.FindBuildingWithGarbage(vehicleData.GetLastFramePosition(), GarbageAIPatch.GARBAGE_DISTANCE_SEARCH);
                        if (newTarget != 0)
                        {
                            // clear flag goingback and waiting target
                            vehicleData.m_flags = vehicleData.m_flags & ~Vehicle.Flags.GoingBack & ~Vehicle.Flags.WaitingTarget;
                            // set new target
                            vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                            GarbageAIPatch.setnewtarget_counter++;
                        }
                    }
                }
            }
        }
    }
}
