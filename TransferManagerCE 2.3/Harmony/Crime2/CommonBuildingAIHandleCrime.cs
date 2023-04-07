using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TransferManagerCE.TransferOffers;
using System.Linq;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class CommonBuildingAIHandleCrime
    {
        private static bool s_bPatched = false;

        public static void PatchCrime2Handler()
        {
            if (DependencyUtils.IsNaturalDisastersDLC())
            {
                if (SaveGameSettings.GetSettings().EnableNewTransferManager)
                {
                    if (!s_bPatched)
                    {
#if DEBUG
                        Debug.Log("Patch Crime2 handler");
#endif
                        Patcher.Patch(typeof(CommonBuildingAIHandleCrime));
                    }
                }
                else if (s_bPatched)
                {
#if DEBUG
                    Debug.Log("Unpatch Crime2 handler");
#endif
                    Patcher.Unpatch(typeof(CommonBuildingAI), "HandleCrime");
                    s_bPatched = false;
                }
            }
        }

        // This transpiler patches CommonBuildingAI.HandleCrime to skip over the AddOutgoingOffer call so we can add our own instead
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> HandleCrimeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));
            MethodInfo methodAddCrimeOffer = AccessTools.Method(typeof(CrimeHandler), nameof(CrimeHandler.AddCrimeOffer));

            // Find insert index and label
            int iInsertIndex = 0;
            Label? jumpLabel = null;

            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count - 1; i++)
            {
                CodeInstruction instruction1 = codes[i];
                CodeInstruction instruction2 = codes[i + 1];

                // We are looking for this line, citizenCount is argument 4.
                // if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
                if (iInsertIndex == 0 && 
                    instruction1.opcode == OpCodes.Ldarg_S && instruction1.operand is byte && (byte)instruction1.operand == 4 &&
                    instruction2.opcode == OpCodes.Brfalse)
                {
                    // AddOutgoingOffer section 
                    iInsertIndex = i;
                }
                else if (iInsertIndex > 0 && instruction1.opcode == OpCodes.Callvirt && instruction1.operand == methodAddOutgoingOffer) // This line: Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                {
                    // Store the label to jump to
                    if (instruction2.opcode == OpCodes.Ldarg_0 && instruction2.labels.Count > 0)
                    {
                        jumpLabel = (Label)instruction2.labels[0];
                        break;
                    }
                }
            }

            // Transpile instructions
            if (iInsertIndex > 0 && jumpLabel is not null)
            {
                // Move labels (if any)
                CodeInstruction instruction = codes[iInsertIndex];

                // Add jump instruction
                CodeInstruction jumpInstruction = new CodeInstruction(OpCodes.Br, jumpLabel) { labels = instruction.labels }; // Relocate labels
                codes.Insert(iInsertIndex, jumpInstruction);

                // Clear old label references
                instruction.labels = new List<Label>();

                // Now call our AddCrimeOffer function at the end of function instead
                codes[codes.Count - 1].opcode = OpCodes.Ldarg_1; // Overwrite Opcodes.Ret so we keep labels (if any) before AddCrimeOffer call
                codes.Add(new CodeInstruction(OpCodes.Ldarg_2));
                codes.Add(new CodeInstruction(OpCodes.Ldarg, 4));
                codes.Add(new CodeInstruction(OpCodes.Call, methodAddCrimeOffer));

                // Now add back on the end Opcodes.Ret
                codes.Add(new CodeInstruction(OpCodes.Ret));

                s_bPatched = true;

                Debug.Log("Patching of HandleCrimeTranspiler succeeded");
            }
            else
            {
                Debug.LogError("Patching of HandleCrimeTranspiler failed");
            }

            return codes;
        }
    }
}
