using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    public class RendererUtils
    {
        public static void HighlightBuilding(Building[] BuildingBuffer, ushort usBuildingId, RenderManager.CameraInfo cameraInfo, UnityEngine.Color color)
        {
            ref Building building = ref BuildingBuffer[usBuildingId];
            if (building.m_flags != 0)
            {
                // Highlight building path
                if (building.Info is not null)
                {
                    building.Info.m_buildingAI.RenderBuildOverlay(cameraInfo, color, building.m_position, building.m_angle, default(Segment3));
                }

                // Highlight building
                BuildingTool.RenderOverlay(cameraInfo, ref building, color, color);

                // Also highlight any sub buildings
                float m_angle = building.m_angle * 57.29578f;
                BuildingInfo info3 = building.Info;
                if (info3 is not null && info3.m_subBuildings is not null && info3.m_subBuildings.Length != 0)
                {
                    // Render sub buildings
                    Matrix4x4 matrix4x = default(Matrix4x4);
                    matrix4x.SetTRS(building.m_position, Quaternion.AngleAxis(m_angle, Vector3.down), Vector3.one);
                    for (int i = 0; i < info3.m_subBuildings.Length; i++)
                    {
                        BuildingInfo buildingInfo = info3.m_subBuildings[i].m_buildingInfo;
                        Vector3 position = matrix4x.MultiplyPoint(info3.m_subBuildings[i].m_position);
                        float angle = (info3.m_subBuildings[i].m_angle + m_angle) * ((float)Math.PI / 180f);
                        BuildingTool.RenderOverlay(cameraInfo, buildingInfo, 0, position, angle, color, radius: false);
                    }
                }
            }
        }

        public static void HighlightVehicle(Vehicle[] VehicleBuffer, CameraInfo cameraInfo, ushort vehicleId, Color color)
        {
            Vehicle vehicle = VehicleBuffer[vehicleId];
            if (vehicle.m_cargoParent != 0)
            {
                // Highlight parent instead
                HighlightVehicle(VehicleBuffer, cameraInfo, vehicle.m_cargoParent, color);
            }
            else
            {
                float alpha8 = 1f;
                vehicle.CheckOverlayAlpha(ref alpha8);
                color.a *= alpha8;
                vehicle.RenderOverlay(cameraInfo, vehicleId, color);

                // Loop through trailing vehicles and render each one
                ushort trailingVehicle = vehicle.m_trailingVehicle;
                int iLoopCount = 0;
                while (trailingVehicle != 0)
                {
                    // highlight trailing vehicle
                    Vehicle trailer = VehicleBuffer[trailingVehicle];
                    trailer.RenderOverlay(cameraInfo, trailingVehicle, color);

                    trailingVehicle = trailer.m_trailingVehicle;

                    if (++iLoopCount > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }

        public static void HighlightNode(CameraInfo cameraInfo, NetNode oNode, Color color)
        {
            RenderManager.instance.OverlayEffect.DrawCircle(
                                            cameraInfo,
                                            color,
                                            oNode.m_position,
                                            oNode.m_bounds.size.magnitude,
                                            oNode.m_position.y - 1f,
                                            oNode.m_position.y + 1f,
                                            true,
                                            true);
        }

        public static void HighlightSegment(RenderManager.CameraInfo cameraInfo, ref NetSegment segment, Color importantColor, Color nonImportantColor)
        {
            NetInfo info = segment.Info;
            if (!(info == null) && ((segment.m_flags & NetSegment.Flags.Untouchable) == 0 || info.m_overlayVisible))
            {
                Bezier3 bezier = default(Bezier3);
                bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[segment.m_startNode].m_position;
                bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[segment.m_endNode].m_position;
                NetSegment.CalculateMiddlePoints(bezier.a, segment.m_startDirection, bezier.d, segment.m_endDirection, smoothStart: false, smoothEnd: false, out bezier.b, out bezier.c);
                bool flag = false;
                bool flag2 = false;
                int privateServiceIndex = ItemClass.GetPrivateServiceIndex(info.m_class.m_service);
                Color color = (((privateServiceIndex == -1 && !info.m_autoRemove) || (segment.m_flags & NetSegment.Flags.Untouchable) != 0) ? importantColor : nonImportantColor);
                Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, color, bezier, info.m_halfWidth * 2f, (!flag) ? (-100000f) : info.m_halfWidth, (!flag2) ? (-100000f) : info.m_halfWidth, -1f, 1280f, renderLimits: false, alphaBlend: false);
            }
        }
    }
}