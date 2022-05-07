using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Util;

namespace TransferManagerCE.Patch.Police
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
            BuildingManager instance = Singleton<BuildingManager>.instance;
            uint numUnits = instance.m_buildings.m_size;    //get number of building units

            // CHECK FORMULAS -> REFERENCE: SMARTERFIREFIGHTERSAI
            int minx = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int minz = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int maxx = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int maxz = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);

            // Initialize default result if no building is found and specify maximum distance
            ushort result = 0;
            float shortestSquaredDistance = maxDistance * maxDistance;

            // Loop through every building grid within maximum distance
            for (int i = minz; i <= maxz; i++)
            {
                for (int j = minx; j <= maxx; j++)
                {
                    ushort currentBuilding = instance.m_buildingGrid[i * 270 + j];
                    int num7 = 0;

                    // Iterate through all buildings at this grid location
                    while (currentBuilding != 0)
                    {
                        // Check Building Crime buffer
                        byte citizencount = instance.m_buildings.m_buffer[currentBuilding].m_citizenCount;
                        int min_crime_amount = Math.Max(200, (CRIME_BUFFER_CITIZEN_MULT * citizencount));

                        if (instance.m_buildings.m_buffer[currentBuilding].m_crimeBuffer >= min_crime_amount)
                        {
                            // check if not already dispatched to
                            long value;
                            if (LRU_DISPATCH_LIST.TryGetValue(currentBuilding, out value))
                            {
                                lru_hit_counter++;
                                // dont consider building
                            }
                            else
                            {
                                // not found in LRU, may consider this building
                                float distanceSqr = VectorUtils.LengthSqrXZ(pos - instance.m_buildings.m_buffer[currentBuilding].m_position);
                                if (distanceSqr < shortestSquaredDistance)
                                {
                                    result = currentBuilding;
                                    shortestSquaredDistance = distanceSqr;
                                }
                            }
                        }

                        currentBuilding = instance.m_buildings.m_buffer[currentBuilding].m_nextGridBuilding;
                        if (++num7 >= numUnits)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            if (result != 0)
                AddBuildingLRU(result);

            return result;
        }

    }


    [HarmonyPatch(typeof(PoliceCarAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class PoliceCarAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // police capacity left?
            if (vehicleData.m_transferSize >= (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].Info?.m_vehicleAI as PoliceCarAI).m_crimeCapacity)
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
                    vehicleData.m_flags = vehicleData.m_flags & (~Vehicle.Flags.GoingBack) & (~Vehicle.Flags.WaitingTarget);
                    // set new target
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                    PoliceAIPatch.setnewtarget_counter++;
#if (DEBUG)
                    var instB = default(InstanceID);
                    instB.Building = newTarget;
                    string targetName = $"ID={newTarget}: {Singleton<BuildingManager>.instance.m_buildings.m_buffer[newTarget].Info?.name} ({Singleton<InstanceManager>.instance.GetName(instB)})";
                    var instV = default(InstanceID);
                    instV.Vehicle = vehicleID;
                    string vehicleName = $"ID={vehicleID} ({Singleton<InstanceManager>.instance.GetName(instV)})";
                    DebugLog.LogInfo($"PoliceCarAI: vehicle {vehicleName} set new target: {targetName}");
#endif
                }
            }
            else if ((vehicleData.m_targetBuilding != 0) && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_crimeBuffer < 50))
            {
                //need to change target because problem already solved?
                vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                PoliceAIPatch.dynamic_redispatch_counter++;
            }
        }
    
    }


    [HarmonyPatch(typeof(PoliceCopterAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class PoliceCopterAIAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // police capacity left?
            if (vehicleData.m_transferSize >= (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].Info?.m_vehicleAI as PoliceCopterAI).m_crimeCapacity)
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
                    vehicleData.m_flags = vehicleData.m_flags & (~Vehicle.Flags.GoingBack) & (~Vehicle.Flags.WaitingTarget);
                    // set new target
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                    PoliceAIPatch.setnewtarget_counter++;
#if (DEBUG)
                    var instB = default(InstanceID);
                    instB.Building = newTarget;
                    string targetName = $"ID={newTarget}: {Singleton<BuildingManager>.instance.m_buildings.m_buffer[newTarget].Info?.name} ({Singleton<InstanceManager>.instance.GetName(instB)})";
                    var instV = default(InstanceID);
                    instV.Vehicle = vehicleID;
                    string vehicleName = $"ID={vehicleID} ({Singleton<InstanceManager>.instance.GetName(instV)})";
                    Debug.Log($"PoliceCopterAI: vehicle {vehicleName} set new target: {targetName}");
#endif
                }
            }
            else if ((vehicleData.m_targetBuilding != 0) && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_crimeBuffer < 50))
            {
                //need to change target because problem already solved?
                vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                PoliceAIPatch.dynamic_redispatch_counter++;
            }
        }
    }
}
