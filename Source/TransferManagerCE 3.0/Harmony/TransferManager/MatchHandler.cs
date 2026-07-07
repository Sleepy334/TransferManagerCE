// ----------------------------------------------------------------------------------------
namespace TransferManagerCore
{
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