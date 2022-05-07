﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(FireCopterAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class FireCopterAIAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            /*
            if ((vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) != 0)
            {
                ushort newTarget = FireAIPatch.FindBuildingWithFire(vehicleData.GetLastFramePosition(), FireAIPatch.FIRE_DISTANCE_SEARCH);
                if (newTarget != 0)
                {
                    // set correct transfertype
                    if (vehicleData.m_transferType == (byte)TransferManager.TransferReason.ForestFire)
                        vehicleData.m_transferType = (byte)TransferManager.TransferReason.Fire2;

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
                    DebugLog.LogDebug((DebugLog.LogReason)TransferManager.TransferReason.Crime, $"PoliceCopterAI: vehicle {vehicleName} set new target: {targetName}");
#endif

                }

            }
            else if ((vehicleData.m_targetBuilding != 0) && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding].m_fireIntensity == 0))
            {
                //need to change target because problem already solved?
                vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                FireAIPatch.dynamic_redispatch_counter++;
            }
            */
        }

    }
}
