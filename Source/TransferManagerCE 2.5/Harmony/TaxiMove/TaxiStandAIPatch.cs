using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class TaxiStandAIPatch
    {
        // This transpiler patches TaxiStandAI.ProduceGoods to skip over the taxi offer calls so we can add our own instead.
        [HarmonyPatch(typeof(TaxiStandAI), "ProduceGoods",
            new Type[] { typeof(ushort), typeof(Building), typeof(Building.Frame), typeof(int), typeof(int), typeof(Citizen.BehaviourData), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TaxiStandAIProduceGoodsTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo methodAddTaxiOffers = AccessTools.Method(typeof(TaxiStandAIPatch), nameof(TaxiStandAIPatch.AddTaxiStandOffers));

            bool bFoundBranch = false;
            bool bPatched = false;

            // Instruction enumerator.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction.
                CodeInstruction instruction1 = instructionsEnumerator.Current;

                // We look for the following, this seems to be the only call to Pop so just look for that for now
                // IL_004D: callvirt System.Int32 ImmaterialResourceManager::AddResource(Resource resource, System.Int32 rate, UnityEngine.Vector3 position, System.Single radius)
                // IL_0052: pop
                if (!bFoundBranch && instruction1.opcode == OpCodes.Pop)
                {
                    bFoundBranch = true;

                    yield return instruction1; // Pop

                    // Insert our offers code
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = instruction1.labels }; // Copy labels from instruction1 if any
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, methodAddTaxiOffers);

                    // return after our offers code instead of continuing to the end
                    yield return new CodeInstruction(OpCodes.Ret);

                    // Clear labels from old instruction
                    instruction1.labels = new List<Label>();

                    bPatched = true;
                    continue;
                }

                // Return normal instruction
                yield return instruction1;
            }

            Debug.Log($"TaxiStandAIProduceGoodsTranspiler - Patching of TaxiStandAI.ProduceGoods {(bPatched ? "succeeded" : "failed")}.", false);
        }

        // Override StartTransfer to dispatch a taxi at the stand (if any) rather than creating one to send.
        [HarmonyPatch(typeof(TaxiStandAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(TaxiStandAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (reason == TransferReason.Taxi)
            {
                Vehicle[] VehicleBuffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                ushort taxiId = GetFrontTaxiId(__instance, data);
                if (taxiId != 0)
                {
                    ref Vehicle vehicle = ref VehicleBuffer[taxiId];
                    if (vehicle.m_flags != 0)
                    {
                        // Dispatch taxi to pick up citizen
                        vehicle.Info.m_vehicleAI.StartTransfer(taxiId, ref vehicle, reason, offer);

                        // Don't call TaxiStandAI.StartTransfer
                        return false;
                    }
                }
            }
            
            // Handle default
            return true;
        }

        public static void AddTaxiStandOffers(TaxiStandAI __instance, ushort buildingID, ref Building buildingData)
        {
            Vehicle[] VehicleBuffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Vector3 queueStartPosition = buildingData.CalculatePosition(__instance.m_queueStartPos); // Location of the start of the queue

            int iTotalTaxiCount = 0;
            ushort usFrontVehicleId = 0;
            int iFrontVehicleWaitCount = 0;
            float fMinDistanceFromStart = float.MaxValue;

            // Increment the wait counter and determine which taxi is at the front of the queue
            BuildingUtils.EnumerateGuestVehicles(buildingData, (vehicleId, vehicleData) =>
            {
                if ((TransferReason)vehicleData.m_transferType == TransferReason.Taxi)
                {
                    iTotalTaxiCount++;

                    if (TaxiAIPatch.IsAtTaxiStand(vehicleData))
                    {
                        // Update the wait counter for each taxi waiting at the stand
                        VehicleBuffer[vehicleId].m_waitCounter = (byte)Mathf.Min(vehicleData.m_waitCounter + 1, 255);

                        // Find the taxi at the front of the queue
                        float fDistance = Vector3.SqrMagnitude(vehicleData.GetLastFramePosition() - queueStartPosition);
                        if (fDistance < fMinDistanceFromStart)
                        {
                            fMinDistanceFromStart = fDistance;
                            usFrontVehicleId = vehicleId;
                            iFrontVehicleWaitCount = vehicleData.m_waitCounter;
                        }
                    }
                }
            });

            // Put in an offer for more taxis if we have room for more
            if (iTotalTaxiCount < __instance.m_maxVehicleCount)
            {
                TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = Mathf.Clamp(__instance.m_maxVehicleCount - iTotalTaxiCount + 1, 0, 7);
                offer.Building = buildingID;
                offer.Position = buildingData.m_position;
                offer.Amount = __instance.m_maxVehicleCount - iTotalTaxiCount;
                offer.Active = false;
                Singleton<TransferManager>.instance.AddIncomingOffer((TransferReason)CustomTransferReason.Reason.TaxiMove, offer);
            }

            // Also put in a taxi request if the front taxi has waited long enough
            if (usFrontVehicleId != 0 && iFrontVehicleWaitCount > SaveGameSettings.GetSettings().TaxiStandDelay && Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0)
            {
                TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = Mathf.Clamp(iFrontVehicleWaitCount / 10, 0, 7); // Increase priority the longer the taxi has been waiting
                offer.Building = buildingID; // We actually send the offer from the taxi stand so we can add taxi stand transfer restrictions
                offer.Position = buildingData.m_position;
                offer.Amount = 1;
                offer.Active = true;
                Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Taxi, offer);
            }
        }

        private static ushort GetFrontTaxiId(TaxiStandAI __instance, Building taxiStand)
        {
            Vehicle[] VehicleBuffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Vector3 queueStartPosition = taxiStand.CalculatePosition(__instance.m_queueStartPos); // Location of the start of the queue

            ushort taxiId = 0;
            float fMinDistanceFromStart = float.MaxValue;

            // Enumerate through guest vehicles to find the taxi at the front of the queue (closest to queue pos)
            BuildingUtils.EnumerateGuestVehicles(taxiStand, (vehicleId, vehicleData) =>
            {
                if ((TransferReason)vehicleData.m_transferType == TransferReason.Taxi && TaxiAIPatch.IsAtTaxiStand(vehicleData))
                {
                    // Find the taxi at the front of the queue
                    float fDistance = Vector3.SqrMagnitude(vehicleData.GetLastFramePosition() - queueStartPosition);
                    if (fDistance < fMinDistanceFromStart)
                    {
                        fMinDistanceFromStart = fDistance;
                        taxiId = vehicleId;
                    }
                }
            });

            return taxiId;
        }
    }
}
