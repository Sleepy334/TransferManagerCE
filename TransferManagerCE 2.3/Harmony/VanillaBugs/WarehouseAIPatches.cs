﻿using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class WarehouseAIPatches
    {
        // A patch for the fish warehouses creating fishing boats instead of trucks.
        [HarmonyPatch(typeof(WarehouseAI), "GetTransferVehicleService")]
        [HarmonyPrefix]
        public static bool GetTransferVehicleServicePrefix(TransferManager.TransferReason material, ItemClass.Level level, ref Randomizer randomizer, ref VehicleInfo __result)
        {
            if (material == TransferReason.Fish)
            {
                __result = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Fishing, ItemClass.SubService.None, ItemClass.Level.Level1, VehicleInfo.VehicleType.Car);
                return false;
            }

            return true; // Handle normally
        }

        [HarmonyPatch(typeof(WarehouseAI), "ProduceGoods")]
        [HarmonyTranspiler]
        //[HarmonyDebug]
        public static IEnumerable<CodeInstruction> ProduceGoodsTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            // We want to find the set_Exclude function of TransferOffer.
            PropertyInfo proertySetExclude = AccessTools.Property(typeof(TransferOffer), nameof(TransferOffer.Exclude));
            MethodInfo methodSetExclude = proertySetExclude.GetSetMethod();

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();

            int iDowngradeCallCount = 0;
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction = instructionsEnumerator.Current;

                // ProduceGoods has a weird loop where it removes all trucks when an Empty mode warehouse ("Downgrading") is more than 20% full.
                // We remove this as we dont want it
                if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == (int)Building.Flags.Downgrading)
                {
                    iDowngradeCallCount++;

                    // Looking for start of if section:
                    // if ((buildingData.m_flags & Building.Flags.Downgrading) != 0)
                    if (iDowngradeCallCount == 2)
                    {
                        // Set the compare flag to 0 so the loop doesnt execute
                        instruction.operand = 0;
                        Debug.Log($"Second Downgrading call found setting to 0 to skip loop.");
                    }
                }

                // The function also stops requesting material once the warehouse hits 20% when in "Empty" mode.
                // This makes no sense so we disable the check by setting the float to 1.0 (100%) instead of 0.2 (20%)
                // We just change all instances to 1.0f as both times it is used make no sense.
                // if ((float)num < (float)m_storageCapacity * 0.2f)
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 0.2f)
                {
                    instruction.operand = 1.0f;
                    Debug.Log($"Setting 0.2 to be 1.0 to disable 20% limit");
                }

                // Patch all .Exclude calls to be true.
                if (instruction.opcode == OpCodes.Ldloc_S)
                {
                    if (instructionsEnumerator.MoveNext())
                    {
                        CodeInstruction instruction2 = instructionsEnumerator.Current;

                        if (instruction2.opcode == OpCodes.Call && instruction2.operand == methodSetExclude)
                        {
                            // Exclude should ALWAYS be true for warehouses.
                            instruction.opcode = OpCodes.Ldc_I4_1; // Load true
                            Debug.Log("Patching Exclude call");
                        }

                        yield return instruction;
                        yield return instruction2;
                        continue;
                    }
                }

                yield return instruction;
            }
        }
    }
}
