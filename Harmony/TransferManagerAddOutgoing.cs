using HarmonyLib;
using System;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
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
            if (ModSettings.GetSettings().StatisticsEnabled && TransferManagerStats.s_Stats != null)
            {
                TransferManagerStats.s_Stats[(int)material].m_stats.TotalOutgoingCount++;
                TransferManagerStats.s_Stats[(int)material].m_stats.TotalOutgoingAmount += offer.Amount;

                // Update the stats
                TransferManagerStats.s_Stats[255].m_stats.TotalOutgoingCount++;
                TransferManagerStats.s_Stats[255].m_stats.TotalOutgoingAmount += offer.Amount;
            }
            
            return true; // Handle normally
        }
    }
}