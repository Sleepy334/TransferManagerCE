using HarmonyLib;
using static TransferManager;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(TransferManager), "AddOutgoingOffer")]
    public class TransferManagerAddOutgoingPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferReason material, ref TransferOffer offer)
        {
            // Update the stats for the specific material
            TransferManagerStats.RecordAddOutgoing(material, offer);

            BuildingPanelThread.HandleOffer(offer);

            return true; // Handle normally
        }
    }
}