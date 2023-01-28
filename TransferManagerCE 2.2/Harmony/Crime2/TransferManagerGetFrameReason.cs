using HarmonyLib;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransferManagerGetFrameReason
    {
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
