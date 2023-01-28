using HarmonyLib;
using System.Runtime.CompilerServices;
using System;

namespace TransferManagerCE
{
    // We use a reverse patch on StartTransfer so we can call it directly as it's a private method
    [HarmonyPatch]
    public class TransferManagerStartTransferReversePatch
    {
        // This gets called by custom transfer manager when a match occurs.
        public static void StartTransfer(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // Handle this match
            MatchHandler.Match(material, offerOut, offerIn, delta);

            // Call actual TransferManager.StartTransfer function through our reverse patch
            StartTransferImpl(TransferManager.instance, material, offerOut, offerIn, delta);
        }

        // Harmony reverse patch to access private method TransferManager.StartTransfer.
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StartTransferImpl(object instance, TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            Debug.Log("StartTransfer reverse Harmony patch wasn't applied");
            throw new NotImplementedException("Harmony reverse patch not applied");
        }
    }

    // We Prefix patch StartTransfer so we can listen to matches when we are using the vanilla TransferManager.
    [HarmonyPatch]
    public class TransferManagerStartTransferVanillaPatch
    {
        // This gets called by vanilla transfer manager when a match occurs.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
        public static bool Prefix(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            // Handle this match
            MatchHandler.Match(material, offerOut, offerIn, delta);

            // Handle normally
            return true;
        }
    }

    public class MatchHandler
    {
        // This gets called by vanilla or custom transfer manager (whichever is running) when a match occurs.
        public static void Match(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            if (MatchLogging.Instance != null)
            {
                MatchLogging.Instance.StartTransfer(material, offerOut, offerIn);
            }

            MatchStats.RecordMatch(material, offerOut, offerIn, delta);
        }
    }
}