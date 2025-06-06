using ColossalFramework;
using HarmonyLib;
using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class PostVanAIUnsortedMailPatch
    {
        // ----------------------------------------------------------------------------------------
        // Vanilla bug fix: We patch PostVanAI.SetTarget so that we don't add a high priority UnsortedMail offer
        // when a post truck is from a post sorting facility as we want it to return the UnsortedMail back to the facility.
        // We are trying to replace the following with a call to our function below:

        // else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
        // {
        //    if (data.m_transferSize > 0)
        //    {
        //        TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
        //        offer2.Priority = 7;
        //       offer2.Vehicle = vehicleID;
        //        if (data.m_sourceBuilding != 0)
        //        {
        //            offer2.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
        //        }
        //        else
        //        {
        //            offer2.Position = data.GetLastFramePosition();
        //        }
        //        offer2.Amount = 1;
        //        offer2.Active = true;
        //        Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer2);
        //        data.m_flags |= Vehicle.Flags.WaitingTarget;
        //    }
        //    else
        //    {
        //        data.m_flags |= Vehicle.Flags.GoingBack;
        //    }
        // }

        [HarmonyPatch(typeof(PostVanAI), "SetTarget")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetTargetTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo fieldVehicleFlags = typeof(Vehicle).GetField("m_flags", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo methodAddMailOffer = AccessTools.Method(typeof(PostVanAIUnsortedMailPatch), nameof(PostVanAIUnsortedMailPatch.AddMailOffer));

            // Search for the following:
            // (data.m_flags & Vehicle.Flags.TransferToTarget) != 0
            // It appears twice in the function, we want the second occurrance.
            ILSearch search = new ILSearch();
            search.AddPattern(new CodeInstruction(OpCodes.Ldfld, fieldVehicleFlags));
            search.AddPattern(new CodeInstruction(OpCodes.Ldc_I4_S, (System.SByte)16));
            search.AddPattern(new CodeInstruction(OpCodes.And));
            search.AddPattern(new CodeInstruction(OpCodes.Brfalse));
            search.Occurrance = 2; // Find the second occurrance of this instruction pattern

            List<CodeInstruction> newInstructionList = new List<CodeInstruction>();

            bool bPatched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                bool bAddInstruction = true;

                if (!bPatched)
                {
                    if (search.IsFound())
                    {
                        // We are now at the instruction after OpCodes.Brfalse.
                        bAddInstruction = false;

                        // Look for CallVirt
                        if (instruction.opcode == OpCodes.Callvirt)
                        {
                            // Insert our function here instead
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_1));
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_2));
                            newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_3));
                            newInstructionList.Add(new CodeInstruction(OpCodes.Call, methodAddMailOffer));

                            // Start adding instructions again
                            bPatched = true;
                        }
                    }

                    search.NextInstruction(instruction);
                }

                if (bAddInstruction)
                {
                    newInstructionList.Add(instruction);
                }
            }

            /*
            foreach (CodeInstruction instruction in newInstructionList)
            {
                CDebug.Log(instruction.ToString());
            }
            */

            CDebug.Log($"PostVanAIUnsortedMailPatch - Patching of PostVanAI.SetTarget {(bPatched ? "succeeded" : "failed")}.", false);
            return newInstructionList.AsEnumerable();
        }

        // ----------------------------------------------------------------------------------------
        private static void AddMailOffer(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            // If post sorting facility, dont put out offer
            if (data.m_transferSize > 0)
            {
                // If UnsortedMail and the source building is a post sorting facility, then just take the unsorted mail back
                // as that is what the post sorting faciity wants
                if ((CustomTransferReason)data.m_transferType == CustomTransferReason.Reason.UnsortedMail &&
                    BuildingTypeHelper.GetBuildingType(data.m_sourceBuilding) == BuildingTypeHelper.BuildingType.PostSortingFacility)
                {
                    data.m_flags |= Vehicle.Flags.GoingBack;
                }
                else
                {
                    // Otherwise request new target
                    TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
                    offer2.Priority = 7;
                    offer2.Vehicle = vehicleID;
                    if (data.m_sourceBuilding != 0)
                    {
                        offer2.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                    }
                    else
                    {
                        offer2.Position = data.GetLastFramePosition();
                    }
                    offer2.Amount = 1;
                    offer2.Active = true;
                    Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer2);
                    data.m_flags |= Vehicle.Flags.WaitingTarget;
                } 
            }
            else
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
            }
        }
    }
}
