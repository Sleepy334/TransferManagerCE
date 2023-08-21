using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CheckRoadAccessPatches
    {
        // Override PlayerBuildingAI.CheckRoadAccess so we can look for a train line instead of a road for the new Warehouse with built in cargo line.
        [HarmonyPatch(typeof(PlayerBuildingAI), "CheckRoadAccess")]
        [HarmonyPrefix]
        public static bool CheckRoadAccessPrefix(BuildingAI __instance, ushort buildingID, ref Building data)
        {
            if (__instance is WarehouseStationAI)
            {
                bool flag = true;
                data.m_accessSegment = 0;

                if ((data.m_flags & Building.Flags.Collapsed) == 0)
                {
                    if (FindTrainAccess(buildingID, ref data, data.m_position, out var segmentID, mostCloser: true))
                    {
                        Debug.Log("Access segment found.");
                        data.m_accessSegment = segmentID;
                        flag = false;
                    }
                    else
                    {
                        Debug.Log("No Train Access segment found.");
                    }
                }

                // Update road connection notification.
                Notification.ProblemStruct problems = data.m_problems;
                data.m_problems = Notification.RemoveProblems(data.m_problems, new Notification.ProblemStruct(Notification.Problem1.RoadNotConnected, Notification.Problem2.NotInPedestrianZone | Notification.Problem2.CannotBeReached));
                if (flag)
                {
                    data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.RoadNotConnected);
                }

                // Do not call PlayerBuildingAI.CheckRoadAccess
                return false;
            }

            // Handle normally.
            return true;
        }

        private static bool FindTrainAccess(ushort buildingID, ref Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true)
        {
            Bounds bounds = new Bounds(position, new Vector3(40f, 40f, 40f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            segmentID = 0;
            float num5 = float.MaxValue;
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    int num6 = 0;
                    for (ushort num7 = instance.m_segmentGrid[i * 270 + j]; num7 != 0; num7 = instance.m_segments.m_buffer[num7].m_nextGridSegment)
                    {
                        if (num6++ >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }

                        NetInfo info = instance.m_segments.m_buffer[num7].Info;
                        if (info.m_class.m_service == ItemClass.Service.PublicTransport && 
                            info.m_class.m_subService == ItemClass.SubService.PublicTransportTrain)
                        {
                            ushort startNode = instance.m_segments.m_buffer[num7].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[num7].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[endNode].m_position;
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num9 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            if ((!(num8 >= 0f) || !(num9 >= 0f)) && 
                                instance.m_segments.m_buffer[num7].m_bounds.Intersects(bounds) && 
                                instance.m_segments.m_buffer[num7].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Train, VehicleInfo.VehicleCategory.CargoTrain, VehicleInfo.VehicleType.None, requireConnect: false, out var positionA, out var _, out var _, out var _, out var _, out var _))
                            {
                                float num10 = Vector3.SqrMagnitude(position - positionA);
                                if (!(num10 >= 400f) && !(num10 >= num5))
                                {
                                    segmentID = num7;
                                    if (!mostCloser)
                                    {
                                        return true;
                                    }

                                    num5 = num10;
                                }
                            }
                        }
                    }
                }
            }

            if (segmentID == 0)
            {
                data.m_flags |= Building.Flags.RoadAccessFailed;
                return false;
            }

            return true;
        }
    }
}
