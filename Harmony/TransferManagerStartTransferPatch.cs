using HarmonyLib;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
    public class TransferManagerStartTransferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferManager __instance, TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // Record Transfer Matches
            TransferManagerCEThreading.StartTransfer(material, offerOut, offerIn, delta);

            // Handle normally
            return true; 
        }
    } //TransferManagerStartTransferPatch
}