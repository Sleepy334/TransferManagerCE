using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataIndustrial : StatusData
    {
        public StatusDataIndustrial(TransferReason reason, ushort BuildingId, ushort responder, ushort target) :
            base(reason, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            int incomingBuffer = (int)((double)building.m_customBuffer1 * 0.001);
            return incomingBuffer.ToString();
        }

        public override string GetResponder()
        {
            if (m_responderBuilding != 0)
            {
                return CitiesUtils.GetBuildingName(m_responderBuilding);
            }

            return "None";
        }

        public override string GetTarget()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                int iCargo = (int) Math.Round(vehicle.m_transferSize * 0.001);
                return CitiesUtils.GetVehicleName(m_targetVehicle) + " (" + iCargo + ")";
            }

            return "None";
        }

        public override string GetMaterialDescription()
        {
            return m_material.ToString(); 
        }

        public static TransferReason GetIncomingTransferReason(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            switch (building.Info.m_class.m_subService)
            {
                case ItemClass.SubService.IndustrialForestry:
                    return TransferManager.TransferReason.Logs;
                case ItemClass.SubService.IndustrialFarming:
                    return TransferManager.TransferReason.Grain;
                case ItemClass.SubService.IndustrialOil:
                    return TransferManager.TransferReason.Oil;
                case ItemClass.SubService.IndustrialOre:
                    return TransferManager.TransferReason.Ore;
                default:
                    switch (new Randomizer(buildingId).Int32(4u))
                    {
                        case 0:
                            return TransferManager.TransferReason.Lumber;
                        case 1:
                            return TransferManager.TransferReason.Food;
                        case 2:
                            return TransferManager.TransferReason.Petrol;
                        case 3:
                            return TransferManager.TransferReason.Coal;
                        default:
                            return TransferManager.TransferReason.None;
                    }
            }
        }

        private TransferManager.TransferReason GetSecondaryIncomingTransferReason(ushort buildingID)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.Info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric)
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
    }
}