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
            // disabled in settings? ->use stock transfer manager
            if (ModSettings.GetSettings().optionEnableNewTransferManager)
            {
                // Dispatch to TransferDispatcher
                CustomTransferDispatcher.Instance.SubmitMatchOfferJob(material);
                return false;
            } 
            else
            {
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