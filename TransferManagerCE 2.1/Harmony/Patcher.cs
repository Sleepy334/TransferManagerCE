using System;
using System.Collections.Generic;
using HarmonyLib;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public static class Patcher {
        public const string HarmonyId = "Sleepy.TransferManagerCE";

        private static bool s_patched = false;
        private static int s_iHarmonyPatches = 0;

        public static int GetHarmonyPatchCount() 
        { 
            return s_iHarmonyPatches; 
        }

        public static void PatchAll() 
        {
            if (!s_patched)
            {
                Debug.Log("Patching...");

                s_patched = true;
                var harmony = new Harmony(HarmonyId);

                List<Type> patchList = new List<Type>();

                // Transfer Manager harmony patches
                patchList.Add(typeof(Patch.TransferManagerMatchOfferPatch));
                patchList.Add(typeof(Patch.TransferManagerAddIncomingPatch));
                patchList.Add(typeof(Patch.TransferManagerAddOutgoingPatch));
                patchList.Add(typeof(Patch.TransferManagerStartTransferPatch));

                // Patch offer bugs in main game
                patchList.Add(typeof(Patch.AirportBuildingAIPatch)); // Crime TransferOffer
                patchList.Add(typeof(Patch.HospitalAIProduceGoods)); // Dead bug
                patchList.Add(typeof(Patch.AuxiliaryBuildingAIProduceGoods)); // Dead bug
                patchList.Add(typeof(Patch.ResidentAIFindHospital)); // ResidentAI.FindHospital bug 
                patchList.Add(typeof(Patch.IndustrialBuildingAISimulationStepActivePatch)); 

                // Improved Sick Collection
                patchList.Add(typeof(Patch.PrivateBuildingAISimulationStepPatch)); 
                patchList.Add(typeof(Patch.PlayerBuildingAISimulationStepActivePatch));
                
                // Path failures
                patchList.Add(typeof(Patch.CarAIPathfindFailurePatch));

                // Outside connection patches
                if (DependencyUtilities.IsAdvancedOutsideConnectionsRunning())
                {
                    string sLogMessage = "Advanced Outside Connections detected, patches skipped:\r\n";
                    sLogMessage += "OutsideConnectionAIPatch\r\n";
                    sLogMessage += "OutsideConnectionAIGenerateNamePatch\r\n";
                    Debug.Log(sLogMessage); 
                }
                else
                {
                    patchList.Add(typeof(Patch.OutsideConnectionAIPatch));
                    patchList.Add(typeof(Patch.OutsideConnectionAIGenerateNamePatch));
                }

                // Improved services AI patches
                patchList.Add(typeof(PoliceCarAISimulationStepPatch));
                patchList.Add(typeof(PoliceCopterAIAISimulationStepPatch));
                patchList.Add(typeof(GarbageTruckAIPatchSimulationStepPatch));

                if (DependencyUtilities.IsSmarterFireFightersRunning())
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
                
                s_iHarmonyPatches = patchList.Count;

                string sMessage = "Patching the following functions:\r\n";
                foreach (var patchType in patchList)
                {
                    sMessage += patchType.ToString() + "\r\n";
                    harmony.CreateClassProcessor(patchType).Patch();
                }
                Debug.Log(sMessage);
            }
        }

        public static void UnpatchAll() {
            if (s_patched)
            {
                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
                s_patched = false;

                Debug.Log("Unpatching...");
            }
        }

        public static int GetPatchCount()
        {
            var harmony = new Harmony(HarmonyId);
            var methods = harmony.GetPatchedMethods();
            int i = 0;
            foreach (var method in methods)
            {
                var info = Harmony.GetPatchInfo(method);
                if (info.Owners?.Contains(harmony.Id) == true)
                {
#if DEBUG
                    DebugLog.LogInfo($"Harmony patch method = {method.FullDescription()}");
                    if (info.Prefixes.Count != 0)
                    {
                        DebugLog.LogInfo("Harmony patch method has PreFix");
                    }
                    if (info.Postfixes.Count != 0)
                    {
                        DebugLog.LogInfo("Harmony patch method has PostFix");
                    }
#endif
                    i++;
                }
            }

            return i;
        }
    }
}
