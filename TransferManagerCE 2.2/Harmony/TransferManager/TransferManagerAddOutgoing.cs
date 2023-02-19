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
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            if (settings.EnableNewTransferManager)
            {
                if (settings.OverrideGenericIndustriesHandler &&
                    IndustrialBuildingAISimulationStepActive.s_bRejectOffers &&
                    TransferManagerModes.IsWarehouseMaterial(material))
                {
                    // Reject this offer as we are going to add our own instead
                    return false;
                }

                if (offer.Exclude)
                {
                    // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                    ImprovedTransfers.ImprovedWarehouseMatchingOutgoing(ref offer);
                }
                else if (offer.Building != 0 && // Check if import is completely disabled at the global level and don't add offer here
                    TransferRestrictions.IsImportRestrictionsSupported(material) &&
                    BuildingTypeHelper.IsOutsideConnection(offer.Building) &&
                    settings.IsWarehouseImportRestricted(material) &&
                    settings.IsImportRestricted(material))
                {
                    return false; // Don't add this offer as Import is completely restricted
                }

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess(material, offer);
            } 

            // Update the stats for the specific material
            MatchStats.RecordAddOutgoing(material, offer);

            // Let building panel know a new offer is available
            if (BuildingPanel.Instance != null)
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }
    }
}