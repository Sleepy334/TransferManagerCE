using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicleSick : StatusDataVehicle
    {
        public StatusDataVehicleSick(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetTooltip()
        {
            // Display name of citizen the ambulance is coming to pick up
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                string strCitizenName = "";

                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    CitizenUtils.EnumerateCitizens(new InstanceID { Vehicle = vehicleId }, vehicle.m_citizenUnits, (citizendId, citizen) =>
                    {
                        if (citizen.Sick)
                        {
                            strCitizenName = $"#{citizendId}:{CitiesUtils.GetCitizenName(citizendId)}";
                            return false; // Break loop
                        }
                        return true; // continue loop
                    });
                }

                if (strCitizenName.Length == 0) 
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                    if (building.m_flags != 0)
                    {
                        CitizenUtils.EnumerateCitizens(new InstanceID { Building = m_buildingId }, building.m_citizenUnits, (citizendId, citizen) =>
                        {
                            if (citizen.m_vehicle == vehicleId)
                            {
                                strCitizenName = $"#{citizendId}:{CitiesUtils.GetCitizenName(citizendId)}";
                                return false; // Break loop
                            }
                            return true; // continue loop
                        });
                    }
                }

                if (strCitizenName.Length > 0)
                {
                    return $"{strCitizenName}\n#{GetVehicleId()}:{GetTarget()}\n#{GetResponderId()}:{GetResponder()}";
                }
                else
                {
                    return $"#{GetVehicleId()}:{GetTarget()}\n#{GetResponderId()}:{GetResponder()}";
                }
            }

            return "";
        }
    }
}