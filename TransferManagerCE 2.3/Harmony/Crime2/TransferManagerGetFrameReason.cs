using HarmonyLib;
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
                if (frameIndex == 149)
                {
                    if (__result == TransferReason.None)
                    {
                        __result = (TransferReason)CustomTransferReason.Reason.Crime2;
                    }
                    else
                    {
                        Debug.LogError($"Error: FrameIndex 149 is in use {__result}, Crime2 not available.");
                    }
                }
            }
        }
    }
}
