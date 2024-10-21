using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using TransferManagerCE.CustomManager;
using static TransferManager;
using static TransferManagerCE.VehicleTypeHelper;

namespace TransferManagerCE
{
    public static class CitiesUtils
    {
        public static string GetBuildingName(ushort buildingId, bool bShowId = false)
        {
            string sName = "";
            if (buildingId != 0 && buildingId < BuildingManager.instance.m_buildings.m_size)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != Building.Flags.None)
                {
                    if (building.m_parentBuilding != 0)
                    {
                        sName += "(S) "; // Add an S for sub building.
                    }
                    sName += Singleton<BuildingManager>.instance.GetBuildingName(buildingId, InstanceID.Empty);
                    if (string.IsNullOrEmpty(sName))
                    {
                        sName = "Building:" + buildingId;
                    }

                    if (bShowId)
                    {
                        sName = "(" + buildingId + ") " + sName;
                    }
                    else
                    {
#if DEBUG
                        sName = "(" + buildingId + ") " + sName;
#endif
                    }

                    return sName;
                }
                else
                {
                    sName = "Building:" + buildingId;
                }
            }

            return sName;
        }

        public static string GetVehicleName(ushort vehicleId)
        {
            if (vehicleId > 0 && vehicleId < VehicleManager.instance.m_vehicles.m_size)
            {
                string sMessage = Singleton<VehicleManager>.instance.GetVehicleName(vehicleId);
                if (string.IsNullOrEmpty(sMessage))
                {
                    sMessage = "Vehicle:" + vehicleId;
                }
#if DEBUG
                return "(" + vehicleId + ") " + sMessage;
#else
                return sMessage;
#endif
            }
            return "";
        }

        public static string GetCitizenName(uint citizenId)
        {
            if (citizenId != 0)
            {
                string sMessage = Singleton<CitizenManager>.instance.GetCitizenName(citizenId);
                if (string.IsNullOrEmpty(sMessage))
                {
                    sMessage = "Citizen:" + citizenId;
                }
#if DEBUG
                return "(" + citizenId + ") " + sMessage;
#else
                return sMessage;
#endif
            }
            return "";
        }

        public static Vector3 GetCitizenInstancePosition(ushort CitizenInstanceId)
        {
            if (CitizenInstanceId > 0 && CitizenInstanceId < CitizenManager.instance.m_instances.m_size)
            {
                ref CitizenInstance cimInstance = ref CitizenManager.instance.m_instances.m_buffer[CitizenInstanceId];
                return cimInstance.GetLastFramePosition();
            }
            return Vector3.zero;
        }

        public static void ShowPosition(Vector3 position)
        {
            ToolsModifierControl.cameraController.m_targetPosition = position;
        }

        public static string GetDistrictName(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            byte district = DistrictManager.instance.GetDistrict(building.m_position);
            if (district != 0)
            {
                return DistrictManager.instance.GetDistrictName(district);
            }

            return "";
        }

        public static string GetParkName(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            byte park = DistrictManager.instance.GetPark(building.m_position);
            if (park != 0)
            {
                return DistrictManager.instance.GetParkName(park);
            }

            return "";
        }

        public static string GetDetectedDistricts(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            string sMessage = "";

            byte district = DistrictManager.instance.GetDistrict(building.m_position);
            if (district != 0)
            {
                sMessage += DistrictManager.instance.GetDistrictName(district);
            }

            byte park = DistrictManager.instance.GetPark(building.m_position);
            if (park != 0)
            {
                if (sMessage.Length > 0)
                {
                    sMessage += ", ";
                }
                sMessage += DistrictManager.instance.GetParkName(park);
            }

            return sMessage;
        }

        public static bool IsInDistrict(ushort buildingId)
        {
            byte district = 0;
            if (buildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                district = DistrictManager.instance.GetDistrict(inBuilding.m_position);
            }

            return district != 0;
        }

        public static bool IsInPark(ushort buildingId)
        {
            byte park = 0;
            if (buildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                park = DistrictManager.instance.GetPark(inBuilding.m_position);
            }

            return park != 0;
        }

        public static bool IsSameDistrict(ushort firstBuildingId, ushort secondBuildingId)
        {
            // get respective districts
            byte districtIncoming = 0;
            if (firstBuildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[firstBuildingId];
                districtIncoming = DistrictManager.instance.GetDistrict(inBuilding.m_position);
            }

            byte districtOutgoing = 0;
            if (secondBuildingId != 0)
            {
                Building outBuilding = BuildingManager.instance.m_buildings.m_buffer[secondBuildingId];
                districtOutgoing = DistrictManager.instance.GetDistrict(outBuilding.m_position);
            }

            return districtIncoming == districtOutgoing && districtIncoming != 0;
        }

        public static bool IsSamePark(ushort firstBuildingId, ushort secondBuildingId)
        {
            // get respective districts
            byte parkFirst = 0;
            if (firstBuildingId != 0)
            {
                Building inBuilding = BuildingManager.instance.m_buildings.m_buffer[firstBuildingId];
                parkFirst = DistrictManager.instance.GetPark(inBuilding.m_position);
            }

            byte parkSecond = 0;
            if (secondBuildingId != 0)
            {
                Building outBuilding = BuildingManager.instance.m_buildings.m_buffer[secondBuildingId];
                parkSecond = DistrictManager.instance.GetPark(outBuilding.m_position);
            }

            return parkFirst == parkSecond && parkFirst != 0;
        }

        public static string GetVehicleTransferValue(ushort vehicleId)
        {
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    // Show values for cargo parent if any
                    if (vehicle.m_cargoParent != 0)
                    {
                        vehicleId = vehicle.m_cargoParent;
                        vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                    }

                    if (vehicle.m_flags != 0)
                    {
                        VehicleType eType = VehicleTypeHelper.GetVehicleType(vehicle);
                        switch (eType)
                        {
                            case VehicleType.PoliceCar:
                            case VehicleType.PoliceCopter:
                            case VehicleType.BankVan:
                            case VehicleType.GarbageTruck:
                            case VehicleType.PostVan:
                            case VehicleType.CargoTruck:
                            case VehicleType.CargoTrain:
                            case VehicleType.CargoShip:
                            case VehicleType.CargoPlane:
                                {
                                    int iCapacity;
                                    int iCount = VehicleTypeHelper.GetBufferStatus(vehicleId, out iCapacity);
                                    if (iCapacity > 0)
                                    {
                                        return $"{Math.Round(((float)iCount / (float)iCapacity * 100.0), 0)}%";
                                    }
                                    else
                                    {
                                        return "0%";
                                    }
                                }
                            case VehicleType.CruiseShip:
                            case VehicleType.PassengerPlane:
                            case VehicleType.PassengerTrain:
                            case VehicleType.MetroTrain:
                            case VehicleType.Bus:
                            case VehicleType.Monorail:
                            case VehicleType.Ferry:
                            case VehicleType.Tram:
                                {
                                    int iCapacity;
                                    int iCount = VehicleTypeHelper.GetVehiclePassengerCount(vehicleId, out iCapacity);
                                    return iCount + "/" + iCapacity;
                                }
                            default:
                                {
                                    return vehicle.m_transferSize.ToString();
                                }
                        }
                    }
                }
            }

            return "";
        }

        public static string GetSafeLineName(int iLineId)
        {
            TransportLine oLine = TransportManager.instance.m_lines.m_buffer[iLineId];
            if ((oLine.m_flags & TransportLine.Flags.CustomName) == TransportLine.Flags.CustomName)
            {
                InstanceID oInstanceId = new InstanceID { TransportLine = (ushort)iLineId };
                return InstanceManager.instance.GetName(oInstanceId);
            }
            else
            {
                return oLine.Info.m_transportType + " Line " + oLine.m_lineNumber;
            }
        }


        public static int GetHomeCount(Building buildingData)
        {
            int homeCount = 0;

            CitizenUtils.EnumerateCitizenUnits(buildingData.m_citizenUnits, (unitId, unit) =>
            {
                if ((unit.m_flags & CitizenUnit.Flags.Home) != 0)
                {
                    homeCount++;
                }
            });

            return homeCount;
        }

        public static int GetWorkerCount(ushort buildingId, Building building)
        {
            int iWorkerCount = 0;

            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case IndustrialBuildingAI buildingAI:
                        {
                            buildingAI.CalculateWorkplaceCount((ItemClass.Level)building.m_level, new Randomizer(buildingId), building.Width, building.Length, out var level, out var level2, out var level3, out var level4);
                            buildingAI.AdjustWorkplaceCount(buildingId, ref building, ref level, ref level2, ref level3, ref level4);
                            iWorkerCount = level + level2 + level3 + level4;
                            break;
                        }
                    case CommercialBuildingAI buildingAI:
                        {
                            buildingAI.CalculateWorkplaceCount((ItemClass.Level)building.m_level, new Randomizer(buildingId), building.Width, building.Length, out var level, out var level2, out var level3, out var level4);
                            buildingAI.AdjustWorkplaceCount(buildingId, ref building, ref level, ref level2, ref level3, ref level4);
                            iWorkerCount = level + level2 + level3 + level4;
                            break;
                        }
                }
            }

            return iWorkerCount;
        }

        public static void CheckRoadAccess(CustomTransferReason.Reason material, TransferOffer offer)
        {
            // Update access segment if using path distance but do it in simulation thread so we don't break anything
            if (offer.Building != 0 && PathDistanceTypes.GetDistanceAlgorithm(material) != PathDistanceTypes.PathDistanceAlgorithm.LineOfSight)
            {
                ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[offer.Building];
                if (building.m_accessSegment == 0 && 
                    (building.m_problems & new Notification.ProblemStruct(Notification.Problem1.RoadNotConnected, Notification.Problem2.NotInPedestrianZone)).IsNone && 
                    building.Info.GetAI() is not OutsideConnectionAI)
                {
                    // See if we can update m_accessSegment.
                    building.Info.m_buildingAI.CheckRoadAccess(offer.Building, ref building);
                    if (building.m_accessSegment == 0)
                    {
                        RoadAccessData.AddInstance(new InstanceID { Building = offer.Building });
                    }
                }
            }
        }

        public static bool IsNearEdgeOfMap(Vector3 position)
        {
            return Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.z)) > 4800f;
        }

        public static bool IsNearOutsideConnection(Vector3 position, ItemClass.SubService subService)
        {
            if (IsNearEdgeOfMap(position))
            {
                Building[] Buildings = BuildingManager.instance.m_buildings.m_buffer;

                // If the vehicle is near an outside connection then let it spawn so it doesnt block up the outside connection
                foreach (ushort outsideConnectionId in BuildingManager.instance.GetOutsideConnections())
                {
                    Building building = Buildings[outsideConnectionId];
                    if (building.m_flags != 0 &&
                        building.Info is not null &&
                        building.Info.GetSubService() == subService &&
                        Vector3.SqrMagnitude(building.m_position - position) < 2500f)
                    {
#if DEBUG
                        Debug.Log($"IsNearOutsideConnection: {position} Near outside connection: {outsideConnectionId}");
#endif
                        return true;
                    }
                }
            }

            return false;
        }

        public static int CalculatePassengerCount(ushort stop, TransportInfo.TransportType transportType)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenInstance[] CitizenInstances = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

            if (stop == 0)
            {
                return 0;
            }

            ushort nextStop = TransportLine.GetNextStop(stop);
            if (nextStop == 0)
            {
                return 0;
            }

            Vector3 position = Nodes[stop].m_position;
            Vector3 position2 = Nodes[nextStop].m_position;
            float num = (transportType != 0 && transportType != TransportInfo.TransportType.EvacuationBus && transportType != TransportInfo.TransportType.TouristBus) ? 64f : 32f;
            int num2 = Mathf.Max((int)((position.x - num) / 8f + 1080f), 0);
            int num3 = Mathf.Max((int)((position.z - num) / 8f + 1080f), 0);
            int num4 = Mathf.Min((int)((position.x + num) / 8f + 1080f), 2159);
            int num5 = Mathf.Min((int)((position.z + num) / 8f + 1080f), 2159);
            int iPassengerCount = 0;

            for (int i = num3; i <= num5; i++)
            {
                for (int j = num2; j <= num4; j++)
                {
                    ushort num7 = instance.m_citizenGrid[i * 2160 + j];
                    int num8 = 0;
                    while (num7 != 0)
                    {
                        ushort nextGridInstance = CitizenInstances[num7].m_nextGridInstance;
                        if ((CitizenInstances[num7].m_flags & CitizenInstance.Flags.WaitingTransport) != 0)
                        {
                            Vector3 a = CitizenInstances[num7].m_targetPos;
                            float num9 = Vector3.SqrMagnitude(a - position);
                            if (num9 < num * num)
                            {
                                CitizenInfo info2 = CitizenInstances[num7].Info;
                                if (info2.m_citizenAI.TransportArriveAtSource(num7, ref CitizenInstances[num7], position, position2))
                                {
                                    iPassengerCount++;
                                }
                            }
                        }

                        num7 = nextGridInstance;
                        if (++num8 > 65536)
                        {
                            Debug.Log("Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return iPassengerCount;
        }
    }
}