using ColossalFramework;
using HarmonyLib;
using System;
using TransferManagerCE.Settings;
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
                switch (material)
                {
                    case TransferReason.Dead:
                        {
                            if (offer.Building != 0 && SaveGameSettings.GetSettings().ExperimentalDeathcare)
                            {
                                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                                CemeteryAI? cemeteryAI = building.Info.m_buildingAI as CemeteryAI;
                                if (cemeteryAI != null)
                                {
                                    // Determine free spots in cemetery / Crematorium
                                    int iAmount;
                                    int iMax;
                                    cemeteryAI.GetMaterialAmount(offer.Building, ref building, TransferReason.Dead, out iAmount, out iMax);
                                    int iCemeteryFree = iMax - iAmount;

                                    // Determine how many free vehicles it has
                                    int iHearseCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Dead);

                                    // Factor in budget
                                    int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                    int iTotalHearses = (productionRate * cemeteryAI.m_hearseCount + 99) / 100;
                                    int iHearsesFree = Math.Max(0, iTotalHearses - iHearseCount - 3); // Reserve 3 hearses so there is reserve capacity

                                    // Set offer amount to be the number of free vehicles available.
                                    if (iHearsesFree > 0 && iCemeteryFree > 0 && building.m_flags != Building.Flags.Downgrading)
                                    {
                                        offer.Amount = Math.Min(iHearsesFree, iCemeteryFree);
                                    }
                                }
                            }
                            break;
                        }
                    case TransferReason.Garbage:
                        {
                            if (offer.Building != 0 && SaveGameSettings.GetSettings().ExperimentalGarbage)
                            {
                                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                                LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                                if (garbageAI != null)
                                {
                                    // Factor in budget
                                    int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                    int iTotalTrucks = (productionRate * garbageAI.m_garbageTruckCount + 99) / 100;
                                    int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Garbage);

                                    int iNewAmount = iTotalTrucks - 3 - iCurrentCount;
                                    if (iNewAmount > offer.Amount)
                                    {
                                        offer.Amount = iNewAmount;
                                    }
                                }
                            }
                            break;
                        }
                    case TransferReason.Crime:
                        {
                            if (offer.Building != 0 && SaveGameSettings.GetSettings().ExperimentalCrime)
                            {
                                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                                PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                                if (buildingAI != null)
                                {
                                    // Factor in budget
                                    int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                                    int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                                    int iTotalVehicles = (productionRate * buildingAI.m_policeCarCount + 99) / 100;
                                    int iCurrentCount = CitiesUtils.GetActiveVehicleCount(building, TransferReason.Crime);
                                    
                                    int iNewAmount = iTotalVehicles - 3 - iCurrentCount;
                                    if (iNewAmount > offer.Amount)
                                    {
                                        offer.Amount = iNewAmount;
                                    }
                                }
                            }
                            break;
                        }
                    case TransferReason.Taxi:
                        {
                            if (offer.Citizen != 0)
                            {
                                Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                                if (citizen.m_flags != 0)
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
            }

            // Update the stats for the specific material
            MatchStats.RecordAddIncoming(material, offer);

            // Let building panel know a new offer is available
            BuildingPanelThread.HandleOffer(offer);

            return true; // Handle normally
        }
    } //TransferManagerMatchOfferPatch
}