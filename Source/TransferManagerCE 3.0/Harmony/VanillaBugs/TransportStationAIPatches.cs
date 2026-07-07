using System.Reflection;
using System;
using ColossalFramework;
using HarmonyLib;
using SleepyCommon;
using TransferManagerCore.Settings;
using UnityEngine;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class TransportStationAIPatches
    {
        // --------------------------------------------------------------------
        private static int s_trainPassengerCapacity = 0;
        private static int s_shipPassengerCapacity = 0;
        private static int s_planePassengerCapacity = 0;
        private static int s_busPassengerCapacity = 0;

        // --------------------------------------------------------------------
        // We patch the ProduceGoods function to check on waiting passenger counts and force vehicle spawn if needed.
        [HarmonyPatch(typeof(TransportStationAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static void ProduceGoodsPrefix(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            // We check the passenger count to trigger an early vehicle spawn if needed.
            if (ModSettings.GetSettings().ForceIntercityStopSpawnAtMaxCount &&
                __instance.m_transportInfo is not null &&
                buildingData.m_netNode != 0 &&
                Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
            {
                // Add to thread for checking
                CheckWaitingPassengers(buildingData.m_netNode);
            }
        }

        // --------------------------------------------------------------------
        public static void UpdatePassengerCapacities()
        {
            s_trainPassengerCapacity = MaxPassengerCapacity(ItemClass.SubService.PublicTransportTrain);
            s_shipPassengerCapacity = MaxPassengerCapacity(ItemClass.SubService.PublicTransportShip);
            s_planePassengerCapacity = MaxPassengerCapacity(ItemClass.SubService.PublicTransportPlane);
            s_busPassengerCapacity = MaxPassengerCapacity(ItemClass.SubService.PublicTransportBus);
        }

        // --------------------------------------------------------------------
        private static void CheckWaitingPassengers(ushort nodeId)
        {
            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

            ushort stop = nodeId;
            int iLoopCount = 0;
            while (stop != 0)
            {
                NetNode nodeStop = Nodes[stop];

                if (nodeStop.m_maxWaitTime > 0 &&
                    nodeStop.m_maxWaitTime < 250 && // otherwise it will trigger soon anyway
                    nodeStop.m_transportLine == 0 &&
                    nodeStop.Info is not null)
                {
                    int iMaxCapacity = GetVehicleCapacity(nodeStop.Info.GetSubService());

                    //CDebug.Log($"Checking Node: {stop} Type: {nodeStop.Info.GetSubService()} Flags: {nodeStop.m_flags} WaitTime: {nodeStop.m_maxWaitTime} MaxCapacity:{iMaxCapacity}");

                    if (iMaxCapacity > 0 && 
                        HasReachedPassengerLimit(stop, iMaxCapacity))
                    {
                        //CDebug.Log($"HasReachedPassengerLimit - FOUND: Node: {stop} WaitTime: {Nodes[stop].m_maxWaitTime} MaxCapacity:{iMaxCapacity}");
                        //InstanceHelper.ShowInstance(new InstanceID { NetNode = stop });
                        // Set MaxValue so a vehicle is spawned.
                        Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_maxWaitTime = byte.MaxValue;
                        Log.Info($"Max vehicle capacity found at node #{stop}, updating wait timer.");
                    }
                }

                stop = nodeStop.m_nextBuildingNode;

                if (++iLoopCount > 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        // --------------------------------------------------------------------
        private static bool HasReachedPassengerLimit(ushort stop, int iLimit)
        {
            if (stop == 0)
            {
                return false;
            }

            ushort nextStop = TransportLine.GetNextStop(stop);
            if (nextStop == 0)
            {
                return false;
            }

            ushort[] InstanceGrid = Singleton<CitizenManager>.instance.m_citizenGrid;
            CitizenInstance[] CitizenInstances = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

            float searchDistance = 64f;
            float searchDistanceSquared = searchDistance * searchDistance;

            Vector3 position = Nodes[stop].m_position;
            Vector3 position2 = Nodes[nextStop].m_position;
            int num2 = Mathf.Max((int)((position.x - searchDistance) / 8f + 1080f), 0);
            int num3 = Mathf.Max((int)((position.z - searchDistance) / 8f + 1080f), 0);
            int num4 = Mathf.Min((int)((position.x + searchDistance) / 8f + 1080f), 2159);
            int num5 = Mathf.Min((int)((position.z + searchDistance) / 8f + 1080f), 2159);
            int iPassengerCount = 0;

            for (int i = num3; i <= num5; i++)
            {
                for (int j = num2; j <= num4; j++)
                {
                    ushort citizenInstanceId = InstanceGrid[i * 2160 + j];
                    int iLoopCount = 0;
                    while (citizenInstanceId != 0)
                    {
                        ref CitizenInstance citizenInstance = ref CitizenInstances[citizenInstanceId];

                        // Get ready for next citizen
                        ushort nextGridInstance = citizenInstance.m_nextGridInstance;

                        if ((citizenInstance.m_flags & CitizenInstance.Flags.WaitingTransport) != 0)
                        {
                            Vector3 a = citizenInstance.m_targetPos;
                            if (Vector3.SqrMagnitude(a - position) < searchDistanceSquared)
                            {
                                if (citizenInstance.Info.m_citizenAI.TransportArriveAtSource(citizenInstanceId, ref citizenInstance, position, position2))
                                {
                                    iPassengerCount++;
                                    if (iPassengerCount >= iLimit)
                                    {
                                        // Don't need to count anymore
                                        return true;
                                    }
                                }
                            }
                        }

                        citizenInstanceId = nextGridInstance;

                        if (++iLoopCount > 65536)
                        {
                            Log.Info("Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return false;
        }

        // --------------------------------------------------------------------
        private static int GetVehicleCapacity(ItemClass.SubService subService)
        {
            int iVehicleCapacity = 0;

            // These are the only vehicle types supported by intercity stops
            switch (subService)
            {
                case ItemClass.SubService.PublicTransportTrain:
                    {
                        iVehicleCapacity = s_trainPassengerCapacity;

                        break;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    {
                        iVehicleCapacity = s_shipPassengerCapacity;
                        break;
                    }
                case ItemClass.SubService.PublicTransportPlane:
                    {
                        iVehicleCapacity = s_planePassengerCapacity;
                        break;
                    }
                case ItemClass.SubService.PublicTransportBus:
                    {
                        iVehicleCapacity = s_busPassengerCapacity;
                        break;
                    }
            }

            return iVehicleCapacity;
        }

        // --------------------------------------------------------------------
        private static int MaxPassengerCapacity(ItemClass.SubService subService)
        {
            int iCapacity = 0;

            try
            {
                FieldInfo m_transferVehiclesInfo = typeof(VehicleManager).GetField("m_transferVehicles", BindingFlags.NonPublic | BindingFlags.Instance);
                FastList<ushort>[] m_transferVehicles = (FastList<ushort>[])m_transferVehiclesInfo.GetValue(VehicleManager.instance);

                if (m_transferVehicles is not null)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        ItemClass.Level level = (ItemClass.Level)i;
                        int iTransferIndex = GetTransferIndex(ItemClass.Service.PublicTransport, subService, level);
                        FastList<ushort> fastList = m_transferVehicles[iTransferIndex];

                        foreach (ushort prefabIndex in fastList)
                        {
                            VehicleInfo info = PrefabCollection<VehicleInfo>.GetPrefab(prefabIndex);
                            iCapacity = Math.Max(iCapacity, GetVehicleInfoCapacity(info));
                        }
                    }

                }
                else
                {
                    Log.Error($"ERROR: Unable to access m_transferVehicles");
                    iCapacity = GetDefaultValue(subService);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR: Exception trying to access m_transferVehicles", ex);
                iCapacity = GetDefaultValue(subService);
            }

            return iCapacity;
        }

        // --------------------------------------------------------------------
        private static int GetTransferIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {
            int num = ((subService == ItemClass.SubService.None) ? ((int)(service - 1)) : ((int)(28 + subService - 1)));
            return (int)(num * 5 + level);
        }

        // --------------------------------------------------------------------
        private static int GetVehicleInfoCapacity(VehicleInfo info)
        {
            int iVehicleCapacity = GetPassengerCapacity(info.m_vehicleAI);

            // Add trailer capacity
            if (info.m_trailers is not null)
            {
                for (int i = 0; i < info.m_trailers.Length; i++)
                {
                    VehicleInfo info2 = info.m_trailers[i].m_info;
                    if (info2.GetAI() is PassengerTrainAI trailer)
                    {
                        iVehicleCapacity += GetPassengerCapacity((VehicleAI)info2.GetAI());
                    }
                }
            }

            return iVehicleCapacity;
        }

        // --------------------------------------------------------------------
        private static int GetPassengerCapacity(VehicleAI vehicleAI)
        {
            switch (vehicleAI)
            {
                case BusAI bus:
                    {
                        return bus.m_passengerCapacity;
                    }
                case PassengerTrainAI train:
                    {
                        return train.m_passengerCapacity;
                    }
                case PassengerPlaneAI plane:
                    {
                        return plane.m_passengerCapacity;
                    }
                case PassengerShipAI ship:
                    {
                        return ship.m_passengerCapacity;
                    }
            }

            return 0;
        }

        // --------------------------------------------------------------------
        private static int GetDefaultValue(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.PublicTransportTrain: return 240;
                case ItemClass.SubService.PublicTransportShip: return 100;
                case ItemClass.SubService.PublicTransportPlane: return 200;
                case ItemClass.SubService.PublicTransportBus: return 60;
            }

            return 0;
        }
    }
}
