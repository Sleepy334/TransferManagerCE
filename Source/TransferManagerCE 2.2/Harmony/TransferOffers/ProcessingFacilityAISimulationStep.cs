using ColossalFramework;
using HarmonyLib;
using ICities;
using TransferManagerCE.TransferOffers;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    // Re-request patch
    [HarmonyPatch]
    public static class ProcessingFacilityAISimulationStep
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProcessingFacilityAI), "SimulationStep")]
        public static void PostFix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager)
            {
                // Use m_incomingProblemTimer in a similar way to Commercial buildings to help with re-request timings
                if ((buildingData.m_problems & Notification.Problem1.NoInputProducts) == Notification.Problem1.NoInputProducts ||
                    (buildingData.m_problems & Notification.Problem1.NoResources) == Notification.Problem1.NoResources ||
                    (buildingData.m_problems & Notification.Problem1.NoFishingGoods) == Notification.Problem1.NoFishingGoods)
                {
                    buildingData.m_incomingProblemTimer = (byte)Mathf.Min(255, buildingData.m_incomingProblemTimer + 1);
                }
                else
                {
                    buildingData.m_incomingProblemTimer = 0;
                }

                // Perform Re-request if needed.
                if (buildingData.m_incomingProblemTimer > 0)
                {
                    ProcessingFacilityAI? buildingAI = buildingData.Info?.m_buildingAI as ProcessingFacilityAI;
                    if (buildingAI != null)
                    {
                        if (buildingAI.m_inputResource1 != TransferManager.TransferReason.None)
                        {
                            if (buildingData.m_customBuffer2 == 0)
                            {
                                Rerequest.RerequestMaterial(buildingAI.m_inputResource1, TransferManager.TransferReason.None, buildingID, buildingData);
                            }
                        }
                        if (buildingAI.m_inputResource2 != TransferManager.TransferReason.None)
                        {
                            int iBuffer = (buildingData.m_teens << 8) | buildingData.m_youngs;
                            if (iBuffer == 0)
                            {
                                Rerequest.RerequestMaterial(buildingAI.m_inputResource2, TransferManager.TransferReason.None, buildingID, buildingData);
                            }
                        }
                        if (buildingAI.m_inputResource3 != TransferManager.TransferReason.None)
                        {
                            int iBuffer = (buildingData.m_adults << 8) | buildingData.m_seniors;
                            if (iBuffer == 0)
                            {
                                Rerequest.RerequestMaterial(buildingAI.m_inputResource3, TransferManager.TransferReason.None, buildingID, buildingData);
                            }
                        }
                        if (buildingAI.m_inputResource4 != TransferManager.TransferReason.None)
                        {
                            int iBuffer = (buildingData.m_education1 << 8) | buildingData.m_education2;
                            if (iBuffer == 0)
                            {
                                Rerequest.RerequestMaterial(buildingAI.m_inputResource4, TransferManager.TransferReason.None, buildingID, buildingData);
                            }
                        }
                    }
                }
            }
        }
    }
}
