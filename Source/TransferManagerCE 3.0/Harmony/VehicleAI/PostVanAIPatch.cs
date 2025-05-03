using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class PostVanAIPatch : VehicleAIPatch
    {
        [HarmonyPatch(typeof(PostVanAI), "SimulationStep",
            new Type[] { typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void PostVanAIPostfix(PostVanAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (ModSettings.GetSettings().PostVanAI && (CustomTransferReason.Reason)vehicleData.m_transferType == CustomTransferReason.Reason.Mail)
            {
                Randomizer random = Singleton<SimulationManager>.instance.m_randomizer;

                // Check if target has had their mail buffer cleared already.
                if (vehicleData.m_targetBuilding != 0 &&
                    (vehicleData.m_flags & (Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget)) == 0 &&
                    (vehicleData.m_flags2 & Vehicle.Flags2.TransferToServicePoint) == 0 &&
                    random.Int32(10U) == 0)
                {
                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                    if (building.m_mailBuffer == 0)
                    {
                        //Debug.Log($"Clearing Target - Vehicle: #{vehicleID} Building: #{vehicleData.m_targetBuilding} BuildingType: {building.Info.GetAI().GetType()} MailBuffer: {building.m_mailBuffer}");

                        //need to change target because problem already solved
                        vehicleData.Info.m_vehicleAI.SetTarget(vehicleID, ref vehicleData, 0); //clear target
                    }
                }
            }
        }
    }
}
