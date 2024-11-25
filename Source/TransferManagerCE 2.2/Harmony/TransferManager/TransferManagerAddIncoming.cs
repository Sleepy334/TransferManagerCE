using HarmonyLib;
using System;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(TransferManager), "AddIncomingOffer")]
    public class TransferManagerAddIncomingPatch
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

                // Pass through to Improved matching to adjust offer
                if (!ImprovedTransfers.HandleIncomingOffer(material, ref offer))
                {
                    // If HandleIncomingOffer returns false then don't add offer to offers list
                    return false;
                }

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess(material, offer);
            }

            // Update the stats for the specific material
            MatchStats.RecordAddIncoming(material, offer);

            // Let building panel know a new offer is available
            if (BuildingPanel.Instance != null)
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }
    } //TransferManagerMatchOfferPatch
}