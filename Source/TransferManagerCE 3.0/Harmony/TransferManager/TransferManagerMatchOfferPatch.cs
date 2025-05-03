using HarmonyLib;
using TransferManagerCE.CustomManager;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransferManagerMatchOfferPatch
    {
        // Three underscores ___ in front of variable name allow you to have private members injected.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TransferManager), "MatchOffers")]
        public static bool MatchOffersPrefix(TransferReason material, 
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
                // Support Employ Over Educated Workers
                switch (material)
                {
                    case TransferReason.Worker0:
                    case TransferReason.Worker1:
                    case TransferReason.Worker2:
                    case TransferReason.Worker3:
                        {
                            if (DependencyUtils.IsEmployOverEducatedWorkersRunning())
                            {
                                // Handle with Employ Overeducated Workers MatchOffers rather than ours
                                return true;
                            }
                            break;
                        }
                }

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
        [HarmonyPatch(typeof(TransferManager), "MatchOffers")]
        public static void MatchOffersPostfix()
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Start queued transfers:
                CustomTransferDispatcher.Instance.StartTransfers();
            }
        }
    } //TransferManagerMatchOfferPatch
}