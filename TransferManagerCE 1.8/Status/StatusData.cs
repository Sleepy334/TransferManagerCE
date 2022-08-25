using System;
using TransferManagerCE.Util;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusData
    {
        public TransferReason m_material;
        public BuildingType m_eBuildingType;
        public ushort m_buildingId;
        public ushort m_responderBuilding;
        public ushort m_targetVehicle;

        public StatusData(TransferReason reason, BuildingType eBuildingType, ushort buildingId, ushort responderBuilding, ushort targetVehicle)
        {
            m_material = reason;
            m_eBuildingType = eBuildingType;
            m_buildingId = buildingId;
            m_responderBuilding = responderBuilding;
            m_targetVehicle = targetVehicle;
        }

        public virtual TransferReason GetMaterial()
        {
            return m_material;
        }

        public virtual string GetValue()
        {
            return "";
        }

        public virtual string GetTimer()
        {
            string sTimer = "";

            ushort targetId = GetTargetId();
            if (targetId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[targetId];
                if (vehicle.m_waitCounter > 0)
                {
                    sTimer += "W:" + vehicle.m_waitCounter + " ";
                }
                if (vehicle.m_blockCounter > 0)
                {
                    sTimer += "B:" + vehicle.m_blockCounter + " ";
                }
            }

            return sTimer;
        }

        public virtual void Update() { }

        public virtual ushort GetResponderId()
        {
            if (m_responderBuilding != 0)
            {
                return m_responderBuilding;
            }
            ushort targetId = GetTargetId();
            if (targetId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[targetId];
                return vehicle.m_sourceBuilding;
            }
            return 0;
        }

        public virtual string GetResponder()
        {
            ushort buildingId = GetResponderId();
            if (buildingId != 0)
            {
                return CitiesUtils.GetBuildingName(buildingId);
            }
            return Localization.Get("txtStatusNone");
        }

        public virtual void OnClickResponder()
        {
            ushort buildingId = GetResponderId();
            if (buildingId != 0)
            {
                CitiesUtils.ShowBuilding(buildingId);
            }
        }

        public virtual ushort GetTargetId()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                if (vehicle.m_cargoParent != 0)
                {
                    return vehicle.m_cargoParent;
                }
                else
                {
                    return m_targetVehicle;
                }
            }

            return 0;
        }

        public virtual string GetTarget()
        {
            ushort vehicleId = GetTargetId();
            if (vehicleId != 0)
            {
                return CitiesUtils.GetVehicleName(vehicleId);
            }

            return Localization.Get("txtStatusNone");
        }

        public virtual string GetLoad()
        {
            ushort vehicleId = GetTargetId();
            if (vehicleId != 0)
            {
                return CitiesUtils.GetVehicleTransferValue(vehicleId);
            }

            return "";
        }
        

        public virtual void OnClickTarget()
        {
            ushort targetId = GetTargetId();
            if (targetId != 0)
            {
                CitiesUtils.ShowVehicle(targetId);
            }
        }
    }
}
