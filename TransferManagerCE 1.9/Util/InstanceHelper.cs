using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class InstanceHelper
    {
        public static string DescribeInstance(InstanceID instance)
        {
            if (instance.IsEmpty)
            {
                return "Empty";
            } 
            else 
            {
                switch (instance.Type)
                {
                    case InstanceType.Building:
                        {
                            if (instance.Building != 0)
                            {
                                string sName = CitiesUtils.GetBuildingName(instance.Building);
                                if (string.IsNullOrEmpty(sName))
                                {
                                    return "Building:" + instance.Building;
                                }
                                else
                                {
                                    return sName;
                                }
                            }
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            string sName = CitiesUtils.GetVehicleName(instance.Vehicle);
                            if (string.IsNullOrEmpty(sName))
                            {
                                return "Vehicle:" + instance.Vehicle;
                            }
                            else
                            {
                                return sName;
                            }
                        }
                    case InstanceType.Citizen:
                        {
                            string sName = CitiesUtils.GetCitizenName(instance.Citizen);
                            if (string.IsNullOrEmpty(sName))
                            {
                                return "Citizen:" + instance.Citizen;
                            }
                            else
                            {
                                return sName;
                            }
                        }
                    case InstanceType.Park:
                        {

                            string sName = DistrictManager.instance.GetParkName(instance.Park);
                            if (string.IsNullOrEmpty(sName))
                            {
                                return "Park:" + instance.Park;
                            }
                            else
                            {
#if DEBUG
                                return $"({instance.Park}) " + sName;
#else
                                 return sName;
#endif
                            }
                        }
                    case InstanceType.NetNode:
                        {
                            if (instance.NetNode > 0 && instance.NetNode < NetManager.instance.m_nodes.m_size)
                            {
                                ushort buildingId = NetNode.FindOwnerBuilding(instance.NetNode, 64f);
                                if (buildingId != 0)
                                {
                                    string sName = CitiesUtils.GetBuildingName(buildingId);
                                    if (string.IsNullOrEmpty(sName))
                                    {
                                        return "Building:" + instance.Building;
                                    }
                                    else
                                    {
                                        return sName;
                                    }
                                }
                            }

                            return instance.Type.ToString() + ": " + instance.Index;
                        }
                    case InstanceType.TransportLine:
                        {
                            if (instance.TransportLine > 0 && instance.TransportLine < TransportManager.instance.m_lines.m_size)
                            {
                                TransportLine line = TransportManager.instance.m_lines.m_buffer[instance.TransportLine];
                                if (line.m_flags != 0)
                                {
                                    return CitiesUtils.GetSafeLineName(instance.TransportLine);
                                }
                            }

                            return instance.Type.ToString() + ": " + instance.Index;
                        }
                    default:
                        {
                            return instance.Type.ToString() + ": " + instance.Index;
                        }
                }
            }

            return "";
        }

        public static void ShowInstance(InstanceID instance)
        {
            if (!instance.IsEmpty)
            {
                switch (instance.Type)
                {
                    case InstanceType.Building:
                        {
                            CitiesUtils.ShowBuilding(instance.Building);
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            CitiesUtils.ShowVehicle(instance.Vehicle);
                            break;
                        }
                    case InstanceType.Citizen:
                        {
                            CitiesUtils.ShowCitizen(instance.Citizen);
                            break;
                        }
                    case InstanceType.NetNode:
                        {
                            CitiesUtils.ShowNode(instance.NetNode);
                            break;
                        }
                    case InstanceType.NetSegment:
                        {
                            CitiesUtils.ShowSegment(instance.NetSegment);
                            break;
                        }
                    case InstanceType.TransportLine:
                        {
                            CitiesUtils.ShowTransportLine(instance.TransportLine);
                            break;
                        }
                }
            }
        }

        public static List<ushort> GetBuildings(InstanceID instance)
        {
            List<ushort> buildings = new List<ushort>();

            if (!instance.IsEmpty)
            {
                switch (instance.Type)
                {
                    case InstanceType.Building:
                        {
                            buildings.Add(instance.Building);
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[instance.Vehicle];
                            if (vehicle.m_flags != 0)
                            {
                                buildings.Add(vehicle.m_sourceBuilding);
                            }
                            break;
                        }
                    case InstanceType.Citizen:
                        {
                            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[instance.Citizen];
                            if (citizen.m_flags != 0)
                            {
                                buildings.Add(citizen.GetBuildingByLocation());
                            }
                            break;
                        }
                    case InstanceType.NetNode:
                        {
                            ushort buildingId = NetNode.FindOwnerBuilding(instance.NetNode, 64f);
                            if (buildingId != 0)
                            {
                                buildings.Add(buildingId);
                            }
                            break;
                        }
                    case InstanceType.Park:
                        {

                            // We need to find a ServicePoint node instead
                            DistrictPark park = DistrictManager.instance.m_parks.m_buffer[instance.Park];
                            if (park.m_flags != 0)
                            {
                                foreach (ushort buildingId in park.m_finalServicePointList)
                                {
                                    if (buildingId != 0)
                                    {
                                        buildings.Add(buildingId);
                                    }
                                }
                                break;
                            }
                            break;
                        }
                }
            }

            return buildings;
        }

        public static Vector3 GetPosition(InstanceID instance)
        {
            if (!instance.IsEmpty)
            {
                switch (instance.Type)
                {
                    case InstanceType.Building:
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[instance.Building];
                            if (building.m_flags != 0)
                            {
                                return building.m_position;
                            }
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[instance.Vehicle];
                            if (vehicle.m_flags != 0)
                            {
                                return vehicle.GetLastFramePosition();
                            }
                            break;
                        }
                    case InstanceType.Citizen:
                        {
                            Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[instance.Citizen];
                            if (citizen.m_flags != 0)
                            {
                                if (citizen.m_instance != 0)
                                {
                                    return CitiesUtils.GetCitizenInstancePosition(citizen.m_instance);
                                }
                                else
                                {
                                    ushort buildingId = citizen.GetBuildingByLocation();
                                    if (buildingId != 0)
                                    {
                                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                                        return building.m_position;
                                    }
                                }
                            }
                            break;
                        }
                    case InstanceType.NetNode:
                        {
                            NetNode oNode = NetManager.instance.m_nodes.m_buffer[instance.NetNode];
                            if (oNode.m_flags != 0)
                            {
                                return oNode.m_position;
                            }
                            break;
                        }
                    case InstanceType.NetSegment:
                        {
                            NetSegment oSegment = NetManager.instance.m_segments.m_buffer[instance.NetSegment];
                            if (oSegment.m_flags != 0)
                            {
                                return oSegment.m_middlePosition;
                            }
                            break;
                        }
                }
            }

            return Vector3.zero;
        }

        public static byte GetDistrict(InstanceID instance)
        {
            if (!instance.IsEmpty)
            {
                Vector3 position = GetPosition(instance);
                if (position != Vector3.zero)
                {
                    return DistrictManager.instance.GetDistrict(position);
                }
            }
            return 0;
        }

        public static byte GetPark(InstanceID instance)
        {
            if (!instance.IsEmpty)
            {
                Vector3 position = GetPosition(instance);
                if (position != Vector3.zero)
                {
                    return DistrictManager.instance.GetPark(position);
                }
            }
            return 0;
        }
    }
}