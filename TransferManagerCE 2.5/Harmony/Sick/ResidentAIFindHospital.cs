using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class ResidentAIFindHospital
    {
        // There is a bug in ResidentAI.FindHospital where it adds Childcare and Eldercare offers as AddOutgoingOffer half the time when it should always be AddIncomingOffer for a citizen
        [HarmonyPatch(typeof(ResidentAI), "FindHospital")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FindHospitalTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            bool bPatchRequested = ModSettings.GetSettings().FixFindHospital;
            bool bPatched = false;

            int iAddOutgoingCount = 0;
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction = instructionsEnumerator.Current;

                if (bPatchRequested && !bPatched)
                {
                    // We want to patch after the second call to AddOutgoingOffer
                    if (instruction.opcode == OpCodes.Callvirt && instruction.operand == methodAddOutgoingOffer)
                    {
                        iAddOutgoingCount++;
                    }

                    // Now look for loading of argument "reason"
                    if (iAddOutgoingCount == 2 && instruction.opcode == OpCodes.Ldarg_3)
                    {
                        // We want to change this to always use transfer reason Sick
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, (int)TransferManager.TransferReason.Sick) { labels = instruction.labels }; // Copy labels from Ldarg_3 instruction (if any)
                        bPatched = true;
                        continue;
                    }
                }

                // Return normal instruction
                yield return instructionsEnumerator.Current;
            }

            if (bPatchRequested)
            {
                if (bPatched)
                {
                    Debug.Log("Patching of ResidentAI.FindHospital succeeded.");
                }
                else
                {
                    Debug.LogError($"Patching of ResidentAI.FindHospital failed.");
                }
            }
        }

        // Bypass the base games FindHospital function with our own fixed version if requested (OverrideResidentialSickHandler)
        [HarmonyPatch(typeof(ResidentAI), "FindHospital")]
        [HarmonyPrefix]
        public static bool Prefix(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason, ref bool __result)
        {
            if (sourceBuilding != 0 &&
                SaveGameSettings.GetSettings().EnableNewTransferManager && 
                SaveGameSettings.GetSettings().OverrideResidentialSickHandler)
            {
                // Bypass vanilla function as we will handle building collection ourselves
                __result = true;
                return false;
            }

            // Fall through to default ResidentAI.FindHospital
            return true; 
        }
    }
}