using HarmonyLib;
using SleepyCommon;
using TransferManagerCE.UI;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransferManagerAddOutgoingPatch
    {
        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(TransferManager), "AddOutgoingOffer")]
        [HarmonyPrefix]
        public static bool AddOutgoingOfferPrefix(ref TransferReason material, ref TransferOffer offer)
        {
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            if (settings.EnableNewTransferManager)
            {
                // Pass through to Improved matching to adjust offer
                if (!ImprovedOutgoingTransfers.HandleOffer(ref material, ref offer))
                {
                    // If HandleOffer returns false then don't add offer to offers list
                    return false;
                }

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                TransferManagerUtils.CheckRoadAccess((CustomTransferReason.Reason) material, offer);
            }

            // Update the stats for the specific material
            MatchStats.RecordAddOutgoing(material, offer.Amount);

            // Let building panel know a new offer is available
            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }
    }
}