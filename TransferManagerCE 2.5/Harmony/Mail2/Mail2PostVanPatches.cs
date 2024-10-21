using HarmonyLib;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class Mail2PostVanPatches
    {
        [HarmonyPatch(typeof(PostVanAI), "SetTarget")]
        [HarmonyPrefix]
        public static void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            // Post trucks are level5
            // TransferToServicePoint is set when it is going to a service point
            // It's transfer type will be Mail
            // And it's target will be 0 once it has reached the service point
            if (targetBuilding == 0 && 
                ModSettings.GetSettings().FixPostTruckCollectingMail && 
                (TransferReason)data.m_transferType == TransferReason.Mail &&
                data.Info.GetClassLevel() == ItemClass.Level.Level5 &&
                (data.m_flags2 & Vehicle.Flags2.TransferToServicePoint) == Vehicle.Flags2.TransferToServicePoint)
            {
                // We don't want the Post Truck to start collecting Mail from other places in the city, so change type to UnsortedMail
                data.m_transferType = (byte)TransferReason.UnsortedMail;
            }
        }
    }
}
