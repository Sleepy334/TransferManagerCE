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
                if ((int)material < TransferManagerStats.s_Stats.Length)
                {
                    TransferManagerStats.s_Stats[(int)material].TotalOutgoingCount++;
                    TransferManagerStats.s_Stats[(int)material].TotalOutgoingAmount += offer.Amount;
                }

                // Update the stats
                if (TransferManagerStats.iMATERIAL_TOTAL_LOCATION < TransferManagerStats.s_Stats.Length)
                {
                    TransferManagerStats.s_Stats[TransferManagerStats.iMATERIAL_TOTAL_LOCATION].TotalOutgoingCount++;
                    TransferManagerStats.s_Stats[TransferManagerStats.iMATERIAL_TOTAL_LOCATION].TotalOutgoingAmount += offer.Amount;
                }
            }
            
            return true; // Handle normally
        }
    }
}