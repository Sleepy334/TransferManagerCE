using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TransferManagerCE
{
    public static class PoliceAIPatch
    {
        internal const ushort CRIME_BUFFER_CITIZEN_MULT = 25; //as multiplier for citizenCount, to be compared with m_crimebuffer value of building!
        internal const float CRIME_DISTANCE_SEARCH = 160f;

        //prevent double-dispatching of multiple vehicles to same target
        private const int LRU_MAX_SIZE = 8;
        private static Dictionary<ushort, long> LRU_DISPATCH_LIST = new Dictionary<ushort, long>(LRU_MAX_SIZE);

        #region STATISTICS
        internal static int lru_hit_counter;
        internal static int setnewtarget_counter;
        internal static int dynamic_redispatch_counter;
        #endregion

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
        /// Find close by building with crime
        /// </summary>
        public static ushort FindBuildingWithCrime(Vector3 pos, float maxDistance)
        {
            // Initialize default result if no building is found and specify maximum distance
            ushort result = 0;
            float shortestSquaredDistance = maxDistance * maxDistance;

            BuildingUtils.EnumerateNearbyBuildings(pos, maxDistance, (buildingID, building) =>
            {
                // Check Building Crime buffer
                if (building.m_flags != 0)
                {
                    byte citizencount = building.m_citizenCount;
                    int min_crime_amount = Math.Max(200, CRIME_BUFFER_CITIZEN_MULT * citizencount);

                    if (building.m_crimeBuffer >= min_crime_amount && building.Info is not null && building.Info.GetService() != ItemClass.Service.PoliceDepartment)
                    {
                        // check if not already dispatched to
                        long value;
                        if (LRU_DISPATCH_LIST.TryGetValue(buildingID, out value))
                        {
                            lru_hit_counter++;
                            // dont consider building
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
                }
            });

            if (result != 0)
            {
                AddBuildingLRU(result);
            }

            return result;
        }

    }


    [HarmonyPatch(typeof(PoliceCarAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class PoliceCarAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (SaveGameSettings.GetSettings().PoliceCarAI)
            {
                // police capacity left?
                if (vehicleData.m_transferSize >= (vehicleData.Info?.m_vehicleAI as PoliceCarAI).m_crimeCapacity)
                    return;

                // check transfertype was not move transfer
                if (vehicleData.m_transferType != (byte)TransferManager.TransferReason.Crime)
                    return;

                if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
                {
                    ushort newTarget = PoliceAIPatch.FindBuildingWithCrime(vehicleData.GetLastFramePosition(), PoliceAIPatch.CRIME_DISTANCE_SEARCH);
                    if (newTarget != 0)
                    {
                        // clear flag goingback and waiting target
                        vehicleData.m_flags = vehicleData.m_flags & ~Vehicle.Flags.GoingBack & ~Vehicle.Flags.WaitingTarget;
                        // set new target
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                        PoliceAIPatch.setnewtarget_counter++;
                    }
                }
                else if (vehicleData.m_targetBuilding != 0)
                {
                    // Check there is crime to go and get.
                    Building building = BuildingManager.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                    if (building.m_crimeBuffer < 50 && BuildingUtils.GetCriminalCount(vehicleData.m_targetBuilding, building) == 0)
                    {
                        //Debug.Log($"PoliceCarAI: vehicle {vehicleID} Clearing target");
                        //need to change target because problem already solved?
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                        PoliceAIPatch.dynamic_redispatch_counter++;
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class PoliceCopterAIAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (SaveGameSettings.GetSettings().PoliceCopterAI)
            {
                // police capacity left?
                if (vehicleData.m_transferSize >= (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].Info?.m_vehicleAI as PoliceCopterAI).m_crimeCapacity)
                    return;

                // check transfertype was not move transfer
                if ((CustomTransferReason.Reason) vehicleData.m_transferType != CustomTransferReason.Reason.Crime2)
                    return;

                if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
                {
                    ushort newTarget = PoliceAIPatch.FindBuildingWithCrime(vehicleData.GetLastFramePosition(), PoliceAIPatch.CRIME_DISTANCE_SEARCH);
                    if (newTarget != 0)
                    {
                        // clear flag goingback and waiting target
                        vehicleData.m_flags = vehicleData.m_flags & ~Vehicle.Flags.GoingBack & ~Vehicle.Flags.WaitingTarget;
                        // set new target
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                        PoliceAIPatch.setnewtarget_counter++;
                    }
                }
                else if (vehicleData.m_targetBuilding != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_crimeBuffer < 50)
                {
                    //need to change target because problem already solved?
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                    PoliceAIPatch.dynamic_redispatch_counter++;
                }
            }
        }
    }
}
