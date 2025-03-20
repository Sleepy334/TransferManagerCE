using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TransferManagerCE.TransferOffers;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class IndustrialBuildingAISimulationStepActive
    {
        private static bool s_bPatched = false;
        private static int? s_maxLoadSize = null;

        public static void PatchGenericIndustriesHandler()
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideGenericIndustriesHandler)
            {
                if (!s_bPatched)
                {
#if DEBUG
                    Debug.Log("Patch generic industries handler");
#endif
                    Patcher.Patch(typeof(IndustrialBuildingAISimulationStepActive));
                }
            }
            else if (s_bPatched)
            {
#if DEBUG
                Debug.Log("Unpatch generic industries handler");
#endif
                Patcher.Unpatch(typeof(IndustrialBuildingAI), "SimulationStepActive");
                s_bPatched = false;
            }
        }

        // This transpiler patches SimulationStepActive to skip over the AddIncomingOffer and AddOutgoingOffer calls so we can add our own instead in AddOffers
        [HarmonyPatch(typeof(IndustrialBuildingAI), "SimulationStepActive")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IndustrialBuildingSimulationStepActiveTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo methodAddOffers = AccessTools.Method(typeof(IndustrialBuildingAISimulationStepActive), nameof(AddOffers));
            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));
            FieldInfo m_fireIntensity = AccessTools.Field(typeof(Building), "m_fireIntensity");

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
                    // if (buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
                    // IL_0A9D: ldarg.2
                    // IL_0A9E: ldfld System.Byte Building::m_fireIntensity
                    if (!bAddedBranch && instruction.opcode == OpCodes.Ldarg_2)
                    {
                        // Look for buildingData.m_fireIntensity
                        if (instructionsEnumerator.MoveNext())
                        {
                            CodeInstruction instruction2 = instructionsEnumerator.Current;

                            if (instruction2.opcode == OpCodes.Ldfld && instruction2.operand == m_fireIntensity)
                            {
                                bAddedBranch = true;

                                // AddIncomingOffer section add Br instruction to new label
                                yield return new CodeInstruction(OpCodes.Br, jumpLabel) { labels = instruction.labels }; // Copy labels from Ldarg_2 instruction

                                instruction.labels = new List<Label>(); // Clear labels from Ldarg_2
                            }

                            // Return instructions
                            yield return instruction;
                            yield return instruction2;

                            continue;
                        }
                    }

                    // Look for the following:
                    // Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer2);
                    // IL_0BF1: callvirt   System.Void TransferManager::AddOutgoingOffer(TransferReason material, TransferOffer offer)
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
                        instruction.opcode = OpCodes.Ldarg_1; // Overwrite Opcodes.Ret so we keep labels (if any) before AddOffers
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call, methodAddOffers);

                        // Add ret back in
                        yield return new CodeInstruction(OpCodes.Ret);

                        s_bPatched = true;
                        continue;
                    }
                }

                yield return instruction;
            }

            Debug.Log($"IndustrialBuildingSimulationStepActiveTranspiler - Patching of IndustrialBuildingAI.SimulationStepActive {(s_bPatched ? "succeeded" : "failed")}", false);
        }

        // Generic processing buildings behave badly, they ask twice in one round and with really high priority due to a bug in IndustrialBuildingAI.SimulationStepActive
        // where it uses the max load capacity (8) instead of storage capacity (up to 16) so priority is often twice what it should be.
        // This patch is an attempt to solve this
        public static void AddOffers(ushort buildingID, ref Building buildingData)
        {
            Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

            // We don't request every time so it gives time for a vehicle to be matched and dispatched before we request again.
            if (buildingData.m_fireIntensity == 0 && random.UInt32(3U) == 0)
            {
                TransferReason primary = GetIncomingTransferReason(buildingID, buildingData.Info);
                if (primary != TransferReason.None)
                {
                    IndustrialBuildingAI? processingAI = buildingData.Info.GetAI() as IndustrialBuildingAI;
                    if (processingAI is not null)
                    {
                        // Determine priority based on current storage level
                        int iProductionCapacity = processingAI.CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), buildingData.Width, buildingData.Length);
                        int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);

                        // Factor in trucks on the way but if timer is ticking up, ignore far away trucks
                        TransferReason secondary = GetSecondaryIncomingTransferReason(buildingID, buildingData.Info);
                        Rerequest.ProblemLevel level = Rerequest.GetLevelIncomingTimer(buildingData.m_incomingProblemTimer);
                        int iTransferSize = Rerequest.GetNearbyGuestVehiclesTransferSize(buildingData, level, primary, secondary, out int iTotalTrucks);
                        if (iTotalTrucks < 10)
                        {
                            // Calculate a more realistic priority
                            int iPriority = Mathf.Clamp((iStorageCapacity - buildingData.m_customBuffer1 - iTransferSize) * 8 / iStorageCapacity, 0, 7);

                            // We clamp priority to 6 until timer starts so we can target the buildings with notification icons first
                            if (iPriority == 7 && buildingData.m_incomingProblemTimer == 0)
                            {
                                iPriority = 6;
                            }

                            TransferOffer offer = default;
                            offer.Priority = iPriority;
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = 1;
                            offer.Active = false;

                            if (iPriority >= 3)
                            {
                                // Add new offer
                                // Alternate primary/secondary offer
                                if (secondary != TransferReason.None && random.UInt32(2U) == 0)
                                {
                                    Singleton<TransferManager>.instance.AddIncomingOffer(secondary, offer);
                                }
                                else
                                {
                                    Singleton<TransferManager>.instance.AddIncomingOffer(primary, offer);
                                }
                            }
                        }
                    }
                }
            }

            // We override the default outgoing offer so we can factor in the problem timer value into priority
            // to ensure buildings with the flashing icon get processed first
            // We don't request every time so it gives time for a vehicle to be matched and
            // dispatched before we request again.
            TransferManager.TransferReason outgoingReason = GetOutgoingTransferReason(buildingData.Info);
            if (outgoingReason != TransferReason.None && 
                buildingData.m_fireIntensity == 0 &&  
                random.UInt32(2U) == 0)
            {
                IndustrialBuildingAI? buildingAI = buildingData.Info.GetAI() as IndustrialBuildingAI;
                if (buildingAI is not null)
                {
                    // Check we have vehicles available before adding offer
                    int iVehicles = BuildingUtils.GetOwnVehicleCount(buildingData, outgoingReason);
                    int iMaxVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.GenericFactory, buildingID, 0);
                    if (iVehicles < iMaxVehicles)
                    {
                        TransferManager.TransferOffer offer = default;

                        // P:0 at storage 8
                        // P:2 at Storage 12
                        // Then scale with Outgoing timer after that
                        int iMaxVehicleLoad = MaxOutgoingLoadSize(buildingAI);
                        if (buildingData.m_customBuffer2 > 12000)
                        {
                            offer.Priority = Mathf.Clamp(2 + buildingData.m_outgoingProblemTimer * 6 / 128, 2, 7); // 128 is when the problem icon appears
                        }
                        else if (buildingData.m_customBuffer2 > iMaxVehicleLoad)
                        {
                            offer.Priority = Mathf.Clamp(buildingData.m_outgoingProblemTimer * 8 / 128, 0, 7); // 128 is when the problem icon appears
                        }
                        else
                        {
                            offer.Priority = 0;
                        }
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = Mathf.Min(buildingData.m_customBuffer2 / iMaxVehicleLoad, iMaxVehicles - iVehicles);
                        offer.Active = true;

                        if (offer.Amount > 0)
                        {
                            // Add our version
                            Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingReason, offer);
                        }
                    }
                }
            }
        }

        // We call this function incase it is altered by something like Rebalanced Industries
        // Cache the result as reflection is slow.
        private static int MaxOutgoingLoadSize(IndustrialBuildingAI instance)
        {
            if (s_maxLoadSize == null)
            {
                // We try to call WarehouseAI.GetMaxLoadSize as some mods such as Industry Rebalanced modify this value
                // Unfortunately it is private so we need to use reflection, so we cache the results.
                MethodInfo getMaxOutgoingLoadSize = typeof(IndustrialBuildingAI).GetMethod("MaxOutgoingLoadSize", BindingFlags.Instance | BindingFlags.NonPublic);
                if (getMaxOutgoingLoadSize != null)
                {
                    s_maxLoadSize = (int)getMaxOutgoingLoadSize.Invoke(instance, null);
                }
                else
                {
                    // Fall back on default if we fail to get the function
                    s_maxLoadSize = 8000;
                }        
            }

            return s_maxLoadSize.Value;
        }

        private static TransferManager.TransferReason GetIncomingTransferReason(ushort buildingID, BuildingInfo info)
        {
            return info.m_class.m_subService switch
            {
                ItemClass.SubService.IndustrialForestry => TransferManager.TransferReason.Logs,
                ItemClass.SubService.IndustrialFarming => TransferManager.TransferReason.Grain,
                ItemClass.SubService.IndustrialOil => TransferManager.TransferReason.Oil,
                ItemClass.SubService.IndustrialOre => TransferManager.TransferReason.Ore,
                _ => new Randomizer(buildingID).Int32(4u) switch
                {
                    0 => TransferManager.TransferReason.Lumber,
                    1 => TransferManager.TransferReason.Food,
                    2 => TransferManager.TransferReason.Petrol,
                    3 => TransferManager.TransferReason.Coal,
                    _ => TransferManager.TransferReason.None,
                },
            };
        }

        private static TransferManager.TransferReason GetSecondaryIncomingTransferReason(ushort buildingID, BuildingInfo info)
        {
            if (info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric)
            {
                switch (new Randomizer(buildingID).Int32(8u))
                {
                    case 0:
                        return TransferManager.TransferReason.PlanedTimber;
                    case 1:
                        return TransferManager.TransferReason.Paper;
                    case 2:
                        return TransferManager.TransferReason.Flours;
                    case 3:
                        return TransferManager.TransferReason.AnimalProducts;
                    case 4:
                        return TransferManager.TransferReason.Petroleum;
                    case 5:
                        return TransferManager.TransferReason.Plastics;
                    case 6:
                        return TransferManager.TransferReason.Metals;
                    case 7:
                        return TransferManager.TransferReason.Glass;
                }
            }

            return TransferManager.TransferReason.None;
        }

        private static TransferManager.TransferReason GetOutgoingTransferReason(BuildingInfo info)
        {
            return info.m_class.m_subService switch
            {
                ItemClass.SubService.IndustrialForestry => TransferManager.TransferReason.Lumber,
                ItemClass.SubService.IndustrialFarming => TransferManager.TransferReason.Food,
                ItemClass.SubService.IndustrialOil => TransferManager.TransferReason.Petrol,
                ItemClass.SubService.IndustrialOre => TransferManager.TransferReason.Coal,
                _ => TransferManager.TransferReason.Goods,
            };
        }
    }
}
