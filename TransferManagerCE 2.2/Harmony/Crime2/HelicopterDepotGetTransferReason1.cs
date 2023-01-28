using HarmonyLib;
using static TransferManager;
using TransferManagerCE;
using Epic.OnlineServices.Presence;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransferManagerGetTransferReason1
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HelicopterDepotAI), "GetTransferReason1")]
        public static void GetTransferReason1(HelicopterDepotAI __instance, ref TransferReason __result)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                switch (__instance.m_info.GetService())
                {
                    case ItemClass.Service.PoliceDepartment:
                        {
                            __result = (TransferReason)CustomTransferReason.Reason.Crime2;
                            break;
                        }
                }
            }
        }
    }
}