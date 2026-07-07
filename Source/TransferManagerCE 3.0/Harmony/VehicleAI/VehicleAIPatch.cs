using ColossalFramework;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCore
{
    public class VehicleAIPatch
    {
        protected static bool ShouldReturnToSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                Building sourceBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
                if ((sourceBuilding.m_flags & Building.Flags.Active) == 0 &&
                    sourceBuilding.m_fireIntensity == 0)
                {
                    return true;
                }
            }
            return false;
        }

        // --------------------------------------------------------------------
        protected static Vector3 GetCargoVehicleOfferPosition(ushort vehicleID, ref Vehicle data)
        {
            // Return the target of the parent vehicle
            if (data.m_cargoParent != 0)
            {
                InstanceID target = VehicleTypeHelper.GetVehicleTarget(data.m_cargoParent, VehicleManager.instance.m_vehicles.m_buffer[data.m_cargoParent]);
                if (target.Building != 0)
                {
                    return (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[target.Building].m_position) * 0.5f;
                }
            }

            if (data.m_sourceBuilding != 0)
            {
                return (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
            }

            return data.GetLastFramePosition();
        }
    }
}
