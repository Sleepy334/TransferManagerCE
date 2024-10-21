using HarmonyLib;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Util;
using static RenderManager;

namespace TransferManagerCE
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
            // Initialize default result if no building is found and specify maximum distance
            ushort result = 0;
            float shortestSquaredDistance = maxDistance * maxDistance;

            BuildingUtils.EnumerateNearbyBuildings(pos, maxDistance, (buildingID, building) =>
            {
                if (building.m_fireIntensity > 0)
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
            });

            if (result != 0)
            {
                AddBuildingLRU(result);
            }

            return result;
        }

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
            });  ;
        }
    }
}
