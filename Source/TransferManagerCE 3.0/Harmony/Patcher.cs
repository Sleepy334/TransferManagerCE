using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SleepyCommon;

namespace TransferManagerCore
{
    public static class Patcher {
#if TRANSFER_MANAGER_EXTENDED
        public const string HarmonyId = "Sleepy.TransferManagerExtended";
#else
        public const string HarmonyId = "Sleepy.TransferManagerCE";
#endif
        private static bool s_patched = false;
        private static bool s_bTaxiStandPatched = false;

        private static List<Type> GetPatchList()
        {
            List<Type> patchList = new List<Type>();

            // General patches
            patchList.Add(typeof(Patch.EscapePatch));

            // Transfer Manager harmony patches
            // patchList.Add(typeof(TransferManagerAwakePatch)); Patched much earlier in the loading process
            patchList.Add(typeof(TransferManagerPatches));

            // Crime2
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
            patchList.Add(typeof(SickHandlerPatch));
            patchList.Add(typeof(ResidentAIUpdateHealth));
            patchList.Add(typeof(BuildingRenderInstancePatch));

            // ForestFire
            patchList.Add(typeof(FirewatchTowerPatch));

            // Path failures
            patchList.Add(typeof(CarAIPathfindFailurePatch));
            patchList.Add(typeof(RelocateBuildingPatch));

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
            patchList.Add(typeof(CargoDespawnPatches));

            // Patch vanilla bugs in main game
            patchList.Add(typeof(ArriveAtTargetPatches)); // Fix vehicles spawning at outside connections then despawning
            patchList.Add(typeof(CheckPassengersPatches)); // Reset max wait time patch
            patchList.Add(typeof(CheckRoadAccessPatches)); // Override to set the train track as the access segemnt.
            patchList.Add(typeof(HumanAIPathfindFailure)); // MovingIn citizens should be released
            patchList.Add(typeof(ResidentAIUpdateWorkplace)); // Don't add offers for citizens that are about to be released.
            patchList.Add(typeof(StartPathFindPatches)); // Cargo Station infinite loop bug
            patchList.Add(typeof(TransportStationAIPatches)); // TransportStationAI.CreateIncomingVehicle bug introduced in H&T update
            patchList.Add(typeof(WarehouseAIPatches)); // WarehouseAI bugs introduced in H&T update
            patchList.Add(typeof(WarehouseStationAIPatch)); // Trains head to the wrong side of the cargo warehouse.
            patchList.Add(typeof(FindCargoStationPatch)); // CargoTruckAI.FindCargoStation has a bad bug and will not find the nearest cargo station quite often
            patchList.Add(typeof(IntercityBusPatch));

            // Outside connection patches
            if (DependencyUtils.IsAdvancedOutsideConnectionsRunning())
            {
                string sLogMessage = "Advanced Outside Connections detected, patches skipped:\r\n";
                sLogMessage += "OutsideConnectionAIPatch\r\n";
                sLogMessage += "OutsideConnectionAIGenerateNamePatch\r\n";
                Log.Info(sLogMessage);
            }
            else
            {
                patchList.Add(typeof(OutsideConnectionAIPatch));
                patchList.Add(typeof(OutsideConnectionAIGenerateNamePatch));
            }

            patchList.Add(typeof(CargoTruckAIPatch));

            // Vehicle AI Patches
            patchList.Add(typeof(PoliceVehicleAIPatch));
            patchList.Add(typeof(GarbageTruckAIPatch));
            patchList.Add(typeof(PostVanAIPatch));

            if (DependencyUtils.IsSmarterFireFightersRunning())
            {
                string sLogMessage = "Smarter Fire Fighters detected, patches skipped:\r\n";
                sLogMessage += "FireTruckAISimulationStepPostfix\r\n";
                sLogMessage += "FireCopterAISimulationStepPostfix\r\n";
                Log.Info(sLogMessage);
            }
            else
            {
                patchList.Add(typeof(FireVehicleAIPatch));
            }

            // Improved Employ Overeducated Workers
            patchList.Add(typeof(EmployOvereducatedWorkersPatch));

            patchList.Add(typeof(PathDistancePatches));


            // Autofill support
            patchList.Add(typeof(AutofillPatches));
            patchList.Add(typeof(AutofillArriveAtDestinationPatches));
            patchList.Add(typeof(AutofillCargoAIPatch));

            // Intercity Stops
            patchList.Add(typeof(TransportStationAIReversePatches));
#if DEBUG
            patchList.Add(typeof(CargoVehicleCheckPatch)); 
            patchList.Add(typeof(VehicleManagerPatch)); 
#endif

            return patchList;
        }


