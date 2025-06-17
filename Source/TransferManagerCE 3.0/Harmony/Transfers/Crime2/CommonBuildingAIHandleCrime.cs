using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using ColossalFramework;
using static TransferManager;
using static TransferManagerCE.CustomTransferReason;
using UnityEngine;
using SleepyCommon;

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
                        CDebug.Log("Patching Crime2 handler...", false);
#endif
                        Patcher.Patch(typeof(CommonBuildingAIHandleCrime));
                    }
                }
                else if (s_bPatched)
                {
#if DEBUG
                    CDebug.Log("Unpatching Crime2 handler...", false);
#endif
                    Patcher.Unpatch(typeof(CommonBuildingAI), "HandleCrime", HarmonyPatchType.Transpiler);
                    s_bPatched = false;
                }
            }
        }

        // This transpiler patches CommonBuildingAI.HandleCrime to skip over the AddOutgoingOffer call so we can add our own instead
        [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
        [HarmonyTranspiler]
        //[HarmonyDebug]
        public static IEnumerable<CodeInstruction> HandleCrimeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            // Have we already patched the function, if so just return unaltered.
            if (s_bPatched)
            {
                CDebug.Log($"ERROR: CommonBuildingAI.HandleCrime - Already patched!", false);
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                }
                yield break;
            }

            s_bPatched = true;

            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));
            MethodInfo methodAddCrimeOffer = AccessTools.Method(typeof(CommonBuildingAIHandleCrime), nameof(CommonBuildingAIHandleCrime.AddCrimeOffer));

            // Find insert index and label
            bool bAddedBranch = false;
            bool bAddedLabel = false;
            bool bAddedAddOffers = false;

            Label jumpLabel = generator.DefineLabel();

            // citizenCount is argument 4
            CodeInstruction branchInstruction = new CodeInstruction(OpCodes.Ldarg_S, (byte)4);

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction = instructionsEnumerator.Current;

                // Look for the following:
                // if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)

                // Which produces the following IL code:
                // IL_0134: stfld      System.UInt16 Building::m_crimeBuffer
                // IL_0139: ldarg.2
                // IL_013A: ldfld System.UInt16 Building::m_crimeBuffer
                // IL_013F: stloc.3
                // IL_0140: ldarg.s    4
                // IL_0142: brfalse => Label8

                // citizenCount is argument 4
                if (!bAddedBranch)
                {
                    if (TranspilerUtils.CompareInstructions(instruction, branchInstruction))
                    {
                        // Look for Brfalse
                        if (instructionsEnumerator.MoveNext())
                        {
                            CodeInstruction instruction2 = instructionsEnumerator.Current;
                            if (instruction2.opcode == OpCodes.Brfalse)
                            {
                                bAddedBranch = true;

                                // AddOutgoingOffer section add Br instruction to new label
                                yield return new CodeInstruction(OpCodes.Br, jumpLabel) { labels = instruction.labels }; // Copy labels from Ldarg_S instruction

                                instruction.labels = new List<Label>(); // Clear labels from Ldarg_S
                            }

                            // Return instructions
                            yield return instruction;
                            yield return instruction2;

                            continue;
                        }
                    }
                }

                // Look for the following:
                // This line: Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                // IL_0BF1: callvirt   System.Void TransferManager::AddOutgoingOffer(TransferManager.TransferReason.Crime, TransferOffer offer)
                if (bAddedBranch && !bAddedLabel && instruction.Calls(methodAddOutgoingOffer))
                {
                    bAddedLabel = true;

                    // return current instruction 
                    yield return instruction;

                    // Now add the jump label to the next instruction
                    if (instructionsEnumerator.MoveNext())
                    {
                        CodeInstruction instruction2 = instructionsEnumerator.Current;

                        // Add the jump label for the branch to jump to.
                        instruction2.labels.Add(jumpLabel);
                        yield return instruction2;
                    }

                    continue;
                }

                // Now insert our AddOffers call at the end
                // IL_0C10: ret
                if (bAddedBranch && bAddedLabel && instruction.opcode == OpCodes.Ret)
                {
                    bAddedAddOffers = true;

                    instruction.opcode = OpCodes.Ldarg_1; // Overwrite Opcodes.Ret so we keep labels (if any) before AddOffers
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg, 4);
                    yield return new CodeInstruction(OpCodes.Call, methodAddCrimeOffer);

                    // Add ret back in
                    yield return new CodeInstruction(OpCodes.Ret);
                        
                    continue;
                }

                yield return instruction;
            }

            if (bAddedBranch && bAddedLabel && bAddedAddOffers)
            {
                CDebug.Log("HandleCrimeTranspiler - Patching of CommonBuildingAI.HandleCrime succeeded", false);
            }
            else
            {
                CDebug.Log($"HandleCrimeTranspiler - Patching of CommonBuildingAI.HandleCrime failed. bAddedBranch: {bAddedBranch} bAddedLabel: {bAddedLabel} bAddedAddOffers: {bAddedAddOffers}", false);
            }
        }

        private static void AddCrimeOffer(ushort buildingID, ref Building buildingData, int iCitizenCount)
        {
            // No citizens
            if (iCitizenCount == 0)
            {
                return;
            }

            // Dont request police when at a police station or prison.
            if (buildingData.Info is not null && buildingData.Info.GetService() == ItemClass.Service.PoliceDepartment)
            {
                return;
            }

            if (buildingData.m_crimeBuffer > 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0)
            {
                int iPriority = Mathf.Clamp(buildingData.m_crimeBuffer / Mathf.Max(1, iCitizenCount * 10), 0, 7);

                int iMinPriority = 2; // 2 is default vanilla

                // Check if "Tough on crime" is set.
                if (SaveGameSettings.GetSettings().PoliceToughOnCrime) 
                {
                    iMinPriority = 1;
                }

                if (iPriority >= iMinPriority)
                {
                    // Check if we have police vehicles responding
                    int count = BuildingUtils.GetGuestVehicleCount(buildingData, TransferReason.Crime, (TransferReason)Reason.Crime2);
                    if (count == 0)
                    {
                        AddOutgoingCrimeOffer(buildingID, ref buildingData, iPriority);
                        return; // Only add 1 offer
                    }
                }
            }

            // Occasionally add a low priority offer to arrest criminals
            if (SaveGameSettings.GetSettings().PoliceToughOnCrime && Singleton<SimulationManager>.instance.m_randomizer.Int32(50u) == 0)
            {
                // We only request a police car when there is more than 1 criminal in the building.
                int iCriminalCount = BuildingUtils.GetCriminalCount(buildingID, buildingData);
                if (iCriminalCount > 1)
                {
                    AddOutgoingCrimeOffer(buildingID, ref buildingData, 0);
                }
            }
        }

        private static void AddOutgoingCrimeOffer(ushort buildingId, ref Building buildingData, int iPriority)
        {
            TransferOffer offer = default;
            offer.Priority = iPriority;
            offer.Building = buildingId;
            offer.Position = buildingData.m_position;
            offer.Amount = 1;

            // Add support for the helicopter policy
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(buildingData.m_position);
            DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;

            // Occasionally add a Crime2 offer instead of a Crime offer
            TransferReason reason;
            if (DependencyUtils.IsNaturalDisastersDLC() &&
                ((buildingData.m_flags & Building.Flags.RoadAccessFailed) != 0 ||
                (servicePolicies & DistrictPolicies.Services.HelicopterPriority) != 0 ||
                Singleton<SimulationManager>.instance.m_randomizer.Int32(15U) == 0))
            {
                // Add Crime2 offer instead
                reason = (TransferReason)Reason.Crime2;
            }
            else
            {
                reason = TransferReason.Crime;
            }

            Singleton<TransferManager>.instance.AddOutgoingOffer(reason, offer);
        }
    }
}
