using HarmonyLib;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class PathDistancePatches
    {
        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(NetManager), "ReleaseLaneImplementation")]
        [HarmonyPostfix]
        public static void ReleaseLaneImplementationPostfix(uint lane)
        {
            PathDistanceCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(NetManager), "ReleaseSegmentImplementation",
            new[] { typeof(ushort), typeof(NetSegment), typeof(bool) },
            new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void ReleaseSegmentImplementationPostfix(ushort segment) 
        {
            PathDistanceCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(NetAI), "CreateSegment")]
        [HarmonyPostfix]
        public static void CreateSegment(ushort segmentID, ref NetSegment data)
        {
            PathDistanceCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(BuildingManager), "UpdateNotifications")]
        [HarmonyPostfix]
        public static void UpdateNotifications(ushort building, Notification.ProblemStruct oldProblems, Notification.ProblemStruct newProblems)
        {
            if ((oldProblems & Notification.Problem1.TurnedOff).IsNone && 
                (newProblems & Notification.Problem1.TurnedOff).IsNotNone)
            {
                // Turned OFF
                PathDistanceCache.Invalidate();
            }
            else if ((oldProblems & Notification.Problem1.TurnedOff).IsNotNone &&
                     (newProblems & Notification.Problem1.TurnedOff).IsNone)
            {
                // Turned ON
                PathDistanceCache.Invalidate();
            }
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(RaceEventAI), "BeginPreparing")]
        [HarmonyPostfix]
        public static void BeginBeginPreparingPostfix(ushort eventID, ref EventData data)
        {
            PathDistanceCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(RaceEventAI), "BeginEvent")]
        [HarmonyPostfix]
        public static void BeginEventPostfix(ushort eventID, ref EventData data)
        {
            PathDistanceCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(RaceEventAI), "EndEvent")]
        [HarmonyPostfix]
        public static void EndEventPostfix(ushort eventID, ref EventData data)
        {
            PathDistanceCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(RaceEventAI), "EndDisorganizing")]
        [HarmonyPostfix]
        public static void EndDisorganizing(ushort eventID, ref EventData data)
        {
            PathDistanceCache.Invalidate();
        }
    }
}