using ColossalFramework;
using HarmonyLib;
using TransferManagerCore.Util;
using UnityEngine;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class RelocateBuildingPatch
    {
        // --------------------------------------------------------------------
        // Clear path failed stats when building moved.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingManager), "RelocateBuilding")]
        public static void RelocateBuilding(ushort building, Vector3 position, float angle)
        {
            PathFindFailure.ResetPathingStatistics(new InstanceID { Building = building });
        }

        // --------------------------------------------------------------------
        // We also remove any crime2 offers when building deactivated
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CommonBuildingAI), "BuildingDeactivated")]
        public static void BuildingDeactivatedPrefix(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer offer = new TransferManager.TransferOffer
            {
                Building = buildingID
            };
            Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason) CustomTransferReason.Reason.Crime2, offer);
        }
    }
}