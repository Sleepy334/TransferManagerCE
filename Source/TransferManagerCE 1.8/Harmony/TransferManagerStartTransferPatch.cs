using HarmonyLib;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
    public class TransferManagerStartTransferPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferManager __instance, TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // This gets called by vanilla or custom transfer manager (whichever is running) when a match occurs.
            if (MatchLogging.instance != null)
            {
                MatchLogging.instance.StartTransfer(material, offerOut, offerIn, delta);
            }

            TransferManagerStats.RecordMatch(material, offerOut, offerIn, delta);

            // Handle normally
            return true; 
        }
    } //TransferManagerStartTransferPatch
}