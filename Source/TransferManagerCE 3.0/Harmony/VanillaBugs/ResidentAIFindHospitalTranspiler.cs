using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TransferManagerCore.Settings;
using SleepyCommon;
using System.Linq;

namespace TransferManagerCore
{
    // ------------------------------------------------------------------------
    [HarmonyPatch]
    public static class ResidentAIFindHospitalTranspiler
    {
        private static bool s_bFindHospitalPatched = false;

        // --------------------------------------------------------------------
        public static void PatchResidentAIFindHospital()
        {
            if (!s_bFindHospitalPatched && ModSettings.GetSettings().FixFindHospital)
            {
                Log.Info("Patching ResidentAI.FindHospital...");
                Patcher.Patch(typeof(ResidentAIFindHospitalTranspiler));
            }
        }

        // --------------------------------------------------------------------
        public static void UnpatchResidentAIFindHospital()
        {
            if (s_bFindHospitalPatched)
            {
                Log.Info("Unpatching ResidentAI.FindHospital...");
                Patcher.Unpatch(typeof(ResidentAI), "FindHospital");
                s_bFindHospitalPatched = false;
            }
        }

        // --------------------------------------------------------------------
        // There is a bug in ResidentAI.FindHospital where it adds Childcare and Eldercare offers as AddOutgoingOffer half the time when it should always be AddIncomingOffer for a citizen
        [HarmonyPatch(typeof(ResidentAI), "FindHospital")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FindHospitalTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            // Have we already patched the function, if so just return unaltered.
            if (s_bFindHospitalPatched)
            {
                Log.Error($"ERROR: ResidentAIFindHospital.FindHospitalTranspiler - Already patched!");
                return instructions.AsEnumerable();
            }

            if (!ModSettings.GetSettings().FixFindHospital)
            {
                return instructions.AsEnumerable();
            }

            s_bFindHospitalPatched = true;

            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));

            bool bPatched = false;
            int iAddOutgoingCount = 0;

            // Instruction enumerator.
            List<CodeInstruction> newInstructionList = new List<CodeInstruction>();
            foreach (CodeInstruction instruction in instructions)
            {
                if (!bPatched)
                {
                    // We want to patch after the second call to AddOutgoingOffer
                    if (instruction.Calls(methodAddOutgoingOffer))
                    {
                        iAddOutgoingCount++;
                    }

                    // Now look for loading of argument "reason"
                    if (iAddOutgoingCount == 2 && instruction.opcode == OpCodes.Ldarg_3)
                    {
                        // We want to change this to always use transfer reason Sick
                        newInstructionList.Add(new CodeInstruction(OpCodes.Ldc_I4_S, (int)TransferManager.TransferReason.Sick) { labels = instruction.labels }); // Copy labels from Ldarg_3 instruction (if any)
                        bPatched = true;
                        continue; // Dont return original instruction
                    }
                }

                // Return normal instruction
                newInstructionList.Add(instruction);
            }

            Log.Info($"FindHospitalTranspiler - Patching of ResidentAI.FindHospital bug {(bPatched ? "succeeded" : "failed")}.");
            return newInstructionList.AsEnumerable();
        }
    }
}