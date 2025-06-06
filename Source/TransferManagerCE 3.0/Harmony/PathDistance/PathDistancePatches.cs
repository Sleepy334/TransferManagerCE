using HarmonyLib;

namespace TransferManagerCE
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
            OutsideConnectionCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(NetManager), "ReleaseSegmentImplementation",
            new[] { typeof(ushort), typeof(NetSegment), typeof(bool) },
            new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void ReleaseSegmentImplementationPostfix(ushort segment) 
        {
            PathDistanceCache.Invalidate();
            OutsideConnectionCache.Invalidate();
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(NetAI), "CreateSegment")]
        [HarmonyPostfix]
        public static void CreateSegment(ushort segmentID, ref NetSegment data)
        {
            PathDistanceCache.Invalidate();
            OutsideConnectionCache.Invalidate();
        }
    }
}