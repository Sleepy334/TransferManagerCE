using HarmonyLib;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch]
    public class TransferManagerStartTransferPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
        public static bool Prefix(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // This gets called by vanilla or custom transfer manager (whichever is running) when a match occurs.
            if (MatchLogging.Instance != null)
            {
                MatchLogging.Instance.StartTransfer(material, offerOut, offerIn);
            }

            MatchStats.RecordMatch(material, offerOut, offerIn, delta);

            // Handle normally
            return true; 
        }
    } //TransferManagerStartTransferPatch
}