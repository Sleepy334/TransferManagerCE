using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace TransferManagerCE
{
    public static class Patcher {
        public const string HarmonyId = "Sleepy.TransferManagerCE";

        private static bool s_patched = false;
        private static bool s_bTaxiStandPatched = false;

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

                // Dead
                patchList.Add(typeof(HospitalAIProduceGoods)); // Dead bug
                patchList.Add(typeof(AuxiliaryBuildingAIProduceGoods)); // Dead bug

                // Improve on vanilla goods handlers
                patchList.Add(typeof(CommercialBuildingAISimulationStepActive));
                patchList.Add(typeof(ProcessingFacilityAISimulationStep));

                // Improved Sick Collection
                patchList.Add(typeof(CommonBuildingAIHandleSickPatch));
                patchList.Add(typeof(PrivateBuildingAISimulationStepPatch)); 
                patchList.Add(typeof(PlayerBuildingAISimulationStepActivePatch));
                patchList.Add(typeof(ResidentAIFindHospital)); // ResidentAI.FindHospital bug

                // Path failures
                patchList.Add(typeof(CarAIPathfindFailurePatch));

                // Mail2
                patchList.Add(typeof(Mail2PostOfficePatches));
                patchList.Add(typeof(Mail2PostVanPatches));

                // DistrictSelection
                patchList.Add(typeof(DistrictSelectionPatches));

                // Spawn patches
                patchList.Add(typeof(ShipSpawnPatches));
                patchList.Add(typeof(AircraftSpawnPatches));
                patchList.Add(typeof(AirportGateAIPatches));

                // Despawn patches
                patchList.Add(typeof(CargoTrainDespawnPatches));
                patchList.Add(typeof(CargoShipDespawnPatches));
                patchList.Add(typeof(CargoPlaneDespawnPatches));

                // Patch vanilla bugs in main game
                patchList.Add(typeof(ArriveAtTargetPatches)); // Fix vehicles spawning at outside connections then despawning
                patchList.Add(typeof(CheckPassengersPatches)); // Reset max wait time patch
                patchList.Add(typeof(CheckRoadAccessPatches)); // Override to set the train track as the access segemnt.
                patchList.Add(typeof(HumanAIPathfindFailure)); // MovingIn citizens should be released
                patchList.Add(typeof(ResidentAIUpdateWorkplace)); // Don't add offers for citizens that are about to be released.
                patchList.Add(typeof(StartPathFindPatches)); // Cargo Station infinite loop bug
                patchList.Add(typeof(TransportStationAIPatches)); // TransportStationAI.CreateIncomingVehicle bug introduced in H&T update
                patchList.Add(typeof(WarehouseAIPatches)); // WarehouseAI bugs introduced in H&T update

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

                // Improved taxi stand support
                PatchTaxiStandHandler();
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
            // Get all methods
            MethodInfo[] methods = patchType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Now check method name matches
            foreach (MethodInfo? method in methods)
            {
                //Debug.Log($"method: {method}.");
                if (method.Name.Equals(sMethod))
                {
                    harmony.Unpatch(method, HarmonyPatchType.All, HarmonyId);
#if DEBUG
                    Debug.Log($"{patchType}.{sMethod} unpatched.");
#endif
                }
            }
        }

        public static void PatchTaxiStandHandler()
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().TaxiMove)
            {
                if (!s_bTaxiStandPatched)
                {
#if DEBUG
                    Debug.Log("Patching taxi stand handler");
#endif
                    Patcher.Patch(typeof(TaxiAIPatch));
                    Patcher.Patch(typeof(TaxiStandAIPatch));
                    s_bTaxiStandPatched = true;
                }
            }
            else if (s_bTaxiStandPatched)
            {
#if DEBUG
                Debug.Log("Unpatch taxi stand handler");
#endif
                Patcher.Unpatch(typeof(TaxiAI), "SimulationStep");
                Patcher.Unpatch(typeof(TaxiStandAI), "ProduceGoods");
                s_bTaxiStandPatched = false;
            }
        }
    }
}
