using HarmonyLib;
using TransferManagerCE.CustomManager;
using TransferManagerCE.UI;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch(typeof(TransferManager), "AddIncomingOffer")]
    public class TransferManagerAddIncomingPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref TransferReason material, ref TransferOffer offer)
        {
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            if (settings.EnableNewTransferManager)
            {
                // Pass through to Improved matching to adjust offer
                if (!ImprovedTransfers.HandleIncomingOffer(material, ref offer))
                {
                    // If HandleIncomingOffer returns false then don't add offer to offers list
                    return false;
                }

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess((CustomTransferReason.Reason) material, offer);
            }

            // Update the stats for the specific material
            MatchStats.RecordAddIncoming(material, offer);

            // Let building panel know a new offer is available
            if (BuildingPanel.Instance is not null)
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }
    } //TransferManagerMatchOfferPatch
}