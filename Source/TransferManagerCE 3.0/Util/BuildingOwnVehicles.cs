﻿using ColossalFramework;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;
using static RenderManager;

namespace TransferManagerCE.Util
{
    internal class BuildingOwnVehicles
    {
        List<VehicleData> m_listInternal = new List<VehicleData>();
        List<VehicleData> m_listExternal = new List<VehicleData>();
        List<VehicleData> m_listReturning = new List<VehicleData>();

        public List<VehicleData> GetVehicles(ushort buildingId)
        {
            List<VehicleData> list = new List<VehicleData>();

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
                if (m_listInternal.Count > 0)
                {
                    list.Add(new VehicleDataHeading(Localization.Get("txtLocalVehicles")));

                    m_listInternal.Sort();
                    foreach (VehicleData vehicleData in m_listInternal)
                    {
                        list.Add(vehicleData);
                    }
                }
                    

                if (m_listExternal.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    list.Add(new VehicleDataHeading(Localization.Get("txtExternalVehicles")));

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

                    list.Add(new VehicleDataHeading(Localization.Get("txtReturningVehicles")));

                    // Returning
                    m_listReturning.Sort();
                    foreach (VehicleData vehicleData in m_listReturning)
                    {
                        list.Add(vehicleData);
                    }
                }
            }

            return list;
        }

        private void EnumerateBuildingVehicles(Building building)
        {
            if (building.m_flags != 0)
            {
                BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                {
                    // Construct vehicle data object of correct type
                    VehicleData vehicleData;

                    switch (BuildingTypeHelper.GetBuildingType(building))
                    {
                        case BuildingTypeHelper.BuildingType.PostOffice:
                        case BuildingTypeHelper.BuildingType.PostSortingFacility:
                            {
                                vehicleData = new VehicleDataMail(building.m_position, vehicleId);
                                break;
                            }
                        default:
                            {
                                vehicleData = new VehicleData(building.m_position, vehicleId);
                                break;
                            }
                    }

                    // Add to correct list
                    InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleId, vehicle);
                    if (target.IsEmpty || (target.Building != 0 && target.Building == vehicle.m_sourceBuilding))
                    {
                        m_listReturning.Add(vehicleData);
                    }
                    else if ((vehicle.m_flags & Vehicle.Flags.Exporting) != 0 || (vehicle.m_flags & Vehicle.Flags.Importing) != 0)
                    {
                        m_listExternal.Add(vehicleData);
                    }
                    else
                    {
                        switch (target.Type)
                        {
                            case InstanceType.Building:
                                {
                                    if (BuildingTypeHelper.IsOutsideConnection(target.Building))
                                    {
                                        m_listExternal.Add(vehicleData);
                                    }
                                    else
                                    {
                                        m_listInternal.Add(vehicleData);
                                    }
                                    break;
                                }
                            case InstanceType.NetNode:
                                {
                                    if (CitiesUtils.IsOutsideConnectionNode(target.NetNode))
                                    {
                                        m_listExternal.Add(vehicleData);
                                    }
                                    else
                                    {
                                        m_listInternal.Add(vehicleData);
                                    }
                                        
                                    break;
                                }
                            default:
                                {
                                    m_listInternal.Add(vehicleData);
                                    break;
                                }
                        }
                    }
                    return true;
                });
            }
        }
    }
}
