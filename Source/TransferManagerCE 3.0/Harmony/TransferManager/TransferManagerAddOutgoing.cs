using ColossalFramework;
using HarmonyLib;
using TransferManagerCE.CustomManager;
using TransferManagerCE.UI;
using static RenderManager;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TransferManagerAddOutgoingPatch
    {
        [HarmonyPatch(typeof(TransferManager), "AddOutgoingOffer")]
        [HarmonyPrefix]
        public static bool AddOutgoingOfferPrefix(ref TransferReason material, ref TransferOffer offer)
        {
            SaveGameSettings settings = SaveGameSettings.GetSettings();

            if (settings.EnableNewTransferManager)
            {
                if (offer.Exclude)
                {
                    // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                    ImprovedTransfers.ImprovedWarehouseMatchingOutgoing(ref offer);
                }
                else if (IsGlobalImportDisabled(settings, (CustomTransferReason.Reason)material, offer))
                {
                    return false; // Don't add this offer as Import is completely restricted
                }
                else if (material == TransferReason.Taxi && settings.TaxiMove && offer.Building != 0)
                {
                    HandleOutgoingTaxiOffer(ref material, offer);
                }
                else if (material == TransferReason.Mail)
                {
                    HandleOutgoingMailOffer(ref material, offer);
                }

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess((CustomTransferReason.Reason) material, offer);
            } 

            // Update the stats for the specific material
            MatchStats.RecordAddOutgoing(material, offer.Amount);

            // Let building panel know a new offer is available
            if (BuildingPanel.Instance is not null)
            {
                BuildingPanel.Instance.HandleOffer(offer);
            }

            return true; // Handle normally
        }

        // Check if import is completely disabled at the global level and don't add offer here
        private static bool IsGlobalImportDisabled(SaveGameSettings settings, CustomTransferReason.Reason material, TransferOffer offer)
        {
            return offer.Building != 0 && 
                    OutsideConnectionAIPatch.IsInAddConnectionOffers &&
                    TransferManagerModes.IsImportRestrictionsSupported(material) &&
                    settings.IsWarehouseImportRestricted(material) &&
                    settings.IsImportRestricted(material);
        }

        private static void HandleOutgoingTaxiOffer(ref TransferReason material, TransferOffer offer)
        {
            // Check it is a Taxi Depot
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building];
            if (building.m_flags != 0 && building.Info is not null && building.Info.GetAI() is DepotAI)
            {
                // We occasionally allow a Taxi call from the depot so that they will still work if you don't have any taxi stands.
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) != 0)
                {
                    material = (TransferReason)CustomTransferReason.Reason.TaxiMove;
                }
                else
                {
                    offer.Amount = 1; // Only allow 1 taxi at a time to match so we keep some back for taxi stands
                }
            }
        }

        private static void HandleOutgoingMailOffer(ref TransferReason material, TransferOffer offer)
        {
            if (offer.Park != 0 && offer.m_isLocalPark == 0)
            {
                // This is a service point request, convert it to Mail2.
                material = (TransferReason)CustomTransferReason.Reason.Mail2;
            }
            else if (offer.Building != 0 && SaveGameSettings.GetSettings().MainBuildingPostTruck)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                if (building.m_flags != 0 && building.Info is not null)
                {
                    switch (building.Info.GetAI())
                    {
                        case ParkGateAI:
                        case AirportEntranceAI:
                        case MainIndustryBuildingAI:
                        case MainCampusBuildingAI:
                            {
                                // Collect mail with post trucks for these building types
                                material = (TransferReason)CustomTransferReason.Reason.Mail2;
                                break;
                            }
                    }
                }
            }
        }
    }
}