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
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                ImprovedMatching.ImprovedWarehouseMatchingOutgoing(ref offer);

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess(material, offer);
            }

            // Update the stats for the specific material
            MatchStats.RecordAddOutgoing(material, offer);

            BuildingPanelThread.HandleOffer(offer);

            return true; // Handle normally
        }
    }
}