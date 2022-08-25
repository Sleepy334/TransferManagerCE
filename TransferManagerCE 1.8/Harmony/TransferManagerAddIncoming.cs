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
                if (material == TransferReason.Dead && offer.Building != 0 && SaveGameSettings.GetSettings().ExperimentalDeathcare)
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
                        int iTotalHearses = cemeteryAI.m_hearseCount;
                        int iHearsesFree = Math.Max(0, iTotalHearses - iHearseCount - 3); // Reserve 3 hearses so there is reserve capacity

                        // Set offer amount to be the number of free vehicles available.
                        if (iHearsesFree > 0 && iCemeteryFree > 0 && building.m_flags != Building.Flags.Downgrading)
                        {
                            offer.Amount = Math.Min(iHearsesFree, iCemeteryFree);
                        }
                    }
                }
                else if (material == TransferReason.Garbage && offer.Building != 0 && SaveGameSettings.GetSettings().ExperimentalGarbage)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                    LandfillSiteAI? garbageAI = building.Info.m_buildingAI as LandfillSiteAI;
                    if (garbageAI != null)
                    {
                        int iTotalTrucks = garbageAI.m_garbageTruckCount;
                        int iCurrentCount = GetActiveVehicleCount(building, TransferReason.Garbage);
                        int iNewAmount = iTotalTrucks - 3 - iCurrentCount;
                        if (iNewAmount > offer.Amount)
                        {
                            offer.Amount = iNewAmount;
                        }
                    }
                }
                else if (material == TransferReason.Crime && offer.Building != 0 && SaveGameSettings.GetSettings().ExperimentalCrime)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                    PoliceStationAI? buildingAI = building.Info.m_buildingAI as PoliceStationAI;
                    if (buildingAI != null)
                    {
                        int iTotalVehicles = buildingAI.m_policeCarCount;
                        int iCurrentCount = GetActiveVehicleCount(building, TransferReason.Crime);
                        int iNewAmount = iTotalVehicles - 3 - iCurrentCount;
                        if (iNewAmount > offer.Amount)
                        {
                            offer.Amount = iNewAmount;
                        }
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
            }
            return iVehicles;
        }
    } //TransferManagerMatchOfferPatch
}