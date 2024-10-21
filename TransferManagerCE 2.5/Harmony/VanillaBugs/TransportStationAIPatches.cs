using ColossalFramework;
using HarmonyLib;
using System;
using System.Diagnostics;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransportStationAIPatches
    {
        //static Stopwatch s_stopwatch = Stopwatch.StartNew();

        // --------------------------------------------------------------------
        // There is a bug in TransportStationAI.CreateIncomingVehicle since 1.16.1 that it doesnt check if m_transportLineInfo is null before calling it
        // Bus stations seem to fail this check.
        [HarmonyPatch(typeof(TransportStationAI), "CreateIncomingVehicle")]
        [HarmonyPrefix]
        public static bool CreateIncomingVehiclePrefix(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort startStop, int gateIndex, ref bool __result)
        {
            if (__instance.m_transportLineInfo == null && ModSettings.GetSettings().FixTransportStationNullReferenceException)
            {
#if DEBUG
                Debug.Log($"BuildingId: {buildingID} - Error: m_transportLineInfo is null ");
#endif
                __result = false;

                // Don't call CreateIncomingVehicle as it would crash
                return false;
            }

            // Run normal CreateIncomingVehicle function
            return true;
        }

        // --------------------------------------------------------------------
        // We patch the ProduceGoods function to check on waiting passenger counts and force vehicle spawn if needed.
        [HarmonyPatch(typeof(TransportStationAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static bool ProduceGoodsPrefix(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            // We check the passenger count to trigger an early vehicle spawn if needed.
            if (__instance.m_transportInfo is not null &&
                buildingData.m_netNode != 0 &&
                Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
            {
                int iVehicleCapacity = GetVehicleCapacity(__instance.m_transportInfo.m_transportType);
                if (iVehicleCapacity > 0)
                {
                    //long startTimeTicks = s_stopwatch.ElapsedTicks;

                    NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
                    ushort stop = buildingData.m_netNode;
                    int iLoopCount = 0;
                    while (stop != 0)
                    {
                        NetNode node = Nodes[stop];

                        if (node.m_maxWaitTime > 0 &&
                            node.m_maxWaitTime < 250 && // otherwise it will trigger soon anyway
                            node.m_transportLine == 0 &&
                            node.Info is not null &&
                            node.Info.m_class.m_layer == ItemClass.Layer.PublicTransport)
                        {
                            if (HasReachedPassengerLimit(stop, __instance.m_transportInfo.m_transportType, iVehicleCapacity))
                            {
                                // Set MaxValue so a vehicle is spawned.
                                Nodes[stop].m_maxWaitTime = byte.MaxValue;
#if DEBUG
                                Debug.Log($"Building: {buildingID} Stop: {stop} - Set to {byte.MaxValue}");
#endif
                            }
                        }

                        stop = node.m_nextBuildingNode;
                        if (++iLoopCount > 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }

                    //long jobMatchTimeTicks = s_stopwatch.ElapsedTicks - startTimeTicks;
                    //Debug.Log($"buildingID: {buildingID} TransportType: {__instance.m_transportInfo.m_transportType} Time: {(jobMatchTimeTicks * 0.0001).ToString("F")}");
                }
            }

            return true;
        }

        // --------------------------------------------------------------------
        private static int GetVehicleCapacity(TransportInfo.TransportType eType)
        {
            int iVehicleCapacity = 0;
            switch (eType)
            {
                case TransportInfo.TransportType.Train:
                    {
                        iVehicleCapacity = ModSettings.GetSettings().ForceTrainSpawnAtCount;
                        break;
                    }
                case TransportInfo.TransportType.Ship:
                    {
                        iVehicleCapacity = ModSettings.GetSettings().ForceShipSpawnAtCount;
                        break;
                    }
                case TransportInfo.TransportType.Airplane:
                    {
                        iVehicleCapacity = ModSettings.GetSettings().ForcePlaneSpawnAtCount;
                        break;
                    }
                case TransportInfo.TransportType.Bus:
                    {
                        iVehicleCapacity = ModSettings.GetSettings().ForceBusSpawnAtCount;
                        break;
                    }
            }

            return iVehicleCapacity;
        }

        // --------------------------------------------------------------------
        public static bool HasReachedPassengerLimit(ushort stop, TransportInfo.TransportType transportType, int iLimit)
        {
            ushort[] InstanceGrid = Singleton<CitizenManager>.instance.m_citizenGrid;
            CitizenInstance[] CitizenInstances = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

            if (stop == 0)
            {
                return false;
            }

            ushort nextStop = TransportLine.GetNextStop(stop);
            if (nextStop == 0)
            {
                return false;
            }

            float searchDistance = (transportType != 0 && transportType != TransportInfo.TransportType.EvacuationBus && transportType != TransportInfo.TransportType.TouristBus) ? 64f : 32f;
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
                            Debug.Log("Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return false;
        }
    }
}
