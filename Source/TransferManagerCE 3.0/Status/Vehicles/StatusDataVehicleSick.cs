using SleepyCommon;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleSick : StatusDataVehicle
    {
        public StatusDataVehicleSick(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) : 
            base(reason, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
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
                    return $"{strCitizenName}\n#{GetVehicleId()}:{GetDescription1()}\n#{GetSourceId()}:{GetDescription2()}";
                }
                else
                {
                    return $"#{GetVehicleId()}:{GetDescription1()}\n#{GetSourceId()}:{GetDescription2()}";
                }
            }

            return "";
        }
    }
}