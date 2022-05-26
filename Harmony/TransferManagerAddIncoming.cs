using HarmonyLib;
using System;
using TransferManagerCE.CustomManager;
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
            if (material == TransferReason.Dead && offer.Building != 0 && ModSettings.GetSettings().TransferManagerExperimentalDeathcare)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                if (building.Info?.m_buildingAI is CemeteryAI)
                { 
                    CemeteryAI cemeteryAI = (CemeteryAI)building.Info.m_buildingAI;

                    // Determine free spots in cemetery / Crematorium
                    int iAmount;
                    int iMax;
                    cemeteryAI.GetMaterialAmount(offer.Building, ref building, TransferReason.Dead, out iAmount, out iMax);
                    int iCemeteryFree = iMax - iAmount;

                    // Determine how many free vehicles it has
                    int iHearseCount = GetActiveVehicleCount(building);
                    int iTotalHearses = cemeteryAI.m_hearseCount;
                    int iHearsesFree = iTotalHearses - iHearseCount;

                    // Set offer amount to be the number of free vehicles available.
                    if (iHearsesFree > 0 && building.m_flags != Building.Flags.Downgrading)
                    {
                        offer.Amount = Math.Min(iHearsesFree, iCemeteryFree);
                    }
                }
            }

            // Update the stats for the specific material
            if (ModSettings.GetSettings().StatisticsEnabled && TransferManagerStats.s_Stats != null)
            {
                TransferManagerStats.s_Stats[(int)material].m_stats.TotalIncomingCount++;
                TransferManagerStats.s_Stats[(int)material].m_stats.TotalIncomingAmount += offer.Amount;

                // Update the stats
                TransferManagerStats.s_Stats[255].m_stats.TotalIncomingCount++;
                TransferManagerStats.s_Stats[255].m_stats.TotalIncomingAmount += offer.Amount;
            }
            return true; // Handle normally
        }

        private static int GetActiveVehicleCount(Building building)
        {
            int iVehicles = 0;
            ushort usVehicleId = building.m_ownVehicles;
            while (usVehicleId != 0)
            {
                iVehicles++;

                // Update for next car
                Vehicle oVehicle = VehicleManager.instance.m_vehicles.m_buffer[usVehicleId];
                usVehicleId = oVehicle.m_nextOwnVehicle;
            }
            return iVehicles;
        }
    } //TransferManagerMatchOfferPatch
}