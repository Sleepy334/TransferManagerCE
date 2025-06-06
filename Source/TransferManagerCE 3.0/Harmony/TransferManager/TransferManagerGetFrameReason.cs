using HarmonyLib;
using SleepyCommon;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransferManagerGetFrameReason
    {
        // Patch GetFrameReason to support our new Crime2 reason. 149 was unused.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TransferManager), "GetFrameReason")]
        public static void GetFrameReasonPostFix(int frameIndex, ref TransferReason __result)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // We specifically choose even numbers as we are less likely to clash with the base games numbers.
                // Also as the matching is done in separate threads I don't think we need the gap like they have done.
                switch (frameIndex)
                {
                    case 148:
                        {
                            if (__result == TransferReason.None)
                            {
                                __result = (TransferReason)CustomTransferReason.Reason.Crime2;
                            }
                            else
                            {
                                CDebug.LogError($"Error: FrameIndex 148 is in use {__result}, Crime2 not available.");
                            }
                            break;
                        }
                    case 180:
                        {
                            if (__result == TransferReason.None)
                            {
                                __result = (TransferReason)CustomTransferReason.Reason.TaxiMove;
                            }
                            else
                            {
                                CDebug.LogError($"Error: FrameIndex 180 is in use {__result}, TaxiMove not available.");
                            }
                            break;
                        }
                    case 212:
                        {
                            if (__result == TransferReason.None)
                            {
                                __result = (TransferReason)CustomTransferReason.Reason.Mail2;
                            }
                            else
                            {
                                CDebug.LogError($"Error: FrameIndex 212 is in use {__result}, Mail2 not available.");
                            }
                            break;
                        }
                    case 214:
                        {
                            if (__result == TransferReason.None)
                            {
                                __result = (TransferReason)CustomTransferReason.Reason.IntercityBus;
                            }
                            else
                            {
                                CDebug.LogError($"Error: FrameIndex 212 is in use {__result}, IntercityBus not available.");
                            }
                            break;
                        }
                }
            }
        }
    }
}
