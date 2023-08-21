﻿using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using ColossalFramework;
using static TransferManager;
using static TransferManagerCE.CustomTransferReason;
using UnityEngine;

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
        public static IEnumerable<CodeInstruction> HandleCrimeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));
            MethodInfo methodAddCrimeOffer = AccessTools.Method(typeof(CommonBuildingAIHandleCrime), nameof(CommonBuildingAIHandleCrime.AddCrimeOffer));

            // Find insert index and label
            bool bAddedBranch = false;
            bool bAddedLabel = false;
            Label jumpLabel = generator.DefineLabel();

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();

            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction = instructionsEnumerator.Current;

                if (!s_bPatched)
                {
                    // Look for the following:
                    // if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
                    if (!bAddedBranch && instruction.opcode == OpCodes.Ldarg_S && instruction.operand is byte && (byte)instruction.operand == 4)
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

                    // Look for the following:
                    // This line: Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                    // IL_0BF1: callvirt   System.Void TransferManager::AddOutgoingOffer(TransferManager.TransferReason.Crime, TransferOffer offer)
                    if (bAddedBranch && !bAddedLabel && instruction.opcode == OpCodes.Callvirt && instruction.operand == methodAddOutgoingOffer)
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
                        instruction.opcode = OpCodes.Ldarg_1; // Overwrite Opcodes.Ret so we keep labels (if any) before AddOffers
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Ldarg, 4);
                        yield return new CodeInstruction(OpCodes.Call, methodAddCrimeOffer);

                        // Add ret back in
                        yield return new CodeInstruction(OpCodes.Ret);

                        s_bPatched = true;
                        continue;
                    }
                }

                yield return instruction;
            }

            if (s_bPatched)
            {
                Debug.Log("Patching of CommonBuildingAI.HandleCrimeTranspiler succeeded");
            }
            else
            {
                Debug.LogError($"Patching of CommonBuildingAI.HandleCrimeTranspiler failed bAddedBranch: {bAddedBranch} bAddedLabel: {bAddedLabel} s_bPatched: {s_bPatched}");
            }
        }

        private static void AddCrimeOffer(ushort buildingID, ref Building buildingData, int iCitizenCount)
        {
            int crimeBuffer = buildingData.m_crimeBuffer;
            if (iCitizenCount != 0 && crimeBuffer > iCitizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
            {
                // Check if we have police vehicles responding
                int count = BuildingUtils.GetGuestVehicleCount(buildingData, TransferReason.Crime, (TransferReason)Reason.Crime2);
                if (count == 0)
                {
                    TransferOffer offer = default;
                    offer.Priority = Mathf.Clamp(crimeBuffer / Mathf.Max(1, iCitizenCount * 10), 0, 7);
                    offer.Building = buildingID;
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
                        Singleton<SimulationManager>.instance.m_randomizer.Int32(20U) == 0))
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
    }
}