        public static void PatchAll() 
        {
            if (!s_patched)
            {
                s_patched = true;

                // Acutal patching
                Log.Info("");
                Log.Info($"Patching started...{HarmonyId}");
                Log.Separator();

                // Perform the patching
                PatchAll(GetPatchList());

                // Reversible patch functions
                PatchReversibleTranspilers();

                Log.Info("Patching finished.");
                Log.Separator();
                Log.Info("");
            }
        }

        public static void PatchReversibleTranspilers()
        {
            Log.Info("");
            Log.Info("Patching reversible transpilers");
            Log.Separator();

            ResidentAIFindHospitalTranspiler.PatchResidentAIFindHospital();

            // Generic industries handler is handled separately as we need to be able to unpatch it as well
            // as it uses a transpiler
            IndustrialBuildingAIGoodsPatch.PatchGenericIndustriesHandler();

            // Crime2 Handler
            CommonBuildingAIHandleCrime.PatchCrime2Handler();

            // Improved taxi stand support
            PatchTaxiStandHandler();

            Log.Separator();
            Log.Info("");
        }

        public static void UnpatchReversibleTranspilers()
        {
            Log.Info("");
            Log.Info("Unpatching reversible transpilers");
            Log.Separator();

            ResidentAIFindHospitalTranspiler.UnpatchResidentAIFindHospital();

            // Generic industries handler is handled separately as we need to be able to unpatch it as well
            // as it uses a transpiler
            IndustrialBuildingAIGoodsPatch.UnpatchGenericIndustriesHandler();

            // Crime2 Handler
            CommonBuildingAIHandleCrime.UnpatchCrime2Handler();

            // Improved taxi stand support
            UnpatchTaxiStandHandler();

            Log.Separator();
            Log.Info("");
        }

        public static void PatchAll(List<Type> patchList)
        {
            Log.Info("");
            Log.Info($"Patching:{patchList.Count} functions");
            Log.Separator();

            var harmony = new Harmony(HarmonyId);

            foreach (var patchType in patchList)
            {
                Patch(harmony, patchType);
            }

            Log.Separator();
            Log.Info("");
        }

        public static void UnpatchAll() {

            Log.Info("");
            Log.Info("Unpatching started");
            Log.Separator();

            UnpatchReversibleTranspilers();

            if (s_patched)
            {
                Log.Info($"Calling Harmony.UnpatchAll()");
                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
                s_patched = false;
            }
            
            Log.Info("Unpatching finished");
            Log.Separator();
            Log.Info("");
        }

        public static void Patch(Type classType)
        {
            Patch(new Harmony(HarmonyId), classType);
        }

        private static void Patch(Harmony harmony, Type classType)
        {
            Log.Info($"{classType}");
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
            Log.Info($"Unpatch: Class: {classType} Method: {sMethod} PatchType: {patchType}");

            // Get all methods
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Now check method name matches
            foreach (MethodInfo? method in methods)
            {
                //Debug.Log.Log($"method: {method}.");
                if (method.Name.Equals(sMethod))
                {
                    harmony.Unpatch(method, patchType, HarmonyId);
                    Log.Info($"{classType}.{method.Name} unpatched.");
                }
            }
        }

        public static void PatchTaxiStandHandler(bool bPatch)
        {
            if (bPatch) 
            {
                PatchTaxiStandHandler();
            }
            else
            {
                UnpatchTaxiStandHandler();
            }
        }

        public static void PatchTaxiStandHandler()
        {
            if (!s_bTaxiStandPatched &&
                SaveGameSettings.GetSettings().TaxiMove)
            {
                Log.Info("Patching taxi stand handler");
                Patcher.Patch(typeof(TaxiAIPatch));
                Patcher.Patch(typeof(TaxiStandAIPatch));
                s_bTaxiStandPatched = true;
            }
        }

        public static void UnpatchTaxiStandHandler()
        {
            if (s_bTaxiStandPatched)
            {
                Log.Info("Unpatch taxi stand handler");
                Patcher.Unpatch(typeof(TaxiAI), "SimulationStep");
                Patcher.Unpatch(typeof(TaxiStandAI), "ProduceGoods");
                s_bTaxiStandPatched = false;
            }
        }
    }
}
