using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TaxiAIPatch
    {
        // This transpiler patches TaxiAI.SimulationStep to skip over the transfer offer calls so we can add our own instead
        [HarmonyPatch(typeof(TaxiAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo fieldTransferSize = AccessTools.DeclaredField(typeof(Vehicle), nameof(Vehicle.m_transferSize));
            MethodInfo methodAddTaxiOffers = AccessTools.Method(typeof(TaxiAIPatch), nameof(TaxiAIPatch.AddTaxiOffers));

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();

            bool bFoundLocation = false;
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction1 = instructionsEnumerator.Current;

                if (!bFoundLocation && instruction1.opcode == OpCodes.Ldarg_2)
                {
                    if (instructionsEnumerator.MoveNext())
                    {
                        // Get next instruction.
                        CodeInstruction instruction2 = instructionsEnumerator.Current;

                        if (instruction2.opcode == OpCodes.Ldfld && instruction2.operand == fieldTransferSize)
                        {
                            bFoundLocation = true;

                            yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = instruction1.labels }; // Copy labels from instruction1 if any
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, methodAddTaxiOffers);

                            // Add ret back in
                            yield return new CodeInstruction(OpCodes.Ret);

                            // Clear labels from old instruction
                            instruction1.labels = new List<Label>(); 
                        }

                        yield return instruction1;
                        yield return instruction2;
                        continue;
                    }
                }

                // Return normal instruction
                yield return instruction1;
            }
        }

        public static void AddTaxiOffers(TaxiAI __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            bool bIsAtTaxiStand = IsAtTaxiStand(vehicleData);
            
            // Clear the block counter so the taxi's don't despawn while waiting at a taxi stand
            if (bIsAtTaxiStand)
            {
                vehicleData.m_blockCounter = 0;
            }

            // Check we have capacity left and reduce number of frames this gets handled on
            if (vehicleData.m_transferSize < __instance.m_travelCapacity &&
                (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0 && 
                ((Singleton<SimulationManager>.instance.m_currentFrameIndex >> 4) & 0xF) == (vehicleID & 0xF) &&
                Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
            {
                // Heading back to depot, occasionally add a TaxiMove offer to head to Taxi Stand instead
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = 7;
                offer.Vehicle = vehicleID;
                offer.Position = vehicleData.GetLastFramePosition();
                offer.Amount = 1;
                offer.Active = true;
                Singleton<TransferManager>.instance.AddOutgoingOffer((TransferReason)CustomTransferReason.Reason.TaxiMove, offer);
            }
        }

        public static bool IsAtTaxiStand(Vehicle vehicleData)
        {
            // WaitingCargo = Waiting at TaxiStand
            return vehicleData.m_targetBuilding != 0 && (vehicleData.m_flags & Vehicle.Flags.WaitingCargo) != 0;
        }
    }
}
