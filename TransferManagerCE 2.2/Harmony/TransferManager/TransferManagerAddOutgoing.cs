using HarmonyLib;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(TransferManager), "AddOutgoingOffer")]
    public class TransferManagerAddOutgoingPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferReason material, ref TransferOffer offer)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                if (SaveGameSettings.GetSettings().OverrideGenericIndustriesHandler &&
                    IndustrialBuildingAISimulationStepActive.s_bRejectOffers &&
                    TransferManagerModes.IsWarehouseMaterial(material))
                {
                    // Reject this offer as we are going to add our own instead
                    return false;
                }

                // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                ImprovedTransfers.ImprovedWarehouseMatchingOutgoing(ref offer);

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess(material, offer);
            } 

            // Update the stats for the specific material
            MatchStats.RecordAddOutgoing(material, offer);

            // Add offer to offer list and update transfers tab
            BuildingPanelThreadExtension.HandleOffer(offer);

            return true; // Handle normally
        }
    }
}