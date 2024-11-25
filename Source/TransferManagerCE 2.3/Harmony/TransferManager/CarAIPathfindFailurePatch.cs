using ColossalFramework;
using HarmonyLib;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(CarAI), "PathfindFailure")]
    public class CarAIPathfindFailurePatch
    {
        public static int s_pathFailCount = 0;

        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle data)
        {
            InstanceID source = new InstanceID { Building = data.m_sourceBuilding };
            InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleID, data);
            Util.PathFindFailure.RecordPathFindFailure(source, target);
            s_pathFailCount++;
        }
    }
}