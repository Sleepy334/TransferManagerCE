using System;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataShelter : StatusData
    {
        public StatusDataShelter(TransferReason reason, ushort BuildingId, ushort responder, ushort target) :
            base(reason, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ShelterAI? buildingAI = building.Info?.m_buildingAI as ShelterAI;
            if (buildingAI != null)
            {
                int amount;
                int max;
                if (m_material == TransferReason.Food || m_material == TransferReason.Goods)
                {
                    buildingAI.GetFoodStatus(m_buildingId, ref building, out amount, out max);
                    return Math.Round(amount * 0.001)  + "/" + Math.Round(max * 0.001);
                }
            }
            return 0.ToString();
        }

        public override string GetTarget()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                return CitiesUtils.GetVehicleName(m_targetVehicle) + " (" + Math.Round(vehicle.m_transferSize * 0.001) + ")";
            }

            return "None";
        }
    }
}