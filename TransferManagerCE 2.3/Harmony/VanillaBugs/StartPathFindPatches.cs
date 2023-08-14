using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class StartPathFindPatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CarAI), "StartPathFind",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CarAIStartPathFindReverse(CarAI instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)
        {
            throw new NotImplementedException();
        }

        // Patch PostVanAI.StartPathFind so it doesnt get stuck in an infinite cargo station loop.
        [HarmonyPatch(typeof(PostVanAI), "StartPathFind",
                        new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool)},
                        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal})]
        [HarmonyPrefix]
        public static bool PostVanAIStartPathFindPrefix(PostVanAI __instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget, ref bool __result)
        {
            // DEBUG: Set this flag to test infinite loop behaviour.
            //vehicleData.m_flags2 |= Vehicle.Flags2.TransferToServicePoint;
            
            // If GoingBack or a mail van then we always want to call base.StartPathFind so we don't get a route through a cargo station and end up in an infinte loop
            if ((TransferReason) vehicleData.m_transferType == TransferReason.Mail || (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                __result = CarAIStartPathFindReverse(__instance, vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
                return false; // Don't call normal function
            }
            
            return true; // Handle normally
        }

        // Patch BankVanAI.StartPathFind so it doesnt get stuck in an infinite cargo station loop.
        [HarmonyPatch(typeof(BankVanAI), "StartPathFind",
                        new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) },
                        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool BankVanAIStartPathFindPrefix(BankVanAI __instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget, ref bool __result)
        {
            // DEBUG: Set this flag to test infinite loop behaviour.
            // vehicleData.m_flags2 |= Vehicle.Flags2.TransferToServicePoint;

            // We dont ever want a bank van to route through a cargo station so override vanilla StartPathFind call with a call to CarAI.StartPathFind instead.
            __result = CarAIStartPathFindReverse(__instance, vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
            return false; // Don't call normal function
        }

        // Patch GarbageTruckAI.StartPathFind as it's pathing code for a service point looks very buggy and users have reported issues with garbage not getting collected.
        [HarmonyPatch(typeof(GarbageTruckAI), "StartPathFind",
                        new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) },
                        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool GarbageTruckAIStartPathFindPrefix(GarbageTruckAI __instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget, ref bool __result)
        {
            // DEBUG: Set this flag to test infinite loop behaviour.
            // vehicleData.m_flags2 |= Vehicle.Flags2.TransferToServicePoint;

            // Always use CarAI.StartPathFind
            __result = CarAIStartPathFindReverse(__instance, vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
            return false; // Don't call normal function
        }
    }
}
