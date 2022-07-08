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
            // Check if disabled in settings? or not supported material
            if (ModSettings.GetSettings().optionEnableNewTransferManager)
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
            if (ModSettings.GetSettings().optionEnableNewTransferManager)
            {
                // Start queued transfers:
                CustomTransferDispatcher.Instance.StartTransfers();
            }
        }

    } //TransferManagerMatchOfferPatch
}