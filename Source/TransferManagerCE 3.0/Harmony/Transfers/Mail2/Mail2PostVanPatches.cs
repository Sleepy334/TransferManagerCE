using ColossalFramework;
using HarmonyLib;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class Mail2PostVanPatches
    {
        // ----------------------------------------------------------------------------------------
        // Post trucks take SortedMail to post offices and bring back unsorted mail.
        // Vanilla bug fix: When a post truck returns to a post sorting facility with UnsortedMail, the vehicles buffer isnt added to the building.
        [HarmonyPatch(typeof(PostVanAI), "ArriveAtSource")]
        [HarmonyPrefix]
        public static void ArriveAtSource(ushort vehicleID, ref Vehicle data)
        {
            if (ModSettings.GetSettings().FixPostTruckCollectingMail || SaveGameSettings.GetSettings().MainBuildingPostTruck)
            {
                if ((TransferReason)data.m_transferType == TransferReason.UnsortedMail &&
                    (data.m_flags & Vehicle.Flags.TransferToTarget) != 0 &&
                    data.m_targetBuilding == 0 &&
                    data.m_transferSize > 0 &&
                    data.m_sourceBuilding != 0)
                {
                    if (VehicleUtils.IsPostTruck(data))
                    {
                        ref Building building = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
                        if (BuildingTypeHelper.IsPostSortingFacility(building))
                        {
                            int amountDelta3 = data.m_transferSize;
                            if (amountDelta3 != 0)
                            {
                                // Add to building buffer and remove from truck.
                                building.Info.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref building, (TransferManager.TransferReason)data.m_transferType, ref amountDelta3);
                                data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - amountDelta3, 0, data.m_transferSize);
                            }
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        // If a post truck is being used to collect mail then return to base immediately afterwards
        [HarmonyPatch(typeof(PostVanAI), "ShouldReturnToSource")]
        [HarmonyPostfix]
        public static void ShouldReturnToSource(ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if (!__result && ModSettings.GetSettings().FixPostTruckCollectingMail || SaveGameSettings.GetSettings().MainBuildingPostTruck)
            {
                if (data.m_sourceBuilding != 0 &&
                    data.m_targetBuilding == 0 &&
                    (TransferReason)data.m_transferType == TransferReason.Mail &&
                    VehicleUtils.IsPostTruck(data))
                {
                    __result = true; // Should return to base
                }
            }
        }
    }
}
