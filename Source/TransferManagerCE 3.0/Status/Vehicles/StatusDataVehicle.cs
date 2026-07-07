using ColossalFramework;
using SleepyCommon;
using System;
using TransferManagerCore.Settings;
using UnityEngine;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicle : StatusData
    {
        // vehicle information
        public ushort m_vehicleId;
        public ushort m_sourceBuildingId;
        public InstanceID m_target;

        // --------------------------------------------------------------------
        public StatusDataVehicle(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) :
            base(reason, eBuildingType, BuildingId)
        {
            m_sourceBuildingId = sourceBuildingId;
            m_vehicleId = vehicleId;
            m_target = target;
            m_color = KnownColor.lightGrey;
        }

        // --------------------------------------------------------------------
        public override bool IsBuildingData()
        {
            return false;
        }

        // --------------------------------------------------------------------
        public override bool IsVehicleData()
        {
            return true;
        }

        // --------------------------------------------------------------------
        public override bool CanDelete()
        {
            return true;
        }

        // --------------------------------------------------------------------
        public override string GetDeleteTooltip()
        {
            return $"{Localization.Get("btnDeleteVehicle")} #{GetVehicleId()}";
        }

        // --------------------------------------------------------------------
        public override void OnClickDelete()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                // Remove vehicle
                InstanceID vehicleInstace = new InstanceID { Vehicle = vehicleId };
                Singleton<SimulationManager>.instance.AddAction(() =>
                {
                    // If vehicle is stuck we may need to add Created flag to remove it
                    ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleInstace.Vehicle];
                    vehicle.m_flags |= Vehicle.Flags.Created;

                    // Remove vehicle
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleInstace.Vehicle);
                });
            }
        }

        // --------------------------------------------------------------------
        public override int CompareTo(object second)
        {
            if (second is StatusDataVehicle)
            {
                StatusDataVehicle oSecond = (StatusDataVehicle)second;

                if (GetDistance() != oSecond.GetDistance())
                {
                    return GetDistance().CompareTo(oSecond.GetDistance());
                }
            }

            return base.CompareTo(second);
        }

        // --------------------------------------------------------------------
        public override string GetMaterialDisplay()
        {
            if (m_eBuildingType == BuildingType.OutsideConnection || 
                !ModSettings.GetSettings().StatusHideVehicleReason ||
                GetMaterial() == CustomTransferReason.Reason.None)
            {
                return GetMaterialDescription();
            }
            else if (!HasBuildingReason(GetMaterial()))
            {
                return GetMaterialDescription();
            }
            else
            {
                // We leave this column blank so they become sub-items for the building.
                return "";
            } 
        }

        // --------------------------------------------------------------------
        protected override string CalculateValue(out string tooltip)
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                string sValue = CitiesUtils.GetVehicleTransferValue(GetVehicleId(), out int current, out int max);
                tooltip = $"Vehicle Load: {DisplayBufferLong(current)} / {DisplayBufferLong(max)}";
                return sValue;
            }

            tooltip = "";
            return "";
        }

        // --------------------------------------------------------------------
        protected override string CalculateTimer(out string tooltip)
        {
            string sTimer = "";
            tooltip = "";

            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_waitCounter > 0)
                {
                    sTimer += "W:" + vehicle.m_waitCounter + " ";
                    tooltip += $"Waiting Timer: {vehicle.m_waitCounter}";
                }
                if (vehicle.m_blockCounter > 0)
                {
                    sTimer += "B:" + vehicle.m_blockCounter + " ";
                    tooltip += $"Blocked Timer: {vehicle.m_blockCounter}";
                }
            }

            return sTimer;
        }

        // --------------------------------------------------------------------
        protected override double CalculateDistance()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    InstanceID target = new InstanceID { Building = m_buildingId };
                    Vector3 buildingPos = InstanceHelper.GetPosition(target);
                    Vector3 vehiclePos = vehicle.GetLastFramePosition();
                    return Math.Sqrt(Vector3.SqrMagnitude(vehiclePos - buildingPos)) * 0.001;
                }
            }

            return double.MaxValue;
        }

        // --------------------------------------------------------------------
        protected override string CalculateDescription1(out string tooltip)
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                // Get expanded tooltip
                tooltip = VehicleUtils.GetVehicleTooltip(vehicleId);

                InstanceID instance = new InstanceID { Vehicle = vehicleId };
                return InstanceHelper.DescribeInstance(instance, true, false);
            }

            tooltip = "";
            return "";
        }

        // --------------------------------------------------------------------
        // Status tab = Responder (Vehicle Source)
        // Stops tab = Vehicle Target
        protected override string CalculateDescription2(out string tooltip)
        {
            // Tooltip
            InstanceID instance = new InstanceID { Building = GetSourceId() };

            tooltip = $"{InstanceHelper.DescribeInstance(instance, true, true)}";

            if (GetVehicleId() != 0)
            {
                string sState = DescribeVehicleState(GetVehicleId());
                if (!string.IsNullOrEmpty(sState))
                {
                    return sState;
                }
            }

            return InstanceHelper.DescribeInstance(instance, false, false);
        }

        // --------------------------------------------------------------------
        public override ushort GetVehicleId()
        {
            if (m_vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
                if (vehicle.m_cargoParent != 0)
                {
                    return vehicle.m_cargoParent;
                }
                else
                {
                    return m_vehicleId;
                }
            }

            return 0;
        }

        // --------------------------------------------------------------------
        public ushort GetSourceId()
        {
            if (m_sourceBuildingId != 0)
            {
                return m_sourceBuildingId;
            }

            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                return vehicle.m_sourceBuilding;
            }

            return 0;
        }

        // --------------------------------------------------------------------
        public InstanceID GetTarget()
        {
            return m_target;
        }

        // --------------------------------------------------------------------
        public override void OnClickDescription1()
        {
            InstanceHelper.ShowInstance(new InstanceID { Vehicle = GetVehicleId() }); 
        }

        // --------------------------------------------------------------------
        public override void OnClickDescription2()
        {
            InstanceHelper.ShowInstance(new InstanceID { Building = GetSourceId() });
        }

        // --------------------------------------------------------------------
        protected string DescribeVehicleState(ushort vehiceId)
        {
            if (GetVehicleId() != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[GetVehicleId()];
                if ((vehicle.m_flags & Vehicle.Flags.WaitingLoading) != 0)
                {
                    if ((vehicle.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.Spawned)) == (Vehicle.Flags.Stopped | Vehicle.Flags.Spawned))
                    {
                        return "Loading (Phase 2)";
                    }
                    else
                    {
                        return "Loading";
                    }
                }
                else if ((vehicle.m_flags & Vehicle.Flags.WaitingSpace) != 0)
                {
                    return "Waiting space";
                }
                else if ((vehicle.m_flags & Vehicle.Flags.WaitingCargo) != 0)
                {
                    return "Waiting cargo";
                }
                else if ((vehicle.m_flags & Vehicle.Flags.Congestion) != 0)
                {
                    return "Congestion";
                }
            }

            return "";
        }
    }
}