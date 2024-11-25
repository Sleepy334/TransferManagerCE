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
                                    int iHearseCount = GetActiveVehicleCount(building, TransferReason.Dead);

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
                                    int iCurrentCount = GetActiveVehicleCount(building, TransferReason.Garbage);

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
                                    int iCurrentCount = GetActiveVehicleCount(building, TransferReason.Crime);
                                    
                                    int iNewAmount = iTotalVehicles - 3 - iCurrentCount;
                                    if (iNewAmount > offer.Amount)
                                    {
                                        offer.Amount = iNewAmount;
                                    }
                                }
                            }
                            break;
                        }
                }
            }

            // Update the stats for the specific material
            TransferManagerStats.RecordAddIncoming(material, offer);

            BuildingPanelThread.HandleOffer(offer);

            return true; // Handle normally
        }

        private static int GetActiveVehicleCount(Building building, TransferReason material)
        {
            int iVehicles = 0;
            int iLoopCount = 0;
            ushort usVehicleId = building.m_ownVehicles;
            while (usVehicleId != 0)
            {
                // Check transfer type matches
                Vehicle oVehicle = VehicleManager.instance.m_vehicles.m_buffer[usVehicleId];
                if ((TransferReason)oVehicle.m_transferType == material)
                {
                    iVehicles++;
                }

                // Update for next car
                usVehicleId = oVehicle.m_nextOwnVehicle;

                // Check for bad list
                if (++iLoopCount > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return iVehicles;
        }
    } //TransferManagerMatchOfferPatch
}