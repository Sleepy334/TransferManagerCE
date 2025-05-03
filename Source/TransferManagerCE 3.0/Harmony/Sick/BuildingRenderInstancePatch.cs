using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class BuildingRenderInstancePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Building), "RenderInstance",
            new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int), typeof(BuildingInfo), typeof(RenderManager.Instance) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        public static void RenderInstancePostFix(RenderManager.CameraInfo cameraInfo, ushort buildingID, int layerMask, BuildingInfo info, ref RenderManager.Instance data)
        {
            if (SaveGameSettings.GetSettings().DisplaySickNotification)
            {
                ToolController properties = Singleton<ToolManager>.instance.m_properties;
                if (properties is null || properties.m_mode != ItemClass.Availability.AssetEditor)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[buildingID];
                    if (building.m_flags != 0 &&
                        building.m_healthProblemTimer > SickHandler.iSICK_MINOR_PROBLEM_TIMER_VALUE &&
                        (building.m_problems & (Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Noise)).IsNone)
                    {
                        int iImageIndex;
                        if (building.m_healthProblemTimer > SickHandler.iSICK_MAJOR_PROBLEM_TIMER_VALUE)
                        {
                            iImageIndex = 72; // NotificationMajorSick
                        }
                        else
                        {
                            iImageIndex = 209; // Sad balloon
                        }

                        Vector3 position2 = building.m_position;
                        position2.y += Mathf.Min(info.m_size.y, data.m_dataVector0.y);
                        RenderInstance(cameraInfo, iImageIndex, position2, 1.0f);
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, int index, Vector3 position, float scale)
        {
            NotificationManager instance = Singleton<NotificationManager>.instance;

            Vector4 @params = new Vector4(0.1f, 1f, 5f, 0f);
            @params.x = 0.2f;
            @params.z = 6f;

            NotificationManager.BufferedItem item = default(NotificationManager.BufferedItem);
            item.m_position = new Vector4(position.x, position.y, position.z, scale);
            item.m_params = @params;
            item.m_distanceSqr = Vector3.SqrMagnitude(position - cameraInfo.m_position);
            item.m_regionIndex = index;
            instance.m_bufferedItems.Add(item);
        }
    }
}