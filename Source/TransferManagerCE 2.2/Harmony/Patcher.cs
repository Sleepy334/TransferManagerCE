using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace TransferManagerCE
{
    public static class Patcher {
        public const string HarmonyId = "Sleepy.TransferManagerCE";

        private static bool s_patched = false;

        public static void PatchAll() 
        {
            if (!s_patched)
            {
#if DEBUG
                Debug.Log("Patching...");
#endif
                s_patched = true;
                var harmony = new Harmony(HarmonyId);

                List<Type> patchList = new List<Type>();

                // Transfer Manager harmony patches
                patchList.Add(typeof(TransferManagerMatchOfferPatch));
                patchList.Add(typeof(TransferManagerAddIncomingPatch));
                patchList.Add(typeof(TransferManagerAddOutgoingPatch));
                patchList.Add(typeof(TransferManagerStartTransferVanillaPatch));

                // Crime2
                patchList.Add(typeof(TransferManagerGetFrameReason));
                patchList.Add(typeof(TransferManagerGetTransferReason1));
                patchList.Add(typeof(CommonBuildingAIHandleCrime)); 

                // Patch offer bugs in main game
                patchList.Add(typeof(HospitalAIProduceGoods)); // Dead bug
                patchList.Add(typeof(AuxiliaryBuildingAIProduceGoods)); // Dead bug
                patchList.Add(typeof(ResidentAIFindHospital)); // ResidentAI.FindHospital bug 
                patchList.Add(typeof(TransportStationAICreateIncomingVehicle)); // TransportStationAI.CreateIncomingVehicle bug
                patchList.Add(typeof(WarehouseAIRemoveGuestVehicles));

                // Improve on vanilla goods handlers
                patchList.Add(typeof(IndustrialBuildingAISimulationStepActive));
                patchList.Add(typeof(CommercialBuildingAISimulationStepActive));
                patchList.Add(typeof(ProcessingFacilityAISimulationStep));

                // Improved Sick Collection
                patchList.Add(typeof(PrivateBuildingAISimulationStepPatch)); 
                patchList.Add(typeof(PlayerBuildingAISimulationStepActivePatch));
                
                // Path failures
                patchList.Add(typeof(CarAIPathfindFailurePatch));

                // Outside connection patches
                if (DependencyUtils.IsAdvancedOutsideConnectionsRunning())
                {
                    string sLogMessage = "Advanced Outside Connections detected, patches skipped:\r\n";
                    sLogMessage += "OutsideConnectionAIPatch\r\n";
                    sLogMessage += "OutsideConnectionAIGenerateNamePatch\r\n";
                    Debug.Log(sLogMessage); 
                }
                else
                {
                    patchList.Add(typeof(OutsideConnectionAIPatch));
                    patchList.Add(typeof(OutsideConnectionAIGenerateNamePatch));
                }

                // Vehicle AI Patches
                patchList.Add(typeof(PoliceCarAISimulationStepPatch));
                patchList.Add(typeof(PoliceCopterAIAISimulationStepPatch));
                patchList.Add(typeof(GarbageTruckAIPatchSimulationStepPatch));

                if (DependencyUtils.IsSmarterFireFightersRunning())
                {
                    string sLogMessage = "Smarter Fire Fighters detected, patches skipped:\r\n";
                    sLogMessage += "FireCopterAIAISimulationStepPatch\r\n";
                    sLogMessage += "FireTruckAISimulationStepPatch\r\n";
                    Debug.Log(sLogMessage);
                }
                else
                {
                    patchList.Add(typeof(FireCopterAIAISimulationStepPatch));
                    patchList.Add(typeof(FireTruckAISimulationStepPatch));
                }

                // General patches
                patchList.Add(typeof(Patch.EscapePatch));

                // Perform the patching
                PatchAll(patchList);
            }
        }

        public static void PatchAll(List<Type> patchList)
        {
            Debug.Log($"Patching:{patchList.Count} functions");
            var harmony = new Harmony(HarmonyId);

            foreach (var patchType in patchList)
            {
                Patch(harmony, patchType);
            }
        }

        public static void UnpatchAll() {
            if (s_patched)
            {
                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
                s_patched = false;
#if DEBUG
                Debug.Log("Unpatching...");
#endif
            }
        }

        public static void Patch(Harmony harmony, Type patchType)
        {
#if DEBUG
            Debug.Log($"Patch:{patchType}");
#endif
            PatchClassProcessor processor = harmony.CreateClassProcessor(patchType);
            processor.Patch();
        }

        public static void Unpatch(Harmony harmony, Type patchType, string sMethod)
        {
#if DEBUG
            Debug.Log($"Patch:{patchType} Method:{sMethod}");
#endif
            MethodInfo info = AccessTools.Method(patchType, sMethod);
            harmony.Unpatch(info, HarmonyPatchType.All, HarmonyId);
        }
    }
}
