using ColossalFramework;
using SleepyCommon;
using System;
using TransferManagerCE.Settings;
using UnityEngine;
using static RenderManager;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataVehicle : StatusData
    {
        public ushort m_responderBuilding;
        public ushort m_vehicleId;

        public StatusDataVehicle(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId)
        {
            m_responderBuilding = responder;
            m_vehicleId = target;
            m_color = KnownColor.lightGrey;
        }

        public override bool IsBuildingData()
        {
            return false;
        }

        public override bool IsNodeData()
        {
            return false;
        }

        public override bool HasVehicle()
        {
            return true;
        }

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

        protected override string CalculateVehicle(out string tooltip)
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                // Get expanded tooltip
                tooltip = VehicleUtils.GetVehicleTooltip(vehicleId);

                InstanceID instance = new InstanceID { Vehicle = vehicleId };
                return InstanceHelper.DescribeInstance(instance, InstanceID.Empty, false);
            }

            tooltip = "";
            return "";
        }

        protected override string CalculateResponder(out string tooltip)
        {
            ushort buildingId = GetResponderId();
            if (buildingId != 0)
            {
                InstanceID instance = new InstanceID { Building = buildingId };
                tooltip = $"{InstanceHelper.DescribeInstance(instance, InstanceID.Empty, true)}";
                return InstanceHelper.DescribeInstance(instance, InstanceID.Empty, false);
            }

            tooltip = "";
            return "";
        }
        public override ushort GetVehicleId()
        {
            if (HasVehicle())
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

        public override ushort GetResponderId()
        {
            if (m_responderBuilding != 0)
            {
                return m_responderBuilding;
            }
            ushort targetId = GetVehicleId();
            if (targetId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[targetId];
                return vehicle.m_sourceBuilding;
            }
            return 0;
        }
    }
}