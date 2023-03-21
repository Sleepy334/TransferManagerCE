using ColossalFramework;
using HarmonyLib;
using TransferManagerCE.TransferOffers;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class CommercialBuildingAISimulationStepActive
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CommercialBuildingAI), "SimulationStepActive")]
        public static void SimulationStepActivePostfix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            // Re-request more material if storage is empty and timer has reached MahjorProblem (64) and trucks aren't close by
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                buildingData.m_fireIntensity == 0 && 
                buildingData.m_customBuffer1 == 0 &&
                Singleton<SimulationManager>.instance.m_randomizer.UInt32(3U) == 0)
            {
                // Check if we are running out of time to get material
                Rerequest.ProblemLevel level = Rerequest.GetLevelIncomingTimer(buildingData.m_incomingProblemTimer);
                if (level != Rerequest.ProblemLevel.Level0)
                {
                    CommercialBuildingAI? buildingAI = buildingData.Info?.GetAI() as CommercialBuildingAI;
                    if (buildingAI is not null)
                    {
                        TransferReason incomingTransferReason = buildingAI.m_incomingResource;
                        if (incomingTransferReason != TransferReason.None)
                        {
                            // Do we need to check for LuxuryProducts as well
                            TransferReason secondary = TransferReason.None;
                            if (incomingTransferReason == TransferReason.Goods || incomingTransferReason == TransferManager.TransferReason.Food)
                            {
                                secondary = TransferReason.LuxuryProducts;
                            }

                            // Alternate requests
                            if (secondary != TransferReason.None && Singleton<SimulationManager>.instance.m_randomizer.UInt32(2U) == 0)
                            {
                                Rerequest.RerequestMaterial(secondary, incomingTransferReason, buildingID, buildingData);
                            }
                            else
                            {
                                Rerequest.RerequestMaterial(incomingTransferReason, secondary, buildingID, buildingData);
                            }
                        }
                    }
                }
            }
        }
    }
}
