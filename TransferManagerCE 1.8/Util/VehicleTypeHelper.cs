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
        }

        public static VehicleType GetVehicleType(Vehicle vehicle)
        {
            switch (vehicle.Info.m_vehicleAI)
            {
                case HearseAI:
                    {
                        return VehicleType.Hearse;
                    }
                case AmbulanceAI:
                    {
                        return VehicleType.Ambulance;
                    }
                case GarbageTruckAI:
                    {
                        return VehicleType.GarbageTruck;
                    }
                case FireTruckAI:
                    {
                        return VehicleType.FireTruck;
                    }
                case FireCopterAI:
                    {
                        return VehicleType.FireCopter;
                    }
                case PoliceCarAI:
                    {
                        return VehicleType.PoliceCar;
                    }
                case PoliceCopterAI:
                    {
                        return VehicleType.PoliceCopter;
                    }
                case PostVanAI:
                    {
                        return VehicleType.PostVan;
                    }
                case ParkMaintenanceVehicleAI:
                    {
                        return VehicleType.ParkMaintenanceVehicle;
                    }
                case MaintenanceTruckAI:
                    {
                        return VehicleType.RoadMaintenanceTruck;
                    }
                case CargoTruckAI:
                    {
                        return VehicleType.CargoTruck;
                    }
                case CargoTrainAI:
                    {
                        return VehicleType.CargoTrain;
                    }
                case CargoShipAI:
                    {
                        return VehicleType.CargoShip;
                    }
                case CargoPlaneAI:
                    {
                        return VehicleType.CargoPlane;
                    }
                case TaxiAI:
                    {
                        return VehicleType.Taxi;
                    }
                case MetroTrainAI:
                    {
                        return VehicleType.MetroTrain;
                    }
                case PassengerTrainAI:
                    {
                        return VehicleType.PassengerTrain;
                    }
                case BusAI:
                    {
                        return VehicleType.Bus;
                    }
                case SnowTruckAI:
                    {
                        return VehicleType.SnowPlow;
                    }
                default:
                    {
                        return VehicleType.Unknown;
                    }
            }
        }

        public static InstanceID GetVehicleTarget(Vehicle vehicle)
        {
            VehicleType eType = GetVehicleType(vehicle);
            switch (eType)
            {
                case VehicleType.SnowPlow:
                case VehicleType.RoadMaintenanceTruck:
                    {
                        // Target is actually a segment
                        return new InstanceID { NetSegment = vehicle.m_targetBuilding };
                    }
                case VehicleType.MetroTrain:
                case VehicleType.PassengerTrain:
                case VehicleType.Bus:
                    {
                        // Target is actually a node
                        return new InstanceID { NetNode = vehicle.m_targetBuilding };
                    }
                case VehicleType.Taxi:
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
                default: 
                    {
                        return new InstanceID { Building = vehicle.m_targetBuilding };
                    }
            }
        }

        public static string DescribeVehicleTarget(Vehicle vehicle, InstanceID target)
        {
            if (!target.IsEmpty)
            {
                return InstanceHelper.DescribeInstance(target);
            }
            else if ((vehicle.m_flags & Vehicle.Flags.GoingBack) != 0)
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

            return Localization.Get("txtVehiclesNone");
        }

        private static ushort GetPassengerInstance(ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
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

        public static int GetVehiclePassengerCount(ushort usVehicleId, out int iCapacity)
        {
            int iPassengers = 0;
            iCapacity = 0;

            var VMInstance = VehicleManager.instance;
            Vehicle oVehicle = VMInstance.m_vehicles.m_buffer[usVehicleId];

            // Copied from PublicTransportVehicleWorldInfoPanel.UpdateBindings()
            ushort firstVehicle = oVehicle.GetFirstVehicle(usVehicleId);
            if (firstVehicle > 0)
            {
                oVehicle.Info.m_vehicleAI.GetBufferStatus(firstVehicle, ref VMInstance.m_vehicles.m_buffer[firstVehicle], out string _, out int current, out int max);
                if (max != 0)
                {
                    iPassengers = current;
                    iCapacity = max;
                }
            }

            return iPassengers;
        }
    }
}