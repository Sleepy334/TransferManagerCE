using ColossalFramework;
using HarmonyLib;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(CarAI), "PathfindFailure")]
    public class CarAIPathfindFailurePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle data)
        {
            Util.PathFindFailure.RecordPathFindFailure(vehicleID, ref data);
        }
    }
}