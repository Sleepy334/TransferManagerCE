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
                    __result = (TransferReason)CustomTransferReason.Reason.Crime2;
                }
            }
        }
    }
}
