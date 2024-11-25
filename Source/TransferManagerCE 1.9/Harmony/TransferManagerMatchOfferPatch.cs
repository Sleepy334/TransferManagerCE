using HarmonyLib;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(TransferManager), "MatchOffers")]
    public class TransferManagerMatchOfferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferManager.TransferReason material)
        {
            // Check if disabled in settings?
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Dispatch to TransferDispatcher
                CustomTransferDispatcher.Instance.SubmitMatchOfferJob(material);
                return false;
            } 
            else
            {
                // Handle with vanilla Transfer Manager
                return true;
            }
        }


        [HarmonyPostfix]
        public static void Postfix()
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Start queued transfers:
                TransferResultQueue.Instance.StartTransfers();
            }
        }

    } //TransferManagerMatchOfferPatch
}