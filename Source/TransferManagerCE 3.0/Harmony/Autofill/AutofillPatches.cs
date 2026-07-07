using System;
using ColossalFramework;
using HarmonyLib;
using SleepyCommon;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCore
{
    // --------------------------------------------------------------------
    [HarmonyPatch]
    public class AutofillPatches
    {
        // --------------------------------------------------------------------
        [HarmonyPatch(typeof(CargoShipAI), "StartPathFind",
        new[] { typeof(ushort), typeof(Vehicle) },
        new[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        public static void CargoShipAIStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            Autofill(vehicleID, ref vehicleData);
        }

        // --------------------------------------------------------------------
        [HarmonyPatch(typeof(CargoTrainAI), "StartPathFind",
            new[] { typeof(ushort), typeof(Vehicle) },
            new[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        public static void CargoTrainAIStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            Autofill(vehicleID, ref vehicleData);
        }

        // --------------------------------------------------------------------
        [HarmonyPatch(typeof(CargoPlaneAI), "StartPathFind",
            new[] { typeof(ushort), typeof(Vehicle) },
            new[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        public static void CargoPlaneAIStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            Autofill(vehicleID, ref vehicleData);
        }

        // --------------------------------------------------------------------
        private static void Autofill(ushort vehicleID, ref Vehicle vehicleData)
        {
            // Check its enabled
            if (!SaveGameSettings.GetSettings().Autofill)
            {
                return;
            }

            // Auto fill cargo ship, plane train with random cargo
            if ((vehicleData.m_flags & Vehicle.Flags.Importing) != 0 &&
                (vehicleData.m_flags & Vehicle.Flags.Exporting) == 0 &&
                (vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 &&
                vehicleData.m_sourceBuilding != 0 &&
                vehicleData.m_targetBuilding != 0 &&
                BuildingTypeHelper.IsOutsideConnection(vehicleData.m_sourceBuilding) &&
                CitiesUtils.IsNearEdgeOfMap(vehicleData.GetLastFramePosition()))
            {
                // Check target is not OC or CargoWarehouse
                BuildingTypeHelper.BuildingType buildingType = BuildingTypeHelper.GetBuildingType(vehicleData.m_targetBuilding);
                if (buildingType != BuildingTypeHelper.BuildingType.OutsideConnection &&
                    buildingType != BuildingTypeHelper.BuildingType.CargoWarehouse)
                {
                    Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                    // Is it close to outside connection
                    Vector3 buildingPos = buildings[vehicleData.m_sourceBuilding].m_position;
                    if (Vector3.SqrMagnitude(vehicleData.GetLastFramePosition() - buildingPos) < 40000f)
                    {
                        CitiesUtils.GetVehicleTransferValue(vehicleID, out int current, out int max);
                        float randomFillLevel = UnityEngine.Random.Range(0.6f, 1.0f);

                        int iAddCount = (int)((float)max * randomFillLevel - (float)current);
                        if (iAddCount > 0)
                        {
                            //string sText = $"Building: {vehicleData.m_sourceBuilding} | Vehicle: {vehicleID} Source: {vehicleData.m_sourceBuilding} Target: {vehicleData.m_targetBuilding} Flags: {vehicleData.m_flags} Current: {current} Max: {max} AddCount: {iAddCount} Distance: {Vector3.SqrMagnitude(vehicleData.GetLastFramePosition() - buildingPos)}";
                            for (int i = 0; i < iAddCount; ++i)
                            {
                                VehicleInfo vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialGeneric, ItemClass.Level.Level1);
                                ushort childVehicleId = CreateVehicle(vehicleID, GetMaterial(), vehicleInfo, vehicleData.m_sourceBuilding, vehicleData.m_targetBuilding);

                                Vehicle childVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[childVehicleId];
                                //sText += $"\r\nNew Vehicle: {childVehicleId} Flags: {childVehicle.m_flags} Source: {childVehicle.m_sourceBuilding} Target: {childVehicle.m_targetBuilding} Parent: {childVehicle.m_cargoParent}";
                            }

                            //CDebug.Log(sText);
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------
        private static ushort CreateVehicle(ushort parentVehicleId, TransferReason material, VehicleInfo vehicleInfo, ushort sourceBuildingId, ushort targetBuildingId)
        {
            Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicleId, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, Buildings[sourceBuildingId].m_position, material, false, true))
            {
                ref Vehicle vehicle = ref Vehicles[vehicleId];

                // Set appropriate flags
                vehicle.m_waitCounter = 0;
                vehicle.m_flags &= ~Vehicle.Flags.WaitingLoading;
                vehicle.m_flags |= Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.TransferToTarget;
                vehicle.m_transferSize = (ushort) ((CargoTruckAI) vehicleInfo.GetAI()).m_cargoCapacity; // Max load

                // Attach to cargo parent (Refer CargoTruckAI.ChangeVehicleType)
                ref Vehicle parentVehicle = ref Vehicles[parentVehicleId];
                vehicle.m_cargoParent = parentVehicleId;
                vehicle.m_nextCargo = parentVehicle.m_firstCargo;
                parentVehicle.m_firstCargo = vehicleId;

                //CDebug.Log($"Vehicle attached: {vehicleId} Flags: {vehicle.m_flags}  Parent: {parentVehicleId} Source: {parentVehicle.m_sourceBuilding} Target: {parentVehicle.m_targetBuilding}");

                return vehicleId;
            }

            return 0;
        }

        // --------------------------------------------------------------------
        private static TransferManager.TransferReason GetMaterial()
        {
            int iRandom = UnityEngine.Random.Range(0, 20);
            switch (iRandom)
            {
                case 0: return TransferManager.TransferReason.Ore;
                case 1: return TransferManager.TransferReason.Oil;
                case 2: return TransferManager.TransferReason.Grain;
                case 3: return TransferManager.TransferReason.Logs;
                case 4: return TransferManager.TransferReason.Coal;
                case 5: return TransferManager.TransferReason.Petrol;
                case 6: return TransferManager.TransferReason.Food;
                case 7: return TransferManager.TransferReason.Lumber;
                case 8: return TransferManager.TransferReason.SortedMail;
                case 9: return TransferManager.TransferReason.IncomingMail;
                default:
                    {
                        return TransferManager.TransferReason.Goods;
                    }
            }
        }
    }
}