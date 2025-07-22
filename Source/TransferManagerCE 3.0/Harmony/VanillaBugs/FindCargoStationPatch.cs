using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class FindCargoStationPatch
    {
        // ----------------------------------------------------------------------------------------
        // We override CargoTruckAI.FindCargoStation as it is badly written and will often not find the nearest cargo station
        [HarmonyPatch(typeof(CargoTruckAI), "FindCargoStation")]
        [HarmonyPrefix]
        public static bool FindCargoStation(Vector3 position, ItemClass.Service service, ItemClass.SubService subservice, ref ushort __result)
        {
            Building[] BuldingBuffer = BuildingManager.instance.m_buildings.m_buffer;
            ushort[] BuildingGridBuffer = BuildingManager.instance.m_buildingGrid;

            if (subservice != ItemClass.SubService.PublicTransportPlane)
            {
                subservice = ItemClass.SubService.None;
            }

            float maxDistance = 100f;

            int num = Mathf.Max((int)((position.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((position.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((position.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((position.z + maxDistance) / 64f + 135f), 269);

            ushort result = 0;
            float minDistance = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort buildingId = BuildingGridBuffer[i * 270 + j];

                    int iLoopCount = 0;
                    while (buildingId != 0)
                    {
                        Building building = BuldingBuffer[buildingId];
                        if (building.m_flags != 0)
                        {
                            BuildingInfo info = building.Info;
                            if ((info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subservice || subservice == ItemClass.SubService.None))
                            {
                                // Check if this building actually has a cargo station in it
                                ushort cargoBuildingId = GetCargoBuilding(buildingId, out Vector3 spawnPos);
                                if (cargoBuildingId != 0)
                                {
                                    float distanceSquared = Vector3.SqrMagnitude(position - spawnPos);
                                    if (distanceSquared < minDistance)
                                    {
                                        result = cargoBuildingId;
                                        minDistance = distanceSquared;
                                    }
                                }
                            }
                        }

                        buildingId = building.m_nextGridBuilding;

                        if (++iLoopCount >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            __result = result;
            return false; // Do not call vanilla function
        }

        // ----------------------------------------------------------------------------------------
        private static ushort GetCargoBuilding(ushort buildingId, out Vector3 spawnPosition)
        {
            Building[] BuldingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            int num2 = 0;
            while (buildingId != 0)
            {
                Building building = BuldingBuffer[buildingId];

                ushort parentBuilding = building.m_parentBuilding;

                BuildingInfo info = building.Info;
                if (info is not null)
                {
                    switch (info.m_buildingAI)
                    {
                        case WarehouseStationAI:
                            {
                                // We add on the spawn position as it is quite large and otherwise we can end up with 
                                // cargo warehouses getting confused over which station to go to.
                                WarehouseStationAI cargoStation = (WarehouseStationAI)info.m_buildingAI;
                                spawnPosition = building.CalculatePosition(cargoStation.m_spawnPosition);

                                return buildingId;
                            }
                        case CargoStationAI:
                            {
                                CargoStationAI cargoStation = (CargoStationAI)info.m_buildingAI;
                                spawnPosition = building.CalculatePosition(cargoStation.m_spawnPosition);

                                return buildingId;
                            }
                        case OutsideConnectionAI:
                            {
                                spawnPosition = building.m_position;
                                return buildingId;
                            }
                    }
                }

                buildingId = parentBuilding;

                if (++num2 > 49152)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            spawnPosition = Vector3.zero;
            return 0;
        }
    }
}