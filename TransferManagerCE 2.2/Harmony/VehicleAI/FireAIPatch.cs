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
                        Building building = instance.m_buildings.m_buffer[currentBuilding];
                        if (building.m_fireIntensity > 0)
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
                                float distanceSqr = VectorUtils.LengthSqrXZ(pos - building.m_position);
                                if (distanceSqr < shortestSquaredDistance)
                                {
                                    result = currentBuilding;
                                    shortestSquaredDistance = distanceSqr;
                                }
                            }
                        }

                        currentBuilding = building.m_nextGridBuilding;
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
}
