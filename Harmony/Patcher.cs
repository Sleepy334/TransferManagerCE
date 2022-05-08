using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public static class Patcher {
        public const string HarmonyId = "Sleepy.TransferManagerCE";

        private static bool s_patched = false;
        private static int s_iHarmonyPatches = 0;

        public static int GetHarmonyPatchCount() { return s_iHarmonyPatches; }

        public static void PatchAll() {
            if (!s_patched)
            {
                UnityEngine.Debug.Log("TransferManagerCE: Patching...");

                s_patched = true;
                var harmony = new Harmony(HarmonyId);

                List<Type> patchList = new List<Type>();

                // Transfer Manager harmony patches
                patchList.Add(typeof(Patch.TransferManagerMatchOfferPatch));
                patchList.Add(typeof(Patch.CarAIPathfindFailurePatch));

                // Improved services AI patches
                if (!DependencyUtilities.IsSmarterFireFightersRunning())
                {
                    patchList.Add(typeof(Patch.FireCopterAIAISimulationStepPatch));
                    patchList.Add(typeof(Patch.FireTruckAISimulationStepPatch));
                }
                patchList.Add(typeof(Patch.Police.PoliceCarAISimulationStepPatch));
                patchList.Add(typeof(Patch.Police.PoliceCopterAIAISimulationStepPatch));
                patchList.Add(typeof(Patch.Garbage.GarbageTruckAIPatchSimulationStepPatch));

                s_iHarmonyPatches = patchList.Count;

                string sMessage = "Patching the following functions:\r\n";
                foreach (var patchType in patchList)
                {
                    sMessage += patchType.ToString() + "\r\n";
                    harmony.CreateClassProcessor(patchType).Patch();
                }
                Debug.Log(sMessage);

                //harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        public static void UnpatchAll() {
            if (s_patched)
            {
                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
                s_patched = false;

                UnityEngine.Debug.Log("TransferManagerCE: Unpatching...");
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
                    DebugLog.LogInfo($"Harmony patch method = {method.FullDescription()}");
                    if (info.Prefixes.Count != 0)
                    {
                        DebugLog.LogInfo("Harmony patch method has PreFix");
                    }
                    if (info.Postfixes.Count != 0)
                    {
                        DebugLog.LogInfo("Harmony patch method has PostFix");
                    }
                    i++;
                }
            }

            return i;
        }
    }
}
