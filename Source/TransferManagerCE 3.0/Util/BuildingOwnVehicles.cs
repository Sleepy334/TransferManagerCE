using ColossalFramework;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCore.Data;
using static ParadeGroupInfo;
using static RenderManager;

namespace TransferManagerCore.Util
{
    internal class BuildingOwnVehicles
    {
        List<VehicleData> m_listInternal = new List<VehicleData>();
        List<VehicleData> m_listImporting = new List<VehicleData>();
        List<VehicleData> m_listExporting = new List<VehicleData>();
        List<VehicleData> m_listReturning = new List<VehicleData>();
        List<VehicleData> m_listTaxiStand = new List<VehicleData>();
        List<VehicleData> m_listDummy = new List<VehicleData>();

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
                    

                if (m_listImporting.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    list.Add(new VehicleDataHeading(Localization.Get("listConnectionImport")));

                    // External
                    m_listImporting.Sort();
                    foreach (VehicleData vehicleData in m_listImporting)
                    {
                        list.Add(vehicleData);
                    }
                }

                if (m_listExporting.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    list.Add(new VehicleDataHeading(Localization.Get("listConnectionExport")));

                    // External
                    m_listExporting.Sort();
                    foreach (VehicleData vehicleData in m_listExporting)
                    {
                        list.Add(vehicleData);
                    }
                }

                if (m_listTaxiStand.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    list.Add(new VehicleDataHeading(Localization.Get("txtTaxiStand")));

                    // Returning
                    m_listTaxiStand.Sort();
                    foreach (VehicleData vehicleData in m_listTaxiStand)
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

                if (m_listDummy.Count > 0)
                {
                    if (list.Count > 0)
                    {
                        list.Add(new VehicleDataSeparator());
                    }

                    list.Add(new VehicleDataHeading(Localization.Get("txtVehicleDummyTraffic")));

                    // Returning
                    m_listDummy.Sort();
                    foreach (VehicleData vehicleData in m_listDummy)
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
                    if ((vehicle.m_flags & Vehicle.Flags.DummyTraffic) != 0)
                    {
                        m_listDummy.Add(vehicleData);
                    }
                    else if (target.IsEmpty || 
                            (target.Building != 0 && target.Building == vehicle.m_sourceBuilding))
                    {
                        m_listReturning.Add(vehicleData);
                    }
                    else if ((vehicle.m_flags & Vehicle.Flags.Importing) != 0 && (vehicle.m_flags & Vehicle.Flags.Exporting) != 0)
                    {
                        AddExternalVehicle(vehicle, target, vehicleData);
                    }
                    else if ((vehicle.m_flags & Vehicle.Flags.Exporting) != 0)
                    {
                        m_listExporting.Add(vehicleData);
                    }
                    else if ((vehicle.m_flags & Vehicle.Flags.Importing) != 0)
                    {
                        m_listImporting.Add(vehicleData);
                    }
                    else if ((vehicle.m_flags & Vehicle.Flags.GoingBack) != 0)
                    {
                        m_listReturning.Add(vehicleData);
                    }
                    else if (vehicle.Info is not null && 
                                vehicle.Info.GetSubService() == ItemClass.SubService.PublicTransportTaxi &&
                                vehicle.m_targetBuilding != 0 &&
                                BuildingTypeHelper.GetBuildingType(vehicle.m_targetBuilding) == BuildingTypeHelper.BuildingType.TaxiStand)
                    {
                        m_listTaxiStand.Add(vehicleData);
                    }
                    else
                    {
                        // Just add it to internal list
                        m_listInternal.Add(vehicleData);
                    }
                    return true;
                });
            }
        }

        private void AddExternalVehicle(Vehicle vehicle, InstanceID target, VehicleData vehicleData)
        {
            if ((vehicle.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                if (BuildingTypeHelper.IsOutsideConnection(vehicle.m_sourceBuilding))
                {
                    m_listExporting.Add(vehicleData);
                }
                else
                {
                    m_listImporting.Add(vehicleData);
                }
            }

            if ((vehicle.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                if (target.Building != 0 && BuildingTypeHelper.IsOutsideConnection(target.Building))
                {
                    m_listExporting.Add(vehicleData);
                }
                else if (target.NetNode != 0 && CitiesUtils.IsOutsideConnectionNode(target.NetNode))
                {
                    m_listExporting.Add(vehicleData);
                }
                else
                {
                    m_listImporting.Add(vehicleData);
                }
            }
        }
    }
}
