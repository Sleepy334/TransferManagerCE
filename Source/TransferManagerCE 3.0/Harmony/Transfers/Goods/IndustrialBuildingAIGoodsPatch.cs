using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TransferManagerCE.TransferOffers;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class IndustrialBuildingAIGoodsPatch
    {
        private static int? s_maxLoadSize = null;
        private static bool s_bPatched = false;

        public static void PatchGenericIndustriesHandler()
        {
            if (SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideGenericIndustriesHandler)
            {
                if (!s_bPatched)
                {
#if DEBUG
                    CDebug.Log("Patching generic industries handler...", false);
#endif
                    Patcher.Patch(typeof(IndustrialBuildingAIGoodsPatch));
                }
            }
            else if (s_bPatched)
            {
#if DEBUG
                CDebug.Log("Unpatching generic industries handler...", false);
#endif
                Patcher.Unpatch(typeof(IndustrialBuildingAI), "SimulationStepActive");
                s_bPatched = false;
            }
        }

        // SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        [HarmonyPatch(typeof(IndustrialBuildingAI), "SimulationStepActive")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SimulationStepActiveTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            // Have we already patched the function, if so just return unaltered.
            if (s_bPatched)
            {
                CDebug.Log($"ERROR: IndustrialBuildingAI.SimulationStepActive - Already patched!", false);
                return instructions.AsEnumerable();
            }

            s_bPatched = true;

            FieldInfo fieldFireIntensity = typeof(Building).GetField("m_fireIntensity", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo methodAddIncomingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddIncomingOffer));
            MethodInfo methodAddOutgoingOffer = AccessTools.Method(typeof(TransferManager), nameof(TransferManager.AddOutgoingOffer));
            MethodInfo methodAddIncomingOfferPatch = AccessTools.Method(typeof(IndustrialBuildingAIGoodsPatch), nameof(AddIncomingOffer));
            MethodInfo methodAddOutgoingOfferPatch = AccessTools.Method(typeof(IndustrialBuildingAIGoodsPatch), nameof(AddOutgoingOffer));

            List<CodeInstruction> newInstructionList = new List<CodeInstruction>();

            // if (buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
            ILSearch searchIncoming = new ILSearch();
            searchIncoming.AddPattern(new CodeInstruction(OpCodes.Ldarg_2));
            searchIncoming.AddPattern(new CodeInstruction(OpCodes.Ldfld, fieldFireIntensity));
            searchIncoming.AddPattern(new CodeInstruction(OpCodes.Brtrue));
            searchIncoming.AddPattern(new CodeInstruction(OpCodes.Ldloc_S));
            searchIncoming.AddPattern(new CodeInstruction(OpCodes.Ldc_I4, 255));
            searchIncoming.AddPattern(new CodeInstruction(OpCodes.Beq));

            // 2nd call to Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
            ILSearch searchIncomingAddOffer = new ILSearch();
            searchIncomingAddOffer.AddPattern(new CodeInstruction(OpCodes.Callvirt, methodAddIncomingOffer));
            searchIncomingAddOffer.Occurrance = 2;

            // Search for the following:
            // (data.m_flags & Vehicle.Flags.TransferToTarget) != 0
            // It appears twice in the function, we want the second occurrance.
            // IL_0C7B: ldarg.2
            // IL_0C7C: ldfld      System.Byte Building::m_fireIntensity
            // IL_0C81: brtrue =>  Label76
            // IL_0C86: ldloc.s    12 (TransferManager+TransferReason)
            // IL_0C88: ldc.i4     255
            // IL_0C8D: beq =>     Label77
            ILSearch searchOutgoing = new ILSearch();
            searchOutgoing.AddPattern(new CodeInstruction(OpCodes.Ldarg_2));
            searchOutgoing.AddPattern(new CodeInstruction(OpCodes.Ldfld, fieldFireIntensity));
            searchOutgoing.AddPattern(new CodeInstruction(OpCodes.Brtrue));
            searchOutgoing.AddPattern(new CodeInstruction(OpCodes.Ldloc_S));
            searchOutgoing.AddPattern(new CodeInstruction(OpCodes.Ldc_I4, 255));
            searchOutgoing.AddPattern(new CodeInstruction(OpCodes.Beq));
            searchOutgoing.Occurrance = 2;

            bool bIncomingPatched = false;
            bool bOutgoingPatched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                bool bAddInstruction = true;

                if (!bIncomingPatched)
                {
                    if (searchIncoming.IsFound())
                    {
                        bAddInstruction = false;

                        searchIncomingAddOffer.NextInstruction(instruction);

                        // Now start looking for second call to AddIncomingOffer
                        if (searchIncomingAddOffer.IsFound())
                        {
                            // Insert our function here instead
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_1)); 
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_2));
                            newInstructionList.Add(new CodeInstruction(OpCodes.Call, methodAddIncomingOfferPatch) { labels = instruction.labels }); // Copy labels from the call function

                            // Start adding instructions again
                            bIncomingPatched = true;
                        }
                    }

                    searchIncoming.NextInstruction(instruction);
                }

                if (!bOutgoingPatched)
                {
                    if (searchOutgoing.IsFound())
                    {
                        bAddInstruction = false;

                        // Look for CallVirt
                        if (instruction.Calls(methodAddOutgoingOffer))
                        {
                            // Insert our function here instead
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_1)); 
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_2));
                            newInstructionList.Add(new CodeInstruction(OpCodes.Call, methodAddOutgoingOfferPatch) { labels = instruction.labels }); // Copy labels from the call function

                            // Start adding instructions again
                            bOutgoingPatched = true;
                        }
                    }

                    searchOutgoing.NextInstruction(instruction);
                }

                if (bAddInstruction)
                {
                    newInstructionList.Add(instruction);
                }
            }

            CDebug.Log($"IndustrialBuildingAIGoodsPatch - Patching of IndustrialBuildingAI.SimulationStepActive {((bOutgoingPatched && bIncomingPatched) ? "succeeded" : "failed")}.", false);
            return newInstructionList.AsEnumerable();
        }

        public static void AddIncomingOffer(ushort buildingID, ref Building buildingData)
        {
            Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;
            if (random.Int32(2U) == 0)
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
                            // We clamp priority to 6 until timer starts so we can target the buildings with notification icons first
                            int iPriority;
                            if (buildingData.m_incomingProblemTimer > 0)
                            {
                                iPriority = 7;
                            }
                            else
                            {
                                // Calculate a more realistic priority
                                iPriority = Mathf.Clamp((iStorageCapacity - buildingData.m_customBuffer1 - iTransferSize) * 7 / iStorageCapacity, 0, 6);
                            }

                            TransferOffer offer = default;
                            offer.Priority = iPriority;
                            offer.Building = buildingID;
                            offer.Position = buildingData.m_position;
                            offer.Amount = 1;
                            offer.Active = false;

                            // Add new offer
                            // Alternate primary/secondary offer
                            if (iPriority >= 3)
                            {
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
        }

        public static void AddOutgoingOffer(ushort buildingID, ref Building buildingData)
        {
            // We override the default outgoing offer so we can factor in the problem timer value into priority
            // to ensure buildings with the flashing icon get processed first
            // We don't request every time so it gives time for a vehicle to be matched and
            // dispatched before we request again.
            TransferReason outgoingReason = GetOutgoingTransferReason(buildingData);
            if (outgoingReason != TransferReason.None && Singleton<SimulationManager>.instance.m_randomizer.UInt32(2U) == 0)
            {
                IndustrialBuildingAI? buildingAI = buildingData.Info.GetAI() as IndustrialBuildingAI;
                if (buildingAI is not null)
                {
                    // Check we have vehicles available before adding offer
                    int iVehicles = BuildingUtils.GetOwnVehicleCount(buildingData, outgoingReason);
                    int iMaxVehicles = BuildingVehicleCount.GetMaxVehicleCount(BuildingTypeHelper.BuildingType.GenericFactory, buildingID, 0);
                    if (iVehicles < iMaxVehicles)
                    {
                        // Determine priority based on current storage level
                        int iProductionCapacity = buildingAI.CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), buildingData.Width, buildingData.Length);
                        int iStorageCapacity = Mathf.Max(iProductionCapacity * 500, 8000 * 2);
                        int iMaxVehicleLoad = MaxOutgoingLoadSize(buildingAI);

                        // We clamp priority to 5 until timer starts so we can target the buildings with notification icons first
                        int iPriority = -1;
                        if (buildingData.m_outgoingProblemTimer > 128) // 128 is when the notification icon first appears
                        {
                            iPriority = 7;
                        }
                        else if (buildingData.m_outgoingProblemTimer > 0)
                        {
                            iPriority = 6;
                        }
                        else if (buildingData.m_customBuffer2 > iMaxVehicleLoad)
                        {
                            // Calculate a more realistic priority scaled between 0 and 5.
                            iPriority = Mathf.Clamp(buildingData.m_customBuffer2 * 6 / iStorageCapacity, 0, 5);
                        }

                        if (iPriority >= 0)
                        {
                            TransferManager.TransferOffer offer = default;
                            offer.Priority = iPriority;
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

        private static TransferManager.TransferReason GetOutgoingTransferReason(Building building)
        {
            return building.Info.m_class.m_subService switch
            {
                ItemClass.SubService.IndustrialForestry => TransferManager.TransferReason.Lumber,
                ItemClass.SubService.IndustrialFarming => TransferManager.TransferReason.Food,
                ItemClass.SubService.IndustrialOil => TransferManager.TransferReason.Petrol,
                ItemClass.SubService.IndustrialOre => TransferManager.TransferReason.Coal,
                _ => TransferManager.TransferReason.Goods,
            };
        }

        // We call this function incase it is altered by something like Rebalanced Industries
        // Cache the result as reflection is slow.
        private static int MaxOutgoingLoadSize(IndustrialBuildingAI instance)
        {
            if (s_maxLoadSize == null)
            {
                // We try to call IndustrialBuildingAI.GetMaxLoadSize as some mods such as Industry Rebalanced modify this value
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
    }
}