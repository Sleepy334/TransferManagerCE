using HarmonyLib;
using UnityEngine;
using ColossalFramework;

namespace TransferManagerCE
{
    [HarmonyPatch]

    public class FirewatchTowerPatch
    {
        const int iEmergencyFireAmount = 10;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FirewatchTowerAI), "NearObjectInFire")]
        public static bool NearObjectInFire(ushort buildingID, ref Building data, InstanceID itemID, Vector3 itemPos, ref bool __result)
        {
            if (itemID.Type != InstanceType.Tree)
            {
                __result = false;
                return false; // Dont call vanilla function
            }
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.Collapsed)) != 0)
            {
                __result = false;
                return false; // Dont call vanilla function
            }

            uint uiTreesOnFire = 0;

            InstanceManager.Group group = Singleton<InstanceManager>.instance.GetGroup(itemID);
            if (group != null)
            {
                ushort disaster = group.m_ownerInstance.Disaster;
                if (disaster != 0)
                {
                    DisasterData disasterData = Singleton<DisasterManager>.instance.m_disasters.m_buffer[disaster];
                    if ((disasterData.m_flags & DisasterData.Flags.Significant) != 0)
                    {
                        Singleton<DisasterManager>.instance.DetectDisaster(disaster, located: true);

                        // How many trees are burning for this disaster
                        uiTreesOnFire = disasterData.m_treeFireCount;
                    }
                }
            }

            int count = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;
            BuildingUtils.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.ForestFire, ref count, ref cargo, ref capacity, ref outside);
            if (count < 3 || uiTreesOnFire >= iEmergencyFireAmount)
            {
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                if (uiTreesOnFire >= iEmergencyFireAmount)
                {
                    offer.Priority = 7; // Fire is getting out of hand, call all fire helicopters
                }
                else
                {
                    offer.Priority = Mathf.Max(8 - count - 1, 4);
                }
                offer.Building = buildingID;
                offer.Position = data.m_position;
                offer.Amount = 1;
                Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.ForestFire, offer);
            }

            __result = true;
            return false; // Dont call vanilla function
        }
    }
}
