using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Data;

namespace TransferManagerCE.Util
{
    internal class BuildingOwnVehicles
    {
        List<VehicleData> m_listInternal = new List<VehicleData>();
        List<VehicleData> m_listExternal = new List<VehicleData>();
        List<VehicleData> m_listReturning = new List<VehicleData>();

        public List<VehicleData> GetVehicles(ushort buildingId, out int iVehicleCount)
        {
            List<VehicleData> list = new List<VehicleData>();
            iVehicleCount = 0;

            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    EnumerateBuildingVehicles(building);

                    // Enumerate sub buildings as well
                    int iLoopCount = 0;
                    ushort subBuildingId = building.m_subBuilding;
                    while (subBuildingId != 0)
                    {
                        Building subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuildingId];
                        if (subBuilding.m_flags != 0)
                        {
                            EnumerateBuildingVehicles(subBuilding);
                        }

                        // setup for next sub building
                        subBuildingId = subBuilding.m_subBuilding;

                        if (++iLoopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }

                // Now produce output list
                // Internal first
                m_listInternal.Sort();
                foreach (VehicleData vehicleData in m_listInternal)
                {
                    list.Add(vehicleData);
                }

                if (m_listExternal.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    // External
                    m_listExternal.Sort();
                    foreach (VehicleData vehicleData in m_listExternal)
                    {
                        list.Add(vehicleData);
                    }
                }

                if (m_listReturning.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    // Returning
                    m_listReturning.Sort();
                    foreach (VehicleData vehicleData in m_listReturning)
                    {
                        list.Add(vehicleData);
                    }
                }

                iVehicleCount = m_listInternal.Count + m_listExternal.Count + m_listReturning.Count;
            }

            return list;
        }

        private void EnumerateBuildingVehicles(Building building)
        {
            if (building.m_flags != 0)
            {
                BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                {
                    if (vehicle.m_flags != 0)
                    {
                        InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleId, vehicle);
                        if (target.IsEmpty || (target.Building != 0 && target.Building == vehicle.m_sourceBuilding))
                        {
                            m_listReturning.Add(new VehicleData(vehicleId));
                        }
                        else
                        {
                            switch (target.Type)
                            {
                                case InstanceType.Building:
                                    {
                                        if (BuildingTypeHelper.IsOutsideConnection(target.Building))
                                        {
                                            m_listExternal.Add(new VehicleData(vehicleId));
                                        }
                                        else
                                        {
                                            m_listInternal.Add(new VehicleData(vehicleId));
                                        }
                                        break;
                                    }
                                case InstanceType.NetNode:
                                    {
                                        NetNode node = NetManager.instance.m_nodes.m_buffer[target.NetNode];
                                        if ((node.m_flags & NetNode.Flags.Outside) != 0)
                                        {
                                            m_listExternal.Add(new VehicleData(vehicleId));
                                        }
                                        else
                                        {
                                            m_listInternal.Add(new VehicleData(vehicleId));
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        m_listInternal.Add(new VehicleData(vehicleId));
                                        break;
                                    }
                            }
                        }
                    }
                });
            }
        }
    }
}
