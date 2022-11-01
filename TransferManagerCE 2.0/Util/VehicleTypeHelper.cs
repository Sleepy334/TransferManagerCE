using ColossalFramework;
using System;
using TransferManagerCE.Util;

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
                                }
                                break;
                            case ItemClass.SubService.PublicTransportPlane:
                                switch (vehicle.Info.GetAI())
                                {
                                    case CargoPlaneAI: return VehicleType.CargoPlane;
                                    case PassengerPlaneAI: return VehicleType.PassengerPlane;
                                }
                                break;
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

            Debug.Log($"Service: {vehicle.Info.GetService()} SubService: {vehicle.Info.GetSubService()} AI: {vehicle.Info.GetAI()}");
            return VehicleType.Unknown;
        }

        public static InstanceID GetVehicleTarget(ushort vehicleId, Vehicle vehicle)
        {
            if (vehicle.Info != null && vehicle.Info.GetSubService() == ItemClass.SubService.PublicTransportTaxi)
            {
                ushort passengerInstance = GetPassengerInstance(ref vehicle);
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
                return default(InstanceID); // Empty
            }
            else
            {
                VehicleAI? vehcileAI = vehicle.Info?.GetAI() as VehicleAI;
                if (vehcileAI != null)
                {
                    return vehcileAI.GetTargetID(vehicleId, ref vehicle);
                }
            }

            return new InstanceID { Building = vehicle.m_targetBuilding };
        }

        public static string DescribeVehicleTarget(Vehicle vehicle, InstanceID target)
        {
            if ((vehicle.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                return "Returning to facility";
            }
            else if ((vehicle.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                return "Waiting for target";
            }
            else if ((vehicle.m_flags & Vehicle.Flags.WaitingLoading) != 0)
            {
                return "Loading";
            }
            else if ((vehicle.m_flags & Vehicle.Flags.WaitingCargo) != 0)
            {
                return "Waiting for cargo";
            }
            else if (!target.IsEmpty)
            {
                return InstanceHelper.DescribeInstance(target);
            }

            return Localization.Get("txtVehiclesNone");
        }

        public static ushort GetPassengerInstance(ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                CitizenUnit unit = instance.m_units.m_buffer[num];
                uint nextUnit = unit.m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = unit.GetCitizen(i);
                    if (citizen != 0)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
                        if (instance2 != 0)
                        {
                            return instance2;
                        }
                    }
                }

                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return 0;
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