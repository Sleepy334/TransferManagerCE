using HarmonyLib;

namespace TransferManagerCE
{
    // We Prefix patch StartTransfer so we can listen to matches when we are using the vanilla TransferManager.
    [HarmonyPatch]
    public class TransferManagerStartTransferVanillaPatch
    {
        // This gets called by vanilla transfer manager when a match occurs.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
        public static void StartTransferPrefix(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // Handle this match
            MatchHandler.Match(material, offerOut, offerIn, delta);
        }
    }

    public class MatchHandler
    {
        // This gets called by vanilla or custom transfer manager (whichever is running) when a match occurs.
        public static void Match(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            if (MatchLogging.Instance is not null)
            {
                MatchLogging.Instance.StartTransfer(material, offerOut, offerIn);
            }

            MatchStats.RecordMatch(material, offerOut, offerIn, delta);
        }
    }
}