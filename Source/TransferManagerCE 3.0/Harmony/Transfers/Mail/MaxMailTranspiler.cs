using HarmonyLib;
using Mono.Cecil.Cil;
using SleepyCommon;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class MaxMailTranspiler
    {
        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(IndustryBuildingAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IndustryBuildingAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("IndustryBuildingAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(CampusBuildingAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CampusBuildingAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("CampusBuildingAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(AirportAuxBuildingAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AirportAuxBuildingAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("AirportAuxBuildingAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(AirportCargoGateAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AirportCargoGateAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("AirportCargoGateAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(AirportGateAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AirportGateAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("AirportGateAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(MuseumAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MuseumAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("MuseumAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        // We patch out the hard coded 2000 mail buffer in IndustryBuildingAI.HandleCommonConsumption
        // and replace it with the call to GetMaxMailBuffer.
        [HarmonyPatch(typeof(ParkBuildingAI), "HandleCommonConsumption")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ParkBuildingAIHandleCommonConsumptionTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            return HandleCommonConsumptionTranspiler("ParkBuildingAI.HandleCommonConsumption", generator, instructions);
        }

        // ----------------------------------------------------------------------------------------
        public static IEnumerable<CodeInstruction> HandleCommonConsumptionTranspiler(string functionName, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo methodGetMaxMailBuffer = typeof(MaxMailTranspiler).GetMethod("GetMaxMailBuffer", BindingFlags.Public | BindingFlags.Static);

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();

            bool bPatched = false;
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction = instructionsEnumerator.Current;

                if (methodGetMaxMailBuffer is not null &&
                    instruction.opcode == OpCodes.Ldc_I4 &&
                    (int)instruction.operand == 2000)
                {
                    bPatched = true;

                    // Replace hard coded 2000, with call to GetMaxMailBuffer below
                    yield return new CodeInstruction(OpCodes.Call, methodGetMaxMailBuffer) { labels = instruction.labels }; // Copy labels from instruction

                    // Skip original instruction
                    continue;
                }

                yield return instruction;
            }

            CDebug.Log($"Patching of {functionName} {(bPatched ? "succeeded" : "failed")}.", false);
        }

        // ----------------------------------------------------------------------------------------
        public static int GetMaxMailBuffer()
        {
            return Mathf.Max(2000, SaveGameSettings.GetSettings().MainBuildingMaxMail);
        }
    }
}
