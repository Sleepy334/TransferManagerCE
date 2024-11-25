using HarmonyLib;
using System;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(FireCopterAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    public static class FireCopterAIAISimulationStepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (SaveGameSettings.GetSettings().FireCopterAI)
            {
                if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack)
                {
                    ushort newTarget = FireAIPatch.FindBuildingWithFire(vehicleData.GetLastFramePosition(), FireAIPatch.FIRE_DISTANCE_SEARCH);
                    if (newTarget != 0)
                    {
                        // set correct transfertype
                        if (vehicleData.m_transferType == (byte)TransferManager.TransferReason.ForestFire)
                        {
                            vehicleData.m_transferType = (byte)TransferManager.TransferReason.Fire2;
                        }

                        // set new target
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, newTarget);
                        FireAIPatch.setnewtarget_counter++;
                    }
                }
            }
        }
    }
}
