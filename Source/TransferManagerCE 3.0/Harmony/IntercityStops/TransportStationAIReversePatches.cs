using System.Runtime.CompilerServices;
using HarmonyLib;
using System;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class TransportStationAIReversePatches
    {
        // --------------------------------------------------------------------
        [HarmonyReversePatch]
        [HarmonyPatch(
            typeof(TransportStationAI), 
            "CreateConnectionLines",
            new Type[] { typeof(ushort), typeof(Building) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref }
         )]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CreateConnectionLines(TransportStationAI __instance, ushort buildingID, ref Building buildingData)
        {
            throw new NotImplementedException();
        }

        // --------------------------------------------------------------------
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TransportStationAI), "RemoveConnectionLines")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RemoveConnectionLines(TransportStationAI __instance, ushort buildingID, ref Building buildingData)
        {
            throw new NotImplementedException();
        }

        // --------------------------------------------------------------------
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TransportStationAI), "ReleaseVehicles")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReleaseVehicles(TransportStationAI __instance, ushort buildingID, ref Building buildingData)
        {
            throw new NotImplementedException();
        }

        // --------------------------------------------------------------------
        public static void ResetIntercityLines(TransportStationAI __instance, ushort buildingID, ref Building buildingData)
        {
            ReleaseVehicles(__instance, buildingID, ref buildingData);
            RemoveConnectionLines(__instance, buildingID, ref buildingData);

            // Turn off downgrading flag if set
            buildingData.m_flags &= ~Building.Flags.Downgrading;

            // Generate lines
            CreateConnectionLines(__instance, buildingID, ref buildingData);
        }

        // --------------------------------------------------------------------
        public static void ClearIntercityLines(TransportStationAI __instance, ushort buildingID, ref Building buildingData)
        {
            ReleaseVehicles(__instance, buildingID, ref buildingData);

            buildingData.m_flags |= Building.Flags.Downgrading;

            RemoveConnectionLines(__instance, buildingID, ref buildingData);
        }
    }
}