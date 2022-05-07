using System.Reflection;
using HarmonyLib;

namespace TransferManagerCE
{
    public static class Patcher {
        public const string HarmonyId = "Sleepy.TransferManagerCE";

        private static bool s_patched = false;

        public static void PatchAll() {
            if (!s_patched)
            {
                UnityEngine.Debug.Log("TransferManagerCE: Patching...");

                s_patched = true;
                var harmony = new Harmony(HarmonyId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
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
    }
}
