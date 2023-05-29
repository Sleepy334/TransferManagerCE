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

                // Patch bugs in main game
                patchList.Add(typeof(HospitalAIProduceGoods)); // Dead bug
                patchList.Add(typeof(AuxiliaryBuildingAIProduceGoods)); // Dead bug
                patchList.Add(typeof(ResidentAIFindHospital)); // ResidentAI.FindHospital bug 
                patchList.Add(typeof(AirportBuildingAIHandleCrime)); // Reduce crime rate so it doesnt warn all the time.
                patchList.Add(typeof(TransportStationAICreateIncomingVehicle)); // TransportStationAI.CreateIncomingVehicle bug introduced in H&T update
                patchList.Add(typeof(WarehouseAIRemoveGuestVehicles)); // WarehouseAI bugs introduced in H&T update
                patchList.Add(typeof(ResidentAIUpdateWorkplace)); // Don't add offers for citizens that are about to be released.
                patchList.Add(typeof(HumanAIPathfindFailure)); // MovingIn citizens should be released

                // Improved Taxi stand support
                patchList.Add(typeof(TaxiAIPatch));
                patchList.Add(typeof(TaxiStandAIPatch));

                // Improve on vanilla goods handlers
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

                // Generic industries handler is handled separately as we need to be able to unpatch it as well
                // as it uses a transpiler
                IndustrialBuildingAISimulationStepActive.PatchGenericIndustriesHandler();

                // Crime2 Handler
                CommonBuildingAIHandleCrime.PatchCrime2Handler();
            }
        }

        public static void PatchAll(List<Type> patchList)
        {
#if DEBUG           
            Debug.Log($"Patching:{patchList.Count} functions");
#endif
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

        public static void Patch(Type patchType)
        {
            Patch(new Harmony(HarmonyId), patchType);
        }

        public static void Unpatch(Type patchType, string sMethod)
        {
            Unpatch(new Harmony(HarmonyId), patchType, sMethod);
        }

        private static void Patch(Harmony harmony, Type patchType)
        {
#if DEBUG
            Debug.Log($"Patch:{patchType}");
#endif
            PatchClassProcessor processor = harmony.CreateClassProcessor(patchType);
            processor.Patch();
        }

        private static void Unpatch(Harmony harmony, Type patchType, string sMethod)
        {
#if DEBUG
            Debug.Log($"Unpatch:{patchType} Method:{sMethod}");
#endif
            MethodInfo info = AccessTools.Method(patchType, sMethod);
            harmony.Unpatch(info, HarmonyPatchType.All, HarmonyId);
        }
    }
}
