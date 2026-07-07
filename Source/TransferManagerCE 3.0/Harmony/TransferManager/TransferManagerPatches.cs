using System.Collections.Generic;
using HarmonyLib;
using SleepyCommon;
using TransferManagerCore.CustomManager;
using TransferManagerCore.UI;
using static TransferManager;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class TransferManagerPatches
    {
        // We specifically choose even numbers as we are less likely to clash with the base games numbers.
        // Also as the matching is done in separate threads I don't think we need the gap like they have done.
        private static Dictionary<int, CustomTransferReason.Reason> s_frameReasonList = new Dictionary<int, CustomTransferReason.Reason>()
        {
#if TRANSFER_MANAGER_EXTENDED
            // Prison Helicopter Mod
            { 100, CustomTransferReason.Reason.PoliceVanCriminalMove },
            { 102, CustomTransferReason.Reason.CriminalPickup2 },
            { 104, CustomTransferReason.Reason.CriminalMove2 },
#endif
            // Transfer Manager Extended/CE reasons
            { 148, CustomTransferReason.Reason.Crime2 },
            { 180, CustomTransferReason.Reason.TaxiMove },
            { 212, CustomTransferReason.Reason.Mail2 },
            { 214, CustomTransferReason.Reason.IntercityBus },
        };


        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(TransferManager), "AddIncomingOffer")]
        [HarmonyPrefix]
        public static bool AddIncomingOfferPrefix(ref TransferReason material, ref TransferOffer offer)
        {
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            // Pass through to Improved matching to adjust offer
            if (!ImprovedIncomingTransfers.HandleOffer(material, ref offer))
            {
                // If HandleIncomingOffer returns false then don't add offer to offers list
                return false;
            }

            // Update access segment if using path distance but do it in simulation thread so we don't break anything
            TransferManagerUtils.CheckRoadAccess((CustomTransferReason.Reason)material, offer);

            // Update the stats for the specific material
            MatchStats.RecordAddIncoming(material, offer.Amount);

            // Let building panel know a new offer is available
            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(TransferManager), "AddOutgoingOffer")]
        [HarmonyPrefix]
        public static bool AddOutgoingOfferPrefix(ref TransferReason material, ref TransferOffer offer)
        {
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            // Pass through to Improved matching to adjust offer
            if (!ImprovedOutgoingTransfers.HandleOffer(ref material, ref offer))
            {
                // If HandleOffer returns false then don't add offer to offers list
                return false;
            }

            // Update access segment if using path distance but do it in simulation thread so we don't break anything
            TransferManagerUtils.CheckRoadAccess((CustomTransferReason.Reason)material, offer);

            // Update the stats for the specific material
            MatchStats.RecordAddOutgoing(material, offer.Amount);

            // Let building panel know a new offer is available
            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }

        // ----------------------------------------------------------------------------------------
        // Patch GetFrameReason to support our new transfer reasons.
        [HarmonyPatch(typeof(TransferManager), "GetFrameReason")]
        [HarmonyPostfix]
        public static void GetFrameReasonPostfix(int frameIndex, ref TransferReason __result)
        {
            if (s_frameReasonList.TryGetValue(frameIndex, out CustomTransferReason.Reason reason))
            {
                if (__result == TransferReason.None)
                {
                    __result = (TransferReason) reason;
                }
                else
                {
                    Log.Error($"Error: FrameIndex {frameIndex} is in use by {__result}, {reason} not available.");
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        // Three underscores ___ in front of variable name allow you to have private members injected.
        [HarmonyPatch(typeof(TransferManager), "MatchOffers")] 
        [HarmonyPrefix]
        public static bool MatchOffersPrefix(TransferReason material,
                                    ref ushort[] ___m_incomingCount,
                                    ref ushort[] ___m_outgoingCount,
                                    TransferOffer[] ___m_incomingOffers,
                                    TransferOffer[] ___m_outgoingOffers,
                                    ref int[] ___m_incomingAmount,
                                    ref int[] ___m_outgoingAmount)
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

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(TransferManager), "MatchOffers")]
        [HarmonyPostfix]
        public static void MatchOffersPostfix()
        {
            // Start queued transfers:
            CustomTransferDispatcher.Instance.StartTransfers();
        }

        // ----------------------------------------------------------------------------------------
        // This gets called by vanilla transfer manager when a match occurs.
        [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
        [HarmonyPrefix]
        public static void StartTransferPrefix(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // Handle this match
            MatchHandler.Match(material, offerOut, offerIn, delta);
        }
    }
}