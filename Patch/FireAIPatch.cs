using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Util;

namespace TransferManagerCE.Patch.Fire
{
    public static class FireAIPatch
    {
        internal const float FIRE_DISTANCE_SEARCH = 160f;

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
        public static ushort FindBuildingWithFire(Vector3 pos, float maxDistance)
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
                        // Check Building Fire
                        if (instance.m_buildings.m_buffer[currentBuilding].m_fireIntensity > 0)
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

        public static void TargetCimsParentVehicleTarget(ushort vehicleID, ref Vehicle vehicleData)
        {
            //not close enough? return
            if (Vector3.SqrMagnitude(vehicleData.GetLastFramePosition() - Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_position) > 50 * 50)
                return;

            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint numCitizenUnits = instance.m_units.m_size;
            uint num = vehicleData.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                    if (citizen == 0)
                    {
                        continue;
                    }
                    ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
                    if (instance2 == 0)
                    {
                        continue;
                    }
                    CitizenInfo info = instance.m_instances.m_buffer[instance2].Info;
                    info.m_citizenAI.SetTarget(instance2, ref instance.m_instances.m_buffer[instance2], vehicleData.m_targetBuilding);
                }
                num = nextUnit;
                if (++num2 > numCitizenUnits)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

    }


    [HarmonyPatch(typeof(FireTruckAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class FireTruckAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            // check transfertype
            if (vehicleData.m_transferType != (byte)TransferManager.TransferReason.Fire)
                return;

            if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
            {
                ushort newTarget = FireAIPatch.FindBuildingWithFire(vehicleData.GetLastFramePosition(), FireAIPatch.FIRE_DISTANCE_SEARCH);
                if (newTarget != 0)
                {
                    // clear flag goingback and waiting target
                    vehicleData.m_flags = vehicleData.m_flags & (~Vehicle.Flags.GoingBack) & (~Vehicle.Flags.WaitingTarget);
                    // set new target
                    vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                    FireAIPatch.setnewtarget_counter++;
#if (DEBUG)
                    var instB = default(InstanceID);
                    instB.Building = newTarget;
                    string targetName = $"ID={newTarget}: {Singleton<BuildingManager>.instance.m_buildings.m_buffer[newTarget].Info?.name} ({Singleton<InstanceManager>.instance.GetName(instB)})";
                    var instV = default(InstanceID);
                    instV.Vehicle = vehicleID;
                    string vehicleName = $"ID={vehicleID} ({Singleton<InstanceManager>.instance.GetName(instV)})";
                    DebugLog.LogInfo($"FireTruckAI: vehicle {vehicleName} set new target: {targetName}");
#endif

                    // If the fire truck is stopped, the new target building is close enough that it will not move again so retarget deployed firefighting cims
                    if ((vehicleData.m_flags & Vehicle.Flags.Stopped) != 0)
                    {
                        FireAIPatch.TargetCimsParentVehicleTarget(vehicleID, ref vehicleData);
                    }
                }
            }
            else if ((vehicleData.m_targetBuilding != 0) && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_fireIntensity == 0))
            {
                //need to change target because problem already solved?
                vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                FireAIPatch.dynamic_redispatch_counter++;
            }
        }
    
    }
}
