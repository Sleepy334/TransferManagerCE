using ColossalFramework;
using HarmonyLib;
using System;
using static TransferManager;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class WarehouseAIRemoveGuestVehicles
    {
        private static bool s_bInProduceGoods = false;

        // We don't want to process the RemoveGuestVehicles while in WarehouseAI.ProduceGoods as it stops "Empty" mode warehouses from ever receiving the transfer material.
        [HarmonyPatch(typeof(WarehouseAI), "ProduceGoods")]
        [HarmonyPrefix]
        public static void ProduceGoodsPrefix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            s_bInProduceGoods = true;
        }

        [HarmonyPatch(typeof(WarehouseAI), "ProduceGoods")]
        [HarmonyPostfix]
        public static void ProduceGoodsPostfix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            s_bInProduceGoods = false;
        }


        // There is a bug in WarehouseAI.RemoveGuestVehicles where it was applying a bit mask for the m_transferType field instead of just an equality comparison.
        // This resulted in lots of service vehicles being removed as well, especially in the ProduceGoods call which I have now stopped as well.
        [HarmonyPatch(typeof(WarehouseAI), "RemoveGuestVehicles")]
        [HarmonyPrefix]
        public static bool RemoveGuestVehiclesPrefix(ushort buildingID, ref Building data, TransferReason material)
        {
            // We don't want to process the RemoveGuestVehicles while in WarehouseAI.ProduceGoods
            if (!s_bInProduceGoods)
            {
                Vehicle[] buffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                ushort num = data.m_guestVehicles;
                int num2 = 0;
                while (num != 0)
                {
                    ushort nextGuestVehicle = buffer[num].m_nextGuestVehicle;

                    // In WarehouseAI it treats m_transferType as a bit mask, which it isnt so it removes police cars and hearses and all sorts of stuff as well as the trucks
                    if (buffer[num].m_targetBuilding == buildingID && ((TransferReason)buffer[num].m_transferType == material)) 
                    {
                        VehicleInfo info = buffer[num].Info;
                        if (info != null)
                        {
                            info.m_vehicleAI.SetTarget(num, ref buffer[num], 0);
                        }
                    }

                    num = nextGuestVehicle;
                    if (++num2 > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }

            return false; // Don't call actual function as it is sooo buggy!
        }
    }
}
