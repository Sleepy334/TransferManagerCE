using ColossalFramework;
using HarmonyLib;
using System;
using static TransferManager;

namespace TransferManagerCE.Patch
{
    [HarmonyPatch(typeof(TransferManager), "AddIncomingOffer")]
    public class TransferManagerAddIncomingPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TransferReason material, ref TransferOffer offer)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;

                switch (material)
                {
                    case TransferReason.Dead:
                        {
                            ImprovedMatching.ImprovedDeadMatchingIncoming(Buildings, ref offer);                           
                            break;
                        }
                    case TransferReason.Garbage:
                        {
                            ImprovedMatching.ImprovedGarbageMatchingIncoming(Buildings, ref offer);
                            break;
                        }
                    case TransferReason.Crime:
                        {
                            ImprovedMatching.ImprovedCrimeMatchingIncoming(Buildings, ref offer);
                            break;
                        }
                    case TransferReason.Sick:
                        {
                            ImprovedMatching.ImprovedSickMatchingIncoming(Buildings, ref offer);
                            break;
                        }
                    case TransferReason.Taxi:
                        {
                            if (offer.Citizen != 0)
                            {
                                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                                if (citizen.m_flags != 0 && citizen.m_instance != 0)
                                {
                                    ref CitizenInstance instance = ref CitizenManager.instance.m_instances.m_buffer[citizen.m_instance];
                                    if (instance.m_flags != 0)
                                    {
                                        if (instance.m_sourceBuilding != 0 && BuildingTypeHelper.IsOutsideConnection(instance.m_sourceBuilding))
                                        {
                                            // Taxi's do not work when cims coming from outside connections
                                            //Debug.Log($"Citizen: {offer.Citizen} Waiting for taxi at outside connection {instance.m_sourceBuilding} - SKIPPING");

                                            // Speed up waiting
                                            if (instance.m_waitCounter > 0)
                                            {
                                                instance.m_waitCounter = (byte)Math.Max((int)instance.m_waitCounter, 254);
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }

                // Adjust Priority of warehouse offers if ImprovedWarehouseMatching enabled
                ImprovedMatching.ImprovedWarehouseMatchingIncoming(Buildings, ref offer);

                // Update access segment if using path distance but do it in simulation thread so we don't break anything
                CitiesUtils.CheckRoadAccess(material, offer);
            }

            // Update the stats for the specific material
            MatchStats.RecordAddIncoming(material, offer);

            // Let building panel know a new offer is available
            BuildingPanelThread.HandleOffer(offer);

            return true; // Handle normally
        }
    } //TransferManagerMatchOfferPatch
}