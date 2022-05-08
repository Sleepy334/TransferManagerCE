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


#if (DEBUG_VANILLA)
    [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
    public class TransferManagerStartTransferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferManager __instance, TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            if (material == TransferManager.TransferReason.Dead)
                DebugLog.DebugMsg($"[VANILLA StartTransfer]: {material}, out: {offerOut.Priority},{offerOut.Active},{offerOut.Building}/{offerOut.Vehicle} -- in: {offerIn.Priority},{offerIn.Active},{offerIn.Building}/{offerIn.Vehicle} -- amt: {delta}");

            return true;
        }
    } //TransferManagerStartTransferPatch
#endif

}