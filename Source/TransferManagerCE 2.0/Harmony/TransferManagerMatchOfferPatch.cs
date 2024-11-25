using HarmonyLib;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(TransferManager), "MatchOffers")]
    public class TransferManagerMatchOfferPatch
    {
        // Three underscores ___ in front of variable name allow you to have private members injected.
        [HarmonyPrefix]
        public static bool Prefix(TransferReason material, 
                                    ref ushort[] ___m_incomingCount, 
                                    ref ushort[] ___m_outgoingCount, 
                                    TransferOffer[] ___m_incomingOffers, 
                                    TransferOffer[] ___m_outgoingOffers, 
                                    ref int[] ___m_incomingAmount,
                                    ref int[] ___m_outgoingAmount)
        {
            // Check if disabled in settings?
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Dispatch to TransferDispatcher
                CustomTransferDispatcher.Instance.SubmitMatchOfferJob(material, ref ___m_incomingCount, ref ___m_outgoingCount, ___m_incomingOffers, ___m_outgoingOffers, ref ___m_incomingAmount, ref ___m_outgoingAmount);
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