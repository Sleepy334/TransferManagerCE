using ColossalFramework;
using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Text;
using TransferManagerCE.Common;

namespace TransferManagerCE
{
    public class VehicleTypeHelper
    {
        public enum VehicleType
        {
            Unknown,
            Hearse,
            Ambulance,
            AmbulanceCopter,
            GarbageTruck,
            FireTruck,
            FireCopter,
            PoliceCar,
            PoliceCopter,
            BankVan,
            PostVan,
            ParkMaintenanceVehicle,
            RoadMaintenanceTruck,
            CargoTruck,
            CargoShip,
            CargoPlane,
            CargoTrain,
            Taxi,
            PassengerTrain,
            MetroTrain,
            Bus,
            SnowPlow,
            CruiseShip,
            DisasterResponseTruck,
            DisasterResponseCopter,
            FishingBoat,
            PassengerCar,
            PassengerPlane,
            Tram,
            Monorail,
            Ferry,
        }

        public static VehicleType GetVehicleType(Vehicle vehicle)
        {
            switch (vehicle.Info.GetService())
            {
                case ItemClass.Service.Residential:
                    {
                        return VehicleType.PassengerCar;
                    }
                case ItemClass.Service.Beautification:
                    {
                        switch (vehicle.Info.GetSubService())
                        {
                            case ItemClass.SubService.BeautificationParks:
                                return VehicleType.ParkMaintenanceVehicle;
                        }
                        break;
                    }
                case ItemClass.Service.PublicTransport:
                    {
                        switch (vehicle.Info.GetSubService())
                        {
                            case ItemClass.SubService.PublicTransportPost:
                                return VehicleType.PostVan;
                            case ItemClass.SubService.PublicTransportTaxi:
                                return VehicleType.Taxi;
                            case ItemClass.SubService.PublicTransportMetro:
                                return VehicleType.MetroTrain;
                            case ItemClass.SubService.PublicTransportBus:
                                return VehicleType.Bus;
                            case ItemClass.SubService.PublicTransportTrain:
                                switch (vehicle.Info.GetAI())
                                {
                                    case CargoTrainAI: return VehicleType.CargoTrain;
                                    case PassengerTrainAI: return VehicleType.PassengerTrain;
                                }
                                break;
                            case ItemClass.SubService.PublicTransportShip:
                                switch (vehicle.Info.GetAI())
                                {
                                    case CargoShipAI: return VehicleType.CargoShip;
                                    case PassengerShipAI: return VehicleType.CruiseShip;
                                    case PassengerFerryAI: return VehicleType.Ferry;
                                }
                                break;
                            case ItemClass.SubService.PublicTransportPlane:
                                switch (vehicle.Info.GetAI())
                                {
                                    case CargoPlaneAI: return VehicleType.CargoPlane;
                                    case PassengerPlaneAI: return VehicleType.PassengerPlane;
                                }
                                break;
                            case ItemClass.SubService.PublicTransportTram:
                                {
                                    return VehicleType.Tram;
                                }
                            case ItemClass.SubService.PublicTransportMonorail:
                                {
                                    return VehicleType.Monorail;
                                }
                        }
                        break;
                    }
                case ItemClass.Service.Industrial:
                case ItemClass.Service.PlayerIndustry:
                    {
                        switch (vehicle.Info.GetAI())
                        {
                            case CargoTruckAI: return VehicleType.CargoTruck;
                            case CargoShipAI: return VehicleType.CargoShip;
                            case CargoTrainAI: return VehicleType.CargoTrain;
                            case CargoPlaneAI: return VehicleType.CargoPlane;
                        }
                        break;
                    }
                case ItemClass.Service.HealthCare:
                    {
                        switch (vehicle.Info.GetAI())
                        {
                            case AmbulanceAI: return VehicleType.Ambulance;
                            case AmbulanceCopterAI: return VehicleType.AmbulanceCopter;
                            case HearseAI: return VehicleType.Hearse;
                        }
                        break;
                    }
                case ItemClass.Service.FireDepartment:
                    switch (vehicle.Info.GetAI())
                    {
                        case FireTruckAI: return VehicleType.FireTruck;
                        case FireCopterAI: return VehicleType.FireCopter;
                    }
                    break;
                case ItemClass.Service.Fishing:
                    {
                        switch (vehicle.Info.GetAI())
                        {
                            case CargoTruckAI: return VehicleType.CargoTruck;
                            case FishingBoatAI: return VehicleType.FishingBoat;
                        }
                        break; 
                    }
                case ItemClass.Service.PoliceDepartment:
                    {
                        switch (vehicle.Info.GetAI())
                        {
                            case PoliceCarAI: return VehicleType.PoliceCar;
                            case PoliceCopterAI: return VehicleType.PoliceCopter;
                            case BankVanAI: return VehicleType.BankVan;
                        }
                        break;
                    }
                case ItemClass.Service.Garbage:
                    {
                        return VehicleType.GarbageTruck;
                    }
                case ItemClass.Service.Road:
                    switch (vehicle.Info.GetAI())
                    {
                        case MaintenanceTruckAI: return VehicleType.RoadMaintenanceTruck;
                        case SnowTruckAI: return VehicleType.SnowPlow;
                    }
                    break;
                case ItemClass.Service.Disaster:
                    switch (vehicle.Info.GetAI())
                    {
                        case DisasterResponseCopterAI: return VehicleType.DisasterResponseCopter;
                        case DisasterResponseVehicleAI: return VehicleType.DisasterResponseTruck;
                    }
                    break;
            }

            return VehicleType.Unknown;
        }

