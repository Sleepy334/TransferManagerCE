using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SleepyCommon;

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
                CDebug.Log("Patching...");
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
                //DO NOT ADD CommonBuildingAIHandleCrime HERE -- It's patched through PatchCrime2Handler instead.
                patchList.Add(typeof(CrimeCitizenCountPatches)); 

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
                patchList.Add(typeof(ResidentAITryMoveFamily));
                patchList.Add(typeof(ResidentAIFindHospital)); // ResidentAI.FindHospital bug
                patchList.Add(typeof(ResidentAIUpdateHealth));
                patchList.Add(typeof(BuildingRenderInstancePatch));

                // ForestFire
                patchList.Add(typeof(FirewatchTowerPatch)); 

                // Path failures
                patchList.Add(typeof(CarAIPathfindFailurePatch));

                // Mail
                patchList.Add(typeof(MaxMailPatch)); // Main area buildings have rediculously small mail buffers.
                patchList.Add(typeof(MaxMailTranspiler)); // Patch the area sub buildings to recognise the larger buffers.

                // Mail2
                patchList.Add(typeof(Mail2BuildingPatches));
                patchList.Add(typeof(Mail2PostVanPatches));

                // DistrictSelection
                patchList.Add(typeof(DistrictSelectionPatches));
                patchList.Add(typeof(DistrictEventPatches));
                
                // UnsortedMail
                patchList.Add(typeof(PostVanAIUnsortedMailPatch)); 

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
                    CDebug.Log(sLogMessage); 
                }
                else
                {
                    patchList.Add(typeof(OutsideConnectionAIPatch));
                    patchList.Add(typeof(OutsideConnectionAIGenerateNamePatch));
                }

                // Vehicle AI Patches
                patchList.Add(typeof(PoliceVehicleAIPatch));
                patchList.Add(typeof(GarbageTruckAIPatch));
                patchList.Add(typeof(PostVanAIPatch)); 

                if (DependencyUtils.IsSmarterFireFightersRunning())
                {
                    string sLogMessage = "Smarter Fire Fighters detected, patches skipped:\r\n";
                    sLogMessage += "FireTruckAISimulationStepPostfix\r\n";
                    sLogMessage += "FireCopterAISimulationStepPostfix\r\n";
                    CDebug.Log(sLogMessage);
                }
                else
                {
                    patchList.Add(typeof(FireVehicleAIPatch));
                }

                // Improved Employ Overeducated Workers
                patchList.Add(typeof(EmployOvereducatedWorkersPatch));

                patchList.Add(typeof(PathDistancePatches)); 

                // General patches
                patchList.Add(typeof(Patch.EscapePatch));

                // Perform the patching
                PatchAll(patchList);

                // Reversible patch functions
                PatchReversibleTranspilers();
            }
        }

        public static void PatchReversibleTranspilers()
        {
            // Generic industries handler is handled separately as we need to be able to unpatch it as well
            // as it uses a transpiler
            IndustrialBuildingAIGoodsPatch.PatchGenericIndustriesHandler();

            // Crime2 Handler
            CommonBuildingAIHandleCrime.PatchCrime2Handler();

            // Improved taxi stand support
            PatchTaxiStandHandler();
        }

        public static void PatchAll(List<Type> patchList)
        {
#if DEBUG           
            CDebug.Log($"Patching:{patchList.Count} functions", false);
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
                CDebug.Log("Unpatching...", false);
#endif
            }
        }

        public static void Patch(Type classType)
        {
            Patch(new Harmony(HarmonyId), classType);
        }

        private static void Patch(Harmony harmony, Type classType)
        {
#if DEBUG
            CDebug.Log($"Patch: {classType}", false);
#endif
            PatchClassProcessor processor = harmony.CreateClassProcessor(classType);
            processor.Patch();
        }
        
        public static void Unpatch(Type classType, string sMethod)
        {
            Unpatch(new Harmony(HarmonyId), classType, sMethod, HarmonyPatchType.All);
        }
        
        public static void Unpatch(Type classType, string sMethod, HarmonyPatchType patchType)
        {
            Unpatch(new Harmony(HarmonyId), classType, sMethod, patchType);
        }
        
        private static void Unpatch(Harmony harmony, Type classType, string sMethod, HarmonyPatchType patchType)
        {
#if DEBUG
            CDebug.Log($"Unpatch: Class: {classType} Method: {sMethod} PatchType: {patchType}", false);
#endif
            // Get all methods
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Now check method name matches
            foreach (MethodInfo? method in methods)
            {
                //Debug.Log.Log($"method: {method}.");
                if (method.Name.Equals(sMethod))
                {
                    harmony.Unpatch(method, patchType, HarmonyId);
#if DEBUG
                    CDebug.Log($"{classType}.{method.Name} unpatched.", false);
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
                    CDebug.Log("Patching taxi stand handler", false);
#endif
                    Patcher.Patch(typeof(TaxiAIPatch));
                    Patcher.Patch(typeof(TaxiStandAIPatch));
                    s_bTaxiStandPatched = true;
                }
            }
            else if (s_bTaxiStandPatched)
            {
#if DEBUG
                CDebug.Log("Unpatch taxi stand handler", false);
#endif
                Patcher.Unpatch(typeof(TaxiAI), "SimulationStep");
                Patcher.Unpatch(typeof(TaxiStandAI), "ProduceGoods");
                s_bTaxiStandPatched = false;
            }
        }
    }
}