        public static InstanceID GetVehicleTarget(ushort vehicleId, Vehicle vehicle)
        {
            if (vehicle.Info is not null && vehicle.Info.m_vehicleAI is not null)
            {
                switch (vehicle.Info.GetAI())
                {
                    case TaxiAI taxiAI:
                        {
                            ushort passengerInstance = GetPassengerInstance(vehicleId, ref vehicle);
                            if (passengerInstance != 0)
                            {
                                CitizenInstance citizenInstance = Singleton<CitizenManager>.instance.m_instances.m_buffer[passengerInstance];
                                if ((citizenInstance.m_flags & CitizenInstance.Flags.Character) != 0)
                                {
                                    return new InstanceID { Citizen = citizenInstance.m_citizen };
                                }
                                else if ((citizenInstance.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                                {
                                    return new InstanceID { NetNode = citizenInstance.m_targetBuilding };
                                }
                                else
                                {
                                    return new InstanceID { Building = citizenInstance.m_targetBuilding };
                                }
                            }

                            return new InstanceID { Building = vehicle.m_targetBuilding };
                        }
                    case VehicleAI vehicleAI:
                        {
                            return vehicleAI.GetTargetID(vehicleId, ref vehicle);
                        }
                }
            }

            return new InstanceID { Building = vehicle.m_targetBuilding };
        }

        public static string DescribeVehicleTarget(ushort vehicleId, Vehicle vehicle, InstanceID target)
        {
            if (vehicle.Info is not null && vehicle.Info.m_vehicleAI is not null)
            {
                string sStatus = vehicle.Info.m_vehicleAI.GetLocalizedStatus(vehicleId, ref vehicle, out InstanceID newTarget);
                if (!newTarget.IsEmpty)
                {
                    return InstanceHelper.DescribeInstance(newTarget);
                }
                else
                {
                    // Is it a transport line
                    switch (target.Type)
                    {
                        case InstanceType.NetNode:
                            {
                                NetNode[] NetNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
                                NetNode node = NetNodes[target.Index];
                                if (node.m_flags != 0 && node.m_transportLine != 0)
                                {
                                    return InstanceHelper.DescribeInstance(new InstanceID { TransportLine = node.m_transportLine });
                                }
                                break;
                            }
                        case InstanceType.Building:
                            {
                                switch (vehicle.Info.GetAI())
                                {
                                    case CargoTruckAI:
                                        {
                                            if ((vehicle.m_flags & Vehicle.Flags.GoingBack) != 0)
                                            {
                                                return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                                            }
                                            else if ((vehicle.m_flags & Vehicle.Flags.WaitingTarget) != 0)
                                            {
                                                return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_UNLOAD");
                                            }
                                            else if ((vehicle.m_flags & Vehicle.Flags.WaitingLoading) != 0)
                                            {
                                                return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_WAIT");
                                            }
                                            else if ((vehicle.m_flags & Vehicle.Flags.WaitingCargo) != 0)
                                            {
                                                return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_WAIT");
                                            }
                                            else if ((vehicle.m_flags & Vehicle.Flags.Exporting) != 0)
                                            {
                                                return InstanceHelper.DescribeInstance(target);
                                            }
                                            break;
                                        }
                                    case GarbageTruckAI:
                                        {
                                            // Show target building rather than "Collecting Garbage" message.
                                            if (sStatus.Equals(Locale.Get("VEHICLE_STATUS_GARBAGE_COLLECT")))
                                            {
                                                return InstanceHelper.DescribeInstance(target);
                                            }
                                            break;
                                        }
                                    case PostVanAI:
                                        {
                                            // Show target building rather than "Delivering Mail" message.
                                            if (sStatus.Equals(Locale.Get("VEHICLE_STATUS_POST_VEHICLE_DELIVER")))
                                            {
                                                return InstanceHelper.DescribeInstance(target);
                                            }
                                            break;
                                        }
                                    case PoliceCarAI:
                                        {
                                            // Show target building rather than "Patrolling" message.
                                            if (sStatus.Equals(Locale.Get("VEHICLE_STATUS_POLICE_PATROL")))
                                            {
                                                return InstanceHelper.DescribeInstance(target);
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                    }

                    return sStatus;
                }
            }
            else if (!target.IsEmpty)
            {
                return InstanceHelper.DescribeInstance(target);
            }

            return Localization.Get("txtVehiclesNone");
        }

        public static ushort GetPassengerInstance(ushort vehicleId, ref Vehicle data)
        {
            ushort usPassengerInstanceId = 0;

            CitizenUtils.EnumerateCitizens(new InstanceID { Vehicle = vehicleId }, data.m_citizenUnits, (uint citizenID, Citizen citizen) =>
            {
                if (citizen.m_flags != 0)
                {
                    usPassengerInstanceId = citizen.m_instance;
                }

                // Exit loop (return false) if we found a passenger
                return (usPassengerInstanceId == 0);
            });

            return usPassengerInstanceId;
        }

        public static int GetBufferStatus(ushort usVehicleId, out int iCapacity)
        {
            int iPassengers = 0;
            iCapacity = 0;

            Vehicle[] vehicleBuffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Vehicle oVehicle = vehicleBuffer[usVehicleId];

            // Copied from PublicTransportVehicleWorldInfoPanel.UpdateBindings()
            ushort firstVehicle = oVehicle.GetFirstVehicle(usVehicleId);
            if (firstVehicle > 0)
            {
                oVehicle.Info.m_vehicleAI.GetBufferStatus(firstVehicle, ref vehicleBuffer[firstVehicle], out string _, out int current, out int max);
                if (max != 0)
                {
                    iPassengers = current;
                    iCapacity = max;
                }
            }

            return iPassengers;
        }

        public static int GetVehiclePassengerCount(ushort usVehicleId, out int iCapacity)
        {
            return GetBufferStatus(usVehicleId, out iCapacity);
        }
    }
}